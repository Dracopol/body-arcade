using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Spawning
{
    internal class SplineRegistry : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private SplineContainer[] _splinesRight;
        [SerializeField] private SplineContainer[] _splinesLeft;

        #endregion

        #region Properties

        internal IReadOnlyList<SplineContainer> SplinesRight => _splinesRight;
        internal IReadOnlyList<SplineContainer> SplinesLeft => _splinesLeft;

        #endregion
    }
}