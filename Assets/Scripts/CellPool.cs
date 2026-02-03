using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Pool;

public class CellPool : MonoBehaviour
{
    public static CellPool _instance;       // Singleton
    public GameObject cell;                 // Hold reference to a cell game object prefab

    public ObjectPool<GameObject> pool;    // Object pool of game object cell

    private void Awake()
    {
        _instance = this;
        
        // Performs collection check, default capacity = 35, max size = 50
        pool = new ObjectPool<GameObject>(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, true, 35, 50);
    }

    // Used to create a new instance when the pool is empty, instantiates and returns a new game object
    private GameObject createFunc()
    {
        var obj = Instantiate(cell, transform);
        return obj;
    }

    // Called when the instance is taken from the pool, activate the instance
    private void actionOnGet(GameObject _obj)
    {
        _obj.SetActive(true);
    }

    // Called when the instance is returned to the pool, disactivate the instance
    private void actionOnRelease(GameObject _obj)
    {
        _obj.SetActive(false);
    }

    // Called when the element could not be returned to the pool due to the pool reaching the max size, destroys the element
    private void actionOnDestroy(GameObject _obj)
    {
        Destroy(_obj);
    }

    // Getting an object from the pool, setting its position, initializing its properties
    public GameObject Spawn(Vector2 _pos, notes _noteType, int _noteLevel, instruments _instru, int _trackIndex, bool _isSM)
    {
        GameObject tempCell = pool.Get();
        tempCell.transform.position = _pos;
        tempCell.GetComponent<Cell>().InitializeCell(_noteType, _noteLevel, _instru, _trackIndex, _isSM);

        tempCell.transform.SetParent(transform);
        return tempCell;
    }
}
