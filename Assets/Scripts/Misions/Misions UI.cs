using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
public class MisionsUI : MonoBehaviour
{
    public GameObject missionPanel;

    public GameObject missionListPanel;
    public TextMeshProUGUI missionName1;
    public TextMeshProUGUI missionDescription;
    public TextMeshProUGUI missionReward , missionReward2 , missionReward3;

    public GameObject missionListPanelQ;
    public TextMeshProUGUI missionName1Q;
    public TextMeshProUGUI missionDescriptionQ;
    public TextMeshProUGUI missionRewardQ , missionReward2Q , missionReward3Q;
    public RawImage missionImageQ;
    public GameObject objectivePanelQ;

    public GameObject missionPrefab;
    public GameObject objectivePrefab;
    public GameObject objectivePanel;
    public RawImage missionImage;
    private MisionsBase currentOpenMission;
    public Texture completedTexture; 
    public List<Texture> missionImages = new List<Texture>(); //0 explore , 1 kill, 2 collect , 3 talk
    public void Start()
    {
        missionPanel.SetActive(false);
    }
    public void GiveMision(MisionsBase mision, bool quest = false)
    {
        if(!quest)
        {
            ConfigureMisionDescription(mision);
        }
        else
        {
            ConfigureMisionDescriptionQ(mision);
        }
        ConfigureMision(mision, InstantiateMission(mision, quest));
    }
    public GameObject InstantiateMission(MisionsBase mision, bool quest = false)
    {
        GameObject newMission;
        if(quest)
        {
            newMission = Instantiate(missionPrefab, missionListPanelQ.transform);
            newMission.GetComponent<MisionInteract>().quest = true;
            newMission.transform.GetChild(1).GetComponent<RawImage>().texture = missionImageQ.texture;
        }
        else
        {
            newMission = Instantiate(missionPrefab, missionListPanel.transform);
            newMission.GetComponent<MisionInteract>().quest = false;
            newMission.transform.GetChild(1).GetComponent<RawImage>().texture = missionImage.texture;
        }
        return newMission;
    }
    public void ConfigureMision(MisionsBase mision , GameObject newMissionB)
    {
        GameObject newMission = newMissionB;
        newMission.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = mision.name;
        newMission.GetComponent<MisionInteract>().mision = mision;
        switch(mision.misionCategory)
        {
            case Category.Main:
                newMission.GetComponent<Image>().color = Color.lightYellow;
                break;
            case Category.Side:
                newMission.GetComponent<Image>().color = Color.lightBlue;
                break;
            case Category.Daily:
                newMission.GetComponent<Image>().color = Color.lightGreen;
                break;
        }
    }
    public void ConfigureObjective(ObjectivesBase objective, MisionsBase missionData)
    {
        GameObject newObjective = Instantiate(objectivePrefab, objectivePanel.transform);
        newObjective.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = objective.name;
        
        // Pasamos la misión y el objetivo
        int currentProgress = 0;
        if (MissionManager.Instance != null)
        {
            currentProgress = MissionManager.Instance.GetObjectiveProgress(missionData, objective.id);
        }
        
        newObjective.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = currentProgress.ToString();
        newObjective.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = objective.number.ToString();
        // NUEVO: Comprobar si el objetivo está completado para cambiar su imagen
        if (currentProgress >= objective.number)
        {
            // NOTA: Cambia el '0' por el índice del hijo donde tengas tu RawImage en el prefab del objetivo
            newObjective.transform.GetChild(0).GetComponent<RawImage>().texture = completedTexture;
        }
    }
    public void ConfigureObjectiveQ(ObjectivesBase objective, MisionsBase missionData)
    {
        GameObject newObjective = Instantiate(objectivePrefab, objectivePanelQ.transform);
        newObjective.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = objective.name;
        
        // Pasamos la misión y el objetivo
        int currentProgress = 0;
        if (MissionManager.Instance != null)
        {
            currentProgress = MissionManager.Instance.GetObjectiveProgress(missionData, objective.id);
        }
        
        newObjective.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = currentProgress.ToString();
        newObjective.transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = objective.number.ToString();
        // NUEVO: Comprobar si el objetivo está completado para cambiar su imagen
        if (currentProgress >= objective.number)
        {
            // NOTA: Cambia el '0' por el índice del hijo donde tengas tu RawImage en el prefab del objetivo
            newObjective.transform.GetChild(0).GetComponent<RawImage>().texture = completedTexture;
        }
    }
    public void RefreshCurrentMissionUI()
    {
        if (missionPanel.activeSelf && currentOpenMission != null)
        {
            ConfigureMisionDescription(currentOpenMission);
        }
    }
    public void ConfigureMisionDescription(MisionsBase mision)
    {
        currentOpenMission = mision; // <-- Añade esta línea al principio
        
        missionName1.text = mision.name;
        missionDescription.text = mision.description;
        missionReward.text = mision.reward1.ToString();
        missionReward2.text = mision.reward2.ToString();
        missionReward3.text = mision.reward3.ToString();
        switch(mision.misionType)
        {
            case MisionType.Collect:
                missionImage.texture = missionImages[2];
                break;
            case MisionType.Kill:
                missionImage.texture = missionImages[1];
                break;
            case MisionType.Explore:
                missionImage.texture = missionImages[0];
                break;
            case MisionType.Talk:
                missionImage.texture = missionImages[3];
                break;
        }
        CleanObjectives();
        foreach(ObjectivesBase obj in mision.objectives)
        {
            ConfigureObjective(obj, mision);
        } 
    }

    public void ConfigureMisionDescriptionQ(MisionsBase mision)
    {
        currentOpenMission = mision; // <-- Añade esta línea al principio
        
        missionName1Q.text = mision.name;
        missionDescriptionQ.text = mision.description;
        missionRewardQ.text = mision.reward1.ToString();
        missionReward2Q.text = mision.reward2.ToString();
        missionReward3Q.text = mision.reward3.ToString();
        switch(mision.misionType)
        {
            case MisionType.Collect:
                missionImageQ.texture = missionImages[2];
                break;
            case MisionType.Kill:
                missionImageQ.texture = missionImages[1];
                break;
            case MisionType.Explore:
                missionImageQ.texture = missionImages[0];
                break;
            case MisionType.Talk:
                missionImageQ.texture = missionImages[3];
                break;
        }
        CleanObjectivesQ();
        foreach(ObjectivesBase obj in mision.objectives)
        {
            ConfigureObjectiveQ(obj, mision);
        } 
    }
    public void CleanObjectives()
    {
        foreach(Transform child in objectivePanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void CleanObjectivesQ()
    {
        foreach(Transform child in objectivePanelQ.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void MarkMissionAsCompletedUI(MisionsBase missionData)
    {
        // Recorremos todos los prefabs instanciados en el panel de misiones
        foreach (Transform child in missionListPanel.transform)
        {
            MisionInteract interact = child.GetComponent<MisionInteract>();
            
            // Si encontramos el prefab que corresponde a esta misión
            if (interact != null && interact.mision == missionData)
            {
                // Cambiamos la imagen principal del prefab (cambia el '1' si tu RawImage está en otro índice)
                child.GetChild(3).GetComponent<RawImage>().texture = completedTexture;
                
                // Opcional: Cambiar el color de fondo para indicar que está terminada
                child.GetComponent<Image>().color = Color.gray; 
                break;
            }
        }
    }
}
