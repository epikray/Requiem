using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class AffinityManager{

    // BALANCE!!!
    const float primeMod = 1.1f; // Catergory to Category
    const float secondaryMod = 1.1f; // Inter categorical element to element
    const float tertiaryMod = 1.25f; // Outer catergorical element to element 
    private AffinitiesInstance aInstance;

    // How do we set up the relations?
    // If we are attacked by lets say force
    // Then we two big primary affinities to influence, two secondary, and one internal to the primary.
    // That means 5 Affinities to affect.

    private Dictionary<ACT_DMG, ((ACT_DMGCAT, ACT_DMGCAT), (ACT_DMG, ACT_DMG), ACT_DMG)> affRelations;

    public AffinityManager(CharacterInstance _cInstance) {
        aInstance = _cInstance.Affinities;


        // Is this a good way to model the relations, or is there a better one?
        affRelations = new Dictionary<ACT_DMG, ((ACT_DMGCAT, ACT_DMGCAT), (ACT_DMG, ACT_DMG), ACT_DMG)>();

        // The physicals
        affRelations.Add(ACT_DMG.FORCE, 
        (
            (ACT_DMGCAT.ALCHEMICAL, ACT_DMGCAT.MENTAL), // primary
            (ACT_DMG.COLD, ACT_DMG.MANIA), // secondary
            ACT_DMG.BLUNT) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.BLUNT, 
        (
            (ACT_DMGCAT.ALCHEMICAL, ACT_DMGCAT.MENTAL), // primary
            (ACT_DMG.CHARGE, ACT_DMG.FEAR), // secondary
            ACT_DMG.SLASH) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.SLASH, 
        (
            (ACT_DMGCAT.ALCHEMICAL, ACT_DMGCAT.MENTAL), // primary
            (ACT_DMG.HEAT, ACT_DMG.STUPEFY), // secondary
            ACT_DMG.PIERCE) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.PIERCE, 
        (
            (ACT_DMGCAT.ALCHEMICAL, ACT_DMGCAT.MENTAL), // primary
            (ACT_DMG.DOUSE, ACT_DMG.LUST), // secondary
            ACT_DMG.FORCE) // tertiary 
        ); 

        // The alchemicals
        affRelations.Add(ACT_DMG.HEAT, 
        (
            (ACT_DMGCAT.PHYSICAL, ACT_DMGCAT.META), // primary
            (ACT_DMG.SLASH, ACT_DMG.LIFE), // secondary
            ACT_DMG.COLD) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.DOUSE, 
        (
            (ACT_DMGCAT.PHYSICAL, ACT_DMGCAT.META), // primary
            (ACT_DMG.PIERCE, ACT_DMG.ORDER), // secondary
            ACT_DMG.CHARGE) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.COLD, 
        (
            (ACT_DMGCAT.PHYSICAL, ACT_DMGCAT.META), // primary
            (ACT_DMG.FORCE, ACT_DMG.DEATH), // secondary
            ACT_DMG.HEAT) // tertiary 
        ); 
        affRelations.Add(ACT_DMG.CHARGE, 
        (
            (ACT_DMGCAT.PHYSICAL, ACT_DMGCAT.META), // primary
            (ACT_DMG.BLUNT, ACT_DMG.CHAOS), // secondary
            ACT_DMG.DOUSE) // tertiary 
        ); 
        
    }

    public void InfluenceAffinities(ACT_DMG type, float v) {
        ((var primary1, var primary2), (var secondary1, var secondary2), var tertiary) = affRelations[type];

        aInstance.InfluenceAffinityCategory(primary1, v*primeMod);
        aInstance.InfluenceAffinityCategory(primary2, v*primeMod);

        aInstance.InfluenceAffinity(secondary1, v*secondaryMod);
        aInstance.InfluenceAffinity(secondary2, v*secondaryMod);
        aInstance.InfluenceAffinity(tertiary, v*tertiaryMod);
    }

    public float AffinityInfluence(ACT_DMG type) {
        ACT_DMGCAT typecat = (ACT_DMGCAT)((((int)type - 1)/4) + 1); // Theres nothing dubious happening here. but enums are obnoxious
        Affinity aff = aInstance.GetAffinity(type);

        Affinity affCat = aInstance.GetAffinityCategory(typecat);

        float res = 2f - aff.val/100f*(affCat.val/100f); 

        //Debug.Log("AffinityInfluence " + type + " res " + res);
        return res;
    }
}