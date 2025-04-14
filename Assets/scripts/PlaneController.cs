using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour {
    public enum AirplaneState {
        Grounded, // No forward thrust.
        Flying    // Airborne (forward motion enabled).
    }

    // Speed variables.
    private float maxSpeed;
    private float currentSpeed;

    // Rotation speeds (set via Inspector).
    [Header("Rotating Speeds")]
    [SerializeField] private float yawSpeed = 50f;
    [SerializeField] private float pitchSpeed = 100f;
    [SerializeField] private float rollSpeed = 200f;

    // Movement speed variables.
    [Header("Moving Speed")]
    [SerializeField] private float defaultSpeed = 10f;
    [SerializeField] private float accelerating = 10f;
    [SerializeField] private float decelerating = 5f;

    // Control sensitivities.
    [Header("Control Sensitivity")]
    [SerializeField] private float mouseSensitivityPitchRoll = 0.1f;
    [SerializeField] private float keyboardSensitivityPitchRoll = 1f;

    // Stabilization: Factor to dampen angular velocity when no input is provided.
    [Header("Stabilization")]
    [SerializeField] private float angularStabilizationFactor = 2f;

    // Maximum allowed angular velocities (in radians per second).
    [Header("Max Angular Speeds")]
    [SerializeField] private float maxPitchAngularVelocity = 2f; // around local X axis.
    [SerializeField] private float maxYawAngularVelocity = 2f;   // around local Y axis.
    [SerializeField] private float maxRollAngularVelocity = 2f;  // around local Z axis.

    // New: Rate at which yaw angular velocity opposite to input is cancelled.
    [Header("Yaw Cancellation")]
    [SerializeField] private float yawCancellationRate = 10f;

    // New: Additional cancellation for pitch & roll when not explicitly commanded.
    [Header("Pitch and Roll Cancellation")]
    [SerializeField] private float pitchRollCancellationFactor = 5f;

    // Plane state.
    public AirplaneState airplaneState;

    // Reference to the Rigidbody.
    private Rigidbody rb;

    // Input values (set via the new Input System).
    private float inputPitch = 0f;
    private float inputRoll = 0f;
    private float inputYaw = 0f;

    private void Start() {
        // Initialize speed values.
        maxSpeed = defaultSpeed;
        currentSpeed = 0f;

        // Setup Rigidbody (ensure it's non-kinematic for physics interactions).
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        // Start with the plane on the ground.
        airplaneState = AirplaneState.Grounded;
    }

    // FixedUpdate is used for physics-based updates.
    private void FixedUpdate() {
        switch (airplaneState) {
            case AirplaneState.Flying:
                FlyingFixedUpdate();
                break;
            case AirplaneState.Grounded:
                GroundedFixedUpdate();
                break;
        }
    }

    private void FlyingFixedUpdate() {
        // Apply rotations based on user input.
        // Use AddRelativeTorque for pitch and roll (local axes).
        rb.AddRelativeTorque(Vector3.right * (inputPitch * pitchSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);
        rb.AddRelativeTorque(Vector3.forward * (-inputRoll * rollSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);

        // Apply yaw using world space torque.
        rb.AddTorque(Vector3.up * (inputYaw * yawSpeed * Time.fixedDeltaTime), ForceMode.VelocityChange);

        // -- Yaw Cancellation Enhancement --
        // If the user provides a yaw input and the current yaw angular velocity is opposing it,
        // cancel that opposing component more quickly.
        Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
        if (inputYaw != 0f && (localAngularVelocity.y * inputYaw < 0)) {
            localAngularVelocity.y = Mathf.MoveTowards(localAngularVelocity.y, 0f, yawCancellationRate * Time.fixedDeltaTime);
            rb.angularVelocity = transform.TransformDirection(localAngularVelocity);
        }
        // ------------------------------------

        // Dampen pitch and roll if not explicitly commanded.
        localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
        if (Mathf.Approximately(inputPitch, 0f)) {
            localAngularVelocity.x = Mathf.Lerp(localAngularVelocity.x, 0f, pitchRollCancellationFactor * Time.fixedDeltaTime);
        }
        if (Mathf.Approximately(inputRoll, 0f)) {
            localAngularVelocity.z = Mathf.Lerp(localAngularVelocity.z, 0f, pitchRollCancellationFactor * Time.fixedDeltaTime);
        }
        rb.angularVelocity = transform.TransformDirection(localAngularVelocity);

        // When no rotational input is provided at all, apply additional damping.
        if (Mathf.Approximately(inputPitch, 0f) &&
            Mathf.Approximately(inputRoll, 0f) &&
            Mathf.Approximately(inputYaw, 0f))
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, angularStabilizationFactor * Time.fixedDeltaTime);
        }

        // Clamp angular velocities to prevent them from building up too quickly.
        ClampAngularVelocity();

        // Update the speed: accelerate toward maxSpeed or decelerate if above it.
        if (currentSpeed < maxSpeed) {
            currentSpeed += accelerating * Time.fixedDeltaTime;
        } else {
            currentSpeed -= decelerating * Time.fixedDeltaTime;
        }

        // Force the airplane to always fly in its forward direction.
        rb.linearVelocity = transform.forward * currentSpeed;
    }

    private void GroundedFixedUpdate() {
        // Gradually reduce both linear and angular velocities.
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, decelerating * Time.fixedDeltaTime);
        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, decelerating * Time.fixedDeltaTime);
    }

    // Clamp the Rigidbody's angular velocity based on defined maximums.
    private void ClampAngularVelocity() {
        Vector3 localAngularVelocity = transform.InverseTransformDirection(rb.angularVelocity);
        localAngularVelocity.x = Mathf.Clamp(localAngularVelocity.x, -maxPitchAngularVelocity, maxPitchAngularVelocity);
        localAngularVelocity.y = Mathf.Clamp(localAngularVelocity.y, -maxYawAngularVelocity, maxYawAngularVelocity);
        localAngularVelocity.z = Mathf.Clamp(localAngularVelocity.z, -maxRollAngularVelocity, maxRollAngularVelocity);
        rb.angularVelocity = transform.TransformDirection(localAngularVelocity);
    }

    // Handle mouse delta input for pitch and roll.
    public void OnMouseDelta(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.canceled) {
            CancelPitchRoll();
        } else {
            Vector2 mouseDelta = context.ReadValue<Vector2>();
            PitchRoll(mouseDelta, mouseSensitivityPitchRoll);
        }
    }

    // Handler for arrow key input.
    public void OnArrowDelta(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.canceled) {
            CancelPitchRoll();
        } else {
            Vector2 arrowInput = context.ReadValue<Vector2>();
            PitchRoll(arrowInput, keyboardSensitivityPitchRoll);
        }
    }

    // Yaw input handlers.
    public void OnYawLeft(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed) {
            inputYaw = -1f;
        } else if (context.canceled) {
            inputYaw = 0f;
        }
    }

    public void OnYawRight(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed) {
            inputYaw = 1f;
        } else if (context.canceled) {
            inputYaw = 0f;
        }
    }
    public void OnYawLeftMobile(bool activated) {
        if (activated) {
            inputYaw = -1f;
        } else {
            inputYaw = 0f;
        }
    }

    public void OnYawRightMobile(bool activated) {
        if (activated) {
            inputYaw = 1f;
        } else {
            inputYaw = 0f;
        }
    }

    // Takeoff event.
    // This removes any downward (negative Y) component if the airplane was falling.
    public void OnTakeoff(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed) {
            Takeoff();
        }
    }

    // Landing event.
    public void OnLand(InputAction.CallbackContext context) {
        Debug.Log(context);
        if (context.performed) {
            Land();
        }
    }
    
    public void OnMobileMoveDelta(Vector2 delta) {
        inputPitch = delta.y * keyboardSensitivityPitchRoll;
        inputRoll = delta.x * keyboardSensitivityPitchRoll;
    }

    public void OnMobileTakeoff(bool takeoff) {
        if (takeoff) {
            Takeoff();
        }
    }

    public void OnMobileLand(bool land) {
        if (land) {
            Land();
        }
    }

    private void PitchRoll(Vector2 delta, float sensitivity) {
        inputPitch = delta.y * sensitivity;
        inputRoll = delta.x * sensitivity;
    }
    
    private void CancelPitchRoll() {
        inputPitch = 0f;
        inputRoll = 0f;
    }
    
    private void Takeoff() {
        if (airplaneState == AirplaneState.Grounded) {
            rb.useGravity = false;
            Vector3 currentVel = rb.linearVelocity;
            if (currentVel.y < 0f) {
                currentVel.y = 0f;
                rb.linearVelocity = currentVel;
            }

            airplaneState = AirplaneState.Flying;
            currentSpeed = defaultSpeed * 0.5f;
            
            Vector3 forwardDirection = transform.forward;
            forwardDirection.y = 0f;
            forwardDirection.Normalize();
            rb.linearVelocity = forwardDirection * currentSpeed;
        }
    }

    private void Land() {
        if (airplaneState == AirplaneState.Flying) {
            rb.useGravity = true;
            airplaneState = AirplaneState.Grounded;
        }
    }
}
