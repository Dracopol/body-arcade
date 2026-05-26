using System;
using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    internal class BallSequencer : MonoBehaviour
    {
        #region Events

        internal event Action<GameObject> OnBallChanged;

        #endregion

        #region Serialized fields

        [SerializeField] private List<GameObject> _balls;

        #endregion

        #region Fields

        private int _currentIndex = -1;

        #endregion

        #region Properties

        internal GameObject CurrentBall => _balls != null && _balls.Count > 0
            ? _balls[_currentIndex]
            : null;

        #endregion

        #region Setup

        private void Start()
        {
            Advance();
        }

        #endregion

        #region Internal

        internal void Advance()
        {
            if (_balls == null || _balls.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _balls.Count;
            OnBallChanged?.Invoke(CurrentBall);
        }

        #endregion
    }
}