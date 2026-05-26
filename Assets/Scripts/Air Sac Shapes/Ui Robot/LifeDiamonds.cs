using UnityEngine;
using UnityEngine.UI;

namespace IMVR.BodyArcade.UiRobot
{
    public class LifeDiamonds : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _outlineImage;

        #endregion

        #region Public

        public void SetSlotEnabled(bool enabled)
        {
            _fillImage.enabled    = enabled;
            _outlineImage.enabled = enabled;
        }

        public void SetFilled(bool filled)
        {
            _outlineImage.enabled = true;
            _fillImage.enabled    = filled;
        }

        #endregion
    }
}