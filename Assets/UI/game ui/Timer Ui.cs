using System;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    #region Events

    public event Action OnTimerExpired;

    #endregion

    #region Serialized Fields

    [SerializeField] private Image _fillImage;
    [SerializeField] private float _durationSeconds = 30f;

    #endregion

    #region Fields

    private float _timeRemaining;
    private bool _isRunning;

    #endregion

    #region Properties

    public float Progress => _timeRemaining / _durationSeconds;
    public bool IsRunning => _isRunning;

    #endregion

    #region Setup

    private void Awake()
    {
        _timeRemaining = _durationSeconds;
        _fillImage.fillAmount = 1f;
    }

    #endregion

    #region Update

    private void Update()
    {
        if (!_isRunning) return;

        _timeRemaining = Mathf.Max(_timeRemaining - Time.deltaTime, 0f);
        _fillImage.fillAmount = Progress;

        if (_timeRemaining > 0f) return;

        _isRunning = false;
        OnTimerExpired?.Invoke();
    }

    #endregion

    #region Public

    public void StartTimer() => _isRunning = true;

    public void StopTimer() => _isRunning = false;

    public void ResetTimer()
    {
        _timeRemaining = _durationSeconds;
        _fillImage.fillAmount = 1f;
    }

    public void RestartTimer()
    {
        ResetTimer();
        StartTimer();
    }

    #endregion
}