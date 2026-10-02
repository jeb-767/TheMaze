using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;


public class Maze_Generator_Devblog : MonoBehaviour
{
    // Dimensions of the maze (must be odd)
    public int width;
    public int height;

    [Header("Maze Prefabs")]

    public List<GameObject> walls = new List<GameObject>();
    public List<GameObject> doors_list = new List<GameObject>();
    public List<bool> Doors_Name = new List<bool>();
    public List<GameObject> keys = new List<GameObject>();
    public GameObject wallPrefab_Destroy;
    public GameObject wallPrefab_Break;
    public GameObject wallPrefab_Border;
    public GameObject entrance_object;
    public GameObject exit_object;

    public GameObject floorPrefab;
    public GameObject stretchPrefab;
    public GameObject stretchPrefabLight;

    [Header("Traps")]

    public GameObject Trap1;
    public GameObject Trap2Prefab;

    [Header("Enemyes")]

    public List<GameObject> Bats_prefabs = new List<GameObject>();
    public List<GameObject> ghost = new List<GameObject>();
    public List<GameObject> skeleton = new List<GameObject>();
    public List<GameObject> spider = new List<GameObject>();
    public List<GameObject> golem = new List<GameObject>();
    public List<GameObject> orc = new List<GameObject>();




    [Header("Player")]
    public GameObject playerPrefab;

    public GameObject goal1;
    public GameObject goal2;

    // Size of the central empty zone (must be odd)
    public int emptyZoneSize;

    // Matrix to store the maze layout (0 = path, 1 = wall)
    public new int[,] maze;
    public Vector2Int entrance; // Entrance of the maze
    public Vector2Int exit; // Exit of the maze

    private int destroying_walls = 0;
    private int counter_walls_destroy = 0;
    private int number_of_walls = 0;
    private int counter_walls_destroy_distance = 0;

    private int doors = 0;
    private int counters_doors = 0;
    private int counter_doors_distance = 0;

    private int break_walls = 0;
    private int counter_walls_break = 0;
    private int counter_walls_break_distance = 0;

    private int trap2 = 0;
    private int counter_traps_2 = 0;
    private int counter_traps_2_distance = 0;

    private int counter_stretch_light_distance = 0;

    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public List<Vector2Int> OriginalemptyCells = new List<Vector2Int>();


    [SerializeField] private NavMeshSurface m_NavMeshSurface;
    void Awake()
    {
        width = GameManager.width;
        height = GameManager.height;
        emptyZoneSize = GameManager.center;
        m_NavMeshSurface.GetComponent<NavMeshSurface>();
        // Ensure maze dimensions and empty zone size are odd
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;
        if (emptyZoneSize % 2 == 0) emptyZoneSize++;

        // Initialize maze matrix and set entrance and exit points
        maze = new int[width, height];
        entrance = new Vector2Int(0, Random.Range(1, height - 2));
        exit = new Vector2Int(width - 1, Random.Range(1, height - 2));

        // Generate the maze, build it in the scene, and spawn the player
        GenerateMaze();
        DestroyWallAt(entrance.x, entrance.y);
        DestroyWallAt(entrance.x + 1, entrance.y);
        DestroyWallAt(exit.x - 1, exit.y);
        BuildMaze();
        SpawnPlayer(playerPrefab, new Vector3(entrance.x, 0.8f, entrance.y) ,0 , 0 , 0);
        m_NavMeshSurface.BuildNavMesh();
        if ((int)(width * height) / 100 != 0)
        {
            int number = Random.Range(0, (int)(width * height) / 100);
            for (int i = 0; i <= number; i++)
            {
                Instantiate_Object(Trap1, 0);
            }
        }
    }

