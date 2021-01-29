using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    Animator anim;
    PlayerMovement movement;
    //获得Animator的参数的编号
    int groundID,hangingID,crouchID,fallID;
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        //获取父级Playermovement的所有变量和方法
        movement = GetComponentInParent<PlayerMovement>();
        rb = GetComponentInParent<Rigidbody2D>();
        //将ID与unity中的参数关联
        groundID = Animator.StringToHash("isOnGround");
        hangingID = Animator.StringToHash("isHanging");
        crouchID = Animator.StringToHash("isCrouching");
        fallID = Animator.StringToHash("verticalVelocity");
    }

    // Update is called once per frame
    void Update()
    {
        //两种写法
        anim.SetFloat("speed",Mathf.Abs(movement.xVelocity));
        //anim.SetBool("isOnGround", movement.isOnGround);
        anim.SetBool(groundID, movement.isOnGround);
        anim.SetBool(hangingID, movement.isHanging);
        anim.SetBool(crouchID, movement.isCrouch);
        //设置跳跃BlendTree的动画，其float值就是角色刚体y轴跳跃的加速度
        anim.SetFloat(fallID, rb.velocity.y);
    }

    public void StepAudio()
    {
        AudioManager.PlayFootstepAudio();
    }

    public void CrouchStepAudio()
    {
        AudioManager.PlayCrouchFootstepAudio();
    }
}
