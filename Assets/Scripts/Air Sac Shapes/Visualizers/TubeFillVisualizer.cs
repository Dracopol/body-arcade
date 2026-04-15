using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracker;

namespace IMVR.BodyArcade.AirSacShapes.Visualizers
{
    internal class TubeFillVisualizer : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private SplineProgressTracker _progressTracker;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _tubeRenderer;

        [Header("Win Appearance")]
        [SerializeField] private Color _winColor = Color.green;

        #endregion

        #region Fields

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private static readonly int UvMaxId = Shader.PropertyToID("_UVMax");
        private static readonly int ReversedId = Shader.PropertyToID("_Reversed");
        private static readonly int ActiveColorId = Shader.PropertyToID("_ActiveColor");

        private Material _material;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_tubeRenderer != null)
                _material = _tubeRenderer.material;
        }

        private void Start()
        {
            DetectUvMax();

            if (_material != null && _progressTracker != null)
                _material.SetFloat(ReversedId, _progressTracker.IsReversed ? 1f : 0f);
        }

        private void OnEnable()
        {
            if (_progressTracker != null)
                _progressTracker.OnProgressChange += HandleProgressChange;
                _progressTracker.OnWinStateChange += HandleWinStateChange;

        }

        private void OnDisable()
        {
            if (_progressTracker != null)
                _progressTracker.OnProgressChange -= HandleProgressChange;
                _progressTracker.OnWinStateChange -= HandleWinStateChange;
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

        #endregion

        #region Event handlers

        private void HandleProgressChange(float progress)
        {
            if (_material == null) return;

            _material.SetFloat(ProgressId, progress);

        }

        private void HandleWinStateChange()
        {
            if (_progressTracker == null)
                return;

            ApplyWinAppearance();
        }

        #endregion

        #region Private

        private void DetectUvMax()
        {
            if (_material == null || _meshFilter == null || _meshFilter.sharedMesh == null)
                return;

            var uvs = _meshFilter.sharedMesh.uv;
            float maxV = 0f;

            foreach (var uv in uvs)
                if (uv.y > maxV) maxV = uv.y;

            if (maxV > 0f)
                _material.SetFloat(UvMaxId, maxV);
        }

        private void ApplyWinAppearance()
        {
            _material.SetColor(ActiveColorId, _winColor);
        }

        #endregion
    }
}