using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

// TODO: Solidify and make concrete the two stage calculation. We have a calc based on the ID, which is mostly static
//      and then we have based on CharDataManager, 

public enum ACT_SELECTION : int {
    NON,
    HIGHEST_DAMAGE,
    HIGHEST_ACCURACY,
    MOST_PAYLOADS,
}

public class ActionFunction
{
    private CharacterInstance cInstance;
    private PropertyCalculator pCalc;
    public bool debug = false;
    
    //public CharActionSO charAction;
    public CharacterInstance CInstance {
        get {
            return cInstance;
        }
    }
    
    public ActionFunction(CharacterInstance instance, PropertyCalculator calculator) {
        cInstance = instance; 
        pCalc = calculator; 
    }

    /*
        Calc ActionResults to send out from this character to the reciever
    */
    public ImpactResult CalcOut(CharActionSO action) {
        switch(action.type) {
            case ACT_TYPE.ATTACK :
                return CalcAttackImpact((AttackSO)action);
            case ACT_TYPE.SKILL :   
                return CalcSkillImpact((SkillSO)action);
            case ACT_TYPE.STANCE : 
                return null;
            case ACT_TYPE.STRATEGY :
                return null;
        }

        return null;
    }

    
    public ImpactResult CalcInEquipment(ImpactResult resultIn) {
        ImpactResult res = resultIn;

        
        foreach(ImpactPayload impact in res.impacts) {
            
            // Through Armor
            if(cInstance.Armor != null) {
                if(impact.dmgType == ACT_DMG.FORCE || 
                    impact.dmgType == ACT_DMG.BLUNT || 
                    impact.dmgType == ACT_DMG.SLASH || 
                    impact.dmgType == ACT_DMG.PIERCE) 
                {
                    impact.damage -= cInstance.Armor.forceRecuction;
                }

                if(impact.dmgType == ACT_DMG.BLUNT) {
                    impact.damage *= (100f - cInstance.Armor.bluntAbsorption)/100f;
                } else if(impact.dmgType == ACT_DMG.SLASH) {
                    impact.damage *= (100f - cInstance.Armor.slashAbsorption)/100f;
                } else if(impact.dmgType == ACT_DMG.PIERCE) {
                    impact.damage *= (100f - cInstance.Armor.pierceAbsorption)/100f;
                }
            }
        }

        return res;
    }

