using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public enum BEHAVIOUR_CONTEXT : int {
    NON,
    WORLD,
    BATTLE
}

// ActionManager

public abstract class Behaviour : MonoBehaviour
{   
    public CharacterID id;

    //private CharacterInstance characterInstance;
    public RuntimeSet mySet;
    public RuntimeSet opponentSet;

    //protected CharacterDataManager dataManager;
    
    protected CharDataManager dataManager;
    protected NavMeshAgent navAgent;
    protected bool hasAnimationBehaviour;
    protected AnimationBehaviour animBehaviour;
    protected float navAgentSpeed;

    // TODO: Move ActionFunction and -Selector into DataManager
    // protected ActionSelector actionSelector;

    public VoidEventChannelSO onBattleInit; // Before BattleBegin
    public VoidEventChannelSO onBattleBegin;
    public VoidEventChannelSO onBattleEscape;
    
    //public VoidEventChannelSO onBattleVictory;
    //public VoidEventChannelSO onBattleDefeat;
    //public VoidEventChannelSO onBattleEscape;

    public BEHAVIOUR_CONTEXT context; 

    protected Vector3 cVelocity;
    protected float logTimmer = 0f;

    protected void Awake() {
        cVelocity = new Vector3(0, 0, 0);
        
        dataManager = new CharDataManager(this);
        //actionFunc = new ActionFunction(dataManager.cInstance);
        //actionSelector = new ActionSelector(dataManager.cInstance);
        //characterInstance = id.Instantiate();

        
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        hasAnimationBehaviour = TryGetComponent<AnimationBehaviour>(out animBehaviour);
        
        dataManager.Instantiate();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        
        navAgentSpeed = navAgent.speed;
        onBattleBegin.OnEventRaised += ListenOnBattle; // new Function; ListenOnBattle(), determine whether this should act upon the event 
    }

    // Update is called once per frame
    protected void Update()
    {
        if(navAgent.speed != navAgentSpeed && 
            navAgent.remainingDistance < 0.1f) {
            navAgent.speed = navAgentSpeed;
        }

        //Debug.Log(this.name + " is " + dataManager.lifeState + " and " + dataManager.actionState);

        if(dataManager.lifeState != LIFE_STATE.ALIVE) {

            gameObject.SetActive(false);

            return;
        }
            
        
        switch(context) {
            case BEHAVIOUR_CONTEXT.WORLD :
                BehaveWorld();
                break;
            case BEHAVIOUR_CONTEXT.BATTLE :
                BehaveBattle();
                break;
            case BEHAVIOUR_CONTEXT.NON :
                break;
        }
    }

    public CharDataManager GetDataManager() {
        return dataManager;
    }

    protected abstract void BehaveWorld();

    protected abstract void BehaveBattle();

    // Turn off any runaway process in BehaveWorld and start new ones for BehaveBattle
    public bool TrySwitchContext(BEHAVIOUR_CONTEXT _context) {

        if(_context == context)
            return true;
            
        navAgent.ResetPath();

        switch(_context) {
            case BEHAVIOUR_CONTEXT.WORLD : 
                SwitchToWorld();
                return true;
            case BEHAVIOUR_CONTEXT.BATTLE : 
                SwitchToBattle();
                dataManager.FirstActionCharge();
                return true;
            case 0 :
                return false;
        }

        return false;
    }
    protected abstract void SwitchToBattle();
    protected abstract void SwitchToWorld();

    protected void ListenOnBattle() {
        // If 'this' gives a shit, join the battle
    }

    /* --- Move Component --- */
    // A similar function as above will sooner or later need to be made.
    //protected abstract void SwitchToWorld();

    // Takes a position and attempts to go there.
    public void MoveTo(Vector3 dest, float speedOveride = -1f) {
        if(speedOveride > 0f) {
            navAgent.speed = speedOveride;
        }
        
        navAgent.SetDestination(dest);
    }

    // Takes a vector, and moves in that direction.
    public abstract void MoveDir(Vector3 dir, float speedOveride = -1f);

    /* --- Battle Component ---*/

    
    public void DoAction() {
        //Debug.Log("DoAction check " + !actionSelector.HasSelection() + " or " + (dataManager.target == null));
        if( !dataManager.aSelector.HasSelection() ) {
            return;
        }  

        // Only if the skill requires a target to actually do something should it failsafe if we dont.

        Behaviour target = dataManager.targeting.GetConfirmedTarget();

        if (dataManager.aSelector.ActionRequiresTarget() && target == null) {
            return;
        }
        bool isActionLess = dataManager.aSelector.GetActionType() == ACT_TYPE.STANCE || dataManager.aSelector.GetActionType() == ACT_TYPE.STRATEGY;

        // TODO:
        // if(ActionRequiresTarget) Target.ReactAction()
        // aFunc.OnDoAction();
        
        dataManager.actionToDo = dataManager.aSelector.GetSubmittedAction(true);

        // Check costs
        //if(dataManager.actionToDo.costs)
        if(!dataManager.aFunc.VerifyCostsAreMet(dataManager.actionToDo)) {
            //Debug.Log(gameObject.name + " can't met the requirements of the action.");
            dataManager.actionToDo = null;
            return;
        }
        if(!isActionLess) {
            dataManager.SetActionChargerTime(dataManager.actionToDo.recoveryTime);
            dataManager.SetChargeState(ARGE_STATE.CONSUMED);
        } 

        dataManager.SetCharacterState(CHAR_STATE.PREACTION, true);
        
        // if actionTime is zero, i would like that the character doesnt do an action

        // attacktime needs to be converted into a multiplier
        float actionTimeMod = dataManager.actionToDo.attackTime;
        actionTimeMod *= dataManager.cProperties.GetAttribute(ATTRIBUTE.ACTION_TIME_SPEED);
        
        //Debug.Log(name + " DoAction");
        
        //Debug.Log("Combat:\n" + gameObject.name + " attacked using " + dataManager.actionToDo.name);
        //Do animation
        if(hasAnimationBehaviour) animBehaviour.PlayActionAnimation(actionTimeMod, dataManager.actionToDo);
        else {
            if(!isActionLess) {
                dataManager.SetChargeState(ARGE_STATE.IDLE);
            }
            dataManager.SetCharacterState(CHAR_STATE.INACTION, false);
        }
    } 

