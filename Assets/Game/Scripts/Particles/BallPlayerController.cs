using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class BallPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 35f;
    public float maxSpeed = 8f;
    public float idleDrag = 2f;

    [Header("Grounding")]
    public float groundCheckDistance = 0.6f;

    [Tooltip("Which layers count as ground.")]
    public LayerMask groundMask = ~0;

    [Range(0f, 1f)] public float airControl = 0.35f;

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(x, 0f, z);
        input = Vector3.ClampMagnitude(input, 1f);

        bool grounded = IsGrounded(out RaycastHit hit);

        Vector3 moveDir;

        moveDir = new Vector3(input.x, 0f, input.z);

        if (grounded)
            moveDir = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized * moveDir.magnitude;

        float control = grounded ? 1f : airControl;
        if (moveDir.sqrMagnitude > 0f)
        {
            _rb.AddForce(moveDir * (moveForce * control), ForceMode.Acceleration);
        }

        Vector3 v = _rb.linearVelocity;
        Vector3 vXZ = new Vector3(v.x, 0f, v.z);
        if (vXZ.magnitude > maxSpeed)
        {
            Vector3 capped = vXZ.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(capped.x, v.y, capped.z);
        }

        if (grounded)
            _rb.linearDamping = (input.sqrMagnitude < 0.001f) ? idleDrag : 0f;
        else
            _rb.linearDamping = 0f;
    }

    bool IsGrounded(out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }
}
