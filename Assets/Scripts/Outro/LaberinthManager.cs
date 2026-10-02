using UnityEngine;
using System.Collections.Generic;

public class LabyrinthManager : MonoBehaviour
{
    public static LabyrinthManager Instance;
    public List<GameObject> sections = new List<GameObject>();
    public Dictionary<Vector2Int, GameObject> cellGroup = new Dictionary<Vector2Int, GameObject>();
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}