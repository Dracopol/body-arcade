using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.UiRobot;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    // Pre-instantiates one disabled instance of every selected shape and hands them
    // out in shuffle-bag order: every shape appears once (random order) before any
    // repeats, and the first pick of a new bag never equals the last shape played.
    internal class ShapePool
    {
        #region Types

        internal class PooledShape
        {
            internal Shape         Shape;
            internal GameObject    GameObject;
            internal PreparedShape Prepared;
        }

        #endregion

        #region Fields

        private readonly List<PooledShape> _all = new();
        private readonly List<PooledShape> _bag = new();
        private PooledShape _lastPlayed;

        #endregion

        #region Properties

        internal int Count => _all.Count;

        #endregion

        #region Internal

        internal void Build(List<PreparedShape> shapes, Transform parent)
        {
            _all.Clear();
            _bag.Clear();
            _lastPlayed = null;

            if (shapes == null) return;

            foreach (PreparedShape prepared in shapes)
            {
                if (prepared?.Config == null || prepared.Config.Prefab == null) continue;

                GameObject go = Object.Instantiate(
                    prepared.Config.Prefab,
                    parent.position,
                    Quaternion.identity,
                    parent);

                Shape shape = go.GetComponent<Shape>();
                if (shape == null)
                {
                    Object.Destroy(go);
                    continue;
                }

                go.SetActive(false);
                _all.Add(new PooledShape { Shape = shape, GameObject = go, Prepared = prepared });
            }
        }

        internal PooledShape Next()
        {
            if (_all.Count == 0) return null;

            bool justRefilled = false;
            if (_bag.Count == 0)
            {
                _bag.AddRange(_all);
                justRefilled = true;
            }

            int index = Random.Range(0, _bag.Count);

            // On a fresh bag, avoid handing back the shape we just played last
            // (the only place a back-to-back repeat can occur). Re-roll once if needed.
            if (justRefilled && _bag.Count > 1 && _bag[index] == _lastPlayed)
                index = (index + 1) % _bag.Count;

            PooledShape picked = _bag[index];
            _bag.RemoveAt(index);
            _lastPlayed = picked;
            return picked;
        }

        #endregion
    }
}
