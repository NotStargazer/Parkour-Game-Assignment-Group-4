using System;
using UnityEngine;

namespace Level
{
    [RequireComponent(typeof(BoxCollider))]
    public class LevelGate : MonoBehaviour
    {
        [SerializeField] private Collider _collider;

        private Action _enterGate;
        private bool _enabled;

        public void ResetGate(Vector3 position, Action onEnterGate)
        {
            _enterGate = onEnterGate;
            transform.position = position;
            _enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_enabled)
            {
                _enabled = false;
                _enterGate?.Invoke();
            }
        }

        private void OnValidate()
        {
            if (TryGetComponent<Collider>(out var col))
            {
                _collider = col;
                _collider.isTrigger = true;
            }
        }

        private void OnDrawGizmos()
        {
            if (_collider.enabled)
            {
                Gizmos.color = Color.lawnGreen;
                Gizmos.DrawWireCube(transform.position, transform.localScale);
            }
        }
    }
}