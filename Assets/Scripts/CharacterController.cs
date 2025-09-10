using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpForce = 5f;
    public float backwardsSpeed = 3f;
    
    [Header("Animation Settings")]
    public float acceleration = 10f;
    public float deceleration = 5f;
    
    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private float currentSpeed;
    private bool isJumping = false;
    private bool isRunning = false;
    private bool isTurningLeft = false;
    private bool isTurningRight = false;
    private bool isMovingBackwards = false;
    private bool isGrounded;
    
    // Animation parameters
    private int speedHash = Animator.StringToHash("Speed");
    private int isRunningHash = Animator.StringToHash("IsRunning");
    private int isJumpingHash = Animator.StringToHash("IsJumping");
    private int turnLeftHash = Animator.StringToHash("TurnLeft");
    private int turnRightHash = Animator.StringToHash("TurnRight");
    private int isMovingBackwardsHash = Animator.StringToHash("IsMovingBackwards");
    private int directionHash = Animator.StringToHash("Direction");
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        currentSpeed = 0f;
    }
    
    void Update()
    {
        HandleInput();
        UpdateAnimationParameters();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }
    
    void HandleInput()
    {
        // Movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        // Calculate movement direction
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Check if moving backwards
        isMovingBackwards = (vertical < 0 && horizontal == 0);
        
        // Running
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        // Turning
        isTurningLeft = Input.GetKey(KeyCode.Q);
        isTurningRight = Input.GetKey(KeyCode.E);
        
        // Jumping (only when grounded)
        if (Input.GetButtonDown("Jump") && isGrounded && !isTurningLeft && !isTurningRight)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    
    void HandleMovement()
    {
        if (moveDirection != Vector3.zero && !isJumping)
        {
            // Calculate target speed
            float targetSpeed = isMovingBackwards ? backwardsSpeed : 
                               isRunning ? runSpeed : moveSpeed;
            
            // Smoothly accelerate/decelerate
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.fixedDeltaTime * acceleration);
            
            // Move character
            Vector3 movement = moveDirection * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movement);
            
            // Rotate character
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
        }
        else
        {
            // Decelerate when not moving
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.fixedDeltaTime * deceleration);
        }
    }
    
    void HandleJump()
    {
        // Check if landed
        if (isJumping && isGrounded)
        {
            isJumping = false;
            animator.SetBool(isJumpingHash, false);
        }
    }
    
    void UpdateAnimationParameters()
    {
        // Update speed parameter (0-1 range)
        animator.SetFloat(speedHash, currentSpeed / moveSpeed);
        
        // Update other parameters
        animator.SetBool(isRunningHash, isRunning);
        animator.SetBool(isJumpingHash, isJumping);
        animator.SetBool(turnLeftHash, isTurningLeft);
        animator.SetBool(turnRightHash, isTurningRight);
        animator.SetBool(isMovingBackwardsHash, isMovingBackwards);
        
        // Update direction (0-7 for 8 directions)
        if (moveDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            int direction = Mathf.RoundToInt(angle / 45) % 8;
            if (direction < 0) direction += 8;
            animator.SetInteger(directionHash, direction);
        }
    }
    
    // Ground detection
    void OnCollisionEnter(Collision collision)
    {
        CheckGrounded(collision);
    }
    
    void OnCollisionStay(Collision collision)
    {
        CheckGrounded(collision);
    }
    
    void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
    
    void CheckGrounded(Collision collision)
    {
        if (collision.contacts.Length > 0)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Check if collision normal is pointing up (ground is below)
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }
    
    // Animation event handlers
    public void OnLanding()
    {
        isJumping = false;
        animator.SetBool(isJumpingHash, false);
    }
    
    public void OnTurnComplete()
    {
        isTurningLeft = false;
        isTurningRight = false;
        animator.SetBool(turnLeftHash, false);
        animator.SetBool(turnRightHash, false);
    }
}