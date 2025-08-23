using System;
using System.Collections;
using System.Text.RegularExpressions;
using Script;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    [Header("Stats")] [SerializeField] private float range = 20f;
    [SerializeField] private LayerMask layerMask;

    [Header("Movement")] [SerializeField] private InputActionAsset inputActions;
    private float speed;
    [SerializeField] private float hspeed = 7f;
    [SerializeField] private float sspeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Look Settings")] [SerializeField]
    private float lookSensitivity = 2f;
    [SerializeField] private ParticleSystem muzzleFlash;

    [SerializeField] private Transform cameraTransform;
    private float cameraPitch = 0f;

    [Header("Input Actions")] InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _lookAction;
    InputAction _sprintAction;
    InputAction _attackAction;

    [Header("References")] private Rigidbody rb;
    public int coinsCollected { get; private set; } = 0;
    public int killCount { get; private set; } = 0;
    public float health { get; private set; } = 100f;
    private Coroutine waitCoroutine = null;

    [Header("Ground Check")] [SerializeField]
    private float groundCheckDistance = 0.1f;

    [SerializeField] private LayerMask groundMask;
    private bool isGrounded = true;

    [Header("Display")] [SerializeField] private TMPro.TextMeshProUGUI coinsText;
    [SerializeField] private TMPro.TextMeshProUGUI killCountText;
    [SerializeField] private TMPro.TextMeshProUGUI healthText;

    bool sprinting = false;
    bool attacking = false;

    private float lastHitTime;

    [SerializeField] private float hitCooldown = 0.5f;

    [SerializeField] private float lastRecTime = 0.5f;

    private bool canShoot = true;

    [SerializeField] private float fireRate = 0.01f;

    private Animator animator;
    // private LayerMask enemyMask;

    private void Awake()
    {
        // Find the references to the "Move" and "Jump" actions from the asset
        speed = sspeed;
        _moveAction = inputActions.FindActionMap("Player").FindAction("Move");
        _jumpAction = inputActions.FindActionMap("Player").FindAction("Jump");
        _lookAction = inputActions.FindActionMap("Player").FindAction("Look");
        _sprintAction = inputActions.FindActionMap("Player").FindAction("Sprint");
        _attackAction = inputActions.FindActionMap("Player").FindAction("Attack");
        rb = GetComponent<Rigidbody>();
        coinsText.text = "Coins: " + coinsCollected;
        killCountText.text = "Kills: " + killCount;
        healthText.text = "Health: " + health;
        lastHitTime = Time.time;
        layerMask = LayerMask.GetMask("Enemy");
        // enemyMask = LayerMask.GetMask("Enemy");
        muzzleFlash.Stop();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);
        // Debug.Log(enemyMask);
        // Debug.Log(other.transform.parent.name);
        if (other.transform.parent.name == "Coins")
        {
            coinsCollected++;
            Destroy(other.transform.gameObject);
            coinsText.text = "Coins: " + coinsCollected;
        }
        // other.transform.parent.name, "^Enemy \\(\\d+\\)$")
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy") && Time.time > lastHitTime + hitCooldown)
        {
            health -= 1f;
            if (animator = other.GetComponent<Animator>())
            {
                animator.SetBool(IsAttacking, true);
            }

            lastHitTime = Time.time;
            healthText.text = "Health: " + (Mathf.RoundToInt(health * 10)) / 10f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        animator = other.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool(IsAttacking, false);
        }
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _lookAction.Enable();
        _sprintAction.Enable();
        _attackAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _lookAction.Disable();
        _sprintAction.Disable();
        _attackAction.Disable();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 2f, groundMask);
        // Debug ray for ground check
        Debug.DrawRay(transform.position, Vector3.down * (groundCheckDistance + 2f),
            isGrounded ? Color.green : Color.red);
        // Sprint
        CollectCoins();
        CheckSprint();
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
        Recovery();
        if (_attackAction.triggered)
        {
            Shoot();
        }
    }

    private IEnumerator Sprint()
    {
        if (!sprinting)
        {
            sprinting = true;
            speed = hspeed;
            yield return new WaitForSeconds(5f); //sprint duration
            speed = sspeed;
            yield return new WaitForSeconds(1f); // sprint cooldown
            sprinting = false;
        }
    }

    private void CollectCoins()
    {
        //
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

    private void Recovery()
    {
        if (health < 100f && lastHitTime + hitCooldown < Time.time && lastRecTime + hitCooldown < Time.time)
        {
            health += 0.5f;
            healthText.text = "Health: " + (Mathf.RoundToInt(health * 10)) / 10f;
            lastRecTime = Time.time;
        }
    }

    private void HandleLook()
    {
        Vector2 lookValue = _lookAction.ReadValue<Vector2>() * lookSensitivity;

        // Rotate player left/right (yaw)
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, lookValue.x, 0f));

        // Camera up/down (pitch)
        cameraPitch -= lookValue.y;
        cameraPitch = Mathf.Clamp(cameraPitch, -45f, 45f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Shoot()
    {
        // if (!canShoot)
        // {
        //     return;
        // }
        // Debug.DrawRay(cameraTransform.position, cameraTransform.forward, Color.cyan, 2f, true);
        StartCoroutine(Muzzle());
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, range, layerMask))
        {
            Debug.Log($"Raycast hit: {hit.transform.name}, Tag: {hit.transform.tag}, Layer: {LayerMask.LayerToName(hit.transform.gameObject.layer)}");
            if (hit.transform.CompareTag("Enemy"))
            {
                EnemyAI enemyAI = hit.transform.GetComponentInParent<EnemyAI>();
                if (enemyAI)
                {
                    Debug.Log($"Starting Death coroutine for {hit.transform.name}, Enemy State: {enemyAI?.currentState}");
                    StartCoroutine(enemyAI.Death());
                    killCount++;
                    killCountText.text = "Kills: " + killCount;
                }
            }
        }
    }

    IEnumerator Muzzle()
    {
        attacking = true;
        muzzleFlash.Play();
        yield return new WaitForSeconds(0.1f);
        muzzleFlash.Stop();
        yield return new WaitForSeconds(0.1f);
        attacking = false;
    }

    IEnumerator Wait()
    {
        canShoot = false;
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
        waitCoroutine = null;
    }
}