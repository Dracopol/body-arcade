using System;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class Signal : MonoBehaviour
    {
        #region Entities

        private enum SignalType
        {
            Cube,
            Sphere,
            Spiky
        }

        #endregion

        #region Events

        internal event Action<Vector3> OnTargetDestroy;
        internal event Action<Vector3> OnProjectileDestroy;
        internal event Action<Vector3> OnIncorrectProjectile;

        #endregion

        #region Serialized fields

        [SerializeField] private SignalType _signalType;
        [SerializeField] private int _hitPoints = 1;
        [SerializeField] private float _movementSpeed = 1f;
        [SerializeField] private bool _isProjectile = false;

        #endregion

        #region Fields

        private int _initialHitPoints;
        private Vector3 _initialScale;
        private bool _isDead;
        private Rigidbody _rigidbody;

        #endregion

        #region Properties

        internal float MovementSpeed => _movementSpeed;
        internal bool IsProjectile => _isProjectile;
        internal Rigidbody Rigidbody => _rigidbody;

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
                OnTargetDestroy?.Invoke(transform.position);
                Destroy(projectile.gameObject);
                Destroy(gameObject);
            }
            else
            {
                projectile._isDead = true;
                OnProjectileDestroy?.Invoke(projectile.transform.position);
                ScaleBasedOnHealth();
                Destroy(projectile.gameObject);
            }
        }

        private void HandleIncorrectProjectile(Signal projectile)
        {
            OnIncorrectProjectile?.Invoke(projectile.transform.position);
            Destroy(projectile.gameObject);
        }

        private void ScaleBasedOnHealth()
        {
            float healthRatio = (float)_hitPoints / _initialHitPoints;
            transform.localScale = _initialScale * healthRatio;
        }

        #endregion
    }
}