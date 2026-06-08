using System;
using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.VFX.AirSac;
using IMVR.BodyArcade.AirSacShapes.Syringes;
using IMVR.BodyArcade.AirSacShapes.Sound;
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
        [SerializeField] private GameObject  _shapeParent;
        [SerializeField] private MirrorHands _mirrorHands;

        [Header("Syringes")]
        [SerializeField] private SyringesController _syringes;

        [Header("Audio")]
        [SerializeField] private SoundManager _audioProgress;

        [Header("Air Sac")]
        [SerializeField] private AirSacColor                       _airSacColor;
        [SerializeField] private AirSacSize                        _airSacSize;
        [SerializeField] private AirSacProgressParticleVisualizer  _airSacParticleVisualizer;

        [Header("Connector")]
        [SerializeField] private Material  _connectorMaterial;
        [SerializeField] private Material  _connectorFillMaterial;
        [SerializeField] private Transform _connectorMidPoint;

        #endregion

        #region Fields

        private List<PreparedShape> _shapes;
        private GameObject _currentShapeObject;
        private GameObject _currentBall;
        private Shape      _currentShape;
        private bool       _hasSpawned;

        #endregion

        #region Properties

        internal BallShapeConnector CurrentConnector { get; private set; }

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_syringes == null) return;
            _syringes.OnMinProgressChange    += HandleMinProgressChange;
            _syringes.OnMinMaxProgressChange += HandleMinMaxProgressChange;
            _syringes.OnWinStateChange       += HandleWinStateChange;
            _syringes.OnFirstFillComplete    += HandleFirstFillComplete;
            _syringes.OnWinFillComplete      += HandleWinFillComplete;
            _syringes.OnAppearComplete       += HandleAppearComplete;
            _syringes.OnComplete             += HandleComplete;
        }

        private void OnDisable()
        {
            if (_syringes == null) return;
            _syringes.OnMinProgressChange    -= HandleMinProgressChange;
            _syringes.OnMinMaxProgressChange -= HandleMinMaxProgressChange;
            _syringes.OnWinStateChange       -= HandleWinStateChange;
            _syringes.OnFirstFillComplete    -= HandleFirstFillComplete;
            _syringes.OnWinFillComplete      -= HandleWinFillComplete;
            _syringes.OnAppearComplete       -= HandleAppearComplete;
            _syringes.OnComplete             -= HandleComplete;
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
                _currentShape    = null;
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
            if (_currentShape == null || _syringes == null)
            {
                OnShapeAppearComplete?.Invoke();
                return;
            }
            _syringes.TriggerAppear();
        }

        internal void RegisterDestroyDelay(float seconds)
        {
            if (_syringes != null) _syringes.RegisterDestroyDelay(seconds);
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

            if (_syringes != null)
                _syringes.Wire(_currentShape, prepared.DrainSpeed, prepared.SplineRadius, prepared.WinThreshold);

            _hasSpawned = true;

            AssignHands();
            WireConnector(_currentShapeObject, _currentBall);
            WireAirSac();

            if (_audioProgress != null)
                _audioProgress.SetShape(_currentShape);
        }

        private void ResetSceneComponents()
        {
            if (_syringes != null)                    _syringes.ResetState();
            if (_audioProgress != null)            _audioProgress.SetShape(null);
            if (_airSacColor != null)              _airSacColor.SetShape(null);
            if (_airSacSize != null)               _airSacSize.SetShape(null);
            if (_airSacParticleVisualizer != null)  _airSacParticleVisualizer.SetShape(null);
        }

        private void HideShapeRenderers()
        {
            if (_currentShapeObject == null) return;
            foreach (MeshRenderer r in _currentShapeObject.GetComponentsInChildren<MeshRenderer>())
                r.enabled = false;
        }

        #endregion

        #region Private — event handlers

        private void HandleMinProgressChange(float v)    { if (_currentShape) _currentShape.InvokeMinProgressChange(v); }
        private void HandleMinMaxProgressChange(float v) { if (_currentShape) _currentShape.InvokeMinMaxChange(v); }
        private void HandleWinStateChange()              { if (_currentShape) _currentShape.InvokeWinStateChange(); }
        private void HandleFirstFillComplete()           { if (_currentShape) _currentShape.InvokeFirstFillComplete(); }

        private void HandleWinFillComplete()
        {
            if (_currentShape) _currentShape.InvokeWinFillComplete();
            OnShapeWinFill?.Invoke();
        }

        private void HandleAppearComplete()
        {
            if (_currentShape) _currentShape.InvokeAppearComplete();
            OnShapeAppearComplete?.Invoke();
        }

        private void HandleComplete()
        {
            if (_currentShapeObject != null)
                Destroy(_currentShapeObject);
        }

        #endregion

        #region Private — wiring helpers

        private void AssignHands()
        {
            if (_syringes == null || _mirrorHands == null || _currentShape == null) return;

            if (_currentShape.LeftSpline != null)
            {
                GameObject hand = _mirrorHands.ActiveLeft;
                if (hand != null) _syringes.SetLeftHand(hand.transform, hand.GetComponent<Collider>());
            }

            if (_currentShape.RightSpline != null)
            {
                GameObject hand = _mirrorHands.ActiveRight;
                if (hand != null) _syringes.SetRightHand(hand.transform, hand.GetComponent<Collider>());
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

        private void WireAirSac()
        {
            if (_currentShape == null) return;
            if (_airSacColor             != null) _airSacColor.SetShape(_currentShape);
            if (_airSacSize              != null) _airSacSize.SetShape(_currentShape);
            if (_airSacParticleVisualizer != null) _airSacParticleVisualizer.SetShape(_currentShape);
        }

        #endregion
    }
}
