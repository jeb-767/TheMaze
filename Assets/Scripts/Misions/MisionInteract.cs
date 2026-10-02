using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;

public class MisionInteract : MonoBehaviour , IPointerClickHandler, IPointerEnterHandler
{
    public bool newItem = true;
    public GameObject newItemIndicator;
    public MisionsUI misionUI;
    public MisionsBase mision;
    public bool quest;
    public ConfigureQuestPanel questConfiguration;
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (newItem == true)
        {
            newItemIndicator.SetActive(false);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (misionUI != null)
        {
            
            if(quest)
            {
                questConfiguration.missionPanel.SetActive(true);
                misionUI.ConfigureMisionDescriptionQ(mision);
                questConfiguration.actualMission = this;
            }
            else
            {
                misionUI.missionPanel.SetActive(true);
                misionUI.ConfigureMisionDescription(mision);
            }
        }
    }
    public void Start()
    {
        misionUI = FindObjectOfType<MisionsUI>();
        questConfiguration = FindObjectOfType<ConfigureQuestPanel>();
    }
}
