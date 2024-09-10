using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Calling this an Affinity when the structure is so simple seems unwarranted.
// We know what the idea of an Affinity is. But what does it do.
[Serializable]
public class Affinity {
    [SerializeField]
    private float _valBase;
    private float _val;

    public float valBase {
        get {
            return _valBase;
        }
    }
    public float val {
        get {
            return _val;
        }
    }

    public Affinity() {
        _valBase = 100f;
        _val = _valBase;
    }

    public Affinity(float _base) {
        _valBase = _base;
        _val = _valBase;
    }

    public Affinity(Affinity _in) {
        _valBase = _in._valBase;
        _val = _in._valBase;

    }

    // TODO: How should opposing affinities, like Heat and Cold work when an Affinity can be negative or positive
    public void Influence(float v) {
        // affecting away from base has inverse effectiveness
        // affecting towards base has increasing effectiveness the further away
        // TODO: how does this work with other affinities?
        // v is zero...

        _val += v;
        
    }
    public void Tick(float dt) {
        // How does an affinity work over time?
        // Flat regen, proportional regen, no regen at all?


        _val = _val + 0.2f*(_valBase - _val)*dt; // _val < valBase => positive regen, _val > valBase => negative regen
    }

    public override string ToString() {
        return "v = " + val + "\n" + "base = " + valBase;
    }

}

[CreateAssetMenu(fileName = "CharacterAffinities", menuName = "Scriptable Objects/Character Affinities")]
public class CharacterAffinities : ScriptableObject
{
    [Header("Physical")]
    public Affinity PhysicalAffinity;
    public Affinity ForceAffinity;
    public Affinity BluntAffinity;
    public Affinity SlashAffinity; 
    public Affinity PierceAffinity;

    [Header("Alchemical (Elemental)")]
    public Affinity AlchemicalAffinity;
    public Affinity HeatAffinity;
    public Affinity ColdAffinity;
    public Affinity DouseAffinity;
    public Affinity ChargeAffinity;


    void OnValidate() {

    }
}

public class AffinitiesInstance {

    float counter;
    private Dictionary<ACT_DMG, Affinity> affinities;
    private Dictionary<ACT_DMGCAT, Affinity> categoricalAffinities;
    public AffinitiesInstance(CharacterAffinities affinSO) {
        counter = 0f;
        affinities = new Dictionary<ACT_DMG, Affinity>();
        
        Affinity temp = affinSO.ForceAffinity;
        //temp.val = temp.valBase;
        affinities.Add(ACT_DMG.FORCE, new Affinity(temp));
        temp = affinSO.BluntAffinity;
        affinities.Add(ACT_DMG.BLUNT, new Affinity(temp));
        temp = affinSO.SlashAffinity;
        affinities.Add(ACT_DMG.SLASH, new Affinity(temp));
        temp = affinSO.PierceAffinity;
        affinities.Add(ACT_DMG.PIERCE, new Affinity(temp));
        
        temp = affinSO.HeatAffinity;
        affinities.Add(ACT_DMG.HEAT, new Affinity(temp));
        temp = affinSO.ColdAffinity;
        affinities.Add(ACT_DMG.COLD, new Affinity(temp));
        temp = affinSO.DouseAffinity;
        affinities.Add(ACT_DMG.DOUSE, new Affinity(temp));
        temp = affinSO.ChargeAffinity;
        affinities.Add(ACT_DMG.CHARGE, new Affinity(temp));

        categoricalAffinities = new Dictionary<ACT_DMGCAT, Affinity>();
        temp = affinSO.PhysicalAffinity;
        categoricalAffinities.Add(ACT_DMGCAT.PHYSICAL, new Affinity(temp));
        temp = affinSO.AlchemicalAffinity;
        categoricalAffinities.Add(ACT_DMGCAT.ALCHEMICAL, new Affinity(temp));
    }

    public void TickAll(float dt) {
        foreach(KeyValuePair<ACT_DMG, Affinity> affinityPair in affinities) {
            affinityPair.Value.Tick(dt);
            
            if(counter > 1f) {
                //Debug.Log("Affinity " + affinityPair.Key + ", " + affinityPair.Value.ToString()); 
            }
        }

        foreach(KeyValuePair<ACT_DMGCAT, Affinity> affinityPair in categoricalAffinities) {
            affinityPair.Value.Tick(dt);
            if(counter > 1f) {
                //Debug.Log("Affinity " + affinityPair.Key + ", " + affinityPair.Value.ToString()); 
            }
        }
        if(counter > 1f) {
            counter = 0f;
        }
        counter += dt;
    }

    public void InfluenceAffinity(ACT_DMG type, float v) {
        
        if(!affinities.ContainsKey(type)) return;

        affinities[type].Influence(v);
        //Debug.Log("InfluenceAffinity: Type " + type + " is " + affinities[type].ToString());
    }

    public void InfluenceAffinityCategory(ACT_DMGCAT type, float v) {

        if(!categoricalAffinities.ContainsKey(type)) return;

        categoricalAffinities[type].Influence(v);
        //Debug.Log("InfluenceAffinityCategory: Type " + type + " is " + categoricalAffinities[type].ToString());
    }

    public Affinity GetAffinity(ACT_DMG type) {

        if(!affinities.ContainsKey(type)) return null;


        return affinities[type];
    }
    
    public Affinity GetAffinityCategory(ACT_DMGCAT type) {
        if(!categoricalAffinities.ContainsKey(type)) return null;

        return categoricalAffinities[type];
    }

}