using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Player ship: moves ONLY along X and is clamped to the camera bounds.
/// Works with both the legacy Input Manager and the new Input System (Unity 6 default).
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 60f;   // units/sec^2 toward the target speed

    [Header("Bounds")]
    [Tooltip("Extra margin kept between the ship's edge and the screen edge.")]
    [SerializeField] private float edgePadding = 0.1f;

    private Camera cam;
    private float halfShipWidth;
    private float currentVelocityX;

    private void Awake()
    {
        cam = Camera.main;

        // Use the sprite's width so the whole ship stays on screen, not just its pivot.
        var sr = GetComponentInChildren<SpriteRenderer>();
        halfShipWidth = sr != null ? sr.bounds.extents.x : 0.5f;
    }

    private void Update()
    {
        float input = ReadHorizontalInput();

        // Accelerate smoothly toward the desired velocity (vector arithmetic on a single axis).
        float targetVelocity = input * maxSpeed;
        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocity, acceleration * Time.deltaTime);

        // Build the new position. Y and Z are copied unchanged -> movement only in X.
        Vector3 pos = transform.position;
        pos.x += currentVelocityX * Time.deltaTime;

        // Clamp to the camera's visible horizontal range.
        float halfCamWidth = cam.orthographicSize * cam.aspect;
        float minX = cam.transform.position.x - halfCamWidth + halfShipWidth + edgePadding;
        float maxX = cam.transform.position.x + halfCamWidth - halfShipWidth - edgePadding;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        // Kill leftover velocity when pressed against a wall so it doesn't "stick".
        if (pos.x <= minX && currentVelocityX < 0f) currentVelocityX = 0f;
        if (pos.x >= maxX && currentVelocityX > 0f) currentVelocityX = 0f;

        transform.position = pos;
    }

    private static float ReadHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        float x = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        }
        return x;
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }
}
