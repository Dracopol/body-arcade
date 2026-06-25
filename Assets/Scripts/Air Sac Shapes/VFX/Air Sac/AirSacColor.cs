using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.AirSac
{
    // Applies the air sac's "sick → healed" color based on a fraction (0 = sick, 1 = healed).
    // A pure "apply fraction" primitive: the end animator drives Apply() each frame from the
    // green tube fill during the end reveal. Skipped balls use ResetToStart/SetHealthy.
    internal class AirSacColor : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
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

        #endregion

        #region Setup

        private void Awake()
        {
            ResetToStart();   // show Start colors from session start, before any shape binds
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

        #endregion

        #region Internal

        // Apply a healed fraction directly (0 = sick, 1 = healed). Driven each frame by the end
        // animator as the green tube fill rises.
        internal void Apply(float t)
        {
            EnsureMaterial();
            ApplyColor(t);
        }

        internal void ResetToStart()
        {
            EnsureMaterial();
            ApplyColor(0f);
        }

        // Skipped balls (outside the play window) start already healed: show the play-end color.
        internal void SetHealthy()
        {
            EnsureMaterial();
            ApplyColor(1f);
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