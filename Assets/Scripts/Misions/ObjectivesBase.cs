using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.Cinemachine;
using Unity.Netcode;
[CreateAssetMenu(menuName = "Misions/Objective Data")]
public class ObjectivesBase : ScriptableObject
{
    public int id;
    public string name;
    public int number;
    public bool isCompleted;
}
