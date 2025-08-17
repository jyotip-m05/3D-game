using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")] [SerializeField] private InputActionAsset inputActions;
    private float speed;
    [SerializeField] private float hspeed = 7f;
    [SerializeField] private float sspeed = 5f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Look Settings")] [SerializeField]
    private float lookSensitivity = 2f;

    [SerializeField] private Transform cameraTransform;
    private float cameraPitch = 0f;

    [Header("Input Actions")] InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _lookAction;
    InputAction _sprintAction;

    [Header("References")] private Rigidbody rb;

    [Header("Ground Check")] [SerializeField]
    private float groundCheckDistance = 0.1f;

    [SerializeField] private LayerMask groundMask;
    private bool isGrounded = true;
    
    bool sprinting = false;

    private void Awake()
    {
        // Find the references to the "Move" and "Jump" actions from the asset
        speed = sspeed;
        _moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        _jumpAction = inputActions.FindActionMap("Player").FindAction("Jump");
        _lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        _sprintAction = inputActions.FindActionMap("Player").FindAction("Sprint");
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _lookAction.Enable();
        _sprintAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _lookAction.Disable();
        _sprintAction.Disable();
    }

    void Update()
    {
        // Ground check
        // isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, groundMask);
        // Sprint
        CheckSprint();
        // Read the "Move" action value, which is a 2D vector
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        // Move relative to player orientation
        Vector3 moveDirection = transform.right * moveValue.x + transform.forward * moveValue.y;

        // Move using Rigidbody for better physics
        Vector3 velocity = moveDirection * speed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        if (_jumpAction.triggered && isGrounded)
        {
            Jump();
        }
        HandleLook();
    }
    
    private IEnumerator Sprint()
    {
        if (!sprinting)
        {
            sprinting = true;
            speed = hspeed;
            yield return new WaitForSeconds(1f);
            speed = sspeed;
            sprinting = false;
        }
    }

    private void CheckSprint()
    {
        if (_sprintAction.triggered)
        {
            StartCoroutine(Sprint());
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void HandleLook()
    {
        Vector2 lookValue = _lookAction.ReadValue<Vector2>() * lookSensitivity;

        // Rotate player left/right (yaw)
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, lookValue.x, 0f));

        // Camera up/down (pitch)
        cameraPitch -= lookValue.y;
        // cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);
        // if (cameraTransform is not null)
        // {
        //     cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        // }
    }
}