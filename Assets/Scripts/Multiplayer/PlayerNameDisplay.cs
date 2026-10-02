using Unity.Netcode;
using Unity.Collections;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : NetworkBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text nameText; // Arrastra aquí tu TextMeshPro desde el Inspector

    // Usamos un NetworkVariable para que Netcode sincronice automáticamente este valor
    // Se inicializa vacío, lo puede leer cualquiera, pero SOLO el servidor puede escribirlo.
    // Usamos un NetworkVariable para sincronizar el nombre en la red
    private NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        "", 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // 1. Nos suscribimos al evento para que se actualice si cambia en el futuro
        playerName.OnValueChanged += UpdateNameUI;

        // 2. Lógica para asignar el nombre justo al nacer
        if (IsOwner)
        {
            string mySavedName = PlayerPrefs.GetString("MyPlayerName", "Jugador");

            if (IsServer)
            {
                // Si somos el Host, cambiamos el valor directamente (evitamos el bug del ServerRpc en el frame 0)
                playerName.Value = new FixedString32Bytes(mySavedName);
            }
            else
            {
                // Si somos Cliente, le pedimos al Servidor que nos lo cambie
                SetNameServerRpc(mySavedName);
            }
            nameText.enabled = false; // Actualizamos el texto localmente para evitar esperar al primer cambio sincronizado
        }

        // 3. Forzamos la actualización visual INMEDIATA para todos.
        // Esto asegura que cuando entras, veas los nombres de los que ya estaban.
        RefreshUI(playerName.Value.ToString());
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= UpdateNameUI;
    }

    // --- LÓGICA DE RED ---

    [ServerRpc]
    private void SetNameServerRpc(string newName)
    {
        playerName.Value = new FixedString32Bytes(newName);
    }

    private void UpdateNameUI(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        RefreshUI(newValue.ToString());
    }

    private void RefreshUI(string newName)
    {
        if (nameText != null)
        {
            nameText.text = newName;
        }
    }

    private void LateUpdate()
    {
        // Esto hace que el texto mire siempre hacia la cámara del jugador local
        // Es crucial para que los nombres no se vean al revés cuando rotas la cámara
        if (Camera.main != null && nameText != null)
        {
            nameText.transform.LookAt(
                nameText.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }
}