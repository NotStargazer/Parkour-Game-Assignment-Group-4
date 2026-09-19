using UnityEngine;

namespace Level
{
    public interface ILevelObject
    {
        Mesh Mesh { get; }
        Vector3 Scale { get; }
        GameObject GameObject { get; }
        public void Spawn();
    }
}