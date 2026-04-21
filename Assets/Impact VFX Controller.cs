using UnityEngine;
using UnityEngine.VFX;

namespace IMVR.BodyArcade.Neuromania
{
    public class ImpactVFXController : MonoBehaviour
    {
        [SerializeField] private VisualEffect _vfx;

        private static readonly int HitPositionId = Shader.PropertyToID("HitPosition");
        private static readonly int HitNormalId   = Shader.PropertyToID("HitNormal");

        internal void PlayImpact(Vector3 position, Vector3 normal)
        {
            _vfx.SetVector3(HitPositionId, position);
            _vfx.SetVector3(HitNormalId, normal);
            _vfx.SendEvent("OnHit");
        }
    }
}