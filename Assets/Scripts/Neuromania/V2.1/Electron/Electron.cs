using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Electron
{
    internal class Electron : MonoBehaviour
    {
        #region Events

        internal event Action OnReachedEnd;
        internal event Action OnHit;

        #endregion

        #region Fields

        private Camera _camera;
        private float _xOffset;
        private float _yOffset;
        private float _segment0Duration;
        private float _segment1Duration;
        private CancellationTokenSource _animationCts;

        #endregion

        #region Internal

        internal void Launch(Camera camera, float xOffset, float yOffset, SplineContainer spline, float segment0Duration, float segment1Duration)
        {
            _camera = camera;
            _xOffset = xOffset;
            _yOffset = yOffset;
            _segment0Duration = segment0Duration;
            _segment1Duration = segment1Duration;
            AnimateAsync(spline, destroyCancellationToken).Forget();
        }

        internal void Hit()
        {
            _animationCts?.Cancel();
            OnHit?.Invoke();
        }

        #endregion

        #region Private

        private async UniTaskVoid AnimateAsync(SplineContainer spline, CancellationToken ct)
        {
            _animationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            await ElectronSplineAnimator.AnimateAsync(
                transform,
                spline,
                _camera,
                _xOffset,
                _yOffset,
                _segment0Duration,
                _segment1Duration,
                _animationCts.Token);

            if (!_animationCts.Token.IsCancellationRequested)
                OnReachedEnd?.Invoke();
        }

        #endregion
    }
}