using System;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class Signal : MonoBehaviour
    {
        #region Events

        internal event Action<Vector3, SignalType, bool> OnTargetDestroy;
        internal event Action<Vector3, SignalType, bool, float> OnCorrectTargetHit;
        internal event Action<Vector3, SignalType, bool> OnIncorrectTargetHit;
        internal event Action<Signal> OnDestroyed;

        #endregion

        #region Serialized fields

        [SerializeField] private SignalType _signalType;
        [SerializeField] private float _hitPoints = 1;
        [SerializeField] private float _movementSpeed = 1f;
        [SerializeField] private bool _isProjectile = false;

        #endregion

        #region Fields

        private float _initialHitPoints;
        private Vector3 _initialScale;
        private bool _isDead;
        private Rigidbody _rigidbody;

        #endregion

        #region Properties

        internal float MovementSpeed => _movementSpeed;
        internal bool IsProjectile => _isProjectile;
        internal Rigidbody Rigidbody => _rigidbody;
        internal SignalType Type => _signalType;

        #endregion

        #region Setup

        private void Awake()
        {
            _initialHitPoints = _hitPoints;
            _initialScale = transform.localScale;
            _rigidbody = GetComponent<Rigidbody>();
        }

        #endregion

        #region Event handlers

        private void OnTriggerEnter(Collider other)
        {
            if (_isProjectile) return;
            if (_isDead) return;

            Signal otherSignal = other.GetComponent<Signal>();
            if (otherSignal == null) return;
            if (otherSignal._isDead) return;

            if (otherSignal._signalType == _signalType)
                TargetHit(otherSignal);
            else
                HandleIncorrectProjectile(otherSignal);
        }

        #endregion

        #region Private

        private void TargetHit(Signal projectile)
        {
            if (projectile._isDead) return;

            _hitPoints -= projectile._hitPoints;

            if (_hitPoints <= 0)
            {
                _isDead = true;
                projectile._isDead = true;
                OnTargetDestroy?.Invoke(transform.position, _signalType, false);
                Destroy(projectile.gameObject);
                Destroy(gameObject);
            }
            else
            {
                projectile._isDead = true;
                float healthRatio = _hitPoints / _initialHitPoints;
                OnCorrectTargetHit?.Invoke(projectile.transform.position, projectile._signalType, projectile._isProjectile, healthRatio);
                ScaleBasedOnHealth();
                Destroy(projectile.gameObject);
            }
        }

        private void HandleIncorrectProjectile(Signal projectile)
        {
            OnIncorrectTargetHit?.Invoke(projectile.transform.position, projectile._signalType, projectile._isProjectile);
            Destroy(projectile.gameObject);
        }

        private void ScaleBasedOnHealth()
        {
            float healthRatio = _hitPoints / _initialHitPoints;
            transform.localScale = _initialScale * healthRatio;
        }

        private void OnDestroy()
        {
            OnDestroyed?.Invoke(this);
        }

        #endregion
    }
}