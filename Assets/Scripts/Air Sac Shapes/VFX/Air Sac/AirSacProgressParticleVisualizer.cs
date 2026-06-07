using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacProgressParticleVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private ProgressTracking.Shape _progressTracker;
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
                _progressTracker.OnMinProgressChange += HandleMinProgressChanged;
                _progressTracker.OnFirstFillComplete  += HandleFirstFillComplete;
            }
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinProgressChange -= HandleMinProgressChanged;
                _progressTracker.OnFirstFillComplete  -= HandleFirstFillComplete;
            }
        }

        #endregion

        #region Internal

        internal void SetShape(ProgressTracking.Shape tracker)
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinProgressChange -= HandleMinProgressChanged;
                _progressTracker.OnFirstFillComplete  -= HandleFirstFillComplete;
            }

            _progressTracker = tracker;

            if (_progressTracker != null)
            {
                _progressTracker.OnMinProgressChange += HandleMinProgressChanged;
                _progressTracker.OnFirstFillComplete  += HandleFirstFillComplete;
            }
        }

        #endregion

        #region Event handlers

        private void HandleMinProgressChanged(float minProgress)
        {
            _debugProgress = minProgress;

            int currentThresholdIndex = Mathf.FloorToInt(minProgress / _progressStepSize);

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

            ParticleSystem.MainModule main = instance.main;
            float lifetime = main.duration + Mathf.Max(main.startLifetime.constantMax, main.startLifetime.constant, 2f);
            Destroy(instance.gameObject, lifetime);

            _progressTracker.RegisterDestroyDelay(lifetime);
        }

        #endregion
    }
}