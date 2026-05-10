using System.Linq;
using UnityEngine;

namespace IMVR.BodyArcade.UI
{
    internal class SessionConfigDebugger : MonoBehaviour
    {
        private void Start()
        {
            if (SessionConfig.Instance == null)
            {
                Debug.LogWarning("[SessionConfig] No SessionConfig found — likely started from gameplay scene directly.");
                return;
            }

            var shapes = SessionConfig.Instance.SelectedShapes;
            var shapeNames = shapes != null && shapes.Count > 0
                ? string.Join(", ", shapes.Select(s => s != null ? s.name : "null"))
                : "none";

            Debug.Log($"[SessionConfig] MirrorMode: {SessionConfig.Instance.MirrorMode}");
            Debug.Log($"[SessionConfig] SelectedShapes ({shapes?.Count ?? 0}): {shapeNames}");
        }
    }
}