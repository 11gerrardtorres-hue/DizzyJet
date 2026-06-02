using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 15f;
    public float rotateSpeed = 10f;

    private Vector2 moveInput;

    void Update()
    {
        HandleInput();
        Move();
        RotateToDirection();
    }

    void HandleInput()
    {
        moveInput = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed)  moveInput.x = -1f;
            if (keyboard.rightArrowKey.isPressed) moveInput.x = 1f;
            if (keyboard.upArrowKey.isPressed)    moveInput.y = 1f;
            if (keyboard.downArrowKey.isPressed)  moveInput.y = -1f;
        }

        var touchscreen = Touchscreen.current;
        if (touchscreen != null && touchscreen.primaryTouch.press.isPressed)
        {
            Vector2 touchPos = touchscreen.primaryTouch.position.ReadValue();
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            moveInput = (touchPos - screenCenter).normalized;
        }
    }

    void Move()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void RotateToDirection()
    {
        if (moveInput.magnitude < 0.1f) return;
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }
}