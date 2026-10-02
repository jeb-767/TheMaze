using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class NetworkEnemyOptimizer : NetworkBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Renderer[] renderers;
    
    [Header("Configuración")]
    public float distanciaCulling = 40f;
    private float sqrDistLimite;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        renderers = GetComponentsInChildren<Renderer>();
        sqrDistLimite = distanciaCulling * distanciaCulling;
    }

    void Start()
    {
        // Comprobamos la distancia cada segundo
        InvokeRepeating(nameof(OptimizeTick), 1f, 1f);
    }

    void OptimizeTick()
    {
        // 1. LÓGICA DE SERVIDOR (IA)
        if (IsServer)
        {
            bool alguienCerca = false;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (client.PlayerObject != null)
                {
                    float d2 = (transform.position - client.PlayerObject.transform.position).sqrMagnitude;
                    if (d2 < sqrDistLimite) { alguienCerca = true; break; }
                }
            }
            if (agent != null) agent.enabled = alguienCerca;
        }

        // 2. LÓGICA DE CLIENTE (Visuales)
        if (IsClient)
        {
            var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (localPlayer != null)
            {
                float d2 = (transform.position - localPlayer.transform.position).sqrMagnitude;
                bool yoCerca = d2 < sqrDistLimite;

                if (anim != null) anim.enabled = yoCerca;
                foreach (var r in renderers) r.enabled = yoCerca;
            }
        }
    }
}