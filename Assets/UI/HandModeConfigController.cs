using System.Collections.Generic;
using UnityEngine;
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

    internal class HandModeConfigController : MonoBehaviour
    {
        #region Serialized fields

        [SerializeField] private UIDocument _document;

        #endregion

        #region Fields

        private VisualElement _root;
        private List<VisualElement> _handOptionRows;
        private List<VisualElement> _mirrorBtns;
        private VisualElement _bothHandsPanel;

        private HandMode _selectedHandMode = HandMode.BothSimultaneously;
        private MirrorMode _selectedMirrorMode = MirrorMode.Unmirrored;

        #endregion

        #region Events

        internal System.Action<HandMode> OnHandModeChanged;
        internal System.Action<MirrorMode> OnMirrorModeChanged;

        #endregion

        #region Setup

        private void Awake()
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

            _bothHandsPanel = _root.Q<VisualElement>("both-hands-options");

            RegisterHandModeCallbacks();
            RegisterMirrorCallbacks();
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

        private void ApplyInitialState()
        {
            SelectHandMode(_selectedHandMode);
            SelectMirrorMode(_selectedMirrorMode);
        }

        #endregion

        #region Private

        private void SelectHandMode(HandMode mode)
        {
            _selectedHandMode = mode;

            foreach (var row in _handOptionRows)
                row.RemoveFromClassList("selected");

            _handOptionRows[(int)mode].AddToClassList("selected");

            if (mode == HandMode.BothSimultaneously)
                _bothHandsPanel.AddToClassList("visible");
            else
                _bothHandsPanel.RemoveFromClassList("visible");

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

        #endregion
    }
}