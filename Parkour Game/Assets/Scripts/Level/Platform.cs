using UnityEngine;
using Utility;

namespace Level
{
    public class Platform : MonoBehaviour, ILevelObject
    {
        [SerializeField] private bool _isGeometry;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [ShowInLevelEditor] [SerializeField] private Vector3 _endOffset;

        private Vector3 _start;
        private Vector3 _end;
        
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
            _start = transform.position;
            _end = transform.position + _endOffset;
        }

        private void OnValidate()
        {
            if (!TryGetComponent(out _filter))
            {
                _filter = gameObject.AddComponent<MeshFilter>();
            }
            if (!TryGetComponent(out _renderer))
            {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _endOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawWireMesh(Mesh, 0, transform.position + _endOffset, transform.rotation, transform.localScale);
        }
    }
}