using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCWorldBehavior : WorldBehavior
{

    public Vec3SO pos;

    new void Start()
    {
        base.Start();
        //Debug.Log("PCWorldBehavior, Start()");
    }

    new void Update()
    {   
        base.Update();
        Move();
        if(ReadAttack()) EnterBattle();
        //Debug.Log(transform.position);
        
    }

    new void onEnable(){
        base.onEnable();
    }

    void LateUpdate() {
        pos.set(transform.position);
    }

    private Vector3 ReadMove() {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        return input;
    }

    public override void Move() {
        float speed = running ? walkSpeed : runSpeed;

        groundedPlayer = CController.isGrounded;

        if(groundedPlayer && playerVelocity.y < 0f) {
            playerVelocity.y = 0f;
        }

        Vector3 input = ReadMove();

        //Debug.Log(input * Time.deltaTime * speed + " " +  walkSpeed + " " + runSpeed);

        CController.Move(input * Time.deltaTime * speed);
        //CController.Move(input * Time.deltaTime * speed);

        if (input != Vector3.zero)
        {
            gameObject.transform.forward = input;
        }
        
        playerVelocity.y += gravityValue * Time.deltaTime;
        CController.Move(playerVelocity * Time.deltaTime);
    }

    public void Interact() {
        
    }

    //exchanges/activates itself [the script] for a BattleBehavior instead
    public override void EnterBattle() {
        if(tryChangeBehavior()) {
            Debug.Log(data.charName + " is entering battle!");
        }
    }

    private bool ReadAttack() {
        return Input.GetKeyDown(KeyCode.Backspace) ? true : false;
    }

}
