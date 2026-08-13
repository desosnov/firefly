using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Firefly
{
    /// <summary>
    /// Port of FireflyController.h / .cpp.
    ///
    /// GLFW window creation, the GL state setup and the while(!shouldClose) loop are
    /// replaced by Unity's lifecycle: Start() is the constructor plus start(), Update()
    /// is one pass of render(). Input moves from GLFW callbacks to polling. The
    /// per-pixel immediate-mode sphere drawing becomes instanced rendering.
    /// See Port Notes.
    /// </summary>
    public class FireflyController : MonoBehaviour
    {
        public const int DEFAULT_WINDOW_WIDTH = 1920;
        public const int DEFAULT_WINDOW_HEIGHT = 1280;
        public const string DEFAULT_WINDOW_TITLE = "Firefly Controller";

        public const int FFC_MAX_SMOOTHING = 1;
        public const int FFC_MIN_SMOOTHING = 1;

        public const double CAM_SPEED_HORIZ = 0.5;
        public const double CAM_SPEED_VERT = 0.3;

        // Configured in Firefly.cs, matching the arguments Firefly.cpp's main() passed
        // to the FireflyController constructor.
        public string windowTitle = DEFAULT_WINDOW_TITLE;
        public int windowWidth = DEFAULT_WINDOW_WIDTH;
        public int windowHeight = DEFAULT_WINDOW_HEIGHT;
        public PixelStageOption stageType = PixelStageOption.FIREFLY_V2_CYLINDER;
        public double animMinDuration = 10.0;
        public double animMaxDuration = 75.0;

        private FireflyCamera cam;
        private PixelStage stage;
        private CylinderCalibration calibration;
        private AAnimation activeAnim;
        private Serial serial;
        private WifiTransport wifi;
        private ATransport activeTransport;

        private double lastX, lastY;
        private bool moveCamera;
        private bool cameraAutoSpin;

        private double nextUpdateTime = 5.0;
        private int frameCount = 0, smoothingFrames = 1;
        private double timeAnchor = 0.0, lastFrame = 0.0, processTime = 0.0, outputTime = 0.0, lastProcessTime = 0.0, lastOutputTime = 0.0;
        private double cumulativePowerDraw = 0.0;

        private double startClock;

        // Rendering resources
        private Camera unityCamera;
        private Mesh sphereMesh;
        private Material pixelMaterial;
        private Matrix4x4[] matrixBatch;
        private Vector4[] colorBatch;
        private MaterialPropertyBlock propertyBlock;
        private Vector3[] smoothedColors;
        private GameObject cylinderWalls;

        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private const int INSTANCE_BATCH = 1023; // Unity's per-call instancing limit

        void Start()
        {
            // A GLFW window kept rendering whether or not it had focus. Unity stops
            // by default, which would freeze the sculpture the moment another window
            // is clicked. Mirrors the Run In Background player setting, so the
            // behaviour survives a fresh clone.
            Application.runInBackground = true;

            // Firefly.cpp advances the (unseeded) global rand() a clock-derived
            // number of steps before anything else.
            int randVal = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) % 123;
            for (int i = 0; i < randVal; i++) FireflyUtils.Rand();

            InitRendering();

            stage = new PixelStage(stageType);

            cam = new FireflyCamera();
            cam.MoveTo(stage.GetCentroid());

            //activeAnim = new SpheresAnimation(stage);
            //activeAnim = new BalloonsAnimation(stage);
            activeAnim = new AnimationSetPlayer(
                stage,
                new AllAnimations(stage),
                new AllPixelShaders(),
                new AllColorPalettes(),
                new AllColorSchemes(),
                animMinDuration, animMaxDuration);

            lastX = -1.0;
            lastY = -1.0;
            moveCamera = false;
            cameraAutoSpin = false;

            serial = new Serial();
            serial.InitComms();
            portField = serial.CurrentPort;

            wifi = new WifiTransport();
            if (serial.Available()) activeTransport = serial;

            calibration = null;
            FireflyUtils.Log("[FFC] Finish controller instantiation");

            smoothedColors = new Vector3[stage.pixelsLen];
            BuildCylinderWalls();

            startClock = Time.realtimeSinceStartupAsDouble;
            activeAnim.Init(0.0);
        }

        void OnDestroy()
        {
            if (serial != null) serial.Close();
            if (wifi != null) wifi.Close();
            FireflyUtils.Log("[FFC] Shut down");
        }

        void Update()
        {
            HandleInput();
            Render(Time.realtimeSinceStartupAsDouble - startClock);
        }

        private void Render(double time)
        {
            if (smoothingFrames < FFC_MAX_SMOOTHING && (smoothingFrames + 1.0) / smoothingFrames * lastProcessTime / (lastProcessTime + lastOutputTime) < 0.8)
            {
                smoothingFrames++;
                Debug.Log(string.Format("[FFC] Increase smoothing to {0}", smoothingFrames));
            }
            else if (smoothingFrames > FFC_MIN_SMOOTHING && lastProcessTime / (lastProcessTime + lastOutputTime) > 0.85)
            {
                smoothingFrames--;
                Debug.Log(string.Format("[FFC] Decrease smoothing to {0}", smoothingFrames));
            }

            if (time > nextUpdateTime)
            {
                Debug.Log(string.Format(
                    "[FFC] {0:F2} sec | {1:F1} FPS | {2:F2}ms process | {3:F2}ms output\n[FFC] {4:F2}% brightness | {5:F2} mA avg | {6} smoothing",
                    nextUpdateTime,
                    frameCount / 5.0,
                    processTime * 1000.0 / frameCount,
                    outputTime * 1000.0 / frameCount,
                    stage.GetBrightness() * 100.0,
                    cumulativePowerDraw / frameCount,
                    smoothingFrames));
                nextUpdateTime += 5.0;
                processTime = 0.0;
                outputTime = 0.0;
                frameCount = 0;
                cumulativePowerDraw = 0.0;
            }

            timeAnchor = Time.realtimeSinceStartupAsDouble;

            if (cameraAutoSpin) cam.Rotate(0.5, 0.0);
            if (calibration != null)
            {
                // Follows the pixel's height. Y rather than Z now the world is Y-up.
                Vector3 camPos = cam.GetPos();
                camPos.y = (float)calibration.PixelInFocus().GetY();
                cam.MoveTo(camPos);
            }
            cam.ApplyTo(unityCamera);

            ClearStage();

            if (calibration != null)
            {
                calibration.LightPixels(time);
            }
            else if (smoothingFrames == 1)
            {
                activeAnim.Render(time);
            }
            else
            {
                double smoothingInterval = (time - lastFrame) / (double)smoothingFrames;

                for (int f = 0; f < smoothingFrames; f++)
                {
                    lastFrame += smoothingInterval;
                    activeAnim.Render(lastFrame);

                    for (int p = 0; p < stage.pixelsLen; p++)
                    {
                        if (f == 0) smoothedColors[p] = stage.pixels[p].GetColor();
                        else smoothedColors[p] += stage.pixels[p].GetColor();
                    }
                }
                for (int p = 0; p < stage.pixelsLen; p++)
                {
                    stage.pixels[p].SetColor(smoothedColors[p] / (float)smoothingFrames);
                }
            }
            lastFrame = time;

            RenderGL();
            lastProcessTime = Time.realtimeSinceStartupAsDouble - timeAnchor;
            processTime += lastProcessTime;
            timeAnchor = Time.realtimeSinceStartupAsDouble;

            if (activeTransport != null && activeTransport.Available()) stage.RenderLED(activeTransport);

            lastOutputTime = Time.realtimeSinceStartupAsDouble - timeAnchor;
            outputTime += lastOutputTime;
            frameCount++;
            cumulativePowerDraw += stage.GetPowerDraw();
        }

        private void ClearStage()
        {
            for (int p = 0; p < stage.pixelsLen; p++)
            {
                stage.pixels[p].SetColor(new Vector3(0.0f, 0.0f, 0.0f));
            }
        }

        // ── Rendering ───────────────────────────────────────────

        private void InitRendering()
        {
            // initGL's glfwCreateWindow(width, height, title). Unity owns the window,
            // so the size is applied through Screen; the title can only be set from
            // Player Settings (Product Name) — there is no runtime API for it.
            // No-op in the Editor, which sizes the Game view itself.
            Screen.SetResolution(windowWidth, windowHeight, FullScreenMode.Windowed);

            unityCamera = Camera.main;
            if (unityCamera == null)
            {
                GameObject camObj = new GameObject("Firefly Camera");
                unityCamera = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }
            unityCamera.clearFlags = CameraClearFlags.SolidColor;
            unityCamera.backgroundColor = Color.black;
            unityCamera.fieldOfView = 60.0f;   // gluPerspective(60.0, ...)
            unityCamera.nearClipPlane = 0.2f;
            unityCamera.farClipPlane = 100.0f;

            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);

            // Loaded as a material asset, not built from a shader at runtime. A
            // material asset in Resources with Enable GPU Instancing ticked is what
            // tells the build to keep the shader's INSTANCING_ON variant; a material
            // constructed at runtime is invisible to variant stripping, and without
            // that variant every pixel draws at one transform. See Port Notes.
            pixelMaterial = new Material(Resources.Load<Material>("FireflyPixel"));
            pixelMaterial.enableInstancing = true;

            matrixBatch = new Matrix4x4[INSTANCE_BATCH];
            colorBatch = new Vector4[INSTANCE_BATCH];
            propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Port of PixelStage::renderGL. One instanced draw per batch of 1023 rather
        /// than an immediate-mode sphere per pixel.
        /// </summary>
        private void RenderGL()
        {
            float r = (float)(stage.GetDrawRadius() * 2.0); // Unity's sphere is diameter 1
            Vector3 scale = new Vector3(r, r, r);

            RenderParams rp = new RenderParams(pixelMaterial);
            rp.matProps = propertyBlock;

            int i = 0;
            while (i < stage.pixelsLen)
            {
                int count = Math.Min(INSTANCE_BATCH, stage.pixelsLen - i);
                for (int b = 0; b < count; b++)
                {
                    Pixel px = stage.pixels[i + b];
                    matrixBatch[b] = Matrix4x4.TRS(px.GetPos(), Quaternion.identity, scale);
                    Vector3 c = px.GetColor();
                    colorBatch[b] = new Vector4(c.x, c.y, c.z, 1.0f);
                }

                propertyBlock.SetVectorArray(BaseColorID, colorBatch);
                Graphics.RenderMeshInstanced(rp, sphereMesh, 0, matrixBatch, count);

                i += count;
            }
        }

        /// <summary>Port of drawDefaultCylinderWalls, as a persistent transparent mesh.</summary>
        private void BuildCylinderWalls()
        {
            if (!stage.ShouldDrawCylinderWalls()) return;

            double radius = (PixelStage.CYL_DIAM / 2.0 - PixelStage.CYL_PIXEL_RADIUS) * PixelStage.SCALE_FACTOR;
            double height = PixelStage.CYL_HEIGHT * PixelStage.SCALE_FACTOR;

            cylinderWalls = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinderWalls.name = "Cylinder Walls";
            Destroy(cylinderWalls.GetComponent<Collider>());

            // Unity's cylinder is 2 units tall, 1 unit across, centred on its origin.
            // Firefly's runs 0..CYL_HEIGHT in Y around (0.5, 0.5) in x/z.
            cylinderWalls.transform.position = new Vector3(0.5f, (float)(height / 2.0), 0.5f);
            cylinderWalls.transform.localScale = new Vector3((float)(radius * 2.0), (float)(height / 2.0), (float)(radius * 2.0));

            // Blending and double-sidedness are declared in the shader rather than
            // poked into a stock URP material's keywords at runtime.
            Material wallMat = new Material(Resources.Load<Material>("FireflyWall"));
            wallMat.SetColor(BaseColorID, new Color(PixelStage.CYL_DARKNESS, PixelStage.CYL_DARKNESS, PixelStage.CYL_DARKNESS, PixelStage.CYL_ALPHA));
            cylinderWalls.GetComponent<MeshRenderer>().material = wallMat;
        }

        // ── Connection UI ───────────────────────────────────────
        //
        // Not part of the port. The C++ had the device name in a #define and knew
        // only about serial. IMGUI is used deliberately: it needs no canvas, no
        // prefabs and nothing wired up in the editor, which keeps the "open and press
        // Play" property. See Port Notes §3b.

        private const float PANEL_MARGIN = 10f;
        private const float PANEL_WIDTH = 220f;

        private string portField = "";
        private string ssidField = "";
        private string passField = "";
        private List<FireflyDevice> networkDevices = new List<FireflyDevice>();
        private FireflyDevice pendingDevice;   // found on its own AP, awaiting credentials

        void OnGUI()
        {
            if (serial == null) return;

            GUILayout.BeginArea(new Rect(PANEL_MARGIN, PANEL_MARGIN, PANEL_WIDTH, Screen.height - PANEL_MARGIN * 2));

            GUILayout.Label(activeTransport != null && activeTransport.Available()
                ? activeTransport.Describe()
                : "Not connected");

            // ── USB ──
            GUILayout.Space(8);
            GUILayout.Label("Port");

            GUILayout.BeginHorizontal();
            portField = GUILayout.TextField(portField);
            bool applyPort = GUILayout.Button("Apply", GUILayout.Width(55));
            GUILayout.EndHorizontal();

            string clickedPort = null;
            for (int i = 0; i < serial.RecentPorts.Count; i++)
            {
                // Captured rather than acted on immediately — TryOpen reorders the
                // list this loop is walking.
                if (GUILayout.Button(serial.RecentPorts[i])) clickedPort = serial.RecentPorts[i];
            }

            // ── Fireflies on the network ──
            GUILayout.Space(10);
            GUILayout.Label("Wifi");
            bool scanNetwork = GUILayout.Button("Scan network");

            FireflyDevice clickedDevice = null;
            for (int i = 0; i < networkDevices.Count; i++)
            {
                if (GUILayout.Button(networkDevices[i].ToString())) clickedDevice = networkDevices[i];
            }
            if (scanNetwork && networkDevices.Count == 0) GUILayout.Label("None found");

            // ── Pairing a new Firefly ──
            GUILayout.Space(10);
            GUILayout.Label("Pair new");
            GUILayout.Label("Join the Firefly's own network first", GUI.skin.box);
            bool scanAP = GUILayout.Button("Scan for new Firefly");

            bool provision = false;
            if (pendingDevice != null)
            {
                GUILayout.Label("Found " + pendingDevice.name);
                GUILayout.Label("Network");
                ssidField = GUILayout.TextField(ssidField);
                GUILayout.Label("Password");
                passField = GUILayout.PasswordField(passField, '*');
                provision = GUILayout.Button("Send credentials");
            }

            GUILayout.EndArea();

            // Acted on after the layout pass, so nothing mutates a list mid-draw.
            if (applyPort) ConnectSerial(portField);
            else if (clickedPort != null) { portField = clickedPort; ConnectSerial(clickedPort); }
            else if (scanNetwork) networkDevices = FireflyDiscovery.ScanNetwork();
            else if (clickedDevice != null) ConnectWifi(clickedDevice);
            else if (scanAP) pendingDevice = FireflyDiscovery.ScanSoftAP();
            else if (provision && FireflyDiscovery.Provision(ssidField, passField))
            {
                pendingDevice = null;
                passField = "";
            }
        }

        private void ConnectSerial(string portName)
        {
            if (serial.TryOpen(portName))
            {
                wifi.Close();
                activeTransport = serial;
            }
        }

        private void ConnectWifi(FireflyDevice device)
        {
            if (wifi.Connect(device))
            {
                serial.Close();
                activeTransport = wifi;
            }
        }

        // ── Input ───────────────────────────────────────────────

        // GLFW delivered GLFW_PRESS then a stream of GLFW_REPEAT events at the OS's
        // key-repeat rate — roughly a 500ms delay then ~30/second. The Input System
        // has no equivalent: isPressed is simply true every frame, which at Firefly's
        // several-hundred FPS steps about twenty times faster than the original and
        // with no initial delay. PollRepeat reproduces the OS cadence.
        private const double KEY_REPEAT_DELAY = 0.5;  // seconds held before repeating
        private const double KEY_REPEAT_RATE = 30.0;  // repeats per second thereafter

        private readonly Dictionary<Key, double> nextKeyRepeat = new Dictionary<Key, double>();

        /// <summary>
        /// True on the frame the key goes down, then again at the repeat rate for as
        /// long as it's held. Equivalent of GLFW's PRESS-then-REPEAT stream.
        /// </summary>
        private bool PollRepeat(Keyboard kb, Key key, double now)
        {
            ButtonControl control = kb[key];

            if (control.wasPressedThisFrame)
            {
                nextKeyRepeat[key] = now + KEY_REPEAT_DELAY;
                return true;
            }

            if (!control.isPressed)
            {
                nextKeyRepeat.Remove(key);
                return false;
            }

            double next;
            if (!nextKeyRepeat.TryGetValue(key, out next)) return false;

            if (now >= next)
            {
                nextKeyRepeat[key] = now + 1.0 / KEY_REPEAT_RATE;
                return true;
            }

            return false;
        }

        private void HandleInput()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (kb == null || mouse == null) return;

            double now = Time.realtimeSinceStartupAsDouble;

            if (kb[Key.Escape].wasReleasedThisFrame)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            // The C++ tested action != GLFW_RELEASE, so these fired on press and then
            // on every OS auto-repeat. KeyRepeat reproduces that cadence.
            if (PollRepeat(kb, Key.Equals, now)) stage.BrightnessUp();
            if (PollRepeat(kb, Key.Minus, now)) stage.BrightnessDown();

            if (kb[Key.Space].wasPressedThisFrame) { }

            if (kb[Key.Backslash].wasPressedThisFrame) activeAnim.ToggleSubpixelSampling();

            if (kb[Key.LeftBracket].wasPressedThisFrame)
            {
                smoothingFrames = Math.Max(1, smoothingFrames - 1);
                Debug.Log(string.Format("[FFC] Decrease smoothing to {0}", smoothingFrames));
            }
            if (kb[Key.RightBracket].wasPressedThisFrame)
            {
                smoothingFrames = Math.Min(5, smoothingFrames + 1);
                Debug.Log(string.Format("[FFC] Increase smoothing to {0}", smoothingFrames));
            }

            bool shift = kb[Key.LeftShift].isPressed || kb[Key.RightShift].isPressed;

            if (calibration != null)
            {
                if (kb[Key.Enter].wasPressedThisFrame) calibration.Select();

                // The C++ handled GLFW_PRESS and GLFW_REPEAT, so holding an arrow
                // key auto-repeats the step.
                if (PollRepeat(kb, Key.LeftArrow, now))
                {
                    if (shift) calibration.GoLeft(10); else calibration.GoLeft(1);
                }
                if (PollRepeat(kb, Key.RightArrow, now))
                {
                    if (shift) calibration.GoRight(10); else calibration.GoRight(1);
                }

                if (kb[Key.C].wasPressedThisFrame)
                {
                    calibration.PrintCalibration();
                    calibration = null;
                    cam.MoveTo(stage.GetCentroid());
                }
            }
            else if (kb[Key.C].wasPressedThisFrame)
            {
                calibration = new CylinderCalibration(stage);
            }

            // Mouse
            if (mouse.leftButton.wasPressedThisFrame)
            {
                cameraAutoSpin = false;
                moveCamera = true;
                Cursor.visible = false;
            }
            if (mouse.leftButton.wasReleasedThisFrame)
            {
                moveCamera = false;
                Cursor.visible = true;
            }
            if (mouse.rightButton.wasPressedThisFrame) cameraAutoSpin = true;

            Vector2 mousePos = mouse.position.ReadValue();
            double xPos = mousePos.x;
            double yPos = mousePos.y;
            if (lastX != -1.0 && moveCamera)
            {
                // Unity's mouse Y grows upward where GLFW's grew downward, so the
                // vertical term is negated relative to the original.
                cam.Rotate((lastX - xPos) * CAM_SPEED_HORIZ, (lastY - yPos) * CAM_SPEED_VERT);
            }
            lastX = xPos;
            lastY = yPos;

            float scroll = mouse.scroll.ReadValue().y;
            if (scroll < 0) cam.ZoomOut();
            if (scroll > 0) cam.ZoomIn();
        }
    }
}
