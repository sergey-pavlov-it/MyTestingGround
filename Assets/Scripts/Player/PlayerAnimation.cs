using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator playerAnimator;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerAnimator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        Vector3 horizontalVelocity = characterController.velocity;
        horizontalVelocity.y = 0f;

        float speed = horizontalVelocity.magnitude;

        playerAnimator.SetFloat("Speed", speed);
        playerAnimator.SetBool("IsGrounded", characterController.isGrounded);
        playerAnimator.SetFloat("VerticalVelocity", characterController.velocity.y);
    }
}
