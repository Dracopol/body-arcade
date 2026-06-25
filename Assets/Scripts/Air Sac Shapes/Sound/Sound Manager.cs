using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Sound
{
    internal class SoundManager : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _winAudioSource;
        [SerializeField] private AudioClip _progressClip;
        [SerializeField] private AudioClip _winProgressClip;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _failClip;

        [Header("Lose Life SFX")]
        [SerializeField] private AudioSource _loseLifeAudioSource;
        [SerializeField] private AudioClip _loseLifeClip;

        [Header("Plunger SFX")]
        [SerializeField] private AudioSource _plungerAudioSource;
        [SerializeField] private AudioClip _plungerAppearClip;
        [SerializeField] private AudioClip _plungerDisappearClip;

        [Header("Behaviour")]
        [SerializeField, Range(0.01f, 1f)] private float _progressStepSize = 0.05f;
        [SerializeField, Range(0.01f, 1f)] private float _winStepSize = 0.10f;
        [SerializeField, Range(0, 1f)] private float _minPitch = 0.8f;
        [SerializeField, Range(1f, 2f)] private float _maxPitch = 2.0f;
        [SerializeField, Range(0f, 1f)] private float _progressVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _winProgressVolume = 1f;

        #endregion

        #region Fields

        private Shape _progressTracker;

        private int _lastThresholdIndex = 0;
        private int _maxThresholdIndex;
        private bool _isWinPhase = false;
        private bool _isFirstFillComplete = false;

        #endregion

        #region Setup

        internal void SetShape(Shape shape)
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange      -= HandleMaxProgressChange;
                _progressTracker.OnWinStateChange    -= HandleWinStateChange;
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }

            _progressTracker       = shape;
            _isWinPhase            = false;
            _isFirstFillComplete   = false;
            _lastThresholdIndex    = 0;
            _maxThresholdIndex     = Mathf.FloorToInt(1f / _progressStepSize);

            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange      += HandleMaxProgressChange;
                _progressTracker.OnWinStateChange    += HandleWinStateChange;
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        internal void PlayPlungerAppear()
        {
            if (_plungerAudioSource != null && _plungerAppearClip != null)
                _plungerAudioSource.PlayOneShot(_plungerAppearClip);
        }

        internal void PlayPlungerDisappear()
        {
            if (_plungerAudioSource != null && _plungerDisappearClip != null)
                _plungerAudioSource.PlayOneShot(_plungerDisappearClip);
        }

        // Distinct cue for an incomplete shape (timer/lives fail). Counterpart to the win clip,
        // played directly by the spawner's fail path (the win clip fires via the Shape event).
        internal void PlayShapeFail()
        {
            if (_winAudioSource != null && _failClip != null)
                _winAudioSource.PlayOneShot(_failClip);
        }

        // Played by GameStateManager when the player loses a life (hand off the plunger).
        internal void PlayLoseLife()
        {
            if (_loseLifeAudioSource != null && _loseLifeClip != null)
                _loseLifeAudioSource.PlayOneShot(_loseLifeClip);
        }

        private void OnEnable()
        {
            _maxThresholdIndex = Mathf.FloorToInt(1f / _progressStepSize);

            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange      += HandleMaxProgressChange;
                _progressTracker.OnWinStateChange    += HandleWinStateChange;
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange      -= HandleMaxProgressChange;
                _progressTracker.OnWinStateChange    -= HandleWinStateChange;
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }
        }

        #endregion

        #region Event handlers

        private void HandleWinStateChange()
        {
            _isWinPhase         = true;
            _lastThresholdIndex = 0;
            _maxThresholdIndex  = Mathf.FloorToInt(1f / _winStepSize);
        }

        private void HandleMaxProgressChange(float maxProgress)
        {
            if (_isFirstFillComplete) return;

            float activeStepSize      = _isWinPhase ? _winStepSize : _progressStepSize;
            int currentThresholdIndex = Mathf.FloorToInt(maxProgress / activeStepSize);

            if (currentThresholdIndex > _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;

                if (currentThresholdIndex < _maxThresholdIndex)
                    PlayAudio(currentThresholdIndex);
            }
            else if (currentThresholdIndex < _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
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
            AudioClip clip   = _isWinPhase ? _winProgressClip : _progressClip;
            float     volume = _isWinPhase ? _winProgressVolume : _progressVolume;
            if (_audioSource == null || clip == null) return;

            float t = _maxThresholdIndex > 0
                ? Mathf.Clamp01((float)thresholdIndex / _maxThresholdIndex)
                : 0f;

            _audioSource.pitch = Mathf.Lerp(_minPitch, _maxPitch, t);
            _audioSource.PlayOneShot(clip, volume);
        }

        #endregion
    }
}