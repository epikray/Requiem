using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


// Make a list of these! One part is physical, one part is special, etc etc.

// A character can only be in one stance or trigger at a time.
// doesnt make sense to have it on a payload then.

// What makes an attack different from a spell, stance change or trigger set?
// An attack makes use of an armament to hit an enemy. Using properties of the armament to enhance the attack (actionbase + armbase)*str?
// A skill/spell makes use of an armament as a 'focus'. Allright, weird gameplay idea, using an item is a skill. 
//  But it makes the most sense in this construct im making. Physical Skill that deals fire damage? Use your weapon on a flintstone, consumes the flintstone and some focus

//CharActionSO -> Attacks, Skills, Stance, Strategy is badly designed.
// Stances and strategy dont incur and impact yet they need to implement CalcImpacts
// As well, stance changes and costs are immediates. So even if Attacks and Skills don't incur a Stance change,
// they work with the implication that they could...

public abstract class CharActionSO : ScriptableObject
{   
    [TextArea( 2, 8 )]
    public string description;

    protected ACT_TYPE _type = ACT_TYPE.NON;
    public ACT_TYPE type {
        get {
            return _type;
        }
    }

    [SerializeField]
    public float recoveryTime; // How long should recharge be?

    [SerializeField]
    public float attackTime; // How long should the animations be?

    [SerializeField]
    public string attackVFXEvent; // What VFX should be played on the actor ?

    [SerializeField]
    public string hitVFXEvent; // What VFX should be played on the target ?

    [SerializeField]
    public List<CostPayload> costs;

    

    public virtual void GetAttributes() {
        
    }

    public abstract int GetImpactListCount();
    public abstract float GetTotalDamage();
    public abstract float GetAverageDamage();
    public abstract float GetDamageAdjustedByAccuracy();
    public abstract float GetTotalAccuracy();
    public abstract float GetAverageAccuracy();
}

