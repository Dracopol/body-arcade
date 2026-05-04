using System.Threading;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Spawning
{
    internal class Target : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private SplineDriver _splineDriver;

        #endregion

        #region Internal

        internal void Drive(SplineContainer spline, float xOffset, float yOffset, Camera camera, CancellationToken ct)
        {
            _splineDriver.Drive(spline, xOffset, yOffset, camera, ct);
        }

        #endregion
    }
}