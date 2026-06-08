using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;
using IMVR.BodyArcade.AirSacShapes.VFX.AirSac;
using IMVR.BodyArcade.UiRobot;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    internal class ShapeSpawner : MonoBehaviour
    {
        #region Events

        internal event Action OnShapeDestroyed;
        internal event Action OnShapeWinFill;
        internal event Action OnShapeAppearComplete;

        #endregion

        #region Serialized fields

        [Header("References")]
        [SerializeField] private GameObject _shapeParent;
        [SerializeField] private MirrorHands _mirrorHands;
        [SerializeField] private Knob _leftKnob;
        [SerializeField] private Knob _rightKnob;

        [Header("Connector")]
        [SerializeField] private Material _connectorMaterial;
        [SerializeField] private Material _connectorFillMaterial;
        [SerializeField] private Transform _connectorMidPoint;

        [Header("Debug")]
        [SerializeField] private GameObject _debugActiveLeft;
        [SerializeField] private GameObject _debugActiveRight;

        #endregion

        #region Fields

        private List<PreparedShape> _shapes;
        private GameObject _currentShapeObject;
        private GameObject _currentBall;
        private Shape _currentShape;
        private bool _hasSpawned;

        #endregion

        #region Properties

        internal BallShapeConnector CurrentConnector { get; private set; }

        #endregion

        #region Setup

        private void Start()
        {
            if (SessionConfigAirSacShapes.Instance != null && SessionConfigAirSacShapes.Instance.SelectedShapes.Count > 0)
                _shapes = SessionConfigAirSacShapes.Instance.SelectedShapes;
        }

        #endregion

        #region Update

        private void Update()
        {
            if (_hasSpawned && _currentShapeObject == null)
            {
                CurrentConnector = null;
                _hasSpawned      = false;
                if (_leftKnob != null)  _leftKnob.SetTracker(null, null);
                if (_rightKnob != null) _rightKnob.SetTracker(null, null);
                OnShapeDestroyed?.Invoke();
            }
        }

        #endregion

        #region Internal

        internal void SpawnHidden(GameObject ball)
        {
            _currentBall = ball;
            SpawnShape();
            HideShapeRenderers();
        }

        internal void ShowCurrentShape()
        {
            if (_currentShapeObject == null) return;
            foreach (MeshRenderer r in _currentShapeObject.GetComponentsInChildren<MeshRenderer>())
                if (r != null) r.enabled = true;
        }

        internal void TriggerAppear()
        {
            if (_currentShape == null)
            {
                OnShapeAppearComplete?.Invoke();
                return;
            }
            _currentShape.TriggerAppear();
        }

        #endregion

        #region Event handlers

        private void HandleTrackerWinFill()
        {
            OnShapeWinFill?.Invoke();
        }

        private void HandleTrackerAppearComplete()
        {
            OnShapeAppearComplete?.Invoke();
        }

        #endregion

        #region Private

        private void SpawnShape()
        {
            if (_shapes == null || _shapes.Count == 0 || _shapeParent == null) return;

            int index              = UnityEngine.Random.Range(0, _shapes.Count);
            PreparedShape prepared = _shapes[index];
            if (prepared.Config == null || prepared.Config.Prefab == null) return;

            _currentShapeObject = Instantiate(
                prepared.Config.Prefab,
                _shapeParent.transform.position,
                Quaternion.identity,
                _shapeParent.transform);

            foreach (SplineProgressTracker tracker in _currentShapeObject.GetComponentsInChildren<SplineProgressTracker>())
                tracker.ApplyDifficultySettings(prepared.DrainSpeed, prepared.SplineRadius);

            _hasSpawned = true;

            SubscribeEvents();
            AssignHand(_currentShapeObject);
            AssignKnobs(_currentShapeObject);
            WireTracker(_currentShapeObject, _currentBall);
            WireConnector(_currentShapeObject, _currentBall);
        }

        private void SubscribeEvents()
        {
            Shape tracker = _currentShapeObject.GetComponent<Shape>();
            if (tracker == null) return;
            _currentShape             = tracker;
            tracker.OnWinFillComplete        += HandleTrackerWinFill;
            tracker.OnAppearComplete         += HandleTrackerAppearComplete;
        }

        private void HideShapeRenderers()
        {
            if (_currentShapeObject == null) return;
            foreach (MeshRenderer r in _currentShapeObject.GetComponentsInChildren<MeshRenderer>())
                r.enabled = false;
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

        private void AssignKnobs(GameObject shape)
        {
            if (shape.CompareTag("Left") || shape.CompareTag("Right"))
            {
                AssignKnobToObject(shape);
                return;
            }

            foreach (Transform child in shape.transform)
                AssignKnobToObject(child.gameObject);
        }

        private void AssignKnobToObject(GameObject obj)
        {
            bool isLeft  = obj.CompareTag("Left");
            bool isRight = obj.CompareTag("Right");
            if (!isLeft && !isRight) { Debug.Log($"[ShapeSpawner] AssignKnob skipped '{obj.name}' — tag is '{obj.tag}'"); return; }

            Knob knob = isLeft ? _leftKnob : _rightKnob;
            if (knob == null) { Debug.Log($"[ShapeSpawner] AssignKnob '{obj.name}' — knob is null"); return; }

            SplineProgressTracker tracker = obj.GetComponent<SplineProgressTracker>();
            if (tracker == null) { Debug.Log($"[ShapeSpawner] AssignKnob '{obj.name}' — no SplineProgressTracker on this GO"); return; }

            Debug.Log($"[ShapeSpawner] AssignKnob '{obj.name}' — SplineContainer is {(tracker.SplineContainer != null ? "SET" : "NULL")}");
            knob.SetTracker(tracker, tracker.SplineContainer);
            tracker.SetKnobCollider(knob.Collider);
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
            if (tracker != null)
                tracker.SetHand(handObject.transform, handObject.GetComponent<Collider>());
        }

        private void WireConnector(GameObject shape, GameObject ball)
        {
            Shape tracker = shape.GetComponent<Shape>();
            if (tracker == null || tracker.Trackers.Count == 0) return;

            SplineContainer splineContainer = tracker.Trackers[0].SplineContainer;
            if (splineContainer == null) return;

            CurrentConnector = shape.AddComponent<BallShapeConnector>();
            CurrentConnector.Setup(ball.transform, splineContainer, _connectorMaterial, _connectorFillMaterial, _connectorMidPoint, tracker);
        }

        private void WireTracker(GameObject shape, GameObject ball)
        {
            if (ball == null) return;

            Shape tracker = shape.GetComponent<Shape>();
            if (tracker == null) return;

            AirSacColor airSacColor = ball.GetComponent<AirSacColor>();
            if (airSacColor != null)
                airSacColor.SetShape(tracker);

            AirSacSize airSacSize = ball.GetComponent<AirSacSize>();
            if (airSacSize != null)
                airSacSize.SetShape(tracker);

            AirSacProgressParticleVisualizer particleVisualizer = ball.GetComponent<AirSacProgressParticleVisualizer>();
            if (particleVisualizer != null)
                particleVisualizer.SetShape(tracker);
        }

        #endregion
    }
}
