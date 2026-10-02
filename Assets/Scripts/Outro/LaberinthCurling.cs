using UnityEngine;

public class LabyrinthCulling : MonoBehaviour
{
    Camera cam;
    Plane[] planes;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        planes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (GameObject section in LabyrinthManager.Instance.sections)
        {
            Bounds b = CalculateBounds(section);
            bool visible = GeometryUtility.TestPlanesAABB(planes, b);

            foreach (Renderer r in section.GetComponentsInChildren<Renderer>())
                r.enabled = visible;
        }
    }

    Bounds CalculateBounds(GameObject parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(parent.transform.position, Vector3.zero);

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        return combined;
    }
}