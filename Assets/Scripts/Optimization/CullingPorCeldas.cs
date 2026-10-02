using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CullingPorCeldas : MonoBehaviour
{
    public int tamañoCelda = 10;
    public float distanciaActivacion = 30f;
    
    [Tooltip("Déjalo vacío para que siga a la cámara principal automáticamente")]
    public Transform objetivoASeguir; 
    
    private Dictionary<Vector2Int, List<GameObject>> sectores = new Dictionary<Vector2Int, List<GameObject>>();
    private HashSet<Vector2Int> sectoresActivosActuales = new HashSet<Vector2Int>();
    
    private Vector2Int celdaActualizada = new Vector2Int(99999, 99999);
    private bool inicializado = false;

    public void RegistrarEnSectores(GameObject obj)
    {
        if (obj == null) return; 

        Vector2Int coord = PosicionACelda(obj.transform.position);

        if (!sectores.ContainsKey(coord)) sectores[coord] = new List<GameObject>();
        sectores[coord].Add(obj);
        
        if (sectoresActivosActuales.Contains(coord))
            ToggleVisibility(obj, true);
        else
            ToggleVisibility(obj, false);
    }

    private Vector2Int PosicionACelda(Vector3 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / tamañoCelda),
            Mathf.FloorToInt(pos.z / tamañoCelda)
        );
    }

    void Update()
    {
        // Encontrar objetivo de forma dinámica. Si la cámara muere o cambia, se readapta.
        Transform target = objetivoASeguir;
        if (target == null)
        {
            if (Camera.main != null) target = Camera.main.transform;
        }

        // Si no hay jugador ni cámara en escena todavía, esperamos al siguiente frame
        if (target == null) return;

        Vector2Int celdaActual = PosicionACelda(target.position);

        // Si cambiamos de celda, actualizamos
        if (!inicializado || celdaActual != celdaActualizada)
        {
            ActualizarSectores(celdaActual);
            
            // Registramos la actualización INCLUSO si hubo errores internos
            celdaActualizada = celdaActual;
            inicializado = true;
        }
    }

    void ActualizarSectores(Vector2Int celdaCentro)
    {
        int radioCeldas = Mathf.CeilToInt(distanciaActivacion / tamañoCelda);
        HashSet<Vector2Int> nuevosSectoresActivos = new HashSet<Vector2Int>();

        for (int x = -radioCeldas; x <= radioCeldas; x++)
        {
            for (int y = -radioCeldas; y <= radioCeldas; y++)
            {
                nuevosSectoresActivos.Add(new Vector2Int(celdaCentro.x + x, celdaCentro.y + y));
            }
        }

        foreach (Vector2Int coord in sectoresActivosActuales)
        {
            if (!nuevosSectoresActivos.Contains(coord))
            {
                SetEstadoSector(coord, false);
            }
        }

        foreach (Vector2Int coord in nuevosSectoresActivos)
        {
            if (!sectoresActivosActuales.Contains(coord))
            {
                SetEstadoSector(coord, true);
            }
        }

        sectoresActivosActuales = nuevosSectoresActivos;
    }

    void SetEstadoSector(Vector2Int coord, bool estado)
    {
        if (sectores.TryGetValue(coord, out List<GameObject> objetos))
        {
            for (int i = objetos.Count - 1; i >= 0; i--)
            {
                // El Try-Catch evitará que un objeto destruido o corrupto congele todo el mapa
                try 
                {
                    GameObject obj = objetos[i];
                    if (obj != null && !obj.Equals(null)) 
                    {
                        ToggleVisibility(obj, estado);
                    }
                    else 
                    {
                        // Si el objeto se destruyó en red, lo sacamos de la lista
                        objetos.RemoveAt(i); 
                    }
                }
                catch (System.Exception)
                {
                    // Error silencioso interceptado. Extraemos el objeto corrupto y continuamos cargando el mapa
                    objetos.RemoveAt(i);
                }
            }
        }
    }

    void ToggleVisibility(GameObject obj, bool estado)
    {
        if (obj == null) return;
        
        // Si es un objeto multijugador que no debe perder sincronización:
        if (obj.TryGetComponent<NetworkObject>(out _))
        {
            // Apagamos mallas y colisiones con extrema precaución revisando que existan
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true)) 
                if (r != null) r.enabled = estado;
                
            foreach (Collider c in obj.GetComponentsInChildren<Collider>(true)) 
                if (c != null) c.enabled = estado;
                
            foreach (Light l in obj.GetComponentsInChildren<Light>(true)) 
                if (l != null) l.enabled = estado;
        }
        else
        {
            // Para el 99% de paredes estáticas y techos, esto es más seguro y rápido
            obj.SetActive(estado);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform target = objetivoASeguir != null ? objetivoASeguir : (Camera.main != null ? Camera.main.transform : null);
        if (target == null) return;
        
        Gizmos.color = Color.cyan;
        int radio = Mathf.CeilToInt(distanciaActivacion / tamañoCelda);
        Vector2Int centro = PosicionACelda(target.position);

        for (int x = -radio; x <= radio; x++)
        {
            for (int y = -radio; y <= radio; y++)
            {
                Vector3 posCelda = new Vector3((centro.x + x) * tamañoCelda + (tamañoCelda * 0.5f), 0, (centro.y + y) * tamañoCelda + (tamañoCelda * 0.5f));
                Gizmos.DrawWireCube(posCelda, new Vector3(tamañoCelda, 1, tamañoCelda));
            }
        }
    }
}