    /*
        Calcs ActionResults or ActionSOs to be applied to this character
    */
    // Checks defensive stats and stuff to reduce the scaled result
    // TODO: Resistance doesnt do anything. Implement in affliction somewhere; Resistance increases time it takes to deal damage, 
    // and minimizes debuffs.
    public ImpactResult CalcInImpact(ImpactResult resultIn) {

        ImpactResult res = resultIn;
        int[] toRemove = {0};
        
        // afflictions are double dipped by the defenses.
        // So lets make damage not from affliction the exception.
        // Only force (pure) go directly to health
        
        foreach(ImpactPayload impact in res.impacts) {
            

            // Through Stats
            impact.damage -= pCalc.GetStat(STAT.TOUGHNESS)/10f;
            impact.damage = Mathf.Max(impact.damage, 0f);

            // Agility could be integrated into the accuracy mod
            

            // agility as a divider or subtractor?
            // N accuracy againts N agility =>
            // 100 accuracy againts 50 agility => 100% to hit, 50% chance to crit
            // 100 accuracy againts 50 agility => 100% to glance, 50% chance to hit?
            // 100 accuracy against 200 agility => 100% to miss? 50% chance to glance
            (float hMax, float hCur) = pCalc.GetPool(STAT.HEALTH);
            float stunThreshold = pCalc.GetStat(STAT.BALANCE) * hCur / (hMax*10f);

            if(impact.calcHitType) {
                // TODO: Impact should be modified, since it is a reference to class, but make sure it is
                CalcHitType(impact, ref stunThreshold);
            } 
            // To Stun should be based on a combination of max hp and balance
            // roughly dmg > max_hp*(balance/10)/100 => balance = 50 -> over 5 percent, balance = 100 -> over 10 percent, balance = 200 -> 20 percent
            // But other things will affect stun threshold
            if(impact.damage > stunThreshold) {
                impact.stunType = ACT_STUN.NORMAL;
            } else {
                impact.stunType = ACT_STUN.NON;
            }
        }

        //Debug.Log(log);
        return res;
    }
    void CalcHitType(ImpactPayload payload, ref float stunThreshold) {
        float curAgility = pCalc.GetStat(STAT.AGILITY);
        float chanceToHit = 100f*(payload.accuracy/(curAgility + 2e-6f));
        // 100/200 = 0.5 => 50, crit => 50/100 - 1 = -0.5, hit => 50/100 = 0.5, 50/50 > 1
        // 100/100 = 1 => crit never, hit always, glance always => hit always
        // we need more than 200 agility to even have a chance to dodge a 100 accuracy attack.
        // Is this resonable, or does it simply show that 100 accuracy does mean something like always accuracte
        // Something more interesting then.
        // 90/100 = 0.9 => never crit, 90% chance to hit, glance always.

        // if accuracy > 2*agility, we always crit, do we add suppah crits lol?
        // if accuracy > agility, we have a chance to crit
        // If accuracy = agility, we always hit
        // if accuracy < agility, we have a chance to glance
        // if accuracy < 0.5*agility, we have a chance to miss, or awlays glance
        
        float random = Random.value; // rng is fine, giving us a value between 0 and 1
        bool crit = (chanceToHit/100f - 1f) > random ? true : false;
        bool hit = chanceToHit/100f > random  ? true : false;
        bool glance = chanceToHit/50f > random ? true : false;

        if(crit) {
            payload.hitType = ACT_HIT.CRIT;
            stunThreshold *= 2.0f;
        } else if (hit) {
            payload.hitType = ACT_HIT.HIT;
            
        } else if (glance) {
            payload.hitType = ACT_HIT.GLANCE;
            stunThreshold *= 0.5f;
        } else {
            payload.hitType = ACT_HIT.MISS;
            stunThreshold *= 0.0f;
        }
    }


    // Check whether action cost requirements are met by cInstance.
    public bool VerifyCostsAreMet(CharActionSO action) {

        float hCost = 0f;
        float sCost = 0f;
        float fCost = 0f;
        float wCost = 0f;
        foreach(CostPayload cost in action.costs) {
            switch(cost.stat) {
                case COST_STAT.HEALTH : 
                    hCost += cost.value;
                    break;
                case COST_STAT.STAMINA : 
                    sCost += cost.value;
                    break;
                case COST_STAT.FOCUS : 
                    fCost += cost.value;
                    break;
                case COST_STAT.WILL : 
                    wCost += cost.value;
                    break;
            }
        }
        if(hCost > pCalc.GetPool(STAT.HEALTH).Item1) return false;

        if(sCost > pCalc.GetPool(STAT.STAMINA).Item1) return false;

        if(fCost > pCalc.GetPool(STAT.FOCUS).Item1) return false;

        if(wCost > pCalc.GetPool(STAT.WILL).Item1) return false;
        
        return true;
    }

