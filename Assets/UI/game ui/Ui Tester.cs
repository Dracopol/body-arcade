using UnityEngine;
using UnityEngine.InputSystem;

namespace IMVR.BodyArcade.UI
{
    internal class UITester : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TimerUI _timerUI;
        [SerializeField] private LivesUI _livesUI;

        #endregion

        #region Setup

        private void Awake()
        {
            _timerUI.OnTimerExpired += HandleTimerExpired;
            _livesUI.OnLivesEmpty   += HandleLivesEmpty;
        }

        private void OnDestroy()
        {
            _timerUI.OnTimerExpired -= HandleTimerExpired;
            _livesUI.OnLivesEmpty   -= HandleLivesEmpty;
        }

        private void Start()
        {
            PrintHelp();
        }

        #endregion

        #region Update

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            // Timer
            if (kb.spaceKey.wasPressedThisFrame) TryStartTimer();
            if (kb.sKey.wasPressedThisFrame)     _timerUI.StopTimer();
            if (kb.rKey.wasPressedThisFrame)     _timerUI.ResetTimer();
            if (kb.tKey.wasPressedThisFrame)     _timerUI.RestartTimer();

            // Lives
            if (kb.leftArrowKey.wasPressedThisFrame)  TryLoseLife();
            if (kb.rightArrowKey.wasPressedThisFrame) TryGainLife();

            // Help
            if (kb.hKey.wasPressedThisFrame) PrintHelp();
        }

        #endregion

        #region Event Handlers

        private void HandleTimerExpired() => Debug.Log("[UITester] Timer expired");
        private void HandleLivesEmpty()   => Debug.Log("[UITester] No lives left");

        #endregion

        #region Private

        private void TryStartTimer()
        {
            if (_timerUI.IsDisabled)
            {
                Debug.Log("[UITester] Timer is disabled (no time limit configured)");
                return;
            }

            _timerUI.StartTimer();
            Debug.Log("[UITester] Timer started");
        }

        private void TryLoseLife()
        {
            if (_livesUI.Unlimited)
            {
                Debug.Log("[UITester] Lives are unlimited — LoseLife is a no-op");
                return;
            }

            _livesUI.LoseLife();
            Debug.Log($"[UITester] Lost a life — {_livesUI.CurrentLives}/{_livesUI.MaxLives} remaining");
        }

        private void TryGainLife()
        {
            if (_livesUI.Unlimited)
            {
                Debug.Log("[UITester] Lives are unlimited — GainLife is a no-op");
                return;
            }

            _livesUI.GainLife();
            Debug.Log($"[UITester] Gained a life — {_livesUI.CurrentLives}/{_livesUI.MaxLives} remaining");
        }

        private void PrintHelp()
        {
            var timerStatus = _timerUI.IsDisabled ? "disabled" : $"{_timerUI.Progress * 100f:0}% remaining";
            var livesStatus = _livesUI.Unlimited  ? "unlimited" : $"{_livesUI.CurrentLives}/{_livesUI.MaxLives}";

            Debug.Log(
                $"[UITester] ── Keyboard Controls ──\n" +
                $"  SPACE      Start timer\n" +
                $"  S          Stop timer\n" +
                $"  R          Reset timer\n" +
                $"  T          Restart timer\n" +
                $"  ←          Lose life\n" +
                $"  →          Gain life\n" +
                $"  H          Print this help\n" +
                $"── Current State ──\n" +
                $"  Timer : {timerStatus}\n" +
                $"  Lives : {livesStatus}"
            );
        }

        #endregion
    }
}