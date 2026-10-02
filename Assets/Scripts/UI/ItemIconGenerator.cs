
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
public class ItemIconGenerator : MonoBehaviour
{
    public Camera renderCamera;
    public RenderTexture renderTex;
    public GameObject backgroundPrefab;
    [Tooltip("Margen extra alrededor del objeto (1.0f es ajuste exacto, 1.2f da un poco de aire)")]
    public float fitPadding = 1.2f;

    public Sprite GenerateIcon(GameObject prefab, ItemData item)
    {
        // 1. Instanciar el objeto en una posición temporal
        GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        obj.SetActive(true);
        ActivateAllChildren(obj.transform);
        if (!(item.tipoEquipo == TipoEquipo.Weapon || item.tipoEquipo == TipoEquipo.Key || item.tipoEquipo == TipoEquipo.Tourch))
        {
            obj.transform.rotation = Quaternion.Euler(270, 180, 0);
        }
        else if (item.tipoEquipo == TipoEquipo.Dropped)
        {
            obj.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        // Asegurar que todo está activo (para objetos compuestos)


        // 2. Calcular los límites (Bounds) combinados de todos los hijos visuales
        Bounds bounds = GetBounds(obj, item);

        // 3. Calcular la distancia necesaria para que el objeto quepa en la cámara
        float objectSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        float distance = CalculateCameraDistance(objectSize, renderCamera.fieldOfView, fitPadding);

        // 4. Calcular la posición para centrar el objeto
        // El truco: Alinear el CENTRO del bounds con el eje de la cámara, no el pivote del objeto.
        Vector3 pivotToCenter = bounds.center - obj.transform.position;
        Vector3 targetPosition = renderCamera.transform.position + (renderCamera.transform.forward * distance) - pivotToCenter;

        obj.transform.position = targetPosition;

        // Opcional: Rotar el objeto para que se vea mejor (ej: un poco inclinado)
        // obj.transform.rotation = Quaternion.Euler(0, 150, 0); 

        // 5. Renderizar
        renderCamera.targetTexture = renderTex;
        renderCamera.Render();

        // 6. Crear textura
        RenderTexture.active = renderTex;
        Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        obj.SetActive(false);
        Destroy(obj);
        backgroundPrefab.SetActive(true);
        return sprite;
    }

    // Activa recursivamente todos los hijos
    private void ActivateAllChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(true);
            if (child.childCount > 0) ActivateAllChildren(child);
        }
    }

    // Calcula la caja que engloba todos los renderers (MeshRenderer, SkinnedMeshRenderer)
    private Bounds GetBounds(GameObject obj, ItemData item)
    {
        Bounds bounds = new Bounds(obj.transform.position, Vector3.zero);
        Renderer[] renderers;
        if (item.tipoEquipo == TipoEquipo.Tourch)
        {
            Renderer mainRenderer = obj.GetComponent<Renderer>();
            renderers = (mainRenderer != null) ? new Renderer[] { mainRenderer } : new Renderer[0];
        }
        else
        {
            renderers = obj.GetComponentsInChildren<Renderer>();
        }
        if (renderers.Length > 0)
        {
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }
        return bounds;
    }

    // Calcula la distancia óptima para una cámara de perspectiva
    private float CalculateCameraDistance(float objectSize, float fieldOfView, float paddingMultiplier)
    {
        // Fórmula trigonométrica para encajar un objeto en el frustum de la cámara
        float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * fieldOfView);
        float distance = paddingMultiplier * objectSize / cameraView;

        // Aseguramos una distancia mínima para evitar clipping si el objeto es muy pequeño
        return Mathf.Max(distance, 0.3f);
    }
    public void DesactivateBackground()
    {
        backgroundPrefab.SetActive(false);
    }
}