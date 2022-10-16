using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string startingAnimation;

    [Header("Player Control and State")]
    [SerializeField] private PlayerControl playerControl;
    [SerializeField] private PlayerState prevState;

    // Start is called before the first frame update
    private void Start()
    {
        playerControl = GetComponent<PlayerControl>();

        animator.Play(startingAnimation);
    }

    // Update is called once per frame
    private void Update()
    {

    }

    // Research for animation state machine conduct via these sources:
    // https://docs.unity3d.com/Manual/class-Transition.html
    // https://docs.unity3d.com/ScriptReference/Animator.SetTrigger.html
    // https://learn.unity.com/tutorial/animator-controllers#5ce2b1caedbc2a0d2ece30f3
    // public methods called by playerController to set 'Trigger' paramaters,
    // which the animator controller state machine use as conditionals
    // for transitioning between animation states
    public void TriggerIdle()
    {        
        animator.ResetTrigger("TrWalk");
        animator.SetTrigger("TrIdle");
    }

    public void TriggerWalk()
    {
        animator.ResetTrigger("TrIdle");
        animator.SetTrigger("TrWalk");
    }

    public void TriggerJumpStart()
    {
        animator.ResetTrigger("TrLand");
        animator.SetTrigger("TrJump");
    }

    public void TriggerLanding()
    {
        animator.ResetTrigger("TrGlide");
        animator.ResetTrigger("TrJump");
        animator.SetTrigger("TrLand");
    }

    public void TriggerGlide()
    {
        animator.ResetTrigger("TrLand");
        animator.ResetTrigger("TrJump");
        animator.SetTrigger("TrGlide");
    }

    public void TriggerJumpMid()
    {
        animator.ResetTrigger("TrGlide");
        animator.SetTrigger("TrJump");
    }

    public void TriggerAttach()
    {
        animator.ResetTrigger("TrDetach");
        animator.SetTrigger("TrAttach");
    }

    public void TriggerDetach()
    {
        animator.ResetTrigger("TrAttach");
        animator.SetTrigger("TrDetach");
    }

    public Animator GetAnimator()
    {
        return animator;
    }

}
