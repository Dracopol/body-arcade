using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR;

public class HapticTest : MonoBehaviour
{
    [Range(0f, 1f)]
    public float hapticIntensity = 0.7f;
    public float hapticDuration = 0.1f;

    private HapticImpulsePlayer hapticPlayer;
    private UnityEngine.XR.InputDevice rightController;
    private UnityEngine.XR.InputDevice leftController;
    private bool rightControllerFound = false;
    private bool leftControllerFound = false;

    void Start()
    {
        hapticPlayer = GetComponent<HapticImpulsePlayer>();

        if (hapticPlayer == null)
            Debug.LogError("HapticImpulsePlayer not found on this GameObject!");

        // Find the right controller input device
        var rightDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            rightDevices);

        if (rightDevices.Count > 0)
        {
            rightController = rightDevices[0];
            rightControllerFound = true;
            Debug.Log("Right controller found: " + rightController.name);
        }

        // Find the left controller input device
        var leftDevices = new System.Collections.Generic.List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            leftDevices);

        if (leftDevices.Count > 0)
        {
            leftController = leftDevices[0];
            leftControllerFound = true;
            Debug.Log("Left controller found: " + leftController.name);
        }
    }

    // void Update()
    // {
    //     if (hapticPlayer == null || !rightControllerFound)
    //         return;

    //     // Check if trigger is pressed
    //     if (rightController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue) && triggerValue >= 0.1f)
    //     {
    //         TriggerHapticFeedback(hapticIntensity, hapticDuration);
    //     }
    // }
    public void TriggerHapticFeedback(float intensity, float duration)
    {
        if (hapticPlayer != null && rightControllerFound)
        {
            hapticPlayer.SendHapticImpulse(intensity, duration);
        }

        if (hapticPlayer != null && leftControllerFound)
        {
            hapticPlayer.SendHapticImpulse(intensity, duration);
        }
    }

    // Parameterless version for Unity Events
    public void TriggerHapticFeedback()
    {
        TriggerHapticFeedback(hapticIntensity, hapticDuration);
    }
}

