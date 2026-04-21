using UnityEngine;

namespace IMVR.BodyArcade.Neuromania
{
    public class GloveHitVFX : MonoBehaviour
    {
        [SerializeField] private ImpactVFXController _impactVfx;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[GloveHitTest] OnTriggerEnter fired with: {other.gameObject.name} | tag: {other.tag}");

            if (!other.CompareTag("Target"))
            {
                Debug.Log($"[GloveHitTest] Ignored — not tagged Target");
                return;
            }

            if (_impactVfx == null)
            {
                Debug.LogError("[GloveHitTest] _impactVfx is null — assign it in the Inspector");
                return;
            }

            var contact = other.ClosestPoint(transform.position);
            var normal = (transform.position - other.transform.position).normalized;

            Debug.Log($"[GloveHitTest] Hit Target at contact: {contact} | normal: {normal}");

            _impactVfx.PlayImpact(contact, normal);
        }
    }
}