using UnityEngine;

namespace IMVR.BodyArcade.UiRobot
{
    // Floats the robot up and down on the Y axis with a smooth sine bob around its start height.
    internal class RobotHover : MonoBehaviour
    {
        #region Serialized fields

        [Header("Hover")]
        [SerializeField] private float _amplitudeMeters = 0.1f;
        [SerializeField] private float _cyclesPerSecond = 0.5f;

        [Header("Face camera")]
        [SerializeField] private Camera _camera;
        [SerializeField] private bool   _yAxisOnly = true;   // stay upright, spin only around Y
        [SerializeField] private float  _yawOffsetDegrees = -90f;   // model's face is offset from +Z

        #endregion

        #region Fields

        private Transform _transform;
        private float _startY;
        private float _phase;

        #endregion

        #region Setup

        private void Awake()
        {
            _transform = transform;
            _startY    = _transform.position.y;
        }

        #endregion

        #region Update

        private void Update()
        {
            _phase += Time.deltaTime * _cyclesPerSecond * 2f * Mathf.PI;

            Vector3 position = _transform.position;
            position.y = _startY + Mathf.Sin(_phase) * _amplitudeMeters;
            _transform.position = position;

            FaceCamera();
        }

        #endregion

        #region Private

        private void FaceCamera()
        {
            if (_camera == null) return;

            Vector3 toCamera = _camera.transform.position - _transform.position;
            if (_yAxisOnly) toCamera.y = 0f;
            if (toCamera.sqrMagnitude < 0.0001f) return;

            _transform.rotation = Quaternion.LookRotation(toCamera) * Quaternion.Euler(0f, _yawOffsetDegrees, 0f);
        }

        #endregion
    }
}
