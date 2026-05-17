using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.UI
{
    internal class SessionConfigAirSacShapes : MonoBehaviour
    {
        internal static SessionConfigAirSacShapes Instance { get; private set; }

        internal MirrorMode MirrorMode;
        internal HandMode HandMode;
        internal List<ShapeConfig> SelectedShapes = new();
        internal int? StrikeAmount = 10;
        internal int? TimerSeconds = 60;

        [SerializeField] private List<ShapeConfig> _leftHandShapes  = new();
        [SerializeField] private List<ShapeConfig> _rightHandShapes = new();
        [SerializeField] private List<ShapeConfig> _bothHandsShapes = new();

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        internal List<ShapeConfig> GetShapesForMode(HandMode mode)
        {
            return mode switch
            {
                HandMode.LeftOnly          => _leftHandShapes,
                HandMode.RightOnly         => _rightHandShapes,
                HandMode.BothSimultaneously => _bothHandsShapes,
                _                          => _bothHandsShapes
            };
        }
    }
}