using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracker;

namespace IMVR.BodyArcade.AirSacShapes.Visualizers
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
                _progressTracker.OnProgressChange += HandleProgressChange;
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
                _progressTracker.OnProgressChange -= HandleProgressChange;
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float progress)
        {
            _debugProgress = progress;

            int currentThresholdIndex = Mathf.FloorToInt(progress / _progressStepSize);

            if (currentThresholdIndex > _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;

                SpawnParticleEffect();
            }
            else if (currentThresholdIndex < _lastThresholdIndex)
            {
                _lastThresholdIndex = currentThresholdIndex;
                _debugLastThreshold = currentThresholdIndex;
            }
        }

        #endregion

        #region Private

        private void SpawnParticleEffect()
        {
            if (_particleEffectPrefab == null)
                return;

            ParticleSystem instance = Instantiate(
                _particleEffectPrefab,
                transform.position,
                transform.rotation);

            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }

        #endregion
    }
}