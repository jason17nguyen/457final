using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float laneSwitchSpeed = 14f;

    [Header("Jump Settings")]
    [SerializeField] private float gravity = -22f;
    [SerializeField] private float firstJumpHeight = 2.8f;
    [SerializeField] private float secondJumpHeight = 3.2f;
    [SerializeField] private float fastFallExtra = 100f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Slide")]
    [SerializeField] private float slideDuration = 0.7f;
    
    [Header("Slide Collider")]
    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField][Range(0.2f, 1f)] private float slideHeightMultiplier = 0.5f;

    private readonly float[] lanePositions = { -2f, 0f, 2f };
    private int currentLane = 1;
    private float targetX;

    private float groundY;
    private float verticalVelocity = 0f;
    private int jumpsUsed = 0;

    private bool isSliding = false;
    private float slideTimer = 0f;

    private const float groundTolerance = 0.01f;

    private float standingColliderHeight;
    private Vector3 standingColliderCenter;

    void Start()
    {
        targetX = lanePositions[currentLane];
        groundY = transform.position.y;

        Vector3 pos = transform.position;
        pos.x = targetX;
        pos.y = groundY;
        transform.position = pos;

        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }

        if (playerCollider != null)
        {
            standingColliderHeight = playerCollider.height;
            standingColliderCenter = playerCollider.center;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        if (animator != null)
        {
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsSliding", false);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return;

        if (Keyboard.current == null)
            return;

        HandleLaneInput();

        // Allow either action to interrupt the other:
        HandleSlideInput(); // Down can interrupt Jump (via AnyState->Slide)
        HandleJumpInput();  // Up can interrupt Slide

        ApplyVerticalMovement();
        ApplyHorizontalMovement();

        TickSlideTimer();
        UpdateAnimatorBools();
        AutoEndSlideIfAirborne();
    }

    void HandleLaneInput()
    {
        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            currentLane = Mathf.Clamp(currentLane - 1, 0, 2);
            targetX = lanePositions[currentLane];
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            currentLane = Mathf.Clamp(currentLane + 1, 0, 2);
            targetX = lanePositions[currentLane];
        }
    }

    void HandleJumpInput()
    {
        bool grounded = IsGrounded();

        if (grounded)
        {
            jumpsUsed = 0;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            // Jump should interrupt slide immediately.
            if (isSliding)
            {
                EndSlide();
            }

            if (jumpsUsed == 0)
            {
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * firstJumpHeight);
                jumpsUsed = 1;
                TriggerJumpAnimation();
            }
            else if (jumpsUsed == 1)
            {
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * secondJumpHeight);
                jumpsUsed = 2;
                TriggerJumpAnimation();
            }
        }
    }

    void HandleSlideInput()
    {
        if (Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            // Slide should interrupt jump too:
            // If airborne, we treat it like "slam down" + slide animation now.
            if (!IsGrounded())
            {
                // Force stronger downward motion immediately (so it "feels" like a slam).
                if (verticalVelocity > 0f)
                {
                    verticalVelocity = 0f;
                }
                verticalVelocity += (-fastFallExtra);
            }

            // Start/refresh slide even if already sliding (optional behavior).
            StartSlideOrRefresh();
        }
    }

    void ApplyVerticalMovement()
    {
        bool grounded = IsGrounded();

        // Hold down = extra fall speed while airborne
        float currentGravity = gravity;
        if (!grounded &&
            (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
        {
            currentGravity -= fastFallExtra;
        }

        verticalVelocity += currentGravity * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;

        if (pos.y <= groundY)
        {
            pos.y = groundY;
            verticalVelocity = 0f;
            jumpsUsed = 0;
        }

        transform.position = pos;
    }

    void ApplyHorizontalMovement()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, targetX, laneSwitchSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void TickSlideTimer()
    {
        if (!isSliding)
            return;

        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            EndSlide();
        }
    }

    void StartSlideOrRefresh()
    {
        isSliding = true;
        slideTimer = slideDuration;

        ApplySlideCollider();

        if (animator != null)
        {
            animator.ResetTrigger("Jump");
            animator.SetTrigger("Slide");
            animator.SetBool("IsSliding", true);
        }
    }

    void EndSlide()
    {
        isSliding = false;
        RestoreSlideCollider();

        if (animator != null)
        {
            animator.SetBool("IsSliding", false);
        }
    }

    void AutoEndSlideIfAirborne()
    {
        // Optional: if you don't want "sliding pose" while flying through the air,
        // end it as soon as we leave the ground.
        if (isSliding && !IsGrounded())
        {
            EndSlide();
        }
    }

    void TriggerJumpAnimation()
    {
        if (animator == null)
            return;

        animator.ResetTrigger("Slide");
        animator.SetTrigger("Jump");
    }

    void UpdateAnimatorBools()
    {
        if (animator == null)
            return;

        animator.SetBool("IsGrounded", IsGrounded());
        animator.SetBool("IsSliding", isSliding);
    }

    void ApplySlideCollider()
    {
        if (playerCollider == null)
            return;

        float newHeight = standingColliderHeight * slideHeightMultiplier;
        float standingBottom = standingColliderCenter.y - (standingColliderHeight * 0.5f);

        Vector3 newCenter = standingColliderCenter;
        newCenter.y = standingBottom + (newHeight * 0.5f);

        playerCollider.height = newHeight;
        playerCollider.center = newCenter;
    }

    void RestoreSlideCollider()
    {
        if (playerCollider == null)
            return;

        playerCollider.height = standingColliderHeight;
        playerCollider.center = standingColliderCenter;
    }

    bool IsGrounded()
    {
        return transform.position.y <= groundY + groundTolerance;
    }
}