using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    // Applies the air sac's size based on a fraction (0 = default, 1 = enlarged/healed).
    // A pure "apply fraction" primitive: the end animator drives Apply() each frame from the
    // green tube fill during the end reveal. Skipped balls use ResetToStart/SetHealthy.
    internal class AirSacSize : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private float _healthySizeMultiplier = 1.5f;

        #endregion

        #region Fields

        private Vector3 _initialScale;
        private bool _scaleCached;

        #endregion

        #region Setup

        private void Awake()
        {
            ResetToStart();   // show default Start size from session start, before any shape binds
        }

        #endregion

        #region Internal

        // Apply a healed fraction directly (0 = default, 1 = enlarged/healed). Driven each frame
        // by the end animator as the green tube fill rises.
        internal void Apply(float progress)
        {
            EnsureInitialScale();
            ApplyScale(progress);
        }

        internal void ResetToStart()
        {
            EnsureInitialScale();
            ApplyScale(0f);   // default size — antidote not yet injected
        }

        // Skipped balls (outside the play window) start already healed: show the enlarged play-end size.
        internal void SetHealthy()
        {
            EnsureInitialScale();
            ApplyScale(1f);   // enlarged — already healed
        }

        #endregion

        #region Private

        private void EnsureInitialScale()
        {
            if (_scaleCached || _renderer == null) return;
            _initialScale = _renderer.transform.localScale;
            _scaleCached  = true;
        }

        private void ApplyScale(float progress)
        {
            if (_renderer == null) return;

            // Start at default size (t=0) and grow above it (t=1) as antidote fills the air sac.
            float scaleFactor = Mathf.Lerp(1f, _healthySizeMultiplier, progress);
            _renderer.transform.localScale = _initialScale * scaleFactor;
        }

        #endregion
    }
}