using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerScript : MonoBehaviour
{
    [Header("Player Movement")]
    public float movementSpeed = 5f;
    public MainCameraController MCC;
    public EnvironmentChecker environmentChecker;
    public float rotSpeed = 600f;
    Quaternion requiredRotation;
    bool playerControl = true;
    public bool playerInAction { get; private set; }

    [Header("Player Animator")]
    public Animator animator;

    [Header("Player Collision & Gravity")]
    public CharacterController CC;
    public float surfaceCheckRadius = 0.3f;
    public Vector3 surfaceCheckOffset;
    public LayerMask surfaceLayer;
    bool onSurface;
    public bool playerOnLedge { get; set; }
    public bool playerHanging { get; set; }
    public LedgeInfo LedgeInfo { get; set; }
    [SerializeField] float fallingSpeed;
    [SerializeField] Vector3 moveDir;
    [SerializeField] Vector3 requiredMoveDir;
    Vector3 velocity;

    private void Update()
    {
        if (!playerControl)
            return;

        if (playerHanging)
            return;

        velocity = Vector3.zero;

        if (onSurface)
        {
            fallingSpeed = 0f;
            velocity = moveDir * movementSpeed;

            playerOnLedge = environmentChecker.CheckLedge(moveDir,out LedgeInfo ledgeInfo);

            if (playerOnLedge)
            {
                LedgeInfo = ledgeInfo;
                //Debug.Log("Player is on ledge ledgeHeight" + LedgeInfo.height);
                if (LedgeInfo.height >= 5)
                {
                    playerLedgeMovement();
                }
            }

            animator.SetFloat("movementValue", velocity.magnitude / movementSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            fallingSpeed += Physics.gravity.y * Time.deltaTime;

            velocity = transform.forward * movementSpeed / 2;
        }

        velocity.y = fallingSpeed;
      //  Debug.Log("PlayerHanging:" + playerHanging);
        PlayerMovement();
        SurfaceCheck();
        animator.SetBool("onSurface", onSurface);
        //Debug.Log("Player on Surface" + onSurface);
    }

    void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float movementAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
        var movementInput = (new Vector3(horizontal, 0, vertical)).normalized;
        //Debug.Log("[[PlayerMovement]]Time deltaTime:" + movementInput + "," + Time.deltaTime);

        //Debug.Log("[[PlayerMovement]]MCC.flatRotation" + MCC.flatRotation);
        //Debug.Log("[[PlayerMovement]]movementInput->movementDirection (MCC.flatRotation * movementInput)" + movementInput+"*"+ MCC.flatRotation+"->" + moveDir);

        requiredMoveDir = MCC.flatRotation * movementInput;
        CC.Move(velocity * Time.deltaTime);

        if (movementAmount > 0 && moveDir.magnitude > 0.2f)
        {
            requiredRotation = Quaternion.LookRotation(moveDir);
        }

        moveDir = requiredMoveDir;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, requiredRotation, rotSpeed * Time.deltaTime);
    }

    void SurfaceCheck()
    {
        //Debug.Log("SurfaceCheck transform.TransformPoint(surfaceCheckOffset)" + transform.TransformPoint(surfaceCheckOffset));
        onSurface = Physics.CheckSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius, surfaceLayer);
        //이 개체의 상대적로컬 offset로컬좌표를 절대적 worldSpace좌표로 변환
    }

    void playerLedgeMovement()
    {
        float angle = Vector3.Angle(LedgeInfo.surfaceHit.normal, requiredMoveDir);

        if(angle < 90)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(surfaceCheckOffset), surfaceCheckRadius);
    }

    public IEnumerator PerformAction(string AnimationName,CompareTargetParameter ctp = null, Quaternion RequiredRotation = new Quaternion(), 
        bool LookAtObstacle=false,float ParkourActionDelay = 0f)
    {
        playerInAction = true;

        animator.CrossFadeInFixedTime(AnimationName, 0.2f);
        yield return null;

       // Debug.Log("PerformAction|AnimationName:" + AnimationName+ ",RequiredRotation:"+ RequiredRotation);

        var animationState = animator.GetNextAnimatorStateInfo(0);
        if (!animationState.IsName(AnimationName))
            Debug.Log("Animation Name is Incorrect");

        float rotateStartTime = (ctp != null) ? ctp.StartTime : 0f;
        float timerCounter = 0f;

        while (timerCounter < animationState.length)
        {
            timerCounter += Time.deltaTime;

            float normalizedTimerCounter = timerCounter / animationState.length;

            //make player to look towards the obstacle
            if (LookAtObstacle && normalizedTimerCounter > rotateStartTime)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, RequiredRotation, rotSpeed * Time.deltaTime);
            }

            if (ctp != null)
            {
                CompareTarget(ctp);
            }

            if (animator.IsInTransition(0) && timerCounter > 0.5f)
            {
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(ParkourActionDelay);

        playerInAction = false;
    }

    void CompareTarget(CompareTargetParameter compareTargetParameter)
    {
        animator.MatchTarget(compareTargetParameter.position, transform.rotation, compareTargetParameter.bodyPart,
            new MatchTargetWeightMask(compareTargetParameter.positionWeight, 0), compareTargetParameter.StartTime, compareTargetParameter.endTime);
    }

    public void SetControl(bool hasControl)
    {
        this.playerControl = hasControl;  
        CC.enabled = hasControl;

        if (!hasControl)
        {
            animator.SetFloat("movementValue", 0f);
            requiredRotation = transform.rotation;
        }
    }

    public void EnableCC(bool enabled)
    {
        CC.enabled = enabled;
    }
    public void ResetRequiredRotation()
    {
        requiredRotation = transform.rotation;
    }
    public bool HasPlayerControl
    {
        get => playerControl;
        set => playerControl = value;
    }
}

public class CompareTargetParameter
{
    public Vector3 position;
    public AvatarTarget bodyPart;
    public Vector3 positionWeight;
    public float StartTime;
    public float endTime;
}