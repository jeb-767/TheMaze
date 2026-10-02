using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PortalTP : NetworkBehaviour , IInteractable
{
    public Transform destination;
    public bool canTp = true;
    public AudioSource interactSource;
    void Start()
    {
        // El 'true' le indica a Unity que incluya los objetos desactivados en la búsqueda
        PortalTP[] todosLosPortales = FindObjectsOfType<PortalTP>(true);

        string targetTag = gameObject.CompareTag("PortalBlue") ? "PortalRed" : "PortalBlue";

        foreach (PortalTP portal in todosLosPortales)
        {
            if (portal.gameObject.CompareTag(targetTag))
            {
                destination = portal.transform;
                break;
            }
        }
    }
    public void Interact(Player player)
    {
        interactSource.Play();
        if(!player.GetComponent<NetworkObject>().IsOwner || !canTp) return;
        Vector3 direccionHaciaJugador = player.transform.position - transform.position; 
        float lado = Mathf.Sign(Vector3.Dot(transform.forward, direccionHaciaJugador));
        Vector3 destinationPosition = destination.position + (destination.forward * -0.25f * lado);
        destinationPosition.y = player.transform.position.y;
        player.TeleportPlayerClientRpc(destinationPosition);
        canTp = false;
        StartCoroutine(ResetTp());
    }
    // Update is called once per frame
    public IEnumerator ResetTp()
    {
        yield return new WaitForSeconds(1f);
        canTp = true;
    }
}
