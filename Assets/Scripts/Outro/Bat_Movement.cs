using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;

public class Bat_Movement : MonoBehaviour
{
    public GameObject[] goalLocations;
    private UnityEngine.AI.NavMeshAgent agent;
    public Transform mesh;
    public Transform rig;
    public GameObject bat;

    void Start()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = Random.Range(4f, 7f);

        goalLocations = GameObject.FindGameObjectsWithTag("goal");

        int i = Random.Range(0, goalLocations.Length);
        agent.SetDestination(goalLocations[0].transform.position);
    }

    void Update()
    {
        Vector3 vector = new Vector3(transform.position.x - goalLocations[0].transform.position.x ,  transform.position.y - goalLocations[0].transform.position.y , transform.position.z - goalLocations[0].transform.position.z);
        float modulo = vector.sqrMagnitude;
        if (modulo <= 1)
        {
            Destroy(bat);
        }
        if(agent.velocity.sqrMagnitude <= 1f)
        {
            StartCoroutine("WaitForDestroy");
        }
    }

    public IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(10);
        if(agent.velocity.sqrMagnitude <= 1f)
        {
            Destroy(bat);
        }
        
    }
}
