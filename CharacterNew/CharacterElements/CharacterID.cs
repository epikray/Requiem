using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterID", menuName = "Scriptable Objects/Character ID")]
public class CharacterID : ScriptableObject
{
    // Info

    // Stats
    public CharacterStats Stats;
    // Equipment
    
    public ArmorSO Armor;
    public WeaponSO Weapon;

    // Affinitites
    public CharacterAffinities Affinities;

    // List of Actions
    public List<AttackSO> Attacks;
    public List<SkillSO> Skills;
    public List<StanceSO> Stances;
    public List<StrategySO> Strategies;

    void Awake() {
        
    }

    public CharacterInstance Instantiate() {

        return new CharacterInstance(this);
    }
    
}

[Serializable]
public class CharacterInstance {
    [SerializeField]
    public Stats Stats;

    // Hopefully, just getting a reference of the equipment will be fine
    public ArmorSO Armor;
    public WeaponSO Weapon;
    public AffinitiesInstance Affinities;

    /* Should be simple references */
    public List<AttackSO> Attacks;
    public List<SkillSO> Skills;
    public List<StanceSO> Stances;
    public List<StrategySO> Strategies;

    public CharacterInstance(CharacterID ID) {
        // a referece to the ID could be stored here aswell, it would be a strictly readonly relationship tho.

        Stats = new Stats(ID.Stats);
        Affinities = new AffinitiesInstance(ID.Affinities);
        Weapon = ID.Weapon;
        Armor = ID.Armor;

        // Skills do not be seperately instanced from their SciptObjects, and so only need to be referenced
        // now that we are adding new skills to an instance. We want to 
        Attacks = new List<AttackSO>(ID.Attacks);
        Skills = new List<SkillSO>(ID.Skills);
        Stances = new List<StanceSO>(ID.Stances);
        Strategies = new List<StrategySO>(ID.Strategies);
    }

    public void UpdateMembers(float dt) {
        // If Stats shift dependent on time, do it here.
        // Is it resonable to place property calc here? Granted that the refactoring wouldnt be awful?
        Affinities.TickAll(dt);
    }

    public void InsertAction(CharActionSO action) {
        switch(action.type) {
            case ACT_TYPE.ATTACK :
                Attacks.Add(action as AttackSO);
                break;
            case ACT_TYPE.SKILL :   
                Skills.Add(action as SkillSO);
                break;
            case ACT_TYPE.STANCE :
                Stances.Add(action as StanceSO);
                break;
            case ACT_TYPE.STRATEGY :
                Strategies.Add(action as StrategySO);
                break;
        }
    }

    public void InsertItem(Item item) {

    }

    public void AddGrowth(float growth) {

    }
}

// How we handle character stats is still too inconsitent for my taste.
// I should try and do a full feature list to figure out how it should behave,
// and by extension how the Asset (CharacterStats ScriptableObject) and Instance (Stats class) should interact
[Serializable]
public class Stats {
    
    public Pool Health; 
    public Pool Stamina;
    public Pool Focus;
    public Pool Will;

    public Stat Strength;
    public Stat Dexterity;
    public Stat Intellect;
    public Stat Spirit;

    public Pool Rage;
    public Pool Grace;
    public Pool Attention;
    public Pool Awarness;

    public Stat Toughness;
    public Stat Agility;
    public Stat Resistance;
    public Stat Balance;

    public Stats(CharacterStats statsSO) {
        Health = new Pool(statsSO.Health);
        Stamina = new Pool(statsSO.Stamina);
        Focus = new Pool(statsSO.Focus);
        Will = new Pool(statsSO.Will);

        Strength = new Stat(statsSO.Strength);
        Dexterity = new Stat(statsSO.Dexterity);
        Intellect = new Stat(statsSO.Intellect);
        Spirit = new Stat(statsSO.Spirit);

        Rage = new Pool(statsSO.Rage);
        Grace = new Pool(statsSO.Grace);
        Attention = new Pool(statsSO.Attention);
        Awarness = new Pool(statsSO.Awarness);

        Toughness = new Stat(statsSO.Toughness);
        Agility = new Stat(statsSO.Agility);
        Resistance = new Stat(statsSO.Resistance);
        Balance = new Stat(statsSO.Balance);
    }
}