using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Electron.Spawning
{
    internal class ElectronSpawner : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _electronPrefabRight;
        [SerializeField] private GameObject _electronPrefabLeft;
        [SerializeField] private GameObject _splinesRightParent;
        [SerializeField] private GameObject _splinesLeftParent;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _segment0Duration = 1f;
        [SerializeField] private float _segment1Duration = 3f;
        [SerializeField] private bool _destroyOnEnd = true;

        #endregion

        #region Fields

        private List<SplineContainer> _splinesRight;
        private List<SplineContainer> _splinesLeft;

        #endregion

        #region Setup

        private void Awake()
        {
            _splinesRight = GatherSplines(_splinesRightParent);
            _splinesLeft = GatherSplines(_splinesLeftParent);
        }

        #endregion

        #region Internal

        internal void SpawnLeft(float xOffset, float yOffset) =>
            SpawnOnSpline(_electronPrefabLeft, _splinesLeft, xOffset, yOffset);

        internal void SpawnRight(float xOffset, float yOffset) =>
            SpawnOnSpline(_electronPrefabRight, _splinesRight, -xOffset, yOffset);

        #endregion

        #region Private

        private List<SplineContainer> GatherSplines(GameObject parent)
        {
            var list = new List<SplineContainer>();

            if (parent == null)
            {
                Debug.LogWarning("[ElectronSpawner] Spline parent is null.", this);
                return list;
            }

            foreach (Transform child in parent.transform)
            {
                if (child.TryGetComponent(out SplineContainer spline))
                    list.Add(spline);
            }

            if (list.Count == 0)
                Debug.LogWarning($"[ElectronSpawner] No SplineContainers found under '{parent.name}'.", this);

            return list;
        }

        private void SpawnOnSpline(GameObject prefab, List<SplineContainer> splines, float xOffset, float yOffset)
        {
            if (prefab == null || splines == null || splines.Count == 0)
            {
                Debug.LogWarning($"[ElectronSpawner] Missing reference — prefab: {prefab}, splines: {splines?.Count ?? 0}", this);
                return;
            }

            SplineContainer spline = splines[Random.Range(0, splines.Count)];
            GameObject instance = Instantiate(prefab, transform);

            if (!instance.TryGetComponent(out Electron electron))
            {
                Debug.LogWarning("[ElectronSpawner] Prefab is missing Electron component.", this);
                Destroy(instance);
                return;
            }

            electron.Launch(_camera, xOffset, yOffset, spline, _segment0Duration, _segment1Duration, _destroyOnEnd);
        }

        #endregion
    }
}