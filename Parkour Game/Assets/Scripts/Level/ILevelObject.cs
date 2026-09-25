using UnityEngine;

namespace Level
{
    public interface ILevelObject
    {
        bool IsGeometry { get; set; }
        Mesh Mesh { get; }
        Vector3 Scale { get; set; }
        Vector3 Position { get; set; }
        GameObject GameObject { get; }
        public void Spawn();
        void Regenerate();
    }
}