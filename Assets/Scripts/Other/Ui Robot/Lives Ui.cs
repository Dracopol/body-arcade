using System;
using UnityEngine;

namespace IMVR.BodyArcade.UiRobot
{
    public class LivesUI : MonoBehaviour
    {
        #region Events

        public event Action OnLivesEmpty;

        #endregion

        #region Serialized Fields

        [SerializeField] private LifeDiamond[] _diamonds;
        [SerializeField] private int _editorStartingLives = 7;

        #endregion

        #region Fields

        private int  _currentLives;
        private int  _activeSlots;
        private bool _unlimited;

        #endregion

        #region Properties

        public int  CurrentLives => _currentLives;
        public int  MaxLives     => _activeSlots;
        public bool Unlimited    => _unlimited;

        #endregion

        #region Setup

        private void Awake()
        {
            var config = SessionConfigAirSacShapes.Instance;

            if (config != null)
            {
                _unlimited   = !config.StrikeAmount.HasValue;
                _activeSlots = config.StrikeAmount ?? _diamonds.Length;
            }
            else
            {
                _unlimited   = false;
                _activeSlots = _editorStartingLives;
            }

            _activeSlots = Mathf.Clamp(_activeSlots, 0, Mathf.Min(_diamonds.Length, 10));

            for (var i = 0; i < _diamonds.Length; i++)
                _diamonds[i].SetSlotEnabled(i < _activeSlots);

            _currentLives = _activeSlots;
            Refresh();
        }

        #endregion

        #region Public

        public void LoseLife()
        {
            if (_unlimited || _currentLives <= 0) return;

            _currentLives--;
            Refresh();

            if (_currentLives <= 0)
                OnLivesEmpty?.Invoke();
        }

        public void GainLife()
        {
            if (_unlimited || _currentLives >= _activeSlots) return;

            _currentLives++;
            Refresh();
        }

        public void SetLives(int lives)
        {
            _currentLives = Mathf.Clamp(lives, 0, _activeSlots);
            Refresh();
        }

        #endregion

        #region Private

        private void Refresh()
        {
            for (var i = 0; i < _activeSlots; i++)
                _diamonds[i].SetFilled(i < _currentLives);
        }

        #endregion
    }
}