using UnityEngine;
using UnityEngine.InputSystem;

namespace IMVR.CorpusVR.Core.Alignment
{
    internal class BallAlignToCamera : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _ball;
        [SerializeField] private Transform _camera;

        [Header("Settings")]
        [SerializeField] private bool _smoothAlign;
        [SerializeField] private float _alignSpeedUnitsPerSecond = 5f;

        #endregion

        #region Fields

        private Vector3 _targetPosition;
        private Quaternion _targetRotation;
        private bool _isAligning;

        #endregion

        #region Update

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
                CalculateAlignment();

            if (_smoothAlign && _isAligning)
                ApplySmooth();
        }

        #endregion

        #region Private

        private void CalculateAlignment()
        {
            var cameraPos = _camera != null ? _camera.position : Vector3.zero;

            float ballYAbovePivot = _ball.position.y - _pivot.position.y;
            _targetPosition   = _pivot.position;
            _targetPosition.y = cameraPos.y - ballYAbovePivot;

            var localXZ = new Vector2(_ball.localPosition.x, _ball.localPosition.z);

            if (localXZ.sqrMagnitude > 0.0001f)
            {
                var needed = new Vector2(
                    cameraPos.x - _pivot.position.x,
                    cameraPos.z - _pivot.position.z);

                float worldTargetAngle = Mathf.Atan2(needed.x, needed.y) * Mathf.Rad2Deg;
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