using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
[System.Serializable]
public struct CoreStats {
    //We could have a dictionary for the stas, but since we know exactly what stats are exist, its kinda iffy.
    //But as always Float stats are based around 1.0 being average human stats; then 0.1 is 1/10 the strength of a man, 10 is 10 times the strength of a man
    public Stat strength, dexterity, intellect, spirit;
}

[System.Serializable]
public struct PhysicalStats {
    //We could have a dictionary for the stas, but since we know exactly what stats are exist, its kinda iffy.
    //But as always Float stats are based around 1.0 being average human stats; then 0.1 is 1/10 the strength of a man, 10 is 10 times the strength of a man
    public Pool health, stamina, mana, will;
}

[System.Serializable]
public struct MentalStats {
    //We could have a dictionary for the stas, but since we know exactly what stats are exist, its kinda iffy.
    //But as always Float stats are based around 1.0 being average human stats; then 0.1 is 1/10 the strength of a man, 10 is 10 times the strength of a man
    public Stat intimidation, charm, wit, empathy;
}

[System.Serializable]
public struct DefensiveStats {
    //We could have a dictionary for the stas, but since we know exactly what stats are exist, its kinda iffy.
    //But as always Float stats are based around 1.0 being average human stats; then 0.1 is 1/10 the strength of a man, 10 is 10 times the strength of a man
    //armor is substractive
    //dodge is
    public Stat toughness, agility, resistance, stability;
}

[System.Serializable]
public struct Stats {
    //We could have a dictionary for the stas, but since we know exactly what stats are exist, its kinda iffy.
    //But as always Float stats are based around 1.0 being average human stats; then 0.1 is 1/10 the strength of a man, 10 is 10 times the strength of a man
    public CoreStats core;
    public PhysicalStats physical;
    public MentalStats mental;
    public DefensiveStats defensive;
}
*/

/*

its getting complex now. 
    4 damage categories each with 4 types -> 16 
    4 stat categories each with 4 stats -> 16


*/

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "ScriptableObjects/CharacterData")]
public class CharacterDataSO : ScriptableObject
{
    //General Char info
    public string charName;
    public float runSpeed, walkSpeed; 
    
    public float size; //Relative to a 
    
    public bool dead;

    public Logic logic;

    [SerializeField]
    protected PartySO party;
    //private int partyInd;

    [SerializeField]
    private GameObject gameObject;

    //public Stats stats;

    
    /*
    public List<Amplification> activAmplifications;
    public List<Condition> activeConditions;
    public List<ActionSO> actionList;
    */
    private Vector2 physAffliction;
    private Vector2 elemAffliction;
    private Vector2 psycAffliction;
    private Vector2 cosmAffliction;

    //protected ActionSO queuedAction;
    protected AnimActionPort aaPort;

    // What should it always need, but can't always be know beforehand?
    public bool Init(PartySO p) {
        party = p;

        return true;
    }

    public void UpdateData(float dTime) {

    }

    public float getReadyChargeC() {
        /*
        float readyChargeBase = 1/3f;
        
        CoreStats cstats = stats.core;

        float strC = 0.15f;
        float dexC = 0.55f;
        float intC = 0.25f;
        float spiC = 0.05f;

        float readyChargeC = readyChargeBase*(strC*cstats.strength.val + dexC*cstats.dexterity.val + intC*cstats.intellect.val + spiC*cstats.spirit.val);
        */
        return 0;
    }

    public void OnEnable() {
        //Debug.Log(gameObject + " CharDataSO, OnEnable");
        //Debug.Log(gameObject);
        resetStats();
        //having an issue were PartySO instances are not loaded (or something) before this func call
        //annoying, but doesnt really break anything except the first validation
        //validatePartyAssignment();
    }

    public PartySO GetParty() {
        return party;
    }

    public int GetPartyMemberIndex() {
        return party.GetMemberIndex(this);
    }

    public void RecieveAP(/*ActionPayload AP*/) {

        /*
        //Debug.Log(name + " recieved an ActionPayload");
        float dodge = (stats.defensive.agility.val - 100f);

        //0 accuracy means you need 0 dodge to not get hit
        //does 100 accuracy mean its undodgeable? maybe but at the same time maybe not
        //100 accuracy means you need 100 dodge to not get hit
        //Just looking at this, we see that agility is VERY powerful...

        if( AP.accuracy < dodge ) {
            //Penalize consecutive dodging to nerf its potential
            return;
        }
        handleImpacts(AP.impacts);
        handleAmplifications(AP.amplifications);
        handleConditions(AP.conditions);
        handleCosts(AP.costs);
        */
        
    }

    public void DoAction() {
        /*
        if(!queuedAction) {
            Debug.LogError("DoAction called before a action was queued! QueueAction should have been called first!");
        }
        queuedAction.DoActionAll();
        */
    }

    public void QueueAction(string skillname, CharacterDataSO CTo) {
        /*
        queuedAction = getAction(skillname);
        queuedAction.setTargets(this, CTo);
        */
    }

    public void QueueAction(string skillname, List<CharacterDataSO> CTo) {
        /*
        queuedAction = getAction(skillname);
        queuedAction.setTargets(this, CTo);
        */
    }

    public /*ActionSO*/ void getAction(string skillname) {
        //return actionList.Find(x => string.Equals(x.name, skillname));
    }

    public void setAnimActionPort(AnimActionPort _aaPort) {
        //aaPort = _aaPort;
    }

    public GameObject GetPrefab() {
        return gameObject;
    }

    /* --- PROTECTED --- */
    
