using System;
using UnityEngine;

public class LivesUI : MonoBehaviour
{
    #region Events

    public event Action OnLivesEmpty;

    #endregion

    #region Serialized Fields

    [SerializeField] private LifeDiamond[] _diamonds;
    [SerializeField] private int _startingLives = 4;

    #endregion

    #region Fields

    private int _currentLives;

    #endregion

    #region Properties

    public int CurrentLives => _currentLives;
    public int MaxLives => _diamonds.Length;

    #endregion

    #region Setup

    private void Awake()
    {
        SetLives(_startingLives);
    }

    #endregion

    #region Public

    public void LoseLife()
    {
        if (_currentLives <= 0) return;

        SetLives(_currentLives - 1);

        if (_currentLives <= 0)
            OnLivesEmpty?.Invoke();
    }

    public void GainLife()
    {
        if (_currentLives >= MaxLives) return;

        SetLives(_currentLives + 1);
    }

    public void SetLives(int lives)
    {
        _currentLives = Mathf.Clamp(lives, 0, MaxLives);
        Refresh();
    }

    #endregion

    #region Private

    private void Refresh()
    {
        for (int i = 0; i < _diamonds.Length; i++)
            _diamonds[i].SetFilled(i < _currentLives);
    }

    #endregion
}