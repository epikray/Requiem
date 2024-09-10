using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEditor.VersionControl;
using Unity.VisualScripting;
//using UnityEngine.Events;

// As the name describes. It will manage the data of a character.
// This will be the primary component to communicate changes from and to SOs and the instantiated object
// Ideally, this will be the ONLY communication route

// TODO: Rename to CHAR_STATE, remove charging and charged in exchange for idle
public enum CHAR_STATE : int {
    NON,
    IDLE, // Not ready to attack
    READY, // Ready to attack
    PREACTION,
    INACTION,
    POSTACTION,
}

public enum ARGE_STATE : int {
    NON,
    IDLE,
    CHARGING, 
    CHARGED,
    CONSUMED,
}

public enum LIFE_STATE : int {
    ALIVE,
    UNCONSCIOUS,
    DEAD,
}


// NOTE: The distinction between 'target' and 'confirmed  target' is that the latter is what the manager simply is 'hovering over'
//       while the confirmed target is who should any action done by the managers owner should direct towards.
//       This is so the player can observer other chars without changing who the player wishes to attacks
public class TargetingManager {
    Behaviour owner;
    Behaviour target;

    RuntimeSet mySet;
    RuntimeSet opposingSet;
    RuntimeSet activeSet;
    int ind;

    public int targetIndex {
        get {
            return ind;
        }
    }

    

    public TargetingManager(Behaviour _owner, RuntimeSet _opposingSet = null) {
        ind = 0;
        owner = _owner;
        mySet = owner.mySet;
        opposingSet = _opposingSet;
        activeSet = null;
    } 
    /*
        Can be null, meaning character is not targetting anything
    */
    public Behaviour GetTarget(bool NULL_RETURNS_OWNER = false) {
        SetTarget(ind);
        Behaviour B = null;
        
        if(activeSet != null && activeSet.Count > 0) {
            B = activeSet.At(ind).GetComponent<Behaviour>();
        }
        //LogSelf();
        //Debug.Log("Could not retrieve target");
        if(activeSet == null) {
            //Debug.Log("activeset is null");
        } else if(activeSet.Count < 0 ) {
            //Debug.Log("activeset count is zero");
        }
            
        if(NULL_RETURNS_OWNER && B == null) {
            return owner;
        }

        return B;
    }

    public Behaviour GetConfirmedTarget(bool NULL_RETURNS_OWNER = false) {
        SetTarget(ind);
        //LogSelf();
        if (NULL_RETURNS_OWNER) {
            return target == null ? owner : target;
        } else {
            return target;
        }
    }
    
    public bool HasConfirmedTarget() {
        return target != null ? true : false;
    }

    public void SetOpponents(RuntimeSet set) {
        //Debug.Log("Setting opponents " + set.name);
        opposingSet = set;
        //LogSelf();
    }

    public void ConfirmTarget() {
        target = activeSet.At(ind).GetComponent<Behaviour>();
        //LogSelf();
    }

    public void DeconfirmTarget() {
        target = null;
        //LogSelf();
    }

    public void SetTarget(int i) {
        if(activeSet == null) {
            //Debug.Log("SetTarget, actvieset is null");
            return;
        }
        if(activeSet.Count == 0) {
            //Debug.Log("SetTarget, actvieset zeroo");
            return; 
        }
        // if activeSet is empty we will clamp between
        // ind = Math.Clamp(i, 0, activeSet.Count - 1);

        
        int i_ = i%activeSet.Count;
            
        if(i_ < 0)
            ind = activeSet.Count - 1;
        else
            ind = i_;
        
        if(activeSet.At(i).GetComponent<Behaviour>().GetDataManager().lifeState != LIFE_STATE.ALIVE) {
            SetTarget(ind + 1);
        }
        
        //LogSelf();
    }

    public void TargetUs() {
        activeSet = mySet;
        SetTarget(ind); // index needs to be reclamped to fit the active set
        //LogSelf();
    }

