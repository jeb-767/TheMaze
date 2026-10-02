using UnityEngine;

public class ReinforceTable : MonoBehaviour, IInteractable
{
    public Canvas_Controller canvas;
    public AudioSource interactSource;
    void Start()
    {
        /*canvas = FindObjectOfType<Canvas_Controller>();
        canvas.CloseReinforceTable();*/
    }
    public void Interact(Player player)
    {
        player.canvas.OpenReinforceTable();
        interactSource.Play();
    }
}
