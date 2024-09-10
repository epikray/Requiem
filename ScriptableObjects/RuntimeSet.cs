using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RuntimeSet", menuName = "Scriptable Objects/Runtime Set", order = 1)]
public class RuntimeSet : ScriptableObject
{
    [SerializeField]
    private List<GameObject> _set;

    public List<GameObject> set {
        get {
            return _set;
        }
    }

    public int Count {
        get {
            return _set.Count;
        }
    }

    public void Add(GameObject obj) {
        _set.Add(obj);
    }

    public void Remove(GameObject obj) {
        _set.Remove(obj);
    }

    public void Clear() {
        _set.Clear();
    }

    public GameObject At(int i) {
        if(_set.Count == 0)
            return null;

        if(i >= 0) {
            if(i < _set.Count) {
                return _set[i];
            }
            // if set.Count = 0 this breaks.
            return _set[_set.Count - 1];
        } 

        return _set[0];
    }

    

    void OnEnable() {
        if(_set != null)
            _set.Clear();
    }
}
