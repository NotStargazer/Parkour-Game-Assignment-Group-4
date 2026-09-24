using UnityEngine;

namespace Level
{
    public class Token : MonoBehaviour, ILevelObject
    {
        [SerializeField] private bool _isGeometry;
        [SerializeField] private SphereCollider _sphereCollider;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [SerializeField] private Transform _tokenTransform;
        [SerializeField] private float _rotationsPerSecond;
        [SerializeField] private float _score;
        
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
            gameObject.SetActive(true);
        }
        
        private void Update()
        {
            _tokenTransform.Rotate(Vector3.up, _rotationsPerSecond * Time.deltaTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out CharacterController controller))
            {
                //GameManager.Instance.CollectToken(_score);
                gameObject.SetActive(false);
            }
        }

        private void OnValidate()
        {
            if (!TryGetComponent(out _sphereCollider))
            {
                _sphereCollider = gameObject.AddComponent<SphereCollider>();
            }
        }
    }
}