    // Costs and stat changes should be applied fairly immediately. But in special cases, we might want to delay it. 
    public ImmediateResult CalcInImmediates(CharActionSO action) {
        ImmediateResult res = new ImmediateResult
        {
            actType = action.type,
            costs = new List<CostPayload>(action.costs)
        };
        // If i want weight and foci coeff to do anything, do it hear
        switch (action.type) {
            case ACT_TYPE.ATTACK :
                if(cInstance.Weapon != null) {
                    foreach(CostPayload cost in res.costs) {
                        if(cost.stat == COST_STAT.STAMINA) {

                        } else if (cost.stat == COST_STAT.FOCUS) {

                        }
                    }
                }
                break;
            case ACT_TYPE.SKILL :
                if(cInstance.Weapon != null) {
                    foreach(CostPayload cost in res.costs) {
                        if(cost.stat == COST_STAT.STAMINA) {

                        } else if (cost.stat == COST_STAT.FOCUS) {
                            
                        }
                    }
                }
                break;
            case ACT_TYPE.STANCE : 
                CalcStanceResult((StanceSO)action, ref res);
                break;
            case ACT_TYPE.STRATEGY : 
                CalcStrategyResult((StrategySO)action, ref res);
                break;
        }
        
        // TODO: Make use of ACTION_COST, attribute to check that attribute does anything.
        foreach(CostPayload cost in res.costs) {
            cost.value *= pCalc.GetAttribute(ATTRIBUTE.ACTION_COST_ALL);
            if(cost.stat == COST_STAT.HEALTH)
                cost.value *= pCalc.GetAttribute(ATTRIBUTE.ACTION_COST_HEALTH);

            if(cost.stat == COST_STAT.STAMINA)
                cost.value *= pCalc.GetAttribute(ATTRIBUTE.ACTION_COST_STAMINA);

            if(cost.stat == COST_STAT.FOCUS)
                cost.value *= pCalc.GetAttribute(ATTRIBUTE.ACTION_COST_FOCUS);

            if(cost.stat == COST_STAT.WILL)
                cost.value *= pCalc.GetAttribute(ATTRIBUTE.ACTION_COST_WILL);
        }

        return res;
    }

    //Privates
    // TODO: The difference between Attack and Skill will be how they're 'applied' (what animations are used)
    //      And how the armament is used in the action.
    private ImpactResult CalcAttackImpact(AttackSO attack) {
        ImpactResult Ar = new ImpactResult{
            actType = attack.type    
        };
        
        foreach(ImpactDelivery delivery in attack.payloads) {
            ImpactPayload impact = new ImpactPayload
            {   
                dmgType = delivery.dmgType
            };
            
            // Through Equipment
            if (cInstance.Weapon != null) {
                impact.damage = delivery.damage + (delivery.damage / cInstance.Weapon.forceValue);
                impact.accuracy = delivery.accuracy * cInstance.Weapon.balance / 100f;
                switch(impact.dmgType) {
                    case ACT_DMG.BLUNT :
                        impact.damage *= cInstance.Weapon.bluntCoeff / 100f;
                        break;

                    case ACT_DMG.SLASH :
                        impact.damage *= cInstance.Weapon.slashCoeff / 100f;
                        break;

                    case ACT_DMG.PIERCE :
                        impact.damage *= cInstance.Weapon.pierceCoeff / 100f;
                        break;

                    default :
                        break;
                }
            } else { 
                // Character is using a limb to attack. No claws or anything.
                // This is hard to reason about... but, lets leave it for now.
                impact.damage = delivery.damage;
                impact.accuracy = delivery.accuracy;
            }

            // Through Stats
            switch(delivery.scaleType) {
                case ACT_SCALE.PHYSICAL :
                    impact.damage *= pCalc.GetStat(STAT.STRENGTH)/100f;
                    impact.accuracy *= pCalc.GetAttribute(ATTRIBUTE.ACCURACY_MODIFIER) * pCalc.GetStat(STAT.DEXTERITY)/100f;
                    break;
                case ACT_SCALE.MAGIC : 
                    impact.damage *= pCalc.GetStat(STAT.INTELLECT)/100f;
                    impact.accuracy *= pCalc.GetAttribute(ATTRIBUTE.ACCURACY_MODIFIER) * pCalc.GetStat(STAT.SPIRIT)/100f;
                    break;
                case ACT_SCALE.SPECIAL :
                    impact.damage *= 1f;
                    impact.accuracy *= 1f;
                    break;
            }

            Ar.impacts.Add(impact);
        }
        

        return Ar;
    }

