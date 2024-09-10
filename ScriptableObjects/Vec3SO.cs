using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vec3SO", menuName = "ScriptableObjects/Variable/Vec3")]
public class Vec3SO : ScriptableObject
{
    [SerializeField]
    private Vector3 vec;

    public Vector3 get() {
        return vec;
    }

    public void set(Vector3 _vec) {
        vec = _vec;
    }

}
