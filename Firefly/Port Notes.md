# Firefly — C++ to Unity Port Notes

Every deviation from `Code/Firefly Original`, and why it was necessary.

Source: `Code/Firefly Original/eclipse workspace/Firefly-common/src`
Target: `Code/Firefly/Firefly Controller/Assets/Scripts`

Naming, class hierarchy, method structure, constants and comments are carried over as-is. Method names are PascalCase per C# convention; everything else keeps its original spelling, including the quirks.

---

## 1. Forced by the language

**Easing functions bound by pointer.** The C++ `bindValue(double*)` stored a raw pointer and wrote through it on update. C# has no equivalent, so bindings are `Action<T>` setter delegates:

```cpp
posEasingFunc->bindValue(&(spherePrimitive->centerPos));       // C++
posEasingFunc.BindValue(v => spherePrimitive.centerPos = v);   // C#
```

Same semantics — bind once, call `Update(time)`, the target updates itself. The duplicate-binding check (`std::find`) becomes `List.Contains` on the delegate.

**`ArbitraryMap` keys.** The C++ used `std::map<char*, void*>` — keyed on *pointer identity*, not string content. It worked because the keys were string literals the compiler pooled. C# uses `Dictionary<string, object>`, which compares by content. This is what the original meant; it just wasn't what it did.

**Negative modulo.** `ConcentricSpheresPrim` computes `(ring-1) % colors.size()`, which is negative in C++ for `ring == 0` and would throw when indexing in C#. Replaced with `FireflyUtils.Mod()`, which always returns a positive result. Only reachable at `ring == 0, phase == 0.5` exactly, where the C++ read out of bounds.

**Erasing while iterating.** `BalloonsAnimation::updateInternal` called `growthEases.erase(iter)` inside a loop that then did `iter++` on the invalidated iterator. The C# iterates backwards by index instead, which removes the same elements without the undefined behaviour.

**`Camera` renamed to `FireflyCamera`** — collides with `UnityEngine.Camera`.

**Fixed-size stack buffer.** `renderLED` declared `char pixelOut[3*pixelsLen+1]` as a variable-length array. C# allocates it once in the `PixelStage` constructor and reuses it. Same for the smoothing accumulator, which the C++ `new`'d and leaked every frame.

**`SortedList` for `std::map`.** `std::map<int,double>` and `std::map<double,int>` become `SortedList<,>`, which keeps the same sorted-iteration behaviour. `upper_bound` is written out explicitly in `SetUtils.UpperBound`.

---

## 2. Forced by the platform

**No GLFW.** Window creation, the GL context, `glClearColor`/`glEnable`/`gluPerspective` and the `while(!glfwWindowShouldClose)` loop are all Unity's job now. The mapping:

| C++ | C# |
|---|---|
| `FireflyController` constructor | `Start()` |
| `start()` loop body | `Update()` |
| `render(window, serial, time)` | `Render(time)` — body unchanged |
| `initGL()` | `InitRendering()` |
| `glfwGetTime()` | `Time.realtimeSinceStartupAsDouble` |

**Input.** GLFW's callbacks-plus-static-window-map become the **Input System** package, read in `Update()`. The mapping is one-to-one:

| GLFW | Input System |
|---|---|
| `action == GLFW_PRESS` | `wasPressedThisFrame` |
| `action != GLFW_RELEASE` (auto-repeat: `=`, `-`) | `isPressed` |
| `action == GLFW_RELEASE` (`Esc`) | `wasReleasedThisFrame` |
| mouse button / position / scroll callbacks | `Mouse.current` |

No functional difference. No Player Settings change needed — the Input System is what Unity 6's URP template ships with.

**Mouse Y is inverted.** GLFW's Y grows downward, Unity's upward. `cam.Rotate` takes `(lastY - yPos)` where the C++ took `(yPos - lastY)`, so dragging feels the same.

**Y-up throughout.** The C++ was Z-up: it passed `(0,0,1)` to `gluLookAt` and `PixelStage` built the helix rising in Z with the circle in x/y. The port is Unity-standard Y-up — circle in x/z, rise in y — changed at the three places that name an axis:

- `GenerateCylinderWithAnchors` swaps the y and z terms
- `FireflyCamera.ApplyTo` orbits in x/z and raises in y
- the calibration camera follow reads `GetY()` rather than `GetZ()`

Everything else is axis-agnostic. No animation, primitive, colour or easing code was touched.

**Pixel drawing.** `Pixel::render` / `Pixel::drawSphere` built a sphere per pixel from `glBegin`/`glVertex3f` triangle strips, every frame. Now the mesh is built once by `MeshBuilder.BuildSphere(PIXEL_SLICES, PIXEL_STACKS)` — same 8 × 3 tessellation — and drawn with `Graphics.RenderMeshInstanced` in batches of 1023. One draw call per batch instead of 1,440 immediate-mode spheres. `Pixel` keeps its position and colour; the drawing methods move to `MeshBuilder`.

