using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.VFXSFX
{
    internal class FlyAwayEffect : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform _target;
        [SerializeField] private AudioSource _flyAwaySound;

        [Header("Fly")]
        [SerializeField] private float _flySpeedY = 3f;
        [SerializeField] private float _flySpeedX = 0f;
        [SerializeField] private float _flySpeedZ = 0f;
        [SerializeField] private float _flyAcceleration = 2f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private float _rotationAcceleration = 3f;

        #endregion

        #region Fields

        private Vector3 _initialScale;

        #endregion

        #region Setup

        private void Awake()
        {
            if (_target != null)
                _initialScale = _target.localScale;
        }

        #endregion

        #region Internal

        internal void Play()
        {
            PlayAsync(destroyCancellationToken).Forget();
        }

        #endregion

        #region Private

        private async UniTaskVoid PlayAsync(CancellationToken ct)
        {
            if (_target == null) return;

            if (_flyAwaySound == null || _flyAwaySound.clip == null)
            {
                _target.gameObject.SetActive(false);
                return;
            }

            float soundDuration = _flyAwaySound.clip.length;
            float rotationDirection = Random.value > 0.5f ? 1f : -1f;
            float currentFlySpeedY = 0f;
            float currentFlySpeedX = 0f;
            float currentFlySpeedZ = 0f;
            float currentRotationSpeed = 0f;
            float elapsed = 0f;

            _target.localScale = _initialScale;
            _target.gameObject.SetActive(true);
            _flyAwaySound.Play();

            while (_flyAwaySound.isPlaying && !ct.IsCancellationRequested)
            {
                float dt = Time.deltaTime;

                currentFlySpeedY = Mathf.Lerp(currentFlySpeedY, _flySpeedY, dt * _flyAcceleration);
                currentFlySpeedX = Mathf.Lerp(currentFlySpeedX, _flySpeedX, dt * _flyAcceleration);
                currentFlySpeedZ = Mathf.Lerp(currentFlySpeedZ, _flySpeedZ, dt * _flyAcceleration);

                _target.position += new Vector3(
                    currentFlySpeedX * dt,
                    currentFlySpeedY * dt,
                    currentFlySpeedZ * dt
                );

                currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, _rotationSpeed, dt * _rotationAcceleration);
                _target.Rotate(0f, rotationDirection * currentRotationSpeed * dt, 0f, Space.Self);

                elapsed += dt;
                _target.localScale = _initialScale * Mathf.Clamp01(1f - elapsed / soundDuration);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _target.gameObject.SetActive(false);
        }

        #endregion
    }
}