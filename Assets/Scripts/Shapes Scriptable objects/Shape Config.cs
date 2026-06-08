using UnityEngine;

namespace IMVR.BodyArcade
{
    [CreateAssetMenu(fileName = "ShapeConfig", menuName = "IMVR/Body Arcade/Shape Config")]
    internal class ShapeConfig : ScriptableObject
    {
        [SerializeField] internal bool IsEasy;
        [SerializeField] internal bool IsMedium;
        [SerializeField] internal bool IsHard;
        [SerializeField] internal GameObject Prefab;
    }
}