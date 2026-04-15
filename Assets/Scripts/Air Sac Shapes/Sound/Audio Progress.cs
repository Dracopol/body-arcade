using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracker;

namespace IMVR.BodyArcade.AirSacShapes.Sound
{
    internal class AudioProgress : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private AudioClip _winClip;

        [Header("Behaviour")]
        [SerializeField, Range(0.01f, 1f)] private float _progressStepSize = 0.05f;
        [SerializeField, Range(0, 1f)] private float _minPitch = 0.8f;
        [SerializeField, Range(1f, 2f)] private float _maxPitch = 2.0f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;
        [SerializeField] private int _debugLastThreshold = 0;

        #endregion

        #region Fields
        
        private int _lastThresholdIndex = 0;
        private int _maxThresholdIndex;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _maxThresholdIndex = Mathf.FloorToInt(1f / _progressStepSize);

            if (_progressTracker != null)
                _progressTracker.OnMaxProgressChange += HandleMaxProgressChange;
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
                _progressTracker.OnMaxProgressChange -= HandleMaxProgressChange;
        }

        #endregion

        #region Event handlers

        private void HandleMaxProgressChange(float maxProgress)
        {
            _debugProgress = maxProgress;

            int currentThresholdIndex = Mathf.FloorToInt(maxProgress / _progressStepSize);

            if (currentThresholdIndex > _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;

                if (currentThresholdIndex <100)
                {
                    PlayAudio(currentThresholdIndex);
                }
                else
                {
                    _audioSource.PlayOneShot(_winClip);
                }
            }
            else if (currentThresholdIndex < _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;
            }
        }

        #endregion

        #region Private

        private void PlayAudio(int thresholdIndex)
        {
            if (_audioSource == null || _clip == null)
                return;

            float t = _maxThresholdIndex > 0
                ? Mathf.Clamp01((float)thresholdIndex / _maxThresholdIndex)
                : 0f;

            _audioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, t);
            _audioSource.PlayOneShot(_clip);
        }

        #endregion
    }
}