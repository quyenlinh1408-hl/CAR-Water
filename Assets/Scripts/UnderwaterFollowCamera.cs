using UnityEngine;

public class UnderwaterFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -7f);
    public float followSmoothTime = 0.2f;
    public float lookAhead = 4f;
    public float speedOffsetPullBack = 2f;
    public float speedOffsetRise = 0.55f;
    public float speedLookAheadBoost = 2.2f;
    public float rotationLerpSpeed = 4f;

    private Vector3 velocity;
    private UnderwaterCarController cachedController;

    private void Start()
    {
        CacheController();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        CacheController();

        var speedRatio = cachedController != null
            ? cachedController.NormalizedForwardSpeed
            : 0f;

        var dynamicOffset = offset + new Vector3(0f, speedOffsetRise * speedRatio, -speedOffsetPullBack * speedRatio);
        var dynamicSmoothTime = Mathf.Lerp(followSmoothTime, followSmoothTime * 0.72f, speedRatio);

        var desiredPosition = target.TransformPoint(dynamicOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, dynamicSmoothTime);

        var dynamicLookAhead = lookAhead + speedLookAheadBoost * speedRatio;
        var lookPoint = target.position + target.forward * dynamicLookAhead + Vector3.up * (0.45f + speedRatio * 0.35f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(lookPoint - transform.position, Vector3.up),
            Time.deltaTime * rotationLerpSpeed);
    }

    private void CacheController()
    {
        if (cachedController != null)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        cachedController = target.GetComponent<UnderwaterCarController>();
    }
}
