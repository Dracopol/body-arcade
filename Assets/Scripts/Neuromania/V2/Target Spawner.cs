using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania
{
    internal class TargetSpawner : MonoBehaviour
    {
        #region Entities

        private enum SpawnMode { Left, Right, Both }

        #endregion

        #region Serialized Fields

        [SerializeField] private GameObject _targetPrefabRight;
        [SerializeField] private GameObject _targetPrefabLeft;
        [SerializeField] private GameObject _splinesRightParent;
        [SerializeField] private GameObject _splinesLeftParent;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _minInterval = 1f;
        [SerializeField] private float _maxInterval = 3f;
        [SerializeField] private float _segment0Duration = 1f;
        [SerializeField] private float _segment1Duration = 3f;
        [SerializeField] private float _minXOffset = -0.5f;
        [SerializeField] private float _maxXOffset = 0.5f;
        [SerializeField] private float _minYOffset = -0.5f;
        [SerializeField] private float _maxYOffset = 0.5f;

        #endregion

        #region Fields

        private List<SplineContainer> _splinesRight;
        private List<SplineContainer> _splinesLeft;

        #endregion

        #region Setup

        private void Awake()
        {
            _splinesRight = GatherSplines(_splinesRightParent);
            _splinesLeft = GatherSplines(_splinesLeftParent);
        }

        private void Start()
        {
            RunAsync(destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private List<SplineContainer> GatherSplines(GameObject parent)
        {
            var list = new List<SplineContainer>();

            if (parent == null)
            {
                Debug.LogWarning("[TargetSpawner] Spline parent is null.", this);
                return list;
            }

            foreach (Transform child in parent.transform)
            {
                if (child.TryGetComponent(out SplineContainer spline))
                    list.Add(spline);
            }

            if (list.Count == 0)
                Debug.LogWarning($"[TargetSpawner] No SplineContainers found under '{parent.name}'.", this);

            return list;
        }

        private async UniTaskVoid RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                float delay = UnityEngine.Random.Range(_minInterval, _maxInterval);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                SpawnMode mode = (SpawnMode)UnityEngine.Random.Range(0, 3);

                float xOffset = UnityEngine.Random.Range(_minXOffset, _maxXOffset);
                float yOffset = UnityEngine.Random.Range(_minYOffset, _maxYOffset);

                switch (mode)
                {
                    case SpawnMode.Left:
                        SpawnOnSpline(_targetPrefabLeft, _splinesLeft, xOffset, yOffset, ct);
                        break;
                    case SpawnMode.Right:
                        SpawnOnSpline(_targetPrefabRight, _splinesRight, -xOffset, yOffset, ct);
                        break;
                    case SpawnMode.Both:
                        SpawnOnSpline(_targetPrefabLeft, _splinesLeft, xOffset, yOffset, ct);
                        SpawnOnSpline(_targetPrefabRight, _splinesRight, -xOffset, yOffset, ct);
                        break;
                }
            }
        }

        private void SpawnOnSpline(GameObject prefab, List<SplineContainer> splines, float xOffset, float yOffset, CancellationToken ct)
        {
            if (prefab == null || splines == null || splines.Count == 0)
            {
                Debug.LogWarning($"[TargetSpawner] Missing reference — prefab: {prefab}, splines: {splines?.Count ?? 0}", this);
                return;
            }

            SplineContainer spline = splines[UnityEngine.Random.Range(0, splines.Count)];

            if (spline == null)
            {
                Debug.LogWarning("[TargetSpawner] Null entry in splines list.", this);
                return;
            }

            GameObject instance = Instantiate(prefab, transform);
            Transform interactable = instance.transform.GetChild(0);

            Vector3 interactableLocal = interactable.localPosition;
            interactableLocal.x += xOffset;
            interactableLocal.y = _camera.transform.localPosition.y -1 + yOffset;
            //interactableLocal.y = yOffset;
            interactable.localPosition = interactableLocal;

            DriveAlongSplineAsync(instance, interactable, spline, ct).Forget();
        }

        private async UniTaskVoid DriveAlongSplineAsync(GameObject instance, Transform interactable, SplineContainer spline, CancellationToken ct)
        {
            float totalLength = spline.CalculateLength();
            float curve0Length = spline.Spline.GetCurveLength(0);
            float tMid = curve0Length / totalLength;

            await DriveSegmentAsync(instance, interactable, spline, 0f, tMid, _segment0Duration, ct);

            if (!ct.IsCancellationRequested)
                await DriveSegmentAsync(instance, interactable, spline, tMid, 1f, _segment1Duration, ct);

            Destroy(instance);
        }

        private async UniTask DriveSegmentAsync(GameObject instance, Transform interactable, SplineContainer spline, float tStart, float tEnd, float duration, CancellationToken ct)
        {
            float elapsed = 0f;

            while (elapsed < duration && !ct.IsCancellationRequested)
            {
                float t = Mathf.Lerp(tStart, tEnd, elapsed / duration);
                spline.Evaluate(t, out float3 position, out _, out _);

                instance.transform.position = (Vector3)position;

                Vector3 dir = _camera.transform.position - interactable.position;
                Vector3 euler = Quaternion.LookRotation(dir).eulerAngles;
                euler.z = 90f;
                interactable.rotation = Quaternion.Euler(euler);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }

        #endregion
    }
}