using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Lane Movement")]
    [SerializeField] private float laneSwitchSpeed = 14f;

    [Header("Jump Settings")]
    [SerializeField] private float gravity = -18f;
    [SerializeField] private float firstJumpHeight = 1f;
    [SerializeField] private float secondJumpHeight = 1.5f;
    [SerializeField] private float fastFallExtra = 100f;
    private readonly float[] lanePositions = { -2f, 0f, 2f };
    private int currentLane = 1;
    private float targetX;

    private float groundY;
    private float verticalVelocity = 0f;
    private int jumpsUsed = 0;

    private const float groundTolerance = 0.01f;

    void Start()
    {
        targetX = lanePositions[currentLane];
        groundY = transform.position.y;

        Vector3 pos = transform.position;
        pos.x = targetX;
        pos.y = groundY;
        transform.position = pos;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        HandleLaneInput();
        HandleJumpInput();
        ApplyVerticalMovement();
        ApplyHorizontalMovement();
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
            // First jump
            if (jumpsUsed == 0)
            {
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * firstJumpHeight);
                jumpsUsed = 1;
            }
            // Double jump
            else if (jumpsUsed == 1)
            {
                verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * secondJumpHeight);
                jumpsUsed = 2;
            }
        }
    }

    void ApplyVerticalMovement()
    {
        bool grounded = IsGrounded();

        // Extra downward acceleration while in air if holding down / S
        float currentGravity = gravity;
        if (!grounded &&
            (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed))
        {
            currentGravity -= fastFallExtra;
        }

        verticalVelocity += currentGravity * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y += verticalVelocity * Time.deltaTime;

        // Land on the ground cleanly
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

    bool IsGrounded()
    {
        return transform.position.y <= groundY + groundTolerance;
    }
}