    public void TargetThem() {
        activeSet = opposingSet;
        SetTarget(ind); // index needs to be reclamped to fit the active set
        //LogSelf();
    }

    public void TargetStop() {
        activeSet = null;
        target = null;
        //LogSelf();
    }

    void LogSelf() {
        Debug.Log("Owner is: " + owner + "\nactiveSet: " + activeSet + ", ind is: " + ind + " and my target is: " + target);
    }

}

// If we wanna get complicated with the base stats and do derived stats.
// We should prolly have some sort of dictionary that contains a method defining the output value.
// Dictionary<string statName, Method float(Character ID) GetStat>
public class CharDataManager {
    //public delegate void VoidEvent();
    [SerializeField]
    public CharacterInstance cInstance;
    public CharacterStates cStates;

    public TargetingManager targeting;
    public ActionFunction aFunc;
    public DynamicActionFunction daFunc;
    public ActionSelector aSelector;
    public PropertyCalculator cProperties;
    RuntimeSet _mySet;
    public RuntimeSet mySet {
        get {
            return _mySet;
        }
    }

    bool firstTurn = true;
    //Make life easy, we need these clocks
    public Util.Charger actionCharge;
    public Util.Timer actionTimer;
    
    // TODO: Make a getter setter?
    public CharActionSO actionToDo;

    private CHAR_STATE _characterState;
    public CHAR_STATE characterState {
        get { return _characterState; }
    }

    private ARGE_STATE _chargeState;
    public ARGE_STATE chargeState {
        get { return _chargeState; }
    }

    private LIFE_STATE _lifeState;
    public LIFE_STATE lifeState {
        get { return _lifeState; }
    }
    // Privates
    private Behaviour B;

    private AfflictionManager afflictionManager;

    private AffinityManager affinityManager;

    private float initialActionChargeTime = 10f;

    //private StateStructure stateStructure;
    public CharDataManager(Behaviour _B) {
        B = _B;
    }

    public void Instantiate() {
        cInstance = B.id.Instantiate();
        affinityManager = new AffinityManager(cInstance);
        _mySet = B.mySet;
        actionCharge = new Util.Charger(initialActionChargeTime);
        //actionTimer = new Util.Timer(3f);
        
        
        //stateStructure = new StateStructure(cInstance);
        
        targeting = new TargetingManager(B);

        // TODO: Maybe move CharacterStates into C_ID and Instance
        cStates = new CharacterStates();
        
        // we get a scary situation were PropertyCalculator and AfflictionManager relies on each other
        // and it doesnt work
        cProperties = new PropertyCalculator(cInstance, cStates); 
        

        aFunc = new ActionFunction(cInstance, cProperties);

        aSelector = new ActionSelector(cInstance);

        // cProperties is needed for resistance
        afflictionManager = new AfflictionManager(cInstance, cStates, cProperties); 
        cProperties.InjectAfflictionManager(afflictionManager);

        daFunc = new DynamicActionFunction();
        daFunc.InjectMembers(cInstance, cProperties, cStates);
    }

    /* --- FUNCTIONS --- */
    public void Update() {
        float dt = Time.deltaTime;
        cInstance.UpdateMembers(dt);
        ManageStates();
        
        if(_lifeState != LIFE_STATE.ALIVE) {
            return;
        }
        
        ManageActionUtils();
        HandleAfflictions();  
        HandleCharacterPools(); 
    }

    void ManageStates() {
        float dt = Time.deltaTime;
        cStates.UpdateTime(dt);

        if(cProperties.GetPool(STAT.HEALTH).Item1 <= 0f)
            _lifeState = LIFE_STATE.DEAD;

    }

    void HandleAfflictions() {
        float dt = Time.deltaTime;
        afflictionManager.ResolveAllAfflictions(dt, B.name);
    }

