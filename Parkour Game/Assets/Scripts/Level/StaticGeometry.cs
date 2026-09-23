using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Level
{
    public class StaticGeometry : MonoBehaviour, ILevelObject
    {
        [SerializeField] private bool _isGeometry;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [ShowInLevelEditor] [SerializeField] private Material _material;

        public bool IsGeometry
        {
            get => _isGeometry;
            set => _isGeometry = value;
        }
        public Mesh Mesh => _filter.sharedMesh;
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }
        public Vector3 Scale
        {
            get => transform.localScale;
            set => transform.localScale = value;
        }
        public GameObject GameObject => gameObject;
        
        public void Spawn()
        {
            //Play some sort of animation here
        }

        private void Awake()
        {
            CreateMesh();
        }

        private void OnValidate()
        {
            if (!TryGetComponent(out _filter))
            {
                _filter = gameObject.AddComponent<MeshFilter>();
            }
            else
            {
                CreateMesh();
            }
            if (!TryGetComponent(out _renderer))
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        private void CreateMesh()
        {
            var scale = transform.localScale;
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
                Verts = verts,
                Normals = normals,
                UVs = uvs,
                Triangles = triangles,
            };
            
            meshBuilder.AddCubePlane(Vector3.back,    new Vector2(scale.x, scale.y), lbl, ubl, ubr, lbr);
            meshBuilder.AddCubePlane(Vector3.forward, new Vector2(scale.x, scale.y), lfr, ufr, ufl, lfl);
            meshBuilder.AddCubePlane(Vector3.left,    new Vector2(scale.z, scale.y), lfl, ufl, ubl, lbl);
            meshBuilder.AddCubePlane(Vector3.right,   new Vector2(scale.z, scale.y), lbr, ubr, ufr, lfr);
            meshBuilder.AddCubePlane(Vector3.down,    new Vector2(scale.x, scale.z), lfl, lbl, lbr, lfr);
            meshBuilder.AddCubePlane(Vector3.up,      new Vector2(scale.x, scale.z), ubl, ufl, ufr, ubr);

            var generatedMesh = new Mesh
            {
                name = "StaticGeoemtry",
                vertices = meshBuilder.Verts.ToArray(),
                normals = meshBuilder.Normals.ToArray(),
                uv = meshBuilder.UVs.ToArray(),
                triangles = meshBuilder.Triangles.ToArray()
            };
            
            generatedMesh.RecalculateBounds();
            generatedMesh.RecalculateTangents();
            _filter.sharedMesh = generatedMesh;
            if (_material)
            {
                _renderer.material = _material;
            }
        }
    }
}