using Cysharp.Threading.Tasks;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    [RequireComponent(typeof(LineRenderer))]
    internal class BallShapeConnector : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private float _width = 0.03f;
        [SerializeField] private int _segmentCount = 20;
        [SerializeField] private float _retractDurationSeconds = 0.5f;
        [SerializeField] private float _growDurationSeconds = 0.5f;

        #endregion

        #region Fields

        private LineRenderer _lineRenderer;
        private LineRenderer _fillLineRenderer;
        private Transform _ballTransform;
        private Transform _midPoint;
        private SplineContainer _splineContainer;
        private Shape _shape;
        private float _fillProgress;
        private float _retractProgress = 1f;
        private bool _isAnimating = true;
        private bool _animationUseFill;

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_shape != null)
                _shape.OnMinMaxChange += HandleMinMaxChanged;
        }

        private void OnDisable()
        {
            if (_shape != null)
                _shape.OnMinMaxChange -= HandleMinMaxChanged;
        }

        #endregion

        #region Internal

        internal void Setup(Transform ballTransform, SplineContainer splineContainer,
            Material material, Material fillMaterial, Transform midPoint, Shape shape)
        {
            _lineRenderer  = GetComponent<LineRenderer>();
            _ballTransform = ballTransform;
            _splineContainer = splineContainer;
            _midPoint = midPoint;

            ConfigureLineRenderer(_lineRenderer, material);
            _lineRenderer.enabled = false;

            var fillGO = new GameObject("ConnectorFill");
            fillGO.transform.SetParent(transform, false);
            _fillLineRenderer = fillGO.AddComponent<LineRenderer>();
            ConfigureLineRenderer(_fillLineRenderer, fillMaterial);
            _fillLineRenderer.enabled = false;

            SetShape(shape);
        }

        internal void SetShape(Shape shape)
        {
            if (_shape != null)
                _shape.OnMinMaxChange -= HandleMinMaxChanged;
            _shape = shape;
            if (_shape != null)
                _shape.OnMinMaxChange += HandleMinMaxChanged;
        }

        internal async UniTask RetractAsync(CancellationToken ct)
        {
            if (_shape != null)
                _shape.RegisterDestroyDelay(_retractDurationSeconds);
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            await AnimateAsync(retract: true, linked.Token);
        }

        internal async UniTask GrowAsync(CancellationToken ct)
        {
            using CancellationTokenSource linked = CancellationTokenSource.CreateLinkedTokenSource(ct, destroyCancellationToken);
            await AnimateAsync(retract: false, linked.Token);
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_ballTransform == null || _splineContainer == null) return;

            float3 localPoint = SplineUtility.EvaluatePosition(_splineContainer.Spline, 1f);
            Vector3 splineEnd = _splineContainer.transform.TransformPoint(localPoint);

            if (_isAnimating)
                DrawAnimating(splineEnd);
            else
                DrawNormal(splineEnd);
        }

        #endregion

        #region Event handlers

        private void HandleMinMaxChanged(float progress)
        {
            _fillProgress = progress;
        }

        #endregion

        #region Private

        private async UniTask AnimateAsync(bool retract, CancellationToken ct)
        {
            _isAnimating      = true;
            _animationUseFill = retract;
            _retractProgress  = retract ? 0f : 1f;
            float duration    = retract ? _retractDurationSeconds : _growDurationSeconds;
            float elapsed     = 0f;

            while (elapsed < duration)
            {
                await UniTask.NextFrame(ct);
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _retractProgress = retract ? t : 1f - t;
            }

            _retractProgress = retract ? 1f : 0f;
            if (!retract)
                _isAnimating = false;
        }

        private void DrawAnimating(Vector3 splineEnd)
        {
            LineRenderer activeRenderer   = _animationUseFill ? _fillLineRenderer : _lineRenderer;
            LineRenderer inactiveRenderer = _animationUseFill ? _lineRenderer : _fillLineRenderer;
            inactiveRenderer.enabled = false;

            float visibleLength = 1f - _retractProgress;
            int segments        = Mathf.RoundToInt(visibleLength * _segmentCount);

            if (segments <= 0)
            {
                activeRenderer.enabled = false;
                return;
            }

            activeRenderer.enabled      = true;
            activeRenderer.positionCount = segments + 1;
            for (int i = 0; i <= segments; i++)
            {
                float t = _retractProgress + visibleLength * (i / (float)segments);
                activeRenderer.SetPosition(i, SampleCurve(splineEnd, _ballTransform.position, t));
            }
        }

        private void DrawNormal(Vector3 splineEnd)
        {
            float boundary    = 1f - _fillProgress;
            int fillSegments  = Mathf.RoundToInt(_fillProgress * _segmentCount);
            int baseSegments  = _segmentCount - fillSegments;

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

        private Vector3 SampleCurve(Vector3 splineEnd, Vector3 ballPos, float t)
        {
            return _midPoint != null
                ? EvaluateBezier(splineEnd, _midPoint.position, ballPos, t)
                : Vector3.Lerp(splineEnd, ballPos, t);
        }

        private void ConfigureLineRenderer(LineRenderer lr, Material material)
        {
            lr.positionCount = _segmentCount + 1;
            lr.startWidth    = _width;
            lr.endWidth      = _width;
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
