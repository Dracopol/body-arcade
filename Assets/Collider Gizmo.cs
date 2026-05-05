using UnityEngine;

namespace IMVR.BodyArcade
{
    internal class ColliderGizmo : MonoBehaviour
    {
        [SerializeField] private Color _gizmoColor = Color.yellow;
        [SerializeField] private bool _drawOnlyWhenSelected = false;

        private BoxCollider _boxCollider;
        private SphereCollider _sphereCollider;
        private CapsuleCollider _capsuleCollider;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _sphereCollider = GetComponent<SphereCollider>();
            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void OnDrawGizmos()
        {
            if (_drawOnlyWhenSelected) return;
            DrawGizmo();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_drawOnlyWhenSelected) return;
            DrawGizmo();
        }

        private void DrawGizmo()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (_boxCollider != null)
            {
                Gizmos.DrawWireCube(_boxCollider.center, _boxCollider.size);
            }
            else if (_sphereCollider != null)
            {
                Gizmos.DrawWireSphere(_sphereCollider.center, _sphereCollider.radius);
            }
            else if (_capsuleCollider != null)
            {
                DrawWireCapsule(_capsuleCollider);
            }
        }

        private void DrawWireCapsule(CapsuleCollider col)
        {
            float radius = col.radius;
            float height = Mathf.Max(col.height * 0.5f, radius);

            // Capsule axis: 0 = X, 1 = Y, 2 = Z
            Vector3 axisDir = col.direction switch
            {
                0 => Vector3.right,
                2 => Vector3.forward,
                _ => Vector3.up
            };

            Vector3 offset = axisDir * (height - radius);
            Gizmos.DrawWireSphere(col.center + offset, radius);
            Gizmos.DrawWireSphere(col.center - offset, radius);
        }
    }
}