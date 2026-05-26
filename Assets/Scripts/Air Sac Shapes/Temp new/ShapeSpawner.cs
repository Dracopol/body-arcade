using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;
using IMVR.BodyArcade.AirSacShapes.VFX;
using IMVR.BodyArcade.UiRobot;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.AirSacShapes
{
    internal class ShapeSpawner : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private BallSequencer _sequencer;
        [SerializeField] private List<ShapeConfig> _shapes;
        [SerializeField] private GameObject _shapeParent;
        [SerializeField] private MirrorHands _mirrorHands;

        [Header("Debug")]
        [SerializeField] private GameObject _debugActiveLeft;
        [SerializeField] private GameObject _debugActiveRight;

        #endregion

        #region Fields

        private GameObject _currentShape;
        private GameObject _currentBall;
        private bool _hasSpawned;

        #endregion

        #region Setup

        private void Start()
        {
            if (SessionConfigAirSacShapes.Instance != null && SessionConfigAirSacShapes.Instance.SelectedShapes.Count > 0)
                _shapes = SessionConfigAirSacShapes.Instance.SelectedShapes;
        }

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
            if (_shapes == null || _shapes.Count == 0 || _shapeParent == null) return;

            int index          = Random.Range(0, _shapes.Count);
            ShapeConfig config = _shapes[index];
            if (config == null || config.Prefab == null) return;

            _currentShape = Instantiate(
                config.Prefab,
                _shapeParent.transform.position,
                Quaternion.identity,
                _shapeParent.transform);

            _hasSpawned = true;

            AssignHand(_currentShape);
            WireTracker(_currentShape, _currentBall);
        }

        private void AssignHand(GameObject shape)
        {
            _debugActiveLeft  = _mirrorHands.ActiveLeft;
            _debugActiveRight = _mirrorHands.ActiveRight;

            if (shape.CompareTag("Left") || shape.CompareTag("Right"))
            {
                Debug.Log($"[ShapeSpawner] Single-handed shape: {shape.name} tag: {shape.tag}");
                AssignHandToObject(shape);
                return;
            }

            Debug.Log($"[ShapeSpawner] Both-handed shape: {shape.name} children: {shape.transform.childCount}");
            foreach (Transform child in shape.transform)
            {
                Debug.Log($"[ShapeSpawner] Child: {child.name} tag: {child.tag}");
                AssignHandToObject(child.gameObject);
            }
        }

        private void AssignHandToObject(GameObject obj)
        {
            bool isLeft  = obj.CompareTag("Left");
            bool isRight = obj.CompareTag("Right");
            if (!isLeft && !isRight) return;

            GameObject handObject = isLeft
                ? _mirrorHands.ActiveLeft
                : _mirrorHands.ActiveRight;

            if (handObject == null) return;

            SplineProgressTracker tracker = obj.GetComponent<SplineProgressTracker>();
            tracker?.SetHand(handObject.transform, handObject.GetComponent<Collider>());
        }

        private void WireTracker(GameObject shape, GameObject ball)
        {
            if (ball == null) return;

            ProgressTrackerParent tracker = shape.GetComponent<ProgressTrackerParent>();
            if (tracker == null) return;

            ball.GetComponent<AirSacColor>()?.SetProgressTrackerParent(tracker);
            ball.GetComponent<AirSacSize>()?.SetProgressTrackerParent(tracker);
            ball.GetComponent<AirSacProgressParticleVisualizer>()?.SetProgressTrackerParent(tracker);
        }

        #endregion
    }
}