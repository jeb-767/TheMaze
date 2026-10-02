using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class Armor_Setup : NetworkBehaviour
{
    public GameObject Armor; // El objeto padre que contiene las categorías
    [Serializable]
    public class EquipmentCategory // Esta clase sirve de "envoltorio"
    {
        public string categoryName;
        public List<GameObject> prefabs;
    }
    public List<EquipmentCategory> equipmentCategories = new List<EquipmentCategory>();
    public override void OnNetworkSpawn()
    {
        // Solo el servidor tiene permiso para generar y spawnear objetos de red
        if (IsServer)
        {
            SpawnRandomArmor();
        }
    }
    public void Start()
    {
        /*if (IsServer)
        {
            SpawnRandomArmor();
        }*/
    }
    private void SpawnRandomArmor()
    {
        // 1. SOLO EL SERVIDOR decide qué se crea
        if (IsServer)
        {
            /*int catNum = 0;
            foreach (Transform Category_Armor in Armor.transform)
            {
                if (catNum >= equipmentCategories.Count) break;

                var categoryData = equipmentCategories[catNum];
                if (Category_Armor.childCount > 0 && categoryData.prefabs.Count > 0)
                {
                    int randPos = UnityEngine.Random.Range(0, Category_Armor.childCount);
                    int randPrefab = UnityEngine.Random.Range(0, categoryData.prefabs.Count);

                    Transform template = Category_Armor.GetChild(randPos);
                    GameObject newArmor = Instantiate(categoryData.prefabs[randPrefab], template.position, template.rotation);

                    // IMPORTANTE: No le pongas Parent para evitar problemas de sincronización inicial
                    newArmor.GetComponent<NetworkObject>().Spawn();
                    newArmor.transform.SetParent(Category_Armor, true); // Lo asignamos YA SPAWNEADO
                }
                catNum++;
            }*/
            foreach (EquipmentCategory category in equipmentCategories)
            {
                if(category.prefabs.Count > 0)
                {
                    int randPrefab = UnityEngine.Random.Range(0, category.prefabs.Count);
                    Transform template = category.prefabs[randPrefab].transform; // Usamos la posición del prefab directamente
                    GameObject newArmor = Instantiate(category.prefabs[randPrefab], template.position, template.rotation); // Solo rotamos en Y para que mire hacia el centro
                    newArmor.SetActive(true); // Aseguramos que esté activo para que se vea al spawnear
                    newArmor.GetComponent<NetworkObject>().Spawn();
                }
            }
        }
        // 2. TODOS (Host y Clientes) ocultan las plantillas visuales
        // Esto limpia la escena de los objetos "guía" que pusiste en el editor
        //DesactivarPlantillasVisuales();

    }
    

    private void DesactivarPlantillasVisuales()
    {
        foreach (Transform Category_Armor in Armor.transform)
        {
            foreach (Transform child in Category_Armor)
            {
                // Si el objeto NO tiene un NetworkObject, es una plantilla del editor. ¡Fuera!
                child.gameObject.SetActive(false);
            }
        }
    }
}