using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
public class ConfigureQuestPanel : MonoBehaviour
{
    [System.Serializable]
    public class AvailableQuest
    {
        public MisionsBase missionData;
        public List<ObjectivesBase> objectivesData;
    }
    public List<AvailableQuest> availableQuests = new List<AvailableQuest>();
    public Maze_Generator maze;
    public MisionsUI misionUI;
    public List<ObjectivesBase> objectives = new List<ObjectivesBase>();
    public MisionInteract actualMission;
    public Canvas_Controller canvasController;
    public GameObject missionPanel;
    void Start()
    {
        maze = GameObject.FindObjectOfType<Maze_Generator>();
        for(int i = 0; i < 5; i++)
        {
            int objectivesNumber = 0;
            objectives.Clear();
            MisionsBase newMission = ScriptableObject.CreateInstance<MisionsBase>();
            int RandomIndex = Random.Range(0, availableQuests.Count);
            AvailableQuest selectedQuest = availableQuests[RandomIndex];
            foreach (ObjectivesBase obj in selectedQuest.objectivesData)
            {
                ObjectivesBase newObjective = ScriptableObject.CreateInstance<ObjectivesBase>();
                newObjective.id = obj.id;
                newObjective.name = obj.name;
                switch(newObjective.id)
                {
                    case 2:
                        newObjective.number = Random.Range(1, maze.skeletonCount);
                        break;
                    case 4:
                        newObjective.number = Random.Range(1, maze.spiderCount);
                        break;
                    case 3:
                        newObjective.number = Random.Range(1, maze.golemCount);
                        break;
                    case 5:
                        newObjective.number = Random.Range(1, maze.orcCount);
                        break;
                    case 6:
                        newObjective.number = Random.Range(1, maze.goblinsCount);
                        break;
                    case 1:
                        newObjective.number = Random.Range(1, maze.enemyCount);
                        break;
                }
                objectives.Add(newObjective);
                objectivesNumber += newObjective.number;
            }
            newMission.name = selectedQuest.missionData.name;
            newMission.description = selectedQuest.missionData.description;
            newMission.reward1 = objectivesNumber / 10;
            newMission.reward2 = objectivesNumber * 5;
            newMission.reward3 = objectivesNumber * 3;
            newMission.misionType = selectedQuest.missionData.misionType;
            newMission.misionCategory = selectedQuest.missionData.misionCategory;
            int number = Random.Range(1, objectives.Count + 1);
            for(int j = 0; j < number; j++)
            {
                int number2 = Random.Range(0, objectives.Count);
                newMission.objectives.Add(objectives[number2]);
                objectives[number2] = objectives[objectives.Count - 1];
                objectives.RemoveAt(objectives.Count - 1);
            }
            misionUI.GiveMision(newMission , true);
        }
        missionPanel.SetActive(false);
    }
    public void AcceptQuest()
    {
        if(actualMission != null)
        {
            canvasController.GiveMision(actualMission.mision);
            Destroy(actualMission.transform.gameObject);
            missionPanel.SetActive(false);
        }
    }
}

