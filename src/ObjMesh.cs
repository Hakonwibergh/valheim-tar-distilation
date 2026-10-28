using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace TarDistillation
{
    /// <summary>
    ///     Builds a Unity mesh from a Wavefront OBJ file at runtime.
    /// </summary>
    /// <remarks>
    ///     The usual way to ship a model with a Valheim mod is a Unity asset bundle, which
    ///     needs the Unity editor to produce. An OBJ is plain text and a mesh is plain data,
    ///     so the model can be read straight off disk instead - the one thing that genuinely
    ///     cannot be built at runtime is a shader, and we borrow a vanilla one for that
    ///     (see <see cref="ItemVisuals"/>).
    ///
    ///     Only what these models use is supported: v, vt, vn and triangular or convex
    ///     polygon f lines. Materials are ignored; the caller assigns one.
    /// </remarks>
    internal static class ObjMesh
    {
        /// <summary>
        ///     Faces meeting at a sharper angle than this keep a hard edge. Round the sides
        ///     of a split stick off, leave the sawn ends crisp.
        /// </summary>
        private const float SmoothingAngle = 60f;

        private static readonly char[] Whitespace = { ' ', '\t' };

        /// <summary>
        ///     Reads <paramref name="path"/> into a mesh at the size the file states, centred
        ///     on its own bounding box so it hangs on the item's pivot rather than beside it.
        ///     Throws if the file is missing or holds no geometry; callers treat that as
        ///     "fall back to the vanilla look".
        /// </summary>
        public static Mesh Load(string path, string name)
        {
            var positions = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            // One entry per triangle corner. Vertices are not shared between faces here;
            // the smoothing pass below decides which corners end up with the same normal.
            var corners = new List<Corner>();

            foreach (var raw in File.ReadLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                var fields = line.Split(Whitespace, StringSplitOptions.RemoveEmptyEntries);
                switch (fields[0])
                {
                    case "v":
                        // OBJ is right-handed, Unity is left-handed, so z flips. Mirroring
                        // also reverses which side of a face counts as the front, and the
                        // shipped models happen to be wound the way that needs: all 900
                        // faces come out pointing away from the wood they sit on. If a model
                        // added later turns up inside out, it is wound the other way and
                        // wants its corners swapped in AddFace, not a different flip here.
                        positions.Add(new Vector3(Num(fields, 1), Num(fields, 2), -Num(fields, 3)));
                        break;

                    case "vt":
                        uvs.Add(new Vector2(Num(fields, 1), Num(fields, 2)));
                        break;

                    case "vn":
                        normals.Add(new Vector3(Num(fields, 1), Num(fields, 2), -Num(fields, 3)));
                        break;

                    case "f":
                        AddFace(fields, positions.Count, uvs.Count, normals.Count, corners);
                        break;
                }
            }

            if (corners.Count == 0)
            {
                throw new InvalidDataException($"{Path.GetFileName(path)} holds no faces");
            }

            return Build(name, corners, positions, uvs, normals);
        }

        /// <summary>Fans a convex face out into triangles. These models are already triangles.</summary>
        private static void AddFace(string[] fields, int vCount, int vtCount, int vnCount, List<Corner> corners)
        {
            for (var i = 2; i < fields.Length - 1; i++)
            {
                corners.Add(Corner.Parse(fields[1], vCount, vtCount, vnCount));
                corners.Add(Corner.Parse(fields[i], vCount, vtCount, vnCount));
                corners.Add(Corner.Parse(fields[i + 1], vCount, vtCount, vnCount));
            }
        }

        private static Mesh Build(string name, List<Corner> corners, List<Vector3> positions,
            List<Vector2> uvs, List<Vector3> normals)
        {
            var bounds = new Bounds(positions[corners[0].Position], Vector3.zero);
            foreach (var corner in corners)
            {
                bounds.Encapsulate(positions[corner.Position]);
            }

            var offset = bounds.center;

            var count = corners.Count - corners.Count % 3;
            var vertices = new Vector3[count];
            var vertexUvs = new Vector2[count];
            var triangles = new int[count];

            for (var i = 0; i < count; i++)
            {
                var corner = corners[i];
                vertices[i] = positions[corner.Position] - offset;
                vertexUvs[i] = corner.Uv >= 0 && corner.Uv < uvs.Count ? uvs[corner.Uv] : Vector2.zero;
                triangles[i] = i;
            }

            var mesh = new Mesh { name = name };
            mesh.vertices = vertices;
            mesh.uv = vertexUvs;
            mesh.triangles = triangles;
            mesh.normals = Normals(corners, vertices, normals);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>
        ///     Normals, either as the file gave them or worked out from the faces. These models
        ///     carry no vn lines, so the second path is the one that runs: every corner starts
        ///     with the flat normal of its own triangle, then corners sitting on the same OBJ
        ///     vertex average in the faces that lie within <see cref="SmoothingAngle"/> of it.
        ///     That rounds a stick off along its length without smearing the ends into it.
        /// </summary>
        private static Vector3[] Normals(List<Corner> corners, Vector3[] vertices, List<Vector3> fileNormals)
        {
            var count = vertices.Length;
            var result = new Vector3[count];

            if (fileNormals.Count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var index = corners[i].Normal;
                    result[i] = index >= 0 && index < fileNormals.Count
                        ? fileNormals[index].normalized
                        : Vector3.up;
                }

                return result;
            }

            var faceNormals = new Vector3[count / 3];
            for (var face = 0; face < faceNormals.Length; face++)
            {
                var a = vertices[face * 3];
                var b = vertices[face * 3 + 1];
                var c = vertices[face * 3 + 2];
                faceNormals[face] = Vector3.Cross(b - a, c - a).normalized;
            }

            // Which faces touch each original OBJ vertex.
            var shared = new Dictionary<int, List<int>>();
            for (var i = 0; i < count; i++)
            {
                if (!shared.TryGetValue(corners[i].Position, out var faces))
                {
                    faces = new List<int>();
                    shared[corners[i].Position] = faces;
                }

                faces.Add(i / 3);
            }

            var threshold = Mathf.Cos(SmoothingAngle * Mathf.Deg2Rad);
            for (var i = 0; i < count; i++)
            {
                var own = faceNormals[i / 3];
                var sum = Vector3.zero;

                foreach (var face in shared[corners[i].Position])
                {
                    if (Vector3.Dot(own, faceNormals[face]) >= threshold)
                    {
                        sum += faceNormals[face];
                    }
                }

                result[i] = sum == Vector3.zero ? own : sum.normalized;
            }

            return result;
        }

        private static float Num(string[] fields, int index)
        {
            // Invariant on purpose: an OBJ always writes 0.5, never 0,5, whatever the
            // machine's locale would have it do.
            return index < fields.Length
                ? float.Parse(fields[index], NumberStyles.Float, CultureInfo.InvariantCulture)
                : 0f;
        }

        /// <summary>One corner of a triangle: the v/vt/vn triple from an f line, zero-based.</summary>
        private struct Corner
        {
            public int Position;
            public int Uv;
            public int Normal;

            /// <summary>
            ///     Reads "v", "v/vt", "v//vn" or "v/vt/vn". OBJ indices count from 1, and a
            ///     negative one counts back from the end of what has been read so far.
            /// </summary>
            public static Corner Parse(string token, int vCount, int vtCount, int vnCount)
            {
                var parts = token.Split('/');
                return new Corner
                {
                    Position = Index(parts, 0, vCount),
                    Uv = Index(parts, 1, vtCount),
                    Normal = Index(parts, 2, vnCount),
                };
            }

            private static int Index(string[] parts, int slot, int count)
            {
                if (slot >= parts.Length || parts[slot].Length == 0)
                {
                    return -1;
                }

                var value = int.Parse(parts[slot], CultureInfo.InvariantCulture);
                return value < 0 ? count + value : value - 1;
            }
        }
    }
}
