using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimActionPort : MonoBehaviour
{
    // Start is called before the first frame update


    //Ideally this should be general.
    //Script 


    //BattleBehavior invokes DoX(string Xname), X is an Action in the list of Actions in CharacterData
    //Character data begins correct animation and calculates payload of X
    //If animation Hit stage is reached, Animation event AEvent_Attack_hit() is called delivering the payload
    //if Attack_hit isnt reached, then the character was stunned before the hit, and the payload is lost
    //The Stability in CharData should be used to determine if a hit should stun or not
    //There should be no

    [SerializeField]
    private Animator animator;

    //[SerializeField]
    //private AnimActionChannelSO aaChannel;

    [SerializeField]
    private CharacterDataSO Cdata;

    private float t;

    void Start()
    {
        animator = GetComponent<Animator>();
        if(!animator) {
            animator = GetComponentInChildren<Animator>();
        }
        t = 0;
        Debug.Log(animator.ToString());
        Cdata = GetComponentInParent<Character>().data;

    }



    // Update is called once per frame
    void Update()
    {
        
       
    }

    public void Anim_Start_Hurt() {
        animator.SetTrigger("Trigger_Hurt");
    }

    public void Anim_Start_Attack() {
        animator.SetTrigger("Trigger_Attack");
    }

    public void AA_Attack_start() {
        
    }
    
    public void AA_Attack_hit() {
        Cdata.DoAction();
    }

    public void AA_Attack_stop() {
        
    }

    public void debug_anim() {
        t += Time.deltaTime;
        if(t > 5) {
            animator.SetTrigger("Trigger_Attack");
            t = 0;
        }
    }
}
