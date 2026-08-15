using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    public enum Phase
    {
        ANCHOR_SELECTION = 1,
        REFERENCE_SELECTION = 2,
        MOVEMENT = 3
    }

    public class CylinderCalibration
    {
        public const double CALIBRATION_ANCHOR_MOVE_INTERVAL = .01;
        public static readonly Vector3 CALIBRATION_DEFAULT_COLOR = new Vector3(0.2f, 0.2f, 0.2f);
        public static readonly Vector3 CALIBRATION_SELECTED_COLOR = new Vector3(1.0f, 0.0f, 0.0f);
        public static readonly Vector3 CALIBRATION_ANCHOR_COLOR = new Vector3(0.0f, 0.0f, 1.0f);

        private PixelStage pixels;
        private int anchor;
        private int reference;
        private Phase phase;

        private SortedList<int, double> anchors;

        public CylinderCalibration(PixelStage pixelStage)
        {
            pixels = pixelStage;
            anchor = 0;
            reference = 0;
            phase = Phase.ANCHOR_SELECTION;
            anchors = new SortedList<int, double>(pixels.GetAnchors());
        }

        public void GoLeft(int increment)
        {
            switch (phase)
            {
                case Phase.ANCHOR_SELECTION:
                    if (anchor > increment) anchor -= increment;
                    else anchor = 0;
                    break;
                case Phase.REFERENCE_SELECTION:
                    if (reference > increment) reference -= increment;
                    else reference = 0;
                    break;
                case Phase.MOVEMENT:
                    ShiftAnchorsFrom(anchor, -increment * CALIBRATION_ANCHOR_MOVE_INTERVAL);
                    pixels.SetAnchors(anchors);
                    break;
            }
        }

        public void GoRight(int increment)
        {
            switch (phase)
            {
                case Phase.ANCHOR_SELECTION:
                    anchor += increment;
                    break;
                case Phase.REFERENCE_SELECTION:
                    reference += increment;
                    break;
                case Phase.MOVEMENT:
                    ShiftAnchorsFrom(anchor, increment * CALIBRATION_ANCHOR_MOVE_INTERVAL);
                    pixels.SetAnchors(anchors);
                    break;
            }
        }

        /// <summary>
        /// The C++ walked from anchors.find(anchor) to the end, shifting each radial.
        /// If the key isn't present, find() returns end() and nothing moves.
        /// </summary>
        private void ShiftAnchorsFrom(int fromKey, double delta)
        {
            if (!anchors.ContainsKey(fromKey)) return;

            IList<int> keys = anchors.Keys;
            bool started = false;
            List<int> toShift = new List<int>();
            for (int i = 0; i < keys.Count; i++)
            {
                if (keys[i] == fromKey) started = true;
                if (started) toShift.Add(keys[i]);
            }
            foreach (int k in toShift) anchors[k] += delta;
        }

        public void Select()
        {
            double anchorRadial;

            switch (phase)
            {
                case Phase.ANCHOR_SELECTION:
                    anchorRadial = RadialAtIndex(anchor);
                    anchors[anchor] = anchorRadial;
                    pixels.SetAnchors(anchors);
                    reference = NearestIndexToRadial(anchorRadial - FireflyUtils.M_PI * 2.0);
                    phase = Phase.REFERENCE_SELECTION;
                    break;
                case Phase.REFERENCE_SELECTION:
                    phase = Phase.MOVEMENT;
                    break;
                case Phase.MOVEMENT:
                    anchor++;
                    phase = Phase.ANCHOR_SELECTION;
                    break;
            }
        }

        public void Cancel() { }

        public void PrintCalibration()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<int, double> kv in anchors)
            {
                sb.AppendLine("anchors[" + kv.Key + "] = " + kv.Value + ";");
            }
            // One Debug.Log rather than one per line, so the table can be copied
            // out of the Console in a single block. See Port Notes.
            Debug.Log(sb.ToString());
        }

        private double RadialAtIndex(int index)
        {
            int prevAnchor = 0, nextAnchor = 1;
            double prevRadial = 0.0, nextRadial = 1.0;
            foreach (KeyValuePair<int, double> kv in anchors)
            {
                nextAnchor = kv.Key;
                nextRadial = kv.Value;
                if (index >= prevAnchor && index <= nextAnchor) break;
                prevAnchor = nextAnchor;
                prevRadial = nextRadial;
            }
            return prevRadial + (nextRadial - prevRadial) / (nextAnchor - prevAnchor) * (index - prevAnchor);
        }

        private int NearestIndexToRadial(double radial)
        {
            if (radial < 0.0) return 0;

            int prevAnchor = 0, nextAnchor = 1;
            double prevRadial = 0.0, nextRadial = 1.0;
            foreach (KeyValuePair<int, double> kv in anchors)
            {
                nextAnchor = kv.Key;
                nextRadial = kv.Value;
                if (radial >= prevRadial && radial <= nextRadial) break;
                prevAnchor = nextAnchor;
                prevRadial = nextRadial;
            }

            return (int)Math.Round((double)prevAnchor + (double)(nextAnchor - prevAnchor) / (nextRadial - prevRadial) * (radial - prevRadial));
        }

        public Pixel PixelInFocus()
        {
            return pixels.pixels[anchor];
        }

        public void LightPixels(double time)
        {
            for (int pi = 0; pi < pixels.pixelsLen; pi++)
            {
                pixels.pixels[pi].SetColor(CALIBRATION_DEFAULT_COLOR);
            }

            foreach (KeyValuePair<int, double> kv in anchors)
            {
                if (kv.Key < pixels.pixelsLen)
                    pixels.pixels[kv.Key].SetColor(CALIBRATION_ANCHOR_COLOR);
            }

            if (anchor >= pixels.pixelsLen) anchor = pixels.pixelsLen - 1;
            if (reference >= pixels.pixelsLen) reference = pixels.pixelsLen - 1;

            switch (phase)
            {
                case Phase.ANCHOR_SELECTION:
                    pixels.pixels[anchor].SetColor(CALIBRATION_SELECTED_COLOR);
                    break;
                case Phase.REFERENCE_SELECTION:
                    pixels.pixels[anchor].SetColor(new Vector3(1.0f, 1.0f, 1.0f));
                    pixels.pixels[reference].SetColor(CALIBRATION_SELECTED_COLOR);
                    break;
                case Phase.MOVEMENT:
                    pixels.pixels[anchor].SetColor(CALIBRATION_SELECTED_COLOR);
                    pixels.pixels[reference].SetColor(new Vector3(1.0f, 1.0f, 1.0f));
                    break;
            }
        }
    }
}
