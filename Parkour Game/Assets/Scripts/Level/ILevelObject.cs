using UnityEngine;

namespace Level
{
    public interface ILevelObject
    {
        public void Spawn();
        Vector2 Position { get; set; }
        float Height { get; set; }
    }
}