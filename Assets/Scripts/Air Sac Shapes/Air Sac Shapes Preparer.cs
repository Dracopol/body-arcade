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
        [SerializeField] private float _easyDrainSpeed         = 5f;
        [SerializeField] private float _easyKnobSizeMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float _easyWinThreshold = 0.98f;

        [Header("Medium")]
        [SerializeField] private float _mediumDrainSpeed         = 5f;
        [SerializeField] private float _mediumKnobSizeMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float _mediumWinThreshold = 0.98f;

        [Header("Hard")]
        [SerializeField] private float _hardDrainSpeed         = 5f;
        [SerializeField] private float _hardKnobSizeMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float _hardWinThreshold = 0.98f;

        #endregion

        #region Internal

        internal List<PreparedShape> PrepareForDifficulty(string difficulty)
        {
            (float drain, float knobSizeMultiplier, float winThreshold) = difficulty switch
            {
                "Medium" => (_mediumDrainSpeed, _mediumKnobSizeMultiplier, _mediumWinThreshold),
                "Hard"   => (_hardDrainSpeed,   _hardKnobSizeMultiplier,   _hardWinThreshold),
                _        => (_easyDrainSpeed,   _easyKnobSizeMultiplier,   _easyWinThreshold)
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
                    Config             = config,
                    DrainSpeed         = drain,
                    KnobSizeMultiplier = knobSizeMultiplier,
                    WinThreshold       = winThreshold
                });
            }

            return result;
        }

        #endregion
    }
}
