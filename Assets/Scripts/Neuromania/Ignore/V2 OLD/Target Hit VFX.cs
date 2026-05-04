using UnityEngine;
using UnityEngine.InputSystem;

namespace IMVR.BodyArcade.Neuromania.Scripts
{
    internal class TargetHitVFX : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _target;

        [Header("Fly")]
        [SerializeField] private float _flySpeedY = 3f;
        [SerializeField] private float _flySpeedX = 0f;
        [SerializeField] private float _flySpeedZ = 0f;
        [SerializeField] private float _flyAcceleration = 2f;
        [SerializeField] private AudioSource _flyAwaySound;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _rotationAcceleration = 3f;

        [Header("Glass")]
        [SerializeField] private GameObject _glass;
        [SerializeField] private ParticleSystem _glassBreakEffect;
        [SerializeField] private AudioSource _glassBreakSound;

        [Header("Detection")]
        [SerializeField] private Collider _detectionCollider;

        #endregion

        #region Fields

        private bool _isAnimating;
        private float _currentFlySpeedY;
        private float _currentFlySpeedX;
        private float _currentFlySpeedZ;
        private float _currentRotationSpeed;
        private float _rotationDirection;
        private float _shrinkElapsed;
        private float _soundDuration;
        private Vector3 _initialScale;

        #endregion

        #region Setup

        private void Start()
        {
            if (_target == null)
                Debug.LogError($"[TargetHitVFX] _target is not assigned on {gameObject.name}");

            if (_glass == null)
                Debug.LogWarning($"[TargetHitVFX] _glass is not assigned on {gameObject.name}");

            if (_glassBreakEffect == null)
                Debug.LogWarning($"[TargetHitVFX] _glassBreakEffect is not assigned on {gameObject.name}");

            if (_detectionCollider == null)
                Debug.LogError($"[TargetHitVFX] _detectionCollider is not assigned on {gameObject.name}");
            else if (!_detectionCollider.isTrigger)
                Debug.LogWarning($"[TargetHitVFX] _detectionCollider is NOT set as a trigger — OnTriggerEnter will not fire!");

            if (_target != null)
                _initialScale = _target.transform.localScale;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
                StartFlyAway();

            if (Keyboard.current.deleteKey.wasPressedThisFrame)
                BreakGlass();

            if (!_isAnimating) return;

            float dt = Time.deltaTime;

            _currentFlySpeedY = Mathf.Lerp(_currentFlySpeedY, _flySpeedY, dt * _flyAcceleration);
            _currentFlySpeedX = Mathf.Lerp(_currentFlySpeedX, _flySpeedX, dt * _flyAcceleration);
            _currentFlySpeedZ = Mathf.Lerp(_currentFlySpeedZ, _flySpeedZ, dt * _flyAcceleration);

            _target.transform.position += new Vector3(
                _currentFlySpeedX * dt,
                _currentFlySpeedY * dt,
                _currentFlySpeedZ * dt
            );

            _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, _rotationSpeed, dt * _rotationAcceleration);
            _target.transform.rotation = Quaternion.Slerp(
                _target.transform.rotation,
                _target.transform.rotation * Quaternion.Euler(0f, _rotationDirection * _currentRotationSpeed * dt, 0f),
                1f
            );

            _shrinkElapsed += dt;
            float t = Mathf.Clamp01(_shrinkElapsed / _soundDuration);
            float scaleFactor = Mathf.Lerp(1f, 0f, t);
            _target.transform.localScale = _initialScale * scaleFactor;

            if (!_flyAwaySound.isPlaying)
            {
                _isAnimating = false;
                _target.SetActive(false);
            }
        }

        #endregion

        #region Public

        public void NotifyGloveHit(Collider other)
        {
            if (!other.CompareTag("Glove")) return;
            if (_isAnimating) return;
            HandleGloveHit();
        }

        #endregion

        #region Private

        private void StartFlyAway()
        {
            if (_target == null) return;

            if (_flyAwaySound == null || _flyAwaySound.clip == null)
            {
                _target.SetActive(false);
                return;
            }

            _soundDuration = _flyAwaySound.clip.length;
            _isAnimating = true;
            _currentFlySpeedY = 0f;
            _currentFlySpeedX = 0f;
            _currentFlySpeedZ = 0f;
            _currentRotationSpeed = 0f;
            _shrinkElapsed = 0f;
            _rotationDirection = Random.value > 0.5f ? 1f : -1f;

            _target.transform.localScale = _initialScale;
            _target.SetActive(true);

            _flyAwaySound.Play();
        }

        private void BreakGlass()
        {
            if (_glass != null)
                _glass.SetActive(false);

            if (_glassBreakEffect != null)
                _glassBreakEffect.Play();

            if (_glassBreakSound != null)
                _glassBreakSound.Play();
        }

        private void HandleGloveHit()
        {
            BreakGlass();
            StartFlyAway();
        }

        #endregion
    }
}