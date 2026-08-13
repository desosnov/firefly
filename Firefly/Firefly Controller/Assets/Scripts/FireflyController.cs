using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        // Configured in Firefly.cs, matching Firefly.cpp's main().
        public PixelStageOption stageType = PixelStageOption.FIREFLY_V2_CYLINDER;
        public double animMinDuration = 10.0;
        public double animMaxDuration = 75.0;

        private FireflyCamera cam;
        private PixelStage stage;
        private CylinderCalibration calibration;
        private AAnimation activeAnim;
        private Serial serial;

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
            // Firefly.cpp seeds the RNG from the clock before anything else.
            int randVal = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds()) % 123;
            FireflyUtils.Seed(Environment.TickCount);
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

            if (serial.Available()) stage.RenderLED(serial);

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

            sphereMesh = MeshBuilder.BuildSphere(Pixel.PIXEL_SLICES, Pixel.PIXEL_STACKS);

            // Must be the custom shader: URP's stock Unlit keeps _BaseColor in the
            // UnityPerMaterial cbuffer, so it can't vary per instance.
            Shader shader = Shader.Find("Firefly/InstancedUnlit");
            if (shader == null)
            {
                Debug.LogError("[FFC] Firefly/InstancedUnlit shader not found — every pixel will draw the same colour.");
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            pixelMaterial = new Material(shader);
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

            // Built at CYL_SLICES segments, matching the original triangle strip,
            // with the geometry baked in world space exactly as the C++ emitted it.
            Mesh wallMesh = MeshBuilder.BuildCylinderWall(PixelStage.CYL_SLICES, radius, height, 0.5, 0.5);

            cylinderWalls = new GameObject("Cylinder Walls");
            cylinderWalls.AddComponent<MeshFilter>().sharedMesh = wallMesh;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            Material wallMat = new Material(shader);
            wallMat.SetColor(BaseColorID, new Color(PixelStage.CYL_DARKNESS, PixelStage.CYL_DARKNESS, PixelStage.CYL_DARKNESS, PixelStage.CYL_ALPHA));
            SetMaterialTransparent(wallMat);
            // Open tube with no caps — render both faces so it reads as a translucent
            // shell from any angle, as it did under GL with no culling.
            wallMat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);
            cylinderWalls.AddComponent<MeshRenderer>().material = wallMat;
        }

        private static void SetMaterialTransparent(Material m)
        {
            m.SetFloat("_Surface", 1.0f); // Transparent
            m.SetFloat("_Blend", 0.0f);   // Alpha
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.DisableKeyword("_ALPHATEST_ON");
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        // ── Input ───────────────────────────────────────────────

        private void HandleInput()
        {
            Keyboard kb = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (kb == null || mouse == null) return;

            if (kb[Key.Escape].wasReleasedThisFrame)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }

            // Held, not tapped — the C++ tested action != GLFW_RELEASE, so these repeat.
            if (kb[Key.Equals].isPressed) stage.BrightnessUp();
            if (kb[Key.Minus].isPressed) stage.BrightnessDown();

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
                if (kb[Key.LeftArrow].wasPressedThisFrame)
                {
                    if (shift) calibration.GoLeft(10); else calibration.GoLeft(1);
                }
                if (kb[Key.RightArrow].wasPressedThisFrame)
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
