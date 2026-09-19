using UnityEngine;

namespace Level
{
    public class StaticGeometry : MonoBehaviour, ILevelObject
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _scale;

        public Mesh Mesh => _filter.sharedMesh;
        public Vector3 Scale => transform.localScale;
        public GameObject GameObject => gameObject;

        public void Spawn()
        {
            //Play some sort of animation here
        }

        private void Awake()
        {
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
    }
}