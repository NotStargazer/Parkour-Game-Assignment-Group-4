using UnityEngine;
using Utility;

namespace Level
{
    public class MovingPlatform : MonoBehaviour, ILevelObject
    {
        [SerializeField] private bool _isGeometry;
        [SerializeField] private MeshRenderer _renderer;
        [SerializeField] private MeshFilter _filter;
        [ShowInLevelEditor][SerializeField] private Vector3 _endOffset;
        [ShowInLevelEditor][SerializeField] private Material _material;
        [ShowInLevelEditor][SerializeField] private AnimationCurve _pathAnimation;
        [ShowInLevelEditor][SerializeField] private float _pathTime;

        private float _startTime;
        private Vector3 _start;
        private Vector3 _end;
        private Vector3? _moveDifference;
        public Vector3 MoveDifference
        {
            get
            {
                if (_moveDifference.HasValue)
                {
                    var val = _moveDifference.Value;
                    _moveDifference = null;
                    return val;
                }

                return Vector3.zero;
            }
        }

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
        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }
        public GameObject GameObject => gameObject;

        public void Spawn()
        {
            _startTime = Time.deltaTime;
        }

        public void Regenerate()
        {
            CreateMesh();
        }

        private void Awake()
        {
            _start = transform.localPosition;
            _end = transform.localPosition + _endOffset;
            CreateMesh();
        }

        private void Update()
        {
            var oldPosition = transform.localPosition;
            var pathPos = Mathf.PingPong(Time.time - _startTime, _pathTime) / _pathTime;
            var position = Vector3.Lerp(_start, _end, _pathAnimation.Evaluate(pathPos));
            transform.localPosition = position;
            _moveDifference = position - oldPosition;
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
            _filter.sharedMesh = MeshBuilder.CreateUVScaledCubeMesh(transform.localScale, "MovingPlatform");
            if (_material)
            {
                _renderer.material = _material;
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