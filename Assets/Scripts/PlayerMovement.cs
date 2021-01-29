using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private BoxCollider2D coll;

    [Header("移动速度")]
    public float speed = 8f;
    public float crouchSpeedDivison = 3f;

    [Header("跳跃参数")]
    public float jumpForce = 6.3f;
    //长按加成
    public float jumpHoldForce = 1.9f;
    //长按的持续时间
    public float jumpHoldDuration = 0.1f;
    //下蹲跳跃加成
    public float crouchJumpBoost = 2.5f;
    //悬挂跳跃
    public float hangingJump = 15f;
    //跳跃时间
    float jumpTime;

    [Header("状态")]
    public bool isCrouch;
    //是否站在地上
    public bool isOnGround;
    public bool isJump;
    //是否头顶被锁住
    public bool isHeadBlocked;
    //是否在悬挂
    public bool isHanging;

    public float xVelocity;

    //按键设置
    //单次跳跃
    bool jumpPressed;
    //长按跳跃
    bool jumpHeld;
    //长按下蹲
    bool crouchHeld;
    //单次下蹲
    bool crouchPressed;

    [Header("环境检测")]
    public float footOffset = 0.4f;//碰撞体的一半，左右两只脚的距离的位置
    public float headClearance = 0.5f;//头顶检测的距离
    public float groundDistance = 0.2f;//与地面之间的距离
    //角色的高度
    float playerHeight;
    public float eyeHeight = 1.4f;//角色眼睛的位置
    public float grabDistance = 0.4f;//悬挂距离墙壁的距离
    public float reachOffset = 0.7f;//角色头顶往上无墙壁，头顶往下有墙壁


    public LayerMask groundLayer;

    //碰撞体尺寸,保证下蹲碰撞体尺寸减半
    Vector2 colliderStandSize;
    Vector2 colliderStandOffset;
    Vector2 colliderCrouchSize;
    Vector2 colliderCrouchOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        playerHeight = coll.size.y;

        colliderStandSize = coll.size;
        colliderStandOffset = coll.offset;

        colliderCrouchSize = new Vector2(coll.size.x, coll.size.y / 2f);
        colliderCrouchOffset = new Vector2(coll.offset.x, coll.offset.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
        //jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        crouchHeld = Input.GetButton("Crouch");
        crouchPressed = Input.GetButtonDown("Crouch");
    }
    private void FixedUpdate()
    {
        if(isJump)
            jumpPressed = false;
        PhysicsCheck();
        GroundMovement();
        MidAirMovement();
    }

    //物理检测
    void PhysicsCheck()
    {
        //射线
        //查看当前角色的位置
        //Vector2 pos = transform.position;
        //左脚的偏移
        //Vector2 offset = new Vector2(-footOffset, 0f);
        //检测起点为左脚的位置，即左下角，则角色位置+左脚的偏移即为检测起点
        //画射线
        //RaycastHit2D leftCheck = Physics2D.Raycast(pos + offset, Vector2.down, groundDistance, groundLayer);
        //显示射线用于debug
        //Debug.DrawRay(pos + offset, Vector2.down, Color.red, 0.2f);

        //左右脚射线
        RaycastHit2D leftCheck = Raycast(new Vector2(-footOffset,0f),Vector2.down,groundDistance,groundLayer);
        RaycastHit2D rightCheck = Raycast(new Vector2(footOffset, 0f), Vector2.down, groundDistance, groundLayer);

        if (leftCheck||rightCheck)
        //if (coll.IsTouchingLayers(groundLayer))
            isOnGround = true;
        else
            isOnGround = false;

        //判断头顶是否有物体
        RaycastHit2D headCheck = Raycast(new Vector2(0f, coll.size.y), Vector2.up, headClearance, groundLayer);
        isHeadBlocked = headCheck ? true : false;

        //方向
        float direction = transform.localScale.x;
        //射线方向
        Vector2 grabDir = new Vector2(direction, 0f);
        //头顶左右上角射线
        RaycastHit2D blockedCheck = Raycast(new Vector2(footOffset * direction, playerHeight), grabDir, grabDistance, groundLayer);
        //眼睛射线
        RaycastHit2D wallChecked = Raycast(new Vector2(footOffset * direction, eyeHeight), grabDir, grabDistance, groundLayer);
        //判断头顶往上无墙，头顶往下有墙的射线
        RaycastHit2D ledgeCheck = Raycast(new Vector2(reachOffset * direction, playerHeight), Vector2.down, grabDistance, groundLayer);

        //当角色不在地面，正在下落，竖的射线和眼睛射线接触到碰撞体但头顶无碰撞体，则悬挂
        if(!isOnGround && rb.velocity.y < 0f && ledgeCheck && wallChecked && !blockedCheck)
        {
            //角色的位置
            Vector3 pos = transform.position;
            //使角色贴在墙壁上，而没有多余的空隙
            //wallChecked.distance角色距离墙的距离，0.05f留出手臂的空隙
            pos.x += (wallChecked.distance - 0.1f) * direction;
            pos.y -= ledgeCheck.distance;//减掉头顶突出的位置

            transform.position = pos;
            //使角色静止
            rb.bodyType = RigidbodyType2D.Static;
            isHanging = true;
        }
    }

    void GroundMovement()
    {
        //若角色正在悬挂，则无法进行其他移动
        if (isHanging)
            return;

        if (crouchHeld && !isCrouch && isOnGround)
        {
            Crouch(); 
        }
        else if (!crouchHeld && isCrouch && !isHeadBlocked)
        {//人物处于下蹲状态但玩家已经没有在按下蹲的按键则站起来
            StandUp();
        }
        else if(!isOnGround && isCrouch)
        {
            StandUp();
        }
        //左右移动
        xVelocity = Input.GetAxis("Horizontal");
        //如果是下蹲则速度减慢
        if (isCrouch)
            xVelocity /= crouchSpeedDivison;
        //开始移动
        rb.velocity = new Vector2(xVelocity * speed, rb.velocity.y);
        //左右移动改变角色朝向
        FilpDirection();
    }
    void FilpDirection()
    {
        if (xVelocity < 0)
            transform.localScale = new Vector3(-1, 1,1);
        if (xVelocity > 0)
            transform.localScale = new Vector3(1, 1,1);
            
    }
    void Crouch()
    {
        isCrouch = true;
        //下蹲时碰撞体尺寸减半
        coll.size = colliderCrouchSize;
        coll.offset = colliderCrouchOffset;
    }
    void StandUp()
    {
        isCrouch = false;
        //站起来碰撞体回到站立的尺寸
        coll.size = colliderStandSize;
        coll.offset = colliderStandOffset;

    }

    void MidAirMovement()
    {
        if (isHanging)
        {
            if (jumpPressed)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = new Vector2(rb.velocity.x, hangingJump);
                isHanging = false;
            }
            if (crouchPressed)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                isHanging = false;
            }
            return;
        }
        if(jumpPressed && isOnGround && !isJump && !isHeadBlocked)//按下跳跃且在地面且不是跳跃的状态
        {
            if(isCrouch)
            {
                StandUp();
                rb.AddForce(new Vector2(0f, crouchJumpBoost), ForceMode2D.Impulse);
            }

            isOnGround = false;
            isJump = true;

            //计算跳跃时间
            jumpTime = Time.time + jumpHoldDuration;

            //跳跃，增加一个向上的力，ForceMode2D.Impulse是一种突然的力
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);

            AudioManager.PlayJumpAudio();

        }else if (isJump)
        {
            if (jumpHeld)//如果长按跳跃，使用长按的跳跃力跳跃
                rb.AddForce(new Vector2(0f, jumpHoldForce), ForceMode2D.Impulse);
            if (jumpTime < Time.time)//跳跃时间小于当前时间，表示跳跃结束
                isJump = false;
        }
    }
    //重构Raycast方法
    RaycastHit2D Raycast(Vector2 offset, Vector2 rayDirection, float length, LayerMask layer)
    {
        Vector2 pos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(pos + offset, rayDirection, length, layer);
        Color color = hit ? Color.red : Color.green;
        Debug.DrawRay(pos + offset, rayDirection * length,color);
        return hit;
    }
}
