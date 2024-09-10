using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "TriggerStateSO",  menuName = "Scriptable Objects/State/TriggerState")]
public class TriggerStateSO : CharStateSO {
    // Having more than one triggerState doesnt make sense, since each is mutually excluse
    [SerializeField]
    public TriggerStateDelivery triggerState;

    void OnEnable() {
        triggerState.group = this.name;
    }
}