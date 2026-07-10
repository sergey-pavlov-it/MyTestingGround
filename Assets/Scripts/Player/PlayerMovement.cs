using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerControls playerControls;
    private Animator playerAnimator;
   
    // Движение (WASD)
    private Vector2 moveInput;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float sprintSpeed = 5f;

    // Прыжок
    [SerializeField] private float jumpHeight = 1.0f;

    // Гравитация
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedGravity = -2f; // небольшое притяжение к земле
    private float verticalVelocity;

    // Поворот
    [SerializeField] private Transform rotationRoot; // для реализации поворота персонажа за камерой (при движении)
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float rotationSpeed = 10f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerControls = new PlayerControls();
        playerAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
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
        if (characterController.isGrounded && verticalVelocity < 0)
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
        if (characterController.isGrounded && playerControls.Player.Jump.WasPressedThisFrame())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }
    }

    private void MoveCharacter()
    {
        moveInput = playerControls.Player.Move.ReadValue<Vector2>();

        bool isLanding = IsLanding(); // если Animator сейчас проигрывает Jump_Land блокируем перемещение и повторный прыжок
        if (isLanding)
        {
            moveInput = Vector2.zero;
        }


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
        Vector3 cameraForward = cameraRoot.forward;
        Vector3 cameraRight = cameraRoot.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        Vector3 moveDirection = cameraForward.normalized * moveInput.y + cameraRight.normalized * moveInput.x;
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f); // ограничиваем диагональное движение, чтобы оно не было быстрее прямого
        
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            rotationRoot.rotation = Quaternion.Slerp(rotationRoot.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 horizontalMovement = moveDirection * currentSpeed;

        ApplyGravity();

        if (!isLanding)
        {
            Jump();
        }

        // Движение по вертикали
        Vector3 verticalMovement = new Vector3(0f, verticalVelocity, 0f);

        // Финальное движение по всем направлениям
        Vector3 finalMovement = horizontalMovement + verticalMovement;

        characterController.Move(finalMovement * Time.deltaTime);
    }

    private bool IsLanding() 
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        return stateInfo.IsName("Base Layer.Jump_Land:"); // возвращает True - Animator сейчас проигрывает Jump_Land
    }
}
