using UnityEngine;

public class ReflectionSetup : MonoBehaviour
{
    public Maze_Generator maze;
    public Transform transform;
    public ReflectionProbe probe;
    void Start()
    {
        transform.localScale = new Vector3(maze.width, 2, maze.height);
        transform.position = new Vector3(maze.width / 2 , 1 , maze.height /2);
        probe.RenderProbe();
    }
}
