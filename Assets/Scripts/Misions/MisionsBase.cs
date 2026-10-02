using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.AI;
using Unity.Cinemachine;
using Unity.Netcode;
[CreateAssetMenu(menuName = "Misions/Mision Data")]
public class MisionsBase : ScriptableObject
{
    public string name;
    public string description;
    public int reward1; //items
    public int reward2; // gold
    public int reward3; // xp
    public MisionType misionType;
    public Category misionCategory;
    public List<ObjectivesBase> objectives = new List<ObjectivesBase>();
    public bool isSharedMission;
    public bool normalReward;
}
public enum MisionType
{
    Collect,
    Kill,
    Explore,
    Talk,
}
public enum Category
{
    Main,
    Side,
    Daily,
    Event,
}
