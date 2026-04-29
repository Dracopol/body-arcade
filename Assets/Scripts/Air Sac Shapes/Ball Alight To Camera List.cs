using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;

namespace IMVR.CorpusVR.Core.Alignment
{
    internal class BallAlignToCameraList : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private List<Transform> _balls;
        [SerializeField] private Transform _camera;

        [Header("Extra")]
        [SerializeField] private List<GameObject> _shapes;
        [SerializeField] private GameObject _shapeParent;
        [SerializeField] private GameObject _leftHand;
        [SerializeField] private GameObject _rightHand;

        [Header("Settings")]
        [SerializeField] private bool _smoothAlign;
        [SerializeField] private float _alignSpeedUnitsPerSecond = 5f;

        #endregion

        #region Fields

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isAligning;
        private int _currentBallIndex;
        private Vector3 _pivotOrigin;

        #endregion

        #region Properties

        private Transform CurrentBall => _balls != null && _balls.Count > 0
            ? _balls[_currentBallIndex]
            : null;

        #endregion

        #region Setup

        private void Awake()
        {
            _pivotOrigin = _pivot.position;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                AdvanceBall();
                CalculateAlignment();
                SpawnRandomShape();
            }

            if (_smoothAlign && _isAligning)
                ApplySmooth();
        }

        #endregion

        #region Private

        private void AdvanceBall()
        {
            if (_balls == null || _balls.Count == 0) return;
            _currentBallIndex = (_currentBallIndex + 1) % _balls.Count;
        }

        private void CalculateAlignment()
        {
            if (CurrentBall == null) return;

            var cameraPos = _camera != null ? _camera.position : Vector3.zero;

            float ballYAbovePivot = CurrentBall.position.y - _pivot.position.y;
            _targetPosition   = _pivotOrigin;
            _targetPosition.y = cameraPos.y - ballYAbovePivot;

            Vector3 ballInPivotLocal = _pivot.InverseTransformPoint(CurrentBall.position);
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

        private void SpawnRandomShape()
        {
            if (_shapes == null || _shapes.Count == 0 || _shapeParent == null) return;

            int index = Random.Range(0, _shapes.Count);
            GameObject prefab = _shapes[index];

            if (prefab == null) return;

            GameObject spawned = Instantiate(prefab, _shapeParent.transform.position, Quaternion.identity, _shapeParent.transform);

            bool isLeft  = spawned.CompareTag("Left");
            bool isRight = spawned.CompareTag("Right");

            if (!isLeft && !isRight) return;

            GameObject handObject = isLeft ? _leftHand : _rightHand;

            if (handObject == null) return;

            SplineProgressTracker tracker = spawned.GetComponent<SplineProgressTracker>();

            if (tracker == null) return;

            tracker.SetHand(handObject.transform, handObject.GetComponent<Collider>());
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