using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.ProgressTrackering
{
    internal class SplineProximity
    {
        #region Fields

        private readonly Spline _spline;
        private readonly Transform _splineTransform;
        private readonly Collider _handCollider;

        private const int CoarseStepCount = 24;
        private const int RefineStepCount = 12;

        #endregion

        #region Setup

        internal SplineProximity(Spline spline, Transform splineTransform, Collider handCollider)
        {
            _spline = spline;
            _splineTransform = splineTransform;
            _handCollider = handCollider;
        }

        #endregion

        #region Internal

        internal float FindClosestSplineProgress(
            out Vector3 closestSplinePoint,
            out Vector3 closestColliderPoint)
        {
            float coarseStepSize = 1f / CoarseStepCount;

            float[] distances = new float[CoarseStepCount + 1];
            for (int i = 0; i <= CoarseStepCount; i++)
                distances[i] = GetDistanceSquaredToCollider(i * coarseStepSize);

            float bestProgress = 0f;
            float smallestDistance = float.MaxValue;

            for (int i = 0; i <= CoarseStepCount; i++)
            {
                float prev = distances[i > 0 ? i - 1 : 0];
                float next = distances[i < CoarseStepCount ? i + 1 : CoarseStepCount];

                if (distances[i] > prev || distances[i] > next)
                    continue;

                float left = Mathf.Clamp01(i * coarseStepSize - coarseStepSize);
                float right = Mathf.Clamp01(i * coarseStepSize + coarseStepSize);

                for (int iter = 0; iter < RefineStepCount; iter++)
                {
                    float range = right - left;
                    float progressA = left + range / 3f;
                    float progressB = right - range / 3f;

                    if (GetDistanceSquaredToCollider(progressA) < GetDistanceSquaredToCollider(progressB))
                        right = progressB;
                    else
                        left = progressA;
                }

                float candidate = (left + right) * 0.5f;
                float candidateDistance = GetDistanceSquaredToCollider(candidate);

                if (candidateDistance < smallestDistance)
                {
                    smallestDistance = candidateDistance;
                    bestProgress = candidate;
                }
            }

            float3 localPoint = SplineUtility.EvaluatePosition(_spline, bestProgress);
            closestSplinePoint = _splineTransform.TransformPoint(localPoint);
            closestColliderPoint = _handCollider.ClosestPoint(closestSplinePoint);

            return bestProgress;
        }

        #endregion

        #region Private

        private float GetDistanceSquaredToCollider(float progress)
        {
            float3 localPoint = SplineUtility.EvaluatePosition(_spline, progress);
            Vector3 worldPoint = _splineTransform.TransformPoint(localPoint);

            return (worldPoint - _handCollider.bounds.center).sqrMagnitude;
        }

        #endregion
    }
}