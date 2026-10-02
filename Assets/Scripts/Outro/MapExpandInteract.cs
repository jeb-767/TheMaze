using UnityEngine;
using System.Collections;

public class MapExpandInteract : MonoBehaviour, IInteractable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Animator anim;
    public AudioSource audioS;
    void Start()
    {
        anim = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
        anim.SetBool("Floor" , true);
    }

    public void Interact(Player player)
    {
        Map map = FindObjectOfType<Map>();
        if (map != null)
        {
            map.UpdateMapSize();
            audioS.Play();
        }
        Renderer renderer = GetComponent<Renderer>();
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false; // Desactiva el collider para que no se pueda volver a interactuar
        }
        if (renderer != null)
        {
            renderer.enabled = false; // Desactiva el renderer para que no se vea el objeto
        }
        StartCoroutine(desactivateObject());
    }
    public IEnumerator desactivateObject()
    {
        yield return new WaitForSeconds(audioS.clip.length);
        this.gameObject.SetActive(false); // Desactiva el objeto para que no se pueda volver a interactuar
    }
}
