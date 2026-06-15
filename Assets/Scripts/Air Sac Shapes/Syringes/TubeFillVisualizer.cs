using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
{
    internal class TubeFillVisualizer
    {
        #region Fields

        // Palette for this side — supplied once via Init (from SyringesController).
        private Color _fill        = Color.red;
        private Color _empty       = Color.white;
        private Color _win         = Color.green;
        private Color _transparent = Color.clear;

        private MeshFilter   _meshFilter;
        private MeshRenderer _tubeRenderer;
        private Material     _material;

        private static readonly int ProgressId      = Shader.PropertyToID("_Progress");
        private static readonly int UvMaxId         = Shader.PropertyToID("_UVMax");
        private static readonly int ReversedId      = Shader.PropertyToID("_Reversed");
        private static readonly int ActiveColorId   = Shader.PropertyToID("_ActiveColor");
        private static readonly int InactiveColorId = Shader.PropertyToID("_InactiveColor");

        #endregion

        #region Internal

        internal void Init(Color fill, Color empty, Color win, Color transparent)
        {
            _fill        = fill;
            _empty       = empty;
            _win         = win;
            _transparent = transparent;
        }

        internal void AssignMesh(MeshFilter meshFilter, MeshRenderer meshRenderer, bool isReversed)
        {
            _meshFilter = meshFilter;

            // Pooled renderers persist — keep one instanced material for the renderer's
            // whole life; only re-clone when the renderer actually changes. Never destroy
            // here (that would leave a persistent renderer pointing at a dead material →
            // Unity's gray fallback). Destruction happens only in Dispose.
            if (meshRenderer != _tubeRenderer)
            {
                _tubeRenderer = meshRenderer;
                _material     = _tubeRenderer != null ? _tubeRenderer.material : null;
            }

            if (_material == null) return;

            // "Chosen" state: empty-coloured tube, nothing filled, transparent backdrop.
            _material.SetFloat(ReversedId, isReversed ? 1f : 0f);
            _material.SetFloat(ProgressId, 0f);
            _material.SetColor(ActiveColorId,   _empty);
            _material.SetColor(InactiveColorId, _transparent);

            DetectUvMax();
        }

        internal void Dispose()
        {
            if (_material != null)
                Object.Destroy(_material);
        }

        internal void HandleProgressChange(float progress)
        {
            if (_material == null) return;
            _material.SetFloat(ProgressId, progress);
        }

        // Appear animation finished: switch into play colours — tube starts full of
        // antidote and drains from top down as the player pushes (Reversed=1).
        internal void HandleAppearComplete()
        {
            if (_material == null) return;
            _material.SetFloat(ReversedId, 1f);
            _material.SetColor(ActiveColorId,   _empty);
            _material.SetColor(InactiveColorId, _fill);
            _material.SetFloat(ProgressId, 0f);
        }

        internal void HandleWinStateChange()
        {
            if (_material == null) return;
            _material.SetFloat(ProgressId, 0f);
            _material.SetColor(ActiveColorId, _win);
        }

        // First win-fill reached the top: flip to the retract look — drain the win
        // colour away into transparent from the bottom up (Reversed=0).
        internal void HandleFirstFillComplete()
        {
            if (_material == null) return;
            _material.SetFloat(ReversedId, 0f);
            _material.SetColor(InactiveColorId, _transparent);
            _material.SetColor(ActiveColorId, _win);
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
