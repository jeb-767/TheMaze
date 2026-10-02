using UnityEngine;

public static class StatsPlayerManager
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static float health , healthRegen , stamina, staminaRegen , attack , attackVelocity ,crit , critProbability, velocity, capacity , luck , armor , vision , revive, goldGain , xpGain , reloads;
    public static int healthlv , healthRegenlv , staminalv, staminaRegenlv , attacklv , attackVelocitylv ,critlv , critProbabilitylv, velocitylv, capacitylv , lucklv , armorlv , visionlv , revivelv, goldGainlv , xpGainlv, mapLv;
    public static bool hasCompass;

    public static void ResetToDefault()
    {
        // Floats
        health = 0f; 
        healthRegen = 0f; 
        stamina = 0f; 
        staminaRegen = 0f; 
        attack = 0f; 
        attackVelocity = 0f; 
        crit = 0f; 
        critProbability = 0f; 
        velocity = 0f; 
        capacity = 0f; 
        luck = 0f; 
        armor = 0f; 
        vision = 0f; 
        revive = 0f; 
        goldGain = 0f; 
        xpGain = 0f; 
        reloads = 0f;

        // Ints (Niveles)
        healthlv = 0; 
        healthRegenlv = 0; 
        staminalv = 0; 
        staminaRegenlv = 0; 
        attacklv = 0; 
        attackVelocitylv = 0; 
        critlv = 0; 
        critProbabilitylv = 0; 
        velocitylv = 0; 
        capacitylv = 0; 
        lucklv = 0; 
        armorlv = 0; 
        visionlv = 0; 
        revivelv = 0; 
        goldGainlv = 0; 
        xpGainlv = 0; 
        mapLv = 0;

        // Bools
        hasCompass = false;
    }
}
