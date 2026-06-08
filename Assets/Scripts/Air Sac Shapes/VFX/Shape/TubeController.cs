using System;
using UnityEngine;
using UnityEngine.Splines;
using IMVR.BodyArcade.AirSacShapes.ProgressTracking;

namespace IMVR.BodyArcade.AirSacShapes.VFX.Tube
{
    internal class TubeController : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnProgressChange;
        internal event Action<float> OnMaxProgressChange;
        internal event Action OnWinStateChange;
        internal event Action OnWinReady;
        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;
        internal event Action OnComplete;

        #endregion

        #region Serialized fields

        [SerializeField] private SplineProgressTracker _tracker;
        [SerializeField] private TubeAnimator          _animator;
        [SerializeField] private TubeFillVisualizer    _visualizer;

        #endregion

        #region Properties

        internal SplineProgressTracker Tracker        => _tracker;
        internal float                 Progress       => _tracker != null ? _tracker.Progress       : 0f;
        internal float                 MaxProgress    => _tracker != null ? _tracker.MaxProgress    : 0f;
        internal SplineContainer       SplineContainer => _tracker != null ? _tracker.SplineContainer : null;

        #endregion

        #region Setup

        private void OnEnable()
        {
            if (_tracker != null)
            {
                _tracker.OnProgressChange    += HandleProgressChange;
                _tracker.OnMaxProgressChange += HandleMaxProgressChange;
                _tracker.OnWinStateChange    += HandleWinStateChange;
                _tracker.OnWinReady          += HandleWinReady;
            }
            if (_animator != null)
            {
                _animator.OnFirstFillComplete += HandleFirstFillComplete;
                _animator.OnWinFillComplete   += HandleWinFillComplete;
                _animator.OnAppearComplete    += HandleAppearComplete;
                _animator.OnComplete          += HandleComplete;
            }
        }

        private void OnDisable()
        {
            if (_tracker != null)
            {
                _tracker.OnProgressChange    -= HandleProgressChange;
                _tracker.OnMaxProgressChange -= HandleMaxProgressChange;
                _tracker.OnWinStateChange    -= HandleWinStateChange;
                _tracker.OnWinReady          -= HandleWinReady;
            }
            if (_animator != null)
            {
                _animator.OnFirstFillComplete -= HandleFirstFillComplete;
                _animator.OnWinFillComplete   -= HandleWinFillComplete;
                _animator.OnAppearComplete    -= HandleAppearComplete;
                _animator.OnComplete          -= HandleComplete;
            }
        }

        #endregion

        #region Internal

        internal void Assign(SplineContainer splineContainer, MeshFilter meshFilter, MeshRenderer meshRenderer, bool isReversed)
        {
            if (_tracker    != null) _tracker.AssignSpline(splineContainer, isReversed);
            if (_animator   != null) _animator.AssignRenderers(meshRenderer, null);
            if (_visualizer != null) _visualizer.AssignMesh(meshFilter, meshRenderer, isReversed);
        }

        internal void ApplyDifficultySettings(float drainSpeed, float splineRadius)
        {
            if (_tracker != null) _tracker.ApplyDifficultySettings(drainSpeed, splineRadius);
        }

        internal void SetHand(Transform handTransform, Collider handCollider)
        {
            if (_tracker != null) _tracker.SetHand(handTransform, handCollider);
        }

        internal void SetKnobCollider(Collider knobCollider)
        {
            if (_tracker != null) _tracker.SetKnobCollider(knobCollider);
        }

        internal void TriggerAppear()
        {
            if (_animator != null) _animator.TriggerAppear();
        }

        internal void TriggerWin()
        {
            if (_tracker != null) _tracker.TriggerWin();
        }

        internal void RegisterDestroyDelay(float seconds)
        {
            if (_animator != null) _animator.RegisterDestroyDelay(seconds);
        }

        internal void ResetState()
        {
            if (_tracker    != null) _tracker.ResetState();
            if (_animator   != null) _animator.ResetState();
            if (_visualizer != null) _visualizer.AssignMesh(null, null, false);
        }

        #endregion

        #region Private — event forwarding

        private void HandleProgressChange(float v)    => OnProgressChange?.Invoke(v);
        private void HandleMaxProgressChange(float v) => OnMaxProgressChange?.Invoke(v);
        private void HandleWinStateChange()           => OnWinStateChange?.Invoke();
        private void HandleWinReady()                 => OnWinReady?.Invoke();
        private void HandleFirstFillComplete()        => OnFirstFillComplete?.Invoke();
        private void HandleWinFillComplete()          => OnWinFillComplete?.Invoke();
        private void HandleAppearComplete()           => OnAppearComplete?.Invoke();
        private void HandleComplete()                 => OnComplete?.Invoke();

        #endregion
    }
}
