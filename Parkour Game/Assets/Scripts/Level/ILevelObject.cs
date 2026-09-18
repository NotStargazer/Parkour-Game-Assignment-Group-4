using UnityEngine;

namespace Level
{
    public interface ILevelObject
    {
        Mesh Mesh { get; }
        GameObject GameObject { get; }
        public void Spawn();
    }
}