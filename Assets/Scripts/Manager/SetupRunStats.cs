using UnityEngine;

public class SetupRunStats : MonoBehaviour
{
    public void SetupStats()
    {
        RunManager.health = StatsPlayerManager.health;
        RunManager.healthRegen = StatsPlayerManager.healthRegen;
        RunManager.stamina = StatsPlayerManager.stamina;
        RunManager.staminaRegen = StatsPlayerManager.staminaRegen;
        RunManager.attack = StatsPlayerManager.attack;
        RunManager.attackVelocity = StatsPlayerManager.attackVelocity;
        RunManager.crit = StatsPlayerManager.crit;
        RunManager.critProbability = StatsPlayerManager.critProbability;
        RunManager.velocity = StatsPlayerManager.velocity;
        RunManager.capacity = StatsPlayerManager.capacity;
        RunManager.luck = StatsPlayerManager.luck;
        RunManager.armor = StatsPlayerManager.armor;
        RunManager.vision = StatsPlayerManager.vision;
        RunManager.revive = StatsPlayerManager.revive;
        RunManager.goldGain = StatsPlayerManager.goldGain;
        RunManager.xpGain = StatsPlayerManager.xpGain;
        RunManager.reloads = StatsPlayerManager.reloads + 3f;
        RunManager.Gold = 0f;
        RunManager.obtainedGold = 0;
        RunManager.usedReloads = 0;
        RunManager.obtainedReloads = 0;
        RunManager.escaped = false;
    }
}
