using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using TMPro;
/*
    As the name implies. Its seperating out the responsibility of the details of afflictions from DataManager.
    DataManger will end up in the future being a supervisor of other Managers.

    But AfflictionManager maintains and process all Afflictions on a Character.
    Ticking them, applying them, and dealing the necessary side effects.
*/

public enum ACT_AFLCT : int {
    NON,
    FORCE,
    TRAUMA,
    BLEED,
    PUNCT,
    BURN,
    FREEZE,
    DRENCH,
    SHOCK,
}

public abstract class Affliction {
    //protected float val = 0;
    protected float duration = 0;
    protected class Stack {
        public float val = 0;
        public float dur = 0;
        public Stack(float _val = 0, float _dur = 0) {
            val = _val;
            dur = _dur;
        }
    }

    protected List<Stack> stacks;

    public Affliction() {
        stacks = new List<Stack>();
    }
    public float value {
        get {
            float tot_value = 0f;
            
            foreach(Stack s in stacks) {
                tot_value += s.val*(s.dur/duration);
            }
            //Debug.Log("Affliction value " + tot_value);
            return tot_value;
        }
    }

    public virtual void AddStack(float val) {
        stacks.Add(new Stack(val, duration));
    }

    protected ACT_AFLCT aflct;
    // DoAfflict is static things 
    public abstract void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props);

    // Given val and deltaTime, how should ID be effected
    public abstract float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props);

        
}
// We could also init the class with a CharacterInstance.
// Cross-Affliction communication is needed for more interesting behaviour.
// Messaging?
// Affliction can produce state changes

// TODO: Damage is slightly overshot of what is demanded.
//       When val of 65 ought to result in 65 final damage.
public class AfflictionManager {

    // Most phys damage deals damage over a period.
    // trauma being very fast, bleed being moderately fast, puncture(pierce) being slow. Force is instant damage
    // all 'elemental' damage is pure affliction damage 
    // their corresponding affliction (status effect) deals damage
    // Except for drench, which increases the affliction rate of other elementals.
    // mentals don't really have an affliction... the Trance state is their affliction
    // The cosmics are fun... what could Chaos/Order affliction be..?

    public static ACT_AFLCT DMGtoAFLCT(ACT_DMG d) {
        switch(d) {
            // Physicals
            case ACT_DMG.FORCE : 
                return ACT_AFLCT.FORCE;
            case ACT_DMG.BLUNT : 
                return ACT_AFLCT.TRAUMA;
            case ACT_DMG.SLASH : 
                return ACT_AFLCT.BLEED;
            case ACT_DMG.PIERCE : 
                return ACT_AFLCT.PUNCT;

            case ACT_DMG.HEAT : 
                return ACT_AFLCT.BURN;
            case ACT_DMG.COLD : 
                return ACT_AFLCT.FREEZE;
            case ACT_DMG.DOUSE : 
                return ACT_AFLCT.DRENCH;
            case ACT_DMG.CHARGE : 
                return ACT_AFLCT.SHOCK;
        }

        return ACT_AFLCT.NON;
    }

    Dictionary<ACT_AFLCT, Affliction> afflictionDict;
    CharacterInstance cInstance;
    CharacterStates cStates;
    PropertyCalculator cProperties;

    float count = 0f;
    float count_tot_dmg = 0f;

    public Affliction this[ACT_AFLCT a] {
        get { return afflictionDict[a]; }
    }

    public AfflictionManager(CharacterInstance _cInstance, CharacterStates _cStates, PropertyCalculator _cProperties) {
        
        cInstance = _cInstance;
        cStates = _cStates;
        cProperties = _cProperties;

        afflictionDict = new Dictionary<ACT_AFLCT, Affliction> {
            { ACT_AFLCT.NON, new NonAffliction() },
            { ACT_AFLCT.FORCE, new ForceAffliction() },
            { ACT_AFLCT.TRAUMA, new TraumaAffliction() },
            { ACT_AFLCT.BLEED, new BleedAffliction() },
            { ACT_AFLCT.PUNCT, new PunctureAffliction() },
            { ACT_AFLCT.BURN, new BurnAffliction() },
            { ACT_AFLCT.FREEZE, new FreezeAffliction() },
            { ACT_AFLCT.DRENCH, new DrenchAffliction() }, // TODO
            { ACT_AFLCT.SHOCK, new ShockAffliction() } // TODO
        };
        // so on so forth
    }

