using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Splines;
using IMVR.BodyArcade.Neuromania.Input;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class ProjectileSpawnManager : MonoBehaviour
    {
        #region Entities

        [System.Serializable]
        private struct Projectile
        {
            [SerializeField] private SplineContainer _splineContainer;
            [SerializeField] private GameObject _signalPrefab;
            [SerializeField] private SpringButton _button;

            internal SplineContainer SplineContainer => _splineContainer;
            internal GameObject SignalPrefab => _signalPrefab;
            internal SpringButton Button => _button;
        }

        #endregion

        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private Projectile[] _projectiles;

        #endregion

        #region Fields

        private Dictionary<SpringButton, Action> _listeners;

        #endregion

        #region Setup

        private void Awake()
        {
            _listeners = new Dictionary<SpringButton, Action>();

            foreach (var projectile in _projectiles)
            {
                var captured = projectile;
                Action listener = () => HandleButtonPressed(captured);
                _listeners[captured.Button] = listener;
            }
        }

        private void OnEnable()
        {
            foreach (var (button, listener) in _listeners)
                button.OnPressed += listener;
        }

        private void OnDisable()
        {
            foreach (var (button, listener) in _listeners)
                button.OnPressed -= listener;
        }

        #endregion

        #region Event handlers

        private void HandleButtonPressed(Projectile projectile)
        {
            _signalSpawner.SpawnAsync(projectile.SignalPrefab, projectile.SplineContainer, destroyCancellationToken).Forget();
        }

        #endregion
    }
}