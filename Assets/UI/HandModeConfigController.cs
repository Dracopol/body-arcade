using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using IMVR.BodyArcade;

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

        // Hand mode
        private List<VisualElement> _handOptionRows;
        private VisualElement       _bothHandsPanel;

        // Mirror
        private List<VisualElement> _mirrorBtns;

        // Focus mode (NeuroMania)
        private List<VisualElement> _focusModeRows;

        // Shape grid
        private VisualElement                       _shapeGrid;
        private Label                               _shapeCountLabel;
        private VisualElement                       _selectAllBtn;
        private VisualElement                       _clearBtn;
        private List<ShapeConfig>                   _availableShapes = new();
        private HashSet<ShapeConfig>                _selectedShapes  = new();
        private Dictionary<ShapeConfig, VisualElement> _shapeCardMap = new();

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

        // Start
        private VisualElement _startBtn;

        // State
        private HandMode   _selectedHandMode   = HandMode.BothSimultaneously;
        private MirrorMode _selectedMirrorMode = MirrorMode.Unmirrored;
        private FocusMode  _selectedFocusMode  = FocusMode.Unfocused;
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

            _bothHandsPanel  = _root.Q<VisualElement>("both-hands-options");
            _gameSwitcher    = _root.Q<VisualElement>("game-switcher");
            _gameDropdown    = _root.Q<VisualElement>("game-dropdown");
            _contentAirsac   = _root.Q<VisualElement>("content-airsac");
            _contentNeuromania = _root.Q<VisualElement>("content-neuromania");
            _switcherLabel   = _root.Q<Label>("game-switcher__label");
            _startBtn        = _root.Q<VisualElement>("start-btn");

            _shapeGrid       = _root.Q<VisualElement>("shape-grid");
            _shapeCountLabel = _root.Q<Label>("shape-count-label");
            _selectAllBtn    = _root.Q<VisualElement>("select-all-btn");
            _clearBtn        = _root.Q<VisualElement>("clear-btn");

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
            RegisterShapeCallbacks();
            RegisterStrikeCallbacks();
            RegisterTimerCallbacks();
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

        private void RegisterShapeCallbacks()
        {
            _selectAllBtn.RegisterCallback<ClickEvent>(_ => HandleSelectAll());
            _clearBtn.RegisterCallback<ClickEvent>(_ => HandleClearShapes());
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
        }

        private void ApplyInitialState()
        {
            SelectHandMode(_selectedHandMode);
            SelectMirrorMode(_selectedMirrorMode);
            SelectFocusMode(_selectedFocusMode);
            SelectGame(_activeGame);
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

                var orderedSelection = new List<ShapeConfig>();
                foreach (var shape in _availableShapes)
                    if (_selectedShapes.Contains(shape))
                        orderedSelection.Add(shape);

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

        private void HandleSelectAll()
        {
            foreach (var (shape, card) in _shapeCardMap)
            {
                _selectedShapes.Add(shape);
                card.AddToClassList("selected");
            }
            UpdateShapeCountLabel();
        }

        private void HandleClearShapes()
        {
            _selectedShapes.Clear();
            foreach (var card in _shapeCardMap.Values)
                card.RemoveFromClassList("selected");
            UpdateShapeCountLabel();
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

        private void SelectGame(ActiveGame game)
        {
            _activeGame = game;
            _switcherLabel.text = game == ActiveGame.AirSacShapes ? "Air Sac Shapes" : "NeuroMania";
            _contentAirsac.EnableInClassList("hidden", game != ActiveGame.AirSacShapes);
            _contentNeuromania.EnableInClassList("hidden", game != ActiveGame.NeuroMania);
            CloseDropdown();
            OnActiveGameChanged?.Invoke(game);
        }

        private void UpdateAvailableShapes(HandMode mode)
        {
            if (SessionConfigAirSacShapes.Instance == null) return;
            _availableShapes = SessionConfigAirSacShapes.Instance.GetShapesForMode(mode);
            RebuildShapeGrid();
        }

        private void RebuildShapeGrid()
        {
            _shapeGrid.Clear();
            _shapeCardMap.Clear();
            _selectedShapes.Clear();

            foreach (var shape in _availableShapes)
            {
                var card = CreateShapeCard(shape);
                _shapeCardMap[shape] = card;
                _shapeGrid.Add(card);
            }

            UpdateShapeCountLabel();
        }

        private VisualElement CreateShapeCard(ShapeConfig shape)
        {
            var card = new VisualElement();
            card.AddToClassList("shape-card");

            var thumbnail = new VisualElement();
            thumbnail.AddToClassList("shape-card__thumbnail");
            thumbnail.pickingMode = PickingMode.Ignore;

            if (shape.ThumbnailSprite != null)
            {
                thumbnail.style.backgroundImage       = new StyleBackground(shape.ThumbnailSprite);
                thumbnail.style.unityBackgroundScaleMode = ScaleMode.ScaleAndCrop;
            }
            else
            {
                thumbnail.style.backgroundColor = new StyleColor(shape.ThumbnailColor);
                // var fallbackLabel = new Label(shape.ShapeName);
                // fallbackLabel.AddToClassList("shape-card__thumbnail-label");
                // fallbackLabel.pickingMode = PickingMode.Ignore;
                // thumbnail.Add(fallbackLabel);
            }

            var info = new VisualElement();
            info.AddToClassList("shape-card__info");
            info.pickingMode = PickingMode.Ignore;

            var nameLabel = new Label(shape.ShapeName);
            nameLabel.AddToClassList("shape-card__name");
            nameLabel.pickingMode = PickingMode.Ignore;

            var diffLabel = new Label(shape.DifficultyLabel);
            diffLabel.AddToClassList("shape-card__difficulty");
            diffLabel.pickingMode = PickingMode.Ignore;

            info.Add(nameLabel);
            info.Add(diffLabel);
            card.Add(thumbnail);
            card.Add(info);

            card.RegisterCallback<ClickEvent>(_ => ToggleShape(card, shape));
            return card;
        }

        private void ToggleShape(VisualElement card, ShapeConfig shape)
        {
            if (_selectedShapes.Contains(shape))
            {
                _selectedShapes.Remove(shape);
                card.RemoveFromClassList("selected");
            }
            else
            {
                _selectedShapes.Add(shape);
                card.AddToClassList("selected");
            }
            UpdateShapeCountLabel();
        }

        private void UpdateShapeCountLabel()
        {
            var count = _selectedShapes.Count;
            _shapeCountLabel.text = count == 1 ? "1 shape selected" : $"{count} shapes selected";
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

        private void CloseDropdown()
        {
            _dropdownOpen = false;
            _gameDropdown.RemoveFromClassList("open");
        }

        #endregion
    }
}