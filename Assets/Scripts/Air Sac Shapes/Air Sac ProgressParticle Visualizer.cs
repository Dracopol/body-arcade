using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;

namespace IMVR.BodyArcade.AirSacShapes
{
    internal class AirSacProgressParticleVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
        [SerializeField] private ParticleSystem _particleEffectPrefab;

        [Header("Behaviour")]
        [SerializeField, Range(0.01f, 1f)] private float _progressStepSize = 0.09f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;
        [SerializeField] private int _debugLastThreshold = 0;

        #endregion

        #region Fields

        private int _lastThresholdIndex = 0;

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnProgressChange    += HandleProgressChange;
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnProgressChange    -= HandleProgressChange;
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }
        }

        #endregion

        #region Internal

        internal void SetProgressTracker(SplineProgressTracker tracker)
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnProgressChange    -= HandleProgressChange;
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
            }

            _progressTracker = tracker;

            if (_progressTracker != null)
            {
                _progressTracker.OnProgressChange    += HandleProgressChange;
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
            }
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float progress)
        {
            _debugProgress = progress;

            int currentThresholdIndex = Mathf.FloorToInt(progress / _progressStepSize);

            if (currentThresholdIndex != _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;
            }
        }

        private void HandleFirstFillComplete()
        {
            SpawnParticleEffect();
        }

        #endregion

        #region Private

        private void SpawnParticleEffect()
        {
            if (_particleEffectPrefab == null) return;
            if (!_progressTracker.IsWin) return;

            ParticleSystem instance = Instantiate(
                _particleEffectPrefab,
                transform.position,
                transform.rotation);

            var main      = instance.main;
            float lifetime = main.duration + Mathf.Max(main.startLifetime.constantMax, main.startLifetime.constant, 2f);
            Destroy(instance.gameObject, lifetime);

            _progressTracker.RegisterDestroyDelay(lifetime);
        }

        #endregion
    }
}