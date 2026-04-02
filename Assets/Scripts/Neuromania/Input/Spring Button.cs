using System;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Input
{
    internal class SpringButton : MonoBehaviour
    {
        #region Events

        internal event Action OnPressed;
        internal event Action OnReleased;
        internal event Action OnHeld;
        internal event Action<float> OnHeldWithPercentage;

        #endregion

        #region Serialized fields

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _pressThreshold = 0.035f;
        [SerializeField] private float _releaseThreshold = 0.025f;
        [SerializeField] private float _springStrength = 50f;
        [SerializeField] private float _maxDepth = 0.05f;
        [SerializeField] private bool _invertDirection = false;

        #endregion

        #region Fields

        private float _currentDepth;
        private bool _isPressed;
        private Vector3 _restWorldPosition;
        private Vector3 _pressAxis;

        #endregion

        #region Properties

        internal bool IsPressed => _isPressed;
        internal float PressDepth => _currentDepth;
        internal float PressDepthNormalized => Mathf.Clamp01(_currentDepth / _maxDepth);

        #endregion

        #region Setup

        private void Awake()
        {
            _restWorldPosition = transform.position;
            _pressAxis = _invertDirection ? transform.up : -transform.up;
        }

        #endregion

        #region Update

        private void FixedUpdate()
        {
            ApplySpring();
        }

        private void Update()
        {
            UpdateDepth();
            ClampDepth();
            EvaluatePress();
        }

        #endregion

        #region Private

        private void ApplySpring()
        {
            Vector3 displacement = _restWorldPosition - transform.position;
            _rigidbody.AddForce(displacement * _springStrength, ForceMode.Acceleration);
        }

        private void UpdateDepth()
        {
            _currentDepth = Vector3.Dot(transform.position - _restWorldPosition, _pressAxis);
        }

        private void ClampDepth()
        {
            // Prevent movement in the unintended direction (past rest position)
            if (_currentDepth < 0f)
            {
                _rigidbody.MovePosition(_restWorldPosition);
                _rigidbody.linearVelocity = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, _pressAxis);
                _currentDepth = 0f;
                return;
            }

            // Prevent movement past max depth
            if (_currentDepth > _maxDepth)
            {
                _rigidbody.MovePosition(_restWorldPosition + _pressAxis * _maxDepth);
                _rigidbody.linearVelocity = Vector3.ProjectOnPlane(_rigidbody.linearVelocity, _pressAxis);
                _currentDepth = _maxDepth;
            }
        }

        private void EvaluatePress()
        {
            if (!_isPressed && _currentDepth >= _pressThreshold)
            {
                _isPressed = true;
                OnPressed?.Invoke();
            }
            else if (_isPressed && _currentDepth < _releaseThreshold)
            {
                _isPressed = false;
                OnReleased?.Invoke();
            }
            else if (_isPressed)
            {
                OnHeld?.Invoke();
                OnHeldWithPercentage?.Invoke(PressDepthNormalized);
            }
        }

        #endregion
    }
}