    public void ResolveAllAfflictions(float dt, string name) {
        float tot_dmg = 0f;
        
        count += Time.deltaTime;
        foreach (KeyValuePair<ACT_AFLCT, Affliction> entry in afflictionDict) {
            tot_dmg += entry.Value.Tick(dt, cInstance, cStates, cProperties);
            entry.Value.DoAfflict(cInstance, cStates, cProperties);
            // Cross affliction dependence???
            // DoAfflict grabs a ref to prop calculator, and the calculator should use afflictions.
        }
        count_tot_dmg += tot_dmg;

        if(count > 1f && count_tot_dmg > 0f) {
            //Debug.Log(name + " received " + count_tot_dmg + " damage in the last second");
            count_tot_dmg = 0f;
            count = 0f;
        }
    }

    class NonAffliction : Affliction {
        public NonAffliction() {
            aflct = ACT_AFLCT.NON;
            duration = 0;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            return 0f;
        }
    }
    /*
        TODO: Unify Force, Traum and Bleed, Puncture into two afflictions. Trauma and Bleed.
        Force Trauma and Blunt trauma deal *the same* damage. but they have to go through slightly different defences.
        Same with slash bleed and pierce bleed.
        Force dmg is instant, blunt takes 1 sec to deal. if not 0.5
        Slash takes 3 sec to bleed, pierce takes 6.

        because of the large difference in time to tick, Bleed is significantly weaker than Trauma,
        if 1 slash/pierce equals 1 bleed and 1 force/blunt equals 1 trauma. So lets make that 1 damage of each doesnt directly convert to'
        one of the other.

        1 Force = 1 force > 1 blunt > 1 slash < 1 pierce

        force is not instant, just really quick

        conversion from one dmg to another, based on duration:
        dur_A, dur_B
        val_A, val_B
        A is faster than B
        val_A = val_B * (dur_B / dur_A) * 0.5


    */