    private ImpactResult CalcSkillImpact(SkillSO skill) {
        ImpactResult Ar = new ImpactResult
        {
            actType = skill.type    
        };

        foreach(ImpactDelivery delivery in skill.payloads) {
            ImpactPayload impact = new ImpactPayload
            {
                dmgType = delivery.dmgType
            };
            // Through Equipment
            if (cInstance.Weapon != null) {
                
                impact.damage = delivery.damage * (cInstance.Weapon.awe/100f);
                impact.accuracy = delivery.accuracy * (cInstance.Weapon.intricacy/100f);
            } else { 
                // Character is using a limb to attack. No claws or anything.
                // This is hard to reason about... but, lets leave it for now.
                impact.damage = delivery.damage;
                impact.accuracy = delivery.accuracy;
            }

            switch(delivery.scaleType) {
                case ACT_SCALE.PHYSICAL :
                    impact.damage *= pCalc.GetStat(STAT.STRENGTH)/100f;
                    impact.accuracy *= pCalc.GetStat(STAT.DEXTERITY)/100f;
                    break;
                case ACT_SCALE.MAGIC : 
                    impact.damage *= pCalc.GetStat(STAT.INTELLECT)/100f;
                    impact.accuracy *= pCalc.GetStat(STAT.SPIRIT)/100f;
                    break;
                case ACT_SCALE.SPECIAL :
                    impact.damage *= 1f;
                    impact.accuracy *= 1f;
                    break;
            }

            Ar.impacts.Add(impact);
        }

        return Ar;
    }

    private void CalcStanceResult(StanceSO stance, ref ImmediateResult res) {
        // The state delivery is nested. AttribStateGroups is a List<AttribStateGroupSO>
        // where each AttribStateGroupSO has a List<AttributeStateDelivery>

        // TODO: Change here? Change ImmediateResult to include Source and Group for the range
        // so that info is not lost when added? Its not lost but its tricky to know when a new application is added.
        // A single source can come from a result, but that source can have many state groups, which in themselves have many state changes.
        //res.source = new StateSource(stance);
        Dictionary<StateGroupLogic, List<AttributeStateDelivery>> attribGroups = stance.ExtractAttributeStateGroups();
        Debug.AssertFormat(stance.SourceCond != null, "Stance skill has a null StateSourceLogic reference");
        
        res.source = stance.SourceCond;

        foreach(KeyValuePair<StateGroupLogic, List<AttributeStateDelivery>> kvp in attribGroups) {
            // Kinda lame that I cant add the value directly due to the conversion from Delivery to Payload.
            // But it just points to the fact that its a needless abstraction level.
            Debug.AssertFormat(kvp.Key.groupSO != null, "StateGroupLogic has a null reference to its group SO");
            res._AttribStateGroups.Add(kvp.Key, new List<AttributeStatePayload>());
            res._AttribStateGroups[kvp.Key].AddRange(kvp.Value);
        }
        
        Dictionary<StateGroupLogic, List<BehaviourStateDelivery>> behaveGroups = stance.ExtractAndSourceBehaviourStates();

        foreach(KeyValuePair<StateGroupLogic, List<BehaviourStateDelivery>> kvp in behaveGroups) {
            // Kinda lame that I cant add the value directly due to the conversion from Delivery to Payload.
            // But it just points to the fact that its a needless abstraction level.
            Debug.AssertFormat(kvp.Key.groupSO != null, "StateGroupLogic has a null reference to its group SO");
            res._BehaveStateGroups.Add(kvp.Key, new List<BehaviourStatePayload>());
            res._BehaveStateGroups[kvp.Key].AddRange(kvp.Value);
        }
    }

    private void CalcStrategyResult(StrategySO strategy, ref ImmediateResult res) {
        res.triggerState = strategy.triggerState;
    }



}

public class DynamicActionFunction {

    private CharacterInstance cInstance;
    private PropertyCalculator pCalc;
    private CharacterStates cStates;
    public DynamicActionFunction() {

    }
    public void InjectMembers(CharacterInstance _cInstance, PropertyCalculator _pCalc, CharacterStates _cStates) {
        if(cInstance == null) {
            cInstance = _cInstance;
        }
        if(pCalc == null) {
            pCalc = _pCalc;
        }
        if(cStates == null) {
            cStates = _cStates;
        }
    }

