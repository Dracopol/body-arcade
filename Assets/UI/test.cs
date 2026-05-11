using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

namespace IMVR.BodyArcade.Diagnostics
{
    internal sealed class XRControllerDiagnostics : MonoBehaviour
    {
        private const float LogInterval = 3f;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            LogOnce();
            yield return new WaitForSeconds(LogInterval);
            LogOnce();
        }

        private static void LogOnce()
        {
            var sb = new StringBuilder();
            sb.AppendLine("========== XR CONTROLLER DIAGNOSTICS ==========");

            // --- XR General ---
            sb.AppendLine($"[XR] XRSettings.enabled: {XRSettings.enabled}");
            sb.AppendLine($"[XR] XRSettings.loadedDeviceName: '{XRSettings.loadedDeviceName}'");
            sb.AppendLine($"[XR] XRSettings.isDeviceActive: {XRSettings.isDeviceActive}");

            // --- XR Loader ---
            var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
            sb.AppendLine($"[Loader] activeLoader: {(loader != null ? loader.GetType().Name : "NULL")}");

            // --- All Connected Devices ---
            var devices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.None, devices);
            sb.AppendLine($"[Devices] Total connected: {devices.Count}");
            foreach (var device in devices)
            {
                sb.AppendLine($"  - Name: '{device.name}' | Characteristics: {device.characteristics} | Valid: {device.isValid}");

                if (device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked))
                    sb.AppendLine($"    isTracked: {isTracked}");
                if (device.TryGetFeatureValue(CommonUsages.trackingState, out InputTrackingState state))
                    sb.AppendLine($"    trackingState: {state}");
                if (device.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 pos))
                    sb.AppendLine($"    position: {pos}");
            }

            // --- Left / Right Controller specifically ---
            var leftDevices = new List<InputDevice>();
            var rightDevices = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, leftDevices);
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, rightDevices);

            sb.AppendLine($"[Controllers] Left controller devices found: {leftDevices.Count}");
            foreach (var d in leftDevices)
                sb.AppendLine($"  - '{d.name}' valid:{d.isValid}");

            sb.AppendLine($"[Controllers] Right controller devices found: {rightDevices.Count}");
            foreach (var d in rightDevices)
                sb.AppendLine($"  - '{d.name}' valid:{d.isValid}");

            // --- Pose Drivers ---
            LogControllerPoseDrivers(sb);

            // --- XR Origin / Rig GameObjects ---
            var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                sb.AppendLine($"[XROrigin] Found: '{xrOrigin.gameObject.name}' active:{xrOrigin.gameObject.activeSelf}");
                sb.AppendLine($"[XROrigin] Camera Floor Offset Object: {(xrOrigin.CameraFloorOffsetObject != null ? xrOrigin.CameraFloorOffsetObject.name : "NULL")}");

                sb.AppendLine("[XROrigin] Child hierarchy:");
                LogChildren(xrOrigin.transform, sb, 1);
            }
            else
            {
                sb.AppendLine("[XROrigin] NOT FOUND in scene");
            }

            sb.AppendLine("========== END DIAGNOSTICS ==========");
            Debug.Log(sb.ToString());
        }

        private static void LogControllerPoseDrivers(StringBuilder sb)
        {
            sb.AppendLine("[PoseDrivers] Checking controller pose components...");

            var names = new[] { "Left Controller", "Right Controller" };
            foreach (var controllerName in names)
            {
                var go = GameObject.Find(controllerName);
                if (go == null)
                {
                    sb.AppendLine($"  [{controllerName}] GameObject NOT FOUND");
                    continue;
                }

                sb.AppendLine($"  [{controllerName}] found, activeSelf:{go.activeSelf} activeInHierarchy:{go.activeInHierarchy}");
                sb.AppendLine($"  [{controllerName}] localPosition:{go.transform.localPosition} localRotation:{go.transform.localRotation}");
                sb.AppendLine($"  [{controllerName}] worldPosition:{go.transform.position}");

                // TrackedPoseDriver (Unity built-in)
                // IMPORTANT: TrackedPoseDriver.positionAction / rotationAction are plain InputAction,
                // NOT InputActionProperty — there is no .action wrapper on them.
                var tpd = go.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (tpd != null)
                {
                    sb.AppendLine($"  [{controllerName}] TrackedPoseDriver: enabled:{tpd.enabled}");
                    sb.AppendLine($"    updateType:{tpd.updateType} trackingType:{tpd.trackingType}");
                    sb.AppendLine($"    positionAction bound controls:{(tpd.positionAction != null ? tpd.positionAction.controls.Count : 0)}");
                    sb.AppendLine($"    rotationAction bound controls:{(tpd.rotationAction != null ? tpd.rotationAction.controls.Count : 0)}");
                    if (tpd.positionAction != null)
                    {
                        sb.AppendLine($"    positionAction enabled:{tpd.positionAction.enabled}");
                        sb.AppendLine($"    positionAction value:{tpd.positionAction.ReadValue<Vector3>()}");
                    }
                }
                else
                {
                    sb.AppendLine($"  [{controllerName}] TrackedPoseDriver: NOT FOUND on root");
                }

                // ActionBasedController (XR Toolkit, deprecated in XRI 3.0 — suppressed intentionally for diagnostics)
                // Uses InputActionProperty, so the inner .action IS a class reference and allows ?.
                // However .action.controls is ReadOnlyArray<InputControl> (struct) — no ?. allowed on it.
#pragma warning disable CS0618
                var abc = go.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
#pragma warning restore CS0618
                if (abc != null)
                {
                    sb.AppendLine($"  [{controllerName}] ActionBasedController (deprecated): enabled:{abc.enabled}");
                    sb.AppendLine($"    positionAction bound controls:{(abc.positionAction.action != null ? abc.positionAction.action.controls.Count : 0)}");
                    sb.AppendLine($"    rotationAction bound controls:{(abc.rotationAction.action != null ? abc.rotationAction.action.controls.Count : 0)}");
                    if (abc.positionAction.action != null)
                    {
                        sb.AppendLine($"    positionAction enabled:{abc.positionAction.action.enabled}");
                        sb.AppendLine($"    positionAction value:{abc.positionAction.action.ReadValue<Vector3>()}");
                    }
                }
                else
                {
                    sb.AppendLine($"  [{controllerName}] ActionBasedController: NOT FOUND on root (expected in XRI 3.x)");
                }
            }
        }

        private static void LogChildren(Transform t, StringBuilder sb, int depth)
        {
            string indent = new string(' ', depth * 2);
            foreach (Transform child in t)
            {
                sb.AppendLine($"{indent}- '{child.name}' activeSelf:{child.gameObject.activeSelf} activeInHierarchy:{child.gameObject.activeInHierarchy}");
                LogChildren(child, sb, depth + 1);
            }
        }
    }
}