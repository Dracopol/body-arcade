using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.NeuromaniaV1
{
    public class TargetSpawnerOLD : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _targetPrefabRight;
        [SerializeField] private GameObject _targetPrefabLeft;
        [SerializeField] private SplineContainer _splineRight;
        [SerializeField] private SplineContainer _splineLeft;
        [SerializeField] private float _minInterval = 1f;
        [SerializeField] private float _maxInterval = 3f;
        [SerializeField] private float _travelSpeed = 2f;
        [SerializeField][Range(0f, 1f)] private float _tiltProbability = 0.5f;

        #endregion

        #region Setup

        private void Start()
        {
            RunSideAsync(_targetPrefabRight, _splineRight, destroyCancellationToken).Forget();
            RunSideAsync(_targetPrefabLeft, _splineLeft, destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private async UniTaskVoid RunSideAsync(GameObject prefab, SplineContainer spline, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                float delay = UnityEngine.Random.Range(_minInterval, _maxInterval);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                SpawnOnSpline(prefab, spline, ct);
            }
        }

        private void SpawnOnSpline(GameObject prefab, SplineContainer spline, CancellationToken ct)
        {
            if (prefab == null || spline == null)
            {
                Debug.LogWarning($"[TargetSpawner] Missing reference — prefab: {prefab}, spline: {spline}", this);
                return;
            }

            GameObject instance = Instantiate(prefab, transform);
            ApplyChildTilt(instance);
            DriveAlongSplineAsync(instance, spline, ct).Forget();
        }

        private void ApplyChildTilt(GameObject instance)
        {
            if (instance.transform.childCount == 0)
                return;

            Transform child = instance.transform.GetChild(0);

            if (UnityEngine.Random.value < _tiltProbability)
                child.localRotation *= Quaternion.Euler(0f, 0f, -45f);
        }

        private async UniTaskVoid DriveAlongSplineAsync(GameObject instance, SplineContainer spline, CancellationToken ct)
        {
            float splineLength = spline.CalculateLength();
            float t = 0f;

            while (t < 1f && !ct.IsCancellationRequested)
            {
                spline.Evaluate(t, out float3 position, out float3 tangent, out float3 upVector);

                instance.transform.position = (Vector3)position;

                if (!tangent.Equals(float3.zero))
                    instance.transform.rotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)upVector);

                t += (_travelSpeed / splineLength) * Time.deltaTime;

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            Destroy(instance);
        }

        #endregion
    }
}