using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerControls playerControls;
    
    // Движение (WASD)
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;

    // Прыжок
    [SerializeField] private float jumpHeight = 1.5f;

    // Гравитация
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravity = -2f; // небольшое притяжение к земле
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
    }

    void Start()
    {
        
    }

    void Update()
    {
        MoveCharacter();
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }

    private void ApplyGravity() // Гравитация
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void Jump()
    {
        if (characterController.isGrounded && playerControls.Player.Jump.WasPerformedThisFrame())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    private void MoveCharacter()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();

        float currentSpeed;
        if (playerControls.Player.Sprint.IsPressed())
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        // Движение по горизонтали
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f); // ограничиваем диагональное движение, чтобы оно не было быстрее прямого
        Vector3 horizontalMovement = moveDirection * currentSpeed;

        ApplyGravity();

        // Движение по вертикали
        Vector3 verticalMovement = new Vector3(0f, verticalVelocity, 0f);

        // Финальное движение по всем направлениям
        Vector3 finalMovement = horizontalMovement + verticalMovement;

        characterController.Move(finalMovement * Time.deltaTime);
    }
}