    public void SendAction() {
        //Debug.Log(name + " SendAction");
        if(dataManager.actionToDo == null) return;
        bool isActionLess = dataManager.actionToDo.type == ACT_TYPE.STANCE || dataManager.actionToDo.type == ACT_TYPE.STRATEGY;

        ImmediateResult ImmR = dataManager.aFunc.CalcInImmediates(dataManager.actionToDo);
        if(ImmR != null)
            RecieveImmediateResult(ImmR);

        ImpactResult ImpR = dataManager.aFunc.CalcOut(dataManager.actionToDo);
        
        // TODO: 
        //dataManager.aFunc.OnAction(result_Aff);
        //Switch Case for OnAttack, OnSkill, so on.

        if(ImpR != null)
            SendImpactResult(ImpR, dataManager.targeting.GetConfirmedTarget(), 
                dataManager.actionToDo.hitVFXEvent); // or targets ideally, but thats future proofing for no reason
        
        if(hasAnimationBehaviour) animBehaviour.PlayVisualEffect(dataManager.actionToDo.attackVFXEvent);

        dataManager.actionToDo = null;
        if(!hasAnimationBehaviour) {
            if(!isActionLess) {
                dataManager.SetChargeState(ARGE_STATE.CHARGING);
            } 
            dataManager.SetCharacterState(CHAR_STATE.IDLE, false);
        } 
    }

    //Applies Results directly with no intermitten calc. But actually do ALL the intermitten calc lol
    protected void RecieveImpactResult(ImpactResult AR) {

        // Everything can probably be integrated into action func,
        // all that it doesn't have access to is states. which can just be injected.

        // Stage 0 Affinities

        ImpactResult result_Aff = dataManager.InfluenceImpact(AR);
        // can be handled in action func

        // Stage 1 States (Mainly behaviours)
        // This garbage sorta kinda works. But it is garbarge
        
        ImpactResult result_states = dataManager.daFunc.OnRecieveAction(result_Aff);
        dataManager.cStates.UpdateRecieveAction(result_states);
        // Stage 2 Equipment
        ImpactResult result_Equipment = dataManager.aFunc.CalcInEquipment(result_states);

        // Stage 3, Through Stats, actionFunc
        ImpactResult results = dataManager.aFunc.CalcInImpact(result_Equipment);

        // States.updateStates;

        //Debug.Log("Combat:\n" + gameObject.name + " recieves impacts " + results.ToString());
        ImpactAftermath aftermath = dataManager.ResolveImpactResult(results);

        if(aftermath.stuns > 0)
            animBehaviour.PlayHurt();
    }

    //Applies Results directly with no intermitten calc.
    protected void RecieveImmediateResult(ImmediateResult IR) {
        foreach(CostPayload cost in IR.costs) {
            dataManager.ApplyCost(cost);
        }
        
        //TODO
        //dataManager.aFunc.CalcInOnImmediateResult

        if(IR._AttribStateGroups.Count > 0)
            dataManager.cStates.AddStateSource(IR.source, IR._AttribStateGroups);
        
        //dataManager.cStates.AddBehaviourStates(IR.BehaveStateGroups);
        if(IR._BehaveStateGroups.Count > 0) {
            dataManager.cStates.AddStateSource(IR.source, IR._BehaveStateGroups);
        }

        dataManager.cStates.SetTriggerState(dataManager.actionToDo.name, IR.triggerState);
    }

    //Sends Results to be applied directly with no intermitten calc.
    protected void SendImpactResult(ImpactResult A_r, Behaviour B, string hitVFXEvent) {
        B.RecieveImpactResult(A_r);
        // When attacking a target without an Animation Behaviour script, we get some ridiculous
        // bug where SendAction is repeated every frame untill we get back to Idle.

        // Why it does that? My crazy geuss; SendAction executes up to SendImpactResult, SendImpactResult fails due to null reference, 
        // which stops actionToDo from being set to null.
        if(B.animBehaviour != null)
            B.animBehaviour.PlayVisualEffect(hitVFXEvent);
    }
    
    public string GetActionInfo() {
        string info = "Type Selection: " + dataManager.aSelector.actionType + "\n" +
        dataManager.aSelector.ActionInfo();

        return info;
    }

    
/*
    public void LogState() {
        string log = this.gameObject.name + " state:\n" +
            "   dataManager.oponentSet " + dataManager.enemySet.name + "\n" +
            "   dataManager.target " + dataManager.target.name + "\n";

        Debug.Log(log);
    }
*/
}