    // Find all relevant states
    public ImpactResult OnRecieveAction(ImpactResult IR) {
        var states = cStates.GetAffectingBehaviourStates(CHAR_EVENT.ON_RECIEVEACTION);

        if(states.Count == 0) {
            return IR;
        }

        ImpactResult newIR = IR;
        foreach(BehaviourStatePayload state in states) {
            CallFunction_RecieveAction(state, ref newIR);
        }

        return newIR;
    }

    private void CallFunction_RecieveAction(BehaviourStatePayload state, ref ImpactResult IR) {
        switch(state.behaviour) {
            case ACTION_FUNC_ID.BLOCK : 
                RecieveAction_Block(ref IR);
            break;
            case ACTION_FUNC_ID.EVADE : 
                RecieveAction_Evade(ref IR);
            break;
            default : 
                Debug.LogError("Invalid ACTION_FUNC_ID, no valid dynamic action function to call.");

            break;
        }
    }

    // Mitigates N amount of Physical payloads
    private void RecieveAction_Block(ref ImpactResult IR) {
        foreach(ImpactPayload impact in IR.impacts) {
            // all physicals
            if((int)impact.dmgType > 4) {
                continue;
            }

            float v = impact.damage;
            v -= (pCalc.GetStat(STAT.TOUGHNESS) + pCalc.GetStat(STAT.STRENGTH))/10f;
            v = Mathf.Max(0f, v);
            impact.damage = v;
            // block is broken after one hit?
            break;
        }
    }

    // Mitigates N amount of Physical payloads
    private void RecieveAction_Evade(ref ImpactResult IR) {
        foreach(ImpactPayload impact in IR.impacts) {
            if((int)impact.dmgType > 4 && (int)impact.dmgType == 0) {
                continue;
            }
            impact.hitType = ACT_HIT.GLANCE;
            
            break;
        }
    }
    
}



// Main responsibility is to help to get an action give search criteria.
// For PCChar, this simply means being an plug for UI events, or inputs to select actions.
// For NPCChar, this can be used for querrying for the right action the AI wants to use.

public class ActionSelector {

    private Dictionary<ACT_TYPE, ActionSelection> selections;
    private CharActionSO submittedAction;
    private ACT_TYPE type;
    public bool debug = false;
    public ACT_TYPE actionType {
        get {
            return type;
        }
    }
    private int index;

    public ActionSelector(CharacterInstance _cInstance) {
        selections = new Dictionary<ACT_TYPE, ActionSelection>();
        selections.Add(
            ACT_TYPE.ATTACK, 
            new ActionSelection(_cInstance, (IReadOnlyList<CharActionSO>)_cInstance.Attacks, ACT_TYPE.ATTACK)
        );

        selections.Add(
            ACT_TYPE.SKILL, 
            new ActionSelection(_cInstance, (IReadOnlyList<CharActionSO>)_cInstance.Skills, ACT_TYPE.SKILL)
        );

        selections.Add(
            ACT_TYPE.STANCE, 
            new ActionSelection(_cInstance, (IReadOnlyList<CharActionSO>)_cInstance.Stances, ACT_TYPE.STANCE)
        );

        selections.Add(
            ACT_TYPE.STRATEGY, 
            new ActionSelection(_cInstance, (IReadOnlyList<CharActionSO>)_cInstance.Strategies, ACT_TYPE.STRATEGY)
        );

    
        submittedAction = null;
        type = ACT_TYPE.NON;
    }

    // just make it a prop? why not both? why not have 9999 ways to set a member.
    // check that there are is non empty actionList before changing type
    public void SetActionType(ACT_TYPE t) {
        if(t == ACT_TYPE.NON) {
            type = t;
        }
        
        if(selections[t].HasActions()) type = t;
    }
    public ACT_TYPE GetActionType() {
        return type;
    }

    public void SelectAt(int i) {
        if(type == ACT_TYPE.NON) {
            return;
        }

        index = selections[type].Select(i);
    }

