using UnityEngine;

namespace IMVR.BodyArcade.AirSacShapes.Sequencing
{
    internal class ShapeSequenceCoordinator : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private BallSequencer _sequencer;
        [SerializeField] private ShapeSpawner _spawner;

        #endregion

        #region Setup

        private void OnEnable()
        {
            _sequencer.OnBallChanged += HandleBallChanged;
            _spawner.OnShapeDestroyed += HandleShapeDestroyed;
        }

        private void OnDisable()
        {
            _sequencer.OnBallChanged -= HandleBallChanged;
            _spawner.OnShapeDestroyed -= HandleShapeDestroyed;
        }

        #endregion

        #region Event handlers

        private void HandleBallChanged(GameObject ball)
        {
            _spawner.Spawn(ball);
        }

        private void HandleShapeDestroyed()
        {
            _sequencer.Advance();
        }

        #endregion
    }
}
