using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    What philosophy do we want to take into damage scaling?
    Without the player needing to calculate anything; 
    given the base damage, characteristics of the action and the characters characteristics:
    The results of the the actions should be reasonably geussable, i.e. 
    My character is using an attacking move with 50 base damage, he has an above average strength (<25%), 
    and not knowing the opponets defence, but we assume average defences, he should take 75% damage.
    Or whatever...
    The key is that the opponent needs to be able to reason about what is strong and what isnt, and given
    what the opponent is strong or weak against, change plans accordingly. Like pokemon :DDD, but on a more
    continous scale and not just a quad-nary of 4x 2x 1x 0.5x 0.25x.

    Core stats:
        Strength, Dexterity, Intellect, Spirit
        influences
        physical effect potency, physical effects chance to succed, special effect potency, special effects chance to succed.
            I dont want to make too much of a physical/special split, 
            i want there to be some blend so that a high intellect attacker (tactical attacker) or high strength caster (aggresive caster)
            has something that high intellect caster or high strength attacker dosent have. 
            This will have to be done with special actions that help attackers, (apply fire, shield magic)
            and physical actions that help casters (block/dodge to not interrupt cast)
            But this is good, dont let str or int go to waste by being a caster or attacker, allow the player to use str to suppliment int

    Phys Pools:
        Health, Stamina, Will(intellect pool), Focus(spirit pool)
        influences
        availabilty of actions, hp needed to fight, stamina needed to attack, will needed to cast, focus needed to mod(stances, triggers)


    These for the time have nothing to do with actions, but i got ideas
    Mental? Make it a pool???
        Curage/fear, charm/'anticharm', wit/bluntness, wisdom/foolishness

    Defensive?
        toughness, agility, resistance, balance

*/


/*
    We have certain repeating word affixes to describe at what stage of processing
    a struct is.
    Delivery -> First Stage contains specific data regarding how and a struct should be
        scaled through the characters characteristics, like stats, stances, and equipment.
        DoAction
    Payload, -> At this point all necesary scaling has been done, and we are using these
        to apply effects to the character.

    Payload_Prcsd -> These are necesary for making an aftermath report for the Behaviour


*/

[Serializable]
public enum ACT_DMG : int {
    // phys, these arent exlusive to each other, but act more as levels from full  body damage, to pin point
    NON,

    FORCE,  // Whole body trauma, R
    BLUNT,  // area trauma, Y
    SLASH,  // edge traume, G
    PIERCE, // point trauma, B

    // eles, temperature and charge
    HEAT, //oposite of cold, dual of charge, R
    DOUSE, //oposite of charge, dual of cold, Y
    COLD, //oposite of heat, dual of douse, G
    CHARGE, //oposite of douse, dual of heat, B

    // mentals, to do with the mental pools, spook, charm, encourage, etc.
    FEAR, // Fear, R
    MANIA, // Mania, Y
    LUST, // Lust, G
    STUPEFY, // Stupefy, B
    

    // More abstract damages
    ORDER, // Y
    LIFE, // R
    CHAOS, // G
    DEATH, // B
}

[Serializable]
public enum ACT_DMGCAT : int {
    NON,
    PHYSICAL,
    ALCHEMICAL,
    MENTAL,
    META,

}

public enum ACT_TYPE : int {
    NON,
    ATTACK,
    STANCE,
    SKILL,
    STRATEGY,
}

public enum ACT_SCALE : int {
    SPECIAL, //neither, use items, change stance, set trigger,
    PHYSICAL, // action scales with physical capabilities
    MAGIC, // action scales with mental/spiritual capabilities
}

public enum ACT_HIT : int {
    MISS,
    GLANCE,
    HIT,
    CRIT
}

public enum ACT_STUN : int {
    NON,
    NORMAL,
}

// Base of what an Impact needs
public class Impact {
    public ACT_DMG dmgType; // The pool(s) this drains from
    public float damage; //if phys, modded by str, if magic, modded by int
    public float accuracy;
}

// When scaled by sender
[Serializable]
public class ImpactDelivery : Impact {
    public ACT_SCALE scaleType; // How should the action scale
}

// When sent to another
[Serializable]
public class ImpactPayload : Impact {
    public bool calcHitType = true;
    public ACT_HIT hitType;
    public bool calcStunType = false;
    public ACT_STUN stunType;
}


// What the fuck are we doing?


// TODO: Remake this.
// I want to squash all instances of the same damage type into one.
// I want info like, is crit, is glance, whatever, to be stored explicitly.
// And I want to work out some questions regarding damage, like;
// What does it mean for an impact to deal slashing and piercing dmg or heat and cold dmg 
// to be dealt from one impact? Should they be balanced in some manner? Or does it make sense in the same
// way you can get stabbed at different places on the body at the same time?
// does this have implications for defence?
public class ImpactResult {
    public ACT_TYPE actType;
    public List<ImpactPayload> impacts;
    public ImpactResult() {
        impacts = new List<ImpactPayload>();
    }
    public override string ToString() {
        string res = "";
        foreach(ImpactPayload IP in impacts) {
            res += IP.damage + " " + IP.dmgType + " damage at " + IP.accuracy + " accuracy, ";
        }
        return res;
    }
}

