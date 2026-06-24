using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using IMVR.BodyArcade;

namespace IMVR.BodyArcade.UiRobot
{
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
        [SerializeField] private AirSacShapesPreparer _shapesPreparer;

        #endregion

        #region Events

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

        // NeuroMania sections
        private VisualElement _neuroHandModeSection;
        private VisualElement _neuroHeightSection;
        private VisualElement _neuroTargetBiasSection;
        private bool          _isAdvancedMode;

        // Focus mode (NeuroMania)
        private List<VisualElement> _focusModeRows;

        // Height mode (NeuroMania)
        private List<VisualElement> _heightModeRows;

        // Hand focus slider
        private VisualElement _handModeAdvBtn;
        private VisualElement _handModeOptions;
        private VisualElement _handFocusSlider;
        private VisualElement _handFocusTrack;
        private VisualElement _handFocusKnob;
        private bool          _handModeIsAdvanced;
        private bool          _isDraggingHandFocus;
        private float         _handFocusBias = 0f;


        // AirSac hand mode
        private List<VisualElement> _airsacHandModeBtns;
        private string              _selectedAirsacHandMode = "Both";

        // Hand visualization
        private List<VisualElement> _handVizBtns;
        private bool                _handVisualization = true;

        // Vibration
        private List<VisualElement> _vibrationBtns;
        private bool                _vibration = true;

        // Difficulty
        private List<VisualElement> _difficultyBtns;
        private string              _selectedDifficulty = "Easy";

        // NeuroMania hand mode (new section)
        private List<VisualElement> _neuroHmBtns;
        private string              _selectedNeuroHandMode = "Mixed";

        // NeuroMania difficulty
        private List<VisualElement> _neuroDifficultyBtns;
        private string              _selectedNeuroDifficulty = "Easy";

        // Bias picker
        private VisualElement _biasGrid;
        private VisualElement _biasActiveDot;
        private bool          _isDraggingBias;
        private float         _handBias   = 0f;
        private float         _heightBias = 0f;

        // Axis sliders
        private VisualElement _biasHSlider;
        private VisualElement _biasHSliderKnob;
        private VisualElement _biasVSlider;
        private VisualElement _biasVSliderKnob;
        private bool          _isDraggingHSlider;
        private bool          _isDraggingVSlider;

        // Start
        private VisualElement _startBtn;

        // State
        private MirrorMode _selectedMirrorMode = MirrorMode.Unmirrored;
        private FocusMode  _selectedFocusMode  = FocusMode.Unfocused;
        private HeightMode _selectedHeightMode = HeightMode.EyeLevel;
        private ActiveGame _activeGame         = ActiveGame.AirSacShapes;

        #endregion

        #region Setup

        private void Start()
        {
            _root = _document.rootVisualElement;

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

            _gameSwitcher    = _root.Q<VisualElement>("game-switcher");
            _gameDropdown    = _root.Q<VisualElement>("game-dropdown");
            _contentAirsac   = _root.Q<VisualElement>("content-airsac");
            _contentNeuromania = _root.Q<VisualElement>("content-neuromania");
            _switcherLabel   = _root.Q<Label>("game-switcher__label");
            _startBtn               = _root.Q<VisualElement>("start-btn");
            _handModeAdvBtn  = _root.Q<VisualElement>("hand-mode-adv-btn");
            _handModeOptions = _root.Q<VisualElement>("hand-mode-options");
            _handFocusSlider = _root.Q<VisualElement>("hand-focus-slider");
            _handFocusTrack  = _root.Q<VisualElement>("hfocus-track");
            _handFocusKnob   = _root.Q<VisualElement>("hfocus-knob");
            _biasGrid               = _root.Q<VisualElement>("bias-grid");
            _biasActiveDot          = _root.Q<VisualElement>("bias-dot-active");
            _biasHSlider            = _root.Q<VisualElement>("bias-hslider");
            _biasHSliderKnob        = _root.Q<VisualElement>("bias-hslider-knob");
            _biasVSlider            = _root.Q<VisualElement>("bias-vslider");
            _biasVSliderKnob        = _root.Q<VisualElement>("bias-vslider-knob");
            _advancedToggle         = _root.Q<VisualElement>("advanced-toggle");
            _neuroHandModeSection   = _root.Q<VisualElement>("neuro-hand-mode");
            _neuroHeightSection     = _root.Q<VisualElement>("neuro-height");
            _neuroTargetBiasSection = _root.Q<VisualElement>("neuro-target-bias");

            _airsacHandModeBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("airsac-hm-btn-left-mixed"),
                _root.Q<VisualElement>("airsac-hm-btn-mixed"),
                _root.Q<VisualElement>("airsac-hm-btn-right-mixed")
            };

