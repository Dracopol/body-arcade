using System.IO;
using IMVR.BodyArcade.AirSacShapes.SplineExtrude;
using UnityEditor;
using UnityEngine;

namespace IMVR.BodyArcade.Editor.SplineExtrude
{
    [CustomEditor(typeof(SplineMeshExtruder))]
    internal class SplineMeshExtruderEditor : UnityEditor.Editor
    {
        #region Update

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var extruder = (SplineMeshExtruder)target;

            GUILayout.Space(8);

            if (GUILayout.Button("Rebuild Mesh", GUILayout.Height(28)))
            {
                extruder.Rebuild();
                EditorUtility.SetDirty(extruder);
            }

            if (GUILayout.Button("Save Mesh Asset", GUILayout.Height(24)))
                SaveMeshAsset(extruder);
        }

        #endregion

        #region Private

        // Bakes the generated mesh to a .asset so it persists in prefabs without runtime regeneration.
        private static void SaveMeshAsset(SplineMeshExtruder extruder)
        {
            extruder.Rebuild();

            Mesh mesh = extruder.GeneratedMesh;
            if (mesh == null)
            {
                EditorUtility.DisplayDialog("Save Mesh", "No mesh was generated. Check the spline reference.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Extruded Mesh",
                $"{extruder.name} Extruded",
                "asset",
                "Choose where to save the baked mesh asset.");

            if (string.IsNullOrEmpty(path))
                return;

            Mesh baked = Object.Instantiate(mesh);
            baked.name = Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(baked, path);
            AssetDatabase.SaveAssets();

            var meshFilter = extruder.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                meshFilter.sharedMesh = baked;
                EditorUtility.SetDirty(meshFilter);
            }

            EditorGUIUtility.PingObject(baked);
        }

        #endregion
    }
}
