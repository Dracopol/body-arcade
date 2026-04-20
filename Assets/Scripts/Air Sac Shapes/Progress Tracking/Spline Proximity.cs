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

        private const int CoarseStepCount = 12;
        private const int RefineStepCount = 6;

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
            float bestProgress = 0f;
            float smallestDistanceSquared = float.MaxValue;

            float coarseStepSize = 1f / CoarseStepCount;

            for (int stepIndex = 0; stepIndex <= CoarseStepCount; stepIndex++)
            {
                float progress = stepIndex * coarseStepSize;
                float distanceSquared = GetDistanceSquaredToCollider(progress);

                if (distanceSquared < smallestDistanceSquared)
                {
                    smallestDistanceSquared = distanceSquared;
                    bestProgress = progress;
                }
            }

            float searchRange = coarseStepSize;

            for (int iterationIndex = 0; iterationIndex < RefineStepCount; iterationIndex++)
            {
                float leftProgress = Mathf.Clamp01(bestProgress - searchRange);
                float rightProgress = Mathf.Clamp01(bestProgress + searchRange);

                float progressA = Mathf.Lerp(leftProgress, rightProgress, 0.33f);
                float progressB = Mathf.Lerp(leftProgress, rightProgress, 0.66f);

                float distanceA = GetDistanceSquaredToCollider(progressA);
                float distanceB = GetDistanceSquaredToCollider(progressB);

                bestProgress = distanceA < distanceB ? progressA : progressB;

                searchRange *= 0.5f;
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
            Vector3 closestPointOnCollider = _handCollider.ClosestPoint(worldPoint);

            return (worldPoint - closestPointOnCollider).sqrMagnitude;
        }

        #endregion
    }
}