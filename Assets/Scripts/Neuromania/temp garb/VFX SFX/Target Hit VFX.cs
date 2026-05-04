#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
using UnityEngine;

namespace IMVR.BodyArcade.Neuromania.VFXSFX
{
    internal class TargetHitVFX : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GlassBreakEffect _glassBreak;
        [SerializeField] private FlyAwayEffect _flyAway;

        #endregion

        #region Setup

        private void Start()
        {
            if (_glassBreak == null)
                Debug.LogError($"[TargetHitVFX] _glassBreak is not assigned on {gameObject.name}");

            if (_flyAway == null)
                Debug.LogError($"[TargetHitVFX] _flyAway is not assigned on {gameObject.name}");
        }

        #endregion

        #region Update

#if UNITY_EDITOR
        private void Update()
        {
            if (Keyboard.current.gKey.wasPressedThisFrame)
                OnHit();

            if (Keyboard.current.deleteKey.wasPressedThisFrame)
                _glassBreak.Play();
        }
#endif

        #endregion

        #region Internal

        internal void OnHit()
        {
            _glassBreak.Play();
            _flyAway.Play();
        }

        #endregion
    }
}