using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnderwaterCarController : MonoBehaviour
{
    [Header("Input")]
    [Range(0.02f, 0.4f)] public float inputSmoothTime = 0.12f;
    [Range(0f, 0.4f)] public float touchDeadZone = 0.15f;

    [Header("Movement")]
    public float forwardAcceleration = 14f;
    public float reverseAcceleration = 8.5f;
    public float steerTorque = 3.1f;
    public float maxForwardSpeed = 11f;
    public float maxReverseSpeed = 5f;
    public float passiveBraking = 4.2f;
    public float minSpeedForSteering = 0.45f;

    [Header("Stability")]
    public float maxLateralSpeed = 1.25f;
    public float lateralDamping = 4.8f;
    [Range(0.1f, 1f)] public float highSpeedSteerMultiplier = 0.35f;
    public float speedForFullSteerReduction = 9f;

    [Header("Water Physics")]
    public float waterDrag = 2.4f;
    public float waterAngularDrag = 3.4f;
    public float buoyancyAssist = 6f;
    public float quadraticWaterResistance = 0.035f;

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private bool touchInputActive;
    private float touchThrottle;
    private float touchSteer;
    private float throttleInputVelocity;
    private float steerInputVelocity;

    public float CurrentSpeed { get; private set; }
    public float NormalizedForwardSpeed { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 1200f;
        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    private void Update()
    {
        var keyboardThrottle = ReadThrottleFromKeyboard();
        var keyboardSteer = ReadSteerFromKeyboard();

        var targetThrottle = keyboardThrottle;
        var targetSteer = keyboardSteer;

        if (touchInputActive)
        {
            targetThrottle = ApplyDeadZone(touchThrottle);
            targetSteer = ApplyDeadZone(touchSteer);
        }

        throttleInput = Mathf.SmoothDamp(throttleInput, targetThrottle, ref throttleInputVelocity, inputSmoothTime);
        steerInput = Mathf.SmoothDamp(steerInput, targetSteer, ref steerInputVelocity, inputSmoothTime);
    }

    private void FixedUpdate()
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        CurrentSpeed = rb.velocity.magnitude;
        NormalizedForwardSpeed = Mathf.Clamp01(Mathf.Abs(localVelocity.z) / Mathf.Max(0.01f, maxForwardSpeed));

        ApplyPropulsion(localVelocity);
        ApplySteering(localVelocity);
        ApplyPassiveBraking(localVelocity);
        ApplyLateralStabilization();
        ApplySpeedLimit();
        ApplyWaterResistance();

        // Slight upward force to mimic underwater buoyancy while still keeping gravity.
        rb.AddForce(Vector3.up * buoyancyAssist, ForceMode.Acceleration);
    }

    private void ApplyPropulsion(Vector3 localVelocity)
    {
        if (throttleInput > 0f && localVelocity.z < maxForwardSpeed)
        {
            rb.AddForce(transform.forward * throttleInput * forwardAcceleration, ForceMode.Acceleration);
        }

        if (throttleInput < 0f && localVelocity.z > -maxReverseSpeed)
        {
            rb.AddForce(transform.forward * throttleInput * reverseAcceleration, ForceMode.Acceleration);
        }
    }

    private void ApplySteering(Vector3 localVelocity)
    {
        if (Mathf.Abs(steerInput) <= 0.01f)
        {
            return;
        }

        var speed = new Vector2(localVelocity.x, localVelocity.z).magnitude;
        if (speed < minSpeedForSteering)
        {
            return;
        }

        var speedRatio = Mathf.Clamp01(speed / speedForFullSteerReduction);
        var steerMultiplier = Mathf.Lerp(1f, highSpeedSteerMultiplier, speedRatio);
        rb.AddTorque(Vector3.up * steerInput * steerTorque * steerMultiplier, ForceMode.Acceleration);
    }

    private void ApplyPassiveBraking(Vector3 localVelocity)
    {
        if (Mathf.Abs(throttleInput) > 0.01f)
        {
            return;
        }

        var targetForwardSpeed = Mathf.MoveTowards(localVelocity.z, 0f, passiveBraking * Time.fixedDeltaTime);
        SetLocalPlanarVelocity(localVelocity.x, targetForwardSpeed);
    }

    private void ApplyLateralStabilization()
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        var clampedLateral = Mathf.Clamp(localVelocity.x, -maxLateralSpeed, maxLateralSpeed);
        var stabilizedLateral = Mathf.MoveTowards(clampedLateral, 0f, lateralDamping * Time.fixedDeltaTime);
        SetLocalPlanarVelocity(stabilizedLateral, localVelocity.z);
    }

    private void ApplySpeedLimit()
    {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        var clampedForward = Mathf.Clamp(localVelocity.z, -maxReverseSpeed, maxForwardSpeed);
        SetLocalPlanarVelocity(localVelocity.x, clampedForward);
    }

    private void ApplyWaterResistance()
    {
        var speed = rb.velocity.magnitude;
        if (speed < 0.01f)
        {
            return;
        }

        var resistance = speed * speed * quadraticWaterResistance;
        rb.AddForce(-rb.velocity.normalized * resistance, ForceMode.Acceleration);
    }

    private void SetLocalPlanarVelocity(float localX, float localZ)
    {
        var planarWorldVelocity = transform.TransformDirection(new Vector3(localX, 0f, localZ));
        rb.velocity = new Vector3(planarWorldVelocity.x, rb.velocity.y, planarWorldVelocity.z);
    }

    public void SetTouchInput(float throttle, float steer)
    {
        touchInputActive = true;
        touchThrottle = Mathf.Clamp(throttle, -1f, 1f);
        touchSteer = Mathf.Clamp(steer, -1f, 1f);
    }

    public void ClearTouchInput()
    {
        touchInputActive = false;
        touchThrottle = 0f;
        touchSteer = 0f;
    }

    private float ApplyDeadZone(float value)
    {
        return Mathf.Abs(value) < touchDeadZone ? 0f : value;
    }

    private static float ReadThrottleFromKeyboard()
    {
        var throttle = 0f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            throttle += 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            throttle -= 1f;
        }

        return Mathf.Clamp(throttle, -1f, 1f);
    }

    private static float ReadSteerFromKeyboard()
    {
        var steer = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            steer -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            steer += 1f;
        }

        return Mathf.Clamp(steer, -1f, 1f);
    }
}
