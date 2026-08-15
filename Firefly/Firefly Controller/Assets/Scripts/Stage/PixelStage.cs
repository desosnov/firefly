using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    public enum PixelStageOption
    {
        DEFAULT_CYLINDER = 1,
        DEFAULT_FILLED_CUBE = 2,
        FIREFLY_V1_CYLINDER = 3,
        FIREFLY_V2_CYLINDER = 4
    }

    public class PixelStage
    {
        public const double CYL_DIAM = 2.54 * 5.75;
        public const double CYL_STRIP_HEIGHT = 1.25;
        public const int CYL_METER_STRIPS = 10;
        public const int CYL_LEDS_PER_METER = 144;
        public const double CYL_LED_DIST = 100.0 / CYL_LEDS_PER_METER;
        public const int CYL_LEDS = CYL_METER_STRIPS * CYL_LEDS_PER_METER;
        public const double SCALE_FACTOR = 0.2 / 2.54; // # of cm in one unit
        public static readonly double CYL_HEIGHT =
            CYL_METER_STRIPS * 100.0 / Math.Sqrt(Math.Pow(CYL_STRIP_HEIGHT, 2) + Math.Pow(CYL_DIAM * FireflyUtils.M_PI, 2)) * CYL_STRIP_HEIGHT;
        public const double CYL_PIXEL_RADIUS = CYL_LED_DIST * SCALE_FACTOR / 2.0;
        public const double CYL_PIXEL_RENDER_RADIUS = CYL_PIXEL_RADIUS * 0.9;

        public const double MILLIAMPS_PER_COLOR = 20.0;

        public const double PS_MAX_POWER = 9000.0;
        public const double PS_MIN_POWER = 500.0;
        public const double PS_DEFAULT_POWER = 3000.0;
        public const double PS_POWER_INTERVAL = 250.0;
        public const double PS_MAX_BRIGHTNESS = 0.45;
        public const double PS_MIN_BRIGHTNESS = 0.05;
        public const double PS_DEFAULT_BRIGHTNESS = 0.2;
        public const double PS_BRIGHTNESS_MOVE_THRESHOLD = 0.02;
        public const double PS_BRIGHTNESS_INTERVAL = 0.1;

        // Cylinder wall drawing
        public const int CYL_SLICES = 24;
        public const int CYL_STACKS = 24;
        public const float CYL_DARKNESS = 0.1f;
        public const float CYL_ALPHA = 0.6f;

        // Filled cube stage
        public const int CUBE_LEDS_PER_SIDE = 15;
        public const double CUBE_SIDE_LENGTH = 1.0;
        public const int CUBE_LEDS = CUBE_LEDS_PER_SIDE * CUBE_LEDS_PER_SIDE * CUBE_LEDS_PER_SIDE;
        public const double CUBE_PIXEL_DISTANCE = CUBE_SIDE_LENGTH / (double)CUBE_LEDS_PER_SIDE;
        public const double CUBE_PIXEL_SPACE_RATIO = 0.2;

        private double pixelRadius = CYL_PIXEL_RADIUS * SCALE_FACTOR;
        private bool drawCylinderWalls = false;

        private float targetPower, brightness;
        private float powerDraw;
        private int lastReportedBrightness;

        private Vector3 center;
        private bool centerFlag;
        private double maxRadius, medianRadius;
        private bool maxRadiusFlag, medianRadiusFlag;

        private SortedList<int, double> cylinderAnchors = new SortedList<int, double>();

        // TEMP HACK MOVE BACK TO PRIVATE  (carried over from the original)
        public int pixelsLen;
        public Pixel[] pixels;

        // Reusable output buffer for renderLED — the C++ used a stack VLA.
        private byte[] pixelOut;

        public PixelStage() : this(PixelStageOption.FIREFLY_V2_CYLINDER) { }

        public PixelStage(PixelStageOption option)
        {
            brightness = (float)PS_DEFAULT_BRIGHTNESS;
            targetPower = (float)PS_DEFAULT_POWER;

            switch (option)
            {
                //TODO: These should obviously be child classes
                case PixelStageOption.DEFAULT_CYLINDER:
                    GenerateDefaultCylinder();
                    break;
                case PixelStageOption.DEFAULT_FILLED_CUBE:
                    GenerateFilledCube();
                    break;
                case PixelStageOption.FIREFLY_V1_CYLINDER:
                    GenerateFireflyV1Cylinder();
                    break;
                case PixelStageOption.FIREFLY_V2_CYLINDER:
                    GenerateFireflyV2Cylinder();
                    break;
                default:
                    break;
            }

            pixelOut = new byte[3 * pixelsLen];
        }

        private void GenerateCylinderWithAnchors(SortedList<int, double> anchors)
        {
            centerFlag = false;
            maxRadiusFlag = false;
            cylinderAnchors = anchors;
            drawCylinderWalls = true;
            pixels = new Pixel[CYL_LEDS];
            pixelsLen = CYL_LEDS;
            pixelRadius = CYL_PIXEL_RENDER_RADIUS;

            double radialInterval;
            double verticalInterval = CYL_STRIP_HEIGHT / (2 * FireflyUtils.M_PI);

            int prevAnchor = 0, nextAnchor;
            double prevAnchorRadial = 0.0, nextAnchorRadial;
            int pixel = 0;
            double curRadial = 0.0;
            foreach (KeyValuePair<int, double> anchor in anchors)
            {
                nextAnchor = anchor.Key;
                nextAnchorRadial = anchor.Value;
                radialInterval = (nextAnchorRadial - prevAnchorRadial) / (nextAnchor - prevAnchor);

                // Y is up (Unity convention). The C++ was Z-up: it put the circle in
                // x/y and the rise in z. Here the circle is in x/z and the rise is y.
                for (pixel = prevAnchor; pixel < nextAnchor; pixel++)
                {
                    pixels[pixel] = new Pixel(new Vector3(
                        (float)(0.5 + Math.Cos(curRadial) * CYL_DIAM / 2.0 * SCALE_FACTOR),
                        (float)(0.0 + curRadial * verticalInterval * SCALE_FACTOR),
                        (float)(0.5 + Math.Sin(curRadial) * CYL_DIAM / 2.0 * SCALE_FACTOR)));
                    curRadial += radialInterval;
                }

                prevAnchor = nextAnchor;
                prevAnchorRadial = nextAnchorRadial;
            }

            pixels[pixel] = new Pixel(new Vector3(
                (float)(0.5 + Math.Cos(curRadial) * CYL_DIAM / 2.0 * SCALE_FACTOR),
                (float)(0.0 + curRadial * verticalInterval * SCALE_FACTOR),
                (float)(0.5 + Math.Sin(curRadial) * CYL_DIAM / 2.0 * SCALE_FACTOR)));

            // Any anchor table that doesn't reach the last LED leaves nulls behind;
            // the C++ left them default-constructed. See Port Notes.
            for (int p = 0; p < pixelsLen; p++)
            {
                if (pixels[p] == null) pixels[p] = new Pixel();
            }
        }

        private void GenerateDefaultCylinder()
        {
            centerFlag = false;
            maxRadiusFlag = false;

            double lengthOfOneLoop = Math.Sqrt(Math.Pow(CYL_STRIP_HEIGHT, 2) + Math.Pow(CYL_DIAM * FireflyUtils.M_PI, 2));
            double ledsPerLoop = lengthOfOneLoop / CYL_LED_DIST;

            SortedList<int, double> anchors = new SortedList<int, double>();
            anchors[CYL_LEDS - 1] = 2.0 * FireflyUtils.M_PI / ledsPerLoop * CYL_LEDS;

            GenerateCylinderWithAnchors(anchors);
        }

        private void GenerateFireflyV1Cylinder()
        {
            centerFlag = false;
            maxRadiusFlag = false;

            SortedList<int, double> a = new SortedList<int, double>();
            a[52] = 6.30062; a[103] = 12.5297; a[141] = 17.1286; a[144] = 17.3632;
            a[156] = 18.8118; a[208] = 25.1225; a[215] = 25.9733; a[216] = 26.1449;
            a[259] = 31.3615; a[287] = 34.7449; a[288] = 35.0065; a[310] = 37.6806;
            a[359] = 43.5766; a[360] = 43.7381; a[362] = 43.9912; a[414] = 50.2718;
            a[431] = 52.3182; a[432] = 52.5797; a[465] = 56.6009; a[503] = 61.2098;
            a[504] = 61.3714; a[516] = 62.83;   a[568] = 69.1406; a[575] = 69.9714;
            a[576] = 70.273;  a[619] = 75.4496; a[647] = 78.833;  a[648] = 79.0246;
            a[671] = 81.8003; a[719] = 87.5847; a[720] = 87.8262; a[722] = 88.0693;
            a[774] = 94.3499; a[791] = 96.4063; a[792] = 96.6078; a[826] = 100.701;
            a[863] = 105.178; a[864] = 105.439; a[877] = 107;     a[929] = 113.26;
            a[935] = 113.98;  a[936] = 114.171; a[981] = 119.601; a[1007] = 122.741;
            a[1008] = 122.993; a[1032] = 125.88; a[1079] = 131.553; a[1080] = 131.714;
            a[1084] = 132.201; a[1136] = 138.481; a[1151] = 140.284;

            GenerateCylinderWithAnchors(a);
        }

        private void GenerateFireflyV2Cylinder()
        {
            centerFlag = false;
            maxRadiusFlag = false;

            SortedList<int, double> a = new SortedList<int, double>();
            a[0] = -0.03;      a[67] = 6.22355;  a[71] = 6.60606;  a[72] = 6.74919;
            a[133] = 12.4561;  a[143] = 13.4032; a[144] = 13.5384; a[199] = 18.6804;
            a[215] = 20.1824;  a[216] = 20.3376; a[266] = 25.0139; a[287] = 26.9816;
            a[288] = 27.1267;  a[333] = 31.3475; a[359] = 33.7708; a[360] = 33.9159;
            a[400] = 37.671;   a[431] = 40.57;   a[432] = 40.7151; a[467] = 43.9946;
            a[503] = 47.3692;  a[504] = 47.5043; a[534] = 50.3181; a[575] = 54.1584;
            a[576] = 54.2935;  a[601] = 56.6417; a[647] = 60.9476; a[648] = 61.0927;
            a[668] = 62.9652;  a[719] = 67.7567; a[720] = 68.0219; a[733] = 69.2385;
            a[791] = 74.6759;  a[792] = 74.8111; a[800] = 75.5621; a[863] = 81.4651;
            a[864] = 81.6002;  a[867] = 81.8856; a[935] = 88.2443; a[936] = 88.3885;
            a[1001] = 94.4727; a[1007] = 95.0435; a[1008] = 95.1786; a[1068] = 100.786;
            a[1078] = 101.738; a[1079] = 101.823; a[1080] = 101.958; a[1081] = 102.053;
            a[1083] = 102.243; a[1087] = 102.614; a[1135] = 107.1;  a[1151] = 108.602;
            a[1152] = 108.747; a[1202] = 113.433; a[1223] = 115.391; a[1224] = 115.536;
            a[1269] = 119.757; a[1295] = 122.19; a[1296] = 122.335; a[1336] = 126.08;
            a[1367] = 128.979; a[1368] = 129.125; a[1403] = 132.394; a[1439] = 135.779;

            GenerateCylinderWithAnchors(a);
        }

        public SortedList<int, double> GetAnchors() { return cylinderAnchors; }

        public void SetAnchors(SortedList<int, double> anchors)
        {
            GenerateCylinderWithAnchors(anchors);
        }

        private void GenerateFilledCube()
        {
            centerFlag = false;
            maxRadiusFlag = false;

            drawCylinderWalls = false;
            pixels = new Pixel[CUBE_LEDS];
            pixelsLen = CUBE_LEDS;
            pixelRadius = CUBE_PIXEL_DISTANCE * CUBE_PIXEL_SPACE_RATIO;

            int pi = 0;
            for (int xi = 0; xi < CUBE_LEDS_PER_SIDE; xi++)
            {
                for (int yi = 0; yi < CUBE_LEDS_PER_SIDE; yi++)
                {
                    for (int zi = 0; zi < CUBE_LEDS_PER_SIDE; zi++)
                    {
                        pixels[pi] = new Pixel(new Vector3(
                            (float)(xi * CUBE_PIXEL_DISTANCE),
                            (float)(yi * CUBE_PIXEL_DISTANCE),
                            (float)(zi * CUBE_PIXEL_DISTANCE)));
                        pi++;
                    }
                }
            }
        }

        public bool ShouldDrawCylinderWalls() { return drawCylinderWalls; }

        /// <summary>
        /// Port of renderLED. Writes 3 bytes per pixel to whichever transport is
        /// connected — the C++ only knew about serial. See Port Notes §3b.
        /// </summary>
        public void RenderLED(ATransport transport)
        {
            if (powerDraw > 0.0 && Math.Abs(targetPower / powerDraw - 1.0) > PS_BRIGHTNESS_MOVE_THRESHOLD)
            {
                int oldBrightness = (int)Math.Floor(brightness * 5.0);
                brightness = (float)Math.Max(PS_MIN_BRIGHTNESS, Math.Min(PS_MAX_BRIGHTNESS,
                    brightness * ((targetPower / powerDraw - 1.0) * PS_BRIGHTNESS_INTERVAL + 1.0)));
                if ((int)Math.Floor(brightness * 5.0) != oldBrightness && (int)Math.Round(brightness * 5.0) != lastReportedBrightness)
                {
                    Debug.Log(string.Format("[PS] Auto-brightness at {0:F2}%", brightness * 100.0));
                    lastReportedBrightness = (int)Math.Round(brightness * 5.0);
                }
            }
            powerDraw = 0.0f;

            Vector3 color;
            int r, g, b;

            for (int pi = 0; pi < pixelsLen; pi++)
            {
                color = pixels[pi].GetColor();
                r = Math.Max(Math.Min((int)Math.Round(color.x * brightness * 255), 255), 0);
                g = Math.Max(Math.Min((int)Math.Round(color.y * brightness * 255), 255), 0);
                b = Math.Max(Math.Min((int)Math.Round(color.z * brightness * 255), 255), 0);

                pixelOut[pi * 3] = (byte)r;
                pixelOut[pi * 3 + 1] = (byte)g;
                pixelOut[pi * 3 + 2] = (byte)b;

                powerDraw += ((float)(r + g + b)) / 255.0f * (float)MILLIAMPS_PER_COLOR;
            }

            transport.Write(pixelOut, 3 * pixelsLen);
        }

        public Vector3 GetCentroid()
        {
            if (centerFlag) return center;

            center = new Vector3(0.0f, 0.0f, 0.0f);
            for (int p = 0; p < pixelsLen; p++)
            {
                center += pixels[p].GetPos();
            }

            center /= (float)pixelsLen;
            centerFlag = true;
            return center;
        }

        public double GetMaxRadius()
        {
            if (maxRadiusFlag) return maxRadius;

            if (!centerFlag) GetCentroid();

            maxRadius = 0.0;
            for (int pi = 0; pi < pixelsLen; pi++)
            {
                double dist = (center - pixels[pi].GetPos()).magnitude;
                if (dist > maxRadius) maxRadius = dist;
            }
            maxRadiusFlag = true;
            return maxRadius;
        }

        public double GetMedianRadius() { return 0.0; }

        public double GetPixelRadius() { return CYL_PIXEL_RADIUS; }

        /// <summary>Radius used for drawing, as distinct from GetPixelRadius.</summary>
        public double GetDrawRadius() { return pixelRadius; }

        public void BrightnessUp()
        {
            targetPower = (float)Math.Min(targetPower + PS_POWER_INTERVAL, PS_MAX_POWER);
            Debug.Log(string.Format("Target power use: {0:F2} | Cur brightness: {1:F2}", targetPower, brightness));
        }

        public void BrightnessDown()
        {
            targetPower = (float)Math.Max(targetPower - PS_POWER_INTERVAL, PS_MIN_POWER);
            Debug.Log(string.Format("Target power use: {0:F2} | Cur brightness: {1:F2}", targetPower, brightness));
        }

        public float GetBrightness() { return brightness; }
        public float GetTargetPower() { return targetPower; }
        public float GetPowerDraw() { return powerDraw; }
    }
}
