using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class SpringButton : MonoBehaviour
{
    [Header("Spring")]
    public float springForce = 800f;
    public float damper     = 40f;
    public float maxTravel  = 0.04f;   // metres the cap can sink (e.g. 4 cm)

    [Header("Press")]
    [Range(0f, 1f)]
    public float pressThreshold = 0.8f; // fraction of maxTravel = "pressed"
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    ConfigurableJoint _joint;
    bool _wasPressed;
    Vector3 _restLocalPos;

    void Start()
    {
        _restLocalPos = transform.localPosition;

        _joint = gameObject.AddComponent<ConfigurableJoint>();

        // Connect to parent (the static ButtonRoot)
        // If ButtonRoot has no Rigidbody the joint anchors to world — fine here.
        // Optionally add a kinematic Rigidbody to ButtonRoot for cleaner anchoring.

        // Lock all DOF except Y
        _joint.xMotion = ConfigurableJointMotion.Locked;
        _joint.zMotion = ConfigurableJointMotion.Locked;
        _joint.angularXMotion = ConfigurableJointMotion.Locked;
        _joint.angularYMotion = ConfigurableJointMotion.Locked;
        _joint.angularZMotion = ConfigurableJointMotion.Locked;

        // Allow Y to move downward only (limited)
        _joint.yMotion = ConfigurableJointMotion.Limited;

        var limit = new SoftJointLimit { limit = maxTravel };
        _joint.linearLimit = limit;

        // Spring drive on Y
        var drive = new JointDrive
        {
            positionSpring = springForce,
            positionDamper = damper,
            maximumForce   = Mathf.Infinity
        };
        _joint.yDrive = drive;

        // Rest position is "up" (local Y = 0 relative to joint anchor)
        _joint.targetPosition = Vector3.zero;

        GetComponent<Rigidbody>().useGravity = false;
    }

    void Update()
    {
        // How far has the cap sunk? (world-space delta vs rest position)
        float travel = _restLocalPos.y - transform.localPosition.y;
        float fraction = Mathf.Clamp01(travel / maxTravel);

        bool isPressed = fraction >= pressThreshold;

        if (isPressed && !_wasPressed)  OnPressed.Invoke();
        if (!isPressed && _wasPressed)  OnReleased.Invoke();

        _wasPressed = isPressed;
    }
}