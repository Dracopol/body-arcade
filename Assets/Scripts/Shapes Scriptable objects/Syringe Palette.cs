using UnityEngine;

namespace IMVR.BodyArcade
{
    [CreateAssetMenu(fileName = "SyringePalette", menuName = "IMVR/Body Arcade/Syringe Palette")]
    internal class SyringePalette : ScriptableObject
    {
        [Header("Left")]
        [SerializeField] private Color _fillLeft  = Color.red;
        [SerializeField] private Color _emptyLeft = new(1f, 0.5f, 0.5f, 1f);

        [Header("Right")]
        [SerializeField] private Color _fillRight  = Color.blue;
        [SerializeField] private Color _emptyRight = new(0.5f, 0.5f, 1f, 1f);

        [Header("Shared")]
        [SerializeField] private Color _fillWin     = Color.green;
        [SerializeField] private Color _transparent = Color.clear;

        internal Color FillLeft    => _fillLeft;
        internal Color EmptyLeft   => _emptyLeft;
        internal Color FillRight   => _fillRight;
        internal Color EmptyRight  => _emptyRight;
        internal Color FillWin     => _fillWin;
        internal Color Transparent => _transparent;
    }
}
