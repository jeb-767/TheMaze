using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CullingDinamico : MonoBehaviour
{
    [Header("Configuración Principal")]
    [Tooltip("Distancia a la que los objetos se desactivarán (en metros)")]
    public float distanciaOcultacion = 30f; 
    
    [Tooltip("Cada cuánto tiempo comprueba las distancias (0.5s es ideal)")]
    public float tiempoComprobacion = 0.5f; 

    [Header("Listas de Objetos (Se llenan automáticamente)")]
    public List<GameObject> enemigosActivos = new List<GameObject>();
    public List<Light> lucesActivas = new List<Light>();
    public List<AudioSource> audiosActivos = new List<AudioSource>();
    public List<ParticleSystem> vfxActivos = new List<ParticleSystem>();
    public List<MeshRenderer> mallasActivas = new List<MeshRenderer>(); // NUEVA LISTA
    private Transform jugadorLocal;

    void Start()
    {
        // Iniciamos el bucle infinito de optimización al empezar la partida
        StartCoroutine(RutinaDeOptimizacion());
    }

    #region Métodos de Registro
    // Estos métodos los llamarás desde tu Maze_Generator al instanciar cosas
    
    public void RegistrarEnemigo(GameObject enemigo)
    {
        if (enemigo != null && !enemigosActivos.Contains(enemigo)) 
            enemigosActivos.Add(enemigo);
    }

    public void RegistrarLuz(Light luz)
    {
        if (luz != null && !lucesActivas.Contains(luz)) 
            lucesActivas.Add(luz);
    }

    public void RegistrarAudio(AudioSource audio)
    {
        if (audio != null && !audiosActivos.Contains(audio)) 
            audiosActivos.Add(audio);
    }

    public void RegistrarVFX(ParticleSystem vfx)
    {
        if (vfx != null && !vfxActivos.Contains(vfx)) 
            vfxActivos.Add(vfx);
    }
    public void RegistrarMalla(MeshRenderer malla)
    {
        if (malla != null && !mallasActivas.Contains(malla)) 
            mallasActivas.Add(malla);
    }
    public void EscanearYRegistrarObjetoCompleto(GameObject objetoPadre)
    {
        // 1. Registramos el objeto padre si es un enemigo o tiene IA
        if (objetoPadre.CompareTag("Enemy")) // O el tag que uses
        {
            RegistrarEnemigo(objetoPadre);
        }

        // 2. Buscamos TODAS las luces (en el padre y en todos los hijos)
        Light[] lucesEncontradas = objetoPadre.GetComponentsInChildren<Light>(true);
        foreach (Light luz in lucesEncontradas)
        {
            RegistrarLuz(luz);
        }

        // 3. Buscamos TODOS los sistemas de partículas
        ParticleSystem[] vfxEncontrados = objetoPadre.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem vfx in vfxEncontrados)
        {
            RegistrarVFX(vfx);
        }

        // 4. Buscamos TODOS los audios
        AudioSource[] audiosEncontrados = objetoPadre.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource audio in audiosEncontrados)
        {
            RegistrarAudio(audio);
        }
        MeshRenderer[] mallasEncontradas = objetoPadre.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer malla in mallasEncontradas)
        {
            RegistrarMalla(malla);
        }
    }
    #endregion

    private IEnumerator RutinaDeOptimizacion()
    {
        while (true) // Bucle infinito (seguro porque tiene yield return)
        {
            // Esperamos el tiempo definido antes de volver a comprobar
            yield return new WaitForSeconds(tiempoComprobacion);

            // 1. Asegurarnos de tener al jugador (útil en multijugador si tarda en spawnear)
            if (jugadorLocal == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) 
                {
                    jugadorLocal = playerObj.transform;
                }
                else 
                {
                    continue; // Si aún no hay jugador, saltamos al siguiente frame
                }
            }

            Vector3 posJugador = jugadorLocal.position;
            // Usamos la distancia al cuadrado por matemáticas (es mucho más rápido para la CPU)
            float distanciaCuadradaLimite = distanciaOcultacion * distanciaOcultacion;

            // --------------------------------------------------------
            // 2. OPTIMIZAR ENEMIGOS / IA (GameObjects enteros)
            // Recorremos de atrás hacia adelante por si alguno murió/fue destruido
            // --------------------------------------------------------
            for (int i = enemigosActivos.Count - 1; i >= 0; i--)
            {
                if (enemigosActivos[i] == null)
                {
                    enemigosActivos.RemoveAt(i);
                    continue;
                }

                float distanciaSq = (enemigosActivos[i].transform.position - posJugador).sqrMagnitude;
                bool cerca = distanciaSq <= distanciaCuadradaLimite;
                
                if (enemigosActivos[i].activeSelf != cerca)
                {
                    enemigosActivos[i].SetActive(cerca);
                }
            }

            // --------------------------------------------------------
            // 3. OPTIMIZAR LUCES
            // --------------------------------------------------------
            for (int i = lucesActivas.Count - 1; i >= 0; i--)
            {
                if (lucesActivas[i] == null)
                {
                    lucesActivas.RemoveAt(i);
                    continue;
                }

                float distanciaSq = (lucesActivas[i].transform.position - posJugador).sqrMagnitude;
                bool cerca = distanciaSq <= distanciaCuadradaLimite;

                if (lucesActivas[i].enabled != cerca)
                {
                    lucesActivas[i].enabled = cerca;
                }
            }

            // --------------------------------------------------------
            // 4. OPTIMIZAR AUDIOS EN BUCLE (Antorchas, portales, etc)
            // --------------------------------------------------------
            for (int i = audiosActivos.Count - 1; i >= 0; i--)
            {
                if (audiosActivos[i] == null)
                {
                    audiosActivos.RemoveAt(i);
                    continue;
                }

                float distanciaSq = (audiosActivos[i].transform.position - posJugador).sqrMagnitude;
                bool cerca = distanciaSq <= distanciaCuadradaLimite;

                if (audiosActivos[i].enabled != cerca)
                {
                    audiosActivos[i].enabled = cerca;
                }
            }

            // --------------------------------------------------------
            // 5. OPTIMIZAR PARTÍCULAS (VFX)
            // --------------------------------------------------------
            for (int i = vfxActivos.Count - 1; i >= 0; i--)
            {
                if (vfxActivos[i] == null)
                {
                    vfxActivos.RemoveAt(i);
                    continue;
                }

                float distanciaSq = (vfxActivos[i].transform.position - posJugador).sqrMagnitude;
                bool cerca = distanciaSq <= distanciaCuadradaLimite;

                if (cerca && !vfxActivos[i].isPlaying)
                {
                    vfxActivos[i].Play(); // Reanuda la emisión
                }
                else if (!cerca && vfxActivos[i].isPlaying)
                {
                    // Detiene suavemente (no apaga de golpe, deja morir las partículas viejas)
                    vfxActivos[i].Stop(true, ParticleSystemStopBehavior.StopEmitting); 
                }
            }



            for (int i = mallasActivas.Count - 1; i >= 0; i--)
            {
                if (mallasActivas[i] == null)
                {
                    mallasActivas.RemoveAt(i);
                    continue;
                }

                float distanciaSq = (mallasActivas[i].transform.position - posJugador).sqrMagnitude;
                bool cerca = distanciaSq <= distanciaCuadradaLimite;

                if (mallasActivas[i].enabled != cerca)
                {
                    mallasActivas[i].enabled = cerca;
                }
            }
        }
    }
}