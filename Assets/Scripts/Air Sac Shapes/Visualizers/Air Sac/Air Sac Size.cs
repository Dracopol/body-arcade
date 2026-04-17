using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracker;

namespace IMVR.BodyArcade.AirSacShapes.Visualizers
{
    internal class AirSacSize : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
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
            if (_progressTracker.IsWin)
                return;
            ApplyScale(maxProgress);
        }

        #endregion

        #region Private

        private void ApplyScale(float progress)
        {
            if (_renderer == null)
                return;

            float scaleFactor = Mathf.Lerp(1f, _finalSizeMultiplier, progress);
            _renderer.transform.localScale = _initialScale * scaleFactor;
        }

        #endregion
    }
}