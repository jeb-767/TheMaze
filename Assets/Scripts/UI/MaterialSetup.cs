using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections; // Necesario para la Corrutina

public class MaterialSetup : MonoBehaviour, IPointerClickHandler
{
    public TMP_Dropdown miDropdown;
    public ItemIconGenerator itemIconGenerator;
    public ItemDataBase itemDatabase;
    public Image imageBase;
    public ItemData currentItem;

    public void Start()
    {
        Debug.Log("Iniciando MaterialSetup");
        // Si el generador no está asignado, lo buscamos
        if (itemIconGenerator == null)
            itemIconGenerator = FindObjectOfType<ItemIconGenerator>();
        imageBase.sprite = itemIconGenerator.GenerateIcon(itemDatabase.objects[0].prefab, itemDatabase.objects[0]);
        currentItem = itemDatabase.objects[0];
    }

    // Detectamos el clic
    public void OnPointerClick(PointerEventData eventData)
    {
        // Usamos una Corrutina para esperar a que el Dropdown se genere en la jerarquía
        StartCoroutine(ModificarLosComponentes());
    }

    IEnumerator ModificarLosComponentes()
    {
        // Esperamos al final del frame para que 'Dropdown List' ya exista
        yield return new WaitForEndOfFrame();

        GameObject listaDesplegada = GameObject.Find("Dropdown List");

        if (listaDesplegada != null)
        {
            Transform content = listaDesplegada.transform.Find("Viewport/Content");

            // En el objeto clonado "Dropdown List", los hijos del Content son los items.
            // OJO: Empezamos desde i = 0.
            int i = -1;
            foreach (Transform item in content)
            {
                // El primer hijo en el Content suele ser el ítem 'Template', 
                // pero en la lista generada, los activos son los que nos importan.
                // Usamos i-1 si el primer hijo es el fondo o un objeto extra. 
                // Normalmente, si hay 3 opciones, hay 3 hijos + el template oculto.

                // Saltamos el objeto que no sea un item real (usualmente llamado "Item")
                if (!item.name.Contains("Item")) continue;
                if (i == -1)
                {
                    i++;
                    continue;
                }
                if (i < itemDatabase.objects.Count)
                {
                    // RECOMENDACIÓN: En lugar de GetChild(3), que es frágil,
                    // busca el componente Image que está dentro del ítem.
                    // Si el componente está en el cuarto hijo, GetChild(3) está bien, 
                    // pero asegúrate de que el índice sea exacto.

                    Image iconoTarget = item.GetChild(0).GetComponent<Image>();

                    if (iconoTarget != null)
                    {
                        iconoTarget.sprite = itemIconGenerator.GenerateIcon(
                            itemDatabase.objects[i].prefab,
                            itemDatabase.objects[i]
                        );
                    }
                }
                i++;
            }
        }
        else
        {
            Debug.LogError("No se encontró 'Dropdown List'. Asegúrate de que el clic se detecta.");
        }
    }

    public void onValueChanged()
    {
        imageBase.sprite = itemIconGenerator.GenerateIcon(itemDatabase.objects[miDropdown.value].prefab, itemDatabase.objects[miDropdown.value]);
        currentItem = itemDatabase.objects[miDropdown.value];
    }
}