public class ImpactAftermath {
    public int stuns;
    public int crits;
    public int hits;
    public int glances;
    public int misses;
}

public enum BehaviourModType : int {
    ADD,
    OVERRIDE,
}

// Attribute! Make Attributes inherit from Stats?
// How do I do freeze a Char?
// 1. They cant act. 2. They cant move. 3. They get a layer of ICE armor

// adding/modifying a behaviour, changing a stat, giving a trigger
[System.Serializable]
public class AttributeStatePayload {
    // Identify what StateSO this minor state belongs to
    public ATTRIBUTE attribToChange;
    public StatModType modType;
    public float value;

    public AttributeStatePayload() {}

    public AttributeStatePayload(ATTRIBUTE _statToChange, StatModType _modType, float _value) {
        attribToChange = _statToChange;
        modType = _modType;
        value = _value;
    }
}


// TODO: Some states need to have Stop conditions. Like a Block stance can very much be a one time use, so OnRecieveAction it expires after calc.

// Alright, now - to consider - the fuck is a behaviour state
// Some limited state during which a function will be inserted into a step of the Behaviour loop, or for the ActionFunction to calculate
// Attribute states are always active stat changes to a character.
// A behaviour states are event callbacks, so; OnHit -> call this function
// I got an idea, Behaviour raises events that the behaviour is subscribed to, OnHit, OnAttack, OnSkill, OnEverySecond etc... 
// more can be made up, but it will be easier if its in our time/phys domain and not in the data domain.
// These behaviours respond by giving a function to call (it needs to be able to take a Behaviour class as argument otherwise it will be annoying or unworkable), or telling the Behaviour to call
// a function it knows of its own.

// It will then work something like this; Behaviour has a function OnRecieveAction called when an action has been sent to it. This function raises an event OnRecieveAction in which our relevant state is subscribed to. 
// The state will then return an identifier (enum) for a function to be called. ActionFunction, or some new class, takes this identifier and runs that function, 
// perchance with arguments from the behaviour.

// BehaviourStates have no animation tied to them, as that would make them forced actions.
// BehaviourStates can however, if thats a dimension to wish for, force a character to do an acion.
public enum CHAR_EVENT : int {
    ON_HIT,

    ON_DOACTION,

    ON_SENDACTION,
    ON_SENDAATTACK,

    ON_REACTACTION,

    ON_RECIEVEACTION,
    ON_RECIEVEATTACK,
}

public enum ACTION_FUNC_ID : int {
    BLOCK,
    EVADE,
    ALWAYS_CRITICAL,
}

[System.Serializable]
public class BehaviourStatePayload {
    // At what event does this state behaviour mod.
    public CHAR_EVENT eventToMod;
    // Whether function overrides or simply adds a step to each calc event
    public BehaviourModType modType;
    // Lookup ID for what function to call at the event stage
    public ACTION_FUNC_ID behaviour;
    // Arguments to the Action Function

    public BehaviourStatePayload() {}

    public BehaviourStatePayload(CHAR_EVENT _eventToMod, BehaviourModType _modType) {
        eventToMod = _eventToMod;
        modType = _modType;
    }
}

public class TriggerStatePayload {
    // Identify what StateSO this minor state belongs to
    public string group;
    public string source;
    public bool triggerOnEnemy;
    public bool triggerOnSendAction;
    public CHAR_STATE actToTriggerOn;
    public TriggerStatePayload() {}

    public TriggerStatePayload(CHAR_STATE _actToTriggerOn) {
        actToTriggerOn = _actToTriggerOn;
    }
}


[Serializable]
public class AttributeStateDelivery : AttributeStatePayload {
    
}

[Serializable]
public class BehaviourStateDelivery : BehaviourStatePayload {
    
}

[Serializable]
public class TriggerStateDelivery : TriggerStatePayload {
    
}


public enum COST_TYPE : int {
    FLAT,
    DRAIN,
}

public enum COST_STAT : int {
    HEALTH,
    STAMINA,
    FOCUS,
    WILL,
}

[Serializable]
public class CostPayload {
    public COST_TYPE type;
    public COST_STAT stat;
    public float value;
}

public class ImmediateResult {
    public ACT_TYPE actType;
    public StateSourceLogic source;
    public Dictionary<StateGroupLogic, List<AttributeStatePayload>> _AttribStateGroups;
    public Dictionary<StateGroupLogic, List<BehaviourStatePayload>> _BehaveStateGroups;
    public TriggerStatePayload triggerState;
    public List<CostPayload> costs;

    public ImmediateResult() {
        costs = new List<CostPayload>();
        _AttribStateGroups = new Dictionary<StateGroupLogic, List<AttributeStatePayload>>();
        _BehaveStateGroups = new Dictionary<StateGroupLogic, List<BehaviourStatePayload>>();
    }
}
