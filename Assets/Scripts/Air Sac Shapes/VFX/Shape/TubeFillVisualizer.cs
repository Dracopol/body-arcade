using System;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.VFX.Tube
{
    [Serializable]
    internal class TubeFillVisualizer
    {
        #region Serialized fields

        [Header("Win Appearance")]
        [SerializeField] private Color _winColor = Color.green;

        #endregion

        #region Fields

        private MeshFilter   _meshFilter;
        private MeshRenderer _tubeRenderer;
        private Material     _material;

        private readonly Color _transparent = Color.clear;

        private static readonly int ProgressId     = Shader.PropertyToID("_Progress");
        private static readonly int UvMaxId        = Shader.PropertyToID("_UVMax");
        private static readonly int ReversedId     = Shader.PropertyToID("_Reversed");
        private static readonly int ActiveColorId  = Shader.PropertyToID("_ActiveColor");
        private static readonly int InactiveColorId = Shader.PropertyToID("_InactiveColor");

        #endregion

        #region Internal

        internal void AssignMesh(MeshFilter meshFilter, MeshRenderer meshRenderer, bool isReversed)
        {
            if (_material != null)
            {
                UnityEngine.Object.Destroy(_material);
                _material = null;
            }

            _meshFilter   = meshFilter;
            _tubeRenderer = meshRenderer;

            if (_tubeRenderer != null)
                _material = _tubeRenderer.material;

            if (_material != null)
            {
                _material.SetFloat(ReversedId, isReversed ? 1f : 0f);
                _material.SetFloat(ProgressId, 0f);
            }

            DetectUvMax();
        }

        internal void Dispose()
        {
            if (_material != null)
                UnityEngine.Object.Destroy(_material);
        }

        internal void HandleProgressChange(float progress)
        {
            if (_material == null) return;
            _material.SetFloat(ProgressId, progress);
        }

        internal void HandleWinStateChange()
        {
            if (_material == null) return;
            _material.SetColor(ActiveColorId, _winColor);
        }

        internal void HandleFirstFillComplete()
        {
            if (_material == null) return;
            _material.SetColor(ActiveColorId,   _transparent);
            _material.SetColor(InactiveColorId, _winColor);
        }

        #endregion

        #region Private

        private void DetectUvMax()
        {
            if (_material == null || _meshFilter == null || _meshFilter.sharedMesh == null) return;

            var uvs  = _meshFilter.sharedMesh.uv;
            float maxV = 0f;
            foreach (var uv in uvs)
                if (uv.y > maxV) maxV = uv.y;

            if (maxV > 0f)
                _material.SetFloat(UvMaxId, maxV);
        }

        #endregion
    }
}
