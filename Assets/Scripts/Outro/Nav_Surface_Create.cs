using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class Nav_Surface_Create : MonoBehaviour
{
    public NavMeshSurface m_NavMeshSurface;

    void Awake()
    {
        m_NavMeshSurface.GetComponent<NavMeshSurface>();
    }
    void Start()
    {
        m_NavMeshSurface.BuildNavMesh();
    }
}
