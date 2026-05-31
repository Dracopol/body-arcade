using UnityEngine;

namespace IMVR.BodyArcade.UiRobot
{
    internal enum FocusMode
    {
        Unfocused,
        LeftFocused,
        RightFocused
    }

    internal class SessionConfigNeuromania : MonoBehaviour
    {
        internal static SessionConfigNeuromania Instance { get; private set; }

        internal FocusMode FocusMode   = FocusMode.Unfocused;
        internal float     HandBias    = 0f;
        internal float     HeightBias  = 0f;

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