    // TODO: combine Force with Trauma, and Bleed with Puncture.
    //       smarter api is needed to grant modded stacks. Force -> 0.5 Trauma, Blunt -> 1.0 Trauma
    class ForceAffliction : Affliction {
        float forceCoeff = 1f;
        public ForceAffliction() {
            aflct = ACT_AFLCT.FORCE;
            duration = 0.5f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // Trauma affliction
            // Slow action charge speed for the duration of the affliction?
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else {
                    hit += stack.val/duration*dt; // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    class TraumaAffliction : Affliction {
        public TraumaAffliction() {
            aflct = ACT_AFLCT.TRAUMA;
            duration = 1.0f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // Trauma affliction
            // Slow action charge speed for the duration of the affliction?
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else {
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);; // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    class BleedAffliction : Affliction {
        float bleedCoeff = 1;
        public BleedAffliction() {
            aflct = ACT_AFLCT.BLEED;
            duration = 3f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // bleed affliction
            // Slow action charge speed for the duration of the affliction?
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else {
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION); // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    class PunctureAffliction : Affliction {
        float punctureCoeff = 0.5f;
        public PunctureAffliction() {
            aflct = ACT_AFLCT.PUNCT;
            duration = 6f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else {
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION); // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    /*  
        Elemental afflictions:
            They all work according to a threshold, in which their effects are triggered.
              weakens all stats ***
            I like the idea that elementals work to 'weaken' the enemy
            burn -> lowers toughness

            Burn: slow regen
            Freeze: Slow action speed
            Drench: Increase affliction dmg accreument
            Charge: Sap action charge
    */
    class BurnAffliction : Affliction {
        float burnCoeff = 2;
        bool burning;
        public BurnAffliction() {
            aflct = ACT_AFLCT.BURN;
            duration = 20f;
        }

        // The stack system breaks how we do burn and freeze.
        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {   
            /*
            // But does it?
            float val = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                val += stack.val/duration*stack.dur; //stack.val/duration = dps, dps*dur = total dmg
                i++;
            }
            
            // base on hp?
            if(val > 50) {
                //burn state
                burning = true;
            } else if (val < 0.001) {
                burning = false;
            }
            */
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) { 
                    hit += burnCoeff*stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else  {
                    hit += burnCoeff*stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION); // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    // TODO: fine tune freeze mechanic
    /*
        elemental affliction that effects the speed of characters actions
        freeze needs to mimic the effects of dexterity in a negative way
            *less chance to dodge
            *less chance to hit
            *more time needed before making another action
    */
    class FreezeAffliction : Affliction {
        float freezeCoeff = 1;
        // using States.attributes["Frozen"] didnt work, got null ref. Race condition?
        AttribStateGroupSO freezeState;
        bool freeze;

        //AttributeState

        public FreezeAffliction() {
            aflct = ACT_AFLCT.FREEZE;
            duration = 20f;
            // TODO: YAAAAWN, this is such utter cringe, and its gonna drive me insane. 
            freezeState = Resources.Load<AttribStateGroupSO>("ScriptObjects/States/Frozen");
            //Debug.Log("Found freezeState " + freezeState.name);            
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            /*
            float val = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                val += stack.val/duration*stack.dur;
                i++;
            }

            if(val > 50) {
                //freeze state
                //cStates.AddAttributeStates(freezeState.attributeStates);
                freeze = true;
            } else if (val < 0.001) {
                //cStates.RemoveStatesByGroup("Frozen");
                freeze = false;
            }
            */
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {    
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else  {    
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION); // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    // TODO: Implement drench
    /*
        The core idea is that the total val of all drench stacks increases effectivness of everything else.
        drench needs to mimic the effects of spirit in a negative way. which is hard right now when spirit and intellect is underdeveloped.
        spirit is sadly at the moment magical dex. when probably i want it to be more like magical strength
            *less chance to dodge
            *less chance to hit
            *more time needed before making another action
    */
    class DrenchAffliction : Affliction {
        bool drenched = false;
        public DrenchAffliction() {
            aflct = ACT_AFLCT.DRENCH;
            duration = 20f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            /*
            float val = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                val += stack.val/duration*stack.dur;
                i++;
            }

            if(val > 50) {
                //freeze state
                //cStates.AddAttributeStates(freezeState.attributeStates);
                drenched = true;
            } else if (val < 0.001) {
                //cStates.RemoveStatesByGroup("Frozen");
                drenched = false;
            }
            */
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            // This is ingeneral, correct.
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else  {
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION); // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }

    class ShockAffliction : Affliction {
        bool discharge = false;
        public ShockAffliction() {
            aflct = ACT_AFLCT.SHOCK;
            duration = 20f;
        }

        public override void DoAfflict(CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            /*
            float val = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                val += stack.val/duration*stack.dur;
                i++;
            }

            if(val > 50) {
                discharge = true;
            } else if (val < 0.001) {
                discharge = false;
            }
            */
        }

        public override float Tick(float dt, CharacterInstance _cInstance, CharacterStates cStates, PropertyCalculator props)
        {
            float hit = 0f;
            int i = 0;
            while(i < stacks.Count) {
                Stack stack = stacks[i];
                stack.dur -= dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);
                if(stack.dur < 0f) {
                    hit += stack.val/duration*(dt+stack.dur); // Note: Last bit of damage afforded. stack.dur is negative.
                    stacks.RemoveAt(i);
                } else  {
                    hit += stack.val/duration*dt*props.GetAttribute(ATTRIBUTE.AFFLICTION_SPEED_REDUCTION);; // Note: duration works as the coefficient. Balancing dmg per tick and duration
                }
                i++;
            }

            if(hit > 0.0001) {
                props.HurtPool(STAT.HEALTH, hit);
            }
            return hit;
        }
    }
}