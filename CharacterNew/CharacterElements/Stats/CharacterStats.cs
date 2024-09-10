using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum STAT : int {
    HEALTH,
    STAMINA,
    FOCUS,
    WILL,

    STRENGTH,
    DEXTERITY,
    INTELLECT,
    SPIRIT,

    RAGE,
    GRACE,
    ATTENTION,
    AWARNESS,

    TOUGHNESS,
    AGILITY,
    RESISTANCE,
    BALANCE,
}
// TODO: Clean up goal, use Attributes for all 'behaviour effecting' values, so stats and secondary stats.
public enum ATTRIBUTE : int {
    HEALTH,
    STAMINA,
    FOCUS,
    WILL,

    STRENGTH,
    DEXTERITY,
    INTELLECT,
    SPIRIT,

    RAGE,
    GRACE,
    ATTENTION,
    AWARNESS,

    TOUGHNESS,
    AGILITY,
    RESISTANCE,
    BALANCE,

    ACTION_CHARGE_SPEED,
    ACTION_TIME_SPEED,

    ARMOR,
    ARMOR_BLUNT,
    ARMOR_SLASH,
    ARMOR_PIERCE,
    AFFINITY,
    
    REGEN_SPEED,

    ACCURACY_MODIFIER,

    ACTION_COST_ALL,
    ACTION_COST_HEALTH,
    ACTION_COST_STAMINA,
    ACTION_COST_FOCUS,
    ACTION_COST_WILL,

    AFFLICTION_SPEED_REDUCTION,
    AFFLICTION_ACCRUEMENT,
}


[System.Serializable]
public class Stat {
    [SerializeField]
    protected float valBase;

    public float baseValue {
        get {
            return valBase;
        }
    }
    protected float _val;

    public float val {
        get {
            return _val;
        }
    }

    public Stat() {
    }

    public Stat(float value) {
        valBase = value;
        _val = valBase;
    }

    public Stat(Stat _in) {
        valBase = _in.valBase;
        _val = _in.valBase;
    }
    public virtual void Print() {
        string log = "type : " + this.GetType() + "\n" +
        "valBase : " + valBase + "_val : " + _val + ", val : " + val;
        Debug.Log(log);
    }

    public void Validate() {
        _val = valBase;
    }
}

[System.Serializable]
public class Pool : Stat {
    protected float _max;

    public float max {
        get {
            return _max;
        }
    }
    public Pool() : base() {
        _max = valBase;
    }
    public Pool(float value) : base(value) {
        _max = value;
    }

    public Pool(Pool _in) : base(_in) {
        _max = _in._max;
    }

    public void ModifyVal(float value) {
        _val = _val + value;
    }

    public void ModifyMax(float value) {
        _max = _max + value;
    }

    public override void Print() {
        string log = "type : " + this.GetType() + "\n" +
        "valBase : " + valBase + ", _val : " + _val + ", val : " + val + ", _max : " + _max;
        Debug.Log(log);
    }

    public void Validate() {
        _max = valBase;
    }
}

/* --- STAT AUXILARIES ---  */
public enum StatModType : int {
    Flat,
    Percent,
    Multiply,
}

public class StatModifier {
    public readonly float val;
    public readonly StatModType type;
    public readonly int priority;

    public StatModifier(float value, StatModType _type, int _priority) {
        val = value;
        type = _type;
        priority = _priority;
    }

    public StatModifier(float value, StatModType _type) : this(value, _type, (int)_type) { }

    public static int CompareModifierPriority(StatModifier a, StatModifier b) {
        if (a.priority > b.priority)
            return 1;
        else if (a.priority < b.priority)
            return -1;
        
        return 0;
    }
}

[CreateAssetMenu(fileName = "CharacterStats", menuName = "Scriptable Objects/Character Stats")]
public class CharacterStats : ScriptableObject
{
    
    // The new holy Growth stat. The concept to replace level ups.
    // Will be defined in another SO

    // Stats, o stats.

    // With the new structure of stats, huge props to : 
    // https://www.youtube.com/watch?v=SH25f3cXBVc&list=PLm7W8dbdflojT-OqfBJvqK6L9LRwKmymz&index=1
    // We can have our cake and eat it too.
    // Make the Stat classes private. -> have a float to determine pre growth bases. 
    // make new Stats with the post growth bases. 
    // CharacterStats keeps track of 

    /* 
        On Health and taking damage: It's apparent that theres two layers effecting how much damage something can take before it breaks.
        Size and robustness. 
    */