    void ManageActionUtils() {
        // Special case for first turn

        // TODO: this shit works kinda badly with how I want strategy/stance to work. 
        // We can't base whether to tick or not tick the action timer based on char state, if every action will change char state.
        // We'd have to seperate act state from charging action meter, but that'd be a big can of worms to unravel, since state and action meter are basically one system.
        // This needs to be pondered. Consider doing a post "as of right now" design, and redesigning it. 

        if(firstTurn) {
            if(_chargeState == ARGE_STATE.CHARGING) {
                if(actionCharge.CheckAndTick(Time.deltaTime)) {
                    SetCharacterState(CHAR_STATE.READY);
                    //CHARGESTATE CHARGED
                    firstTurn = false;
                }      
            }
        } else {
            if(_chargeState == ARGE_STATE.CHARGING) {
                if(actionCharge.CheckAndTick(Time.deltaTime * cProperties.GetAttribute(ATTRIBUTE.ACTION_CHARGE_SPEED))) {
                    SetCharacterState(CHAR_STATE.READY);
                    //CHARGESTATE CHARGED
                }      
            } 
        }
        
    }

    void HandleCharacterPools() {
        float dt = Time.deltaTime;
        float regenMod = cProperties.GetAttribute(ATTRIBUTE.REGEN_SPEED);
        (float Hcur, float Hmax) = cProperties.GetPool(STAT.HEALTH);

        float v = (Hmax - Hcur)*0.01f; // 1% at most towards 0% if not -1%;
        if(v > 0.0001f || Math.Abs(Hmax - Hcur) > 0.0001f)
            cProperties.HitPool(STAT.HEALTH, -v*dt*regenMod, true);

        (float Scur, float Smax) = cProperties.GetPool(STAT.STAMINA);

        v = (Smax - Scur)*0.1f; 
        if(Scur < Smax) v += Scur*0.01f;// interpolation between 10% of max and 1%
        if(v > 0.0001f || Math.Abs(Smax - Scur) > 0.0001f)
            cProperties.HitPool(STAT.STAMINA, -v*dt*regenMod, true);

        (float Fcur, float Fmax) = cProperties.GetPool(STAT.FOCUS);
        
        v = (Fmax - Fcur)*0.05f; // interpolation between 10% of max and 1%
        if(Fcur < Fmax) v += Fcur*0.005f;
        if(v > 0.0001f || Math.Abs(Fmax - Fcur) > 0.0001f)
            cProperties.HitPool(STAT.FOCUS, -v*dt*regenMod, true);

        (float Wcur, float Wmax) = cProperties.GetPool(STAT.WILL);
        
        v = (Wmax - Wcur)*0.01f; // interpolation between 10% of max and 1%
        if(v > 0.0001f || Math.Abs(Wmax - Wcur) > 0.0001f)
            cProperties.HitPool(STAT.WILL, -v*dt*regenMod, true);
    }

    //Manage ActionCharge and Timer
    public void SetActionChargerTime(float t) {
        actionCharge.Set(t);
    }
    public void FirstActionCharge() {
        firstTurn = true;
        actionCharge.Set(initialActionChargeTime);
    }
    public void SetCharacterState(CHAR_STATE state, bool SetActionCharge = true) {
        
        switch(state) {
            case CHAR_STATE.NON :
                //idk
                break;
            case CHAR_STATE.IDLE :
                break;
            case CHAR_STATE.READY :
                break;
            case CHAR_STATE.PREACTION :
                break;
            case CHAR_STATE.INACTION :
                break;
            case CHAR_STATE.POSTACTION :
                break; 
            default :
                // no
                break;

        }
        _characterState = state;
    }
    public void SetChargeState(ARGE_STATE state) {
        switch(state) {
            case ARGE_STATE.NON :
                // just habbit to have a NON state.
                // implies that we have no reason to check charge_state
                break;
            case ARGE_STATE.IDLE :
                actionCharge.Lock();
                break;
            case ARGE_STATE.CHARGING :
                actionCharge.Unlock();
                break;
            case ARGE_STATE.CHARGED :
                actionCharge.Lock();
                break;
            case ARGE_STATE.CONSUMED :
                actionCharge.ResetTime();
                break;
            default :
                // no
                break;
        }
        _chargeState = state;
    }
    public void ExpendCharge() {
        actionCharge.ResetTime();
    }

