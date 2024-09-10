using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Tough design decision now...
// I have my pc that can use 2 attacks, slash and bash. One deals slash dmg, one deals blunt, simple.
// However, my character is holding a sword... how do I reason about bashing with a sword?
// In general, how do I reason about dealing an arbitrary damage with an arbitrary tool?




[CreateAssetMenu (fileName = "WeaponSO",  menuName = "Scriptable Objects/Equipment/Weapon")]
public class WeaponSO : ScriptableObject
{   
    [Header("Physical Attributes")]
    public float balance; // a multiplier to an actions accuracy
    public float weight; // a multiplier to stamina cost

    // idfk know at this point. something something force is added dmg, something something coeff is multiplicative
    public float forceValue; 
    public float bluntCoeff;
    public float slashCoeff;
    public float pierceCoeff;

    [Header("Magical Attributes")]
    public float intricacy; // Multiplier to int
    public float awe; // Multiplier to spirit
    public float fociCoeff; // Multiplier to skill cost

    public ImpactResult AmplifyImpact(ImpactResult impact) {

        return impact;
    }
}

