using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class LoveProducerOrbitCameraController : MonoBehaviour
{
    [Header("Orbit Target")]
    [SerializeField] private Vector3 target = new Vector3(0f, 0.5f, 0.8f);

    [Header("Default View")]
    [SerializeField] private float yaw;
    [SerializeField] private float pitch = 55f;
    [SerializeField] private float distance = 40f;

    [Header("Limits")]
    [SerializeField] private Vector2 pitchLimits = new Vector2(35f, 75f);
    [SerializeField] private Vector2 distanceLimits = new Vector2(25f, 55f);

    [Header("Controls")]
    [SerializeField] private float mouseRotateSpeed = 0.18f;
    [SerializeField] private float keyboardRotateSpeed = 55f;
    [SerializeField] private float zoomSpeed = 2.4f;
    [SerializeField] private bool allowAtNight;
    [SerializeField] private bool showControlHint = true;

    private float _defaultYaw;
    private float _defaultPitch;
    private float _defaultDistance;
    private LoveProducerDayNightController _dayNight;
    private GUIStyle _hintStyle;

    public bool AllowAtNight
    {
        get => allowAtNight;
        set => allowAtNight = value;
    }

    private void Awake()
    {
        _defaultYaw = yaw;
        _defaultPitch = pitch;
        _defaultDistance = distance;
        _dayNight = FindAnyObjectByType<LoveProducerDayNightController>();
        ApplyCameraTransform();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || !CanControl()) return;

        Vector2 drag = ReadOrbitDrag();
        yaw += drag.x * mouseRotateSpeed;
        pitch = Mathf.Clamp(pitch - drag.y * mouseRotateSpeed, pitchLimits.x, pitchLimits.y);

        float keyboardDirection = ReadKeyboardRotation();
        yaw += keyboardDirection * keyboardRotateSpeed * Time.unscaledDeltaTime;

        float scroll = ReadScroll();
        distance = Mathf.Clamp(distance - scroll * zoomSpeed, distanceLimits.x, distanceLimits.y);

        if (ResetPressed()) ResetView();
        ApplyCameraTransform();
    }

    public void ConfigureDefaultView(Vector3 orbitTarget, float defaultYaw, float defaultPitch, float defaultDistance)
    {
        target = orbitTarget;
        yaw = defaultYaw;
        pitch = Mathf.Clamp(defaultPitch, pitchLimits.x, pitchLimits.y);
        distance = Mathf.Clamp(defaultDistance, distanceLimits.x, distanceLimits.y);
        _defaultYaw = yaw;
        _defaultPitch = pitch;
        _defaultDistance = distance;
        ApplyCameraTransform();
    }

    public void ResetView()
    {
        yaw = _defaultYaw;
        pitch = _defaultPitch;
        distance = _defaultDistance;
        ApplyCameraTransform();
    }

    private bool CanControl()
    {
        if (allowAtNight) return true;
        if (_dayNight == null) _dayNight = FindAnyObjectByType<LoveProducerDayNightController>();
        return _dayNight == null || !_dayNight.IsNight;
    }

    private void ApplyCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = target + rotation * new Vector3(0f, 0f, -distance);
        transform.rotation = rotation;
    }

    private static Vector2 ReadOrbitDrag()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.rightButton.isPressed ? mouse.delta.ReadValue() : Vector2.zero;
#else
        return Input.GetMouseButton(1) ? new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 12f : Vector2.zero;
#endif
    }

    private static float ReadScroll()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null ? mouse.scroll.ReadValue().y / 120f : 0f;
#else
        return Input.mouseScrollDelta.y;
#endif
    }

    private static float ReadKeyboardRotation()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return 0f;
        return (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);
#else
        return (Input.GetKey(KeyCode.E) ? 1f : 0f) - (Input.GetKey(KeyCode.Q) ? 1f : 0f);
#endif
    }

    private static bool ResetPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    private void OnGUI()
    {
        if (!Application.isPlaying || !showControlHint || !CanControl()) return;
        if (_hintStyle == null)
        {
            _hintStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
        }
        GUI.Box(new Rect(Screen.width - 345f, 18f, 325f, 38f),
            "Right-drag: Orbit   Wheel: Zoom   Q/E: Rotate   R: Reset", _hintStyle);
    }
}
