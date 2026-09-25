using System;
using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(SphereCollider))]
    public class Token : MonoBehaviour, ILevelObject
    {
        [SerializeField] private bool _isGeometry;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private Transform _tokenTransform;
        [SerializeField] private float _rotationsPerSecond;
        [SerializeField] private int _score;
        
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
            get => _renderer.transform.localScale;
            set => _renderer.transform.localScale = value;
        }
        public Quaternion Rotation
        {
            get => _renderer.transform.rotation;
            set => _renderer.transform.rotation = value;
        }
        public GameObject GameObject => gameObject;
        public void Spawn()
        {
            gameObject.SetActive(true);
        }
        
        public void Regenerate() { }
        
        private void Update()
        {
            _tokenTransform.Rotate(Vector3.up, _rotationsPerSecond * 360 * Time.deltaTime);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent<CharacterController>(out _))
            {
                GameManager.Instance.AddScore(_score);
                gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            if (TryGetComponent(out SphereCollider sphereCollider))
            {
                sphereCollider.isTrigger = true;
            }
        }
    }
}