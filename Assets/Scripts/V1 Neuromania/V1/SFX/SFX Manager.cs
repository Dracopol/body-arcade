using UnityEngine;
using IMVR.BodyArcade.NeuromaniaV1.Signals;

namespace IMVR.BodyArcade.NeuromaniaV1.SFX
{
    internal class SFXManager : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private AudioClip _spawnClip;
        [SerializeField] private AudioClip _targetDestroyClip;
        [SerializeField] private AudioClip _correctTargetHitClip;
        [SerializeField] private AudioClip _incorrectTargetHitClip;
        [SerializeField] private float _correctHitMinPitch = 1f;
        [SerializeField] private float _correctHitMaxPitch = 1.5f;

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
            PlayClip(_spawnClip, signal.transform.position);
            signal.OnTargetDestroy += HandleTargetDestroy;
            signal.OnCorrectTargetHit += HandleCorrectTargetHit;
            signal.OnIncorrectTargetHit += HandleIncorrectTargetHit;
            signal.OnDestroyed += HandleSignalDestroyed;
        }

        private void HandleTargetDestroy(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayClip(_targetDestroyClip, position);
        }

        private void HandleCorrectTargetHit(Vector3 position, SignalType type, bool isProjectile, float healthRatio)
        {
            float pitch = Mathf.Lerp(_correctHitMaxPitch, _correctHitMinPitch, healthRatio);
            PlayClip(_correctTargetHitClip, position, pitch);
        }

        private void HandleIncorrectTargetHit(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayClip(_incorrectTargetHitClip, position);
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

        private void PlayClip(AudioClip clip, Vector3 position, float pitch = 1f)
        {
            if (clip == null) return;
            GameObject host = new GameObject("SFX_OneShot");
            host.transform.position = position;
            AudioSource source = host.AddComponent<AudioSource>();
            source.clip = clip;
            source.pitch = pitch;
            source.Play();
            Destroy(host, clip.length / pitch);
        }

        #endregion
    }
}