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
            [SerializeField] private bool _spawnOnHeld;

            internal SplineContainer SplineContainer => _splineContainer;
            internal GameObject SignalPrefab => _signalPrefab;
            internal SpringButton Button => _button;
            internal bool SpawnOnHeld => _spawnOnHeld;
        }

        #endregion

        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private Projectile[] _projectiles;
        [SerializeField] private float _spawnRate = 0.2f;

        #endregion

        #region Fields

        private Dictionary<SpringButton, (Action Listener, bool SpawnOnHeld)> _listeners;
        private float _lastSpawnTime;

        #endregion

        #region Setup

        private void Awake()
        {
            _listeners = new Dictionary<SpringButton, (Action, bool)>();

            foreach (var projectile in _projectiles)
            {
                var captured = projectile;
                Action listener = () => HandleButtonPressed(captured);
                _listeners[captured.Button] = (listener, captured.SpawnOnHeld);
            }
        }

        private void OnEnable()
        {
            foreach (var (button, entry) in _listeners)
            {
                if (entry.SpawnOnHeld)
                    button.OnHeld += entry.Listener;
                else
                    button.OnPressed += entry.Listener;
            }
        }

        private void OnDisable()
        {
            foreach (var (button, entry) in _listeners)
            {
                if (entry.SpawnOnHeld)
                    button.OnHeld -= entry.Listener;
                else
                    button.OnPressed -= entry.Listener;
            }
        }

        #endregion

        #region Event handlers

        private void HandleButtonPressed(Projectile projectile)
        {
            if (Time.time - _lastSpawnTime < _spawnRate)
                return;

            _lastSpawnTime = Time.time;
            _signalSpawner.SpawnAsync(projectile.SignalPrefab, projectile.SplineContainer, destroyCancellationToken).Forget();
        }

        #endregion
    }
}