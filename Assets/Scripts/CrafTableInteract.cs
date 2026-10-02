using UnityEngine;

public class CrafTableInteract : MonoBehaviour, IInteractable
{
    public GameObject craftPanel;
    public Canvas_Controller canvasController;
    public AudioSource interactSource;
    void Start()
    {
        /*canvasController = GameObject.Find("Canvas").GetComponent<Canvas_Controller>();
        canvasController.CloseCraftPanel();*/
    }
    public void Interact(Player player)
    {
        canvasController = player.canvas;
        canvasController.OpenCraftPanel();
        interactSource.Play();
    }
}
