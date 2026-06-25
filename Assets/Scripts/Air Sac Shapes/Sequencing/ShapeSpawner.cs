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
        internal event Action OnShapeFailed;
        internal event Action OnShapeWinFill;
        internal event Action OnShapeFailFill;       // fail: input locked — run the connector drain
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

        [Header("Connector")]
        [SerializeField] private Material  _connectorMaterial;
        [SerializeField] private Material  _connectorFillMaterial;
        [SerializeField] private Transform _connectorMidPoint;
        [SerializeField] private float     _connectorGrowDuration    = 0.5f;
        [SerializeField] private float     _connectorRetractDuration = 0.5f;

        #endregion

        #region Fields

        private readonly ShapePool _pool = new();
        private BallShapeConnector _connector;

        private GameObject _currentShapeObject;
        private GameObject _currentBall;
        private Shape      _currentShape;

        // The current ball IS the air sac — its visualizers + end animator, cached per cycle for unbind.
        private AirSacColor                      _airSacColor;
        private AirSacSize                       _airSacSize;
        private AirSacProgressParticleVisualizer _airSacParticles;
        private AirSacEndAnimator                _airSacEnd;

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
            _syringes.OnFailReady            += HandleFailReady;
            _syringes.OnFailComplete         += HandleFailComplete;
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
            _syringes.OnFailReady            -= HandleFailReady;
            _syringes.OnFailComplete         -= HandleFailComplete;
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
                _syringes.Wire(_currentShape, pooled.Prepared.DrainSpeed, pooled.Prepared.KnobSizeMultiplier, pooled.Prepared.WinThreshold);

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
            _connector.Setup(_connectorMaterial, _connectorFillMaterial, _connectorMidPoint, _connectorGrowDuration, _connectorRetractDuration);
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
            if (_syringes != null)     _syringes.ResetState();
            if (_audioProgress != null) _audioProgress.SetShape(null);
            UnbindAirSac();
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

        private void HandleMinMaxProgressChange(float v)
        {
            if (_currentShape) _currentShape.InvokeMinMaxChange(v);
            // The green tube fill publishes min-max as it rises during the end reveal — the sac
            // follows it (no-op until BeginReveal, so it stays sick during play).
            _airSacEnd?.Follow(v);
        }

        private void HandleWinStateChange()
        {
            if (_currentShape) _currentShape.InvokeWinStateChange();
            // Win: the tube has turned green and is about to fill — start the sac reveal.
            _airSacEnd?.BeginReveal();
        }

        private void HandleFirstFillComplete()
        {
            if (_currentShape) _currentShape.InvokeFirstFillComplete();
            // The fill reached its target (win = 1, fail = fraction): freeze the sac, punch, and
            // on a win spawn particles. Win sound fires via the Shape event on SoundManager.
            _airSacEnd?.FillComplete(won: _syringes == null || !_syringes.IsFailing);
        }

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

        // Fail: input is locked and plungers are retired. Begin the sac reveal (it will follow the
        // green tube fail-fill up to the fraction reached), play the fail sound, and drain the
        // connector. Punch + freeze happen at fill-complete, just like a win.
        private void HandleFailReady()
        {
            _airSacEnd?.BeginReveal();
            if (_audioProgress != null) _audioProgress.PlayShapeFail();
            OnShapeFailFill?.Invoke();
        }

        private void HandleFailComplete()
        {
            ReturnToPool();
            OnShapeFailed?.Invoke();
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

        // The current ball IS the air sac. Grab its visualizers + end animator and reset color/size
        // to the sick Start look (no during-play animation now — the end animator reveals at the
        // end). Inject the destroy-delay sink so the end animation holds teardown until it finishes.
        private void WireAirSac()
        {
            if (_currentShape == null || _currentBall == null) return;

            _airSacColor     = _currentBall.GetComponent<AirSacColor>();
            _airSacSize      = _currentBall.GetComponent<AirSacSize>();
            _airSacParticles = _currentBall.GetComponent<AirSacProgressParticleVisualizer>();
            _airSacEnd       = _currentBall.GetComponent<AirSacEndAnimator>();

            if (_airSacColor != null) _airSacColor.ResetToStart();
            if (_airSacSize  != null) _airSacSize.ResetToStart();
            if (_airSacEnd   != null)
            {
                _airSacEnd.ResetReveal();
                _airSacEnd.SetDestroyDelaySink(RegisterDestroyDelay);
            }
        }

        private void UnbindAirSac()
        {
            if (_airSacEnd != null) _airSacEnd.SetDestroyDelaySink(null);
            _airSacColor     = null;
            _airSacSize      = null;
            _airSacParticles = null;
            _airSacEnd       = null;
        }

        #endregion
    }
}
