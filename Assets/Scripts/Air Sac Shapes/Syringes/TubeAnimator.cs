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

        // The end fill (win or fail) sweeps 0 → _fillTarget, then drains to 0. Win fills fully (1);
        // a fail fills only to the fraction of progress reached.
        private bool  _isEndFill;
        private float _fillTarget = 1f;
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

            if (!_isEndFill) return;

            if (_isDisappearing)
            {
                float disappeared = Mathf.MoveTowards(_tracker.Progress, 0f, Time.deltaTime * _disappearSpeedPerSecond);
                _tracker.SetProgress(disappeared);
                if (_tracker.Progress <= 0f)
                {
                    _isDisappearing    = false;
                    _isEndFill         = false;
                    if (_knob != null) _knob.enabled = false;
                    if (_tube != null) _tube.enabled = false;
                    OnWinFillComplete?.Invoke();
                }
                return;
            }

            // End fill: sweep 0 → _fillTarget once at fill speed, then hand off to the drain phase.
            float filledProgress = Mathf.MoveTowards(_tracker.Progress, _fillTarget, Time.deltaTime * _winFillSpeedPerSecond);

            if (filledProgress >= _fillTarget)
            {
                if (_isWinFillComplete) return;
                _isWinFillComplete = true;
                _isDisappearing    = true;
                _tracker.SetProgress(_fillTarget);
                _tracker.SetMaxProgress(_fillTarget);
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
            _isEndFill         = false;
            _fillTarget        = 1f;
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

        // Win: fill the tube all the way (fraction 1) before draining.
        internal void HandleWinStateChange()
        {
            _isEndFill  = true;
            _fillTarget = 1f;
        }

        // Fail: fill the tube only to the fraction of progress reached, then drain (same machine).
        internal void TriggerFailFill(float target)
        {
            _isEndFill  = true;
            _fillTarget = Mathf.Clamp01(target);
        }

        #endregion
    }
}
