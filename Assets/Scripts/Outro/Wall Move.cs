using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class WallMove : NetworkBehaviour
{
    public Animator anim;
    public AudioSource audioSource; // Corregido nombre
    public ParticleSystem particle;
    public BoxCollider wallCollider; // Corregido nombre

    // Solo necesitamos una variable para el estado visual
    // true = rota, false = entera
    public NetworkVariable<bool> isBroken = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // Suscribirse al cambio para actualizar visuales
        isBroken.OnValueChanged += OnWallStateChanged;
        //ApplyWallState(isBroken.Value, false, true); // Aplicar estado inicial sin efectos
        // Si soy el servidor, inicio el ciclo de vida de la pared
        if (IsServer)
        {
            StartCoroutine(ServerWallCycle());
        }
    }

    public override void OnNetworkDespawn()
    {
        isBroken.OnValueChanged -= OnWallStateChanged;
    }

    // LÓGICA SOLO DEL SERVIDOR

    private IEnumerator ServerWallCycle()
    {
        while (true) // Ciclo infinito de la pared
        {
            // Esperar para romper
            yield return new WaitForSeconds(Random.Range(15, 25));
            isBroken.Value = true; // Cambiamos el estado directamente desde el servidor sin necesidad de RPC

            // Esperar para recuperar
            yield return new WaitForSeconds(Random.Range(15, 90));
            isBroken.Value = false; // Podemos cambiar el estado directamente desde el servidor sin necesidad de RPC
            // Pequeño delay de seguridad antes de reiniciar el ciclo
            yield return new WaitForSeconds(2);
        }
    }
    // REACCIÓN DE LOS CLIENTES (Y SERVIDOR)
    private void OnWallStateChanged(bool previousValue, bool newValue)
    {
        ApplyWallState(newValue, true, false);
    }

    private void ApplyWallState(bool broken, bool playEffects, bool inicio)
    {
        if (inicio)
        {
            if (broken)
            {
                wallCollider.enabled = false;
            }
            else
            {
                wallCollider.enabled = true;
            }
        }
        else
        {
            if (broken)
            {
                anim.SetTrigger("Break");
                wallCollider.enabled = false;
            }
            else
            {
                anim.SetTrigger("Recover");
                StartCoroutine(colliderEnable());
            }

            // Solo reproducimos sonidos/partículas si es un cambio en tiempo real,
            // no cuando el jugador acaba de spawnear.
            if (playEffects && audioSource != null)
            {
                audioSource.Play();
                if (particle != null) particle.Play();
            }
        }
    }
    private IEnumerator colliderEnable()
    {
        yield return new WaitForSeconds(0.7f);
        wallCollider.enabled = true;
    }
}