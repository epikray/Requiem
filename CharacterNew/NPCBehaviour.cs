using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



[RequireComponent(typeof(NavMeshAgent))]
public class NPCBehaviour : Behaviour
{
    public float hitRangeRadius;

    public bool isPacifist;

    public RewardStruct defeatRewards; // If an inventory system is produced

    NPCController controller;
    CharacterController CController;

    Vector3 inputDest;
    float hitTargetRecharge = 1f;
    float hitTargetCooldown;

    new void Awake() {
        base.Awake();
        defeatRewards.Init();
        Constants.GlobalEnemyCharacterList.Add(gameObject);
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        navAgent = GetComponent<NavMeshAgent>();
        controller = GetComponent<NPCController>();
        CController = GetComponent<CharacterController>();

        hitTargetCooldown = hitTargetRecharge;
        
        //inputDest = new Vector3(0,0,0);
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        dataManager.Update();
        inputDest = controller.targetInput;
    }

    protected override void BehaveWorld() {
        if(inputDest != Constants.NullVector) MoveTo(inputDest);
        HitTargetCooldown();
        if(controller.playerIsTarget) {
            TryAttackTarget();
        }
    }

    /*TODO: Do what we've done in PCBehaviour */
    protected override void BehaveBattle() {
        
        // Determine character state

        InterpretSelectTarget();

        /*
        if(dataManager.actionState == CHAR_STATE.CHARGED) {
            
            DoAction();
        } 

        if(dataManager.actionState == CHAR_STATE.POSTACTION) SendAction();
        */

        TargetingManager T = dataManager.targeting;
        Behaviour target = T.GetConfirmedTarget();
        if(target == null) {
            return;
        }

        CHAR_STATE ourState = dataManager.characterState;
        CHAR_STATE theirState;
        if(target == null) {
            theirState = CHAR_STATE.NON;
        } else {
            theirState = target.GetDataManager().characterState;
        }
        

        if(dataManager.cStates.DoActionTriggerExpire(ourState, theirState) || 
        dataManager.aSelector.GetActionType() == ACT_TYPE.STANCE ||
        dataManager.aSelector.GetActionType() == ACT_TYPE.STRATEGY
        ) {
            //InterpretSelectTarget();
            InterpretSelectAction();
            DoAction();
        }
        
        if(dataManager.cStates.SendActionTriggerExpire(ourState, theirState)) {
            //Debug.Log(this.name + " sends action " + dataManager.actionToDo.name);
            SendAction();
        } 



    }

    void InterpretSelectTarget() {
        // Make a decision based on opponent status

        // For now we can do random targetting.
        // If we make some NPCBattleAI module, that can be interesting.
        // And by AI we just mean state machine
        
        TargetingManager T = dataManager.targeting;
        T.TargetThem();
        

        if(T.GetTarget() == null) {
            T.SetTarget(0);
            T.ConfirmTarget();
        }
        T.ConfirmTarget();
    }

    void InterpretSelectAction() {
        // Given the strongest Core Stat
        // Pick a skill that best utilizes it.

        //Strength -> physical action with high damage
        //Dexterity -> physical action with most effects ***
        //Intelligence -> magic action with high damage
        //Spirt -> magic action with most effects  ***

        // Strongest offensive stat

        if(isPacifist) return;

        ACT_SELECTION s = ACT_SELECTION.HIGHEST_DAMAGE;

        ACT_TYPE t = ACT_TYPE.ATTACK;
        if(dataManager.cInstance.Stats.Intellect.val > dataManager.cInstance.Stats.Strength.val) {
            t = ACT_TYPE.SKILL;
        }

        if(t == ACT_TYPE.ATTACK) {
            if(dataManager.cInstance.Stats.Strength.val < dataManager.cInstance.Stats.Dexterity.val)
                s = ACT_SELECTION.HIGHEST_ACCURACY;
        } else if (t == ACT_TYPE.SKILL) {
            if(dataManager.cInstance.Stats.Intellect.val < dataManager.cInstance.Stats.Spirit.val)
                s = ACT_SELECTION.HIGHEST_ACCURACY;
        }

        if(!dataManager.aSelector.HasSelection()) {
            dataManager.aSelector.SetActionType(t);

            dataManager.aSelector.SelectBy(s);

            dataManager.aSelector.SubmitAction();
        }
        
        
    } 
    

    protected override void SwitchToBattle() {

        //dataManager.enemySet = controller.playerTarget.GetComponent<Behaviour>().GetDataManager().mySet;
        
        dataManager.actionCharge.ResetTime();
        //dataManager.actionTimer.ResetTime();
        
        dataManager.SetCharacterState(CHAR_STATE.IDLE);
        dataManager.SetChargeState(ARGE_STATE.CHARGING);

        context = BEHAVIOUR_CONTEXT.BATTLE;
    }

    protected override void SwitchToWorld() {

        //dataManager.enemySet = null;   
        dataManager.SetCharacterState(CHAR_STATE.NON);  
        dataManager.SetChargeState(ARGE_STATE.NON);   

        context = BEHAVIOUR_CONTEXT.WORLD;
    }

    /*
    public override void MoveTo(Vector3 dest, float speedOveride = 1f) {
        navAgent.speed = navAgent.speed * speedOveride;
        navAgent.SetDestination(dest);   
    }
    */
    
    public override void MoveDir(Vector3 dir, float speedOveride = -1f) {
        
    }
    

    void HitTargetCooldown() {
        if(hitTargetCooldown > 0f) {
            hitTargetCooldown -= Time.deltaTime;
        }
        
    }

    void TryAttackTarget() {
        if(hitTargetCooldown > 0f) return;

        if(!controller.visionField.playerInside) return;

        if( (controller.visionField.playerRef.transform.position - this.transform.position).magnitude < hitRangeRadius ) { // Bug when resting in the middle of a gaurdzone. B stands on inputDest

            hitTargetCooldown = hitTargetRecharge;

            if(controller.agroField.GetType() == typeof(GuardField)) {
                GuardField gField = controller.agroField as GuardField;
                defeatRewards.SumRewards(gField.ExtractRewards());
            }

            onBattleInit.RaiseEvent(this.gameObject, 
                new OnBattleDataObject(transform.position, controller.playerTarget, controller.agroField)
            );

        }
    }

}
