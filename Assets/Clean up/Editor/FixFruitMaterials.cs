// Save as: Assets/Editor/FixFruitMaterials.cs
using UnityEngine;
using UnityEditor;
using System.IO;

public class FixFruitMaterials : EditorWindow
{
    private string modelsFolder = "Assets/Models";      // 📁 Change to your models folder
    private string texturesFolder = "Assets/Textures";  // 📁 Change to your textures folder
    private string materialsFolder = "Assets/Materials/Fruits"; // 📁 Where mats will be saved

    [MenuItem("Tools/Fix Fruit Pink Materials")]
    public static void ShowWindow()
    {
        GetWindow<FixFruitMaterials>("Fix Fruit Materials");
    }

    void OnGUI()
    {
        GUILayout.Label("Fix Pink Fruit Materials", EditorStyles.boldLabel);
        modelsFolder    = EditorGUILayout.TextField("Models Folder",    modelsFolder);
        texturesFolder  = EditorGUILayout.TextField("Textures Folder",  texturesFolder);
        materialsFolder = EditorGUILayout.TextField("Materials Output", materialsFolder);

        GUILayout.Space(10);
        if (GUILayout.Button("FIX ALL PINK MATERIALS", GUILayout.Height(40)))
            FixAll();
    }

    void FixAll()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder(materialsFolder))
        {
            Directory.CreateDirectory(materialsFolder);
            AssetDatabase.Refresh();
        }

        // Find all models (.fbx, .obj, etc.)
        string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { modelsFolder });
        int fixedCount = 0;

        foreach (string guid in modelGuids)
        {
            string modelPath = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
            if (importer == null) continue;

            string modelName = Path.GetFileNameWithoutExtension(modelPath);

            // Find matching texture by name
            string texturePath = FindMatchingTexture(modelName, texturesFolder);
            
            // Create or update material
            string matPath = $"{materialsFolder}/{modelName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // Assign texture if found
            if (!string.IsNullOrEmpty(texturePath))
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (tex != null)
                {
                    mat.SetTexture("_BaseMap", tex); // URP
                    mat.SetTexture("_MainTex", tex); // Built-in fallback
                }
            }

            // Remap material on the model importer
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            var remap = importer.GetExternalObjectMap();

            // Clear old broken remaps and set new one
            foreach (var key in remap.Keys)
            {
                if (key.type == typeof(Material))
                    importer.RemoveRemap(key);
            }

            // Force re-extract
            importer.SearchAndRemapMaterials(
                ModelImporterMaterialName.BasedOnMaterialName,
                ModelImporterMaterialSearch.Local
            );

            AssetDatabase.SaveAssets();
            importer.SaveAndReimport();
            fixedCount++;
        }

        // Final pass: fix any remaining pink (error shader) materials
        FixErrorShaderMaterials();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"✅ Fixed {fixedCount} fruit models!");
        EditorUtility.DisplayDialog("Done!", $"Fixed {fixedCount} models.\nCheck Console for details.", "OK");
    }

    string FindMatchingTexture(string modelName, string texturesFolder)
    {
        // Try exact name match first
        string[] guids = AssetDatabase.FindAssets($"{modelName} t:Texture2D", new[] { texturesFolder });
        if (guids.Length > 0)
            return AssetDatabase.GUIDToAssetPath(guids[0]);

        // Try partial match (e.g. model "apple" matches texture "apple_diffuse")
        string[] allTex = AssetDatabase.FindAssets("t:Texture2D", new[] { texturesFolder });
        foreach (var g in allTex)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            string texName = Path.GetFileNameWithoutExtension(path).ToLower();
            if (texName.StartsWith(modelName.ToLower()))
                return path;
        }

        return null;
    }

    void FixErrorShaderMaterials()
    {
        string[] matGuids = AssetDatabase.FindAssets("t:Material");
        foreach (var guid in matGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader.name.Contains("Hidden/InternalErrorShader"))
            {
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                Debug.Log($"🔧 Fixed error shader on: {path}");
            }
        }
    }
}