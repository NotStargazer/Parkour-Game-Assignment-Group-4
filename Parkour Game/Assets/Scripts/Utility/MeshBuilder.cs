using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public ref struct MeshBuilder
    {
        private List<Vector3> _verts;
        private List<Vector3> _normals;
        private List<Vector2> _uvs;
        private List<int> _triangles;
            
        private void AddCubePlane(Vector3 normal, Vector2 uvScale, params Vector3[] verts)
        {
            foreach (var vert in verts)
            {
                _verts.Add(vert);
                _normals.Add(normal);
            }
            
            _uvs.Add(Vector2.zero);
            _uvs.Add(new Vector2(0, uvScale.y));
            _uvs.Add(new Vector2(uvScale.x, uvScale.y));
            _uvs.Add(new Vector2(uvScale.x, 0));

            var lastTri = _triangles.Count > 0 ? _triangles[^1] + 1 : 0;
            _triangles.Add(lastTri);
            _triangles.Add(lastTri + 1);
            _triangles.Add(lastTri + 2);
            _triangles.Add(lastTri);
            _triangles.Add(lastTri + 2);
            _triangles.Add(lastTri + 3);
        }

        public static Mesh CreateUVScaledCubeMesh(Vector3 scale, string name)
        {
            var half = Vector3.one * 0.5f;
            
            //Lower/Upper (l/u) Back/Forward (b/f) Left/Right (l/r) lbr - ufr
            var lbl = new Vector3(-half.x, -half.y, -half.z);
            var lbr = new Vector3( half.x, -half.y, -half.z);
            var lfl = new Vector3(-half.x, -half.y,  half.z);
            var lfr = new Vector3( half.x, -half.y,  half.z);
            var ubl = new Vector3(-half.x,  half.y, -half.z);
            var ubr = new Vector3( half.x,  half.y, -half.z);
            var ufl = new Vector3(-half.x,  half.y,  half.z);
            var ufr = new Vector3( half.x,  half.y,  half.z);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            var meshBuilder = new MeshBuilder
            {
                _verts = verts,
                _normals = normals,
                _uvs = uvs,
                _triangles = triangles,
            };
            
            meshBuilder.AddCubePlane(Vector3.back,    new Vector2(scale.x, scale.y), lbl, ubl, ubr, lbr);
            meshBuilder.AddCubePlane(Vector3.forward, new Vector2(scale.x, scale.y), lfr, ufr, ufl, lfl);
            meshBuilder.AddCubePlane(Vector3.left,    new Vector2(scale.z, scale.y), lfl, ufl, ubl, lbl);
            meshBuilder.AddCubePlane(Vector3.right,   new Vector2(scale.z, scale.y), lbr, ubr, ufr, lfr);
            meshBuilder.AddCubePlane(Vector3.down,    new Vector2(scale.x, scale.z), lfl, lbl, lbr, lfr);
            meshBuilder.AddCubePlane(Vector3.up,      new Vector2(scale.x, scale.z), ubl, ufl, ufr, ubr);

            var generatedMesh = new Mesh
            {
                name = name,
                vertices = meshBuilder._verts.ToArray(),
                normals = meshBuilder._normals.ToArray(),
                uv = meshBuilder._uvs.ToArray(),
                triangles = meshBuilder._triangles.ToArray()
            };
            
            generatedMesh.RecalculateBounds();
            generatedMesh.RecalculateTangents();

            return generatedMesh;
        }
    }
}