using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class Maze_Generator : NetworkBehaviour
{
    [System.Serializable]
    public class Wall
    {
        public List<GameObject> row;
    }
    // Dimensions of the maze (must be odd)
    public int width;
    public int height;

    [Header("Maze Prefabs")]

    public List<Wall> walls = new List<Wall>();
    public List<GameObject> doors_list = new List<GameObject>();
    public List<bool> Doors_Name = new List<bool>();
    public List<GameObject> keys = new List<GameObject>();
    public List<GameObject> wallPrefab_Destroy = new List<GameObject>();
    public List<GameObject> wallPrefab_Break = new List<GameObject>();
    public List<GameObject> wallPrefab_Chest = new List<GameObject>();
    public GameObject wallPrefab_Border;
    public List<GameObject> entrance_object = new List<GameObject>();
    public List<GameObject> exit_object = new List<GameObject>();

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
    public List<GameObject> goblins = new List<GameObject>();


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

    private int chest_walls = 0;
    private int counter_walls_chest = 0;
    private int counter_walls_chest_distance = 0;

    private int trap2 = 0;
    private int counter_traps_2 = 0;
    private int counter_traps_2_distance = 0;

    private int counter_stretch_light_distance = 0;

    public List<Vector2Int> emptyCells = new List<Vector2Int>();
    public List<Vector2Int> OriginalemptyCells = new List<Vector2Int>();
    public List<GameObject> enemies = new List<GameObject>();
    public Canvas_Controller canvas;
    public int WallType = 0;
    [SerializeField] private NavMeshSurface m_NavMeshSurface;
    public int minSpecialZones = 0;
    public Vector3 spawnPos = new Vector3(0f, 0f ,0f);
    // Esta variable sincronizará la semilla automáticamente con todos los jugadores
    public NetworkVariable<int> mazeSeed = new NetworkVariable<int>(
        0, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );
    [Header("Sistema de Mapa Dinámico")]
    public RawImage contenedorUI_Mapa;   // El RawImage de tu Canvas donde se mostrará el mapa entero
    public Camera camaraEscaneoMapa;     // Una cámara dedicada apuntando hacia abajo (Orthographic)
    public RenderTexture rtCasilla;       // Una RenderTexture cuadrada (ej: de 32x32 píxeles)
    public LayerMask capasAIgnorarEnMapa; // Capas que la cámara no debe fotografiar (Techos, Jugador, Enemigos)
    private Texture2D texturaMaestraMapa;

    public GameObject mapExpandPrefab; // Prefab del objeto que al interactuar expande el mapa

    public GameObject antChild; // Prefab del objeto que al interactuar expande el mapa
    public bool Ant = false;

    public int skeletonCount, spiderCount, golemCount, orcCount, goblinsCount, enemyCount;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Nos suscribimos al evento que avisa cuando TODOS terminaron de cargar
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        }
    }
    private void OnSceneLoaded(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        // Solo generamos si la escena que cargó fue la tuya
        if (sceneName == "PrincipalScene") 
        {
            if (IsServer)
            {
                if(GameManager.changedSeed)
                {
                    mazeSeed.Value = GameManager.seed;
                }
                else
                {
                    // 1. El servidor inventa una semilla aleatoria (ej: 481516)
                    mazeSeed.Value = Random.Range(1, 9999999); 
                }
                // 2. Le decimos a todos los clientes (incluido el Host) que generen el laberinto
                GenerateMazeClientRpc(mazeSeed.Value);
            }
        }
    }
    [ClientRpc]
    private void GenerateMazeClientRpc(int seed)
    {
        // ¡LA MAGIA OCURRE AQUÍ! 
        // Obligamos a Unity a usar esta semilla. A partir de esta línea, 
        // todos los Random.Range darán exactamente los mismos resultados en todas las PCs.
        Random.InitState(seed);

        // Ahora llamamos a tu función original
        StartCoroutine(GenerateAllMaze());
    }
    public override void OnNetworkDespawn()
    {
        // Por limpieza, quitamos el evento al salir
        if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }
    public void SpawnNetworkPlayers()
    {
        if (!IsServer) return;

        spawnPos = new Vector3(entrance.x - 3f + Random.Range(-0.5f, 0.5f), 0f, entrance.y + Random.Range(-0.5f, 0.5f));
        GameObject officialPlayerPrefab = NetworkManager.Singleton.NetworkConfig.PlayerPrefab;

        if (officialPlayerPrefab == null) return;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            // 1. Creamos el muñeco
            GameObject playerInstance = Instantiate(officialPlayerPrefab, spawnPos, Quaternion.identity);
            
            // 2. Le decimos a la Red que es de este cliente
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            // 3. ¡LA MAGIA! Obligamos al cliente a teletransportarse para que no se quede en (0,0,0)
            Player playerScript = playerInstance.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TeleportPlayerClientRpc(spawnPos);
            }
        }
    }
    public IEnumerator GenerateAllMaze()
    {
        WallType = Random.Range(0, walls.Count);
        width = GameManager.width;
        height = GameManager.height;
        emptyZoneSize = GameManager.center;
        if (emptyZoneSize == height || emptyZoneSize == width)
        {
            emptyZoneSize -= 1;
        }
        m_NavMeshSurface.GetComponent<NavMeshSurface>();
        // Ensure maze dimensions and empty zone size are odd
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;
        if (emptyZoneSize % 2 == 0) emptyZoneSize++;

        // Initialize maze matrix and set entrance and exit points
        maze = new int[width, height];
        entrance = new Vector2Int(0, Random.Range(1, height - 2));
        exit = new Vector2Int(width - 1, Random.Range(1, height - 2));
        if(GameManager.changedSpecialZones)
        {
            minSpecialZones = GameManager.specialZones;
        }
        else
        {
            minSpecialZones = (int)((height * width) / 300);
        }
        // Generate the maze, build it in the scene, and spawn the player
        SpawnPlayer(entrance_object[WallType], new Vector3(entrance.x - 3f, 0f, entrance.y), 0, 0, 0);
        GenerateMaze();
        DestroyWallAt(entrance.x, entrance.y);
        DestroyWallAt(entrance.x + 1, entrance.y);
        DestroyWallAt(exit.x - 1, exit.y);
        BuildMaze();
        yield return null;
        //*SpawnPlayer(doors_list[0], new Vector3(entrance.x, 0f, entrance.y), 0, 0, 0);
        SpawnPlayer(exit_object[WallType], new Vector3(exit.x, 0f, exit.y), 0, 180, 0);
        //SpawnPlayer(playerPrefab, new Vector3(entrance.x - 3f, 0.8f, entrance.y), 0, 0, 0);
        SpawnPlayer(goal1, new Vector3(exit.x, 1, exit.y), 0, 0, 0);
        
        for(int i = 0; i <= 5; i++)
        {
            Instantiate_Object(mapExpandPrefab, 0.2f);
        }
        int numberEnemies = 0;
        if(GameManager.changedEnemies)
        {
            numberEnemies = GameManager.enemies;
        }
        else
        {
            numberEnemies = Random.Range((int)(width * height) / 175, (int)(width * height) / 200);
        }
        if(IsServer)
        {
            StartCoroutine(Bat_Spawn());
            StartCoroutine(Ghost_Spawn());
            if ((int)(width * height) / 100 != 0)
            {
                int number = Random.Range(0, (int)(width * height) / 100);
                for (int i = 0; i <= number; i++)
                {
                    Instantiate_Object(Trap1, 0);
                }
            }
            for (int i = 0; i <= numberEnemies; i++)
            {
                int Enemy = Random.Range(0, 5);
                switch (Enemy)
                {
                    case 0:
                        Instantiate_Object(skeleton[Random.Range(0, skeleton.Count)], 0.2f);
                        skeletonCount++;
                        break;
                    case 1:
                        Instantiate_Object(spider[Random.Range(0, spider.Count)], 0.2f);
                        spiderCount++;
                        break;
                    case 2:
                        Instantiate_Object(golem[Random.Range(0, golem.Count)], 00.2f);
                        golemCount++;
                        break;
                    case 3:
                        Instantiate_Object(orc[Random.Range(0, orc.Count)], 0.2f);
                        orcCount++;
                        break;
                    case 4:
                        Instantiate_Object(goblins[Random.Range(0, goblins.Count)], 0.2f);
                        goblinsCount++;
                        break;
                }
                enemyCount = skeletonCount + spiderCount + golemCount + orcCount + goblinsCount;
            }

        }
        SpawnNetworkPlayers();
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
        GenerateSpecialZones();
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
        if(Ant == true)
        {
            Instantiate_Object(antChild, 0.2f);
        }
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
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));
        maze[x, y] = 0;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            Shuffle(directions); // Tu función actual de barajar
            bool moved = false;

            foreach (Vector2Int dir in directions)
            {
                int nx = current.x + dir.x * 2;
                int ny = current.y + dir.y * 2;

                if (IsInBounds(nx, ny) && maze[nx, ny] == 1)
                {
                    // Rompemos la pared y avanzamos
                    maze[current.x + dir.x, current.y + dir.y] = 0;
                    maze[nx, ny] = 0;
                    stack.Push(new Vector2Int(nx, ny));
                    moved = true;
                    break;
                }
            }

            // Si estamos en un callejón sin salida, retrocedemos
            if (!moved)
            {
                stack.Pop();
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
        GameObject mazeRoot = new GameObject("Laberinto_Estatico");
        destroying_walls = (int)number_of_walls / 100;
        int distance_destroy_walls = Random.Range(85, 115);
        doors = (int)((height * width) / 400);
        int distance_doors = Random.Range(200, 300);
        trap2 = (int)((height * width) / 200);
        int distance_trap2 = Random.Range(150, 250);
        int distance_stretch_light = 50;
        break_walls = (int)number_of_walls / 100;
        int distance_break_walls = Random.Range(85, 115);
        chest_walls = (int)number_of_walls / 500;
        int distance_chest_walls = Random.Range(150, 250);

        // Create the maze in the scene  
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Instantiate wall or floor based on the maze matrix
                if (maze[x, y] == 1)
                {
                    GameObject wallPrefab = walls[WallType].row[0];
                    int PrefabNumber = Random.Range(0, 5);
                    if((maze[x , y + 1 ] == 1 && maze[x - 1 , y] == 1) || (maze[x , y - 1 ] == 1 && maze[x - 1 , y] == 1) || (maze[x , y + 1 ] == 1 && maze[x + 1 , y] == 1) || (maze[x , y - 1 ] == 1 && maze[x + 1 , y] == 1))
                    {
                        //esquina del laberinto, no queremos que tenga puertas ni paredes especiales
                        int list = Random.Range(0, walls[WallType].row.Count);
                        wallPrefab = walls[WallType].row[list];
                    }
                    else
                    {
                        if (PrefabNumber == 2 && counter_walls_destroy <= destroying_walls && distance_destroy_walls <= counter_walls_destroy_distance)
                        {
                            wallPrefab = wallPrefab_Destroy[WallType];
                            GameObject floor = Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                            if(floor.TryGetComponent<NetworkObject>(out NetworkObject netObjFloor))
                            {
                                if (IsServer) netObjFloor.Spawn();
                                else Destroy(floor);
                            }
                            else
                            {
                                floor.transform.SetParent(mazeRoot.transform);
                            }
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
                            GameObject floor = Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                            GameObject Stretch = Instantiate(stretchPrefab, new Vector3(x, (float)1.7, y), Quaternion.identity);
                            Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                        }
                        else if (PrefabNumber == 3 && counter_walls_break <= break_walls && distance_break_walls <= counter_walls_break_distance)
                        {
                            wallPrefab = wallPrefab_Break[WallType];
                            GameObject floor = Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                            Vector3 position_stretch = new Vector3(x, (float)1.7, y);
                            GameObject Stretch = Instantiate(stretchPrefab, position_stretch, Quaternion.identity);
                            Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                            counter_walls_break++;
                            distance_break_walls = Random.Range(85, 115);
                            counter_walls_break_distance = 0;
                        }
                        else if (PrefabNumber == 4 && counter_walls_chest <= chest_walls && distance_chest_walls <= counter_walls_chest_distance)
                        {
                            wallPrefab = wallPrefab_Chest[WallType];
                            GameObject floor = Instantiate(floorPrefab, new Vector3(x, 0, y), Quaternion.identity);
                            Vector3 position_stretch = new Vector3(x, (float)1.7, y);
                            GameObject Stretch = Instantiate(stretchPrefab, position_stretch, Quaternion.identity);
                            Stretch.transform.rotation = Quaternion.Euler(180, 0, 0);
                            counter_walls_chest++;
                            distance_chest_walls = Random.Range(150, 250);
                            counter_walls_chest_distance = 0;
                        }
                        else if (PrefabNumber == 0)
                        {
                            int list = Random.Range(0, walls[WallType].row.Count);
                            wallPrefab = walls[WallType].row[list];
                        }
                    }
                    counter_walls_destroy_distance++;
                    counter_doors_distance++;
                    counter_walls_break_distance++;
                    counter_walls_chest_distance++;
                    Vector3 position = new Vector3(x, 0.85f, y);
                    GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
                    RotateWall(wall, x, y);

                    // ========================================================
                    // CAMBIO 1: REGISTRO DE MUROS
                    // ========================================================
                    if (wall.TryGetComponent<NetworkObject>(out NetworkObject netObj))
                    {
                        if (IsServer) 
                        {
                            Debug.Log(wall.name + " spawned at: " + position);
                            netObj.Spawn();
                        }
                        else
                        {
                            Destroy(wall); // El cliente destruye y NO lo añade a la lista
                        }
                    }
                    else
                    {
                        wall.transform.SetParent(mazeRoot.transform);
                    }
                }
                else if (maze[x, y] == 0)
                {
                    // --- 1. GESTIÓN DEL TECHO (Stretch) ---
                    int PrefabNumber_stretch = Random.Range(0, 2);
                    Vector3 position_stretch = new Vector3(x, 1.7f, y);
                    GameObject currentStretchPrefab = stretchPrefab; // Por defecto

                    if (PrefabNumber_stretch == 1 && distance_stretch_light <= counter_stretch_light_distance)
                    {
                        currentStretchPrefab = stretchPrefabLight;
                        counter_stretch_light_distance = 0;
                    }
                    else
                    {
                        counter_stretch_light_distance++;
                    }

                    GameObject instStretch = Instantiate(currentStretchPrefab, position_stretch, Quaternion.identity);
                    instStretch.transform.rotation = Quaternion.Euler(180, 0, 0);

                    // ========================================================
                    // CAMBIO 2: REGISTRO DE TECHOS
                    // ========================================================
                    if (instStretch.TryGetComponent<NetworkObject>(out NetworkObject netObjStretch))
                    {
                        if (IsServer) 
                        {
                            netObjStretch.Spawn();
                        }
                        else 
                        {
                            Destroy(instStretch); 
                        }
                    }
                    else
                    {
                        instStretch.transform.SetParent(mazeRoot.transform);
                    }

                    // --- 2. GESTIÓN DEL SUELO (Floor y Trampas) ---
                    int PrefabNumber_floor = Random.Range(0, 2);
                    GameObject currentFloorPrefab = floorPrefab; // Por defecto
                    Vector3 position_floor = new Vector3(x, 0f, y);
                    Quaternion rotation_floor = Quaternion.identity;

                    if (PrefabNumber_floor == 1 && counter_traps_2 < trap2 && distance_trap2 <= counter_traps_2_distance && trap2 != 0)
                    {
                        currentFloorPrefab = Trap2Prefab;
                        position_floor = new Vector3(x, 0f, y);
                        rotation_floor = Quaternion.Euler(90, 0, 0);
                        
                        counter_traps_2++;
                        distance_trap2 = Random.Range(200, 450);
                        counter_traps_2_distance = 0;
                    }
                    else
                    {
                        counter_traps_2_distance++;
                    }

                    GameObject instFloor = Instantiate(currentFloorPrefab, position_floor, rotation_floor);

                    // ========================================================
                    // CAMBIO 3: REGISTRO DE SUELOS Y TRAMPAS
                    // ========================================================
                    if (instFloor.TryGetComponent<NetworkObject>(out NetworkObject netObjFloor))
                    {
                        if (IsServer) 
                        {
                            netObjFloor.Spawn();
                        }
                        else 
                        {
                            Destroy(instFloor);
                        }
                    }
                    else
                    {
                        instFloor.transform.SetParent(mazeRoot.transform);
                    }
                }
                else if (maze[x, y] == 2)
                {
                    Vector3 position = new Vector3(x, (float)0.85, y);
                    GameObject wall = Instantiate(walls[WallType].row[0], position, Quaternion.identity);
                    wall.transform.SetParent(mazeRoot.transform);
                }
            }
        }
        
        m_NavMeshSurface.BuildNavMesh();
        StaticBatchingUtility.Combine(mazeRoot);
        
    }
    public void RotateWall(GameObject wall, int x , int y)
    {
        // Rotate the wall based on the given rotation value
        if (wall.TryGetComponent<Rotate_Wall>(out Rotate_Wall rotateScript))
        {
            // Calculamos qué hay alrededor comprobando la matriz
            // Nos aseguramos de no salirnos de los bordes del array
            bool forward = (y + 1 < height) && maze[x, y + 1] != 0; 
            bool back = (y - 1 >= 0) && maze[x, y - 1] != 0;
            bool right = (x + 1 < width) && maze[x + 1, y] != 0;
            bool left = (x - 1 >= 0) && maze[x - 1, y] != 0;

            // Le pasamos la información al script
            rotateScript.CalculateRotation(forward, back, right, left);
        }
    }
    public void SpawnPlayer(GameObject prefab, Vector3 position, int x, int y, int z)
    {
        // Spawn the player at the entrance // Adjust height if needed
        
        if(prefab.GetComponent<NetworkObject>() != null)
        {
            if(IsServer)
            {
                GameObject obj = Instantiate(prefab, position, Quaternion.identity);
                obj.transform.rotation = Quaternion.Euler(x, y, z);
                obj.GetComponent<NetworkObject>().Spawn();
            //culling.EscanearYRegistrarObjetoCompleto(obj);
            }
        }
        else
        {
            GameObject obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.rotation = Quaternion.Euler(x, y, z);
            //culling.EscanearYRegistrarObjetoCompleto(obj);
        }
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
        int number = Random.Range(0, emptyCells.Count - 1);
        Vector2Int randomCell = emptyCells[number];
        GameObject obj = Instantiate(prefab, new Vector3(randomCell.x, altura, randomCell.y), Quaternion.identity);
        if (obj.TryGetComponent<NetworkObject>(out NetworkObject netObj))
        {
            netObj.Spawn();
        }
        int lastIndex = emptyCells.Count - 1;
        emptyCells[number] = emptyCells[lastIndex]; // Movemos el último al hueco que dejamos
        emptyCells.RemoveAt(lastIndex); // Borramos el último (coste cero)
        //culling.EscanearYRegistrarObjetoCompleto(obj); // Registramos el nuevo objeto para culling
    }

    public IEnumerator Bat_Spawn()
    {
        yield return new WaitForSeconds(Random.Range(15, 30));
        int random = Random.Range(1, 10);
        for (int i = 0; i <= random; i++)
        {
            SpawnPlayer(Bats_prefabs[Random.Range(0, Bats_prefabs.Count - 1)], new Vector3(entrance.x + 0.5f, 1, entrance.y), 0, 0, 0);
        }

        yield return new WaitForSeconds(150);
        StartCoroutine("Bat_Spawn");

    }

    public IEnumerator Ghost_Spawn()
    {
        yield return new WaitForSeconds(300);
        SpawnPlayer(ghost[Random.Range(0, ghost.Count - 1)], new Vector3(entrance.x + 0.5f, 0, entrance.y), 0, 0, 0);
        canvas.ShowAdvise("A Ghost has appeared, scape from it", Color.red);
        StartCoroutine("Ghost_Spawn");
    }

    [System.Serializable]
    public class SpecialZone
    {
        public string name;
        public GameObject prefab; // Prefab central (ej: mesa de crafteo)
    }

    public List<SpecialZone> specialZones = new List<SpecialZone>();
    public List<SpecialZone> otherZones = new List<SpecialZone>();

    // Cantidad de zonas que quieres generar (depende del tamaño del laberinto)
    private List<Vector2Int> usedZones = new List<Vector2Int>();

    void GenerateSpecialZones()
    {
        // 1. Creamos una copia de la lista original para poder manipularla sin romper los datos originales
        List<SpecialZone> availableZones = new List<SpecialZone>(specialZones);

        // Determinar cuántas zonas colocar (no puede ser mayor al total de tipos que tenemos)
        int zonesToPlace = Mathf.Min(minSpecialZones, availableZones.Count);

        for (int i = 0; i < zonesToPlace; i++)
        {
            Vector2Int pos = FindValidZonePosition();

            if (pos != new Vector2Int(-1, -1))
            {
                // 2. Elegimos un índice aleatorio dentro de lo que queda en la lista temporal
                int randomIndex = Random.Range(0, availableZones.Count);
                SpecialZone zone = availableZones[randomIndex];
                // Vaciar un bloque de $3 \times 3$
                InstantiteZones(zone , pos);
                // 3. REGLA CLAVE: Eliminamos la zona de la lista temporal para que no se repita
                availableZones.RemoveAt(randomIndex);

                // Guardamos la posición para no repetir ubicación
                usedZones.Add(pos);
                Debug.Log("Zona " + zone.name + " colocada en: " + pos);
                if(zone.name == "Portal")
                {
                    Vector2Int pos2 = FindValidZonePosition();
                    zone = otherZones[0]; // Asumimos que el portal es la primera zona de la lista de otras zonas
                    InstantiteZones(zone , pos2);
                    usedZones.Add(pos2);
                }
                else if(zone.name == "Ant")
                {
                    Ant = true;
                }

            }
        }
    }
    void InstantiteZones(SpecialZone zone, Vector2Int pos)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if(pos.x + x < 0 || pos.x + x >= width || pos.y + y < 0 || pos.y + y >= height)
                    continue; // Evitamos salirnos de los límites del laberinto
                int px = pos.x + x;
                int py = pos.y + y;
                maze[px, py] = 0; // lo convertimos en camino
            }
        }

        // Colocar el prefab de la zona elegida
        Vector3 worldPos = new Vector3(pos.x, 0, pos.y);
        GameObject obj = Instantiate(zone.prefab, worldPos, Quaternion.identity);
        obj.GetComponent<NetworkObject>().Spawn();
    }
    Vector2Int FindValidZonePosition()
    {
        for (int attempts = 0; attempts < 100; attempts++)
        {
            int x = Random.Range(5, width - 5);
            int y = Random.Range(5, height - 5);
            Vector2Int pos = new Vector2Int(x, y);

            // Evitar zonas muy cerca del centro (cartografía) o repetidas
            if (Vector2Int.Distance(pos, new Vector2Int(width / 2, height / 2)) <= (emptyZoneSize / 2) + 15)
                continue;

            // Evitar que esté demasiado cerca de otras zonas ya usadas
            bool tooClose = false;
            foreach (Vector2Int used in usedZones)
            {
                if (Vector2Int.Distance(pos, used) <= 15)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose)
                continue;
            // Comprobar que alrededor hay paredes para que se note la zona
            bool valid = true;

            if (valid)
            {
                return pos;
            }
        }

        return new Vector2Int(-1, -1); // no se encontró nada válido
    }
    
}
