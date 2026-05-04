using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Spawning
{
    internal class SplineDriver : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform _interactable;
        [SerializeField] private float _segment0Duration = 3f;
        [SerializeField] private float _segment1Duration = 3f;

        #endregion

        #region Internal

        internal void Drive(SplineContainer spline, float xOffset, float yOffset, Camera camera, CancellationToken ct)
        {
            DriveAsync(spline, xOffset, yOffset, camera, ct).Forget();
        }

        #endregion

        #region Private

        private async UniTaskVoid DriveAsync(SplineContainer spline, float xOffset, float yOffset, Camera camera, CancellationToken ct)
        {
            Vector3 local = _interactable.localPosition;
            local.x += xOffset;
            local.y = camera.transform.localPosition.y - 1f + yOffset;
            _interactable.localPosition = local;

            float totalLength = spline.CalculateLength();
            float curve0Length = spline.Spline.GetCurveLength(0);
            float tMid = curve0Length / totalLength;

            try
            {
                await DriveSegmentAsync(spline, 0f, tMid, _segment0Duration, camera, ct);
                await DriveSegmentAsync(spline, tMid, 1f, _segment1Duration, camera, ct);
            }
            finally
            {
                Destroy(gameObject);
            }
        }

        private async UniTask DriveSegmentAsync(SplineContainer spline, float tStart, float tEnd, float duration, Camera camera, CancellationToken ct)
        {
            float elapsed = 0f;

            while (elapsed < duration && !ct.IsCancellationRequested)
            {
                float t = Mathf.Lerp(tStart, tEnd, elapsed / duration);
                spline.Evaluate(t, out float3 position, out _, out _);
                transform.position = (Vector3)position;

                Vector3 dir = camera.transform.position - _interactable.position;
                Vector3 euler = Quaternion.LookRotation(dir).eulerAngles;
                euler.z = 90f;
                _interactable.rotation = Quaternion.Euler(euler);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
            {
                spline.Evaluate(tEnd, out float3 position, out _, out _);
                transform.position = (Vector3)position;
            }
        }

        #endregion
    }
}