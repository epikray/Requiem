using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldBehavior : MonoBehaviour
{
    /*
        Defines what a Charcter can do in the overworld
    */
    public CharacterController CController;
    protected CharacterDataSO data;

    public Vector3 playerVelocity;
    public bool groundedPlayer;
    [SerializeField]
    public bool running = true;
    //Can be derived from HumanoidSO instead
    public float runSpeed;
    public float walkSpeed;

    public float gravityValue = -9.81f;

    [SerializeField]
    protected bool hasBB;

    protected void Start()
    {
        data = GetComponent<Character>().GetCharacterData();
        CController = gameObject.GetComponent<CharacterController>();
        runSpeed = data.runSpeed;
        walkSpeed = data.walkSpeed;
        
        hasBB = hasCousinBehavior();
        //Debug.Log("WorldBehavior Start()");
    }

    protected void onEnable() {
        Debug.Log(this + "onEnable()");
        hasBB = hasCousinBehavior();
    }

    protected void Update()
    {   

    }

    public abstract void Move();

    public abstract void EnterBattle();

    public bool tryChangeBehavior() {
        if(hasCousinBehavior()) {
            GetComponent<BattleBehavior>().enabled = true;
            this.enabled = false;
            return true;
        } else {
            Debug.Log(this + " could not find an attached BattleBehavior");
            return false;
        }
    }

    
    protected bool tryChangeBehavior(bool fight) {
        if(hasCousinBehavior()) {
            gameObject.GetComponent<WorldBehavior>().enabled = true;
            this.enabled = false;
            return true;
        } else {
            Debug.Log(this + " could not find an attached WorldBehavior");
            return false;
        }
    }
    

    private bool hasCousinBehavior() {
        var bb = GetComponent<BattleBehavior>();
        //Debug.Log("hasCousingBehavior sanity check " + bb.data.charName);
        if(bb) {
            return true;
        } else {
            return false;
        }
    }
}
