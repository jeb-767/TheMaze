using UnityEngine;

public class ExchangeTable : MonoBehaviour, IInteractable
{
    Canvas_Controller canvas;
    public AudioSource interactSource;

    public void Interact(Player player)
    {
        player.canvas.OpenExchangeTable();
        interactSource.Play();
    }
}
