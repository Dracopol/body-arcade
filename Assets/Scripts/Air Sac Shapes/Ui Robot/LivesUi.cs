using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace IMVR.BodyArcade.UiRobot
{
    public class LivesUi : MonoBehaviour
    {
        #region Events

        public event Action OnLivesEmpty;

        #endregion

        #region Serialized Fields

        [SerializeField] private LifeDiamonds[] _diamonds;
        [SerializeField] private int _editorStartingLives = 7;

        #endregion

        #region Fields

        private int  _currentLives;
        private int  _activeSlots;
        private bool _unlimited;

        private CancellationTokenSource _shatterCts;

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

        private void OnDestroy()
        {
            _shatterCts?.Cancel();
            _shatterCts?.Dispose();
        }

        #endregion

        #region Public

        public void LoseLife()
        {
            if (_unlimited || _currentLives <= 0) return;

            _currentLives--;
            PlayShatterAsync(_currentLives).Forget();

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

        // Shatter the diamond at the just-emptied slot, leaving the rest untouched.
        private async UniTaskVoid PlayShatterAsync(int slotIndex)
        {
            _shatterCts ??= new CancellationTokenSource();

            try
            {
                await _diamonds[slotIndex].PlayShatterAsync(_shatterCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Cancelled by OnDestroy; nothing to clean up here.
            }
        }

        private void Refresh()
        {
            for (var i = 0; i < _activeSlots; i++)
                _diamonds[i].SetFilled(i < _currentLives);
        }

        #endregion
    }
}
