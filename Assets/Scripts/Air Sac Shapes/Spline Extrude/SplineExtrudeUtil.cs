using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.AirSacShapes.SplineExtrude
{
    /// <summary>
    /// Generates an extruded mesh by sweeping a 2D profile along a planar (XY) spline.
    /// Uses a constant world-up frame, which is exact for planar splines, so there is no twisting.
    /// Sharp corners are mitered so they stay crisp instead of pinching.
    /// </summary>
    internal static class SplineExtrudeUtil
    {
        #region Entities

        internal enum Profile
        {
            Circle,
            RoundedRectangle,
            Custom
        }

        internal struct Settings
        {
            internal Profile      Profile;
            internal Vector2[]    CustomProfile;
            internal float        Radius;
            internal int          CircleSides;
            internal float        Width;
            internal float        Height;
            internal float        CornerRadius;
            internal int          CornerSegments;
            internal float        SamplesPerUnit;
            internal float        SharpCornerAngleDegrees;
            internal bool         CapEnds;
            internal bool         FlipNormals;
        }

        private struct Frame
        {
            internal float3 Position;
            internal float3 Right;
            internal float3 Up;
            internal float  DistanceAlongPath;
        }

        #endregion

        #region Constants

        // World up. Correct for any XY-planar spline; rings never roll around the path.
        private static readonly float3 PlaneNormal = new(0f, 0f, 1f);

        private const float MinSamplesPerSpline = 8f;

        #endregion

        #region Internal

        internal static Settings DefaultSettings()
        {
            return new Settings
            {
                Profile                 = Profile.Circle,
                CustomProfile           = null,
                Radius                  = 0.1f,
                CircleSides             = 12,
                Width                   = 0.2f,
                Height                  = 0.1f,
                CornerRadius            = 0.03f,
                CornerSegments          = 3,
                SamplesPerUnit          = 20f,
                SharpCornerAngleDegrees = 35f,
                CapEnds                 = true,
                FlipNormals             = false
            };
        }

        /// <summary>
        /// Builds (or rebuilds) the extruded mesh from the spline in local space of the container.
        /// </summary>
        internal static void Generate(Spline spline, Settings settings, Mesh target)
        {
            if (spline == null || spline.Count < 2 || target == null)
                return;

            Vector2[] profile = BuildProfile(settings);
            if (profile.Length < 2)
                return;

            List<Frame> frames = SampleFrames(spline, settings);
            if (frames.Count < 2)
                return;

            BuildMesh(frames, profile, spline.Closed, settings, target);
        }

        #endregion

        #region Private

        // Builds the 2D cross-section, ordered counter-clockwise, centered on the path.
        private static Vector2[] BuildProfile(Settings settings)
        {
            switch (settings.Profile)
            {
                case Profile.Circle:
                    return BuildCircleProfile(settings.Radius, Mathf.Max(3, settings.CircleSides));

                case Profile.RoundedRectangle:
                    return BuildRoundedRectProfile(settings.Width, settings.Height,
                        settings.CornerRadius, Mathf.Max(1, settings.CornerSegments));

                case Profile.Custom:
                    return settings.CustomProfile ?? Array.Empty<Vector2>();

                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.Profile));
            }
        }

        private static Vector2[] BuildCircleProfile(float radius, int sides)
        {
            var points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float angle = (float)i / sides * Mathf.PI * 2f;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            return points;
        }

        private static Vector2[] BuildRoundedRectProfile(float width, float height, float cornerRadius, int cornerSegments)
        {
            float halfWidth  = width  * 0.5f;
            float halfHeight = height * 0.5f;
            float radius     = Mathf.Min(cornerRadius, Mathf.Min(halfWidth, halfHeight));

            var points = new List<Vector2>();

            // Four corners, counter-clockwise starting bottom-right.
            float2[] centers =
            {
                new( halfWidth - radius, -(halfHeight - radius)),
                new( halfWidth - radius,   halfHeight - radius),
                new(-(halfWidth - radius), halfHeight - radius),
                new(-(halfWidth - radius), -(halfHeight - radius))
            };

            for (int corner = 0; corner < 4; corner++)
            {
                float startAngle = -Mathf.PI * 0.5f + corner * Mathf.PI * 0.5f;
                for (int segment = 0; segment <= cornerSegments; segment++)
                {
                    float angle = startAngle + (float)segment / cornerSegments * Mathf.PI * 0.5f;
                    float2 center = centers[corner];
                    points.Add(new Vector2(center.x + Mathf.Cos(angle) * radius,
                                           center.y + Mathf.Sin(angle) * radius));
                }
            }

            return points.ToArray();
        }

        // Samples the spline adaptively, then inserts mitered double-frames at sharp corners.
        private static List<Frame> SampleFrames(Spline spline, Settings settings)
        {
            float length      = spline.GetLength();
            int   sampleCount  = Mathf.Max((int)MinSamplesPerSpline,
                Mathf.CeilToInt(length * settings.SamplesPerUnit));

            var raw = new List<Frame>(sampleCount + 1);

            int endExclusive = spline.Closed ? sampleCount : sampleCount + 1;
            float distanceStep = length / sampleCount;

            for (int i = 0; i < endExclusive; i++)
            {
                float t = (float)i / sampleCount;
                SplineUtility.Evaluate(spline, t, out float3 position, out float3 tangent, out _);

                if (math.lengthsq(tangent) < math.EPSILON)
                    tangent = EstimateTangent(spline, t);

                raw.Add(BuildFrame(position, tangent, i * distanceStep));
            }

            return InsertCornerFrames(raw, spline.Closed, settings);
        }

        private static Frame BuildFrame(float3 position, float3 tangent, float distanceAlongPath)
        {
            tangent = math.normalize(tangent);
            float3 right = math.cross(PlaneNormal, tangent);

            // Degenerate only if the tangent leaves the XY plane, which planar splines never do.
            if (math.lengthsq(right) < math.EPSILON)
                right = new float3(1f, 0f, 0f);

            return new Frame
            {
                Position          = position,
                Right             = math.normalize(right),
                Up                = PlaneNormal,
                DistanceAlongPath = distanceAlongPath
            };
        }

        private static float3 EstimateTangent(Spline spline, float t)
        {
            float delta = 0.001f;
            float3 ahead  = SplineUtility.EvaluatePosition(spline, math.min(1f, t + delta));
            float3 behind = SplineUtility.EvaluatePosition(spline, math.max(0f, t - delta));
            return ahead - behind;
        }

        // Detects vertices where the path turns sharply and duplicates the frame there with an
        // averaged (mitered) right vector, so the swept profile keeps a crisp corner.
        private static List<Frame> InsertCornerFrames(List<Frame> frames, bool closed, Settings settings)
        {
            if (frames.Count < 3)
                return frames;

            float cosThreshold = Mathf.Cos(settings.SharpCornerAngleDegrees * Mathf.Deg2Rad);
            var result = new List<Frame>(frames.Count + 8);

            for (int i = 0; i < frames.Count; i++)
            {
                Frame current = frames[i];

                bool hasPrevious = closed || i > 0;
                bool hasNext     = closed || i < frames.Count - 1;
                if (!hasPrevious || !hasNext)
                {
                    result.Add(current);
                    continue;
                }

                int previousIndex = (i - 1 + frames.Count) % frames.Count;
                int nextIndex     = (i + 1) % frames.Count;

                float3 incoming = math.normalize(current.Position - frames[previousIndex].Position);
                float3 outgoing = math.normalize(frames[nextIndex].Position - current.Position);

                if (math.dot(incoming, outgoing) >= cosThreshold)
                {
                    result.Add(current);
                    continue;
                }

                // Sharp corner: emit two rings at the corner position, one framed by the incoming
                // tangent and one by the outgoing tangent. The profile sweeps cleanly into the corner
                // and back out like a real mitered pipe joint, instead of one stretched ring.
                Frame incomingFrame = current;
                incomingFrame.Right = math.normalize(math.cross(PlaneNormal, incoming));

                Frame outgoingFrame = current;
                outgoingFrame.Right = math.normalize(math.cross(PlaneNormal, outgoing));

                result.Add(incomingFrame);
                result.Add(outgoingFrame);
            }

            return result;
        }

        private static void BuildMesh(List<Frame> frames, Vector2[] profile, bool closed, Settings settings, Mesh target)
        {
            int ringCount    = frames.Count;
            int profileCount = profile.Length;

            int ringStride   = profileCount + 1; // duplicate seam vertex for clean UVs
            int vertexCount  = ringCount * ringStride;

            var vertices = new Vector3[vertexCount];
            var normals  = new Vector3[vertexCount];
            var uvs       = new Vector2[vertexCount];

            float totalLength = frames[ringCount - 1].DistanceAlongPath;
            if (totalLength <= math.EPSILON)
                totalLength = 1f;

            for (int ring = 0; ring < ringCount; ring++)
            {
                Frame frame = frames[ring];
                float v     = frame.DistanceAlongPath / totalLength;

                for (int p = 0; p <= profileCount; p++)
                {
                    int    profileIndex = p % profileCount;
                    int    vertexIndex  = ring * ringStride + p;
                    Vector2 point       = profile[profileIndex];

                    float3 offset = frame.Right * point.x + frame.Up * point.y;
                    float3 world  = frame.Position + offset;

                    vertices[vertexIndex] = world;
                    normals[vertexIndex]  = math.normalize(offset);
                    uvs[vertexIndex]      = new Vector2((float)p / profileCount, v);
                }
            }

            var triangles = new List<int>(ringCount * profileCount * 6);
            int ringPairs  = closed ? ringCount : ringCount - 1;

            for (int ring = 0; ring < ringPairs; ring++)
            {
                int currentBase = ring * ringStride;
                int nextBase     = ((ring + 1) % ringCount) * ringStride;

                for (int p = 0; p < profileCount; p++)
                {
                    int a = currentBase + p;
                    int b = currentBase + p + 1;
                    int c = nextBase + p;
                    int d = nextBase + p + 1;

                    AddQuad(triangles, a, b, c, d, settings.FlipNormals);
                }
            }

            if (settings.CapEnds && !closed)
                AddCaps(frames, profile, vertices, normals, uvs, triangles, ringStride, settings.FlipNormals,
                    out vertices, out normals, out uvs);

            target.Clear();
            target.indexFormat = vertexCount > 65000
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16;
            target.vertices  = vertices;
            target.normals   = normals;
            target.uv        = uvs;
            target.triangles = triangles.ToArray();
            target.RecalculateBounds();
        }

        private static void AddQuad(List<int> triangles, int a, int b, int c, int d, bool flip)
        {
            if (flip)
            {
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
            else
            {
                triangles.Add(a); triangles.Add(b); triangles.Add(c);
                triangles.Add(b); triangles.Add(d); triangles.Add(c);
            }
        }

        // Fans a flat cap over the first and last rings of an open extrusion.
        private static void AddCaps(List<Frame> frames, Vector2[] profile, Vector3[] vertices, Vector3[] normals,
            Vector2[] uvs, List<int> triangles, int ringStride, bool flip,
            out Vector3[] outVertices, out Vector3[] outNormals, out Vector2[] outUvs)
        {
            int profileCount = profile.Length;
            var verts = new List<Vector3>(vertices);
            var norms = new List<Vector3>(normals);
            var uv    = new List<Vector2>(uvs);

            AddCap(frames[0], profile, verts, norms, uv, triangles, -1f, !flip);
            AddCap(frames[frames.Count - 1], profile, verts, norms, uv, triangles, 1f, flip);

            outVertices = verts.ToArray();
            outNormals  = norms.ToArray();
            outUvs      = uv.ToArray();

            void AddCap(Frame frame, Vector2[] profilePoints, List<Vector3> vertexList, List<Vector3> normalList,
                List<Vector2> uvList, List<int> triangleList, float normalSign, bool reverseWinding)
            {
                float3 capNormal = math.normalize(math.cross(frame.Right, frame.Up)) * normalSign;
                int centerIndex  = vertexList.Count;

                vertexList.Add(frame.Position);
                normalList.Add(capNormal);
                uvList.Add(new Vector2(0.5f, 0.5f));

                int rimStart = vertexList.Count;
                for (int p = 0; p < profilePoints.Length; p++)
                {
                    Vector2 point = profilePoints[p];
                    float3 offset = frame.Right * point.x + frame.Up * point.y;
                    vertexList.Add(frame.Position + offset);
                    normalList.Add(capNormal);
                    uvList.Add(new Vector2(point.x, point.y) + new Vector2(0.5f, 0.5f));
                }

                for (int p = 0; p < profilePoints.Length; p++)
                {
                    int rim     = rimStart + p;
                    int rimNext = rimStart + (p + 1) % profilePoints.Length;

                    if (reverseWinding)
                    {
                        triangleList.Add(centerIndex); triangleList.Add(rimNext); triangleList.Add(rim);
                    }
                    else
                    {
                        triangleList.Add(centerIndex); triangleList.Add(rim); triangleList.Add(rimNext);
                    }
                }
            }
        }

        #endregion
    }
}
