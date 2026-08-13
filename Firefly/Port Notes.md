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
| `action != GLFW_RELEASE` (auto-repeat: `=`, `-`, calibration arrows) | `KeyRepeat.Poll` |
| `action == GLFW_RELEASE` (`Esc`) | `wasReleasedThisFrame` |
| mouse button / position / scroll callbacks | `Mouse.current` |

No functional difference. No Player Settings change needed — the Input System is what Unity 6's URP template ships with.

**Key auto-repeat had to be rebuilt.** GLFW delivered `GLFW_PRESS` then a stream of `GLFW_REPEAT` events at the OS's key-repeat rate — roughly a 500 ms delay then ~30/second. The Input System has no equivalent; `isPressed` is just true every frame. Firefly runs at ~600 FPS, so a held `=` stepped the power target 600 times a second with no initial delay, crossing the entire 500–9000 mA range in about 57 ms. `FireflyController.PollRepeat` reimplements the OS cadence and is used for `=`, `-` and the calibration arrows.

**Mouse Y is inverted.** GLFW's Y grows downward, Unity's upward. `cam.Rotate` takes `(lastY - yPos)` where the C++ took `(yPos - lastY)`, so dragging feels the same.

**Y-up throughout.** The C++ was Z-up: it passed `(0,0,1)` to `gluLookAt` and `PixelStage` built the helix rising in Z with the circle in x/y. The port is Unity-standard Y-up — circle in x/z, rise in y — changed at the three places that name an axis:

- `GenerateCylinderWithAnchors` swaps the y and z terms
- `FireflyCamera.ApplyTo` orbits in x/z and raises in y
- the calibration camera follow reads `GetY()` rather than `GetZ()`

Everything else is axis-agnostic. No animation, primitive, colour or easing code was touched.

**Pixel drawing.** `Pixel::render` / `Pixel::drawSphere` built a sphere per pixel from `glBegin`/`glVertex3f` triangle strips, every frame. Now Unity's sphere primitive supplies the mesh and it's drawn with `Graphics.RenderMeshInstanced` in batches of 1023 — one draw call per batch instead of 1,440 immediate-mode spheres. `Pixel` keeps its position and colour; the drawing methods are gone.

**Cylinder walls.** `drawDefaultCylinderWalls` emitted a `GL_TRIANGLE_STRIP` each frame. Now Unity's cylinder primitive, created once and scaled to the same radius and height, with `CYL_DARKNESS` and `CYL_ALPHA` on a transparent material and culling off so it reads as a shell from any angle (GL was drawing it unculled).

**Window size and title.** `Firefly.cpp` passed `WINDOW_WIDTH`, `WINDOW_HEIGHT` and `WINDOW_TITLE` to the controller, which handed them to `glfwCreateWindow`. `Firefly.cs` passes the same three; `InitRendering` applies the size via `Screen.SetResolution` (a no-op in the Editor, which sizes the Game view itself). **The title can't be set from code** — Unity has no runtime window-title API, and `Application.productName` is read-only. The built player's title comes from Player Settings → Product Name, which should be set to "Firefly Controller".

`FireflyController`'s own `DEFAULT_WINDOW_*` constants remain unreferenced, exactly as in the C++: the header declared them but the constructor took the values as arguments with no defaults, so `Firefly.cpp`'s copies always won.

### Constants referenced nowhere

Audited 2026-08-11. Twelve in total; nine were already dead in the C++.

| Constant | Dead in |
|---|---|
| `DEFAULT_WINDOW_WIDTH` / `_HEIGHT` / `_TITLE` | C++ — superseded by `Firefly.cpp`'s own copies |
| `SA_RINGS_PER_SECOND_RANGE` / `_CYCLE` | C++ — `updateInternal` only used `_AVG`. Ring *size* got the range-and-cycle treatment; ring *speed* never did |
| `RSP_MIN_SATURATION` / `RSP_MAX_SATURATION` | C++ — the palette uses its `saturation` field and a literal `1.0` |
| `RDP_MAX_SATURATION` | C++ — only `RDP_MIN_SATURATION` is used |
| `CYL_STACKS` | C++ |
| `PIXEL_SLICES` / `PIXEL_STACKS` / `CYL_SLICES` | **The port** — see below |

**Tessellation constants are inert.** `PIXEL_SLICES`, `PIXEL_STACKS` and `CYL_SLICES` no longer have an effect — `GameObject.CreatePrimitive` returns a fixed baked mesh with no tessellation parameters. Kept as a record of what the original drew at. Denis's call (2026-08-11): the intent was a ball at each pixel and a translucent cylinder around it, not a specific vertex count, and the primitives are simpler. `CYL_STACKS` was already dead in the C++ — declared but never referenced, since the wall was a single strip with no vertical subdivision.

**Materials are assets in `Assets/Resources/`, not built from shaders at runtime.** Getting the shader into the build isn't enough. Unity also strips shader *variants*, and `#pragma multi_compile_instancing` produces an `INSTANCING_ON` variant that only survives if some material in the build uses it with GPU instancing enabled. A material constructed at runtime is invisible to that analysis, so the variant was stripped and `Graphics.RenderMeshInstanced` silently fell back to the non-instanced path — every pixel drawn at one transform in the material's default colour, with no error logged. `FireflyPixel.mat` carries `m_EnableInstancingVariants: 1`, which keeps the variant.

