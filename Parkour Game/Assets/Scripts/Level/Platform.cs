using UnityEngine;

namespace Level
{
    public class Platform : MonoBehaviour, ILevelObject
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private Vector3 _position;
        [SerializeField] private Vector3 _scale;

        public Mesh Mesh => _filter.mesh;
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