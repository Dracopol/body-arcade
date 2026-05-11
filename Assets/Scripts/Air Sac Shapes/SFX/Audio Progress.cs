using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;

namespace IMVR.BodyArcade.AirSacShapes.SFX
{
    internal class AudioProgress : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private ProgressTracker _progressTracker;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _winAudioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private AudioClip _winClip;

        [Header("Behaviour")]
        [SerializeField, Range(0.01f, 1f)] private float _progressStepSize = 0.05f;
        [SerializeField, Range(0.01f, 1f)] private float _winStepSize = 0.10f;
        [SerializeField, Range(0, 1f)] private float _minPitch = 0.8f;
        [SerializeField, Range(1f, 2f)] private float _maxPitch = 2.0f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;
        [SerializeField] private int _debugLastThreshold = 0;

        #endregion

        #region Fields

        private int _lastThresholdIndex = 0;
        private int _maxThresholdIndex;
        private bool _isWinPhase = false;
        private bool _isFirstFillComplete = false;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _maxThresholdIndex = Mathf.FloorToInt(1f / _progressStepSize);

            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChanged    += HandleMaxProgressChange;
                _progressTracker.OnWinStateChange   += HandleWinStateChange;
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChanged    -= HandleMaxProgressChange;
                _progressTracker.OnWinStateChange   -= HandleWinStateChange;
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }
        }

        #endregion

        #region Event handlers

        private void HandleWinStateChange()
        {
            _isWinPhase         = true;
            _lastThresholdIndex = 0;
            _debugLastThreshold = 0;
            _maxThresholdIndex  = Mathf.FloorToInt(1f / _winStepSize);
        }

        private void HandleMaxProgressChange(float maxProgress)
        {
            _debugProgress = maxProgress;

            if (_isFirstFillComplete) return;

            float activeStepSize      = _isWinPhase ? _winStepSize : _progressStepSize;
            int currentThresholdIndex = Mathf.FloorToInt(maxProgress / activeStepSize);

            if (currentThresholdIndex > _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;

                if (currentThresholdIndex < _maxThresholdIndex)
                    PlayAudio(currentThresholdIndex);
            }
            else if (currentThresholdIndex < _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;
            }
        }

        private void HandleFirstFillComplete()
        {
            _winAudioSource.PlayOneShot(_winClip);
            _isFirstFillComplete = true;
        }

        #endregion

        #region Private

        private void PlayAudio(int thresholdIndex)
        {
            if (_audioSource == null || _clip == null) return;

            float t = _maxThresholdIndex > 0
                ? Mathf.Clamp01((float)thresholdIndex / _maxThresholdIndex)
                : 0f;

            _audioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, t);
            _audioSource.PlayOneShot(_clip);
        }

        #endregion
    }
}