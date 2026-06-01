using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.UiRobot
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

        #region Events

        internal System.Action<HandMode>   OnHandModeChanged;
        internal System.Action<MirrorMode> OnMirrorModeChanged;
        internal System.Action<ActiveGame> OnActiveGameChanged;

        #endregion

        #region Fields

        // Root
        private VisualElement _root;

        // Game switcher
        private VisualElement _gameSwitcher;
        private VisualElement _gameDropdown;
        private VisualElement _contentAirsac;
        private VisualElement _contentNeuromania;
        private Label         _switcherLabel;
        private bool          _dropdownOpen;
        private VisualElement _advancedToggle;
        private VisualElement _themeSwitcher;
        private VisualElement _themeDropdown;
        private Label         _themeSwitcherLabel;
        private bool          _themeDropdownOpen;

        // NeuroMania sections
        private VisualElement _neuroHandModeSection;
        private VisualElement _neuroHeightSection;
        private VisualElement _neuroTargetBiasSection;
        private bool          _isAdvancedMode;

        // Hand mode
        private List<VisualElement> _handOptionRows;
        private VisualElement       _bothHandsPanel;

        // Mirror
        private List<VisualElement> _mirrorBtns;

        // Focus mode (NeuroMania)
        private List<VisualElement> _focusModeRows;

        // Height mode (NeuroMania)
        private List<VisualElement> _heightModeRows;

        // Shape availability
        private List<ShapeConfig> _availableShapes = new();

        // Difficulty
        private List<VisualElement> _difficultyBtns;
        private string              _selectedDifficulty = "Easy";

        // Strike stepper
        private VisualElement _strikeDecreaseBtn;
        private VisualElement _strikeIncreaseBtn;
        private VisualElement _strikeUnlimitedBtn;
        private Label         _strikeValueLabel;
        private Label         _strikeInfoLabel;

        // Timer stepper
        private VisualElement _timerDecreaseBtn;
        private VisualElement _timerIncreaseBtn;
        private VisualElement _timerDisableBtn;
        private Label         _timerValueLabel;
        private Label         _timerInfoLabel;

        // Bias picker
        private VisualElement _biasGrid;
        private VisualElement _biasActiveDot;
        private bool          _isDraggingBias;
        private float         _handBias   = 0f;
        private float         _heightBias = 0f;

        // Start
        private VisualElement _startBtn;

        // State
        private HandMode   _selectedHandMode   = HandMode.BothSimultaneously;
        private MirrorMode _selectedMirrorMode = MirrorMode.Unmirrored;
        private FocusMode  _selectedFocusMode  = FocusMode.Unfocused;
        private HeightMode _selectedHeightMode = HeightMode.EyeLevel;
        private ActiveGame _activeGame         = ActiveGame.AirSacShapes;
        private int?       _strikeAmount       = 5;
        private int?       _timerSeconds       = 60;

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

            _heightModeRows = new List<VisualElement>
            {
                _root.Q<VisualElement>("option-eye-level"),
                _root.Q<VisualElement>("option-height-high"),
                _root.Q<VisualElement>("option-height-mixed")
            };

            _bothHandsPanel  = _root.Q<VisualElement>("both-hands-options");
            _gameSwitcher    = _root.Q<VisualElement>("game-switcher");
            _gameDropdown    = _root.Q<VisualElement>("game-dropdown");
            _contentAirsac   = _root.Q<VisualElement>("content-airsac");
            _contentNeuromania = _root.Q<VisualElement>("content-neuromania");
            _switcherLabel   = _root.Q<Label>("game-switcher__label");
            _startBtn               = _root.Q<VisualElement>("start-btn");
            _biasGrid               = _root.Q<VisualElement>("bias-grid");
            _biasActiveDot          = _root.Q<VisualElement>("bias-dot-active");
            _advancedToggle         = _root.Q<VisualElement>("advanced-toggle");
            _themeSwitcher          = _root.Q<VisualElement>("theme-switcher");
            _themeDropdown          = _root.Q<VisualElement>("theme-dropdown");
            _themeSwitcherLabel     = _root.Q<Label>("theme-switcher__label");
            _neuroHandModeSection   = _root.Q<VisualElement>("neuro-hand-mode");
            _neuroHeightSection     = _root.Q<VisualElement>("neuro-height");
            _neuroTargetBiasSection = _root.Q<VisualElement>("neuro-target-bias");

            _difficultyBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("difficulty-easy"),
                _root.Q<VisualElement>("difficulty-medium"),
                _root.Q<VisualElement>("difficulty-hard")
            };

            _strikeDecreaseBtn  = _root.Q<VisualElement>("strike-decrease-btn");
            _strikeIncreaseBtn  = _root.Q<VisualElement>("strike-increase-btn");
            _strikeUnlimitedBtn = _root.Q<VisualElement>("strike-unlimited-btn");
            _strikeValueLabel   = _root.Q<Label>("strike-value-label");
            _strikeInfoLabel    = _root.Q<Label>("strike-info-label");

            _timerDecreaseBtn = _root.Q<VisualElement>("timer-decrease-btn");
            _timerIncreaseBtn = _root.Q<VisualElement>("timer-increase-btn");
            _timerDisableBtn  = _root.Q<VisualElement>("timer-disable-btn");
            _timerValueLabel  = _root.Q<Label>("timer-value-label");
            _timerInfoLabel   = _root.Q<Label>("timer-info-label");

            RegisterHandModeCallbacks();
            RegisterMirrorCallbacks();
            RegisterFocusModeCallbacks();
            RegisterHeightModeCallbacks();
            RegisterDifficultyCallbacks();
            RegisterStrikeCallbacks();
            RegisterTimerCallbacks();
            RegisterStartCallback();
            RegisterDropdownCallbacks();
            RegisterBiasPickerCallbacks();
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

        private void RegisterDifficultyCallbacks()
        {
            _difficultyBtns[0].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Easy"));
            _difficultyBtns[1].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Medium"));
            _difficultyBtns[2].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Hard"));
        }

        private void RegisterStrikeCallbacks()
        {
            _strikeDecreaseBtn.RegisterCallback<ClickEvent>(_ => AdjustStrikeAmount(-1));
            _strikeIncreaseBtn.RegisterCallback<ClickEvent>(_ => AdjustStrikeAmount(1));
            _strikeUnlimitedBtn.RegisterCallback<ClickEvent>(_ => ToggleUnlimitedStrikes());
        }

        private void RegisterTimerCallbacks()
        {
            _timerDecreaseBtn.RegisterCallback<ClickEvent>(_ => AdjustTimer(-5));
            _timerIncreaseBtn.RegisterCallback<ClickEvent>(_ => AdjustTimer(5));
            _timerDisableBtn.RegisterCallback<ClickEvent>(_ => ToggleDisableTimer());
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
            _advancedToggle.RegisterCallback<ClickEvent>(_ => SetAdvancedMode(!_isAdvancedMode));
            _themeSwitcher.RegisterCallback<ClickEvent>(HandleThemeSwitcherClick);
            _root.Q<VisualElement>("theme-dropdown-minimalist").RegisterCallback<ClickEvent>(_ => SelectTheme(false));
            _root.Q<VisualElement>("theme-dropdown-sky")       .RegisterCallback<ClickEvent>(_ => SelectTheme(true));
        }

        private void ApplyInitialState()
        {
            SelectHandMode(_selectedHandMode);
            SelectMirrorMode(_selectedMirrorMode);
            SelectFocusMode(_selectedFocusMode);
            SelectHeightMode(_selectedHeightMode);
            SelectDifficulty(_selectedDifficulty);
            SelectGame(_activeGame);
            SelectTheme(false);
            UpdateStrikeDisplay();
            UpdateTimerDisplay();
        }

        #endregion

        #region Event handlers

        private void HandleStartSession()
        {
            if (_activeGame == ActiveGame.AirSacShapes)
            {
                if (SessionConfigAirSacShapes.Instance == null) return;

                var orderedSelection = _availableShapes
                    .Where(s => s.DifficultyLabel == _selectedDifficulty)
                    .ToList();

                SessionConfigAirSacShapes.Instance.MirrorMode     = ResolveMirrorMode();
                SessionConfigAirSacShapes.Instance.HandMode        = _selectedHandMode;
                SessionConfigAirSacShapes.Instance.SelectedShapes  = orderedSelection;
                SessionConfigAirSacShapes.Instance.StrikeAmount    = _strikeAmount;
                SessionConfigAirSacShapes.Instance.TimerSeconds    = _timerSeconds;
                SceneManager.LoadScene(_airSacShapesSceneName);
            }
            else
            {
                if (SessionConfigNeuromania.Instance == null) return;
                SessionConfigNeuromania.Instance.FocusMode   = _selectedFocusMode;
                SessionConfigNeuromania.Instance.HeightMode  = _selectedHeightMode;
                SessionConfigNeuromania.Instance.HandBias   = _handBias;
                SessionConfigNeuromania.Instance.HeightBias = _heightBias;
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
            if (_themeDropdownOpen)
                CloseThemeDropdown();
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

            UpdateAvailableShapes(mode);
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

        private void RegisterHeightModeCallbacks()
        {
            _heightModeRows[0].RegisterCallback<ClickEvent>(_ => SelectHeightMode(HeightMode.EyeLevel));
            _heightModeRows[1].RegisterCallback<ClickEvent>(_ => SelectHeightMode(HeightMode.High));
            _heightModeRows[2].RegisterCallback<ClickEvent>(_ => SelectHeightMode(HeightMode.Mixed));
        }

        private void SelectHeightMode(HeightMode mode)
        {
            _selectedHeightMode = mode;

            foreach (var row in _heightModeRows)
                row.RemoveFromClassList("selected");

            _heightModeRows[(int)mode].AddToClassList("selected");
        }

        private void SelectGame(ActiveGame game)
        {
            _activeGame = game;
            _switcherLabel.text = game == ActiveGame.AirSacShapes ? "Air Sac Shapes" : "NeuroMania";
            _contentAirsac.EnableInClassList("hidden", game != ActiveGame.AirSacShapes);
            _contentNeuromania.EnableInClassList("hidden", game != ActiveGame.NeuroMania);

            var isNeuro = game == ActiveGame.NeuroMania;
            _advancedToggle.style.display = isNeuro ? DisplayStyle.Flex : DisplayStyle.None;
            _root.EnableInClassList("game-neuromania", isNeuro);
            if (isNeuro) SetAdvancedMode(false);

            CloseDropdown();
            OnActiveGameChanged?.Invoke(game);
        }

        private void UpdateAvailableShapes(HandMode mode)
        {
            if (SessionConfigAirSacShapes.Instance == null) return;
            _availableShapes = SessionConfigAirSacShapes.Instance.GetShapesForMode(mode);
        }

        private void SelectDifficulty(string difficulty)
        {
            _selectedDifficulty = difficulty;
            var labels = new[] { "Easy", "Medium", "Hard" };
            for (int i = 0; i < _difficultyBtns.Count; i++)
                _difficultyBtns[i].EnableInClassList("selected", labels[i] == difficulty);
        }

        private void AdjustStrikeAmount(int delta)
        {
            if (_strikeAmount == null) return;
            _strikeAmount = Mathf.Max(1, _strikeAmount.Value + delta);
            UpdateStrikeDisplay();
        }

        private void ToggleUnlimitedStrikes()
        {
            _strikeAmount = _strikeAmount == null ? 10 : (int?)null;
            UpdateStrikeDisplay();
        }

        private void UpdateStrikeDisplay()
        {
            var isUnlimited = _strikeAmount == null;
            _strikeValueLabel.text = isUnlimited ? "∞" : _strikeAmount.Value.ToString();
            _strikeUnlimitedBtn.EnableInClassList("active", isUnlimited);
            _strikeDecreaseBtn.EnableInClassList("disabled", isUnlimited);
            _strikeIncreaseBtn.EnableInClassList("disabled", isUnlimited);
            _strikeInfoLabel.text = isUnlimited ? "No strike limit" : $"Target: {_strikeAmount} strikes";
        }

        private void AdjustTimer(int delta)
        {
            if (_timerSeconds == null) return;
            _timerSeconds = Mathf.Max(5, _timerSeconds.Value + delta);
            UpdateTimerDisplay();
        }

        private void ToggleDisableTimer()
        {
            _timerSeconds = _timerSeconds == null ? 60 : (int?)null;
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            var isDisabled = _timerSeconds == null;
            _timerValueLabel.text = isDisabled ? "—" : _timerSeconds.Value.ToString();
            _timerDisableBtn.EnableInClassList("active", isDisabled);
            _timerDecreaseBtn.EnableInClassList("disabled", isDisabled);
            _timerIncreaseBtn.EnableInClassList("disabled", isDisabled);
            _timerInfoLabel.text = isDisabled ? "No time limit" : FormatDuration(_timerSeconds.Value);
        }

        private static string FormatDuration(int seconds)
        {
            if (seconds < 60) return $"Session duration: {seconds}s";
            var minutes = seconds / 60;
            var remainder = seconds % 60;
            return remainder == 0
                ? $"Session duration: {minutes}m"
                : $"Session duration: {minutes}m {remainder}s";
        }

        private void PositionDropdownBelowSwitcher()
        {
            var switcherRect = _gameSwitcher.worldBound;
            var rootRect     = _root.worldBound;
            _gameDropdown.style.top  = switcherRect.yMax - rootRect.yMin + 4f;
            _gameDropdown.style.left = switcherRect.xMin - rootRect.xMin;
        }

        private void HandleThemeSwitcherClick(ClickEvent e)
        {
            e.StopPropagation();
            _themeDropdownOpen = !_themeDropdownOpen;
            if (_themeDropdownOpen)
                PositionThemeDropdown();
            _themeDropdown.EnableInClassList("open", _themeDropdownOpen);
        }

        private void SelectTheme(bool sky)
        {
            _themeSwitcherLabel.text = sky ? "Sky" : "Minimalist";
            _root.EnableInClassList("theme-sky", sky);
            CloseThemeDropdown();
        }

        private void PositionThemeDropdown()
        {
            var switcherRect = _themeSwitcher.worldBound;
            var rootRect     = _root.worldBound;
            _themeDropdown.style.top  = switcherRect.yMax - rootRect.yMin + 4f;
            _themeDropdown.style.left = switcherRect.xMin - rootRect.xMin;
        }

        private void CloseThemeDropdown()
        {
            _themeDropdownOpen = false;
            _themeDropdown.RemoveFromClassList("open");
        }

        private void SetAdvancedMode(bool on)
        {
            _isAdvancedMode = on;
            _advancedToggle.EnableInClassList("active", on);
            _neuroHandModeSection.EnableInClassList("section-hidden", on);
            _neuroHeightSection.EnableInClassList("section-hidden", on);
            _neuroTargetBiasSection.EnableInClassList("section-hidden", !on);
        }

        private void CloseDropdown()
        {
            _dropdownOpen = false;
            _gameDropdown.RemoveFromClassList("open");
        }

        private void RegisterBiasPickerCallbacks()
        {
            _biasGrid.RegisterCallback<PointerDownEvent>(OnBiasPointerDown);
            _biasGrid.RegisterCallback<PointerMoveEvent>(OnBiasPointerMove);
            _biasGrid.RegisterCallback<PointerUpEvent>(OnBiasPointerUp);

            _root.Q<VisualElement>("snap-left-abduction") .RegisterCallback<ClickEvent>(_ => SnapBias(-1f,  1f));
            _root.Q<VisualElement>("snap-abduction")      .RegisterCallback<ClickEvent>(_ => SnapBias( 0f,  1f));
            _root.Q<VisualElement>("snap-right-abduction").RegisterCallback<ClickEvent>(_ => SnapBias( 1f,  1f));
            _root.Q<VisualElement>("snap-left")           .RegisterCallback<ClickEvent>(_ => SnapBias(-1f,  0f));
            _root.Q<VisualElement>("snap-default")        .RegisterCallback<ClickEvent>(_ => SnapBias( 0f,  0f));
            _root.Q<VisualElement>("snap-right")          .RegisterCallback<ClickEvent>(_ => SnapBias( 1f,  0f));
            _root.Q<VisualElement>("snap-left-adduction") .RegisterCallback<ClickEvent>(_ => SnapBias(-1f, -1f));
            _root.Q<VisualElement>("snap-adduction")      .RegisterCallback<ClickEvent>(_ => SnapBias( 0f, -1f));
            _root.Q<VisualElement>("snap-right-adduction").RegisterCallback<ClickEvent>(_ => SnapBias( 1f, -1f));
        }

        private void OnBiasPointerDown(PointerDownEvent e)
        {
            _isDraggingBias = true;
            _biasGrid.CapturePointer(e.pointerId);
            UpdateBiasFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnBiasPointerMove(PointerMoveEvent e)
        {
            if (!_isDraggingBias) return;
            UpdateBiasFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnBiasPointerUp(PointerUpEvent e)
        {
            if (!_isDraggingBias) return;
            _isDraggingBias = false;
            _biasGrid.ReleasePointer(e.pointerId);
        }

        private void UpdateBiasFromLocal(Vector2 localPos)
        {
            var w = _biasGrid.resolvedStyle.width;
            var h = _biasGrid.resolvedStyle.height;
            _handBias   = Mathf.Clamp(localPos.x / w * 2f - 1f, -1f, 1f);
            _heightBias = Mathf.Clamp(1f - localPos.y / h * 2f, -1f, 1f);
            PositionActiveDot();
        }

        private void SnapBias(float hand, float height)
        {
            _handBias   = hand;
            _heightBias = height;
            PositionActiveDot();
        }

        private void PositionActiveDot()
        {
            var w = _biasGrid.resolvedStyle.width;
            var h = _biasGrid.resolvedStyle.height;
            if (w <= 0f || h <= 0f) return;

            const float dotRadius = 6f;
            _biasActiveDot.style.left       = (_handBias + 1f) * 0.5f * w - dotRadius;
            _biasActiveDot.style.top        = (1f - _heightBias) * 0.5f * h - dotRadius;
            _biasActiveDot.style.marginLeft = 0;
            _biasActiveDot.style.marginTop  = 0;
        }

        #endregion
    }
}