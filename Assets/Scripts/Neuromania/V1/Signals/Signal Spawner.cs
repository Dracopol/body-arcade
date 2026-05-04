using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.NeuromaniaV1.Signals
{
    internal class SignalSpawner : MonoBehaviour
    {
        #region Events

        internal event Action<Signal> OnSignalSpawned;

        #endregion

        #region Internal

        internal async UniTask SpawnAsync(GameObject prefab, SplineContainer targetSpline, CancellationToken ct)
        {
            try
            {
                if (prefab == null) return;
                if (targetSpline == null) return;

                GameObject instance = Instantiate(prefab, targetSpline.EvaluatePosition(0f), Quaternion.identity);
                Signal instanceSignal = instance.GetComponent<Signal>();

                if (instanceSignal == null)
                {
                    Destroy(instance);
                    return;
                }

                float speed = instanceSignal.MovementSpeed;
                bool isReversed = instanceSignal.IsProjectile;

                if (isReversed)
                    instance.transform.position = targetSpline.EvaluatePosition(1f);

                OnSignalSpawned?.Invoke(instanceSignal);

                await AnimateAsync(instanceSignal, targetSpline, speed, isReversed, ct);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Signal animation cancelled.");
            }
        }

        #endregion

        #region Private

        private async UniTask AnimateAsync(Signal signal, SplineContainer targetSpline, float speedMetersPerSecond, bool isReversed, CancellationToken ct)
        {
            float t = isReversed ? 1f : 0f;
            float direction = isReversed ? -1f : 1f;
            Rigidbody rb = signal.Rigidbody;

            while (true)
            {
                t = Mathf.Clamp01(t);

                if (signal != null)
                {
                    Vector3 position = targetSpline.EvaluatePosition(t);
                    Vector3 tangent = targetSpline.EvaluateTangent(t);

                    if (rb != null)
                        rb.MovePosition(position);
                    else
                        signal.transform.position = position;

                    if (tangent.sqrMagnitude > 0.000001f)
                    {
                        Vector3 forward = isReversed ? -tangent : tangent;
                        signal.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    }
                }

                t += direction * speedMetersPerSecond * Time.fixedDeltaTime;

                if ((!isReversed && t >= 1f) || (isReversed && t <= 0f))
                    break;

                await UniTask.WaitForFixedUpdate(ct);
            }

            if (signal != null)
                Destroy(signal.gameObject);
        }

        #endregion
    }
}