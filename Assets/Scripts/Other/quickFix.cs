using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace IMVR.BodyArcade
{
    public sealed class XRControllerPoseDriver : MonoBehaviour
    {
        [SerializeField] private InputDeviceCharacteristics _characteristics;

        private InputDevice _device;

        private void Update()
        {
            if (!_device.isValid)
            {
                var found = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(_characteristics, found);
                if (found.Count == 0)
                    return;

                _device = found[0];
            }

            if (_device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position))
                transform.localPosition = position;

            if (_device.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
                transform.localRotation = rotation;
        }
    }
}