using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace IMVR.BodyArcade.AirSacShapes
{
    /// <summary>
    /// Tracks hand progress along a spline by checking the hand's CapsuleCollider
    /// against the nearest point on the spline. Uses Physics.ClosestPoint to find
    /// the closest point on the hand collider to the spline point.
    /// </summary>
    internal class SplineProgressTracker : MonoBehaviour
    {
        #region Entities
        #endregion

        #region Events
        #endregion

        #region Serialized Fields

        [Header("References")]
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _tubeRenderer;
        [SerializeField] private Transform _handTransform;
        [SerializeField] private Collider _handCollider;

        [Header("Behaviour")]
        [SerializeField] private float _radiusTolerance = 1.5f;
        [SerializeField] private float _tubeRadius = 0.05f;
        [SerializeField] private bool _forwardOnly = true;
        [SerializeField] private bool _reverse = false;
        [SerializeField] private float _fillSpeed = 5f;

        [Header("Debug")]
        [SerializeField, Range(0f, 1f)] private float _debugProgress = 0f;
        [SerializeField] private bool _handInsideDebug = false;

        #endregion

        #region Fields

        private static readonly int ProgressID = Shader.PropertyToID("_Progress");
        private static readonly int UVMaxID = Shader.PropertyToID("_UVMax");
        private static readonly int ReverseID = Shader.PropertyToID("_Reverse");

        private float _progress = 0f;
        private Material _material;

        #endregion

        #region Properties
        #endregion

        #region Setup

        private void Awake()
        {
            if (_tubeRenderer != null)
            {
                _material = _tubeRenderer.material;
                _material.SetFloat(ReverseID, _reverse ? 1f : 0f);
            }
        }

        private void Start()
        {
            DetectUVMax();
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_handTransform == null || _splineContainer == null || _handCollider == null)
                return;

            float3 localPos = _splineContainer.transform
                .InverseTransformPoint(_handTransform.position);

            SplineUtility.GetNearestPoint(
    _splineContainer.Spline,
    localPos,
    out float3 nearestLocalPoint,
    out float t,
    resolution: 64);
            if (_reverse)
                t = 1f - t;

            Vector3 nearestWorldPoint = _splineContainer.transform
                .TransformPoint(nearestLocalPoint);

            Vector3 closestPointOnHand = Physics.ClosestPoint(
                nearestWorldPoint,
                _handCollider,
                _handTransform.position,
                _handTransform.rotation);

            float distanceFromSpline = Vector3.Distance(nearestWorldPoint, closestPointOnHand);

            bool handInside = distanceFromSpline <= _tubeRadius * _radiusTolerance;
            _handInsideDebug = handInside;

            if (handInside)
            {
                float newProgress = _forwardOnly ? Mathf.Max(_progress, t) : t;
                SetProgress(newProgress); // instant, no lerp
            }
            else
            {
                SetProgress(Mathf.Lerp(_progress, 0f, Time.deltaTime * _fillSpeed));
            }
        }

        #endregion

        #region Event Handlers
        #endregion

        #region Public
        #endregion

        #region Internal
        #endregion

        #region Protected
        #endregion

        #region Private

        private void DetectUVMax()
        {
            if (_material == null || _meshFilter == null || _meshFilter.sharedMesh == null)
                return;

            var uvs = _meshFilter.sharedMesh.uv;
            float maxV = 0f;
            foreach (var uv in uvs)
                if (uv.y > maxV) maxV = uv.y;

            if (maxV > 0f)
            {
                _material.SetFloat(UVMaxID, maxV);
                Debug.Log($"[SplineProgressTracker] UV max detected: {maxV}", this);
            }
            else
            {
                Debug.LogWarning("[SplineProgressTracker] Could not detect UV max — defaulting to 1.", this);
            }
        }

        private void SetProgress(float t)
        {
            _progress = t;
            _debugProgress = t;

            if (_material != null)
                _material.SetFloat(ProgressID, _progress);
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

        #endregion
    }
}