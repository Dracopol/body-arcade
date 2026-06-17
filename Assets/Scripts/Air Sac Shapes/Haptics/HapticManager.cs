using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace IMVR.BodyArcade.AirSacShapes.Haptics
{
    // Per-controller vibration on plunger touch/release. Touch ramps low→high ("happy"),
    // release ramps high→low, both quick. Left side buzzes the left controller, right the
    // right — always, in mirror or unmirrored mode. Resolves XR InputDevices by hand
    // characteristic (re-resolving when a device goes invalid), so it no-ops safely with
    // no headset attached.
    internal class HapticManager : MonoBehaviour
    {
        #region Types

        internal enum Side { Left, Right }

        #endregion

        #region Serialized fields

        [Header("Touch (rising)")]
        [SerializeField] private AnimationCurve _touchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField, Min(0.01f)] private float _touchDuration = 0.12f;

        [Header("Release (falling)")]
        [SerializeField] private AnimationCurve _releaseCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        [SerializeField, Min(0.01f)] private float _releaseDuration = 0.12f;

        [Header("Tuning")]
        [SerializeField, Range(2, 20)] private int _impulseSteps = 8;
        [SerializeField, Range(0f, 1f)] private float _amplitudeScale = 1f;

        #endregion

        #region Fields

        private InputDevice _leftDevice;
        private InputDevice _rightDevice;

        private CancellationTokenSource _leftCts;
        private CancellationTokenSource _rightCts;

        #endregion

        #region Setup

        private void OnDestroy()
        {
            CancelSide(Side.Left);
            CancelSide(Side.Right);
        }

        #endregion

        #region Internal

        internal void PlayTouch(Side side)   => Play(side, _touchCurve, _touchDuration);
        internal void PlayRelease(Side side) => Play(side, _releaseCurve, _releaseDuration);

        #endregion

        #region Private

        private void Play(Side side, AnimationCurve curve, float duration)
        {
            if (UiRobot.SessionConfigAirSacShapes.Instance != null &&
                !UiRobot.SessionConfigAirSacShapes.Instance.Vibration) return;

            InputDevice device = Resolve(side);
            if (!device.isValid) return;

            // Replace any in-flight ramp on this side so a quick touch→release doesn't overlap.
            CancelSide(side);
            CancellationTokenSource cts = new();
            if (side == Side.Left) _leftCts = cts; else _rightCts = cts;

            RampAsync(device, curve, duration, cts.Token).Forget();
        }

        private async UniTaskVoid RampAsync(InputDevice device, AnimationCurve curve, float duration, CancellationToken ct)
        {
            float stepDuration = duration / _impulseSteps;

            for (int i = 0; i < _impulseSteps; i++)
            {
                if (ct.IsCancellationRequested) return;

                float t         = _impulseSteps > 1 ? i / (float)(_impulseSteps - 1) : 1f;
                float amplitude = Mathf.Clamp01(curve.Evaluate(t) * _amplitudeScale);

                if (device.isValid &&
                    device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
                {
                    // Slightly longer per-impulse than the gap so steps blend into a smooth ramp.
                    device.SendHapticImpulse(0u, amplitude, stepDuration * 1.2f);
                }

                await UniTask.WaitForSeconds(stepDuration, cancellationToken: ct);
            }
        }

        private InputDevice Resolve(Side side)
        {
            ref InputDevice cached = ref (side == Side.Left ? ref _leftDevice : ref _rightDevice);
            if (cached.isValid) return cached;

            InputDeviceCharacteristics hand = side == Side.Left
                ? InputDeviceCharacteristics.Left
                : InputDeviceCharacteristics.Right;

            var found = new List<InputDevice>();
            InputDevices.GetDevicesWithCharacteristics(hand | InputDeviceCharacteristics.Controller, found);
            if (found.Count > 0)
                cached = found[0];

            return cached;
        }

        private void CancelSide(Side side)
        {
            ref CancellationTokenSource cts = ref (side == Side.Left ? ref _leftCts : ref _rightCts);
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
        }

        #endregion
    }
}
