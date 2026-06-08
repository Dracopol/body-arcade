using System;
using UnityEngine;
using UnityEngine.Splines;
using IMVR.BodyArcade.AirSacShapes.Sequencing;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTracking
{
    internal class Shape : MonoBehaviour
    {
        #region Events

        internal event Action<float> OnMinProgressChange;
        internal event Action<float> OnMinMaxChange;
        internal event Action OnWinStateChange;
        internal event Action OnFirstFillComplete;
        internal event Action OnWinFillComplete;
        internal event Action OnAppearComplete;

        #endregion

        #region Serialized fields

        [Header("Left")]
        [SerializeField] internal SplineContainer LeftSpline;
        [SerializeField] internal MeshFilter      LeftMeshFilter;
        [SerializeField] internal MeshRenderer    LeftMeshRenderer;
        [SerializeField] internal bool            LeftIsReversed;

        [Header("Right")]
        [SerializeField] internal SplineContainer RightSpline;
        [SerializeField] internal MeshFilter      RightMeshFilter;
        [SerializeField] internal MeshRenderer    RightMeshRenderer;
        [SerializeField] internal bool            RightIsReversed;

        #endregion

        #region Fields

        private ShapeSpawner _spawner;

        #endregion

        #region Internal

        internal void SetSpawner(ShapeSpawner spawner) => _spawner = spawner;

        internal void RegisterDestroyDelay(float seconds)
        {
            if (_spawner != null) _spawner.RegisterDestroyDelay(seconds);
        }

        internal void InvokeMinProgressChange(float v) => OnMinProgressChange?.Invoke(v);
        internal void InvokeMinMaxChange(float v)       => OnMinMaxChange?.Invoke(v);
        internal void InvokeWinStateChange()            => OnWinStateChange?.Invoke();
        internal void InvokeFirstFillComplete()         => OnFirstFillComplete?.Invoke();
        internal void InvokeWinFillComplete()           => OnWinFillComplete?.Invoke();
        internal void InvokeAppearComplete()            => OnAppearComplete?.Invoke();

        #endregion
    }
}
