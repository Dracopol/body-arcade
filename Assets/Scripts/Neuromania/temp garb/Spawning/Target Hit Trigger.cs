using IMVR.BodyArcade.Neuromania.VFXSFX;
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.Spawning
{
    internal class TargetHitTrigger : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TargetHitVFX _hitVFX;

        #endregion

        #region Fields

        private bool _hasBeenHit;

        #endregion

        #region Setup

        private void Start()
        {
            if (_hitVFX == null)
                Debug.LogError($"[TargetHitTrigger] _hitVFX is not assigned on {gameObject.name}");
        }

        #endregion

        #region Event Handlers

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(Tags.Glove)) return;
            if (_hitVFX == null) return;
            if (_hasBeenHit) return;

            _hasBeenHit = true;
            _hitVFX.OnHit();
        }

        #endregion
    }
}