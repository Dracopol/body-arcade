using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.SplineExtrude
{
    /// <summary>
    /// Sweeps a 2D profile along a planar spline to build a tube mesh, replacing Unity's SplineExtrude.
    /// Designed for the flat (XY) air sac shapes: no twisting at cusps, crisp corners, custom profiles.
    /// Works in edit mode; use the inspector to rebuild and to bake the mesh to an asset for prefabs.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    internal class SplineMeshExtruder : MonoBehaviour
    {
        #region Serialized fields

        [Header("Source")]
        [SerializeField] private SplineContainer _splineContainer;

        [Header("Profile")]
        [SerializeField] private SplineExtrudeUtil.Profile _profile = SplineExtrudeUtil.Profile.Circle;
        [SerializeField] private Vector2[] _customProfile;

        [Header("Circle")]
        [SerializeField] private float _radius = 0.1f;
        [SerializeField] private int   _circleSides = 12;

        [Header("Rounded Rectangle")]
        [SerializeField] private float _width = 0.2f;
        [SerializeField] private float _height = 0.1f;
        [SerializeField] private float _cornerRadius = 0.03f;
        [SerializeField] private int   _cornerSegments = 3;

        [Header("Sampling")]
        [SerializeField] private float _samplesPerUnit = 20f;
        [SerializeField] private float _sharpCornerAngleDegrees = 35f;

        [Header("Output")]
        [SerializeField] private bool _capEnds = true;
        [SerializeField] private bool _flipNormals;
        [SerializeField] private bool _rebuildOnValidate = true;

        #endregion

        #region Fields

        private MeshFilter _meshFilter;
        private Mesh       _mesh;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_splineContainer == null)
                _splineContainer = GetComponent<SplineContainer>();

            Rebuild();
        }

        #endregion

        #region Update

        private void OnValidate()
        {
            if (_rebuildOnValidate && isActiveAndEnabled)
                Rebuild();
        }

        #endregion

        #region Internal

        internal void Rebuild()
        {
            if (_splineContainer == null || _splineContainer.Spline == null)
                return;
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();

            EnsureMesh();
            SplineExtrudeUtil.Generate(_splineContainer.Spline, BuildSettings(), _mesh);
        }

        // Returns the generated mesh so editor tooling can save it as an asset.
        internal Mesh GeneratedMesh => _mesh;

        #endregion

        #region Private

        private void EnsureMesh()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh { name = $"{name} Extruded" };
                _mesh.MarkDynamic();
            }

            if (_meshFilter.sharedMesh != _mesh)
                _meshFilter.sharedMesh = _mesh;
        }

        private SplineExtrudeUtil.Settings BuildSettings()
        {
            return new SplineExtrudeUtil.Settings
            {
                Profile                 = _profile,
                CustomProfile           = _customProfile,
                Radius                  = _radius,
                CircleSides             = _circleSides,
                Width                   = _width,
                Height                  = _height,
                CornerRadius            = _cornerRadius,
                CornerSegments          = _cornerSegments,
                SamplesPerUnit          = _samplesPerUnit,
                SharpCornerAngleDegrees = _sharpCornerAngleDegrees,
                CapEnds                 = _capEnds,
                FlipNormals             = _flipNormals
            };
        }

        #endregion
    }
}
