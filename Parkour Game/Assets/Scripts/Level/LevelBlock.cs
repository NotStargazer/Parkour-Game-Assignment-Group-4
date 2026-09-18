using Level;
using UnityEngine;

public class LevelBlock : MonoBehaviour
{
    [SerializeReference] private ILevelObject[] _levelObjects;
    [SerializeField] private float _height;
    [SerializeField] private Vector4 _expanse;
    [SerializeField] private Vector2 _entrance;
    [SerializeField] private Vector2 _exit;
}
