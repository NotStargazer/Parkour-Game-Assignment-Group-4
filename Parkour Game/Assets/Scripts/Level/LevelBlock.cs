using System.Collections;
using Level;
using UnityEngine;

public class LevelBlock : MonoBehaviour
{
    [SerializeField] private GameObject[] _levelObjects;
    [SerializeField] private float _height;
    [SerializeField] private Vector4 _expanse;
    [SerializeField] private Vector2 _entrance;
    [SerializeField] private Vector2 _exit;

    private ILevelObject[] _levelObjectInterfaces;
    public Vector3 EndGatePosition => 
        transform.position + new Vector3(_exit.x, _exit.y + transform.localScale.y * 0.5f, _expanse.w);
    public int BlockIndex { get; set; }

    private void Awake()
    {
        _levelObjectInterfaces = new ILevelObject[_levelObjects.Length];
        for (var i = 0; i < _levelObjects.Length; i++)
        {
            _levelObjectInterfaces[i] = _levelObjects[i].GetComponent<ILevelObject>();
        }
    }

    public void Place(Vector3 previousEndGate)
    {
        gameObject.SetActive(true);
        var placePosition = 
            -new Vector3(_entrance.x, _entrance.y + transform.localScale.y * 0.5f, -_expanse.z) + previousEndGate;
        StartCoroutine(SpawnRoutine());
        transform.position = placePosition;
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var levelObject in _levelObjectInterfaces)
        {
            levelObject.Spawn();
            yield return new WaitForSeconds(0.01f);
        }
    }
}
