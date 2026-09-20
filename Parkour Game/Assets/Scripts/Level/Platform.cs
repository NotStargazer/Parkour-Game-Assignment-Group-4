using UnityEngine;
using Utility;

namespace Level
{
    public class Platform : MonoBehaviour, ILevelObject
    {
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [ShowInLevelEditor] [SerializeField] private Vector3 _startPos;
        [ShowInLevelEditor] [SerializeField] private Vector3 _endPos;

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