using UnityEngine;
using UnityEngine.InputSystem;

public class UITester : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TimerUI _timerUI;
    [SerializeField] private LivesUI _livesUI;

    #endregion

    #region Setup

    private void Awake()
    {
        _timerUI.OnTimerExpired += HandleTimerExpired;
        _livesUI.OnLivesEmpty += HandleLivesEmpty;
    }

    private void OnDestroy()
    {
        _timerUI.OnTimerExpired -= HandleTimerExpired;
        _livesUI.OnLivesEmpty -= HandleLivesEmpty;
    }

    #endregion

    #region Update

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Timer
        if (keyboard.spaceKey.wasPressedThisFrame)      _timerUI.StartTimer();
        if (keyboard.sKey.wasPressedThisFrame)           _timerUI.StopTimer();
        if (keyboard.rKey.wasPressedThisFrame)           _timerUI.ResetTimer();

        // Lives
        if (keyboard.leftArrowKey.wasPressedThisFrame)  _livesUI.LoseLife();
        if (keyboard.rightArrowKey.wasPressedThisFrame) _livesUI.GainLife();
    }

    #endregion

    #region Event Handlers

    private void HandleTimerExpired() => Debug.Log("Timer expired");
    private void HandleLivesEmpty()   => Debug.Log("No lives left");

    #endregion
}