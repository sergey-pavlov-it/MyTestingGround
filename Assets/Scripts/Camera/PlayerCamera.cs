using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private PlayerControls playerControls;

    private float yaw;

    [SerializeField] private float mouseSensitivity = 0.5f;

    private void Awake()
    {
        playerControls = new PlayerControls();

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
        yaw += lookInput.x * mouseSensitivity;

        transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }
}
