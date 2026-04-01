using UnityEngine;
using IMVR.BodyArcade.Neuromania.Signals;

namespace IMVR.BodyArcade.Neuromania.SFX
{
    internal class SFXManager : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private AudioClip _targetDestroyClip;
        [SerializeField] private AudioClip _projectileDestroyClip;
        [SerializeField] private AudioClip _incorrectProjectileClip;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _signalSpawner.OnSignalSpawned += HandleSignalSpawned;
        }

        private void OnDisable()
        {
            _signalSpawner.OnSignalSpawned -= HandleSignalSpawned;
        }

        #endregion

        #region Event handlers

        private void HandleSignalSpawned(Signal signal)
        {
            signal.OnTargetDestroy += HandleTargetDestroy;
            signal.OnCorrectTargetHit += HandleCorrectTargetHit;
            signal.OnIncorrectTargetHit += HandleIncorrectTargetHit;
            signal.OnDestroyed += HandleSignalDestroyed;
        }

        private void HandleTargetDestroy(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayClip(_targetDestroyClip, position);
        }

        private void HandleCorrectTargetHit(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayClip(_projectileDestroyClip, position);
        }

        private void HandleIncorrectTargetHit(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayClip(_incorrectProjectileClip, position);
        }

        private void HandleSignalDestroyed(Signal signal)
        {
            signal.OnTargetDestroy -= HandleTargetDestroy;
            signal.OnCorrectTargetHit -= HandleCorrectTargetHit;
            signal.OnIncorrectTargetHit -= HandleIncorrectTargetHit;
            signal.OnDestroyed -= HandleSignalDestroyed;
        }


        #endregion

        #region Private

        private void PlayClip(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position);
        }

        #endregion
    }
}