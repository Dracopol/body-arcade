using UnityEngine;

namespace IMVR.BodyArcade.Neuromania
{
    public class GloveHitVFX : MonoBehaviour
    {
        [SerializeField] private ImpactVFXController _impactVfx;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Target")) return;

            var contact = other.ClosestPoint(transform.position);
            var normal = (transform.position - other.transform.position).normalized;
            _impactVfx.PlayImpact(contact, normal);
        }
    }
}