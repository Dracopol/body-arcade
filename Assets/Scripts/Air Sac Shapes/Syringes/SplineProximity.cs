using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.Syringes
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

            if (_handCollider is CapsuleCollider capsule)
                return GetDistanceSquaredToCapsuleAxis(worldPoint, capsule);

            return (worldPoint - _handCollider.bounds.center).sqrMagnitude;
        }

        private static float GetDistanceSquaredToCapsuleAxis(Vector3 worldPoint, CapsuleCollider capsule)
        {
            float halfHeight = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);

            Vector3 localAxis = capsule.direction switch
            {
                0 => Vector3.right,
                1 => Vector3.up,
                2 => Vector3.forward,
                _ => throw new ArgumentOutOfRangeException(nameof(capsule.direction))
            };

            Transform t = capsule.transform;
            Vector3 worldCenter = t.TransformPoint(capsule.center);
            Vector3 worldAxis = t.TransformDirection(localAxis);

            Vector3 point0 = worldCenter - worldAxis * halfHeight;
            Vector3 point1 = worldCenter + worldAxis * halfHeight;

            Vector3 seg = point1 - point0;
            float projected = Mathf.Clamp01(Vector3.Dot(worldPoint - point0, seg) / seg.sqrMagnitude);
            Vector3 closestOnAxis = point0 + projected * seg;

            return (worldPoint - closestOnAxis).sqrMagnitude;
        }

        #endregion
    }
}
