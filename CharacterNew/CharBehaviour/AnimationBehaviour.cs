using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public enum ANIM_EFFECT : int {
    BLUNT,
    SLASH,
    HEAT,
    COLD,
    CHARGE,
    DOUSE,
}


// Design stipulation, Behaviour and its subclasses will not make calls to AnimationBehaviour and should therefore not need to be known to Behaviour.
// This is because of the idea that Behaviour does not 'need' AnimationBehaviour to function.
// Thats a grand idea. Im gonna make Behaviour call this

// Issue Im having is coordinating the character act states and the animation states. It would be preferable if I could simply
// call a function PlayAnimation(CharAction action), and based on info from charAction AnimationBehaviour would handle all state 
// changes of the Character.
// Read throught StateMachineBehaviour: https://docs.unity3d.com/ScriptReference/StateMachineBehaviour.html
// Looks very useful, they are made and written like MonoBehaviours

// As for vfx, I think we don't need any new structures to handle it. Behaviour has a ref to animBehaviour, and Behaviour is the primary class
// used to communicate actions and animations between target and sender. In the same way Behaviour asks animBehaviour to play an animation based on
// sent in action, we can ask for visual effetcs. The question will then be how to manage ALL the goddamn possible effetcs that could be needed to play.
// A vfx graph that just contains All my skill vfx graphs with OnEvent triggers? Sounds unmanageable, but maybe thats how its supposed to be?

// Its very hard to know 
public class AnimationBehaviour : MonoBehaviour
{
    Animator animator; 
    VisualEffect visualEffect;
    Behaviour coreBehaviour;
    CharDataManager dataManager;


    // Reference to certain important states
    AnimationState chargingState;

    string statePlaying;
    // Start is called before the first frame update
    void Awake() {
        animator = GetComponent<Animator>();
        if(animator == null) {
            Debug.LogError(this.gameObject.name + " is missing a Animator component!");
        }
        
        if(!TryGetComponent<VisualEffect>(out visualEffect)) {
            Debug.LogError(this.gameObject.name + " is missing a Visual Effect component!");
        }

        coreBehaviour = GetComponent<Behaviour>();
        if(coreBehaviour == null) {
            Debug.LogError(this.gameObject.name + " is missing a Behaviour component!");
        }
    }
    void Start()
    {   
        
        
        // Not too surprising, we have race condition here with Behaviour creating a new CharDataManager and this.Awake
        // Good practice is to ask for data references in other components untill Awake is done.
        dataManager = coreBehaviour.GetDataManager();
        if(dataManager == null) {
            Debug.LogError(this.gameObject.name + " is missing a CharDataManager in Behaviour component!");
        }
    }

    // Update is called once per frame
    void Update()
    {  

    }

    public void PlayActionAnimation(float timeMod, CharActionSO action) {
        

        //Debug.Log("What is the timeMod? " + timeMod);
        // based on action type play Attack or Cast

        switch(action.type) {
            case ACT_TYPE.ATTACK :
                animator.SetFloat("Float_AtkMod", 1f/timeMod);
                animator.SetTrigger("Trigger_Attack");
                break;
            case ACT_TYPE.SKILL :
                animator.SetTrigger("Trigger_Charging");
                // based on specs in Action, invoke ResetToIdle()
                Invoke("Attempt_SetTriggerCast", timeMod);
                break;
            case ACT_TYPE.STANCE :
                animator.SetFloat("Float_AtkMod", 1f/timeMod);
                animator.SetTrigger("Trigger_StanceChange");
                break;
            case ACT_TYPE.STRATEGY :
                animator.SetFloat("Float_AtkMod", 1f/timeMod);
                animator.SetTrigger("Trigger_StrategyChange");
                break;
        }

    }

    public void ResetToIdle() {
        animator.SetTrigger("Trigger_Idle");
        dataManager.SetCharacterState(CHAR_STATE.IDLE);
        dataManager.SetChargeState(ARGE_STATE.IDLE);
    }

    public void PlayHurt() {
        //Debug.Log(gameObject.name + " got stunned!");
        animator.SetTrigger("Trigger_Hurt");
        dataManager.SetCharacterState(CHAR_STATE.IDLE);
        dataManager.SetChargeState(ARGE_STATE.IDLE);
    }

    string ActionNameToEffect(string name) {
        // Muh syntax
        return name switch
        {
            "Strike" => "OnBlunt",
            "Slash" => "OnSlash",
            _ => "OnBlunt",// Default vfx 
        };
    }

    void Attempt_SetTriggerCast() {
        //smells slow, but it can be effective
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Charging")) {
            animator.SetTrigger("Trigger_Cast");
        }
        
    }
    // TODO: Refactor and simplify Charge and Character State code
    void SetCharacterState_Idle_IfNotReady(int arg_int) {
        if(dataManager.characterState != CHAR_STATE.READY) {
            if(arg_int == 1) {
                    dataManager.SetCharacterState(CHAR_STATE.IDLE, true);  
                    dataManager.SetChargeState(ARGE_STATE.CHARGING);
            } else {
                    dataManager.SetCharacterState(CHAR_STATE.IDLE, false); 
            }
        }
    }

    void SetCharacterState_Idle(int arg_int) {
        if(arg_int == 1) {
            dataManager.SetCharacterState(CHAR_STATE.IDLE, true); 
            dataManager.SetChargeState(ARGE_STATE.CHARGING);
        } else {
            dataManager.SetCharacterState(CHAR_STATE.IDLE, false); 
        }
        
    }

    void SetCharacterState_Ready(int arg_int) {
        if(arg_int == 1) {
            dataManager.SetCharacterState(CHAR_STATE.READY, true); 
            dataManager.SetChargeState(ARGE_STATE.CHARGED);
        } else {
            dataManager.SetCharacterState(CHAR_STATE.READY, false); 
        }
    }
    
    void SetCharacterState_PreAction(int arg_int) {
        Debug.Log("Sanity check animation event, SetCharacterState_PreAction with arg_int" + arg_int);
        if(arg_int == 1) {
            dataManager.SetCharacterState(CHAR_STATE.PREACTION, true); 
            dataManager.SetChargeState(ARGE_STATE.CONSUMED);
        } else {
            dataManager.SetCharacterState(CHAR_STATE.PREACTION, false); 
        }
    }
    
    void SetCharacterState_InAction(int arg_int) {
        if(arg_int == 1) {
            dataManager.SetCharacterState(CHAR_STATE.INACTION, true); 
            dataManager.SetChargeState(ARGE_STATE.IDLE);
        } else {
            dataManager.SetCharacterState(CHAR_STATE.INACTION, false); 
        }
    }

    // If we play visual effects it could be done here
    void SetCharacterState_PostAction(int arg_int) {
        //Debug.Log("Sanity check animation event, SetCharacterState_PostAction with arg_int" + arg_int);
        if(arg_int == 1) {
            dataManager.SetCharacterState(CHAR_STATE.POSTACTION, true); 
            dataManager.SetChargeState(ARGE_STATE.CHARGING);
        } else {
            dataManager.SetCharacterState(CHAR_STATE.POSTACTION, false); 
        }
    }

    // But we might aswell make a specific callback for playing animations.
    public void PlayVisualEffect(string name) {
        // Some effects are attacking effects,others are when hit. 
        // These two cases need to be organized.

        visualEffect.SendEvent(name);

    }
    
}


