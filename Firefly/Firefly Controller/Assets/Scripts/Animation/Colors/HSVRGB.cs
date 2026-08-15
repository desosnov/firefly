using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// BSD 3-clause licence (Jan Winkler) — see the original header for the full
    /// licence text, retained in Code/Firefly Original.
    /// </summary>
    public static class HSVRGB
    {
        /// <summary>h in [0,360], s and v in [0,1]; r, g, b out in [0,1].</summary>
        public static void HSVtoRGB(out float fR, out float fG, out float fB, float fH, float fS, float fV)
        {
            float fC = fV * fS;
            float fHPrime = (float)((fH / 60.0) % 6.0);
            float fX = fC * (1 - Mathf.Abs(fHPrime % 2 - 1));
            float fM = fV - fC;

            if (0 <= fHPrime && fHPrime < 1) { fR = fC; fG = fX; fB = 0; }
            else if (1 <= fHPrime && fHPrime < 2) { fR = fX; fG = fC; fB = 0; }
            else if (2 <= fHPrime && fHPrime < 3) { fR = 0; fG = fC; fB = fX; }
            else if (3 <= fHPrime && fHPrime < 4) { fR = 0; fG = fX; fB = fC; }
            else if (4 <= fHPrime && fHPrime < 5) { fR = fX; fG = 0; fB = fC; }
            else if (5 <= fHPrime && fHPrime < 6) { fR = fC; fG = 0; fB = fX; }
            else { fR = 0; fG = 0; fB = 0; }

            fR += fM;
            fG += fM;
            fB += fM;
        }

        /// <summary>r, g, b in [0,1]; h out in [0,360], s and v out in [0,1].</summary>
        public static void RGBtoHSV(float fR, float fG, float fB, out float fH, out float fS, out float fV)
        {
            float fCMax = Mathf.Max(Mathf.Max(fR, fG), fB);
            float fCMin = Mathf.Min(Mathf.Min(fR, fG), fB);
            float fDelta = fCMax - fCMin;

            fH = 0; fS = 0; fV = fCMax;

            if (fDelta > 0)
            {
                if (fCMax == fR) fH = 60 * (((fG - fB) / fDelta) % 6);
                else if (fCMax == fG) fH = 60 * (((fB - fR) / fDelta) + 2);
                else if (fCMax == fB) fH = 60 * (((fR - fG) / fDelta) + 4);

                fS = fCMax > 0 ? fDelta / fCMax : 0;
            }

            if (fH < 0) fH = 360 + fH;
        }
    }
}
