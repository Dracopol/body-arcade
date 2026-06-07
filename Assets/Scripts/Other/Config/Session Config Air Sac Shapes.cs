using System.Collections.Generic;
using UnityEngine;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.UiRobot
{
    internal class SessionConfigAirSacShapes : MonoBehaviour
    {
        internal static SessionConfigAirSacShapes Instance { get; private set; }

        internal MirrorMode MirrorMode;
        internal bool       HandVisualization = true;
        internal List<ShapeConfig> SelectedShapes = new();
        internal int? StrikeAmount = 10;
        internal int? TimerSeconds = 60;

        [SerializeField] private List<ShapeConfig> _shapes = new();

        internal List<ShapeConfig> Shapes => _shapes;

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
    }
}