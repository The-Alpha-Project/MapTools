﻿// TheAlphaProject
// Discord: https://discord.gg/RzBMAKU
// Github:  https://github.com/The-Alpha-Project

using System;
using System.Collections.Generic;

namespace AlphaCoreExtractor.Generators.Mesh
{
    public class MeshClipper
    {
        private const float Epsilon = 1e-3f;
        private const float CellSize = 1e-3f;

        private Rect Bounds;
        private static readonly Dictionary<VertCoord, CVertex> CVertices = new Dictionary<VertCoord, CVertex>(50000);
        private readonly List<CFace> CFaces = new List<CFace>(150000);

        public MeshClipper(Rect bounds)
        {
            this.Bounds = bounds;
        }

        private void InitializeMeshInfo(IList<Vector3> vertices, IList<int> indices)
        {
            CVertices.Clear();
            CFaces.Clear();

            for (var i = 0; i < indices.Count; /**/)
            {
                var index0 = indices[i++];
                var index1 = indices[i++];
                var index2 = indices[i++];

                var vertIdx0 = AddOrGetVertex(vertices[index0]);
                var vertIdx1 = AddOrGetVertex(vertices[index1]);
                var vertIdx2 = AddOrGetVertex(vertices[index2]);

                CFaces.Add(new CFace(new[] { vertIdx0, vertIdx1, vertIdx2 }));
            }
        }

        public void ClipMesh(List<Vector3> vertices, List<int> indices, out List<Vector3> outVerts, out List<int> outIdxs)
        {
            outVerts = new List<Vector3>();
            outIdxs = new List<int>();

            InitializeMeshInfo(vertices, indices);
            switch (Clip())
            {
                case ClipperResponse.AllClipped:
                    return;
                case ClipperResponse.SomeClipped:
                    GenerateOutputMesh(outVerts, outIdxs);
                    return;
                case ClipperResponse.NoneClipped:
                    outVerts = vertices;
                    outIdxs = indices;
                    return;
            }
        }

        private ClipperResponse Clip()
        {
            // The Rect is in a screen coordinate system with (0,0) in the upper left and x-pos-right, y-pos-down
            // The left edge of the bounds (plane y = bounds.Left)
            // normal points into the bounds (negative y direction)
            var westPos = Bounds.Left;
            var westPlane = new Plane(0.0f, -1.0f, 0.0f, westPos);
            var response = Clip(westPlane);
            if (response == ClipperResponse.AllClipped) return response;

            var eastPos = Bounds.Right;
            var eastPlane = new Plane(0.0f, 1.0f, 0.0f, -eastPos);
            var newResponse = Clip(eastPlane);
            if (newResponse == ClipperResponse.AllClipped) return newResponse;
            if (response == ClipperResponse.NoneClipped) response = newResponse;

            var northPos = Bounds.Top;
            var northPlane = new Plane(-1.0f, 0.0f, 0.0f, northPos);
            newResponse = Clip(northPlane);
            if (newResponse == ClipperResponse.AllClipped) return newResponse;
            if (response == ClipperResponse.NoneClipped) response = newResponse;

            var southPos = Bounds.Bottom;
            var southPlane = new Plane(1.0f, 0.0f, 0.0f, -southPos);
            newResponse = Clip(southPlane);
            if (newResponse == ClipperResponse.AllClipped) return newResponse;
            if (response == ClipperResponse.NoneClipped) response = newResponse;

            return response;
        }

        private ClipperResponse Clip(Plane plane)
        {
            var response = CalcVertexDistances(plane);
            switch (response)
            {
                case (ClipperResponse.NoneClipped):
                    return response;
                case (ClipperResponse.AllClipped):
                    return response;
                case (ClipperResponse.SomeClipped):
                    ClipFaces();
                    return response;
                default:
                    throw new InvalidOperationException();
            }
        }

        private ClipperResponse CalcVertexDistances(Plane plane)
        {
            var numInside = 0;
            var numOutside = 0;
            foreach (var vertex in CVertices.Values)
            {
                if (!vertex.IsVisible) continue;

                var planeVertNml = plane.DotNormal(vertex.Point);
                vertex.Distance = (planeVertNml + plane.D);
                if (vertex.Distance >= Epsilon)
                    numInside++;
                else if (vertex.Distance <= -Epsilon)
                {
                    numOutside++;
                    vertex.IsVisible = false;
                }
                else
                    vertex.Distance = 0.0f;
            }

            if (numOutside == 0)
                return ClipperResponse.NoneClipped;

            return numInside == 0 ? ClipperResponse.AllClipped : ClipperResponse.SomeClipped;
        }

        private void ClipFaces()
        {
            for (var faceId = 0; faceId < CFaces.Count; faceId++)
            {
                var face = CFaces[faceId];
                if (!face.IsVisible) continue;

                CFace newFace;
                switch (ClipFace(face, out newFace))
                {
                    case (ClipperResponse.SomeClipped):
                        face.IsVisible = true;
                        if (newFace != null)
                        {
                            newFace.IsVisible = true;
                            CFaces.Add(newFace);
                        }
                        break;
                    case (ClipperResponse.AllClipped):
                        face.IsVisible = false;
                        break;
                    case (ClipperResponse.NoneClipped):
                        face.IsVisible = true;
                        break;
                    default:
                        break;
                }
            }
        }

