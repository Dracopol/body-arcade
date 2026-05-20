using UnityEngine;
using IMVR.BodyArcade.Neuromania.Scripts;

namespace IMVR.BodyArcade.Neuromania.Electron
{
    internal class ElectronCollider : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Electron _electron;
        [SerializeField] private TargetHitVFX _animator;

        #endregion

        #region Fields

        private bool _hasBeenHit;

        #endregion

        #region Setup

        private void Start()
        {
            if (_electron == null)
                Debug.LogError($"[ElectronCollider] _electron is not assigned on {gameObject.name}");

            if (_animator == null)
                Debug.LogError($"[ElectronCollider] _animator is not assigned on {gameObject.name}");
        }

        #endregion

        #region Event Handlers

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Glove")) return;
            if (_animator == null) return;
            if (_hasBeenHit) return;

            _hasBeenHit = true;
            _electron?.Hit();
            _animator.NotifyGloveHit(other);
        }

        #endregion
    }
}