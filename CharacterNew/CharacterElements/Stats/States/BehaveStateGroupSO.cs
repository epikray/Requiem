using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu (fileName = "BehaveStateGroupSO",  menuName = "Scriptable Objects/State/BehaviourState")]
public class BehaveStateGroupSO : CharStateSO {

    [SerializeField]  
    public StateGroupLogic conditional;

    [SerializeField]
    public List<BehaviourStateDelivery> behaviourStates;

    // A unityEvent needs an instansiated object to call the function with. this is great
    // But it means the info needs to be injected when the state gets an owner.
    public UnityEvent e;

    public StateGroupLogic ExtractConditionals() {
        conditional.InjectGroupSO(this);
        return new StateGroupLogic(conditional);
    }
    public List<BehaviourStateDelivery> ExtractStates() {
        return new List<BehaviourStateDelivery>(behaviourStates);
    }

    public void Ping() {
        Debug.Log("Pong");
    }

    void OnEnable() {

        e.Invoke();
    }
}