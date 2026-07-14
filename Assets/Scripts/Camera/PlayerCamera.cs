using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerControls playerControls;

    private float cameraHorizontalAngle;
    private float cameraVerticalAngle;

    [SerializeField] private GameObject cameraTarget;
    [SerializeField] private float mouseSensitivity = 0.5f;

    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 30f;

    private void Awake()
    {
        playerControls = new PlayerControls();

        transform.localRotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
        cameraTarget.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);

        // Блокируем курсор по центру и скрываем
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
    }

    private void Update()
    {
        Vector2 lookInput = playerControls.Player.Look.ReadValue<Vector2>();

        // Горизонтальный осмотр
        cameraHorizontalAngle += lookInput.x * mouseSensitivity;

        // Вертикальный осмотр
        cameraVerticalAngle -= lookInput.y * mouseSensitivity;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(0f, cameraHorizontalAngle, 0f);
        cameraTarget.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }
}
