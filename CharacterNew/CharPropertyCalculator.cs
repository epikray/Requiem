using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;


// Final attributes is based on the charIdInstance, states, and attributes.
public class PropertyCalculator {
    CharacterInstance charInstance;
    CharacterStates charStates;

    AfflictionManager affManager;

    //ATTRIBUTE[][] attributeRelations; 
    Dictionary<
        ATTRIBUTE, 
        List<KeyValuePair<ATTRIBUTE, float>>
        > attributeRelations;
    
    Dictionary<
        ATTRIBUTE, 
        List<KeyValuePair<ACT_AFLCT, float>>
        > afflictionRelations;

    public PropertyCalculator(CharacterInstance _charInstance, CharacterStates _charStates) {
        charInstance = _charInstance;
        charStates = _charStates;
        
        // Attribute Relations
        attributeRelations = new Dictionary<
            ATTRIBUTE, 
            List<
                KeyValuePair<
                    ATTRIBUTE, 
                    float>
                >
            > ();
        // TODO: Fix attributes for most relevant things

        attributeRelations[ATTRIBUTE.ACTION_CHARGE_SPEED] = new List<KeyValuePair<ATTRIBUTE, float>>
            { new KeyValuePair<ATTRIBUTE, float>(ATTRIBUTE.DEXTERITY, 0.25f), };

        attributeRelations[ATTRIBUTE.ACTION_TIME_SPEED] = new List<KeyValuePair<ATTRIBUTE, float>>
            { new KeyValuePair<ATTRIBUTE, float>(ATTRIBUTE.DEXTERITY, 0.75f), };

        /*
        attributeRelations[ATTRIBUTE.IMPACT_REDUCTION] = new List<KeyValuePair<ATTRIBUTE, float>>
            { new KeyValuePair<ATTRIBUTE, float>(ATTRIBUTE.TOUGHNESS, 1f), };
        */
        // This turns into a fuggin busted attribute. 50 resistance means u take double damage from trauma
        attributeRelations[ATTRIBUTE.AFFLICTION_SPEED_REDUCTION] = new List<KeyValuePair<ATTRIBUTE, float>>
            { new KeyValuePair<ATTRIBUTE, float>(ATTRIBUTE.RESISTANCE, 1f), };
        
        // Affliction Relations
        afflictionRelations = new Dictionary<
            ATTRIBUTE, 
            List<
                KeyValuePair<
                    ACT_AFLCT, 
                    float>
                >
            > ();
        
        // Phys afflictions
        afflictionRelations[ATTRIBUTE.ACTION_CHARGE_SPEED] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { 
                new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.FORCE, 1f), 
                new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.TRAUMA, 1f), 
            };
        
        afflictionRelations[ATTRIBUTE.REGEN_SPEED] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { 
                new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.BLEED, 1f), 
                new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.PUNCT, 1f), 
            };


        // Elemental attribute reductions
        // Burn
        afflictionRelations[ATTRIBUTE.TOUGHNESS] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.BURN, 1f), };

        // Freeze
        afflictionRelations[ATTRIBUTE.AGILITY] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.FREEZE, 1f), };

        // Drench
        afflictionRelations[ATTRIBUTE.BALANCE] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.DRENCH, 1f), };

        // Shock
        afflictionRelations[ATTRIBUTE.RESISTANCE] = new List<KeyValuePair<ACT_AFLCT, float>> 
            { new KeyValuePair<ACT_AFLCT, float>(ACT_AFLCT.SHOCK, 1f), };
        
    }

    public void InjectAfflictionManager(AfflictionManager _affManager) {
        affManager = _affManager;
    }

    public float GetStat(STAT stat) {
        Stat s = StatFromCInstance(stat);
        float res = s.val;

        List<AttributeStatePayload> relStates = charStates.GetAffectingStatesNew((ATTRIBUTE)stat);

        float valueToAdd = 0;
        float valueToMult = 1f;
        float valueToMult2 = 1f;

        foreach(AttributeStatePayload state in relStates) {
            if(state.modType == StatModType.Flat) {
                valueToAdd += state.value;
            } else if (state.modType == StatModType.Percent) {
                valueToMult += state.value/100f;
            } else {
                valueToMult2 *= state.value/100f;
            }
        }
        if(valueToMult < 0) {
            valueToMult = 0;
        }

        res = (res + valueToAdd)*valueToMult*valueToMult2;

        return res;
    }

    /* 
        Item1 is current value in the pool. Item2 is the max (more like capacity) of the pool.
    */
    public (float, float) GetPool(STAT stat) {
        Pool p = PoolFromCInstance(stat);
        //List<State>

        if(p == null) {
            Debug.Log("POOLS CLOSED!");
        }
        

        List<AttributeStatePayload> relStates = charStates.GetAffectingStatesNew((ATTRIBUTE)stat);
        

        float valueToAdd = 0;
        float valueToMult = 1f;
        float valueToMult2 = 1f;

        foreach(AttributeStatePayload state in relStates) {
            if(state.modType == StatModType.Flat) {
                valueToAdd += state.value;
            } else if (state.modType == StatModType.Percent) {
                valueToMult += state.value/100f;
            } else {
                valueToMult2 *= state.value/100f;
            }
        }
        if(valueToMult < 0f) {
            valueToMult = 0f;
        }

        float res1 = (p.val + valueToAdd)*valueToMult*valueToMult2;
        float res2 = (p.max + valueToAdd)*valueToMult*valueToMult2;

        return (res1, res2);
    }

    public void HitPool(STAT stat, float v, bool bound = false) {
        // What do we do to make this reasonable in regarding scaling pools?
        // Inversly scale the hit? (unscaled_val/scaled_val)*hit;

        Pool p = PoolFromCInstance(stat);
        (float scaledval, float scaledmax) = GetPool(stat);
        //Debug.Log("HitPool: " + stat);
        //p.Print();

        float c = p.max/scaledmax;
        float hit = c*v;

        // TODO: These seems prime for bugs.
        // I think it works, but im not in the mood to go through the logic
        if(bound) {
            float newhit = hit;

            //newhit = Mathf.Clamp(hit, p.val - p.max, p.val);

            if(p.val > p.max) {

                newhit = Mathf.Clamp(hit, 0f, p.val);
            } else if (p.val <= 0f) {
                newhit = Mathf.Clamp(hit, p.val - p.max, 0f);
            } else {
                newhit = Mathf.Clamp(hit, p.val - p.max , p.val);
            }
            hit = newhit;
        }   
        
        p.ModifyVal(-hit);
    }

    public void ScarPool(STAT stat, float v, bool bound = false) {
        Pool p = PoolFromCInstance(stat);
        (float scaledval, float scaledmax) = GetPool(stat);
        //Debug.Log("ScarPool: " + stat);
        //p.Print();

        float c = p.max/scaledmax;
        float scar = c*v;
        
        if(bound) {
            float newScar = scar;
            
            
            if(p.max <= p.baseValue) {
                // max < base, only healing hits (negative)

                newScar = Mathf.Clamp(scar,  p.max - p.baseValue, 0f);
                //                          value < 0
                
            } else if (p.max > p.baseValue) {
                // max > base, only hurting hits (positive)
                newScar = Mathf.Clamp(scar, 0f, p.max - p.baseValue);
            }
            //Debug.Log("We are binding HitPool: max " + p.max + " val " + p.val + " hit " + hit + " new hit " + newhit);
            scar = newScar;
        }
        p.ModifyMax(-scar);
    }
    public void HurtPool(STAT stat, float v, bool boundHit = false, bool boundScar = false) { 
        HitPool(stat, v, boundHit);
        ScarPool(stat, v/10f, boundScar);
    }

    public float GetAttribute(ATTRIBUTE attribute) {
        if((int)attribute <= 15) {
            return GetStat((STAT) attribute);
        }
        float res = 1f;

        List<AttributeStatePayload> relStates = charStates.GetAffectingStatesNew(attribute);
        List<KeyValuePair<ATTRIBUTE, float>> relStats;
        List<KeyValuePair<ACT_AFLCT, float>> relAfflictions;

        /*
            Get affecting stats
        */

        float statInfluence = 1f;
        if(attributeRelations.ContainsKey(attribute)) {
            relStats = attributeRelations[attribute];

            foreach(KeyValuePair<ATTRIBUTE, float> attributeInfluence in relStats) {
                statInfluence *= 1f - attributeInfluence.Value + attributeInfluence.Value*(GetStat((STAT)attributeInfluence.Key)/100f);
            }
        }
        // affliction.value does not act as a stat, having 100 as a bases and shifting up and down
        // aff.value is zero when not afflicted, going up and up towards more and more affliction.
        
        if(afflictionRelations.ContainsKey(attribute)) {
            relAfflictions = afflictionRelations[attribute];

            // Afflictions need to be normalized somehow.
            // a cold bolt that hits for 25 dmg, will produce a 500 value when called,
            // when a strike hitting for 25 would be 12.5, 25*0.5 duration.
            float healthScale = GetPool(STAT.HEALTH).Item2;
            foreach(KeyValuePair<ACT_AFLCT, float> afflictionInfluence in relAfflictions) {
                statInfluence *= 
                1f - afflictionInfluence.Value + 
                afflictionInfluence.Value*Math.Clamp(1f - affManager[afflictionInfluence.Key].value/healthScale, 0f, 1f);
            }
        }
        float valueToAdd = 0;
        float valueToMult = 1f;
        float valueToMult2 = 1f;

        foreach(AttributeStatePayload state in relStates) {
            if(state.modType == StatModType.Flat) {
                valueToAdd += state.value;
            } else if (state.modType == StatModType.Percent) {
                valueToMult += state.value/100f;
            } else {
                valueToMult2 *= state.value/100f;
            }
        }
        if(valueToMult < 0f) {
            valueToMult = 0f;
        }

        res = (res*statInfluence + valueToAdd)*valueToMult*valueToMult2;

        //if(attribute == ATTRIBUTE.AFFLICTION_ACCRUEMENT) Debug.Log( "Got asked for AFFLICTION_ACCRUEMENT! : " + res);

        return res;
    }

    public float ScaleOffPool() {
        return 0f;
    }

    /* STAT Calculators */
    private Stat StatFromCInstance(STAT stat) {
        switch(stat) {
            case STAT.STRENGTH :
                return charInstance.Stats.Strength;
            case STAT.DEXTERITY :
                return charInstance.Stats.Dexterity;
            case STAT.INTELLECT :
                return charInstance.Stats.Intellect;
            case STAT.SPIRIT :
                return charInstance.Stats.Spirit;
            case STAT.TOUGHNESS :
                return charInstance.Stats.Toughness;
            case STAT.AGILITY :
                return charInstance.Stats.Agility;
            case STAT.RESISTANCE :
                return charInstance.Stats.Resistance;
            case STAT.BALANCE :
                return charInstance.Stats.Balance;
            
        }
        return null;
    }

    private Pool PoolFromCInstance(STAT stat) {
        switch(stat) {
            case STAT.HEALTH :
                return charInstance.Stats.Health;
            case STAT.STAMINA :
                return charInstance.Stats.Stamina;
            case STAT.FOCUS :
                return charInstance.Stats.Focus;
            case STAT.WILL :
                return charInstance.Stats.Will;
            case STAT.RAGE :
                return charInstance.Stats.Rage;
            case STAT.GRACE :
                return charInstance.Stats.Grace;
            case STAT.ATTENTION :
                return charInstance.Stats.Attention;
            case STAT.AWARNESS :
                return charInstance.Stats.Awarness;
        }
        Debug.LogError("PoolFromCInstance was asked to find STAT" + stat + ". Figure it out!");
        return null;
    }
}

