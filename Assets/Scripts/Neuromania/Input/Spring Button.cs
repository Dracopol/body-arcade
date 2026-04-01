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

        #endregion

        #region Serialized fields

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
        private Rigidbody _rigidbody;

        #endregion

        #region Properties

        internal bool IsPressed => _isPressed;

        #endregion

        #region Setup

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
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
            EvaluatePress();
            ClampDepth();
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
            _currentDepth = Vector3.Dot(_restWorldPosition - transform.position, _pressAxis);
        }

        private void ClampDepth()
        {
            if (_currentDepth <= _maxDepth)
            {
                _rigidbody.MovePosition(_restWorldPosition + _pressAxis * _maxDepth);
                _rigidbody.linearVelocity = Vector3.zero;
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
            else if (_isPressed && _currentDepth >= _pressThreshold)
            {
                OnHeld?.Invoke();
            }
        }

        #endregion
    }
}