    //Impacts need to go through defense values before deducted
    protected void handleImpacts(/*List<Impact> Is*/) {
        /*
        float IResult = 0;

        foreach(Impact I in Is) {
            IResult += handleImpact(I);
        }

        stats.physical.health.val -= IResult;

        if(stats.physical.health.val <= 0) {
            Debug.Log(name + " died!");
            dead = true;
            party.CheckAndSetAllDead();
        }

        if(IResult > (stats.defensive.stability.val/10f)) {
            //Trigger Hurt animation
            aaPort.Anim_Start_Hurt();
        }
        */
    }

    private float handleImpactValue(/*Impact I*/) {
        /*
        float res = I.value;
        // 0 toughness = 0 reduction, 100 (human) toughness = 10 reduction
        res = res - (stats.defensive.toughness.val)/10f;
        // 0 res = 0% dmg resisted, 100 (human) res = 10% resisted
        res = res * (1f - (stats.defensive.resistance.val)/1000f);
        */
        return 0;
    }

    private float handleImpact(/*Impact I*/) {
        // Do something with affinities
        /*
        float res = handleImpactValue(I);
        switch(I.dmgtype) {
            case DType.PHYS_FORCE: 
                physAffliction.x += res;
                return res;
                
            case DType.PHYS_BLUNT: 
                physAffliction.x -= res;
                return res;
                
            case DType.PHYS_SLASH: 
                physAffliction.y += res;
                return res;
                
            case DType.PHYS_PIERCE: 
                physAffliction.y -= res;
                return res;
                
            case DType.ELEM_FIRE: 
                elemAffliction.x += res;
                return res;

            case DType.ELEM_COLD: 
                elemAffliction.x -= res;
                return res;
                
            case DType.ELEM_LIGHTNING: 
                elemAffliction.y += res;
                return res;
                
            case DType.ELEM_WATER: 
                elemAffliction.y -= res;
                return res;
                
            case DType.PSYC_AFFECTION: 
                psycAffliction.x += res;
                return res;
                
            case DType.PSYC_FEAR: 
                psycAffliction.x -= res;
                return res;
                
            case DType.PSYC_STRESS: 
                psycAffliction.y += res;
                return res;
                
            case DType.PSYC_CALM: 
                psycAffliction.y -= res;
                return res;
                
            case DType.COSM_HEAL: 
                cosmAffliction.x += res;
                return res;
                
            case DType.COSM_DEGRADE: 
                cosmAffliction.x -= res;
                return res;
                
            case DType.COSM_SPACE: 
                cosmAffliction.y += res;
                return res;
                
            case DType.COSM_VOID: 
                cosmAffliction.y -= res;
                return res;
                
        }
        */
        return 0;
    }

    //Amplification modify a stat
    protected void handleAmplifications(/*List<Amplification> As*/) {

    }

    //Conditions are added with a duration and removed when its time runs out
    protected void handleConditions(/*List<Condition> Cs*/) {

    }

    //Costs are values deducted directly from a interval type stat
    protected void handleCosts(/*List<Cost> Cs*/) {
        // Although a Cost can be applied to a regular Stat, its somewhat ill defined
        // So far Costs apply in a normal manner to Pool type stats
        /*
        foreach(Cost C in Cs) {
            switch (C.stat) {
                case SType.HEALTH:
                    stats.physical.health.val -= C.value;
                    break;
                case SType.STAMINA:
                    stats.physical.stamina.val -= C.value;
                    break;
                case SType.MANA:
                    stats.physical.mana.val -= C.value;
                    break;
                case SType.WILL:
                    stats.physical.will.val -= C.value;
                    break;
            }
        }   
        */
    }

    /* --- PRIVATE --- */
    private void resetStats() {
        /*
        stats.physical.health.reset();
        stats.physical.stamina.reset();
        stats.physical.mana.reset();
        stats.physical.will.reset();

        stats.core.strength.reset();
        stats.core.dexterity.reset();
        stats.core.intellect.reset();
        stats.core.spirit.reset();

        stats.mental.intimidation.reset();
        stats.mental.charm.reset();
        stats.mental.wit.reset();
        stats.mental.empathy.reset();

        stats.defensive.toughness.reset();
        stats.defensive.agility.reset();
        stats.defensive.resistance.reset();
        stats.defensive.stability.reset();

        dead = false;
        */
    }

    
    private bool validatePartyAssignment() {
        PartySO playerParty = (PartySO)Util.Objects.find<PartySO>("PlayerParty");
        bool inPlayerParty = false;

        foreach (CharacterDataSO C in playerParty.members) {
            if(C == this) {
                if(inPlayerParty != true) {
                    //Debug.Log(name + " is in PlayerParty");
                    inPlayerParty = true;
                } else {
                    Debug.LogError("Two or more instances of " + name + " is in the same party!");
                    return false;
                }
                
            }
        }

        PartySO enemyParty = (PartySO)Util.Objects.find<PartySO>("EnemyParty");
        bool inEnemyParty = false;

        foreach (CharacterDataSO C in enemyParty.members) {
            if(C == this) {
                
                if(inEnemyParty != true)  {
                    //Debug.Log(name + " is in EnemyParty");
                    inEnemyParty = true;
                } else {
                    Debug.LogError("Two or more instances of " + name + " is in the same party!");
                    return false;
                }
            }
        }

        if(inPlayerParty && inEnemyParty) {
            Debug.LogError(name + " cannot be in both Enemy- and PlayerParty!");
            return false;
        } else {
            return true;
        }
    }
    
}
