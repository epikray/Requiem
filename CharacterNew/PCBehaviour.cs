using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PCBehaviour : Behaviour
{   
    PCController controller;

    public Vector3 inputDir;
    Vector3 desiredVel;


    
    // TODO: This can most definetly be removed for a simple collider. Its method are not used for anything
    //  Check DetectionField.cs for the bug aswell.
    CharacterController CController;
    
    // This MB never touches the positioning of the camera, it is all determined by the camera 
    // with a ref to the targetting manager
    CameraMovement cameraMovement;
    

    public bool doActionImmediately;
    new void Awake() {
        base.Awake();
        //Constants.PlayerCharacterList.Add(gameObject);
        //dataManager.mySet.Add(this.gameObject);
        CController = GetComponent<CharacterController>();
        controller = GetComponent<PCController>();
        navAgent = GetComponent<NavMeshAgent>();
        inputDir = new Vector3(0,0,0);
        desiredVel = new Vector3(0,0,0);
    }

    new void Start() {
        base.Start();

        cameraMovement = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovement>();
        //cameraMovement.targetingManager = dataManager.targeting;
        
        dataManager.aFunc.debug = false;
        dataManager.aSelector.debug = false;
    }

    float countdown = 0;
    new void Update() {
        base.Update();
        dataManager.Update();
        inputDir = controller.moveInput;

        // Cheat
        if(Input.GetKeyDown(KeyCode.Z)) {
            if(Input.GetKey(KeyCode.LeftShift)) {
                dataManager.cProperties.HitPool(STAT.HEALTH, -100f);
            } else {
                dataManager.cProperties.HitPool(STAT.HEALTH, 10f);
            }
        }

        if(Input.GetKeyDown(KeyCode.X)) {
            if(Input.GetKey(KeyCode.LeftShift)) {
                dataManager.cProperties.ScarPool(STAT.HEALTH, -10f);
            } else {
                dataManager.cProperties.ScarPool(STAT.HEALTH, 1f);
            }
        }

        if(Input.GetKeyDown(KeyCode.F1)) {
            if(Time.timeScale == 1.0f) {
                Time.timeScale = 0.5f;
            } else {
                Time.timeScale = 1.0f;
            }
            //Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;
        }

        countdown += Time.deltaTime;
        if(countdown > 1f) {
            Debug.Log("Amount of groups and states on the player char " + dataManager.cStates.groupCount + " " + dataManager.cStates.stateCount);
            countdown = 0;
        }


        // debug
        /*
        if(Time.frameCount % 1200 == 0) {
            dataManager.PrintAffinities(ACT_DMG.FORCE, 50);
        } else if (Time.frameCount % 1200 == 600) {
            dataManager.PrintAffinities(ACT_DMG.HEAT, 50);
        }
        */
    }

    protected override void BehaveWorld() {
        float speedOveride = -1;
        if(Input.GetKey(KeyCode.LeftControl)) speedOveride = 10f;


        if(inputDir != Constants.NullVector) MoveDir(inputDir, speedOveride);

    }

    /*
    TODO: Abstract this out to Behaviour? See first how it needs to be implemented in NPCBehaviour
    */
    protected override void BehaveBattle() {
        // Select Targets
        
        InterpretSelectTarget();

        /* --- ACTION SELECTION --- */
        InterpretSelectAction();


        // Stance.DoActive
        TargetingManager T = dataManager.targeting;
        Behaviour target = T.GetConfirmedTarget();

        CHAR_STATE ourState = dataManager.characterState;
        CHAR_STATE theirState;
        if(target == null) {
            theirState = CHAR_STATE.NON;
        } else {
            theirState = target.GetDataManager().characterState;
        }
        
        // TODO: Stances and Strategies should always ignore Triggr logic. Makes the character clunky otherwise
        // Im tired of dickhead not doing anything because he's set to OnReact
        if(dataManager.cStates.DoActionTriggerExpire(ourState, theirState) || 
        dataManager.aSelector.GetActionType() == ACT_TYPE.STANCE ||
        dataManager.aSelector.GetActionType() == ACT_TYPE.STRATEGY
        ) {
            DoAction();
        }

        //if(dataManager.characterState == CHAR_STATE.CHARGED) DoAction();

        if(dataManager.cStates.SendActionTriggerExpire(ourState, theirState)) {
            //Debug.Log(this.name + " sends action " + dataManager.actionToDo.name);
            SendAction();
        }

        //if(dataManager.characterState == CHAR_STATE.POSTACTION) SendAction(); 

    }
    
    void InterpretSelectTarget() {
        TargetingManager T = dataManager.targeting;

        //T.TargetThem();


        // I've changed my mind on selection controls.
        // 'D-Pad' and 'bumpers' are used for selection and not just bumpers.
        // Secondary axis' (D-Pad) will be for switching targets and switching teams.
        // Left and right axis will be used for confirmation and cancelled 
        
        if(controller.selectUpPressed) {
            T.TargetThem();
        }

        if(controller.selectDownPressed) {
            T.TargetUs();
        }


        if(controller.selectLeftPressed) {
            //T.TargetThem();
            T.SetTarget(T.targetIndex - 1);
            /*
            if(T.GetTarget().GetDataManager().lifeState != LIFE_STATE.ALIVE) {
                T.SetTarget(T.targetIndex - 1);
            }
            */
        }
            
        if(controller.selectRightPressed) {
            //T.TargetThem();
            T.SetTarget(T.targetIndex + 1);
            /*
            if(T.GetTarget().GetDataManager().lifeState != LIFE_STATE.ALIVE) {
                T.SetTarget(T.targetIndex + 1);
            }
            */
        }

        if(controller.targetLeftPressed) {
            if(T.HasConfirmedTarget()) {
                T.DeconfirmTarget();
            } else {
                T.ConfirmTarget();
            }
            
        }

        if(controller.targetRightPressed) {
            
        }
        
        // TODO: Haven't found a clean way to do this yet
        // But action selection is completely handled in UIActionController
        // Maybe grant an interface for it? or something???
        if(Input.GetKeyDown(KeyCode.Q)) {
            //dataManager.aSelector.CancelSelection();
        }
           
    }
    
    void InterpretSelectAction() {
        /* --- ACTION SELECTION --- */
        if(dataManager.aSelector.HasSelection()) {
            //cameraMovement.FROM_PLAYERTARGET = true;
        }
        if(!dataManager.aSelector.HasSelection() && dataManager.characterState == CHAR_STATE.IDLE) {
            //cameraMovement.FROM_PLAYERTARGET = false;
        }
        /*
        if(Input.GetKeyDown(KeyCode.I)) {
            dataManager.aSelector.SetActionType(ACT_TYPE.ATTACK);
        } else if (Input.GetKeyDown(KeyCode.K)) {
            dataManager.aSelector.SetActionType(ACT_TYPE.SKILL);
        } else if (Input.GetKeyDown(KeyCode.J)) {
            dataManager.aSelector.SetActionType(ACT_TYPE.STANCE);
        } else if (Input.GetKeyDown(KeyCode.L)) {
            dataManager.aSelector.SetActionType(ACT_TYPE.STRATEGY);
        }
        
        
        if(Input.GetKeyDown(KeyCode.Alpha1)) {           
            dataManager.aSelector.SelectAt(0);         
        }

        if(controller.confirmPressed) {
            dataManager.aSelector.SubmitAction();

            //if(doActionImmediately) DoAction();
        }
        */
    }

    protected override void SwitchToBattle() {

        //Write a try catch with exception InvalidCastException
        //Vector3 battleCenter = ( (OnBattleDataObject)onBattleBegin.data);
        //dataManager.enemySet = onBattleBegin.caller.GetComponent<Behaviour>().GetDataManager().mySet;
        dataManager.actionCharge.ResetTime();
        //dataManager.actionTimer.ResetTime();
        cameraMovement.usingCombatOffset = true;
        dataManager.SetCharacterState(CHAR_STATE.IDLE);
        dataManager.SetChargeState(ARGE_STATE.CHARGING);

        context = BEHAVIOUR_CONTEXT.BATTLE;
    } 

    protected override void SwitchToWorld() {

        //Write a try catch with exception InvalidCastException
        //Vector3 battleCenter = ( (OnBattleDataObject)onBattleBegin.data);
        //dataManager.enemySet = null;
        //cameraMovement.target = this.transform;
        cameraMovement.usingCombatOffset = false;
        dataManager.SetCharacterState(CHAR_STATE.NON);
        dataManager.SetChargeState(ARGE_STATE.NON);
        context = BEHAVIOUR_CONTEXT.WORLD;
    }

    public override void MoveDir(Vector3 dir, float speedOveride = -1f) {
        float moveSpeed = 5f;

        Vector3 movement = speedOveride > 0f? dir*speedOveride : dir*moveSpeed;
        desiredVel = movement;

        if (movement != Vector3.zero)
        {
            gameObject.transform.forward = movement;
        }

        navAgent.Move(desiredVel * Time.deltaTime);        
    }

    public void RecieveReward(RewardStruct reward) {
        foreach(CharActionSO action in reward.skillRewards) {
            dataManager.cInstance.InsertAction(action);
        }
        foreach(Item item in reward.itemRewards) {
            dataManager.cInstance.InsertItem(item);
        }
        dataManager.cInstance.AddGrowth(reward.growthReward);
    }
}
