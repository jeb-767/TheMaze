using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;
public static class GameManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static int width = 10;
    public static int height = 10;
    public static int center = 2;
    public static int enemies, seed, specialZones;
    public static float difficulty = 1f;
    public static int sizes = 8;//0 = micro , 1 = tiny, 2 = small , 3 = medium , 4 = large , 5 = huge , 6 = colossal y 7 = custom
    public static int difficulties = 9; //0 = unlocked , 1 = very easy , 2 = easy , 3 = medium , 4 = hard , 5 = very hard , 6 = impossible , 7 = nightmare , 8 = custom
    public static bool[,] unlocked = new bool[sizes, difficulties];
    public static int[,] completed = new int[sizes, difficulties];
    public static int keys = 0;
    public static float coins = 0;
    public static bool iniciatded = false;
    public static int size;
    public static int difficult;
    public static bool multiplayer = false;
    public static bool changedEnemies, changedSeed, changedSpecialZones;
    public static int resIndex = 2 , qualIndex = 3, audioGen = 50, audioBGM = 50, audioSFX = 50;
    public static bool fullScreen = true;
    public static void Start()
    {
        changedEnemies = false;
        changedSeed = false;
        changedSpecialZones = false;
    }
    static GameManager()
    {
        SetUnlock();
    }
    public static void SetUnlock()
    {
        if (iniciatded == true)
        {
            return;
        }
        for (int i = 0; i < sizes; i++)
        {
            for (int j = 0; j < difficulties; j++)
            {
                unlocked[i, j] = false;
                completed[i, j] = 0;
            }
        }
        unlocked[0, 0] = true; //micro - unlocked
        unlocked[0, 2] = true; //micro - easy
        unlocked[1, 2] = true; //tiny - easy
        unlocked[2, 2] = true; //small - easy 
        unlocked[3, 2] = true; //medium - easy
        unlocked[4, 2] = true; //large - easy
        unlocked[5, 2] = true; //huge - easy
        unlocked[6, 2] = true; //colossal - easy
        iniciatded = true;
    }
    public static string GetSizeText(int size)
    {
        switch (size)
        {
            case 0:
                return "Micro";
            case 1:
                return "Tiny";
            case 2:
                return "Small";
            case 3:
                return "Medium";
            case 4:
                return "Large";
            case 5:
                return "Huge";
            case 6:
                return "Colossal";
            case 7:
                return "Custom";
            default:
                return "Unknown Size";
        }
    }
    public static string GetDifficultyText(int difficulty)
    {
        switch (difficulty)
        {
            case 0:
                return "Unlocked";
            case 1:
                return "Very Easy";
            case 2:
                return "Easy";
            case 3:
                return "Medium";
            case 4:
                return "Hard";
            case 5:
                return "Very Hard";
            case 6:
                return "Impossible";
            case 7:
                return "Nightmare";
            case 8:
                return "Custom";
            default:
                return "Unknown Difficulty";
        }
    }
    public static void ResetToDefault()
    {
        coins = 0;
        keys = 0;
        
        // Al poner iniciatded en false, permitimos que SetUnlock() vuelva a hacer su trabajo
        iniciatded = false; 
        SetUnlock();
    }
}
