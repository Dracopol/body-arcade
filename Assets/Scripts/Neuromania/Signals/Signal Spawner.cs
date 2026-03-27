using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class SignalSpawner : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private GameObject _prefabA;
        [SerializeField] private GameObject _prefabB;

        #endregion

        #region Update

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.qKey.wasPressedThisFrame)
                SpawnAndAnimateAsync(_prefabA, _splineContainer, destroyCancellationToken).Forget();

            if (kb.wKey.wasPressedThisFrame)
                SpawnAndAnimateAsync(_prefabB, _splineContainer, destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private async UniTask SpawnAndAnimateAsync(GameObject prefab, SplineContainer targetSpline, CancellationToken ct)
        {
            try
            {
                if (prefab == null) return;
                if (targetSpline == null) return;

                Signal signalData = prefab.GetComponent<Signal>();
                if (signalData == null) return;

                float speed = signalData.MovementSpeed;
                bool isReversed = signalData.IsProjectile;
                float startT = isReversed ? 1f : 0f;
                Vector3 spawnPos = targetSpline.EvaluatePosition(startT);

                GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
                Signal instanceSignal = instance.GetComponent<Signal>();

                await AnimateAsync(instanceSignal, targetSpline, speed, isReversed, ct);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Signal animation cancelled.");
            }
        }

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