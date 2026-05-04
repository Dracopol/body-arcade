using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Spawning
{
    internal class TargetSpawner : MonoBehaviour
    {
        #region Entities

        private enum SpawnMode { Left, Right, Both }

        #endregion

        #region Serialized Fields

        [SerializeField] private SplineRegistry _splineRegistry;
        [SerializeField] private GameObject _targetPrefabRight;
        [SerializeField] private GameObject _targetPrefabLeft;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _minInterval = 1f;
        [SerializeField] private float _maxInterval = 3f;
        [SerializeField] private float _minXOffset = -0.5f;
        [SerializeField] private float _maxXOffset = 0.5f;
        [SerializeField] private float _minYOffset = -0.5f;
        [SerializeField] private float _maxYOffset = 0.5f;

        #endregion

        #region Setup

        private void Start()
        {
            RunAsync(destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private async UniTaskVoid RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                float delay = Random.Range(_minInterval, _maxInterval);
                await UniTask.WaitForSeconds(delay, cancellationToken: ct);

                SpawnMode mode = (SpawnMode)Random.Range(0, 3);
                float xOffset = Random.Range(_minXOffset, _maxXOffset);
                float yOffset = Random.Range(_minYOffset, _maxYOffset);

                switch (mode)
                {
                    case SpawnMode.Left:
                        Spawn(_targetPrefabLeft, _splineRegistry.SplinesLeft, xOffset, yOffset, ct);
                        break;
                    case SpawnMode.Right:
                        Spawn(_targetPrefabRight, _splineRegistry.SplinesRight, -xOffset, yOffset, ct);
                        break;
                    case SpawnMode.Both:
                        Spawn(_targetPrefabLeft, _splineRegistry.SplinesLeft, xOffset, yOffset, ct);
                        Spawn(_targetPrefabRight, _splineRegistry.SplinesRight, -xOffset, yOffset, ct);
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException(nameof(mode), mode, null);
                }
            }
        }

        private void Spawn(GameObject prefab, IReadOnlyList<SplineContainer> splines, float xOffset, float yOffset, CancellationToken ct)
        {
            if (prefab == null || splines == null || splines.Count == 0)
            {
                Debug.LogWarning($"[TargetSpawner] Missing reference — prefab: {prefab}, splines: {splines?.Count ?? 0}", this);
                return;
            }

            SplineContainer spline = splines[Random.Range(0, splines.Count)];

            if (spline == null)
            {
                Debug.LogWarning("[TargetSpawner] Null entry in splines list.", this);
                return;
            }

            GameObject instance = Instantiate(prefab, transform);

            if (!instance.TryGetComponent(out Target target))
            {
                Debug.LogWarning("[TargetSpawner] Prefab is missing Target component.", this);
                return;
            }

            target.Drive(spline, xOffset, yOffset, _camera, ct);
        }

        #endregion
    }
}