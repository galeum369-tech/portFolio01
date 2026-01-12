using System;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    PlayerInput pi;

    //연속 입력 액션 - 프로퍼티 노출(Move, Sprint, Block 등)
    public static Vector2 Input { get; private set; }           //이동 프로퍼티
    public static bool IsSprint { get; private set; }           //왼쪽 쉬프트 프로퍼티
    public static bool IsBlock {  get; private set; }           //우클릭 프로퍼티(방어)

    //카메라 바라보는 방향
    public static Vector2 Look { get; private set; }


    //1회성 액션 - 이벤트로 노출
    public static event Action OnAccess;      //상호작용 입력 이벤트
    public static event Action OnAttack;    //공격 입력 이벤트
    public static event Action OnRolling;   //구르기 입력 이벤트

    public static event Action OnWeaponNone; //비무장 1번키
    public static event Action OnWeaponAxe;  //도끼 2번키
    public static event Action OnWeaponSword;//검 3번키
    public static event Action OnMasic; //마법 4번키


    //Input Sysyem의 액션들
    InputAction moveAction;
    InputAction sprintAction;
    InputAction blockAction;
    InputAction accessAction;
    InputAction attackAction;
    InputAction rollingAction;
    InputAction weaponNoneAction;
    InputAction weaponAxeAction;
    InputAction weaponSwordAction;
    InputAction masicAction;
    InputAction lookAction;


    //콜백 함수들을 저장할 변수들
    Action<InputAction.CallbackContext> onMovePerformed;
    Action<InputAction.CallbackContext> onMoveCanceled;
    Action<InputAction.CallbackContext> onSprintPerformed;
    Action<InputAction.CallbackContext> onSprntCanceled;
    Action<InputAction.CallbackContext> onBlockPerformed;
    Action<InputAction.CallbackContext> onBlockCanceled;
    Action<InputAction.CallbackContext> onAccessPerformed;
    Action<InputAction.CallbackContext> onAttackPerformed;
    Action<InputAction.CallbackContext> onRollingPerformed;
    Action<InputAction.CallbackContext> onWeaponNonePerformed;
    Action<InputAction.CallbackContext> onWeaponAxePerformed;
    Action<InputAction.CallbackContext> onWeaponSwordPerformed;
    Action<InputAction.CallbackContext> onMasicPerformed;

    Action<InputAction.CallbackContext> onLookPerformed;
    Action<InputAction.CallbackContext> onLookCanceled;



    void OnEnable()
    {
        //PlayerInput 컴포넌트 초기화
        pi = GetComponent<PlayerInput>();
        pi.defaultActionMap = "Player";
        pi.defaultControlScheme = "Keboard&Mouse";  // "Default"
        pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        //각각의 액션들 찾기
        moveAction = pi.actions.FindAction("Move");
        sprintAction = pi.actions.FindAction("Sprint");
        blockAction = pi.actions.FindAction("Block");
        accessAction = pi.actions.FindAction("Access");
        attackAction = pi.actions.FindAction("Attack");
        rollingAction = pi.actions.FindAction("Rolling");
        weaponNoneAction = pi.actions.FindAction("WeaponNone");
        weaponAxeAction = pi.actions.FindAction("WeaponAxe");
        weaponSwordAction = pi.actions.FindAction("WeaponSword");
        masicAction = pi.actions.FindAction("Masic");

        lookAction = pi.actions.FindAction("Look");

        //무브 액션 콜백등록
        if (moveAction != null)
        {
            onMovePerformed = ctx => Input = ctx.ReadValue<Vector2>();
            onMoveCanceled = ctx => Input = Vector2.zero;
            moveAction.performed += onMovePerformed;
            moveAction.canceled += onMoveCanceled;
        }
        //스프린트 액션 콜백등록
        if (sprintAction != null)
        {
            onSprintPerformed = ctx => IsSprint = true;
            onSprntCanceled = ctx => IsSprint = false;
            sprintAction.performed += onSprintPerformed;
            sprintAction.canceled += onSprntCanceled;
        }
        //블록 액션 콜백등록
        if (blockAction != null)
        {
            onBlockPerformed = ctx => IsBlock = true;
            onBlockCanceled = ctx => IsBlock = false;
            blockAction.performed += onBlockPerformed;
            blockAction.canceled += onBlockCanceled;
        }
        //점프 액션 콜백등록
        if (accessAction != null)
        {
            onAccessPerformed = ctx => OnAccess?.Invoke();
            accessAction.performed += onAccessPerformed;
        }
        //공격액션 콜백등록
        if (attackAction != null)
        {
            onAttackPerformed = ctx => OnAttack?.Invoke();
            attackAction.performed += onAttackPerformed;
        }
        //구르기액션 콜백등록
        if (rollingAction != null)
        {
            onRollingPerformed = ctx => OnRolling?.Invoke();
            rollingAction.performed += onRollingPerformed;
        }
        //비무장 1번 콜백
        if (weaponNoneAction != null)
        {
            onWeaponNonePerformed = ctx => OnWeaponNone?.Invoke();
            weaponNoneAction.performed += onWeaponNonePerformed;
        }
        //도끼 2번 콜백
        if (weaponAxeAction != null)
        {
            onWeaponAxePerformed = ctx => OnWeaponAxe?.Invoke();
            weaponAxeAction.performed += onWeaponAxePerformed;
        }
        //검 3번 콜백
        if (weaponSwordAction != null)
        {
            onWeaponSwordPerformed = ctx => OnWeaponSword?.Invoke();
            weaponSwordAction.performed += onWeaponSwordPerformed;
        }
        //마법 4번 콜백
        if (masicAction != null)
        {
            onMasicPerformed = ctx => OnMasic?.Invoke();
            masicAction.performed += onMasicPerformed;
        }

        if (lookAction != null)
        {
            onLookPerformed = ctx => Look = ctx.ReadValue<Vector2>();
            onLookCanceled = ctx => Look = Vector2.zero;

            lookAction.performed += onLookPerformed;
            lookAction.canceled += onLookCanceled;
        }
    }
}