    void GenerateMaze()
    {
        // Initialize the maze with walls (1)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    maze[x, y] = 2; //Side Wall
                }
                else
                {
                    maze[x, y] = 1;
                }
            }
        }

        // Reserve space for the central empty zone

        // Set entrance and exit as paths (0)
        maze[exit.x, exit.y] = 0;
        maze[entrance.x, entrance.y] = 0;

        // Start carving paths from the top-left corner of the maze
        CarvePath(1, 1);

        // Example of destroying a wall at a specific position (adjust coordinates as needed)

        ClearCenterZone();
        Count_Number_Of_Walls();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    emptyCells.Add(new Vector2Int(x, y));
                }
            }
        }
        OriginalemptyCells = emptyCells;
    }

    void ClearCenterZone()
    {
        // Define the central zone based on its size
        int centerX = width / 2;
        int centerY = height / 2;
        int halfZone = emptyZoneSize / 2;

        // Clear cells in the central zone
        for (int x = centerX - halfZone; x <= centerX + halfZone; x++)
        {
            for (int y = centerY - halfZone; y <= centerY + halfZone; y++)
            {
                maze[x, y] = 0; // Mark as path
            }
        }

    }

    void CarvePath(int x, int y)
    {
        // Mark the current cell as a path
        maze[x, y] = 0;
        // Define possible directions for carving paths
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Shuffle(directions); // Randomize directions

        // Try carving in each direction
        foreach (Vector2Int dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            // Check if the next cell is within bounds and can be carved
            if (IsInBounds(nx, ny) && maze[nx, ny] == 1)
            {
                // Open a wall and move to the next cell
                maze[x + dir.x, y + dir.y] = 0;
                CarvePath(nx, ny);
            }
        }
    }


    void Count_Number_Of_Walls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 1)
                {
                    number_of_walls++;
                }

            }
        }
    }

    void BuildMaze()
    {
        destroying_walls = (int)number_of_walls / 100;
        int distance_destroy_walls = Random.Range(85, 115);
        doors = (int)((height * width) / 500);
        int distance_doors = Random.Range(200, 300);
        trap2 = (int)((height * width) / 200);
        int distance_trap2 = Random.Range(150, 250);
        int distance_stretch_light = 50;
        break_walls = (int)number_of_walls / 100;
        int distance_break_walls = Random.Range(85, 115);
        // Create the maze in the scene  
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Instantiate wall or floor based on the maze matrix
                if (maze[x, y] == 1)
                {
                    GameObject wallPrefab = walls[0];
                    int PrefabNumber = Random.Range(0, 4);
                    if (PrefabNumber == 2 && counter_walls_destroy <= destroying_walls && distance_destroy_walls <= counter_walls_destroy_distance)
                    {
                        wallPrefab = wallPrefab_Destroy;
                        Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        Vector3 position_stretch = new Vector3(x, (float)1.7, y);
                        GameObject Stretch = Instantiate(stretchPrefab, position_stretch, Quaternion.identity);
                        Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                        counter_walls_destroy++;
                        distance_destroy_walls = Random.Range(100 - 15, 100 + 15);
                        counter_walls_destroy_distance = 0;
                    }

                    else if (PrefabNumber == 1 && counters_doors <= doors && distance_doors <= counter_doors_distance)
                    {
                        int DoorPrefabNumber = Random.Range(0, doors_list.Count);
                        if (DoorPrefabNumber == 0)
                        {
                            wallPrefab = doors_list[DoorPrefabNumber];
                        }
                        else
                        {
                            while (true)
                            {
                                if (Doors_Name[DoorPrefabNumber - 1] == false)
                                {
                                    wallPrefab = doors_list[DoorPrefabNumber];
                                    Doors_Name[DoorPrefabNumber - 1] = true;
                                    Instantiate_Object(keys[DoorPrefabNumber - 1], (float)0.2);
                                    break;
                                }
                                else
                                {
                                    DoorPrefabNumber = Random.Range(0, doors_list.Count);
                                    if (DoorPrefabNumber == 0)
                                    {
                                        wallPrefab = doors_list[DoorPrefabNumber];
                                        break;
                                    }
                                }
                            }

                        }

                        counters_doors++;
                        distance_doors = Random.Range(400, 500);
                        counter_doors_distance = 0;
                        Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        GameObject Stretch = Instantiate(stretchPrefab, new Vector3(x, (float)1.7, y), Quaternion.identity);
                        Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);

                    }
                    else if(PrefabNumber == 3 && counter_walls_break <= break_walls && distance_break_walls <= counter_walls_break_distance)
                    {
                        Debug.Log("Break");
                        wallPrefab = wallPrefab_Break;
                        Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        Vector3 position_stretch = new Vector3(x, (float)1.7, y);
                        GameObject Stretch = Instantiate(stretchPrefab, position_stretch, Quaternion.identity);
                        Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                        counter_walls_break++;
                        distance_break_walls = Random.Range(85, 115);
                        counter_walls_break_distance = 0;
                    }
                    else if (PrefabNumber == 0)
                    {
                        int list = Random.Range(0, walls.Count);
                        wallPrefab = walls[list];
                    }

                    counter_walls_destroy_distance++;
                    counter_doors_distance++;
                    counter_walls_break_distance++;
                    Vector3 position = new Vector3(x, (float)0.85, y);
                    Instantiate(wallPrefab, position, Quaternion.identity);

                }

                else if (maze[x, y] == 0)
                {
                    int PrefabNumber_floor = Random.Range(0, 2);
                    int PrefabNumber_stretch = Random.Range(0, 2);
                    Vector3 position = new Vector3(x, (float)1.7, y);

                    if (PrefabNumber_stretch == 1 && distance_stretch_light <= counter_stretch_light_distance)
                    {
                        GameObject Stretch = Instantiate(stretchPrefabLight, position, Quaternion.identity);
                        Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                        counter_stretch_light_distance = 0;
                    }
                    else
                    {
                        GameObject Stretch = Instantiate(stretchPrefab, position, Quaternion.identity);
                        Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                        counter_stretch_light_distance++;
                    }



                    if (PrefabNumber_floor == 1 && counter_traps_2 < trap2 && distance_trap2 <= counter_traps_2_distance && trap2 != 0)
                    {
                        GameObject Trap2 = Instantiate(Trap2Prefab, new Vector3(x, -0.05f, y), Quaternion.identity);
                        Trap2.transform.rotation = Quaternion.Euler(90, 0, 0);
                        counter_traps_2++;
                        distance_trap2 = Random.Range(200, 450);
                        counter_traps_2_distance = 0;
                    }
                    else
                    {
                        Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                        counter_traps_2_distance++;
                    }
                }
                else if (maze[x, y] == 2)
                {
                    Vector3 position = new Vector3(x, (float)0.85, y);
                    Instantiate(wallPrefab_Border, position, Quaternion.identity);
                }
            }
        }
        
    }

    public void SpawnPlayer(GameObject prefab, Vector3 position, int x , int y , int z)
    {
        // Spawn the player at the entrance // Adjust height if needed
        GameObject obj = Instantiate(prefab, position, Quaternion.identity);
        obj.transform.rotation = Quaternion.Euler(x, y, z);
    }

    bool IsInBounds(int x, int y)
    {
        // Check if a position is within the maze bounds
        return x > 0 && x < width - 1 && y > 0 && y < height - 1;
    }

    void Shuffle(Vector2Int[] array)
    {
        // Randomly shuffle an array of directions
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(0, array.Length);
            Vector2Int temp = array[rnd];
            array[rnd] = array[i];
            array[i] = temp;
        }
    }

    void DestroyWallAt(int x, int y)
    {
        // Destroy a wall at the specified position if valid
        if (maze[x, y] == 1)
        {
            maze[x, y] = 0; // Mark the cell as a path
        }
    }

    public void Instantiate_Object(GameObject prefab, float altura)
    {
        int number = Random.Range(0, emptyCells.Count);
        Vector2Int randomCell = emptyCells[number];
        Instantiate(prefab, new Vector3(randomCell.x, altura, randomCell.y), Quaternion.identity);
        emptyCells.RemoveAt(number); // Borra ese elemento si existe
    }
}