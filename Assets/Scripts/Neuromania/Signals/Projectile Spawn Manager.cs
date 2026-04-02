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

        [Serializable]
        internal struct Projectile
        {
            [SerializeField] private SplineContainer _splineContainer;
            [SerializeField] private GameObject _signalPrefab;
            [SerializeField] private SpringButton _button;
            [SerializeField] private ButtonEventType _buttonEventType;
            [SerializeField] private float _spawnRate;
            [SerializeField] private float _minSpawnRate;
            [SerializeField] private float _maxSpawnRate;

            internal SplineContainer SplineContainer => _splineContainer;
            internal GameObject SignalPrefab => _signalPrefab;
            internal SpringButton Button => _button;
            internal ButtonEventType ButtonEventType => _buttonEventType;
            internal float SpawnRate => _spawnRate;
            internal float MinSpawnRate => _minSpawnRate;
            internal float MaxSpawnRate => _maxSpawnRate;
        }

        #endregion

        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private Projectile[] _projectiles;

        #endregion

        #region Fields

        private Dictionary<SpringButton, (Action Listener, Action<float> ListenerWithPercentage, Projectile Projectile)> _listeners;
        private Dictionary<SpringButton, float> _lastSpawnTimes;

        #endregion

        #region Setup

        private void Awake()
        {
            _listeners = new Dictionary<SpringButton, (Action, Action<float>, Projectile)>();
            _lastSpawnTimes = new Dictionary<SpringButton, float>();

            foreach (var projectile in _projectiles)
            {
                var captured = projectile;
                Action listener = () => HandleButtonEvent(captured);
                Action<float> listenerWithPercentage = percentage => HandleButtonEventWithPercentage(captured, percentage);
                _listeners[captured.Button] = (listener, listenerWithPercentage, captured);
                _lastSpawnTimes[captured.Button] = 0f;
            }
        }

        private void OnEnable()
        {
            foreach (var (button, entry) in _listeners)
            {
                switch (entry.Projectile.ButtonEventType)
                {
                    case ButtonEventType.OnPressed:
                        button.OnPressed += entry.Listener;
                        break;
                    case ButtonEventType.OnHeld:
                        button.OnHeld += entry.Listener;
                        break;
                    case ButtonEventType.OnHeldWithPercentage:
                        button.OnHeldWithPercentage += entry.ListenerWithPercentage;
                        break;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var (button, entry) in _listeners)
            {
                switch (entry.Projectile.ButtonEventType)
                {
                    case ButtonEventType.OnPressed:
                        button.OnPressed -= entry.Listener;
                        break;
                    case ButtonEventType.OnHeld:
                        button.OnHeld -= entry.Listener;
                        break;
                    case ButtonEventType.OnHeldWithPercentage:
                        button.OnHeldWithPercentage -= entry.ListenerWithPercentage;
                        break;
                }
            }
        }

        #endregion

        #region Event handlers

        private void HandleButtonEvent(Projectile projectile)
        {
            if (!CanSpawn(projectile.Button, projectile.SpawnRate))
                return;

            Spawn(projectile);
        }

        private void HandleButtonEventWithPercentage(Projectile projectile, float percentage)
        {
            float interval = Mathf.Lerp(projectile.MaxSpawnRate, projectile.MinSpawnRate, percentage);

            if (!CanSpawn(projectile.Button, interval))
                return;

            Spawn(projectile);
        }

        #endregion

        #region Private

        private bool CanSpawn(SpringButton button, float interval)
        {
            if (Time.time - _lastSpawnTimes[button] < interval)
                return false;

            _lastSpawnTimes[button] = Time.time;
            return true;
        }

        private void Spawn(Projectile projectile)
        {
            _signalSpawner.SpawnAsync(projectile.SignalPrefab, projectile.SplineContainer, destroyCancellationToken).Forget();
        }

        #endregion
    }
}