    public void SelectBy(ACT_SELECTION criteria) {
        if(type == ACT_TYPE.NON) {
            return;
        }
        
        index = selections[type].SelectBy(criteria);
    }

    public bool ActionRequiresTarget() {
        bool res = false;
        if (type == ACT_TYPE.ATTACK || type == ACT_TYPE.SKILL) {
            res = true;
        }
        //Debug.Log("Action Requires Target? " + res);
        return res;
    }

    public bool HasSelection() {
        if(submittedAction == null) {
            return false;
        }

        return true;
    }

    public void CancelSelection() {
        Debug.Log(submittedAction.name + " was cancelled!");
        submittedAction = null;
    }

    public void PrintAction() {
        if(type == ACT_TYPE.NON) {
            return;
        }

        selections[type].PrintAction(index);
    }   

    public bool SubmitAction() {
        if(type == ACT_TYPE.NON) {
            return false;
        }

        submittedAction = selections[type].Get(index);
        Debug.Log(submittedAction.name + " was submitted!"); 
        return true;
    }

    // TODO: Make two funcs for nullSubmition equals true or false. I get confused otherwise.
    public CharActionSO GetSubmittedAction(bool nullSubmition) {
        if(type == ACT_TYPE.NON) {
            return null;
        }
        CharActionSO toSend = submittedAction;
        if(nullSubmition)
            submittedAction = null;

        return toSend;
    }

    public string ActionInfo() {
        if(type == ACT_TYPE.NON)
            return "";
        CharActionSO a = selections[type].Get(index);
        string info = "" + a.name + "\n" + a.description;
        return info;
    }


    private class ActionSelection {
        private IReadOnlyList<CharActionSO> actionList;
        private ACT_TYPE type;
        
        public ActionSelection(CharacterInstance _cInstance, IReadOnlyList<CharActionSO> _list, ACT_TYPE _type) {
            type = _type;
            actionList = _list;
        }

        //Clamp between actionEnumer.Count
        public int Select(int i) {
            int _i = (int)Math.Max(Math.Min(i, actionList.Count - 1), 0);
            return _i;
        }

        public int SelectBy(ACT_SELECTION criteria) {
            
            int candidate = 0;
            float comparative = 0;

            int counter = 0;

            // Never nesters seething
            switch(criteria) {
                case ACT_SELECTION.HIGHEST_DAMAGE : 
                    foreach(CharActionSO action in actionList) {
                        
                        float v = action.GetTotalDamage();
                        if(comparative < v) {
                            comparative = v;
                            candidate = counter;
                        }
                        counter += 1;
                    }
                    break;
                case ACT_SELECTION.HIGHEST_ACCURACY :
                    foreach(CharActionSO action in actionList) {
                        // Were lieing with this one. Were searching for the highest effective damage.
                        float v = action.GetDamageAdjustedByAccuracy();
                        if(comparative < v) {
                            comparative = v;
                            candidate = counter;
                        }
                        counter += 1;
                    }
                    break;
                case ACT_SELECTION.MOST_PAYLOADS :
                    foreach(CharActionSO action in actionList) {
                        
                        float v = action.GetImpactListCount();
                        if(comparative < v) {
                            comparative = v;
                            candidate = counter;
                        }
                        counter += 1;
                    }
                    break;
                case ACT_SELECTION.NON : 
                    foreach(CharActionSO action in actionList) {
                        
                    }
                    break;
            }  

            return candidate;
        }

        public CharActionSO Get(int i) {
            return actionList[i];
        }


        // search by name


        // query by attributes; TYPE, SCALING, DMG, EFFECTS

        public bool HasActions() {
            if(actionList.Count > 0) return true;

            return false;
        }

        public void PrintAction(int i) {
            if(actionList.Count < 1)
                return;
            
            string log = "";
            log += type + " " + actionList[i].name + ":\n";
            log += "    " + actionList[i].description + "\n";
            Debug.Log(log);
        }
    }
    
}   




