using System;
using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;

namespace IMVR.BodyArcade.AirSacShapes.VFX.Tube
{
    [Serializable]
    internal class TubeAnimator
    {
        #region Events

        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;

        #endregion

        #region Serialized fields

        [Header("Behaviour")]
        [SerializeField] private float _fillSpeedPerSecond      = 0.5f;
        [SerializeField] private float _disappearSpeedPerSecond = 3.5f;
        [SerializeField] private float _appearSpeedPerSecond    = 1f;

        #endregion

        #region Fields

        private SplineProgressTracker _tracker;

        private MeshRenderer _tube;
        private MeshRenderer _knob;

        private float _activeFillSpeed;
        private bool  _isWin;
        private bool  _isWinFillComplete;
        private bool  _isDisappearing;
        private bool  _isAppearing;
        private int   _winFillsRemaining;

        #endregion

        #region Internal

        internal void Init(SplineProgressTracker tracker)
        {
            _tracker         = tracker;
            _activeFillSpeed = _fillSpeedPerSecond;
        }

        internal void Tick()
        {
            if (_tracker == null) return;

            if (_isAppearing)
            {
                float appeared = Mathf.MoveTowards(_tracker.Progress, 1f, Time.deltaTime * _appearSpeedPerSecond);
                _tracker.SetProgress(appeared);
                if (_tracker.Progress >= 1f)
                {
                    _isAppearing = false;
                    _tracker.SetAnimatorDriven(false);
                    _tracker.SetProgress(0f);
                    _tracker.SetMaxProgress(0f);
                    OnAppearComplete?.Invoke();
                }
                return;
            }

            if (!_isWin) return;

            if (_isDisappearing)
            {
                float disappeared = Mathf.MoveTowards(_tracker.Progress, 0f, Time.deltaTime * _disappearSpeedPerSecond);
                _tracker.SetProgress(disappeared);
                if (_tracker.Progress <= 0f)
                {
                    _isDisappearing    = false;
                    _isWin             = false;
                    if (_knob != null) _knob.enabled = false;
                    if (_tube != null) _tube.enabled = false;
                    OnWinFillComplete?.Invoke();
                }
                return;
            }

            float filledProgress = Mathf.MoveTowards(_tracker.Progress, 1f, Time.deltaTime * _activeFillSpeed);

            if (filledProgress >= 1f)
            {
                if (_winFillsRemaining > 0)
                {
                    _winFillsRemaining--;
                    filledProgress   = 0f;
                    _activeFillSpeed = _disappearSpeedPerSecond;
                    _tracker.SetProgress(filledProgress);
                    _tracker.SetMaxProgress(filledProgress);
                    OnFirstFillComplete?.Invoke();
                    return;
                }

                if (_isWinFillComplete) return;
                _isWinFillComplete = true;
                _isDisappearing    = true;
                _tracker.SetProgress(1f);
                return;
            }

            _tracker.SetProgress(filledProgress);
            _tracker.SetMaxProgress(filledProgress);
        }

        internal void AssignRenderers(MeshRenderer tube, MeshRenderer knob)
        {
            _tube = tube;
            _knob = knob;
        }

        internal void ResetState()
        {
            _activeFillSpeed   = _fillSpeedPerSecond;
            _isWin             = false;
            _isWinFillComplete = false;
            _isDisappearing    = false;
            _isAppearing       = false;
            _winFillsRemaining = 0;

            if (_tube != null) _tube.enabled = true;
            if (_knob != null) _knob.enabled = true;
        }

        internal void TriggerAppear()
        {
            _isAppearing = true;
            _tracker.SetAnimatorDriven(true);
            _tracker.SetProgress(0f);
            _tracker.SetMaxProgress(0f);
        }

        internal void HandleWinStateChange()
        {
            _isWin             = true;
            _winFillsRemaining = 1;
            _activeFillSpeed   = _fillSpeedPerSecond;
        }

        #endregion
    }
}
