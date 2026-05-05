using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Neuromania.Electron
{
    internal static class ElectronSplineAnimator
    {
        internal static async UniTask AnimateAsync(
            Transform target,
            SplineContainer spline,
            Camera camera,
            float xOffset,
            float yOffset,
            float segment0Duration,
            float segment1Duration,
            CancellationToken ct)
        {
            float totalLength = spline.CalculateLength();
            float curve0Length = spline.Spline.GetCurveLength(0);
            float tMid = curve0Length / totalLength;

            await DriveSegmentAsync(target, spline, camera, xOffset, yOffset, 0f, tMid, segment0Duration, ct);

            if (!ct.IsCancellationRequested)
                await DriveSegmentAsync(target, spline, camera, xOffset, yOffset, tMid, 1f, segment1Duration, ct);
        }

        private static async UniTask DriveSegmentAsync(
            Transform target,
            SplineContainer spline,
            Camera camera,
            float xOffset,
            float yOffset,
            float tStart,
            float tEnd,
            float duration,
            CancellationToken ct)
        {
            float elapsed = 0f;

            while (elapsed < duration && !ct.IsCancellationRequested)
            {
                float t = Mathf.Lerp(tStart, tEnd, elapsed / duration);
                ApplyTransform(target, spline, camera, xOffset, yOffset, t);

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (!ct.IsCancellationRequested)
                ApplyTransform(target, spline, camera, xOffset, yOffset, tEnd);
        }

        private static void ApplyTransform(
            Transform target,
            SplineContainer spline,
            Camera camera,
            float xOffset,
            float yOffset,
            float t)
        {
            spline.Evaluate(t, out float3 position, out _, out _);
            target.position = (Vector3)position + new Vector3(xOffset, yOffset, 0f);

            Vector3 dir = camera.transform.position - target.position;
            Vector3 euler = Quaternion.LookRotation(dir).eulerAngles;
            euler.z = 90f;
            target.rotation = Quaternion.Euler(euler);
        }
    }
}