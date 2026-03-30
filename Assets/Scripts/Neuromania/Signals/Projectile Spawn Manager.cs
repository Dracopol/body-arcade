using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Signals
{
    internal class ProjectileSpawnManager : MonoBehaviour
    {
        #region Entities

        [System.Serializable]
        private struct Projectiles
        {
            [SerializeField] private SplineContainer _splineContainer;
            [SerializeField] private GameObject _signalPrefab;
            [SerializeField] private Key _spawnKey;

            internal SplineContainer SplineContainer => _splineContainer;
            internal GameObject SignalPrefab => _signalPrefab;
            internal Key SpawnKey => _spawnKey;
        }

        #endregion

        #region Serialized fields

        [SerializeField] private SignalSpawner _signalSpawner;
        [SerializeField] private Projectiles[] _projectiles;

        #endregion

        #region Update

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            foreach (var projectile in _projectiles)
            {
                if (kb[projectile.SpawnKey].wasPressedThisFrame)
                    _signalSpawner.SpawnAsync(projectile.SignalPrefab, projectile.SplineContainer, destroyCancellationToken).Forget();
            }
        }

        #endregion
    }
}