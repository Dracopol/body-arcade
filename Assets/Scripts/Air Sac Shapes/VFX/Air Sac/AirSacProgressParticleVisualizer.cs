using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacProgressParticleVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Shape _progressTracker;
        [SerializeField] private ParticleSystem _particleEffectPrefab;

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_progressTracker != null)
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;
        }

        #endregion

        #region Internal

        internal void SetShape(Shape tracker)
        {
            if (_progressTracker != null)
                _progressTracker.OnFirstFillComplete -= HandleFirstFillComplete;

            _progressTracker = tracker;

            if (_progressTracker != null)
                _progressTracker.OnFirstFillComplete += HandleFirstFillComplete;
        }

        #endregion

        #region Event handlers

        private void HandleFirstFillComplete()
        {
            SpawnParticleEffect();
        }

        #endregion

        #region Private

        private void SpawnParticleEffect()
        {
            if (_particleEffectPrefab == null) return;

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