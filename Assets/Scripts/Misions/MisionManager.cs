using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode; // NUEVO: Necesario para NetworkBehaviour y RPCs

// 1. Heredamos de NetworkBehaviour en lugar de MonoBehaviour
public class MissionManager : NetworkBehaviour
{
    public static MissionManager Instance;

    public List<ActiveMission> activeMissions = new List<ActiveMission>();
    public Canvas_Controller canvasController;

    // NUEVO: Una base de datos con todas las misiones del juego
    // Debes arrastrar todos tus ScriptableObjects MisionsBase aquí desde el Inspector
    [Header("Database")]
    public List<MisionsBase> allMissionsDatabase = new List<MisionsBase>();
    public MisionsUI missionUI;
    public Player player;
    public ItemDatabase itemDatabase;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AcceptMission(MisionsBase missionData)
    {
        // 2. Evaluamos si es una misión compartida o individual
        if (missionData.isSharedMission)
        {
            // Avisamos al servidor para que la propague a todos. 
            // Pasamos el string 'name' porque Netcode no permite pasar ScriptableObjects directamente.
            AcceptSharedMissionServerRpc(missionData.name);
        }
        else
        {
            // Es individual, la procesamos solo en este cliente
            AcceptMissionLocally(missionData);
        }
    }

    // 3. El cliente que acepta la misión avisa al Servidor/Host
    [ServerRpc(RequireOwnership = false)]
    private void AcceptSharedMissionServerRpc(string missionName)
    {
        // El servidor ordena a todos los clientes conectados que acepten esta misión
        AcceptSharedMissionClientRpc(missionName);
    }

    // 4. Todos los clientes reciben la orden y buscan la misión en su base de datos local
    [ClientRpc]
    private void AcceptSharedMissionClientRpc(string missionName)
    {
        MisionsBase missionData = allMissionsDatabase.Find(m => m.name == missionName);
        
        if (missionData != null)
        {
            AcceptMissionLocally(missionData);
        }
        else
        {
            Debug.LogWarning($"No se encontró la misión {missionName} en la base de datos.");
        }
    }

    // 5. La lógica original que tenías para procesar la misión localmente
    private void AcceptMissionLocally(MisionsBase missionData)
    {
        bool alreadyActive = activeMissions.Exists(m => m.missionData == missionData);
        
        if (!alreadyActive)
        {
            ActiveMission newMission = new ActiveMission(missionData);
            activeMissions.Add(newMission);
            
            FindObjectOfType<MisionsUI>().GiveMision(missionData);
            canvasController.StartCoroutine(canvasController.ShowNewMisionAdvise(missionData.name, true));
        }
    }

    public void AddProgress(int objectiveId, int amount = 1)
    {
        bool isShared = false;

        // 1. Comprobamos si este objetivo pertenece a alguna misión activa que sea compartida
        foreach (ActiveMission mission in activeMissions)
        {
            if (mission.objectiveProgress.ContainsKey(objectiveId) && mission.missionData.isSharedMission)
            {
                isShared = true;
                break; // Con encontrar una es suficiente
            }
        }

        if (isShared)
        {
            // 2. Si es compartida, el jugador avisa al servidor para que reparta el progreso a todos
            AddSharedProgressServerRpc(objectiveId, amount);
        }
        else
        {
            // 3. Si es individual, solo sumamos progreso en la partida de este jugador
            AddProgressLocally(objectiveId, amount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddSharedProgressServerRpc(int objectiveId, int amount)
    {
        // El servidor recibe el aviso y lo retransmite a todos los clientes conectados
        AddSharedProgressClientRpc(objectiveId, amount);
    }

    [ClientRpc]
    private void AddSharedProgressClientRpc(int objectiveId, int amount)
    {
        // Todos los clientes reciben la orden y se aplican el progreso
        AddProgressLocally(objectiveId, amount);
    }

    // 4. Esta es la lógica local que tenías antes, pero ahora puede ser llamada por la red
    private void AddProgressLocally(int objectiveId, int amount)
    {
        bool progressMade = false;

        foreach (ActiveMission mission in activeMissions)
        {
            if (mission.objectiveProgress.ContainsKey(objectiveId))
            {
                int previousProgress = mission.objectiveProgress[objectiveId]; 
                mission.objectiveProgress[objectiveId] += amount;
                int currentProgress = mission.objectiveProgress[objectiveId];
                
                progressMade = true;

                ObjectivesBase obj = mission.missionData.objectives.Find(o => o.id == objectiveId);
                
                if (obj != null && previousProgress < obj.number && currentProgress >= obj.number)
                {
                    if (canvasController != null)
                    {
                        canvasController.StartCoroutine(canvasController.ShowObjectiveAdvise(obj.name));
                    }
                }
            }
        }

        if (progressMade)
        {
            Debug.Log($"Se añadió progreso localmente al objetivo ID: {objectiveId}");
            missionUI.RefreshCurrentMissionUI();
            
            // Al chequear esto aquí, cada jugador completará la misión automáticamente 
            // cuando su cliente reciba el último punto de progreso por la red.
            CheckMissionsCompletion(); 
        }
    }
    private void CheckMissionsCompletion()
    {
        for (int i = activeMissions.Count - 1; i >= 0; i--)
        {
            ActiveMission mission = activeMissions[i];
            bool isMissionComplete = true;

            foreach (ObjectivesBase obj in mission.missionData.objectives)
            {
                int currentProgress = mission.objectiveProgress[obj.id]; 
                if (currentProgress < obj.number)
                {
                    isMissionComplete = false;
                    break; 
                }
            }
            if (isMissionComplete)
            {
                CompleteMission(mission.missionData);
                activeMissions.RemoveAt(i); 
                canvasController.StartCoroutine(canvasController.ShowNewMisionAdvise(mission.missionData.name, false));
            }
        }
    }

    private void CompleteMission(MisionsBase missionData)
    {
        RunManager.obtainedGold += missionData.reward2;
        player.ShowCoins(missionData.reward2);
        player.xp += missionData.reward3;
        player.ShowXP(missionData.reward3);
        if(missionData.normalReward)
        {
            for(int i = 0; i < missionData.reward1; i++)
            {
                int randomIndex = Random.Range(0, itemDatabase.allPrefabs.Count);
                GameObject randomItem = itemDatabase.allPrefabs[randomIndex];
                Vector3 dropPos = player.transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.4f, Random.Range(-0.5f, 0.5f));
                GameObject itemObject = Instantiate(randomItem, dropPos, Quaternion.identity);
                itemObject.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    public int GetObjectiveProgress(MisionsBase missionData, int objectiveId)
    {
        ActiveMission currentMission = activeMissions.Find(m => m.missionData == missionData);
        
        if (currentMission != null && currentMission.objectiveProgress.ContainsKey(objectiveId))
        {
            return currentMission.objectiveProgress[objectiveId];
        }
        return 0;
    }
}

[System.Serializable]
public class ActiveMission
{
    public MisionsBase missionData;
    // Este diccionario guarda el progreso SOLO para esta instancia de la misión
    public Dictionary<int, int> objectiveProgress; 

    public ActiveMission(MisionsBase data)
    {
        missionData = data;
        objectiveProgress = new Dictionary<int, int>();
        
        // Inicializamos los objetivos a 0 al momento de aceptar la misión
        foreach (ObjectivesBase obj in data.objectives)
        {
            objectiveProgress.Add(obj.id, 0);
        }
    }
}