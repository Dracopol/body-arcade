using UnityEngine;
using IMVR.BodyArcade.AirSacShapes.Sequencing;
using IMVR.BodyArcade.AirSacShapes.Syringes;

namespace IMVR.BodyArcade.UiRobot
{
    // Owns the per-shape lives/timer lifecycle and the completed/uncompleted rep tally. The
    // coordinator owns sequence advancement and calls RecordWon/RecordFailed/LogResult so the
    // end-of-reps tally is deterministic regardless of event order.
    internal class GameStateManager : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private LivesUi _lives;
        [SerializeField] private TimerUi _timer;
        [SerializeField] private ShapeSpawner _spawner;
        [SerializeField] private SyringesController _syringes;

        #endregion

        #region Fields

        private int  _completedReps;
        private int  _failedReps;
        private bool _shapeEnding;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _spawner.OnShapeAppearComplete += HandleShapeReady;
            _syringes.OnSourceHandReleased += HandleHandReleased;
            _lives.OnLivesEmpty            += HandleLivesEmpty;
            _timer.OnTimerExpired          += HandleTimerExpired;
        }

        private void OnDisable()
        {
            _spawner.OnShapeAppearComplete -= HandleShapeReady;
            _syringes.OnSourceHandReleased -= HandleHandReleased;
            _lives.OnLivesEmpty            -= HandleLivesEmpty;
            _timer.OnTimerExpired          -= HandleTimerExpired;
        }

        #endregion

        #region Internal

        // Called by the coordinator when a shape ends so the tally is updated before it advances.
        internal void RecordWon()
        {
            _completedReps++;
            _timer.StopTimer();
        }

        internal void RecordFailed()
        {
            _failedReps++;
        }

        internal void LogResult()
        {
            Debug.Log($"Reps finished — completed: {_completedReps}, uncompleted: {_failedReps}");
        }

        #endregion

        #region Event handlers

        // Player just gained control of a new shape: fresh lives, fresh timer.
        private void HandleShapeReady()
        {
            _shapeEnding = false;
            _lives.SetLives(_lives.MaxLives);
            _timer.RestartTimer();
        }

        private void HandleHandReleased()
        {
            _lives.LoseLife();   // no-op when unlimited; does not touch the timer (continuity)
        }

        private void HandleLivesEmpty()
        {
            FailCurrentShape();
        }

        private void HandleTimerExpired()
        {
            FailCurrentShape();
        }

        #endregion

        #region Private

        private void FailCurrentShape()
        {
            if (_shapeEnding) return;
            _shapeEnding = true;
            _timer.StopTimer();
            _spawner.AbortCurrentShape();
        }

        #endregion
    }
}
