/*using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class FixMesh2DShaders : EditorWindow
{
    [MenuItem("Tools/Eliminar Efecto Toon Mesh2D")]
    public static void Execute()
    {
        string shaderBusca = "Universal Render Pipeline/2D/Mesh2D-Lit-Default";
        string shaderNuevo = "Universal Render Pipeline/Lit";

        string[] guids = AssetDatabase.FindAssets("t:Material");
        int cambiados = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader.name == shaderBusca)
            {
                mat.shader = Shader.Find(shaderNuevo);
                EditorUtility.SetDirty(mat);
                cambiados++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Proceso finalizado. Se han arreglado {cambiados} materiales.");
    }
}*/