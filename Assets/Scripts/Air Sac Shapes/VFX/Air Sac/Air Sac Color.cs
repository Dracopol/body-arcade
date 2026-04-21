using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;

namespace IMVR.BodyArcade.AirSacShapes.VFX
{
    internal class AirSacColor : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private Color _startCellColor = new Color32(0xC3, 0x29, 0x29, 0xFF);
        [SerializeField] private Color _startLineColor = new Color32(0x95, 0x50, 0x50, 0xFF);

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;

        #endregion

        #region Fields

        private static readonly int CellColorId = Shader.PropertyToID("_Cell_Color");
        private static readonly int LineColorId = Shader.PropertyToID("_Outside_LineColor");

        private Material _material;
        private Color _baseCellColor;
        private Color _baseLineColor;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_renderer != null)
            {
                _material = _renderer.material;
                _baseCellColor = _material.GetColor(CellColorId);
                _baseLineColor = _material.GetColor(LineColorId);
            }
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

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

        #endregion

        #region Event handlers

        private void HandleMaxProgressChange(float maxProgress)
        {
            _debugProgress = maxProgress;
            ApplyColor(maxProgress);
        }

        #endregion

        #region Private

        private void ApplyColor(float t)
        {
            if (_material == null)
                return;

            _material.SetColor(CellColorId, Color.Lerp(_startCellColor, _baseCellColor, t));
            _material.SetColor(LineColorId, Color.Lerp(_startLineColor, _baseLineColor, t));
        }

        #endregion
    }
}