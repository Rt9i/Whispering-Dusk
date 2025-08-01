using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction sprintAction;

    [Header("Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    private readonly int animSpeedHash = Animator.StringToHash("Speed");
    private readonly int animMotionHash = Animator.StringToHash("MotionSpeed");

    private CharacterController controller;
    private Vector2 moveInput;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        // 1. Read inputs
        moveInput = moveAction.ReadValue<Vector2>();
        bool isSprinting = sprintAction.IsPressed();

        // 2. Move
        Vector3 dir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        dir = transform.TransformDirection(dir);
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        controller.Move(dir * targetSpeed * Time.deltaTime);

        // 3. Animate: pass horizontal speed and motion multiplier
        //    Speed: actual world speed for Blend Tree thresholds
        float horizontalSpeed = new Vector3(controller.velocity.x, 0f, controller.velocity.z).magnitude;
        animator.SetFloat(animSpeedHash, horizontalSpeed);

        //    MotionSpeed: relative input magnitude [0…1] controls playback rate
        float motionSpeed = moveInput.magnitude;
        animator.SetFloat(animMotionHash, motionSpeed);
    }
}
