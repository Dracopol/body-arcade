using System;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Input
{
    internal class SpringButton : MonoBehaviour
    {
        #region Events

        internal event Action OnPressed;
        internal event Action OnReleased;

        #endregion

        #region Serialized fields

        [SerializeField] private float _maxDepth = 0.05f;
        [SerializeField] private float _pressThreshold = 0.035f;
        [SerializeField] private float _springStrength = 50f;

        #endregion

        #region Fields

        private float _currentDepth;
        private float _targetDepth;
        private bool _isPressed;
        private Vector3 _restWorldPosition;
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
        }

        #endregion

        #region Update

        private void FixedUpdate()
        {
            ApplySpring();
            _targetDepth = 0f;
        }

        private void Update()
        {
            UpdateDepth();
            EvaluatePress();
        }

        #endregion

        #region Event handlers

        private void OnTriggerStay(Collider other)
        {
            float fingerHeight = Vector3.Dot(other.ClosestPoint(transform.position), transform.up);
            float surfaceHeight = Vector3.Dot(transform.position, transform.up);
            float depth = surfaceHeight - fingerHeight;
            _targetDepth = Mathf.Clamp(depth, 0f, _maxDepth);
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
            _currentDepth = Vector3.Distance(transform.position, _restWorldPosition);
        }

        private void EvaluatePress()
        {
            if (!_isPressed && _currentDepth >= _pressThreshold)
            {
                _isPressed = true;
                OnPressed?.Invoke();
            }
            else if (_isPressed && _currentDepth < _pressThreshold)
            {
                _isPressed = false;
                OnReleased?.Invoke();
            }
        }

        #endregion
    }
}