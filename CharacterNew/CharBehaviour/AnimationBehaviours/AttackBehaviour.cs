using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour {
    //Id need more references to make these useful
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateEnter : Attack" + animator.name + " " + stateInfo.ToString() + " " + layerIndex);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("OnStateExit : Attack" + animator.name + " " + stateInfo.ToString() + " " + layerIndex);
    }

}