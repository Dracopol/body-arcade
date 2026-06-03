using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    [RequireComponent(typeof(LineRenderer))]
    internal class BallShapeConnector : MonoBehaviour
    {
        [SerializeField] private float _width = 0.03f;
        [SerializeField] private Color _color = new Color(0.5f, 0f, 1f, 1f);
        [SerializeField] private int _segmentCount = 20;

        private LineRenderer _lineRenderer;
        private Transform _ballTransform;
        private Transform _midPoint;
        private SplineContainer _splineContainer;

        internal void Setup(Transform ballTransform, SplineContainer splineContainer, Material material, Transform midPoint)
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _ballTransform = ballTransform;
            _splineContainer = splineContainer;
            _midPoint = midPoint;

            _lineRenderer.positionCount = _midPoint != null ? _segmentCount + 1 : 2;
            _lineRenderer.startWidth = _width;
            _lineRenderer.endWidth = _width;
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;
            _lineRenderer.useWorldSpace = true;

            if (material != null)
                _lineRenderer.material = material;
        }

        private void Update()
        {
            if (_ballTransform == null || _splineContainer == null) return;

            float3 localPoint = SplineUtility.EvaluatePosition(_splineContainer.Spline, 1f);
            Vector3 splineEnd = _splineContainer.transform.TransformPoint(localPoint);

            if (_midPoint != null)
            {
                for (int i = 0; i <= _segmentCount; i++)
                {
                    float t = i / (float)_segmentCount;
                    _lineRenderer.SetPosition(i, EvaluateBezier(splineEnd, _midPoint.position, _ballTransform.position, t));
                }
            }
            else
            {
                _lineRenderer.SetPosition(0, splineEnd);
                _lineRenderer.SetPosition(1, _ballTransform.position);
            }
        }

        private static Vector3 EvaluateBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }
    }
}
