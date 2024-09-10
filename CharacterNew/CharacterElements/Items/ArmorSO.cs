using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: Skin is not armor! Armor is specifically and external layer upon the skin or exoskeleton.
// Armor can be removed, skin cannot. Skin/exoskeleton is modified by toughness and resistance!

[CreateAssetMenu (fileName = "ArmorSO",  menuName = "Scriptable Objects/Equipment/Armor")]
public class ArmorSO : ScriptableObject
{
    [Header("Physical Attributes")]
    public float forceRecuction;    //Flat phys reduction
    public float bluntAbsorption;   //Percent blunt reduction
    public float slashAbsorption;   //Percent blunt reduction
    public float pierceAbsorption;  //Percent blunt reduction

    [Header("Physical Attributes")]
    public float insulation; // ??? How do I wanna deal with the alchemicals?
}

/*
public class Armor
{
    float forceCoeff;
    float bluntCoeff;
    float slashCoeff;
    float pierceCoeff;

    public ImpactResult ReduceImpact(ImpactResult impact) {

        return impact;
    }
}
*/