using UnityEngine;

namespace IMVR.CorpusVR.Core
{
    internal class BallAlignToCamera : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _camera;
        [SerializeField] private BallSequencer _sequencer;

        [Header("Settings")]
        [SerializeField] private bool _smoothAlign;
        [SerializeField] private float _alignSpeedUnitsPerSecond = 5f;

        #endregion

        #region Fields

        private Vector3 _pivotOrigin;
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isAligning;

        #endregion

        #region Setup

        private void Awake()
        {
            _pivotOrigin = _pivot.position;
        }

        private void OnEnable()
        {
            _sequencer.OnBallChanged += HandleBallChanged;
        }

        private void OnDisable()
        {
            _sequencer.OnBallChanged -= HandleBallChanged;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_smoothAlign && _isAligning)
                ApplySmooth();
        }

        #endregion

        #region Event handlers

        private void HandleBallChanged(GameObject ball)
        {
            if (ball == null) return;
            CalculateAlignment(ball);
        }

        #endregion

        #region Private

        private void CalculateAlignment(GameObject ball)
        {
            var cameraPos = _camera != null ? _camera.position : Vector3.zero;

            float ballYAbovePivot = ball.transform.position.y - _pivot.position.y;
            _targetPosition   = _pivotOrigin;
            _targetPosition.y = cameraPos.y - ballYAbovePivot;

            Vector3 ballInPivotLocal = _pivot.InverseTransformPoint(ball.transform.position);
            var localXZ = new Vector2(ballInPivotLocal.x, ballInPivotLocal.z);

            if (localXZ.sqrMagnitude > 0.0001f)
            {
                var toCam = new Vector2(
                    cameraPos.x - _pivotOrigin.x,
                    cameraPos.z - _pivotOrigin.z);

                float worldTargetAngle = Mathf.Atan2(toCam.x, toCam.y) * Mathf.Rad2Deg;
                float localArmAngle    = Mathf.Atan2(localXZ.x, localXZ.y) * Mathf.Rad2Deg;

                _targetRotation = Quaternion.Euler(0f, worldTargetAngle - localArmAngle, 0f);
            }
            else
            {
                _targetRotation = _pivot.rotation;
            }

            if (_smoothAlign)
                _isAligning = true;
            else
                ApplyInstant();
        }

        private void ApplyInstant()
        {
            _pivot.position = _targetPosition;
            _pivot.rotation = _targetRotation;
        }

        private void ApplySmooth()
        {
            _pivot.position = Vector3.Lerp(
                _pivot.position, _targetPosition,
                Time.deltaTime * _alignSpeedUnitsPerSecond);

            _pivot.rotation = Quaternion.Slerp(
                _pivot.rotation, _targetRotation,
                Time.deltaTime * _alignSpeedUnitsPerSecond);

            bool posOk = Vector3.Distance(_pivot.position, _targetPosition) < 0.001f;
            bool rotOk = Quaternion.Angle(_pivot.rotation, _targetRotation)  < 0.1f;

            if (!posOk || !rotOk) return;

            _pivot.position = _targetPosition;
            _pivot.rotation = _targetRotation;
            _isAligning     = false;
        }

        #endregion
    }
}