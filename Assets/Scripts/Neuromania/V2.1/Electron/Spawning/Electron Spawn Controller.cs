using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Electron.Spawning
{
    internal class ElectronSpawnController : MonoBehaviour
    {
        #region Entities

        private enum SpawnMode { Left, Right, Both }

        #endregion

        #region Serialized Fields

        [SerializeField] private ElectronSpawner _spawner;
        [SerializeField] private float _minInterval = 1f;
        [SerializeField] private float _maxInterval = 3f;
        [SerializeField] private float _minXOffset = -0.5f;
        [SerializeField] private float _maxXOffset = 0.5f;
        [SerializeField] private float _minYOffset = -0.5f;
        [SerializeField] private float _maxYOffset = 0.5f;

        #endregion

        #region Properties

        internal float MinInterval { get => _minInterval; set => _minInterval = value; }
        internal float MaxInterval { get => _maxInterval; set => _maxInterval = value; }
        internal float MinXOffset { get => _minXOffset; set => _minXOffset = value; }
        internal float MaxXOffset { get => _maxXOffset; set => _maxXOffset = value; }
        internal float MinYOffset { get => _minYOffset; set => _maxYOffset = value; }
        internal float MaxYOffset { get => _maxYOffset; set => _maxYOffset = value; }

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
                        _spawner.SpawnLeft(xOffset, yOffset);
                        break;
                    case SpawnMode.Right:
                        _spawner.SpawnRight(xOffset, yOffset);
                        break;
                    case SpawnMode.Both:
                        _spawner.SpawnLeft(xOffset, yOffset);
                        _spawner.SpawnRight(xOffset, yOffset);
                        break;
                }
            }
        }

        #endregion
    }
}