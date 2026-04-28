using UnityEngine;

namespace IMVR.BodyArcade.Neuromania
{
    internal class TargetHitTrigger : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TargetHitVFX _animator;

        #endregion

        #region Setup

        private void Start()
        {
            if (_animator == null)
                Debug.LogError($"[TargetHitTrigger] _animator is not assigned on {gameObject.name}");
        }

        #endregion

        #region Event Handlers

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[TargetHitTrigger] OnTriggerEnter hit by {other.gameObject.name} | tag: {other.tag}");
            if (!other.CompareTag("Glove"))
                return;
            if (_animator == null) return;

            _animator.NotifyGloveHit(other);
        }

        #endregion
    }
}