using System;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTrackering
{
    internal class ProgressState
    {
        #region Events

        internal event Action<float> OnProgressChange;
        internal event Action<float> OnMaxProgressChange;
        internal event Action OnWinStateChange;

        #endregion

        #region Fields

        private float _progress;
        private float _maxProgress;
        private bool _isWin;

        private readonly float _winThreshold;

        #endregion

        #region Properties

        internal float Progress => _progress;
        internal float MaxProgress => _maxProgress;
        internal bool IsWin => _isWin;

        #endregion

        #region Setup

        internal ProgressState(float winThreshold)
        {
            _winThreshold = winThreshold;
        }

        #endregion

        #region Internal

        internal void SetProgress(float progress, bool drainOverride)
        {
            if (drainOverride)
            {
                SetCurrentProgress(progress);
                SetMaxProgress(progress);
                return;
            }

            if (_progress >= _winThreshold)
                progress = 1f;

            SetCurrentProgress(progress);

            if (progress <= _maxProgress)
                return;

            SetMaxProgress(progress);

            if (_isWin || progress < 1f)
                return;

            _isWin = true;
            _progress = 0f;
            _maxProgress = 0f;
            Debug.Log("Win condition reached!");
            OnWinStateChange?.Invoke();
        }

        #endregion

        #region Private

        private void SetCurrentProgress(float progress)
        {
            _progress = progress;
            OnProgressChange?.Invoke(_progress);
        }

        private void SetMaxProgress(float maxProgress)
        {
            _maxProgress = maxProgress;
            OnMaxProgressChange?.Invoke(_maxProgress);
        }

        #endregion
    }
}