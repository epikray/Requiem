using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCBattleBehavior : BattleBehavior
{

    [SerializeField]
    private Vec3SO pos;

    public bool controlled;

    private bool selectingParty = false;

    private float s_timer = 10f;


    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        readyActive = true;

        controlled = true;
        //actionChannel.transactionDelegate += readTransactions;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();       

        s_timer += Time.deltaTime;

        if(controlled) {
            InterpretInput();
        }

        //MAKE AN UPDATE BASED ON READYMETER
        if(readyActive) {
            QueueAct();
            
        }
        
        if(Input.GetKeyDown(KeyCode.Backspace)) {
            LeaveBattle();
        }
    }

    //To act should be an event

    //
    new void LateUpdate() {
        base.LateUpdate();
        pos.set(transform.position);


    }

    new void onEnable() 
    {
        base.onEnable();
    }

    public void readTransactions() {
        Debug.Log(name + " got a message");
    }

    protected void InterpretInput() {
        // Animate UI
        
        // Move left right up down
        Selection();
        // accept, canncel

        // 
    }
    // Pick character
    private void Selection() {

        if(target.dead) {
            if(TargetIsInOwnParty()) {
                target = GetFirstLivingFriend();
            } else {
                target = GetFirstLivingEnemy();
            }
        }
        
        if(s_timer < 0.5f) {
            return;
        }

        if(Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f) {
            if(!selectingParty){
                target = data;
            } else {
                target = oponents.members[0];
            }
            selectingParty = !selectingParty;
            
            s_timer = 0f;
            Debug.Log(gameObject.name + " is now targetting " + target.name);
        }

        if(selectingParty) {
            if(Input.GetAxis("Horizontal") > 0.1f) {
                target = GetNextPartyCharacter();
                s_timer = 0f;
                Debug.Log(gameObject.name + " is now targetting " + target.name);
            }
            if(Input.GetAxis("Horizontal") < -0.1f) {
                target = GetPrevPartyCharacter();
                s_timer = 0f;
                Debug.Log(gameObject.name + " is now targetting " + target.name);
            }
        } else {
            if(Input.GetAxis("Horizontal") > 0.1f) {
                target = GetNextEnemyCharacter();
                s_timer = 0f;
                Debug.Log(gameObject.name + " is now targetting " + target.name);
            }
            if(Input.GetAxis("Horizontal") < -0.1f) {
                target = GetPrevEnemyCharacter();
                s_timer = 0f;
                Debug.Log(gameObject.name + " is now targetting " + target.name);
            }
        }
        
        
    }

    public override void UseAttack(string actionname) {

        Debug.Log(data.charName + " attacked opponent " + target);


        data.QueueAction(actionname, target);

        aaPort.Anim_Start_Attack();
    }
    /*
        These are all duplicates now, because everything is an action. As per its namesake.
        What they should *eventually* differ in is the pool of actions they get from.

        Attack/Defend Actions should recide and be derived from the char and the weapon they use.
        Sword gets slash, stab. Magic Mace gets crush, magic crush. etc. etc. 

        Skills and strategies are purely innate.
    
    */ 
    public override void UseDefend(string actionname) {
        Debug.Log(data.charName + " defended against opponent " + target);
        data.QueueAction(actionname, target);
        aaPort.Anim_Start_Attack();
    }

    public override void UseSkill(string actionname) {
        Debug.Log(data.charName + " used skill against opponent " + target);
        data.QueueAction(actionname, target);
        aaPort.Anim_Start_Attack();
    }

    public override void UseStrategy(string actionname) {
        Debug.Log(data.charName + " employed strategy against opponent " + target);
        data.QueueAction(actionname, target);
        aaPort.Anim_Start_Attack();
    }
    
    public void JoinMainParty() {
        //data.JoinParty();
    }

    public void LeaveMainParty() {
        //data.LeaveParty();
    }

    public void pickTarget() {
        
    }

    public void LeaveBattle() {
        if(tryChangeBehavior()) {
            Debug.Log(data.charName + " ran away!");
        }
    }

    private void QueueAct() {
        ActKeyboard();
        //Debug.Log(a.toString());
    }

    private void ActKeyboard() {
        
        if(Input.GetButtonDown("Action1")) {
            UseAttack("TestAttack");
        }
        else if(Input.GetButtonDown("Action2")) {
            UseDefend("TestDefend");
        }
        else if(Input.GetButtonDown("Action3")) {
            UseSkill("TestSkill");
        }
        else if(Input.GetButtonDown("Action4")) {
            UseStrategy("TestStrategy");
        }

        if(Input.GetButtonDown("Cancel")) {
            LeaveBattle();
        }

    }

}
