using UnityEngine;
using UnityEngine.InputSystem;



public class PlayerController : MonoBehaviour
{
   
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float acceleration = 60f;  
    [SerializeField] private float edgePadding = 0.1f;

    private Camera cam;
    private float halfShipWidth;
    private float currentVelocityX;

    private void Awake()
    {
        cam = Camera.main;
        var sr = GetComponentInChildren<SpriteRenderer>();
        halfShipWidth = sr != null ? sr.bounds.extents.x : 0.5f;
    }

    private void Update()
    {
        float input = ReadHorizontalInput();

        
        float targetVelocity = input * maxSpeed;
        //ease towards the target velocity through acceleration
        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocity, acceleration * Time.deltaTime);
        Vector3 pos = transform.position;
        //only operate in x
        pos.x += currentVelocityX * Time.deltaTime;
        //clamp to camera visible range
        float halfCamWidth = cam.orthographicSize * cam.aspect;
        float minX = cam.transform.position.x - halfCamWidth + halfShipWidth + edgePadding;
        float maxX = cam.transform.position.x + halfCamWidth - halfShipWidth - edgePadding;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        //kill leftover velocity
        if (pos.x <= minX && currentVelocityX < 0f) currentVelocityX = 0f;
        if (pos.x >= maxX && currentVelocityX > 0f) currentVelocityX = 0f;

        transform.position = pos;
    }
    //input system handling
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