**Cylinder walls.** `drawDefaultCylinderWalls` emitted a `GL_TRIANGLE_STRIP` each frame. Now `MeshBuilder.BuildCylinderWall(CYL_SLICES, ...)` builds the same 24-segment open tube once, in world space, with `CYL_DARKNESS` and `CYL_ALPHA` on a transparent material and culling off so it reads as a shell from any angle (GL was drawing it uncalled).

`CYL_STACKS` remains unused — the C++ declared it but `drawDefaultCylinderWalls` never referenced it; the wall is a single strip with no vertical subdivision. Kept as a dead constant, as in the original.

**Custom shader for per-pixel colour.** The C++ called `glColor3f` before each pixel's sphere. Instancing needs the equivalent as a per-instance shader property, and URP's stock Unlit declares `_BaseColor` inside `CBUFFER_START(UnityPerMaterial)` to stay SRP Batcher compatible — which makes it per-*material*, so a `MaterialPropertyBlock` vector array can't vary it per instance and every pixel draws the same colour. `Assets/Shaders/FireflyInstancedUnlit.shader` declares it with `UNITY_DEFINE_INSTANCED_PROP` instead. The cylinder wall still uses stock URP Unlit — it's one object and needs transparency, not instancing.

**Why not Unity's built-in primitives.** `GameObject.CreatePrimitive` returns a fixed baked mesh with no tessellation parameters, so using it would have silently discarded `PIXEL_SLICES`, `PIXEL_STACKS` and `CYL_SLICES`. Generating both meshes reproduces the original geometry exactly, and the 8 × 3 sphere is far cheaper than Unity's default sphere across 1,440 instances.

**Serial.** `Serial.h` `#ifdef`-ed between `Serial-PC.cpp` (Win32 `CreateFile`/`WriteFile`) and `arduino-serial-lib.c` (POSIX `termios`). `System.IO.Ports.SerialPort` covers both, so the split collapses to choosing the port name by `Application.platform`. Port names are unchanged: `COM9` and `/dev/cu.usbmodem27946701`. Failure to open is caught and logged rather than left as a dead handle — same outcome, `Available()` returns false and the app runs preview-only.

**Entry point.** `main()` becomes `FireflyMain.Main()`, tagged `[RuntimeInitializeOnLoadMethod]`, which creates the controller GameObject at load. No scene setup, no prefabs, no inspector wiring — open and press Play.

**Dropped from `FireflyUtils.h`:** `drawAxes`, `drawMovingPoint`, `drawSpinningTriangle` (immediate-mode GL debug helpers, unused by the controller) and the Windows `to_string` shims (C# has `ToString`).

---

## 3. Behaviour carried over deliberately

Quirks in the original that were reproduced rather than fixed. None of these are bugs I introduced.

**`SparkleShader` ignores its constructor arguments.** `renderPixel` reads the `SPARKLE_*` macros, not the `sparkleRise`/`sparkleFall`/etc. fields the constructor stores. All three registered presets therefore behave identically. Ported as-is.

**`SparkleShader::getStateKey` returns on its first line.** The per-instance key-building code below it is unreachable, so every shader instance shares the key `"sparkleState"`. Ported as-is — the dead code is dropped since C# warns on unreachable statements, but the returned value is identical.

**`applyIntensity` computes `multiplier` then overwrites it.** The original has `//HACK` above `multiplier = intensity;`. Kept, comment included.

**`AnimationSetPlayer`'s three lifecycle methods are empty.** `readyForNextAnimation()` and `finished()` fall off the end without returning in C++. C# requires a return, so they return `false` — which is what the C++ would produce in practice. Nothing calls them; the player is never nested inside another player.

**`randomizeShaders` overwrites its random count with `1`.** Kept.

**`AllPixelShaders` registers only the NULL entry.** The three Sparkle presets stay commented out, exactly as in the source.

**`FFC_MAX_SMOOTHING` and `FFC_MIN_SMOOTHING` are both `1`,** which makes the adaptive smoothing loop inert. Kept, along with the loop.

**`getMedianRadius()` returns `0.0`.** Kept.

**`//TODO: These should obviously be child classes`** on the `PixelStage` constructor switch. Kept, comment included.

**`// TEMP HACK MOVE BACK TO PRIVATE`** on `PixelStage.pixels` / `pixelsLen`. Kept.

---

## 4. One thing the original left undefined

`GenerateCylinderWithAnchors` fills pixels from index 0 up to the last anchor. If an anchor table doesn't reach `CYL_LEDS-1`, the C++ left the remaining entries default-constructed at the origin. C# would leave them `null` and throw on first access, so the port fills any gap with a default `Pixel`. Same behaviour, no crash. The V1 table ends at 1151 against `CYL_LEDS` of 1440, so this path is live whenever the V1 stage is selected.

---

## 5. Not yet verified

I can't run Unity. Nothing here has been compiled. Expect a first-compile error pass — most likely candidates:

- `RenderParams` / `Graphics.RenderMeshInstanced` signature drift between Unity versions
- `_BaseColor` per-instance property support in the URP Unlit shader
- `System.IO.Ports` availability, which depends on Api Compatibility Level being `.NET Framework`
- The Input System package being present and enabled (it ships with the URP template)
