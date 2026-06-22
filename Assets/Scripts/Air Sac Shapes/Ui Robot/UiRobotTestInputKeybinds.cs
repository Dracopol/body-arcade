using UnityEngine;
using UnityEngine.InputSystem;

namespace IMVR.BodyArcade.UiRobot
{
    /// <summary>
    /// TEMPORARY: hardcoded keyboard bindings for manual testing of the Ui Robot HUD.
    /// L = lose life | G = gain life | T = start timer | Y = stop timer | R = restart timer
    /// Replace with proper Input Actions once VR bindings are defined.
    /// </summary>
    public class UiRobotTestInputKeybinds : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private LivesUi _livesUi;
        [SerializeField] private TimerUi _timerUi;

        #endregion

        #region Update

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.lKey.wasPressedThisFrame) _livesUi.LoseLife();
            if (keyboard.gKey.wasPressedThisFrame) _livesUi.GainLife();

            if (keyboard.tKey.wasPressedThisFrame) _timerUi.StartTimer();
            if (keyboard.yKey.wasPressedThisFrame) _timerUi.StopTimer();
            if (keyboard.rKey.wasPressedThisFrame) _timerUi.RestartTimer();
        }

        #endregion
    }
}
