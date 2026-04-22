using UnityEngine;
using UnityEngine.InputSystem;

namespace IMVR.BodyArcade.Neuromania.VFX
{
    public class TargetFlyAway : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _target;

        [Header("Fly")]
        [SerializeField] private float _flySpeed = 3f;
        [SerializeField] private float _flyAcceleration = 2f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _rotationAcceleration = 3f;

        [Header("Scale")]
        [SerializeField] private float _shrinkDuration = 2f;

        #endregion

        #region Fields

        private bool _isAnimating;
        private float _currentFlySpeed;
        private float _currentRotationSpeed;
        private float _rotationDirection;
        private float _shrinkElapsed;
        private Vector3 _initialScale;

        #endregion

        #region Update

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
                StartFlyAway();

            if (!_isAnimating) return;

            float dt = Time.deltaTime;

            // Fly up — accelerate smoothly
            _currentFlySpeed = Mathf.Lerp(_currentFlySpeed, _flySpeed, dt * _flyAcceleration);
            _target.transform.position += _target.transform.up * (_currentFlySpeed * dt);

            // Rotate on Y — spin up smoothly
            _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, _rotationSpeed, dt * _rotationAcceleration);
            _target.transform.rotation = Quaternion.Slerp(
                _target.transform.rotation,
                _target.transform.rotation * Quaternion.Euler(0f, _rotationDirection * _currentRotationSpeed * dt, 0f),
                1f
            );

            // Shrink to zero
            _shrinkElapsed += dt;
            float t = Mathf.Clamp01(_shrinkElapsed / _shrinkDuration);
            float scaleFactor = Mathf.Lerp(1f, 0f, t * t); // ease-in curve
            _target.transform.localScale = _initialScale * scaleFactor;

            if (t >= 1f)
            {
                _isAnimating = false;
                _target.SetActive(false);
            }
        }

        #endregion

        #region Private

        private void StartFlyAway()
        {
            if (_target == null) return;

            _isAnimating = true;
            _currentFlySpeed = 0f;
            _currentRotationSpeed = 0f;
            _shrinkElapsed = 0f;
            _initialScale = _target.transform.localScale;
            _rotationDirection = Random.value > 0.5f ? 1f : -1f;

            _target.SetActive(true);
        }

        #endregion
    }
}