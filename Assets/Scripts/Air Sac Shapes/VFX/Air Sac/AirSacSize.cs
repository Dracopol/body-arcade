using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacSize : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private ProgressTracking.Shape _progressTracker;
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private float _finalSizeMultiplier = 1.5f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;

        #endregion

        #region Fields

        private Vector3 _initialScale;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_renderer != null)
                _initialScale = _renderer.transform.localScale;
        }

        private void OnEnable()
        {
            if (_progressTracker != null)
                _progressTracker.OnMinMaxChange += HandleMinMaxChanged;
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
                _progressTracker.OnMinMaxChange -= HandleMinMaxChanged;
        }

        #endregion

        #region Internal

        internal void SetShape(ProgressTracking.Shape tracker)
        {
            if (_progressTracker != null)
                _progressTracker.OnMinMaxChange -= HandleMinMaxChanged;

            _progressTracker = tracker;

            if (_progressTracker != null)
                _progressTracker.OnMinMaxChange += HandleMinMaxChanged;
        }

        #endregion

        #region Event handlers

        private void HandleMinMaxChanged(float minMax)
        {
            _debugProgress = minMax;
            if (_progressTracker.IsWin) return;
            ApplyScale(minMax);
        }

        #endregion

        #region Private

        private void ApplyScale(float progress)
        {
            if (_renderer == null) return;

            float scaleFactor = Mathf.Lerp(1f, _finalSizeMultiplier, progress);
            _renderer.transform.localScale = _initialScale * scaleFactor;
        }

        #endregion
    }
}