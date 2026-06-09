using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacSize : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Shape _progressTracker;
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private float _startSizeMultiplier = 1.5f;

        #endregion

        #region Fields

        private Vector3 _initialScale;
        private bool _scaleCached;
        private bool _isWin;

        #endregion

        #region Setup

        private void Awake()
        {
            ResetToStart();   // show enlarged Start size from session start, before any shape binds
        }

        private void OnEnable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange   += HandleMinMaxChanged;
                _progressTracker.OnWinStateChange += HandleWinStateChange;
            }
        }

        private void OnDisable()
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange   -= HandleMinMaxChanged;
                _progressTracker.OnWinStateChange -= HandleWinStateChange;
            }
        }

        #endregion

        #region Internal

        internal void SetShape(Shape tracker)
        {
            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange   -= HandleMinMaxChanged;
                _progressTracker.OnWinStateChange -= HandleWinStateChange;
            }

            _progressTracker = tracker;
            _isWin           = false;

            if (_progressTracker != null)
            {
                _progressTracker.OnMinMaxChange   += HandleMinMaxChanged;
                _progressTracker.OnWinStateChange += HandleWinStateChange;
            }
        }

        internal void ResetToStart()
        {
            EnsureInitialScale();
            _isWin = false;
            ApplyScale(0f);   // enlarged
        }

        #endregion

        #region Event handlers

        private void HandleMinMaxChanged(float minMax)
        {
            if (_isWin) return;
            ApplyScale(minMax);
        }

        private void HandleWinStateChange()
        {
            _isWin = true;
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

            // Start enlarged (t=0) and shrink back to original (t=1) as the player fills.
            float scaleFactor = Mathf.Lerp(_startSizeMultiplier, 1f, progress);
            _renderer.transform.localScale = _initialScale * scaleFactor;
        }

        #endregion
    }
}