using System;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    internal class TubeAnimator
    {
        #region Events

        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;

        #endregion

        #region Fields

        // Tuning — supplied once via Init (shared from SyringesController).
        private float _winFillSpeedPerSecond   = 0.5f;
        private float _disappearSpeedPerSecond = 3.5f;
        private float _appearSpeedPerSecond    = 1f;

        private PlungerTracker _tracker;

        private MeshRenderer _tube;
        private MeshRenderer _knob;

        private bool _isWin;
        private bool _isWinFillComplete;
        private bool _isDisappearing;
        private bool _isAppearing;

        #endregion

        #region Internal

        internal void Init(PlungerTracker tracker, float winFillSpeed, float disappearSpeed, float appearSpeed)
        {
            _tracker                 = tracker;
            _winFillSpeedPerSecond   = winFillSpeed;
            _disappearSpeedPerSecond = disappearSpeed;
            _appearSpeedPerSecond    = appearSpeed;
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

            // Win fill: sweep 0→1 once at fill speed, then hand off to the drain phase.
            float filledProgress = Mathf.MoveTowards(_tracker.Progress, 1f, Time.deltaTime * _winFillSpeedPerSecond);

            if (filledProgress >= 1f)
            {
                if (_isWinFillComplete) return;
                _isWinFillComplete = true;
                _isDisappearing    = true;
                _tracker.SetProgress(1f);
                _tracker.SetMaxProgress(1f);
                OnFirstFillComplete?.Invoke();
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
            _isWin             = false;
            _isWinFillComplete = false;
            _isDisappearing    = false;
            _isAppearing       = false;

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
            _isWin = true;
        }

        #endregion
    }
}
