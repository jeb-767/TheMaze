using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class StatueInteract : NetworkBehaviour, IInteractable
{
    public bool activated = false;
    public Transform statuePart;
    public List<GameObject> enemies;
    public AudioSource statue_audio;
    public GameObject enemieSpawned;
    public ItemDatabase armorDataBase;
    public Animator anim;
    public AnimationClip animReset;
    public bool reset = true;
    public GameObject armorDrop;
    public void Interact(Player player)
    {
        if (!activated && reset)
        {
            enemieSpawned = Instantiate(enemies[Random.Range(0, enemies.Count)], new Vector3(statuePart.position.x + Random.Range(-1, 1f), 0, statuePart.position.z + Random.Range(-1f, 1f)), Quaternion.identity);
            enemieSpawned.SetActive(true);
            enemieSpawned.GetComponent<NetworkObject>().Spawn();
            activated = true;
            reset = false;
            statue_audio.Play();
            anim.SetTrigger("Fall");
        }
    }
    public void Update()
    {
        if (enemieSpawned == null && activated)
        {
            activated = false;
            armorDrop = Instantiate(armorDataBase.allPrefabs[Random.Range(0, armorDataBase.allPrefabs.Count)], statuePart.position + Vector3.forward * 0.5f + Vector3.up * 0.2f, Quaternion.identity);
            armorDrop.SetActive(true);
            if (armorDrop.GetComponent<Armor_Interact>().newItem.tipoEquipo == TipoEquipo.Weapon)
            {
                armorDrop.GetComponent<Armor_Interact>().newItem.Daño = Mathf.Round(((float)(Random.Range(6, 12))) * 100f) / 100f;
                if (armorDrop.GetComponent<Animator>() != null)
                {
                    armorDrop.GetComponent<Animator>().SetBool("Floor", true);
                }
            }
            else
            {
                armorDrop.GetComponent<Armor_Interact>().newItem.Armadura = Mathf.Round(((float)(Random.Range(2, 7) / 10f)) * 100f) / 100f;
                if (armorDrop.GetComponent<Animator>() != null)
                {
                    armorDrop.GetComponent<Animator>().SetBool("Floor", true);
                }
            }
            int ranQual = Random.Range(0, 100);
            if (armorDrop != null)
            {
                if (ranQual >= 90)
                {
                    armorDrop.GetComponent<Armor_Interact>().newItem.rarezaEquipo = Rareza.Mhytic;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Daño *= 2f;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Armadura *= 2f;
                }
                else if (ranQual >= 10)
                {
                    armorDrop.GetComponent<Armor_Interact>().newItem.rarezaEquipo = Rareza.Legendary;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Daño *= 1.8f;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Armadura *= 1.8f;

                }
                else
                {
                    armorDrop.GetComponent<Armor_Interact>().newItem.rarezaEquipo = Rareza.Epic;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Daño *= 1.6f;
                    armorDrop.GetComponent<Armor_Interact>().newItem.Armadura *= 1.6f;
                }
            }
            int time = Random.Range(30, 60);
            anim.SetFloat("SpeedReset", animReset.length / time);
            anim.SetTrigger("Reset");
            armorDrop.GetComponent<NetworkObject>().Spawn();
        }
    }
    public void ResetStatue()
    {
        Debug.Log("Reset Statue");
        reset = true;
    }
}
