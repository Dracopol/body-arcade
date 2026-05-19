using System;
using UnityEngine;
using UnityEngine.UI;

namespace IMVR.BodyArcade.UI
{
    public class TimerUI : MonoBehaviour
    {
        #region Events

        public event Action OnTimerExpired;

        #endregion

        #region Serialized Fields

        [SerializeField] private Image _fillImage;
        [SerializeField] private float _editorDurationSeconds = 30f; // fallback when no SessionConfig

        #endregion

        #region Fields

        private float _durationSeconds;
        private float _timeRemaining;
        private bool  _isRunning;
        private bool  _isDisabled;

        #endregion

        #region Properties

        public float Progress   => _durationSeconds > 0f ? _timeRemaining / _durationSeconds : 1f;
        public bool  IsRunning  => _isRunning;
        public bool  IsDisabled => _isDisabled;

        #endregion

        #region Setup

        private void Awake()
        {
            var config = SessionConfigAirSacShapes.Instance;

            if (config != null)
            {
                _isDisabled      = !config.TimerSeconds.HasValue;
                _durationSeconds = config.TimerSeconds.HasValue
                    ? config.TimerSeconds.Value
                    : 0f;
            }
            else
            {
                _isDisabled      = false;
                _durationSeconds = _editorDurationSeconds;
            }

            _timeRemaining        = _durationSeconds;
            _fillImage.fillAmount = _isDisabled ? 0f : 1f;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining        = Mathf.Max(_timeRemaining - Time.deltaTime, 0f);
            _fillImage.fillAmount = Progress;

            if (_timeRemaining > 0f) return;

            _isRunning = false;
            OnTimerExpired?.Invoke();
        }

        #endregion

        #region Public

        public void StartTimer()
        {
            if (_isDisabled) return;

            _isRunning = true;
        }

        public void StopTimer() => _isRunning = false;

        public void ResetTimer()
        {
            _isRunning            = false;
            _timeRemaining        = _durationSeconds;
            _fillImage.fillAmount = _isDisabled ? 0f : 1f;
        }

        public void RestartTimer()
        {
            ResetTimer();
            StartTimer();
        }

        #endregion
    }
}