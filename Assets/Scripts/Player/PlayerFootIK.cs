using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerFootIK : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TwoBoneIKConstraint leftLegIK;
    [SerializeField] private TwoBoneIKConstraint rightLegIK;

    [Header("Left Leg")]
    [SerializeField] private Transform leftFootBone;
    [SerializeField] private Transform leftFootTarget;
    [SerializeField] private Transform leftKneeBone;
    [SerializeField] private Transform leftKneeHint;

    [Header("Right Leg")]
    [SerializeField] private Transform rightFootBone;
    [SerializeField] private Transform rightFootTarget;
    [SerializeField] private Transform rightKneeBone;
    [SerializeField] private Transform rightKneeHint;

    [Header("Knee Settings")]
    [SerializeField] private Transform forwardReference;
    [SerializeField] private float kneeHintDistance = 0.3f;

    [Header("Ground Detection")]
    [SerializeField] private float rayStartHeight = 0.5f;
    [SerializeField] private float rayLength = 1.0f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private float footHeightOffset = 0.05f;

    [Header("Pelvis")]
    [SerializeField] private OverrideTransform pelvisConstraint;
    [SerializeField] private float maxPelvisDrop = 0.3f;
    [SerializeField] private float pelvisMoveSpeed = 10f;

    private Transform pelvisTarget;
    private Vector3 pelvisStartLocalPosition;
    private float currentPelvisOffset;


    private void Awake()
    {
        pelvisTarget = pelvisConstraint.data.sourceObject;
        pelvisStartLocalPosition = pelvisTarget.localPosition;
    }

    private void Update()
    {
        leftLegIK.weight = animator.GetFloat("LeftFootIK");
        rightLegIK.weight = animator.GetFloat("RightFootIK");

        leftFootTarget.SetPositionAndRotation(leftFootBone.position, leftFootBone.rotation);
        rightFootTarget.SetPositionAndRotation(rightFootBone.position, rightFootBone.rotation);

        leftKneeHint.position = leftKneeBone.position + forwardReference.forward * kneeHintDistance;
        rightKneeHint.position = rightKneeBone.position + forwardReference.forward * kneeHintDistance;

        // Показывает луч (для визуала)
        Vector3 leftRayOrigin = leftFootBone.position + Vector3.up * rayStartHeight;
        Vector3 rightRayOrigin = rightFootBone.position + Vector3.up * rayStartHeight;
        Debug.DrawRay(leftRayOrigin, Vector3.down * rayLength, Color.red);
        Debug.DrawRay( rightRayOrigin, Vector3.down * rayLength, Color.red);

        bool leftGroundFound = Physics.Raycast(leftRayOrigin, Vector3.down, out RaycastHit leftHit, rayLength, groundLayer, QueryTriggerInteraction.Ignore);
        bool rightGroundFound = Physics.Raycast(rightRayOrigin, Vector3.down, out RaycastHit rightHit, rayLength, groundLayer, QueryTriggerInteraction.Ignore);

        if (leftGroundFound)
        {
            Vector3 leftTargetPosition = leftFootBone.position;
            leftTargetPosition.y = leftHit.point.y + footHeightOffset;
            leftFootTarget.position = leftTargetPosition;
        }

        if (rightGroundFound)
        {
            Vector3 rightTargetPosition = rightFootBone.position;
            rightTargetPosition.y = rightHit.point.y + footHeightOffset;
            rightFootTarget.position = rightTargetPosition;
        }

        float desiredPelvisOffset = 0f;

        if (leftGroundFound)
        {
            float leftGroundDifference = leftHit.point.y - transform.position.y;

            desiredPelvisOffset = Mathf.Min(desiredPelvisOffset, leftGroundDifference);
        }

        if (rightGroundFound)
        {
            float rightGroundDifference = rightHit.point.y - transform.position.y;

            desiredPelvisOffset = Mathf.Min(desiredPelvisOffset, rightGroundDifference);
        }

        desiredPelvisOffset = Mathf.Clamp(
            desiredPelvisOffset,
            -maxPelvisDrop,
            0f
        );

        currentPelvisOffset = Mathf.MoveTowards(
            currentPelvisOffset,
            desiredPelvisOffset,
            pelvisMoveSpeed * Time.deltaTime
        );

        pelvisTarget.localPosition =
            pelvisStartLocalPosition + Vector3.up * currentPelvisOffset;
    }

    private void LateUpdate()
    {

    }
}