    /*
        To literate ( making up a word :) ) again the philosophy of the game and character elements
        The stats and damage categorised into 4 'colors' to show relationship between eachother.
        Red, Green, Yellow, Blue

        Duals and compliments... Two axis of reality, but all on a single grid, not seperate.
        Red, yellow aswell as green, blue are duals of eachother, achieving similar goals but in a different spaces.
        Red, green aswell as yellow, blue are compliments of eachother, while at odds theyre greatest effects happen when cycled between eachother. 

        So what lies in Red, Green, Yellow, Blue

                                                                    Damage types
        Red :    Health,  Strength,  Rage,      Toughness,  {Force,  Fire,      Fear,      Life }, Mechanics (physical)     
        Green :  Stamina, Dexterity, Grace,     Agility,    {Blunt,  Ice,       Lust,      Death}, Energetics (elemental)   

        Yellow : Will,    Spirit,    Awarness,  Balance,    {Slash,  Water,     Mania,     Order}, Spirtuality (emotional)  
        Blue :   Focus,   Intellect, Attention, Resistance, {Pierce, Lightning, Confusion, Chaos}, Logicality (metaphysical)

        the rows are categories and the columns are domains. So example of how to design out of these relationships

        So how does each damage type relate to eachother in accordance to the RBGY graph
        R Physical {                    
            Force <-> Blunt (Trauma Affliction)     RG
              ^         ^
              |         |
              v         v
            Slash <-> Pierce (Bleed Affliction)     BY
           (faster)  (slower)
              RY        GB
        }
              ^            ^   
              | Damages hp |   
              v            v

        G Elemental {
            Fire  <->  Ice (Affects combat capabilities)   RG
              ^         ^
              |         |
              v         v
            Oil   <->  Charge (Affects magic capabilities)     BY
           (Per tick)  (Thresholded)
           (Per tick)  (Thresholded)
              RY        GB
        }
            Fire stops / decreases regen.

        
        Y Psychic {

        }

        B Metaphysical {

        }
    */

    [Header("Physical")]
    [SerializeField]
    [Tooltip("The amount of damage a character can take. \nConsider it representative of the size and all in all robustness of a character.")]
    public Pool Health; 
    [Tooltip("How much the character can attack.")]
    public Pool Stamina;
    [Tooltip("The amount of metaphysical strain a character can handle, including giving orders.")]
    public Pool Will;
    [Tooltip("How many complex actions can the character can do.")]
    public Pool Focus;
    

    [Header("Offensive")]
    [SerializeField]
    [Tooltip("The peak of physical capabilites.")]
    public Stat Strength;
    [Tooltip("The integration of all physical capabilites.")]
    public Stat Dexterity;
    [Tooltip("The peak of magical/theoretical capabilites.")]
    public Stat Spirit;
    [Tooltip("The integration of all magical/theoretical capabilites.")]
    public Stat Intellect;
    

    [Header("Mental")]
    [SerializeField]
    public Pool Rage;
    public Pool Grace;
    public Pool Awarness;
    public Pool Attention;
   

    [Header("Defensive")]
    [SerializeField]
    [Tooltip("The ability to withstand impacts.")]
    public Stat Toughness;
    [Tooltip("The ability to evade impacts.")]
    public Stat Agility;
     [Tooltip("The ability to maintain posture against impacts.")]
    public Stat Balance;
    [Tooltip("The ability to mitigate effects of impacts.")]
    public Stat Resistance;
   
    
    //List<IStat> _stats;
    // Some stats are dependant on others. If Growth is defined to change strength at growth => 2
    // then base should change to reflect that.
    // We have a Stat.base now that stats should always be reset to at the end of a Play
    // session. Question is, if Growth is defined, should base be set to that value? 
    // No, that will cause problems. What is needed is a "base after growth".
    // It causes problems if we attempt to Grow the character in play mode.
    // Lets be safe and code such that the game works in both play mode and build.
    // This will require however a 'second' base. One for pre growth, and one for post growth
    void OnAwake() {
        
        

    }

    void OnEnable() {
        
    }

    void OnDisable() {
        
    }

    void OnValidate() {
          
        PokeStats();
    }

    void PokeStats() {
        
        Health.Validate();
        Stamina.Validate();
        Focus.Validate();
        Will.Validate();

        Strength.Validate();
        Dexterity.Validate();
        Intellect.Validate();
        Spirit.Validate();

        Rage.Validate();
        Grace.Validate();
        Attention.Validate();
        Awarness.Validate();

        Toughness.Validate();
        Agility.Validate();
        Resistance.Validate();
        Balance.Validate();
    }
}
