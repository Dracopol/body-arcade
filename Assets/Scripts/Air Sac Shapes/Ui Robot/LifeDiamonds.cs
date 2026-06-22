using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace IMVR.BodyArcade.UiRobot
{
    public class LifeDiamonds : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _outlineImage;
        [SerializeField] private float _shatterDurationSeconds = 0.35f;
        [SerializeField] private float _shatterScalePunch = 1.4f;

        #endregion

        #region Fields

        private RectTransform _fillRectTransform;
        private Vector3 _fillBaseScale;

        #endregion

        #region Setup

        private void Awake()
        {
            _fillRectTransform = (RectTransform)_fillImage.transform;
            _fillBaseScale = _fillRectTransform.localScale;
        }

        #endregion

        #region Public

        public void SetSlotEnabled(bool slotEnabled)
        {
            _fillImage.enabled    = slotEnabled;
            _outlineImage.enabled = slotEnabled;
        }

        public void SetFilled(bool filled)
        {
            _outlineImage.enabled = true;
            RestoreFill();
            _fillImage.enabled = filled;
        }

        // Scale-punch + fade the fill out, leaving the outline. Falls back to an
        // instant hide if the fill is already empty.
        public async UniTask PlayShatterAsync(CancellationToken cancellationToken)
        {
            _outlineImage.enabled = true;

            if (!_fillImage.enabled)
                return;

            var baseColor = _fillImage.color;
            var elapsedSeconds = 0f;

            while (elapsedSeconds < _shatterDurationSeconds)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var progress = Mathf.Clamp01(elapsedSeconds / _shatterDurationSeconds);
                var scale = Mathf.Lerp(1f, _shatterScalePunch, progress);

                _fillRectTransform.localScale = _fillBaseScale * scale;
                _fillImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - progress);

                elapsedSeconds += Time.deltaTime;
                await UniTask.NextFrame(cancellationToken);
            }

            _fillImage.enabled = false;
            RestoreFill();
            _fillImage.color = baseColor;
        }

        #endregion

        #region Private

        private void RestoreFill()
        {
            _fillRectTransform.localScale = _fillBaseScale;

            var color = _fillImage.color;
            _fillImage.color = new Color(color.r, color.g, color.b, 1f);
        }

        #endregion
    }
}
