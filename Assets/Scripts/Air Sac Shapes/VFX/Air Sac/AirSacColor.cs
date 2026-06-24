using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    internal class AirSacColor : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Shape _progressTracker;
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private Color _startCellColor = new Color32(0xC3, 0x29, 0x29, 0xFF);
        [SerializeField] private Color _startLineColor = new Color32(0x95, 0x50, 0x50, 0xFF);

        #endregion

        #region Fields

        private static readonly int CellColorId = Shader.PropertyToID("_Cell_Color");
        private static readonly int LineColorId  = Shader.PropertyToID("_Outside_LineColor");

        private Material _material;
        private Color _baseCellColor;
        private Color _baseLineColor;
        private bool _isWin;

        #endregion

        #region Setup

        private void Awake()
        {
            ResetToStart();   // show Start colors from session start, before any shape binds
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

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
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
            EnsureMaterial();
            _isWin = false;
            ApplyColor(0f);
        }

        // Skipped balls (outside the play window) start already healed: show the play-end color
        // and freeze so a later bind can't drag them back to the unhealthy Start look.
        internal void SetHealthy()
        {
            EnsureMaterial();
            ApplyColor(1f);
            _isWin = true;
        }

        #endregion

        #region Event handlers

        private void HandleMinMaxChanged(float minMax)
        {
            if (_isWin) return;   // hold the play-end colors through the win animation
            ApplyColor(minMax);
        }

        private void HandleWinStateChange()
        {
            _isWin = true;
        }

        #endregion

        #region Private

        private void EnsureMaterial()
        {
            if (_material != null || _renderer == null) return;

            _material      = _renderer.material;
            _baseCellColor = _material.GetColor(CellColorId);
            _baseLineColor = _material.GetColor(LineColorId);
        }

        private void ApplyColor(float t)
        {
            if (_material == null) return;

            _material.SetColor(CellColorId, Color.Lerp(_startCellColor, _baseCellColor, t));
            _material.SetColor(LineColorId, Color.Lerp(_startLineColor, _baseLineColor, t));
        }

        #endregion
    }
}