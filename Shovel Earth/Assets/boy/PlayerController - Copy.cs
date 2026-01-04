using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController characterController;
    private Animator animator;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(horizontal, 0, vertical);
        if(dir!=Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
            animator.SetBool("isRun", true);
            transform.Translate(Vector3.forward * 2 * Time.deltaTime);

        }
        else
        {
            animator.SetBool("isRun", false);

        }

    }
}