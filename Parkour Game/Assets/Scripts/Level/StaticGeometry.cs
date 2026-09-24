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
            _filter.sharedMesh = MeshBuilder.CreateUVScaledCubeMesh(transform.localScale, "StaticGeometry");
            if (_material)
            {
                _renderer.material = _material;
            }
        }
    }
}