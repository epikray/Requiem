using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/*
    Stance Feature List: 
        Is applied to the character, in which certain stats/attributes are changed or added 
        aswell as a behaviour is added or changed to the character.
        Ex. The character can go into a 'Dodge' stance were their agility is increased at the decrease of their prepairedness to attack
            is reduced.
            The character 'Engages' with an enemy, meaning he places himself in combat range of another, doing a certain action
            on repeat until the state is cancelled.

        From the two examples of things I listed above, I think I need a system of AttibuteStates and BehaviourStates (names free to change)
        We can simply call it a State system.
        Each character has a set of States that they are in. Each State can either modify an attribute, such as Strength being doubled when used or something,
        or the state adds a behaviour that is factored into a characters base BehaveBattle.

        This set of States needs to have a structure so that we can: 
            keep track of which States belong together (are from the same source),
            maintain the lifetime of the State
            ease of adding and removing states from the same source.

        To make States having some function on the stats/attributes, its propably smart to have a singular class that can Get the 'current'
        value of a stat or attribute. 
        To be clear, a 'Stat' in my mind is some defining number of an Object (character skill weapon) 
        that is dependent only on the value itself and whatever states change it.
        While an 'Attribute' can be derived from usually more than one 'Stat'.

        In hindsight, State is NOT the right word, it implies there can only be one.
        Modifier is better: AttributeModifier, BehaviourModifier

*/

[CreateAssetMenu (fileName = "StanceSO",  menuName = "Scriptable Objects/Action/Stance")]
public class StanceSO : CharActionSO
{   
    // an SO is not unique! and should therefore be agnostic to its source
    // group can be pre init however.
    [SerializeField]
    public StateSourceLogic SourceCond;
    [SerializeField]
    public List<AttribStateGroupSO> AttribStateGroups;
    [SerializeField]
    public List<BehaveStateGroupSO> BehaveStateGroups;

    void OnEnable() {
        _type = ACT_TYPE.STANCE;
        if(SourceCond != null)
            SourceCond.sourceName = name;
        
    }
    public Dictionary<StateGroupLogic, List<AttributeStateDelivery>> ExtractAttributeStateGroups() {
        Dictionary<StateGroupLogic, List<AttributeStateDelivery>> res = new Dictionary<StateGroupLogic, List<AttributeStateDelivery>>();

        foreach(AttribStateGroupSO attribGroup in AttribStateGroups) {
            res.Add(attribGroup.ExtractConditionals(), attribGroup.ExtractStates());
        }
        return res;
    }

    public Dictionary<StateGroupLogic, List<BehaviourStateDelivery>> ExtractAndSourceBehaviourStates() {

        Dictionary<StateGroupLogic, List<BehaviourStateDelivery>> res = new Dictionary<StateGroupLogic, List<BehaviourStateDelivery>>();

        foreach(BehaveStateGroupSO behaveGroup in BehaveStateGroups) {
            res.Add(behaveGroup.ExtractConditionals(), behaveGroup.ExtractStates());
        }
        return res;
    }

    public override float GetAverageAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override float GetAverageDamage()
    {
        throw new System.NotImplementedException();
    }

    public override float GetDamageAdjustedByAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override int GetImpactListCount()
    {
        throw new System.NotImplementedException();
    }

    public override float GetTotalAccuracy()
    {
        throw new System.NotImplementedException();
    }

    public override float GetTotalDamage()
    {
        throw new System.NotImplementedException();
    }
}