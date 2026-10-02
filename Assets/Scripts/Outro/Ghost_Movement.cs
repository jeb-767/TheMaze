using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using System.Linq;
using Unity.Netcode;
public class Ghost_Movement : MonoBehaviour
{
    public GameObject[] goalLocations;
    private UnityEngine.AI.NavMeshAgent agent;
    public Maze_Generator maze;
    public GameObject player;
    public float health = 50f;
    public float currentHealth = 50f;
    public bool isDead = false;
    public void Start()
    {
        currentHealth = health;
        maze = GameObject.FindObjectOfType<Maze_Generator>();
        maze.enemies.Add(this.gameObject);
        Debug.Log(maze.enemies);
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        agent.SetDestination(player.transform.position);
        StartCoroutine("setDestination");
    }

    void Update()
    {
        if (currentHealth <= 0f && !isDead)
        {
            isDead = true;
            maze.enemies.Remove(this.gameObject);
            Destroy(this.gameObject);
        }
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
            StartCoroutine("setDestination");
        }
        //Vector3 vector = new Vector3(transform.position.x - goalLocations[0].transform.position.x ,  transform.position.y - goalLocations[0].transform.position.y , transform.position.z - goalLocations[0].transform.position.z);
    }
    public IEnumerator setDestination()
    {
        while (player != null)
        {
            agent.SetDestination(player.transform.position);
            yield return new WaitForSeconds(0.3f);
        }
    }
    public void Desapear()
    {
        maze.enemies.Remove(this.gameObject);
        Destroy(this.gameObject);
        this.gameObject.GetComponent<NetworkObject>().Despawn();
    }
}
