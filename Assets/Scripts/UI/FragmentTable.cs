using UnityEngine;

public class FragmentTable : MonoBehaviour, IInteractable
{
    public Canvas_Controller canvas;
    public AudioSource interactSource;

    public void Interact(Player player)
    {
        player.canvas.OpenFragmentPanel();
        interactSource.Play();
    }
}
