using UnityEngine;

namespace IMVR.BodyArcade
{
    [CreateAssetMenu(fileName = "ShapeConfig", menuName = "IMVR/Body Arcade/Shape Config")]
    internal class ShapeConfig : ScriptableObject
    {
        [SerializeField] internal string ShapeName;
        [SerializeField] internal string DifficultyLabel;
        [SerializeField] internal Color ThumbnailColor = new Color(0.5f, 0.65f, 0.85f);
        [SerializeField] internal Sprite ThumbnailSprite;
        [SerializeField] internal GameObject Prefab;
    }
}