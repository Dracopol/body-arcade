using System;
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

        internal event Action OnShapeComplete;
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

        private readonly ShapePool _pool = new();
        private BallShapeConnector _connector;

        private GameObject _currentShapeObject;
        private GameObject _currentBall;
        private Shape      _currentShape;

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
            CreateConnector();

            if (SessionConfigAirSacShapes.Instance != null &&
                SessionConfigAirSacShapes.Instance.SelectedShapes.Count > 0 &&
                _shapeParent != null)
            {
                _pool.Build(SessionConfigAirSacShapes.Instance.SelectedShapes, _shapeParent.transform);
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
            ShapePool.PooledShape pooled = _pool.Next();
            if (pooled == null) return;

            _currentShapeObject = pooled.GameObject;
            _currentShape       = pooled.Shape;
            _currentShapeObject.SetActive(true);

            _currentShape.SetSpawner(this);

            if (_syringes != null)
                _syringes.Wire(_currentShape, pooled.Prepared.DrainSpeed, pooled.Prepared.SplineRadius, pooled.Prepared.WinThreshold);

            AssignHands();
            WireConnector(_currentShape, _currentBall);
            WireAirSac();

            if (_audioProgress != null)
                _audioProgress.SetShape(_currentShape);
        }

        private void CreateConnector()
        {
            var go = new GameObject("BallShapeConnector");
            go.transform.SetParent(transform, false);
            _connector = go.AddComponent<BallShapeConnector>();
            _connector.Setup(_connectorMaterial, _connectorFillMaterial, _connectorMidPoint);
            go.SetActive(false);
        }

        private void ReturnToPool()
        {
            ResetSceneComponents();

            if (_connector != null)
            {
                _connector.SetShape(null);
                _connector.gameObject.SetActive(false);
            }

            if (_currentShapeObject != null)
                _currentShapeObject.SetActive(false);

            CurrentConnector    = null;
            _currentShape       = null;
            _currentShapeObject = null;
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
            ReturnToPool();
            OnShapeComplete?.Invoke();
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

        private void WireConnector(Shape shape, GameObject ball)
        {
            if (_connector == null || shape == null || shape.LeftSpline == null || ball == null) return;

            _connector.gameObject.SetActive(true);
            _connector.Retarget(ball.transform, shape.LeftSpline, shape);
            CurrentConnector = _connector;
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
