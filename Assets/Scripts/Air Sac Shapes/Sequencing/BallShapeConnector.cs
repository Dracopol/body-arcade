using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    [RequireComponent(typeof(LineRenderer))]
    internal class BallShapeConnector : MonoBehaviour
    {
        [SerializeField] private float _width = 0.03f;
        [SerializeField] private int _segmentCount = 20;

        private LineRenderer _lineRenderer;
        private LineRenderer _fillLineRenderer;
        private Transform _ballTransform;
        private Transform _midPoint;
        private SplineContainer _splineContainer;
        private ProgressTrackerParent _trackerParent;
        private float _fillProgress;

        #region Setup

        private void OnEnable()
        {
            if (_trackerParent != null)
                _trackerParent.OnMinMaxChange += HandleMinMaxChanged;
        }

        private void OnDisable()
        {
            if (_trackerParent != null)
                _trackerParent.OnMinMaxChange -= HandleMinMaxChanged;
        }

        #endregion

        #region Internal

        internal void Setup(Transform ballTransform, SplineContainer splineContainer,
            Material material, Material fillMaterial, Transform midPoint, ProgressTrackerParent trackerParent)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _ballTransform = ballTransform;
            _splineContainer = splineContainer;
            _midPoint = midPoint;

            ConfigureLineRenderer(_lineRenderer, material);

            var fillGO = new GameObject("ConnectorFill");
            fillGO.transform.SetParent(transform, false);
            _fillLineRenderer = fillGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(_fillLineRenderer, fillMaterial);
            _fillLineRenderer.enabled = false;

            SetProgressTrackerParent(trackerParent);
        }

        internal void SetProgressTrackerParent(ProgressTrackerParent tracker)
        {
            if (_trackerParent != null)
                _trackerParent.OnMinMaxChange -= HandleMinMaxChanged;
            _trackerParent = tracker;
            if (_trackerParent != null)
                _trackerParent.OnMinMaxChange += HandleMinMaxChanged;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_ballTransform == null || _splineContainer == null) return;

            float3 localPoint = SplineUtility.EvaluatePosition(_splineContainer.Spline, 1f);
            Vector3 splineEnd = _splineContainer.transform.TransformPoint(localPoint);

            float boundary = 1f - _fillProgress;
            int fillSegments = Mathf.RoundToInt(_fillProgress * _segmentCount);
            int baseSegments = _segmentCount - fillSegments;

            // Color A: shape end → boundary (shrinks as fill grows)
            _lineRenderer.enabled = baseSegments > 0;
            if (_lineRenderer.enabled)
            {
                _lineRenderer.positionCount = baseSegments + 1;
                for (int i = 0; i <= baseSegments; i++)
                {
                    float t = boundary * (i / (float)baseSegments);
                    _lineRenderer.SetPosition(i, SampleCurve(splineEnd, _ballTransform.position, t));
                }
            }

            // Color B: boundary → ball (grows as fill increases)
            _fillLineRenderer.enabled = fillSegments > 0;
            if (_fillLineRenderer.enabled)
            {
                _fillLineRenderer.positionCount = fillSegments + 1;
                for (int i = 0; i <= fillSegments; i++)
                {
                    float t = boundary + _fillProgress * (i / (float)fillSegments);
                    _fillLineRenderer.SetPosition(i, SampleCurve(splineEnd, _ballTransform.position, t));
                }
            }
        }

        #endregion

        #region Event handlers

        private void HandleMinMaxChanged(float progress)
        {
            _fillProgress = progress;
        }

        #endregion

        #region Private

        private Vector3 SampleCurve(Vector3 splineEnd, Vector3 ballPos, float t)
        {
            return _midPoint != null
                ? EvaluateBezier(splineEnd, _midPoint.position, ballPos, t)
                : Vector3.Lerp(splineEnd, ballPos, t);
        }

        private void ConfigureLineRenderer(LineRenderer lr, Material material)
        {
            lr.positionCount = _segmentCount + 1;
            lr.startWidth = _width;
            lr.endWidth = _width;
            lr.useWorldSpace = true;
            if (material != null)
                lr.material = material;
        }

        private static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        #endregion
    }
}
