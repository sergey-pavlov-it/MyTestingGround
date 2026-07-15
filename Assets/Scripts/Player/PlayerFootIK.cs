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
        UpdateIKWeights();

        leftKneeHint.position = CalculateKneeHintPosition(leftKneeBone);
        rightKneeHint.position = CalculateKneeHintPosition(rightKneeBone);

        bool leftGroundFound = TryPlaceFootOnGround(leftFootBone, leftFootTarget, out RaycastHit leftHit);
        bool rightGroundFound = TryPlaceFootOnGround(rightFootBone, rightFootTarget, out RaycastHit rightHit);

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

    private bool TryPlaceFootOnGround(Transform footBone, Transform footTarget, out RaycastHit hit)
    {
        footTarget.SetPositionAndRotation(footBone.position, footBone.rotation);
        
        Vector3 rayOrigin = footBone.position + Vector3.up * rayStartHeight;
        
        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.red);
        
        bool groundFound = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer, QueryTriggerInteraction.Ignore);
        
        if (groundFound)
        {
            Vector3 targetPosition = footBone.position;

            targetPosition.y = hit.point.y + footHeightOffset;

            footTarget.position = targetPosition;
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