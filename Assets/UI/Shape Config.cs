using UnityEngine;

namespace IMVR.BodyArcade
{
    [CreateAssetMenu(fileName = "ShapeConfig", menuName = "IMVR/Body Arcade/Shape Config")]
    internal class ShapeConfig : ScriptableObject
    {
        internal string ShapeName => name;

        [SerializeField] internal string DifficultyLabel = "Easy";
        [SerializeField] internal Color ThumbnailColor;
        [SerializeField] internal Sprite ThumbnailSprite;
        [SerializeField] internal GameObject Prefab;

        [SerializeField] private bool _thumbnailColorInitialized;

        private void OnEnable()
        {
            if (_thumbnailColorInitialized) return;

            ThumbnailColor = Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.8f, 1f);
            _thumbnailColorInitialized = true;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}