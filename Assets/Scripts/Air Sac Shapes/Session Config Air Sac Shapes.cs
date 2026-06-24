using System.Collections.Generic;
using UnityEngine;

namespace IMVR.BodyArcade.UiRobot
{
    internal class SessionConfigAirSacShapes : MonoBehaviour
    {
        internal static SessionConfigAirSacShapes Instance { get; private set; }

        internal MirrorMode MirrorMode;
        internal bool       HandVisualization = true;
        internal bool       Vibration = true;
        internal List<PreparedShape> SelectedShapes = new();
        internal int? StrikeAmount = 10;
        internal int? TimerSeconds = 60;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}