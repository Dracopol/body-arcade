using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Splines;
using UnityEngine.Events;

public class checkstarttemp : MonoBehaviour
{
    public GameObject particleSystems;
    public UnityEvent onHover;
    public GameObject scenemanager;
    public GameObject scoresystem;
    public GameObject player;
    public GameObject startui;
    public float raycastDistance = 100f;
    public LayerMask layerMask;
    public Button button;
    public Image image;
    public TMP_Text text;
    public bool isRightController = true; // Check in Inspector for right controller, uncheck for left

    private UnityEngine.XR.InputDevice rightController;
    private bool rightControllerFound = false;
    private UnityEngine.XR.InputDevice leftController;
    private bool leftControllerFound = false;

    static bool leftIsHovering = false;
    static bool rightIsHovering = false;

    void Start()
    {
        var rightDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightDevices);

        var leftDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            leftDevices);

        if (rightDevices.Count > 0)
        {
            rightController = rightDevices[0];
            rightControllerFound = true;
            Debug.Log("Right controller found: " + rightController.name);
        }

        if (leftDevices.Count > 0)
        {
            leftController = leftDevices[0];
            leftControllerFound = true;
            Debug.Log("Left controller found: " + leftController.name);
        }
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, layerMask))
        {
            // Only set this instance's own flag
            if (isRightController) rightIsHovering = true;
            else leftIsHovering = true;

            image.color = Color.green;
            text.color = Color.green;
            onHover.Invoke();

            Debug.Log($"Pointing at: {hit.collider.gameObject.name}");

            if (rightControllerFound)
            {
                float triggerValue;
                if (rightController.TryGetFeatureValue(CommonUsages.trigger, out triggerValue))
                {
                    if (triggerValue >= 0.5f)
                    {
                        startui.SetActive(false);
                        if (player != null)
                        {
                            SplineAnimate spline = player.GetComponent<SplineAnimate>();
                            if (spline != null)
                            {
                                spline.Play();
                            }
                            if (particleSystems != null)
                            {
                                SplineAnimate PsSpline = particleSystems.GetComponent<SplineAnimate>();
                                PsSpline.Play();
                            }
                        }
                        scenemanager.GetComponent<scenetimer>().StartTimer();
                        scoresystem.GetComponent<Timer>().StartTimer();
                    }
                }
            }

            if (leftControllerFound)
            {
                float triggerValue;
                if (leftController.TryGetFeatureValue(CommonUsages.trigger, out triggerValue))
                {
                    if (triggerValue >= 0.5f)
                    {
                        startui.SetActive(false);
                        if (player != null)
                        {
                            SplineAnimate spline = player.GetComponent<SplineAnimate>();
                            if (spline != null)
                            {
                                spline.Play();
                            }
                            if (particleSystems != null)
                            {
                                SplineAnimate PsSpline = particleSystems.GetComponent<SplineAnimate>();
                                PsSpline.Play();
                            }
                        }
                        scenemanager.GetComponent<scenetimer>().StartTimer();
                        scoresystem.GetComponent<Timer>().StartTimer();
                    }
                }
            }
        }
        else
        {
            // Only clear this instance's own flag
            if (isRightController) rightIsHovering = false;
            else leftIsHovering = false;

            // Only reset color if neither controller is hovering
            if (!rightIsHovering && !leftIsHovering)
            {
                image.color = Color.white;
                text.color = Color.white;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 endPoint = transform.position + transform.forward * raycastDistance;
        Gizmos.DrawLine(transform.position, endPoint);
    }
}