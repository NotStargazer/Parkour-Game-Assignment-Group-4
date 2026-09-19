using Level;
using UnityEngine;

public class LevelBlock : MonoBehaviour
{
    [SerializeField] private GameObject[] _levelObjects;
    [SerializeField] private float _height;
    [SerializeField] private Vector4 _expanse;
    [SerializeField] private Vector2 _entrance;
    [SerializeField] private Vector2 _exit;
}