            _handVizBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("hand-viz-btn-no"),
                _root.Q<VisualElement>("hand-viz-btn-yes")
            };

            _vibrationBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("vibration-btn-no"),
                _root.Q<VisualElement>("vibration-btn-yes")
            };

            _difficultyBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("difficulty-easy"),
                _root.Q<VisualElement>("difficulty-medium"),
                _root.Q<VisualElement>("difficulty-hard")
            };

            _neuroHmBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("neuro-hm-btn-0"),
                _root.Q<VisualElement>("neuro-hm-btn-2"),
                _root.Q<VisualElement>("neuro-hm-btn-4")
            };

            _neuroDifficultyBtns = new List<VisualElement>
            {
                _root.Q<VisualElement>("neuro-difficulty-easy"),
                _root.Q<VisualElement>("neuro-difficulty-medium"),
                _root.Q<VisualElement>("neuro-difficulty-hard")
            };

            RegisterFocusModeCallbacks();
            RegisterHeightModeCallbacks();
            RegisterHandFocusSliderCallbacks();
            RegisterAirsacHandModeCallbacks();
            RegisterHandVizCallbacks();
            RegisterVibrationCallbacks();
            RegisterDifficultyCallbacks();
            RegisterNeuroHandModeCallbacks();
            RegisterNeuroDifficultyCallbacks();
            RegisterStartCallback();
            RegisterDropdownCallbacks();
            RegisterBiasPickerCallbacks();
            ApplyInitialState();
        }

        private void RegisterFocusModeCallbacks()
        {
            _focusModeRows[0].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.Unfocused));
            _focusModeRows[1].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.LeftFocused));
            _focusModeRows[2].RegisterCallback<ClickEvent>(_ => SelectFocusMode(FocusMode.RightFocused));
        }

        private void RegisterHandVizCallbacks()
        {
            _handVizBtns[0].RegisterCallback<ClickEvent>(_ => SelectHandViz(false));
            _handVizBtns[1].RegisterCallback<ClickEvent>(_ => SelectHandViz(true));
        }

        private void RegisterVibrationCallbacks()
        {
            _vibrationBtns[0].RegisterCallback<ClickEvent>(_ => SelectVibration(false));
            _vibrationBtns[1].RegisterCallback<ClickEvent>(_ => SelectVibration(true));
        }

        private void RegisterAirsacHandModeCallbacks()
        {
            _airsacHandModeBtns[0].RegisterCallback<ClickEvent>(_ => SelectAirsacHandMode("Left",  MirrorMode.MirrorLeft));
            _airsacHandModeBtns[1].RegisterCallback<ClickEvent>(_ => SelectAirsacHandMode("Both",  MirrorMode.Unmirrored));
            _airsacHandModeBtns[2].RegisterCallback<ClickEvent>(_ => SelectAirsacHandMode("Right", MirrorMode.MirrorRight));
        }

        private void RegisterDifficultyCallbacks()
        {
            _difficultyBtns[0].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Easy"));
            _difficultyBtns[1].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Medium"));
            _difficultyBtns[2].RegisterCallback<ClickEvent>(_ => SelectDifficulty("Hard"));
        }

        private void RegisterNeuroHandModeCallbacks()
        {
            _neuroHmBtns[0].RegisterCallback<ClickEvent>(_ => SelectNeuroHandMode("Left"));
            _neuroHmBtns[1].RegisterCallback<ClickEvent>(_ => SelectNeuroHandMode("Mixed"));
            _neuroHmBtns[2].RegisterCallback<ClickEvent>(_ => SelectNeuroHandMode("Right"));
        }

        private void RegisterNeuroDifficultyCallbacks()
        {
            _neuroDifficultyBtns[0].RegisterCallback<ClickEvent>(_ => SelectNeuroDifficulty("Easy"));
            _neuroDifficultyBtns[1].RegisterCallback<ClickEvent>(_ => SelectNeuroDifficulty("Medium"));
            _neuroDifficultyBtns[2].RegisterCallback<ClickEvent>(_ => SelectNeuroDifficulty("Hard"));
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
        }

        private void ApplyInitialState()
        {
            SelectFocusMode(_selectedFocusMode);
            SelectHeightMode(_selectedHeightMode);
            SelectAirsacHandMode(_selectedAirsacHandMode, _selectedMirrorMode);
            SelectHandViz(_handVisualization);
            SelectVibration(_vibration);
            SelectDifficulty(_selectedDifficulty);
            SelectNeuroHandMode(_selectedNeuroHandMode);
            SelectNeuroDifficulty(_selectedNeuroDifficulty);
            SelectGame(_activeGame);
        }

        #endregion

        #region Event handlers

        private void HandleStartSession()
        {
            if (_activeGame == ActiveGame.AirSacShapes)
            {
                if (_shapesPreparer == null)
                {
                    var preparers = FindObjectsByType<AirSacShapesPreparer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (preparers.Length > 0) _shapesPreparer = preparers[0];
                }

                if (SessionConfigAirSacShapes.Instance == null || _shapesPreparer == null) return;

                SessionConfigAirSacShapes.Instance.MirrorMode        = _selectedMirrorMode;
                SessionConfigAirSacShapes.Instance.HandVisualization = _handVisualization;
                SessionConfigAirSacShapes.Instance.Vibration         = _vibration;
                SessionConfigAirSacShapes.Instance.SelectedShapes    = _shapesPreparer.PrepareForDifficulty(_selectedDifficulty);
                SceneManager.LoadScene(_airSacShapesSceneName);
            }
            else
            {
                if (SessionConfigNeuromania.Instance == null) return;
                SessionConfigNeuromania.Instance.FocusMode     = _selectedFocusMode;
                SessionConfigNeuromania.Instance.HandFocusBias = _handModeIsAdvanced
                    ? _handFocusBias
                    : (_selectedFocusMode == FocusMode.LeftFocused ? -1f : _selectedFocusMode == FocusMode.RightFocused ? 1f : 0f);
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
        }


        #endregion

        #region Private

        private void SelectFocusMode(FocusMode mode)
        {
            _selectedFocusMode = mode;

            foreach (var row in _focusModeRows)
                row.RemoveFromClassList("selected");

            _focusModeRows[(int)mode].AddToClassList("selected");
        }

        private void RegisterHandFocusSliderCallbacks()
        {
            _handModeAdvBtn.RegisterCallback<ClickEvent>(_ => ToggleHandModeAdvanced());
            _handFocusTrack.RegisterCallback<PointerDownEvent>(OnHandFocusPointerDown);
            _handFocusTrack.RegisterCallback<PointerMoveEvent>(OnHandFocusPointerMove);
            _handFocusTrack.RegisterCallback<PointerUpEvent>(OnHandFocusPointerUp);
        }

        private void ToggleHandModeAdvanced()
        {
            _handModeIsAdvanced = !_handModeIsAdvanced;
            _handModeAdvBtn.EnableInClassList("active", _handModeIsAdvanced);
            _handModeOptions.EnableInClassList("section-hidden", _handModeIsAdvanced);
            _handFocusSlider.EnableInClassList("section-hidden", !_handModeIsAdvanced);
        }

        private void OnHandFocusPointerDown(PointerDownEvent e)
        {
            _isDraggingHandFocus = true;
            _handFocusTrack.CapturePointer(e.pointerId);
            UpdateHandFocusFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnHandFocusPointerMove(PointerMoveEvent e)
        {
            if (!_isDraggingHandFocus) return;
            UpdateHandFocusFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnHandFocusPointerUp(PointerUpEvent e)
        {
            if (!_isDraggingHandFocus) return;
            _isDraggingHandFocus = false;
            _handFocusTrack.ReleasePointer(e.pointerId);
        }

        private void UpdateHandFocusFromLocal(Vector2 localPos)
        {
            var w = _handFocusTrack.resolvedStyle.width;
            _handFocusBias = Mathf.Clamp(localPos.x / w * 2f - 1f, -1f, 1f);
            PositionHandFocusKnob();
        }

        private void PositionHandFocusKnob()
        {
            var w = _handFocusTrack.resolvedStyle.width;
            if (w <= 0f) return;
            const float knobRadius = 10f;
            _handFocusKnob.style.left       = (_handFocusBias + 1f) * 0.5f * w - knobRadius;
            _handFocusKnob.style.marginLeft = 0;
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


        private void SelectHandViz(bool show)
        {
            _handVisualization = show;
            _handVizBtns[0].EnableInClassList("selected", !show);
            _handVizBtns[1].EnableInClassList("selected", show);
        }

        private void SelectVibration(bool enabled)
        {
            _vibration = enabled;
            _vibrationBtns[0].EnableInClassList("selected", !enabled);
            _vibrationBtns[1].EnableInClassList("selected", enabled);
        }

        private void SelectAirsacHandMode(string mode, MirrorMode mirror)
        {
            _selectedAirsacHandMode = mode;
            _selectedMirrorMode     = mirror;
            var labels = new[] { "Left", "Both", "Right" };
            for (int i = 0; i < _airsacHandModeBtns.Count; i++)
                _airsacHandModeBtns[i].EnableInClassList("selected", labels[i] == mode);
        }

        private void SelectDifficulty(string difficulty)
        {
            _selectedDifficulty = difficulty;
            var labels = new[] { "Easy", "Medium", "Hard" };
            for (int i = 0; i < _difficultyBtns.Count; i++)
                _difficultyBtns[i].EnableInClassList("selected", labels[i] == difficulty);
        }

        private void SelectNeuroHandMode(string mode)
        {
            _selectedNeuroHandMode = mode;
            var labels = new[] { "Left", "Mixed", "Right" };
            for (int i = 0; i < _neuroHmBtns.Count; i++)
                _neuroHmBtns[i].EnableInClassList("selected", labels[i] == mode);
        }

        private void SelectNeuroDifficulty(string difficulty)
        {
            _selectedNeuroDifficulty = difficulty;
            var labels = new[] { "Easy", "Medium", "Hard" };
            for (int i = 0; i < _neuroDifficultyBtns.Count; i++)
                _neuroDifficultyBtns[i].EnableInClassList("selected", labels[i] == difficulty);
        }

        private void PositionDropdownBelowSwitcher()
        {
            var switcherRect = _gameSwitcher.worldBound;
            var rootRect     = _root.worldBound;
            _gameDropdown.style.top  = switcherRect.yMax - rootRect.yMin + 4f;
            _gameDropdown.style.left = switcherRect.xMin - rootRect.xMin;
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

            _biasHSlider.RegisterCallback<PointerDownEvent>(OnHSliderPointerDown);
            _biasHSlider.RegisterCallback<PointerMoveEvent>(OnHSliderPointerMove);
            _biasHSlider.RegisterCallback<PointerUpEvent>(OnHSliderPointerUp);

            _biasVSlider.RegisterCallback<PointerDownEvent>(OnVSliderPointerDown);
            _biasVSlider.RegisterCallback<PointerMoveEvent>(OnVSliderPointerMove);
            _biasVSlider.RegisterCallback<PointerUpEvent>(OnVSliderPointerUp);
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
            var nx = SnapToGrid(Mathf.Clamp01(localPos.x / w));
            var ny = SnapToGrid(Mathf.Clamp01(localPos.y / h));
            _handBias   = nx * 2f - 1f;
            _heightBias = 1f - ny * 2f;
            PositionActiveDot();
        }

        private static float SnapToGrid(float t)
        {
            const float step = 1f / 8f;
            return Mathf.Round(t / step) * step;
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

            PositionSliderKnob(_biasHSliderKnob, _biasHSlider, (_handBias   + 1f) * 0.5f);
            PositionSliderKnob(_biasVSliderKnob, _biasVSlider, (_heightBias + 1f) * 0.5f);
        }

        private void PositionSliderKnob(VisualElement knob, VisualElement track, float t)
        {
            var w = track.resolvedStyle.width;
            if (w <= 0f) return;
            const float knobRadius = 11f;
            knob.style.left       = t * w - knobRadius;
            knob.style.marginLeft = 0;
        }

        private void OnHSliderPointerDown(PointerDownEvent e)
        {
            _isDraggingHSlider = true;
            _biasHSlider.CapturePointer(e.pointerId);
            UpdateHSliderFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnHSliderPointerMove(PointerMoveEvent e)
        {
            if (!_isDraggingHSlider) return;
            UpdateHSliderFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnHSliderPointerUp(PointerUpEvent e)
        {
            if (!_isDraggingHSlider) return;
            _isDraggingHSlider = false;
            _biasHSlider.ReleasePointer(e.pointerId);
        }

        private void OnVSliderPointerDown(PointerDownEvent e)
        {
            _isDraggingVSlider = true;
            _biasVSlider.CapturePointer(e.pointerId);
            UpdateVSliderFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnVSliderPointerMove(PointerMoveEvent e)
        {
            if (!_isDraggingVSlider) return;
            UpdateVSliderFromLocal(e.localPosition);
            e.StopPropagation();
        }

        private void OnVSliderPointerUp(PointerUpEvent e)
        {
            if (!_isDraggingVSlider) return;
            _isDraggingVSlider = false;
            _biasVSlider.ReleasePointer(e.pointerId);
        }

        private void UpdateHSliderFromLocal(Vector2 localPos)
        {
            var w = _biasHSlider.resolvedStyle.width;
            _handBias = SnapToGrid(Mathf.Clamp01(localPos.x / w)) * 2f - 1f;
            PositionActiveDot();
        }

        private void UpdateVSliderFromLocal(Vector2 localPos)
        {
            var w = _biasVSlider.resolvedStyle.width;
            _heightBias = SnapToGrid(Mathf.Clamp01(localPos.x / w)) * 2f - 1f;
            PositionActiveDot();
        }

        #endregion
    }
}