        private ClipperResponse ClipFace(CFace face, out CFace newFace)
        {
            newFace = null;

            var numInside = 0;
            var numOutside = 0;
            foreach (var vertId in face.VertIdxs)
            {
                if (CVertices[vertId].Distance > 0.0f)
                    numInside++;
                else if (CVertices[vertId].Distance < 0.0f)
                    numOutside++;
            }

            if (numOutside == 0)
                return ClipperResponse.NoneClipped;

            if (numInside == 0)
                return ClipperResponse.AllClipped;

            var count = 0;
            var newPoly = new VertCoord[4];
            var prevIdx = face.VertIdxs.Length - 1;
            for (var idx = 0; idx < face.VertIdxs.Length; idx++)
            {
                var vertIdx0 = face.VertIdxs[prevIdx];
                var vertIdx1 = face.VertIdxs[idx];

                var vert0 = CVertices[vertIdx0];
                var vert1 = CVertices[vertIdx1];

                // vert1 outside
                if (vert1.Distance < 0.0f)
                {
                    // vert0 inside
                    if (vert0.Distance > 0.0f)
                    {
                        var time = vert0.Distance / (vert0.Distance - vert1.Distance);
                        var newPoint = (1.0f - time) * vert0.Point + time * vert1.Point;

                        var newIdx = AddOrGetVertex(newPoint);
                        CVertices[newIdx].IsVisible = true;
                        newPoly[count++] = newIdx;
                    }
                }
                else
                {
                    // vert0 outside, vert1 inside
                    if (vert1.Distance > 0.0f && vert0.Distance < 0.0f)
                    {
                        var time = vert1.Distance / (vert1.Distance - vert0.Distance);
                        var newPoint = (1.0f - time) * vert1.Point + time * vert0.Point;
                        var newIdx = AddOrGetVertex(newPoint);
                        CVertices[newIdx].IsVisible = true;
                        newPoly[count++] = newIdx;
                    }

                    newPoly[count++] = vertIdx1;
                }

                prevIdx = idx;
            }

            if (count == 3)
            {
                for (var i = 0; i < 3; i++)
                    face.VertIdxs[i] = newPoly[i];

                return ClipperResponse.SomeClipped;
            }

            // Re-triangulate the quadrangle
            var newFaces = ReTriangulate(newPoly);
            face.VertIdxs = new[] { newFaces[0], newFaces[1], newFaces[2] };
            newFace = new CFace(new[] { newFaces[3], newFaces[4], newFaces[5] });
            return ClipperResponse.SomeClipped;
        }

        private void GenerateOutputMesh(ICollection<Vector3> vertices, ICollection<int> indices)
        {
            var indexes = new Dictionary<VertCoord, int>(CVertices.Count);
            vertices.Clear();
            indices.Clear();

            foreach (var face in CFaces)
            {
                if (!face.IsVisible)
                    continue;

                foreach (var vertIdx in face.VertIdxs)
                {
                    int newIdx;
                    if (!indexes.TryGetValue(vertIdx, out newIdx))
                    {
                        newIdx = vertices.Count;
                        indexes.Add(vertIdx, newIdx);

                        var vertex = CVertices[vertIdx];
                        vertices.Add(vertex.Point);
                    }

                    indices.Add(newIdx);
                }
            }
        }

        private VertCoord[] ReTriangulate(VertCoord[] poly)
        {
            if (poly == null)
                return null;

            if (poly.Length < 4)
                return poly;

            // Calculate the angle between edge03 and edge01
            // a dot b = |a||b|cosA => cosA = (a dot b)/(|a||b|)
            var edge01 = CVertices[poly[1]].Point - CVertices[poly[0]].Point;
            var edge03 = CVertices[poly[3]].Point - CVertices[poly[0]].Point;
            var dot = Vector3.Dot(edge01, edge03);
            var len01 = edge01.Length();
            var len03 = edge03.Length();
            var cosA = dot / (len01 * len03);
            var alpha = Math.Acos(cosA);

            // Calculate the angle between edge21 and edge23
            var edge21 = CVertices[poly[1]].Point - CVertices[poly[2]].Point;
            var edge23 = CVertices[poly[3]].Point - CVertices[poly[2]].Point;
            dot = Vector3.Dot(edge21, edge23);
            var len21 = edge01.Length();
            var len23 = edge03.Length();
            cosA = dot / (len21 * len23);
            var beta = Math.Acos(cosA);

            return ((alpha + beta) >= Math.PI)
                       ? new[] { poly[0], poly[1], poly[3], poly[1], poly[2], poly[3] }
                       : new[] { poly[0], poly[2], poly[3], poly[0], poly[1], poly[2] };
        }

        private VertCoord AddOrGetVertex(Vector3 point)
        {
            var hash = CalcSpatialHash(point);

            if (!CVertices.ContainsKey(hash))
                CVertices.Add(hash, new CVertex(point));

            return hash;
        }

        private static VertCoord CalcSpatialHash(Vector3 point)
        {
            var i = (int)(point.X / CellSize);
            var j = (int)(point.Y / CellSize);
            var k = (int)(point.Z / CellSize);

            return new VertCoord(i, j, k);
        }
    }
}
