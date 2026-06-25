using System.Collections;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    // Punch-scale + particle burst for the air sac end animation. Driven explicitly by the
    // end animator: Punch() runs on both win and fail; SpawnParticles() is win-only and
    // returns the effect lifetime so the caller can hold teardown until it finishes.
    internal class AirSacProgressParticleVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
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

        #region Internal

        // Squish-and-spring punch. Shared by win and fail.
        internal void Punch()
        {
            if (_scaleTarget == null) return;
            EnsureScale();
            StopAllCoroutines();
            StartCoroutine(PunchCoroutine());
        }

        // Spawns the celebration particle burst (win only) and returns its lifetime in seconds
        // (0 if there is no prefab) so the caller can delay teardown until the effect finishes.
        internal float SpawnParticles()
        {
            if (_particleEffectPrefab == null) return 0f;

            ParticleSystem instance = Instantiate(
                _particleEffectPrefab,
                transform.position,
                transform.rotation);

            ParticleSystem.MainModule main = instance.main;
            float lifetime = main.duration + Mathf.Max(main.startLifetime.constantMax, main.startLifetime.constant, 2f);
            Destroy(instance.gameObject, lifetime);

            return lifetime;
        }

        #endregion

        #region Private

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