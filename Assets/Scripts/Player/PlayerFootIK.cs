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

    [Header("Foot Placement")]
    [SerializeField] private float footHeightOffset = 0.05f;
    [SerializeField] private float modelRotationOffsetY = 180f;

    [Header("Pelvis")]
    [SerializeField] private OverrideTransform pelvisConstraint;
    [SerializeField] private float maxPelvisDrop = 0.3f;
    [SerializeField] private float pelvisMoveSpeed = 10f;

    private Transform pelvisTarget;
    private Vector3 pelvisStartLocalPosition;
    private float currentPelvisOffset;

    private Quaternion leftFootBaseLocalRotation;
    private Quaternion rightFootBaseLocalRotation;

    private void Awake()
    {
        pelvisTarget = pelvisConstraint.data.sourceObject;
        pelvisStartLocalPosition = pelvisTarget.localPosition;

        // запоминаем начальные поворот стоп, для дальнешей реализации их поворота относительно поверхности
        Vector3 leftTargetRotation = leftFootTarget.localEulerAngles;
        leftTargetRotation.y += modelRotationOffsetY;
        leftFootTarget.localEulerAngles = leftTargetRotation;

        Vector3 rightTargetRotation = rightFootTarget.localEulerAngles;
        rightTargetRotation.y += modelRotationOffsetY;
        rightFootTarget.localEulerAngles = rightTargetRotation;

        leftFootBaseLocalRotation = leftFootTarget.localRotation;
        rightFootBaseLocalRotation = rightFootTarget.localRotation;
    }

    private void Update()
    {
        UpdateIKWeights();

        leftKneeHint.position = CalculateKneeHintPosition(leftKneeBone);
        rightKneeHint.position = CalculateKneeHintPosition(rightKneeBone);

        bool leftGroundFound = TryAlignFootToGround(leftFootBone, leftFootTarget, leftFootBaseLocalRotation, out RaycastHit leftHit);
        bool rightGroundFound = TryAlignFootToGround(rightFootBone, rightFootTarget, rightFootBaseLocalRotation, out RaycastHit rightHit);

        UpdatePelvisPosition(leftGroundFound, leftHit, rightGroundFound, rightHit);
    }

    private void UpdateIKWeights()
    {
        leftLegIK.weight = animator.GetFloat("LeftFootIK");
        rightLegIK.weight = animator.GetFloat("RightFootIK");
    }

    private Vector3 CalculateKneeHintPosition(Transform kneeBone)
    {
        return kneeBone.position + forwardReference.forward * kneeHintDistance;
    }

    private bool TryAlignFootToGround(Transform footBone, Transform footTarget, Quaternion baseLocalRotation, out RaycastHit hit)
    {
        footTarget.position = footBone.position;

        Vector3 rayOrigin = footBone.position + Vector3.up * rayStartHeight;

        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);

        bool groundFound = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore);

        if (groundFound)
        {
            Vector3 targetPosition = footBone.position;
            targetPosition.y = hit.point.y + footHeightOffset;
            footTarget.position = targetPosition;

            // Выпрямляем стопу относительно поверхности
            Vector3 footUp = hit.normal;
            Vector3 characterForward = forwardReference.forward;
            characterForward.y = 0;
            characterForward.Normalize();
            Vector3 footForward = Vector3.ProjectOnPlane(characterForward, footUp).normalized;
            Quaternion targetCalculatedRotation = Quaternion.LookRotation(footForward, footUp);
            footTarget.rotation = targetCalculatedRotation * baseLocalRotation;
        }

        return groundFound;
    }

    private void UpdatePelvisPosition(bool leftGroundFound, RaycastHit leftHit, bool rightGroundFound, RaycastHit rightHit)
    {
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

        desiredPelvisOffset = Mathf.Clamp(desiredPelvisOffset, -maxPelvisDrop, 0f);

        currentPelvisOffset = Mathf.MoveTowards(currentPelvisOffset, desiredPelvisOffset, pelvisMoveSpeed * Time.deltaTime);

        pelvisTarget.localPosition = pelvisStartLocalPosition + Vector3.up * currentPelvisOffset;
    }
}