    // TODO: We're at the problem again that Resistance doesnt do anything
    public ImpactAftermath ResolveImpactResult(ImpactResult IR) {
        ImpactAftermath res = new ImpactAftermath();

        foreach(ImpactPayload payload in IR.impacts) {
            if(payload.stunType == ACT_STUN.NORMAL) {
                res.stuns++;
            }
            if(payload.hitType == ACT_HIT.MISS) {
                res.misses++;
            }
            if(payload.hitType == ACT_HIT.GLANCE) {
                res.glances++;
            }
            if(payload.hitType == ACT_HIT.GLANCE) {
                res.glances++;
            }
            ApplyImpact(payload);
        }

        return new ImpactAftermath();
    }

    // Unravel 
    public void ApplyImpact(ImpactPayload impact) {
        Stats stats =  cInstance.Stats;
        
        ModifyProperVal(stats, impact);

        // We want to take off a bit of max hp based on dmg taken.
        // Split between vigor and vitality, vigor is current hp, vitality is current max.
        // if you reach 0>= vigor you become unconscious, but dont necesarily die, regen can help you.
        // but if vitality, max hp, reaches zero you are just dead.

        float dmgmod = cProperties.GetAttribute(ATTRIBUTE.AFFLICTION_ACCRUEMENT);
        dmgmod = 2f - dmgmod;
        //Debug.Log("ApplyImpact: What is the dmgmod from drench? " + dmgmod);
        //Debug.Log("CharDataManger impact.damage*dmgmod before affliction " + impact.damage*dmgmod + ", dmgmod is " + dmgmod);
        
        //TODO: call AflictionManager DamageManager? Since AfflictionManager is the class that deals damage to a behaviour
        afflictionManager[AfflictionManager.DMGtoAFLCT(impact.dmgType)].AddStack(impact.damage*dmgmod);
    }   

    public ImpactResult InfluenceImpact(ImpactResult impacts) {
        //Debug.Log(impacts.ToString());

        foreach(ImpactPayload IP in impacts.impacts) {
            //Debug.Log("InfluenceImpact dmg " + IP.damage + " " + IP.dmgType);
            IP.damage *= affinityManager.AffinityInfluence(IP.dmgType);
            
        }
        
        foreach(ImpactPayload IP in impacts.impacts) {
            //Debug.Log("InfluenceImpact new dmg " + IP.damage + " " + IP.dmgType);
            affinityManager.InfluenceAffinities(IP.dmgType, -IP.damage);
            
        }
        
        return impacts;
    }

    public void ApplyCost(CostPayload cost) {
        // TODO: Change Pool modification to go through PropertyCalculator 
        switch(cost.stat) {
            case COST_STAT.HEALTH :
                cProperties.HitPool(STAT.HEALTH, cost.value, true);
                break;
            case COST_STAT.STAMINA :
                cProperties.HitPool(STAT.STAMINA, cost.value, true);
                break;
            case COST_STAT.FOCUS :
                cProperties.HitPool(STAT.FOCUS, cost.value, true);
                break;
            case COST_STAT.WILL :
                cProperties.HitPool(STAT.WILL, cost.value, true); 
                break;
        }
    }
    
    //Bordering on Deprecation
    public void ModifyProperVal(Stats stats, ImpactPayload impact) {
        // A proportion of damage taken


        if(ACT_DMG.FORCE == impact.dmgType) {

            cProperties.HitPool(STAT.HEALTH, impact.damage);
            cProperties.ScarPool(STAT.HEALTH, impact.damage/10f);
            
        }

    }

}