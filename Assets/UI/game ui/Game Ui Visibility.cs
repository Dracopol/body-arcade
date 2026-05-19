using UnityEngine;

namespace IMVR.BodyArcade.UI
{
    public class GameUIVisibility : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameObject _uiRobot;
        [SerializeField] private GameObject _timerUI;
        [SerializeField] private GameObject _livesUI;

        #endregion

        #region Setup

        private void Awake()
        {
            var config = SessionConfigAirSacShapes.Instance;

            var timerDisabled = config == null || !config.TimerSeconds.HasValue;
            var livesDisabled = config == null || !config.StrikeAmount.HasValue;

            _timerUI.SetActive(!timerDisabled);
            _livesUI.SetActive(!livesDisabled);
            _uiRobot.SetActive(!timerDisabled || !livesDisabled);
        }

        #endregion
    }
}