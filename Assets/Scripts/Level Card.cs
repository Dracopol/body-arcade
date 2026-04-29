using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace IMVR.BodyArcade.UI
{
    internal sealed class LevelCard : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private XRSimpleInteractable _interactable;
        [SerializeField] private Image _background;
        [SerializeField] private Image _levelImage;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private string _sceneName;

        [Header("Hover Animation")]
        [SerializeField] private Color _defaultBgColor = new Color(1f, 1f, 1f, 0.10f);
        [SerializeField] private Color _hoverBgColor   = new Color(0.45f, 0.78f, 1f, 0.30f);
        [SerializeField] private float _hoverImageScale   = 1.15f;
        [SerializeField] private float _animationDuration = 0.18f;

        [Header("Haptics")]
        [SerializeField] private float _hapticAmplitude = 0.12f;
        [SerializeField] private float _hapticDuration  = 0.06f;

        #endregion

        #region Fields

        private Vector3 _imageBaseScale;
        private CancellationTokenSource _animCts;

        #endregion

        #region Setup

        private void Awake()
        {
            _imageBaseScale = _levelImage.rectTransform.localScale;
            _background.color = _defaultBgColor;
        }

        private void OnEnable()
        {
            _interactable.hoverEntered.AddListener(HandleHoverEntered);
            _interactable.hoverExited.AddListener(HandleHoverExited);
            _interactable.selectEntered.AddListener(HandleSelectEntered);
        }

        private void OnDisable()
        {
            _interactable.hoverEntered.RemoveListener(HandleHoverEntered);
            _interactable.hoverExited.RemoveListener(HandleHoverExited);
            _interactable.selectEntered.RemoveListener(HandleSelectEntered);
            CancelAndDisposeCts();
        }

        #endregion

        #region Event Handlers

        private void HandleHoverEntered(HoverEnterEventArgs args)
        {
            TriggerHaptics(args.interactorObject);
            AnimateAsync(hover: true).Forget();
        }

        private void HandleHoverExited(HoverExitEventArgs args)
        {
            AnimateAsync(hover: false).Forget();
        }

        private void HandleSelectEntered(SelectEnterEventArgs args)
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                Debug.LogWarning($"[LevelCard] Scene name is not set on {gameObject.name}.", this);
                return;
            }

            SceneManager.LoadScene(_sceneName);
        }

        #endregion

        #region Private

        private void TriggerHaptics(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor)
        {
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor inputInteractor)
                inputInteractor.SendHapticImpulse(_hapticAmplitude, _hapticDuration);
        }

        private async UniTaskVoid AnimateAsync(bool hover)
        {
            CancelAndDisposeCts();
            _animCts = new CancellationTokenSource();
            var ct = _animCts.Token;

            var startBg     = _background.color;
            var targetBg    = hover ? _hoverBgColor : _defaultBgColor;
            var startScale  = _levelImage.rectTransform.localScale;
            var targetScale = hover ? _imageBaseScale * _hoverImageScale : _imageBaseScale;

            var elapsed = 0f;

            try
            {
                while (elapsed < _animationDuration)
                {
                    var t      = elapsed / _animationDuration;
                    var smooth = Mathf.SmoothStep(0f, 1f, t);

                    _background.color                    = Color.Lerp(startBg, targetBg, smooth);
                    _levelImage.rectTransform.localScale = Vector3.Lerp(startScale, targetScale, smooth);

                    elapsed += Time.deltaTime;
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            catch (System.OperationCanceledException)
            {
                return;
            }

            // Snap to final values
            _background.color                    = targetBg;
            _levelImage.rectTransform.localScale = targetScale;
        }

        private void CancelAndDisposeCts()
        {
            _animCts?.Cancel();
            _animCts?.Dispose();
            _animCts = null;
        }

        #endregion
    }
}