**Shaders live in `Assets/Resources/`, and are loaded with `Resources.Load`, not `Shader.Find`.** Unity only includes a shader in a build if something references it. Both Firefly shaders are referenced solely from code, so the build stripped them and `Shader.Find` returned null — including the stock `Universal Render Pipeline/Unlit` used as a fallback. `new Material(null)` then threw inside `InitRendering`, which aborted `Start()` before the stage, camera, animation or serial port existed, and `Update()` threw every frame after that. In the Editor none of this shows up, because the Editor has every shader loaded. Everything under `Resources/` is always included in a build, which makes the fix travel with the code rather than depending on the Always Included Shaders project setting.

**Custom shader for per-pixel colour.** The C++ called `glColor3f` before each pixel's sphere. Instancing needs the equivalent as a per-instance shader property, and URP's stock Unlit declares `_BaseColor` inside `CBUFFER_START(UnityPerMaterial)` to stay SRP Batcher compatible — which makes it per-*material*, so a `MaterialPropertyBlock` vector array can't vary it per instance and every pixel draws the same colour. `Assets/Shaders/FireflyInstancedUnlit.shader` declares it with `UNITY_DEFINE_INSTANCED_PROP` instead. The cylinder wall still uses stock URP Unlit — it's one object and needs transparency, not instancing.

**Serial.** `Serial.h` `#ifdef`-ed between `Serial-PC.cpp` (Win32 `CreateFile`/`WriteFile`) and `arduino-serial-lib.cpp` (POSIX `termios`). `System.IO.Ports.SerialPort` covers both, so the split collapses to a few platform-conditional settings. Port names are unchanged: `COM9` and `/dev/cu.usbmodem27946701`.

Carried across from `Serial-PC.cpp`, all of which the first draft of this port missed:

- **Baud is 1,000,000 on Windows, not 9,600.** `Serial.cpp`'s `COM_BAUD` of 9600 was only ever passed to the Mac path; the Windows DCB hardcoded `BaudRate = 1000000`. Both are moot on a Teensy, whose USB CDC ignores baud entirely, but the values now match.
- **DTR asserted.** The DCB set `DTR_CONTROL_ENABLE`. The Mac path's DTR ioctls are commented out; asserting it there is harmless.
- **Buffers purged and a 2-second wait after connecting.** `PurgeComm(PURGE_RXCLEAR | PURGE_TXCLEAR)` then `Sleep(ARDUINO_WAIT_TIME)`.

`WriteTimeout` is `InfiniteTimeout` because both originals used blocking writes, and a failed write logs rather than closing the port.

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

## 3a. Additions with no counterpart in the original

Audited 2026-08-11 after Denis asked where logic had been changed without being asked. These are the only places the port does something the C++ didn't.

**Bounds guard in `CylinderCalibration.LightPixels`.** The C++ coloured `pixels[iter->first]` for every anchor with no range check; an anchor key past the end wrote out of bounds. C# throws instead, so the port skips keys `>= pixelsLen`. Additive, but the alternative is a crash.

**Initialised locals in `CylinderCalibration.RadialAtIndex`.** The C++ declared `nextAnchor` and `nextRadial` uninitialised and read them if the loop never matched. C# won't compile that, so they start at 1 and 1.0 as they do in `nearestIndexToRadial` — the one place the original did initialise them.

**Filling anchor-table gaps in `GenerateCylinderWithAnchors`.** `new Pixel[CYL_LEDS]` default-constructs every element in C++ and leaves nulls in C#, so the port fills any pixel the anchor table didn't reach with a default `Pixel`. Identical end state. Live whenever the V1 stage is selected, since its table stops at 1151 against 1440 LEDs.

**Exception handling around `SerialPort.Open`.** `Serial-PC.cpp` printed an error and left `connected = false`; the port catches and leaves `port = null`. Same outcome by the only means C# offers.

Two changes were made and then reverted once they turned out to be unrequested rather than necessary:

- **RNG seeding.** The port briefly seeded from `Environment.TickCount`. `Firefly.cpp` never calls `srand()`, so C's `rand()` always started from the default seed of 1 and was merely advanced a clock-derived number of steps. Now seeded with 1 to match.
- **Hue wrapping in `HSVtoRGB`.** The port briefly wrapped negative hues into range. `fmod` doesn't, and no palette ever produces a negative hue. Reverted to a plain `%`.

## 4. One thing the original left undefined

`GenerateCylinderWithAnchors` fills pixels from index 0 up to the last anchor. If an anchor table doesn't reach `CYL_LEDS-1`, the C++ left the remaining entries default-constructed at the origin. C# would leave them `null` and throw on first access, so the port fills any gap with a default `Pixel`. Same behaviour, no crash. The V1 table ends at 1151 against `CYL_LEDS` of 1440, so this path is live whenever the V1 stage is selected.

---

## 5. Not yet verified

I can't run Unity. Nothing here has been compiled. Expect a first-compile error pass — most likely candidates:

- `RenderParams` / `Graphics.RenderMeshInstanced` signature drift between Unity versions
- `_BaseColor` per-instance property support in the URP Unlit shader
- `System.IO.Ports` availability, which depends on Api Compatibility Level being `.NET Framework`
- The Input System package being present and enabled (it ships with the URP template)
