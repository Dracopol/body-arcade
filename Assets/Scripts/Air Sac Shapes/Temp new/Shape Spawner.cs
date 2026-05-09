using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.AirSacShapes;
using IMVR.BodyArcade.AirSacShapes.ProgressTrackering;
using IMVR.BodyArcade.AirSacShapes.VFX;

namespace IMVR.CorpusVR.Core
{
    internal class ShapeSpawner : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private BallSequencer _sequencer;
        [SerializeField] private List<GameObject> _LeftShapes;
        [SerializeField] private List<GameObject> _RightShapes;
        [SerializeField] private List<GameObject> _BothShapes;
        [SerializeField] private GameObject _shapeParent;
        [SerializeField] private GameObject _leftHand;
        [SerializeField] private GameObject _rightHand;

        #endregion

        #region Fields

        private GameObject _currentShape;
        private GameObject _currentBall;
        private bool _hasSpawned;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _sequencer.OnBallChanged += HandleBallChanged;
        }

        private void OnDisable()
        {
            _sequencer.OnBallChanged -= HandleBallChanged;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_hasSpawned && _currentShape == null)
                _sequencer.Advance();
        }

        #endregion

        #region Event handlers

        private void HandleBallChanged(GameObject ball)
        {
            _currentBall = ball;
            SpawnShape();
        }

        #endregion

        #region Private

        private void SpawnShape()
        {
            if (_LeftShapes == null || _LeftShapes.Count == 0 || _shapeParent == null) return;

            int index = Random.Range(0, _LeftShapes.Count);
            GameObject prefab = _LeftShapes[index];
            if (prefab == null) return;

            _currentShape = Instantiate(
                prefab,
                _shapeParent.transform.position,
                Quaternion.identity,
                _shapeParent.transform);

            _hasSpawned = true;

            AssignHand(_currentShape);
            WireTracker(_currentShape, _currentBall);
        }

        private void AssignHand(GameObject shape)
        {
            bool isLeft  = shape.CompareTag("Left");
            bool isRight = shape.CompareTag("Right");
            if (!isLeft && !isRight) return;

            GameObject handObject = isLeft ? _leftHand : _rightHand;
            if (handObject == null) return;

            SplineProgressTracker tracker = shape.GetComponent<SplineProgressTracker>();
            tracker?.SetHand(handObject.transform, handObject.GetComponent<Collider>());
        }

        private void WireTracker(GameObject shape, GameObject ball)
        {
            if (ball == null) return;

            SplineProgressTracker tracker = shape.GetComponent<SplineProgressTracker>();
            if (tracker == null) return;

            ball.GetComponent<AirSacColor>()?.SetProgressTracker(tracker);
            ball.GetComponent<AirSacSize>()?.SetProgressTracker(tracker);
            ball.GetComponent<AirSacProgressParticleVisualizer>()?.SetProgressTracker(tracker);
        }

        #endregion
    }
}