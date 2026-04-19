using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;

namespace IMVR.BodyArcade.AirSacShapes.VFX
{
    internal class AirSacColorProgressVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
        [SerializeField] private Renderer _renderer;

        [Header("Behaviour")]
        [SerializeField] private Gradient _colorGradient;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;

        #endregion

        #region Fields

        private static readonly int CellColorId = Shader.PropertyToID("_Cell_Color");

        private Material _material;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_renderer != null)
                _material = _renderer.material;

            if (_colorGradient == null)
                _colorGradient = CreateDefaultGradient();
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

            _material.SetColor(CellColorId, _colorGradient.Evaluate(t));
        }

        private static Gradient CreateDefaultGradient()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new(Color.red,    0.0f),
                    new(Color.yellow, 0.5f),
                    new(Color.green,  1.0f)
                },
                new GradientAlphaKey[]
                {
                    new(1f, 0f),
                    new(1f, 1f)
                }
            );
            return gradient;
        }

        #endregion
    }
}