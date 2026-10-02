using Unity.Netcode.Components;
using UnityEngine;

// Esta clase permite que el DUEÑO (Owner) controle las animaciones, no solo el servidor.
public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        // Devolvemos false para que la autoridad sea del DUEÑO del objeto
        return false;
    }
}