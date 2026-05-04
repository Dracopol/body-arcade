using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class TargetSpawnManager : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private GameObject[] _targetPrefabs;
        [SerializeField] private SplineContainer[] _splineContainers;
        [SerializeField] private float _spawnInterval = 2f;

        #endregion

        #region Setup

        private void OnEnable()
        {
            SpawnLoopAsync(destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private async UniTask SpawnLoopAsync(CancellationToken ct)
        {
            while (true)
            {
                await UniTask.WaitForSeconds(_spawnInterval, cancellationToken: ct);

                if (_targetPrefabs.Length == 0 || _splineContainers.Length == 0) continue;

                GameObject prefab = _targetPrefabs[Random.Range(0, _targetPrefabs.Length)];
                SplineContainer spline = _splineContainers[Random.Range(0, _splineContainers.Length)];

                await _signalSpawner.SpawnAsync(prefab, spline, ct);
            }
        }

        #endregion
    }
}