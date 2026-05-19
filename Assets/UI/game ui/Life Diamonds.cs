using UnityEngine;
using UnityEngine.UI;

public class LifeDiamond : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private Image _fillImage;

    #endregion

    #region Public

    public void SetFilled(bool filled) => _fillImage.enabled = filled;

    #endregion
}