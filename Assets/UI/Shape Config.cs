using UnityEngine;

namespace IMVR.BodyArcade
{
    [CreateAssetMenu(fileName = "ShapeConfig", menuName = "IMVR/Body Arcade/Shape Config")]
    internal class ShapeConfig : ScriptableObject
    {
        internal string ShapeName => name;
        [SerializeField] internal string DifficultyLabel;
        internal Color ThumbnailColor;
        [SerializeField] internal Sprite ThumbnailSprite;
        [SerializeField] internal GameObject Prefab;

        private void OnEnable()
        {
            if (ThumbnailColor == Color.white)
                ThumbnailColor = Random.ColorHSV();
        }
    }
}