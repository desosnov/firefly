using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firefly
{
    /// <summary>
    /// Builds the two meshes the C++ drew in immediate mode, at the tessellation
    /// the original specified. Unity's built-in primitives are fixed meshes with no
    /// slices/stacks parameters, so PIXEL_SLICES, PIXEL_STACKS and CYL_SLICES would
    /// otherwise have no effect. See Port Notes.
    /// </summary>
    public static class MeshBuilder
    {
        /// <summary>
        /// Equivalent of Pixel::drawSphere — a UV sphere of `slices` around and
        /// `stacks` from pole to pole. Unit radius; scaled per instance at draw time.
        /// Y-up to match the rest of the port.
        /// </summary>
        public static Mesh BuildSphere(int slices, int stacks)
        {
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> tris = new List<int>();

            double horizInterval = FireflyUtils.M_PI * 2.0 / slices;
            double vertInterval = FireflyUtils.M_PI / stacks;

            // (stacks + 1) rings from the bottom pole to the top pole. The seam
            // column is duplicated so the ring closes.
            for (int st = 0; st <= stacks; st++)
            {
                double vert = -FireflyUtils.M_PI / 2.0 + st * vertInterval;
                for (int sl = 0; sl <= slices; sl++)
                {
                    double horiz = sl * horizInterval;
                    Vector3 p = new Vector3(
                        (float)(Math.Cos(horiz) * Math.Cos(vert)),
                        (float)(Math.Sin(vert)),
                        (float)(Math.Sin(horiz) * Math.Cos(vert)));
                    verts.Add(p);
                    normals.Add(p.normalized);
                }
            }

            int stride = slices + 1;
            for (int st = 0; st < stacks; st++)
            {
                for (int sl = 0; sl < slices; sl++)
                {
                    int a = st * stride + sl;
                    int b = a + 1;
                    int c = a + stride;
                    int d = c + 1;

                    tris.Add(a); tris.Add(c); tris.Add(b);
                    tris.Add(b); tris.Add(c); tris.Add(d);
                }
            }

            Mesh mesh = new Mesh();
            mesh.name = "Firefly Pixel Sphere " + slices + "x" + stacks;
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Equivalent of PixelStage::drawDefaultCylinderWalls — an open tube of
        /// `slices` segments, no caps, running from y = 0 to y = height around
        /// (centerX, centerZ). Note the C++ declared CYL_STACKS but never used it:
        /// the wall is one triangle strip with no vertical subdivision.
        /// </summary>
        public static Mesh BuildCylinderWall(int slices, double radius, double height, double centerX, double centerZ)
        {
            List<Vector3> verts = new List<Vector3>();
            List<int> tris = new List<int>();

            double dir = 0.0;
            double interval = FireflyUtils.M_PI * 2.0 / slices;

            for (int s = 0; s <= slices; s++)
            {
                float x = (float)(centerX + Math.Cos(dir) * radius);
                float z = (float)(centerZ + Math.Sin(dir) * radius);
                verts.Add(new Vector3(x, 0.0f, z));
                verts.Add(new Vector3(x, (float)height, z));
                dir += interval;
            }

            for (int s = 0; s < slices; s++)
            {
                int a = s * 2;
                int b = a + 1;
                int c = a + 2;
                int d = a + 3;

                tris.Add(a); tris.Add(b); tris.Add(c);
                tris.Add(b); tris.Add(d); tris.Add(c);
            }

            Mesh mesh = new Mesh();
            mesh.name = "Firefly Cylinder Wall " + slices;
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
