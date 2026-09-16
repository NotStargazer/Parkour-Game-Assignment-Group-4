using UnityEngine;

namespace Level
{
    public class Platform : MonoBehaviour, ILevelObject
    {
        [SerializeField] private Vector2 _position;
        [SerializeField] private Vector3 _scale;

        public void Spawn()
        {
            //Play some sort of animation here
        }

        public Vector2 Position
        {
            get => _position;
            set => _position = value; 
        }

        public float Height
        {
            get => _scale.y;
            set => _scale.y = value;
        }

        private void Awake()
        {
            
        }
    }
}