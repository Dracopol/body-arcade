using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace IMVR.BodyArcade.Editor.SvgToSpline
{
    internal class SvgToSpline : EditorWindow
    {
        #region Fields

        private string _svgPath       = "";
        private float  _zDepth        = 0f;
        private float  _scale         = 1f;
        private bool   _flipY         = true;
        private bool   _centerPivot   = true;
        private string _statusMsg     = "";
        private bool   _statusIsError = false;

        #endregion

        #region Setup

        [MenuItem("Tools/SVG to Spline Importer")]
        private static void Open() => GetWindow<SvgToSpline>("SVG → Spline");

        #endregion

        #region Update

        private void OnGUI()
        {
            GUILayout.Space(8);
            GUILayout.Label("SVG to Spline Importer", EditorStyles.boldLabel);
            GUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            _svgPath = EditorGUILayout.TextField("SVG File", _svgPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string picked = EditorUtility.OpenFilePanel("Select SVG", Application.dataPath, "svg");
                if (!string.IsNullOrEmpty(picked))
                    _svgPath = picked;
            }
            EditorGUILayout.EndHorizontal();

            _zDepth      = EditorGUILayout.FloatField("Z Depth",          _zDepth);
            _scale       = EditorGUILayout.FloatField("Scale",            _scale);
            _flipY       = EditorGUILayout.Toggle("Flip Y (recommended)", _flipY);
            _centerPivot = EditorGUILayout.Toggle("Center Pivot",         _centerPivot);

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(_statusMsg))
            {
                var style = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = _statusIsError ? Color.red : Color.green }
                };
                EditorGUILayout.LabelField(_statusMsg, style);
                GUILayout.Space(6);
            }

            if (GUILayout.Button("Import as Spline", GUILayout.Height(30)))
                Import();
        }

        #endregion

        #region Private

        private void Import()
        {
            _statusMsg = "";

            if (string.IsNullOrEmpty(_svgPath) || !System.IO.File.Exists(_svgPath))
            {
                SetStatus("No valid SVG file selected.", true);
                return;
            }

            try
            {
                XDocument doc = XDocument.Parse(System.IO.File.ReadAllText(_svgPath));

                List<string> pathDataList = SvgElementNormalizer.ExtractPathData(doc);

                if (pathDataList.Count == 0)
                {
                    SetStatus("No supported shape elements found in SVG.", true);
                    return;
                }

                var root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(_svgPath));
                Undo.RegisterCreatedObjectUndo(root, "Import SVG Spline");

                int splineCount = 0;

                foreach (string d in pathDataList)
                {
                    List<List<BezierKnot>> subPaths = SvgPathParser.ParsePathData(d);

                    foreach (List<BezierKnot> knots in subPaths)
                    {
                        if (knots.Count < 2)
                            continue;

                        bool   closed = false;
                        float3 first  = knots[0].Position;

                        if (math.distance(first, knots[knots.Count - 1].Position) < 0.001f)
                        {
                            knots.RemoveAt(knots.Count - 1);
                            closed = true;
                        }

                        if (closed && knots.Count >= 2 &&
                            math.distance(first, knots[knots.Count - 1].Position) < 0.001f)
                        {
                            BezierKnot arcEnd    = knots[knots.Count - 1];
                            BezierKnot pathStart = knots[0];
                            knots[0] = new BezierKnot(pathStart.Position, arcEnd.TangentIn, pathStart.TangentOut, pathStart.Rotation);
                            knots.RemoveAt(knots.Count - 1);
                        }

                        if (knots.Count < 2)
                            continue;

                        float yMul = _flipY ? -1f : 1f;

                        float3 centroid = float3.zero;
                        if (_centerPivot)
                        {
                            foreach (BezierKnot k in knots)
                                centroid += k.Position;
                            centroid   /= knots.Count;
                            centroid.z  = 0;
                        }

                        var finalKnots = new BezierKnot[knots.Count];
                        for (int i = 0; i < knots.Count; i++)
                        {
                            BezierKnot k    = knots[i];
                            float3     pos  = new float3(k.Position.x   * _scale, k.Position.y   * yMul * _scale, _zDepth);
                            float3     tin  = new float3(k.TangentIn.x  * _scale, k.TangentIn.y  * yMul * _scale, 0f);
                            float3     tout = new float3(k.TangentOut.x * _scale, k.TangentOut.y * yMul * _scale, 0f);

                            if (_centerPivot)
                            {
                                float3 c = new float3(centroid.x * _scale, centroid.y * yMul * _scale, 0f);
                                pos -= c;
                            }

                            finalKnots[i] = new BezierKnot(pos, tin, tout, quaternion.identity);
                        }

                        var go        = new GameObject($"Spline_{splineCount++}");
                        go.transform.SetParent(root.transform, false);
                        var container = go.AddComponent<SplineContainer>();
                        Spline spline = container.Spline;

                        spline.Clear();
                        foreach (BezierKnot k in finalKnots)
                            spline.Add(k, TangentMode.Broken);
                        spline.Closed = closed;
                    }
                }

                Selection.activeGameObject = root;
                SetStatus($"Done! Created {splineCount} spline(s).", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}", true);
                Debug.LogException(ex);
            }
        }

        private void SetStatus(string msg, bool isError)
        {
            _statusMsg     = msg;
            _statusIsError = isError;
            Repaint();
        }

        #endregion
    }
}