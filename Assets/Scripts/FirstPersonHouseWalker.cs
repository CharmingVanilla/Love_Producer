using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonHouseWalker : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.2f;
    [SerializeField] private float runSpeed = 5.5f;
    [SerializeField] private float jumpHeight = 1.1f;
    [SerializeField] private float gravity = -18f;

    [Header("Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float eyeHeight = 1.65f;

    private CharacterController controller;
    private float verticalSpeed;
    private float pitch;
    private bool cursorCaptured = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraPivot == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null) cameraPivot = childCamera.transform;
        }

        if (cameraPivot != null)
            cameraPivot.localPosition = new Vector3(0, eyeHeight, 0);
    }

    private void Start()
    {
        SetCursorCaptured(true);
    }

    private void Update()
    {
        HandleCursor();
        HandleLook();
        HandleMovement();
    }

    private void HandleCursor()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorCaptured(false);

        if (!cursorCaptured && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            SetCursorCaptured(true);
    }

    private void HandleLook()
    {
        if (!cursorCaptured || Mouse.current == null || cameraPivot == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(Vector3.up, mouseDelta.x);

        pitch = Mathf.Clamp(pitch - mouseDelta.y, -82f, 82f);
        cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null) return;

        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        input = Vector2.ClampMagnitude(input, 1);

        bool running = Keyboard.current.leftShiftKey.isPressed ||
                       Keyboard.current.rightShiftKey.isPressed;
        float speed = running ? runSpeed : walkSpeed;
        Vector3 horizontal = (transform.forward * input.y + transform.right * input.x) * speed;

        if (controller.isGrounded)
        {
            if (verticalSpeed < 0) verticalSpeed = -2f;
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            verticalSpeed += gravity * Time.deltaTime;
        }

        Vector3 velocity = horizontal + Vector3.up * verticalSpeed;
        controller.Move(velocity * Time.deltaTime);
    }

    private void SetCursorCaptured(bool captured)
    {
        cursorCaptured = captured;
        Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !captured;
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
