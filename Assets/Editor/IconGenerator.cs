using UnityEngine;
using UnityEditor;
using System.IO;

// Este script solo funciona dentro del editor de Unity
#if UNITY_EDITOR
public class IconGenerator : EditorWindow
{
    private GameObject objetoARenderizar;
    private Camera camaraRender;
    private int tamanoImagen = 256; // Resolución del icono (256x256)

    [MenuItem("Herramientas/Generador de Iconos")]
    public static void ShowWindow()
    {
        GetWindow<IconGenerator>("Generador de Iconos");
    }

    void OnGUI()
    {
        GUILayout.Label("Generador de Iconos desde Prefabs", EditorStyles.boldLabel);
        
        objetoARenderizar = (GameObject)EditorGUILayout.ObjectField("Prefab del Objeto", objetoARenderizar, typeof(GameObject), false);
        camaraRender = (Camera)EditorGUILayout.ObjectField("Cámara de Render", camaraRender, typeof(Camera), true);
        tamanoImagen = EditorGUILayout.IntField("Tamaño del Icono", tamanoImagen);

        if (GUILayout.Button("Generar Icono"))
        {
            if (objetoARenderizar != null && camaraRender != null)
            {
                GenerarIcono();
            }
            else
            {
                Debug.LogError("Asigna un prefab y una cámara de render.");
            }
        }
    }

    void GenerarIcono()
    {
        // 1. Crea una instancia temporal del objeto en una capa que solo vea la cámara de render
        GameObject instancia = Instantiate(objetoARenderizar, Vector3.zero, Quaternion.Euler(25, 45, 25)); // Ángulo típico para items
        
        // 2. Prepara la RenderTexture
        RenderTexture renderTex = new RenderTexture(tamanoImagen, tamanoImagen, 24);
        camaraRender.targetTexture = renderTex;

        // 3. Renderiza la cámara
        camaraRender.Render();

        // 4. Convierte la RenderTexture a una Texture2D y la guarda como PNG
        RenderTexture.active = renderTex;
        Texture2D texture2D = new Texture2D(tamanoImagen, tamanoImagen, TextureFormat.RGBA32, false);
        texture2D.ReadPixels(new Rect(0, 0, tamanoImagen, tamanoImagen), 0, 0);
        texture2D.Apply();
        RenderTexture.active = null;

        byte[] bytes = texture2D.EncodeToPNG();
        string ruta = Path.Combine(Application.dataPath, "GeneratedIcons", objetoARenderizar.name + "_icon.png");
        
        // Asegúrate de que la carpeta existe
        Directory.CreateDirectory(Path.GetDirectoryName(ruta));
        File.WriteAllBytes(ruta, bytes);

        // 5. Limpia
        DestroyImmediate(instancia);
        camaraRender.targetTexture = null;
        DestroyImmediate(renderTex);
        DestroyImmediate(texture2D);

        // Importa el asset para que Unity lo reconozca como sprite
        AssetDatabase.Refresh();
        string rutaAsset = "Assets/GeneratedIcons/" + objetoARenderizar.name + "_icon.png";
        TextureImporter importer = AssetImporter.GetAtPath(rutaAsset) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
        
        Debug.Log("¡Icono generado con éxito en: " + rutaAsset);
    }
}
#endif