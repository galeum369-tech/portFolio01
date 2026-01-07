using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    Animator anim;
    CharacterController cc;

    public Transform cameraRoot;

    public bool IsLockOn = false;

    float walkSpeed = 3;
    float runSpeed = 6;

    //Animator Hash ID
    int hashMoveX;
    int hashMoveY;
    int hashBlock;
    int hashJump;
    int hashAttack;
    int hashRolling;

    void Awake()
    {
        //컴포넌트 참조 초기화
        anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();

        //Animator Hash Init
        hashMoveX = Animator.StringToHash("moveX");
        hashMoveY = Animator.StringToHash("moveY");
        hashBlock = Animator.StringToHash("Block");
        hashJump = Animator.StringToHash("Jump");
        hashAttack = Animator.StringToHash("Attack");
        hashRolling = Animator.StringToHash("Rolling");
    }
   

    private void OnEnable()
    {
        //InputManager 이벤트 등록
        //1회성 입력되는 액션만 등록
        InputManager.OnAttack += HandleAttack;
        InputManager.OnJump += HandleJump;
        InputManager.OnRolling += HandleRolling;
    }
    private void OnDisable()
    {
        //InputManager 이벤트 해제
        //1회성 입력되는 액션만 해제
        InputManager.OnAttack -= HandleAttack;
        InputManager.OnJump -= HandleJump;
        InputManager.OnRolling -= HandleRolling;
    }

    void Update()
    {
        //플레이어 이동
        PlayerMove(InputManager.Input, InputManager.IsSprint);
        Block(InputManager.IsBlock);
    }



    /// <summary>
    /// 플레이어 이동처리
    /// </summary>
    /// <param name="input"></param>
    /// <param name="isLeftShiftPressed"></param>
    void PlayerMove(Vector2 input, bool isLeftShiftPressed)
    {
        if (input.magnitude < 0.1f)
        {
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 0f);
            return;
        }

        // 🔥 카메라 기준 이동 방향 계산
        Vector3 camForward = cameraRoot.forward;
        Vector3 camRight = cameraRoot.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * input.y + camRight * input.x;
        moveDir.Normalize();

        // 이동
        float curSpeed = isLeftShiftPressed ? runSpeed : walkSpeed;
        cc.Move(moveDir * curSpeed * Time.deltaTime);

        // 캐릭터 회전 (이동 방향으로만)
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 10f
        );

        float animSpeedMul = isLeftShiftPressed ? 2f : 1f;

        if (!IsLockOn)
        {
            // 🔥 비락온: 어떤 키든 전진 애니메이션
            anim.SetFloat(hashMoveX, 0f);
            anim.SetFloat(hashMoveY, 1f * animSpeedMul);
        }
        else
        {
            // 🔥 락온: 캐릭터 기준 스트레이프
            Vector3 localMoveDir = transform.InverseTransformDirection(moveDir);
            anim.SetFloat(hashMoveX, localMoveDir.x);
            anim.SetFloat(hashMoveY, localMoveDir.z * animSpeedMul);
        }
    }



    void Block(bool isBlocked)
    {
        anim.SetBool(hashBlock, isBlocked);
    }

    void HandleJump()
    {
        print("Jump");
        anim.SetTrigger(hashJump);
    }
    void HandleAttack()
    {
        print("Attack");
        anim.SetTrigger(hashAttack);
    }
    void HandleRolling()
    {
        print("Rolling");
        anim.SetTrigger(hashRolling);
    }

}
