using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public ref struct MeshBuilder
    {
        public List<Vector3> Verts;
        public List<Vector3> Normals;
        public List<Vector2> UVs;
        public List<int> Triangles;
            
        public void AddCubePlane(Vector3 normal, Vector2 uvScale, params Vector3[] verts)
        {
            foreach (var vert in verts)
            {
                Verts.Add(vert);
                Normals.Add(normal);
            }
            
            UVs.Add(Vector2.zero);
            UVs.Add(new Vector2(0, uvScale.y));
            UVs.Add(new Vector2(uvScale.x, uvScale.y));
            UVs.Add(new Vector2(uvScale.x, 0));

            var lastTri = Triangles.Count > 0 ? Triangles[^1] + 1 : 0;
            Triangles.Add(lastTri);
            Triangles.Add(lastTri + 1);
            Triangles.Add(lastTri + 2);
            Triangles.Add(lastTri);
            Triangles.Add(lastTri + 2);
            Triangles.Add(lastTri + 3);
        }
    }
}