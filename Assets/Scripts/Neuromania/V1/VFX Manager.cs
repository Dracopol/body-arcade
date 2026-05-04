using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.NeuromaniaV1.Signals;

namespace IMVR.BodyArcade.NeuromaniaV1.VFX
{
    internal class VFXManager : MonoBehaviour
    {
        #region Entities

        [System.Serializable]
        private struct SignalEffects
        {
            [SerializeField] private SignalType _type;
            [SerializeField] private bool _isProjectile;
            [SerializeField] private ParticleSystem _effect;

            internal SignalType Type => _type;
            internal bool IsProjectile => _isProjectile;
            internal ParticleSystem Effect => _effect;
        }

        #endregion

        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private SignalEffects[] _effectsByType;

        #endregion

        #region Fields

        private Dictionary<(SignalType, bool), ParticleSystem> _effectsMap;

        #endregion

        #region Setup

        private void Awake()
        {
            _effectsMap = new Dictionary<(SignalType, bool), ParticleSystem>();
            foreach (SignalEffects entry in _effectsByType)
                _effectsMap[(entry.Type, entry.IsProjectile)] = entry.Effect;
        }

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
            PlayMappedEffect(position, type, isProjectile);
        }

        private void HandleCorrectTargetHit(Vector3 position, SignalType type, bool isProjectile, float healthRatio)
        {
            PlayMappedEffect(position, type, isProjectile);
        }

        private void HandleIncorrectTargetHit(Vector3 position, SignalType type, bool isProjectile)
        {
            PlayMappedEffect(position, type, isProjectile);
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

        private void PlayMappedEffect(Vector3 position, SignalType type, bool isProjectile)
        {
            if (_effectsMap.TryGetValue((type, isProjectile), out ParticleSystem effect))
                PlayEffect(effect, position);
        }

        private void PlayEffect(ParticleSystem effect, Vector3 position)
        {
            if (effect == null) return;

            ParticleSystem instance = Instantiate(effect, position, Quaternion.identity);
            instance.Play();

            Destroy(instance.gameObject, instance.main.duration);
        }

        #endregion
    }
}