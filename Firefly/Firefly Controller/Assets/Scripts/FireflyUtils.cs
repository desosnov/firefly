using System;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// The OpenGL debug helpers (drawAxes, drawMovingPoint, drawSpinningTriangle) and
    /// the Windows to_string shims are dropped — see Port Notes.
    /// </summary>
    public static class FireflyUtils
    {
        public const double M_PI = 3.14159265358979323846;

        // The C++ used the global C rand()/srand(). One shared generator keeps the
        // same "one stream for the whole program" behaviour.
        //
        // Firefly.cpp never calls srand(), so C's rand() always started from the
        // default seed of 1 and main() simply advanced it a time-derived number of
        // steps. Seeding from 1 here preserves that: the same stream every launch,
        // offset by the advance in FireflyController.Start.
        private static System.Random rng = new System.Random(1);

        public static void Seed(int seed)
        {
            rng = new System.Random(seed);
        }

        /// <summary>Equivalent of C rand(): non-negative int up to RAND_MAX.</summary>
        public static int Rand()
        {
            return rng.Next();
        }

        /// <summary>Equivalent of rand1(): (double)rand()/(double)RAND_MAX, range [0,1].</summary>
        public static double Rand1()
        {
            return rng.NextDouble();
        }

        /// <summary>Equivalent of rand(min, max).</summary>
        public static double Rand(double min, double max)
        {
            return Rand1() * (max - min) + min;
        }

        public static void Log(string msg)
        {
            Debug.Log(msg);
        }

        public static string Vec3String(Vector3 v)
        {
            return v.x + " " + v.y + " " + v.z;
        }

        /// <summary>
        /// Always-positive modulo. C++ % yields a negative result for negative operands
        /// and the original indexed arrays with it; C# would throw. See Port Notes.
        /// </summary>
        public static int Mod(int a, int n)
        {
            int r = a % n;
            return r < 0 ? r + n : r;
        }
    }
}
