using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace IMVR.BodyArcade.UiRobot
{
    // Quick red screen-edge flash on damage (life loss). A UI overlay sprite faded in fast and out
    // slower; re-triggering restarts the flash rather than stacking.
    internal class DamageVignette : MonoBehaviour
    {
        #region Serialized fields

        [Header("References")]
        [SerializeField] private Image _overlay;

        [Header("Behaviour")]
        [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.6f;
        [SerializeField] private float _fadeInSeconds  = 0.08f;
        [SerializeField] private float _fadeOutSeconds = 0.3f;

        #endregion

        #region Fields

        private CancellationTokenSource _flashCts;

        #endregion

        #region Setup

        private void Awake()
        {
            SetAlpha(0f);
        }

        private void OnDestroy()
        {
            _flashCts?.Cancel();
            _flashCts?.Dispose();
        }

        #endregion

        #region Internal

        internal void Flash()
        {
            if (_overlay == null) return;

            _flashCts?.Cancel();
            _flashCts?.Dispose();
            _flashCts = new CancellationTokenSource();

            FlashAsync(_flashCts.Token).Forget();
        }

        #endregion

        #region Private

        private async UniTaskVoid FlashAsync(CancellationToken ct)
        {
            try
            {
                await FadeAlphaAsync(0f, _maxAlpha, _fadeInSeconds, ct);
                await FadeAlphaAsync(_maxAlpha, 0f, _fadeOutSeconds, ct);
            }
            catch (OperationCanceledException) { }
        }

        private async UniTask FadeAlphaAsync(float from, float to, float duration, CancellationToken ct)
        {
            if (duration <= 0f)
            {
                SetAlpha(to);
                return;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                await UniTask.NextFrame(ct);
                elapsed += Time.deltaTime;
                float a = Mathf.Lerp(from, to, elapsed / duration);
                SetAlpha(a);
                Debug.Log($"[Vignette] set alpha={a:F2}, image.color.a now={_overlay.color.a:F2}");
            }

            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            if (_overlay == null) return;
            Color color = _overlay.color;
            color.a = alpha;
            _overlay.color = color;
        }

        #endregion
    }
}
