using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Collections;

public class NPC_Magician : NPC_Talk
{
    public Door_Rotate door_entrance;
    private int dialogueLine = 0;
    public List<string> dialogueLines = new List<string>(); //[cite: 2]
    public GameObject DoorPrefab;
    private Maze_Generator maze;
    public MisionsBase givedMision;
    public bool misionGived;
    public override void Interact(Player player)
    {
        // Ejecuta la lógica base (sonido, asignar player)
        base.Interact(player);

        // Lógica única del mago[cite: 2]
        dialogueLine = 0;
        if(door_entrance.is_opened.Value)
        {
            player.canvas.OpenDialoguePanel(RandomText());
        }
        else
        {
            player.canvas.OpenDialoguePanel("Are you ready for the MAZE");
        }
    }

    public override void OnYes()
    {
        // Comportamiento del botón Yes específico del mago[cite: 2]
        if(door_entrance.is_opened.Value)
        {
            player.canvas.CloseDialoguePanel();
        }
        else
        {
            if (dialogueLine == 0)
            {
                player.canvas.CloseDialoguePanel();
                door_entrance.ToggleDoorServerRpc();
            }
            else
            {
                dialogueLine = dialogueLine > 5 ? 6 : dialogueLine + 1;
                player.canvas.ChangeDialogueText("Sure" + new string('?', dialogueLine));
            }
        }
        if(misionGived == false)
        {
            player.canvas.GiveMision(givedMision);
        }
    }

    public override void OnNo()
    {
        if(door_entrance.is_opened.Value)
        {
            player.canvas.ChangeDialogueText(RandomText());
            return;
        }
        else
        {
            if (dialogueLine == 0)
            {
                dialogueLine += 1;
                player.canvas.ChangeDialogueText("Sure" + new string('?', dialogueLine));
            }
            else
            {
                dialogueLine = 0;
                player.canvas.ChangeDialogueText("Are you ready for the MAZE");
            }
        }
    }

    private string RandomText()
    {
        int randomIndex = Random.Range(0, dialogueLines.Count); //[cite: 2]
        return dialogueLines[randomIndex]; //[cite: 2]
    }

    void Update()
    {
        if (player != null && player.canvas != null)
        {
            UpdateAnimations(player.canvas.DialoguePanel);
        }
    }
    
    void Start()
    {
        //Talk_Panel.SetActive(false);
        if(IsServer)
        {
            DoorSpawn();
        }
        else
        {
            StartCoroutine(FindDoorClient());
        }
    }
    public void DoorSpawn()
    {
        Debug.Log("Spawning door...");
        maze = GameObject.FindObjectOfType<Maze_Generator>();
        GameObject door = Instantiate(DoorPrefab, new Vector3(maze.entrance.x - 0.75f , 0.8f, maze.entrance.y), Quaternion.identity);
        door.transform.rotation = Quaternion.Euler(0, 90, 0);
        door.GetComponent<NetworkObject>().Spawn();
        door_entrance = door.GetComponentInChildren<Door_Rotate>();
        Debug.Log("Door spawned at: " + new Vector3(maze.entrance.x - 0.75f , 0.8f, maze.entrance.y));
    }
    private IEnumerator FindDoorClient()
    {
        // Esperamos 1 segundo para asegurarnos de que la puerta ya viajó por la red y existe en el cliente
        yield return new WaitForSeconds(0.2f);

        // Buscamos todas las puertas del mapa
        Door_Rotate[] allDoors = FindObjectsOfType<Door_Rotate>();
        float closestDistance = Mathf.Infinity;

        // Calculamos cuál es la puerta que está más cerca del mago (que lógicamente será su propia puerta)
        foreach (Door_Rotate d in allDoors)
        {
            float dist = Vector3.Distance(transform.position, d.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                door_entrance = d;
            }
        }  
    }
}