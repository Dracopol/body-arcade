using System;
using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;
using IMVR.BodyArcade.AirSacShapes.VFX.AirSac;
using IMVR.BodyArcade.AirSacShapes.VFX.Tube;
using IMVR.BodyArcade.AirSacShapes.SFX;
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

        [Header("Left")]
        [SerializeField] private SplineProgressTracker _leftTracker;
        [SerializeField] private TubeAnimator          _leftAnimator;
        [SerializeField] private TubeFillVisualizer    _leftVisualizer;

        [Header("Right")]
        [SerializeField] private SplineProgressTracker _rightTracker;
        [SerializeField] private TubeAnimator          _rightAnimator;
        [SerializeField] private TubeFillVisualizer    _rightVisualizer;

        [Header("Audio")]
        [SerializeField] private AudioProgress _audioProgress;

        [Header("Air Sac")]
        [SerializeField] private AirSacColor _airSacColor;
        [SerializeField] private AirSacSize _airSacSize;
        [SerializeField] private AirSacProgressParticleVisualizer _airSacParticleVisualizer;

        [Header("Connector")]
        [SerializeField] private Material _connectorMaterial;
        [SerializeField] private Material _connectorFillMaterial;
        [SerializeField] private Transform _connectorMidPoint;

        #endregion

        #region Fields

        private List<PreparedShape> _shapes;
        private GameObject _currentShapeObject;
        private GameObject _currentBall;
        private Shape _currentShape;
        private bool _hasSpawned;

        private int _activeSideCount;
        private int _winCount;
        private int _firstFillCount;
        private int _winFillCount;
        private int _appearCount;
        private int _completeCount;

        private float _minProgress;
        private float _minMaxProgress;

        #endregion

        #region Properties

        internal BallShapeConnector CurrentConnector { get; private set; }

        #endregion

        #region Setup

        private void OnEnable()
        {
            SubscribeTrackers();
            SubscribeAnimators();
        }

        private void OnDisable()
        {
            UnsubscribeTrackers();
            UnsubscribeAnimators();
        }

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
                ResetSceneComponents();
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

            _appearCount = 0;
            if (_activeSideCount == 0)
            {
                OnShapeAppearComplete?.Invoke();
                return;
            }

            if (_leftAnimator != null && _currentShape.LeftSpline != null)
                _leftAnimator.TriggerAppear();
            if (_rightAnimator != null && _currentShape.RightSpline != null)
                _rightAnimator.TriggerAppear();
        }

        internal void RegisterDestroyDelay(float seconds)
        {
            if (_leftAnimator != null)  _leftAnimator.RegisterDestroyDelay(seconds);
            if (_rightAnimator != null) _rightAnimator.RegisterDestroyDelay(seconds);
        }

        #endregion

        #region Private — spawning

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

            _currentShape = _currentShapeObject.GetComponent<Shape>();
            if (_currentShape == null) return;

            _currentShape.SetSpawner(this);

            _activeSideCount  = 0;
            _winCount         = 0;
            _firstFillCount   = 0;
            _winFillCount     = 0;
            _appearCount      = 0;
            _completeCount    = 0;
            _minProgress      = 0f;
            _minMaxProgress   = 0f;

            WireLeft(prepared);
            WireRight(prepared);

            _hasSpawned = true;

            AssignHands();
            AssignKnobs();
            WireConnector(_currentShapeObject, _currentBall);
            WireAirSac(_currentBall);

            if (_audioProgress != null)
                _audioProgress.SetShape(_currentShape);
        }

        private void WireLeft(PreparedShape prepared)
        {
            if (_currentShape.LeftSpline == null) return;
            _activeSideCount++;

            if (_leftTracker != null)
            {
                _leftTracker.ApplyDifficultySettings(prepared.DrainSpeed, prepared.SplineRadius);
                _leftTracker.AssignSpline(_currentShape.LeftSpline, _currentShape.LeftIsReversed);
            }
            if (_leftAnimator != null)
                _leftAnimator.AssignRenderers(_currentShape.LeftMeshRenderer, null);
            if (_leftVisualizer != null)
                _leftVisualizer.AssignMesh(_currentShape.LeftMeshFilter, _currentShape.LeftMeshRenderer, _currentShape.LeftIsReversed);
        }

        private void WireRight(PreparedShape prepared)
        {
            if (_currentShape.RightSpline == null) return;
            _activeSideCount++;

            if (_rightTracker != null)
            {
                _rightTracker.ApplyDifficultySettings(prepared.DrainSpeed, prepared.SplineRadius);
                _rightTracker.AssignSpline(_currentShape.RightSpline, _currentShape.RightIsReversed);
            }
            if (_rightAnimator != null)
                _rightAnimator.AssignRenderers(_currentShape.RightMeshRenderer, null);
            if (_rightVisualizer != null)
                _rightVisualizer.AssignMesh(_currentShape.RightMeshFilter, _currentShape.RightMeshRenderer, _currentShape.RightIsReversed);
        }

        private void ResetSceneComponents()
        {
            if (_leftTracker != null)   _leftTracker.ResetState();
            if (_rightTracker != null)  _rightTracker.ResetState();
            if (_leftAnimator != null)  _leftAnimator.ResetState();
            if (_rightAnimator != null) _rightAnimator.ResetState();
            if (_leftVisualizer != null)  _leftVisualizer.AssignMesh(null, null, false);
            if (_rightVisualizer != null) _rightVisualizer.AssignMesh(null, null, false);
            if (_audioProgress != null) _audioProgress.SetShape(null);
        }

        private void HideShapeRenderers()
        {
            if (_currentShapeObject == null) return;
            foreach (MeshRenderer r in _currentShapeObject.GetComponentsInChildren<MeshRenderer>())
                r.enabled = false;
        }

        #endregion

        #region Private — aggregation

        private void SubscribeTrackers()
        {
            if (_leftTracker != null)
            {
                _leftTracker.OnProgressChange    += HandleProgressChange;
                _leftTracker.OnMaxProgressChange += HandleMaxProgressChange;
                _leftTracker.OnWinStateChange    += HandleWinStateChange;
                _leftTracker.OnWinReady          += HandleWinReady;
            }
            if (_rightTracker != null)
            {
                _rightTracker.OnProgressChange    += HandleProgressChange;
                _rightTracker.OnMaxProgressChange += HandleMaxProgressChange;
                _rightTracker.OnWinStateChange    += HandleWinStateChange;
                _rightTracker.OnWinReady          += HandleWinReady;
            }
        }

        private void UnsubscribeTrackers()
        {
            if (_leftTracker != null)
            {
                _leftTracker.OnProgressChange    -= HandleProgressChange;
                _leftTracker.OnMaxProgressChange -= HandleMaxProgressChange;
                _leftTracker.OnWinStateChange    -= HandleWinStateChange;
                _leftTracker.OnWinReady          -= HandleWinReady;
            }
            if (_rightTracker != null)
            {
                _rightTracker.OnProgressChange    -= HandleProgressChange;
                _rightTracker.OnMaxProgressChange -= HandleMaxProgressChange;
                _rightTracker.OnWinStateChange    -= HandleWinStateChange;
                _rightTracker.OnWinReady          -= HandleWinReady;
            }
        }

        private void SubscribeAnimators()
        {
            if (_leftAnimator != null)
            {
                _leftAnimator.OnFirstFillComplete += HandleFirstFillComplete;
                _leftAnimator.OnWinFillComplete   += HandleWinFillComplete;
                _leftAnimator.OnAppearComplete    += HandleAppearComplete;
                _leftAnimator.OnComplete          += HandleComplete;
            }
            if (_rightAnimator != null)
            {
                _rightAnimator.OnFirstFillComplete += HandleFirstFillComplete;
                _rightAnimator.OnWinFillComplete   += HandleWinFillComplete;
                _rightAnimator.OnAppearComplete    += HandleAppearComplete;
                _rightAnimator.OnComplete          += HandleComplete;
            }
        }

        private void UnsubscribeAnimators()
        {
            if (_leftAnimator != null)
            {
                _leftAnimator.OnFirstFillComplete -= HandleFirstFillComplete;
                _leftAnimator.OnWinFillComplete   -= HandleWinFillComplete;
                _leftAnimator.OnAppearComplete    -= HandleAppearComplete;
                _leftAnimator.OnComplete          -= HandleComplete;
            }
            if (_rightAnimator != null)
            {
                _rightAnimator.OnFirstFillComplete -= HandleFirstFillComplete;
                _rightAnimator.OnWinFillComplete   -= HandleWinFillComplete;
                _rightAnimator.OnAppearComplete    -= HandleAppearComplete;
                _rightAnimator.OnComplete          -= HandleComplete;
            }
        }

        private void HandleProgressChange(float _)
        {
            if (_currentShape == null) return;
            float newMin = ComputeMin(t => t.Progress);
            if (Mathf.Approximately(newMin, _minProgress)) return;
            _minProgress = newMin;
            _currentShape.InvokeMinProgressChange(_minProgress);
        }

        private void HandleMaxProgressChange(float _)
        {
            if (_currentShape == null) return;
            float newMin = ComputeMin(t => t.MaxProgress);
            if (Mathf.Approximately(newMin, _minMaxProgress)) return;
            _minMaxProgress = newMin;
            _currentShape.InvokeMinMaxChange(_minMaxProgress);
        }

        private void HandleWinReady()
        {
            if (_leftTracker != null  && _currentShape.LeftSpline != null  && _leftTracker.MaxProgress  < 1f) return;
            if (_rightTracker != null && _currentShape.RightSpline != null && _rightTracker.MaxProgress < 1f) return;

            if (_leftTracker != null  && _currentShape.LeftSpline != null)  _leftTracker.TriggerWin();
            if (_rightTracker != null && _currentShape.RightSpline != null) _rightTracker.TriggerWin();
        }

        private void HandleWinStateChange()
        {
            if (AllReached(ref _winCount))
                _currentShape.InvokeWinStateChange();
        }

        private void HandleFirstFillComplete()
        {
            if (AllReached(ref _firstFillCount))
                _currentShape.InvokeFirstFillComplete();
        }

        private void HandleWinFillComplete()
        {
            if (AllReached(ref _winFillCount))
            {
                _currentShape.InvokeWinFillComplete();
                OnShapeWinFill?.Invoke();
            }
        }

        private void HandleAppearComplete()
        {
            if (AllReached(ref _appearCount))
            {
                _currentShape.InvokeAppearComplete();
                OnShapeAppearComplete?.Invoke();
            }
        }

        private void HandleComplete()
        {
            if (!AllReached(ref _completeCount)) return;
            if (_currentShapeObject != null)
                Destroy(_currentShapeObject);
        }

        private bool AllReached(ref int count) => ++count >= _activeSideCount;

        private float ComputeMin(Func<SplineProgressTracker, float> selector)
        {
            float min = float.MaxValue;
            bool any  = false;

            if (_leftTracker != null && _currentShape != null && _currentShape.LeftSpline != null)
            { min = Mathf.Min(min, selector(_leftTracker)); any = true; }

            if (_rightTracker != null && _currentShape != null && _currentShape.RightSpline != null)
            { min = Mathf.Min(min, selector(_rightTracker)); any = true; }

            return any ? min : 0f;
        }

        #endregion

        #region Private — wiring helpers

        private void AssignHands()
        {
            if (_currentShape.LeftSpline  != null) AssignHandToTracker(_leftTracker);
            if (_currentShape.RightSpline != null) AssignHandToTracker(_rightTracker);
        }

        private void AssignHandToTracker(SplineProgressTracker tracker)
        {
            if (tracker == null) return;
            bool isLeft = tracker == _leftTracker;
            GameObject handObject = isLeft ? _mirrorHands.ActiveLeft : _mirrorHands.ActiveRight;
            if (handObject == null) return;
            tracker.SetHand(handObject.transform, handObject.GetComponent<Collider>());
        }

        private void AssignKnobs()
        {
            if (_currentShape.LeftSpline != null && _leftKnob != null && _leftTracker != null)
            {
                _leftKnob.SetTracker(_leftTracker, _leftTracker.SplineContainer);
                _leftTracker.SetKnobCollider(_leftKnob.Collider);
            }
            if (_currentShape.RightSpline != null && _rightKnob != null && _rightTracker != null)
            {
                _rightKnob.SetTracker(_rightTracker, _rightTracker.SplineContainer);
                _rightTracker.SetKnobCollider(_rightKnob.Collider);
            }
        }

        private void WireConnector(GameObject shape, GameObject ball)
        {
            Shape shapeData = shape.GetComponent<Shape>();
            if (shapeData == null || shapeData.LeftSpline == null) return;

            CurrentConnector = shape.AddComponent<BallShapeConnector>();
            CurrentConnector.Setup(ball.transform, shapeData.LeftSpline,
                _connectorMaterial, _connectorFillMaterial, _connectorMidPoint, shapeData);
        }

        private void WireAirSac(GameObject ball)
        {
            if (ball == null || _currentShape == null) return;

            if (_airSacColor != null)             _airSacColor.SetShape(_currentShape);
            if (_airSacSize != null)              _airSacSize.SetShape(_currentShape);
            if (_airSacParticleVisualizer != null) _airSacParticleVisualizer.SetShape(_currentShape);
        }

        #endregion
    }
}
