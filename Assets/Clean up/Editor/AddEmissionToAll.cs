using UnityEngine;
using UnityEditor;

public class AddFruitEmission : EditorWindow
{
    [MenuItem("Tools/Add Emission To Fruit Materials")]
    public static void AddEmissionToAll()
    {
        string materialsFolder = "Assets/NekoLegends/Food/Fruits/MaterialsAndTextures";
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { materialsFolder });

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Use the actual texture as emission map instead of flat color
            Texture tex = mat.GetTexture("_BaseMap");
            if (tex != null)
            {
                mat.SetTexture("_EmissionMap", tex);
                mat.SetColor("_EmissionColor", Color.white * 2f); // intensity multiplier
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✅ Emission fixed on all fruit materials!");
    }
}