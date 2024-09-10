using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;


// Build a State Asset similar but simpler to CharActions.
// States do not scale, they are simply applied as is, and grant either an AttributeStatePayload or BehaviourStatePayload

// I smell an abstraction here, but besides the name, an attribute, state, and trigger state do too different of things.
// Only thing that is common is how they are applied

public abstract class CharStateSO : ScriptableObject {
    [TextArea( 2, 8 )]
    public string description;
}


// TODO: Yeet this shit!
public class States {
    public static Dictionary<string, AttribStateGroupSO> attributes;
    public static void Init() {
        attributes = new Dictionary<string, AttribStateGroupSO> {
            {"Frozen", Resources.Load<AttribStateGroupSO>("Frozen")}
        };
    }
    
}