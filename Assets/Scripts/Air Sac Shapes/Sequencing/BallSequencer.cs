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

        #region Internal

        // Position the cursor one before the window start (the last `reps` balls) so the next
        // Advance() lands on the first ball of the window. The coordinator drives the kickoff.
        internal void StartAtLast(int reps)
        {
            _currentIndex = _balls.Count - reps - 1;
        }

        // Advance to the next ball. Returns true if a valid ball was reached, false once the
        // sequence has walked past the last ball (window finished).
        internal bool Advance()
        {
            if (_balls == null || _balls.Count == 0) return false;
            _currentIndex++;
            if (_currentIndex >= _balls.Count) return false;
            OnBallChanged?.Invoke(CurrentBall);
            return true;
        }

        #endregion
    }
}