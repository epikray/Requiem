using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



// And as well, we need to give proper consideration to how these conditionals are tracked.
// Since there are good reason to have them be assigned both 'On Source' level and 'On Group' level,
// were Source level would have to trickle them down to Group level.

[CreateAssetMenu (fileName = "AttribStateGroupSO",  menuName = "Scriptable Objects/State/AttributeStateGroup")]
public class AttribStateGroupSO : CharStateSO {

    [SerializeField]  
    public StateGroupLogic conditional;

    [SerializeField]  
    public List<AttributeStateDelivery> attributeStates;

    public StateGroupLogic ExtractConditionals() {
        conditional.InjectGroupSO(this);
        return new StateGroupLogic(conditional);
    }

    public List<AttributeStateDelivery> ExtractStates() {
        return new List<AttributeStateDelivery>(attributeStates);
    }

    void OnEnable() {
        
    }
}