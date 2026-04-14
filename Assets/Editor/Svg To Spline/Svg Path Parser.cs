using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Editor.SvgToSpline
{
    internal static class SvgPathParser
    {
        #region Internal

        // Returns a list of sub-paths; each sub-path is a list of BezierKnots.
        // Knot positions/tangents are in raw SVG coordinates (no flip/scale yet).
        internal static List<List<BezierKnot>> ParsePathData(string d)
        {
            var result  = new List<List<BezierKnot>>();
            var current = new List<BezierKnot>();
            var tokens  = Tokenize(d);
            int idx     = 0;

            float2 cursor   = float2.zero;
            float2 start    = float2.zero;
            float2 lastCtrl = float2.zero;
            char   lastCmd  = '\0';

            while (idx < tokens.Count)
            {
                string tok = tokens[idx];

                if (tok.Length == 1 && char.IsLetter(tok[0]))
                {
                    lastCmd = tok[0];
                    idx++;
                }

                // ── M / m ─────────────────────────────────────────────────
                if (lastCmd == 'M' || lastCmd == 'm')
                {
                    if (current.Count > 0)
                    {
                        result.Add(current);
                        current = new List<BezierKnot>();
                    }

                    float2 p = ReadPair(tokens, ref idx);
                    cursor = lastCmd == 'M' ? p : cursor + p;
                    start  = cursor;
                    current.Add(MakeKnot(cursor, float2.zero, float2.zero));
                    lastCmd = lastCmd == 'M' ? 'L' : 'l';
                    continue;
                }

                // ── Z / z ─────────────────────────────────────────────────
                if (lastCmd == 'Z' || lastCmd == 'z')
                {
                    if (current.Count > 0)
                    {
                        current.Add(MakeKnot(start, float2.zero, float2.zero));
                        result.Add(current);
                        current = new List<BezierKnot>();
                    }
                    cursor  = start;
                    lastCmd = '\0';
                    continue;
                }

                // ── L / l ─────────────────────────────────────────────────
                if (lastCmd == 'L' || lastCmd == 'l')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float2 p  = ReadPair(tokens, ref idx);
                    float2 to = lastCmd == 'L' ? p : cursor + p;
                    current.Add(MakeKnot(to, float2.zero, float2.zero));
                    cursor = to;
                    continue;
                }

                // ── H / h ─────────────────────────────────────────────────
                if (lastCmd == 'H' || lastCmd == 'h')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float  v  = ReadNum(tokens, ref idx);
                    float2 to = lastCmd == 'H'
                        ? new float2(v, cursor.y)
                        : new float2(cursor.x + v, cursor.y);
                    current.Add(MakeKnot(to, float2.zero, float2.zero));
                    cursor = to;
                    continue;
                }

                // ── V / v ─────────────────────────────────────────────────
                if (lastCmd == 'V' || lastCmd == 'v')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float  v  = ReadNum(tokens, ref idx);
                    float2 to = lastCmd == 'V'
                        ? new float2(cursor.x, v)
                        : new float2(cursor.x, cursor.y + v);
                    current.Add(MakeKnot(to, float2.zero, float2.zero));
                    cursor = to;
                    continue;
                }

                // ── C / c ─────────────────────────────────────────────────
                if (lastCmd == 'C' || lastCmd == 'c')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float2 cp1 = ReadPair(tokens, ref idx);
                    float2 cp2 = ReadPair(tokens, ref idx);
                    float2 ep  = ReadPair(tokens, ref idx);

                    if (lastCmd == 'c') { cp1 += cursor; cp2 += cursor; ep += cursor; }

                    SetTangentOut(current, cp1 - cursor);
                    current.Add(MakeKnot(ep, cp2 - ep, float2.zero));

                    lastCtrl = cp2;
                    cursor   = ep;
                    continue;
                }

                // ── S / s ─────────────────────────────────────────────────
                if (lastCmd == 'S' || lastCmd == 's')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float2 cp2 = ReadPair(tokens, ref idx);
                    float2 ep  = ReadPair(tokens, ref idx);

                    if (lastCmd == 's') { cp2 += cursor; ep += cursor; }

                    float2 cp1 = IsSmooth(lastCmd) ? 2f * cursor - lastCtrl : cursor;

                    SetTangentOut(current, cp1 - cursor);
                    current.Add(MakeKnot(ep, cp2 - ep, float2.zero));

                    lastCtrl = cp2;
                    cursor   = ep;
                    continue;
                }

                // ── Q / q ─────────────────────────────────────────────────
                if (lastCmd == 'Q' || lastCmd == 'q')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float2 qp1 = ReadPair(tokens, ref idx);
                    float2 ep  = ReadPair(tokens, ref idx);

                    if (lastCmd == 'q') { qp1 += cursor; ep += cursor; }

                    float2 cp1 = cursor + (2f / 3f) * (qp1 - cursor);
                    float2 cp2 = ep     + (2f / 3f) * (qp1 - ep);

                    SetTangentOut(current, cp1 - cursor);
                    current.Add(MakeKnot(ep, cp2 - ep, float2.zero));

                    lastCtrl = qp1;
                    cursor   = ep;
                    continue;
                }

                // ── T / t ─────────────────────────────────────────────────
                if (lastCmd == 'T' || lastCmd == 't')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float2 ep = ReadPair(tokens, ref idx);
                    if (lastCmd == 't')
                        ep += cursor;

                    float2 qp1 = 2f * cursor - lastCtrl;
                    float2 cp1 = cursor + (2f / 3f) * (qp1 - cursor);
                    float2 cp2 = ep     + (2f / 3f) * (qp1 - ep);

                    SetTangentOut(current, cp1 - cursor);
                    current.Add(MakeKnot(ep, cp2 - ep, float2.zero));

                    lastCtrl = qp1;
                    cursor   = ep;
                    continue;
                }

                // ── A / a ─────────────────────────────────────────────────
                if (lastCmd == 'A' || lastCmd == 'a')
                {
                    if (!PeekIsNumber(tokens, idx)) { lastCmd = '\0'; continue; }
                    float  rx       = ReadNum(tokens, ref idx);
                    float  ry       = ReadNum(tokens, ref idx);
                    float  xRot     = ReadNum(tokens, ref idx);
                    int    largeArc = (int)ReadNum(tokens, ref idx);
                    int    sweep    = (int)ReadNum(tokens, ref idx);
                    float2 ep       = ReadPair(tokens, ref idx);

                    if (lastCmd == 'a')
                        ep += cursor;

                    List<BezierKnot> arcKnots = ArcToCubics(cursor, ep, rx, ry, xRot, largeArc, sweep);
                    if (arcKnots.Count >= 2)
                    {
                        SetTangentOut(current, arcKnots[0].TangentOut.xy - float2.zero);
                        for (int i = 1; i < arcKnots.Count; i++)
                            current.Add(arcKnots[i]);
                    }

                    cursor = ep;
                    continue;
                }

                idx++;
            }

            if (current.Count > 0)
                result.Add(current);

            return result;
        }

        #endregion

        #region Private

        private static List<BezierKnot> ArcToCubics(
            float2 p1, float2 p2,
            float rx, float ry, float xRotDeg,
            int largeArc, int sweep)
        {
            var result = new List<BezierKnot>();

            if (math.distance(p1, p2) < 0.0001f || rx < 0.0001f || ry < 0.0001f)
            {
                result.Add(MakeKnot(p1, float2.zero, float2.zero));
                result.Add(MakeKnot(p2, float2.zero, float2.zero));
                return result;
            }

            float phi    = xRotDeg * Mathf.Deg2Rad;
            float cosPhi = Mathf.Cos(phi);
            float sinPhi = Mathf.Sin(phi);

            float2 mid = (p1 - p2) * 0.5f;
            float x1p  =  cosPhi * mid.x + sinPhi * mid.y;
            float y1p  = -sinPhi * mid.x + cosPhi * mid.y;

            float q = (x1p * x1p) / (rx * rx) + (y1p * y1p) / (ry * ry);
            if (q > 1f) { float sq = Mathf.Sqrt(q); rx *= sq; ry *= sq; }

            float rx2  = rx * rx, ry2 = ry * ry;
            float x1p2 = x1p * x1p, y1p2 = y1p * y1p;

            float num   = rx2 * ry2 - rx2 * y1p2 - ry2 * x1p2;
            float den   = rx2 * y1p2 + ry2 * x1p2;
            float sqVal = Mathf.Sqrt(Mathf.Max(0f, num / den));
            if (largeArc == sweep) sqVal = -sqVal;

            float cxp =  sqVal * rx * y1p / ry;
            float cyp = -sqVal * ry * x1p / rx;

            float2 center = new float2(
                cosPhi * cxp - sinPhi * cyp + (p1.x + p2.x) * 0.5f,
                sinPhi * cxp + cosPhi * cyp + (p1.y + p2.y) * 0.5f);

            float2 u = new float2( (x1p - cxp) / rx,  (y1p - cyp) / ry);
            float2 v = new float2((-x1p - cxp) / rx, (-y1p - cyp) / ry);

            float theta1 = AngleBetween(new float2(1, 0), u);
            float dTheta = AngleBetween(u, v);

            if (sweep == 0 && dTheta > 0) dTheta -= 2f * Mathf.PI;
            if (sweep == 1 && dTheta < 0) dTheta += 2f * Mathf.PI;

            int   segments = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(dTheta) / (Mathf.PI * 0.5f)));
            float dt       = dTheta / segments;
            float alpha    = Mathf.Sin(dt) * (Mathf.Sqrt(4f + 3f * Mathf.Tan(dt * 0.5f) * Mathf.Tan(dt * 0.5f)) - 1f) / 3f;

            float  t       = theta1;
            float2 prevPt  = PointOnEllipse(t, cosPhi, sinPhi, rx, ry, center);
            float2 prevDer = DerivativeOnEllipse(t, cosPhi, sinPhi, rx, ry);

            result.Add(MakeKnot(prevPt, float2.zero, alpha * prevDer));

            for (int i = 0; i < segments; i++)
            {
                t += dt;
                float2 nextPt  = PointOnEllipse(t, cosPhi, sinPhi, rx, ry, center);
                float2 nextDer = DerivativeOnEllipse(t, cosPhi, sinPhi, rx, ry);

                result.Add(MakeKnot(nextPt, -alpha * prevDer, alpha * nextDer));

                prevPt  = nextPt;
                prevDer = nextDer;
            }

            return result;
        }

        private static float2 PointOnEllipse(float angle, float cosPhi, float sinPhi, float rx, float ry, float2 center) =>
            new float2(
                cosPhi * rx * Mathf.Cos(angle) - sinPhi * ry * Mathf.Sin(angle) + center.x,
                sinPhi * rx * Mathf.Cos(angle) + cosPhi * ry * Mathf.Sin(angle) + center.y);

        private static float2 DerivativeOnEllipse(float angle, float cosPhi, float sinPhi, float rx, float ry) =>
            new float2(
                -cosPhi * rx * Mathf.Sin(angle) - sinPhi * ry * Mathf.Cos(angle),
                -sinPhi * rx * Mathf.Sin(angle) + cosPhi * ry * Mathf.Cos(angle));

        private static float AngleBetween(float2 u, float2 v)
        {
            float dot   = math.clamp(math.dot(math.normalize(u), math.normalize(v)), -1f, 1f);
            float angle = Mathf.Acos(dot);
            if (u.x * v.y - u.y * v.x < 0f)
                angle = -angle;
            return angle;
        }

        private static List<string> Tokenize(string d)
        {
            var tokens = new List<string>();
            var regex  = new Regex(@"[MmZzLlHhVvCcSsQqTtAa]|[-+]?(?:\d+\.?\d*|\.\d+)(?:[eE][-+]?\d+)?");
            foreach (Match m in regex.Matches(d))
                tokens.Add(m.Value);
            return tokens;
        }

        private static float ReadNum(List<string> tokens, ref int i) =>
            float.Parse(tokens[i++], CultureInfo.InvariantCulture);

        private static float2 ReadPair(List<string> tokens, ref int i) =>
            new float2(ReadNum(tokens, ref i), ReadNum(tokens, ref i));

        private static bool PeekIsNumber(List<string> tokens, int i)
        {
            if (i >= tokens.Count)
                return false;
            char c = tokens[i][0];
            return tokens[i].Length > 0 && (char.IsDigit(c) || c == '-' || c == '+' || c == '.');
        }

        private static BezierKnot MakeKnot(float2 pos, float2 tangentIn, float2 tangentOut) =>
            new BezierKnot(
                new float3(pos.x, pos.y, 0f),
                new float3(tangentIn.x, tangentIn.y, 0f),
                new float3(tangentOut.x, tangentOut.y, 0f),
                quaternion.identity);

        private static void SetTangentOut(List<BezierKnot> knots, float2 tangentOut)
        {
            if (knots.Count == 0)
                return;
            int i    = knots.Count - 1;
            var k    = knots[i];
            knots[i] = new BezierKnot(k.Position, k.TangentIn, new float3(tangentOut.x, tangentOut.y, 0f), k.Rotation);
        }

        private static bool IsSmooth(char cmd) =>
            cmd == 'S' || cmd == 's' || cmd == 'T' || cmd == 't';

        #endregion
    }
}