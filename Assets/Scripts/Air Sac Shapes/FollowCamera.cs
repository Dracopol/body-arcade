using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes
{
    internal class FollowCamera : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private Camera _camera;
        [SerializeField] private GameObject _target;
        [SerializeField] private bool _followCameraPosition = false;
        [SerializeField] private float _smoothSpeed = 5f;

        #endregion

        #region Fields

        private Vector3 _previousCameraPosition;
        private Vector3 _desiredPosition;
        private Vector3 _targetVelocity;

        #endregion

        #region Setup

        private void Start()
        {
            if (_camera != null)
                _previousCameraPosition = _camera.transform.position;

            if (_target != null)
                _desiredPosition = _target.transform.position;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_camera == null || _target == null)
                return;

            if (_followCameraPosition)
            {
                _desiredPosition = _camera.transform.position;
            }
            else
            {
                Vector3 delta = _camera.transform.position - _previousCameraPosition;
                _desiredPosition += delta;
            }

            _target.transform.position = Vector3.SmoothDamp(
                _target.transform.position,
                _desiredPosition,
                ref _targetVelocity,
                1f / _smoothSpeed
            );

            _previousCameraPosition = _camera.transform.position;
        }

        #endregion
    }
}