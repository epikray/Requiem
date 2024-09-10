using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEventChannelSO", menuName = "ScriptableObjects/VoidChannel")]
public class VoidEventChannelSO : ScriptableObject
{
    public UnityAction OnEventRaised;

    private GameObject _caller;
    private object _data;

    public GameObject caller {
        get {
            return _caller;
        }
    }

    public object data {
        get {
            return _data;
        }
    }

    public void RaiseEvent(GameObject inCaller = null, object inData = null) {
        
        _caller = inCaller;
        _data = inData;

        if(OnEventRaised != null) {
            OnEventRaised.Invoke();
        }
    }
    
}
