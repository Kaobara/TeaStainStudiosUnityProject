using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rigidbody;
    private PlayerControl playerController;

    // Start is called before the first frame update
    private void Start()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerControl>();
    }

    // Update is called once per frame
    private void Update()
    {
    
    }

    //private void SetTrigger

}
