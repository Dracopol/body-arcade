using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace IMVR.BodyArcade.Editor.SvgToSpline
{
    // Converts any supported SVG element type into an equivalent path `d` string,
    // so SvgPathParser only ever needs to handle one format.
    //
    // Supported: <path> <rect> <circle> <ellipse> <polygon> <polyline> <line>
    internal static class SvgElementNormalizer
    {
        #region Entities

        private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

        #endregion

        #region Internal

        internal static List<string> ExtractPathData(XDocument doc)
        {
            var result = new List<string>();

            foreach (var el in All(doc, "path"))
            {
                string d = el.Attribute("d")?.Value;
                if (!string.IsNullOrWhiteSpace(d))
                    result.Add(d);
            }

            foreach (var el in All(doc, "rect"))
            {
                string d = RectToPath(el);
                if (d != null) result.Add(d);
            }

            foreach (var el in All(doc, "circle"))
            {
                string d = CircleToPath(el);
                if (d != null) result.Add(d);
            }

            foreach (var el in All(doc, "ellipse"))
            {
                string d = EllipseToPath(el);
                if (d != null) result.Add(d);
            }

            foreach (var el in All(doc, "polygon"))
            {
                string d = PointsToPath(el, closed: true);
                if (d != null) result.Add(d);
            }

            foreach (var el in All(doc, "polyline"))
            {
                string d = PointsToPath(el, closed: false);
                if (d != null) result.Add(d);
            }

            foreach (var el in All(doc, "line"))
            {
                string d = LineToPath(el);
                if (d != null) result.Add(d);
            }

            return result;
        }

        #endregion

        #region Private

        // Yields elements matching localName under both the SVG namespace and no namespace,
        // deduplicating in case the document mixes both styles.
        private static IEnumerable<XElement> All(XDocument doc, string localName) =>
            doc.Descendants(SvgNs + localName)
               .Concat(doc.Descendants(XNamespace.None + localName))
               .Distinct();

        // ── Shape converters ─────────────────────────────────────────────────

        private static string RectToPath(XElement el)
        {
            float x  = Attr(el, "x",      0f);
            float y  = Attr(el, "y",      0f);
            float w  = Attr(el, "width",  0f);
            float h  = Attr(el, "height", 0f);

            if (w <= 0f || h <= 0f)
                return null;

            // SVG spec: if only one of rx/ry is given, they both take that value.
            float rx = Attr(el, "rx", -1f);
            float ry = Attr(el, "ry", -1f);

            if      (rx < 0f && ry < 0f) { rx = 0f; ry = 0f; }
            else if (rx < 0f)              rx = ry;
            else if (ry < 0f)              ry = rx;

            rx = Mathf.Min(rx, w * 0.5f);
            ry = Mathf.Min(ry, h * 0.5f);

            if (rx < 0.0001f)
                return $"M {N(x)},{N(y)} H {N(x+w)} V {N(y+h)} H {N(x)} Z";

            // Rounded rect — four straight edges connected by corner arcs.
            return
                $"M {N(x+rx)},{N(y)} " +
                $"H {N(x+w-rx)} A {N(rx)},{N(ry)} 0 0 1 {N(x+w)},{N(y+ry)} " +
                $"V {N(y+h-ry)} A {N(rx)},{N(ry)} 0 0 1 {N(x+w-rx)},{N(y+h)} " +
                $"H {N(x+rx)} A {N(rx)},{N(ry)} 0 0 1 {N(x)},{N(y+h-ry)} " +
                $"V {N(y+ry)} A {N(rx)},{N(ry)} 0 0 1 {N(x+rx)},{N(y)} Z";
        }

        private static string CircleToPath(XElement el)
        {
            float cx = Attr(el, "cx", 0f);
            float cy = Attr(el, "cy", 0f);
            float r  = Attr(el, "r",  0f);
            return r > 0f ? EllipsePath(cx, cy, r, r) : null;
        }

        private static string EllipseToPath(XElement el)
        {
            float cx = Attr(el, "cx", 0f);
            float cy = Attr(el, "cy", 0f);
            float rx = Attr(el, "rx", 0f);
            float ry = Attr(el, "ry", 0f);
            return rx > 0f && ry > 0f ? EllipsePath(cx, cy, rx, ry) : null;
        }

        // Two 180° arcs — a single arc can't span a full ellipse in SVG.
        private static string EllipsePath(float cx, float cy, float rx, float ry) =>
            $"M {N(cx+rx)},{N(cy)} " +
            $"A {N(rx)},{N(ry)} 0 1 0 {N(cx-rx)},{N(cy)} " +
            $"A {N(rx)},{N(ry)} 0 1 0 {N(cx+rx)},{N(cy)} Z";

        private static string PointsToPath(XElement el, bool closed)
        {
            string raw = el.Attribute("points")?.Value;
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            var nums = raw.Trim()
                          .Split(new[] { ' ', ',', '\t', '\n', '\r' },
                                 System.StringSplitOptions.RemoveEmptyEntries);

            if (nums.Length < 4)
                return null;

            var sb = new StringBuilder();
            for (int i = 0; i + 1 < nums.Length; i += 2)
            {
                float px = float.Parse(nums[i],     CultureInfo.InvariantCulture);
                float py = float.Parse(nums[i + 1], CultureInfo.InvariantCulture);
                sb.Append(i == 0 ? $"M {N(px)},{N(py)}" : $" L {N(px)},{N(py)}");
            }

            if (closed)
                sb.Append(" Z");

            return sb.ToString();
        }

        private static string LineToPath(XElement el) =>
            $"M {Attr(el, "x1", 0f)},{Attr(el, "y1", 0f)} " +
            $"L {Attr(el, "x2", 0f)},{Attr(el, "y2", 0f)}";

        // ── Helpers ──────────────────────────────────────────────────────────

        private static float Attr(XElement el, string name, float fallback)
        {
            string v = el.Attribute(name)?.Value;
            return float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)
                ? result
                : fallback;
        }

        // Locale-safe float → string for path data.
        private static string N(float f) => f.ToString("G6", CultureInfo.InvariantCulture);

        #endregion
    }
}