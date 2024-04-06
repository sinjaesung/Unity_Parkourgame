using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFallingMovement : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Debug.Log("ControlFallingMovement Animation StateEnter");
        animator.GetComponent<PlayerScript>().HasPlayerControl = false;
    }

    public override void OnStateExit(Animator animator,AnimatorStateInfo animatorStateInfo, int layerIndex)
    {
        Debug.Log("ControlFallingMovement Animation StateExit");
        animator.GetComponent<PlayerScript>().HasPlayerControl = true;
    }
}
