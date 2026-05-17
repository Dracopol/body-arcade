using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace IMVR.BodyArcade.UI
{
    internal enum HandMode
    {
        LeftOnly,
        RightOnly,
        BothSimultaneously
    }

    internal enum MirrorMode
    {
        Unmirrored,
        MirrorLeft,
        MirrorRight
    }

    internal enum ActiveGame
    {
        AirSacShapes,
        NeuroMania
    }

    internal class HandModeConfigController : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private UIDocument _document;
        [SerializeField] private string _airSacShapesSceneName = "AirSacShapesScene";
        [SerializeField] private string _neuromaniaSsceneName  = "NeuromaniScene";

        #endregion

        #region Fields

        private VisualElement _root;
        private List<VisualElement> _handOptionRows;
        private List<VisualElement> _mirrorBtns;
        private List<VisualElement> _focusModeRows;
        private VisualElement _bothHandsPanel;
        private VisualElement _startBtn;

        private VisualElement _gameSwitcher;
        private VisualElement _gameDropdown;
        private VisualElement _contentAirsac;
        private VisualElement _contentNeuromania;
        private Label _switcherLabel;
        private bool _dropdownOpen;

        private HandMode _selectedHandMode   = HandMode.BothSimultaneously;
        private MirrorMode _selectedMirrorMode = MirrorMode.Unmirrored;
        private FocusMode _selectedFocusMode   = FocusMode.Unfocused;
        private ActiveGame _activeGame         = ActiveGame.AirSacShapes;

        #endregion

        #region Events

        internal System.Action<HandMode> OnHandModeChanged;
        internal System.Action<MirrorMode> OnMirrorModeChanged;
        internal System.Action<ActiveGame> OnActiveGameChanged;

        #endregion

        #region Setup

        private void Start()
        {
            _root = _document.rootVisualElement;

            _handOptionRows = new List<VisualElement>
            {
                _root.Q<VisualElement>("option-left-hand"),
                _root.Q<VisualElement>("option-right-hand"),
                _root.Q<VisualElement>("option-both-hands")
            };

            _mirrorBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("mirror-unmirrored"),
                _root.Q<VisualElement>("mirror-left"),
                _root.Q<VisualElement>("mirror-right")
            };

            _focusModeRows = new List<VisualElement>
            {
                _root.Q<VisualElement>("option-normal"),
                _root.Q<VisualElement>("option-left-focused"),
                _root.Q<VisualElement>("option-right-focused")
            };

            _bothHandsPanel    = _root.Q<VisualElement>("both-hands-options");
            _startBtn          = _root.Q<VisualElement>("start-btn");
            _gameSwitcher      = _root.Q<VisualElement>("game-switcher");
            _gameDropdown      = _root.Q<VisualElement>("game-dropdown");
            _contentAirsac     = _root.Q<VisualElement>("content-airsac");
            _contentNeuromania = _root.Q<VisualElement>("content-neuromania");
            _switcherLabel     = _root.Q<Label>("game-switcher__label");

            RegisterHandModeCallbacks();
            RegisterMirrorCallbacks();
            RegisterFocusModeCallbacks();
            RegisterStartCallback();
            RegisterDropdownCallbacks();
            ApplyInitialState();
        }

        private void RegisterHandModeCallbacks()
        {
            _handOptionRows[0].RegisterCallback<ClickEvent>(_ => SelectHandMode(HandMode.LeftOnly));
            _handOptionRows[1].RegisterCallback<ClickEvent>(_ => SelectHandMode(HandMode.RightOnly));
            _handOptionRows[2].RegisterCallback<ClickEvent>(_ => SelectHandMode(HandMode.BothSimultaneously));
        }

        private void RegisterMirrorCallbacks()
        {
            _mirrorBtns[0].RegisterCallback<ClickEvent>(_ => SelectMirrorMode(MirrorMode.Unmirrored));
            _mirrorBtns[1].RegisterCallback<ClickEvent>(_ => SelectMirrorMode(MirrorMode.MirrorLeft));
            _mirrorBtns[2].RegisterCallback<ClickEvent>(_ => SelectMirrorMode(MirrorMode.MirrorRight));
        }

        private void RegisterFocusModeCallbacks()
        {
            _focusModeRows[0].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.Unfocused));
            _focusModeRows[1].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.LeftFocused));
            _focusModeRows[2].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.RightFocused));
        }

        private void RegisterStartCallback()
        {
            _startBtn?.RegisterCallback<ClickEvent>(_ => HandleStartSession());
        }

        private void RegisterDropdownCallbacks()
        {
            _gameSwitcher.RegisterCallback<ClickEvent>(HandleSwitcherClick);
            _root.Q<VisualElement>("dropdown-airsac").RegisterCallback<ClickEvent>(_ => SelectGame(ActiveGame.AirSacShapes));
            _root.Q<VisualElement>("dropdown-neuromania").RegisterCallback<ClickEvent>(_ => SelectGame(ActiveGame.NeuroMania));
            _root.RegisterCallback<ClickEvent>(HandleRootClick);
        }

        private void ApplyInitialState()
        {
            SelectHandMode(_selectedHandMode);
            SelectMirrorMode(_selectedMirrorMode);
            SelectFocusMode(_selectedFocusMode);
            SelectGame(_activeGame);
        }

        #endregion

        #region Event handlers

        private void HandleStartSession()
        {
            if (_activeGame == ActiveGame.AirSacShapes)
            {
                if (SessionConfigAirSacShapes.Instance == null) return;
                SessionConfigAirSacShapes.Instance.MirrorMode = ResolveMirrorMode();
                SessionConfigAirSacShapes.Instance.ApplyHandMode(_selectedHandMode);
                SceneManager.LoadScene(_airSacShapesSceneName);
            }
            else
            {
                if (SessionConfigNeuromania.Instance == null) return;
                SessionConfigNeuromania.Instance.FocusMode = _selectedFocusMode;
                SceneManager.LoadScene(_neuromaniaSsceneName);
            }
        }

        private void HandleSwitcherClick(ClickEvent e)
        {
            e.StopPropagation();
            _dropdownOpen = !_dropdownOpen;

            if (_dropdownOpen)
                PositionDropdownBelowSwitcher();

            _gameDropdown.EnableInClassList("open", _dropdownOpen);
        }

        private void HandleRootClick(ClickEvent e)
        {
            if (_dropdownOpen)
                CloseDropdown();
        }

        #endregion

        #region Private

        private MirrorMode ResolveMirrorMode()
        {
            if (_selectedHandMode == HandMode.LeftOnly || _selectedHandMode == HandMode.RightOnly)
                return MirrorMode.Unmirrored;

            return _selectedMirrorMode;
        }

        private void SelectHandMode(HandMode mode)
        {
            _selectedHandMode = mode;

            foreach (var row in _handOptionRows)
                row.RemoveFromClassList("selected");

            _handOptionRows[(int)mode].AddToClassList("selected");

            _bothHandsPanel.EnableInClassList("visible", mode == HandMode.BothSimultaneously);

            OnHandModeChanged?.Invoke(mode);
        }

        private void SelectMirrorMode(MirrorMode mode)
        {
            _selectedMirrorMode = mode;

            foreach (var btn in _mirrorBtns)
                btn.RemoveFromClassList("selected");

            _mirrorBtns[(int)mode].AddToClassList("selected");

            OnMirrorModeChanged?.Invoke(mode);
        }

        private void SelectFocusMode(FocusMode mode)
        {
            _selectedFocusMode = mode;

            foreach (var row in _focusModeRows)
                row.RemoveFromClassList("selected");

            _focusModeRows[(int)mode].AddToClassList("selected");
        }

        private void SelectGame(ActiveGame game)
        {
            _activeGame = game;
            _switcherLabel.text = game == ActiveGame.AirSacShapes ? "Air Sac Shapes" : "NeuroMania";
            _contentAirsac.EnableInClassList("hidden", game != ActiveGame.AirSacShapes);
            _contentNeuromania.EnableInClassList("hidden", game != ActiveGame.NeuroMania);
            CloseDropdown();

            OnActiveGameChanged?.Invoke(game);
        }

        private void PositionDropdownBelowSwitcher()
        {
            var switcherRect = _gameSwitcher.worldBound;
            var rootRect     = _root.worldBound;

            _gameDropdown.style.top  = switcherRect.yMax - rootRect.yMin + 4f;
            _gameDropdown.style.left = switcherRect.xMin - rootRect.xMin;
        }

        private void CloseDropdown()
        {
            _dropdownOpen = false;
            _gameDropdown.RemoveFromClassList("open");
        }

        #endregion
    }
}