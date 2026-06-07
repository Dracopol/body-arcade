using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.UiRobot
{
    internal class AirSacShapesPreparer : MonoBehaviour
    {
        #region Serialized fields

        [Header("Shapes")]
        [SerializeField] private List<ShapeConfig> _shapes = new();

        [Header("Easy")]
        [SerializeField] private float _easyDrainSpeed   = 5f;
        [SerializeField] private float _easySplineRadius = 0.05f;

        [Header("Medium")]
        [SerializeField] private float _mediumDrainSpeed   = 5f;
        [SerializeField] private float _mediumSplineRadius = 0.05f;

        [Header("Hard")]
        [SerializeField] private float _hardDrainSpeed   = 5f;
        [SerializeField] private float _hardSplineRadius = 0.05f;

        #endregion

        #region Internal

        internal List<PreparedShape> PrepareForDifficulty(string difficulty)
        {
            (float drain, float radius) = difficulty switch
            {
                "Medium" => (_mediumDrainSpeed, _mediumSplineRadius),
                "Hard"   => (_hardDrainSpeed,   _hardSplineRadius),
                _        => (_easyDrainSpeed,   _easySplineRadius)
            };

            var result = new List<PreparedShape>();

            foreach (ShapeConfig config in _shapes)
            {
                bool matches = difficulty switch
                {
                    "Medium" => config.IsMedium,
                    "Hard"   => config.IsHard,
                    _        => config.IsEasy
                };

                if (!matches) continue;

                result.Add(new PreparedShape
                {
                    Config       = config,
                    DrainSpeed   = drain,
                    SplineRadius = radius
                });
            }

            return result;
        }

        #endregion
    }
}
