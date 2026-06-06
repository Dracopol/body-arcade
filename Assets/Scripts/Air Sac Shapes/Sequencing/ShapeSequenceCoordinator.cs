using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    internal class ShapeSequenceCoordinator : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private BallSequencer _sequencer;
        [SerializeField] private ShapeSpawner _spawner;
        [SerializeField] private BallAlignToCamera _alignToCamera;

        #endregion

        #region Fields

        private CancellationTokenSource _cts;
        private UniTaskCompletionSource _appearTcs;
        private bool _isSpawning;

        #endregion

        #region Setup

        private void Awake()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnEnable()
        {
            _spawner.OnShapeWinFill        += HandleShapeWinFill;
            _spawner.OnShapeDestroyed      += HandleShapeDestroyed;
            _spawner.OnShapeAppearComplete += HandleShapeAppearComplete;
            _alignToCamera.OnAlignComplete += HandleAlignComplete;
        }

        private void OnDisable()
        {
            _spawner.OnShapeWinFill        -= HandleShapeWinFill;
            _spawner.OnShapeDestroyed      -= HandleShapeDestroyed;
            _spawner.OnShapeAppearComplete -= HandleShapeAppearComplete;
            _alignToCamera.OnAlignComplete -= HandleAlignComplete;
        }

        #endregion

        #region Event handlers

        private void HandleShapeWinFill()
        {
            RetractConnectorAsync(_cts.Token).Forget();
        }

        private void HandleShapeDestroyed()
        {
            _sequencer.Advance();
        }

        private void HandleAlignComplete()
        {
            if (_isSpawning) return;
            GameObject ball = _sequencer.CurrentBall;
            if (ball == null) return;
            SpawnSequenceAsync(ball, _cts.Token).Forget();
        }

        private void HandleShapeAppearComplete()
        {
            _appearTcs?.TrySetResult();
            _appearTcs = null;
        }

        #endregion

        #region Private

        private async UniTaskVoid RetractConnectorAsync(CancellationToken ct)
        {
            try
            {
                BallShapeConnector connector = _spawner.CurrentConnector;
                if (connector == null) return;
                await connector.RetractAsync(ct);
            }
            catch (OperationCanceledException) { }
        }

        private async UniTaskVoid SpawnSequenceAsync(GameObject ball, CancellationToken ct)
        {
            _isSpawning = true;
            try
            {
                _spawner.SpawnHidden(ball);
                BallShapeConnector connector = _spawner.CurrentConnector;
                if (connector != null)
                    await connector.GrowAsync(ct);
                _spawner.ShowCurrentShape();

                _appearTcs = new UniTaskCompletionSource();
                _spawner.TriggerAppear();
                await _appearTcs.Task.AttachExternalCancellation(ct);
            }
            catch (OperationCanceledException) { }
            finally
            {
                _appearTcs  = null;
                _isSpawning = false;
            }
        }

        #endregion
    }
}
