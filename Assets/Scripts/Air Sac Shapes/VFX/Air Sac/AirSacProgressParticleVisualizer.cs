using System.Collections;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacProgressParticleVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Shape _progressTracker;
        [SerializeField] private ParticleSystem _particleEffectPrefab;
        [SerializeField] private Transform _scaleTarget;

        [Header("Punch")]
        [SerializeField] private float _punchScale    = 0.7f;
        [SerializeField] private float _punchDuration = 0.08f;
        [SerializeField] private float _returnDuration = 0.2f;

        #endregion

        #region Fields

        private Vector3 _originalScale;
        private bool    _scaleCached;

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
            if (_scaleTarget != null)
            {
                EnsureScale();
                StopAllCoroutines();
                StartCoroutine(PunchCoroutine());
            }
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

        private void EnsureScale()
        {
            if (_scaleCached) return;
            _originalScale = _scaleTarget.localScale;
            _scaleCached   = true;
        }

        private IEnumerator PunchCoroutine()
        {
            // Squish down
            float elapsed = 0f;
            while (elapsed < _punchDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _punchDuration);
                _scaleTarget.localScale = Vector3.Lerp(_originalScale, _originalScale * _punchScale, t);
                yield return null;
            }

            // Spring back
            elapsed = 0f;
            while (elapsed < _returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _returnDuration);
                _scaleTarget.localScale = Vector3.Lerp(_originalScale * _punchScale, _originalScale, t);
                yield return null;
            }

            _scaleTarget.localScale = _originalScale;
        }

        #endregion
    }
}