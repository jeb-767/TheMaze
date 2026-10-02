using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
public class UpgradeStats : NetworkBehaviour
{
    public GameObject health, healthRegen, stamina, staminaRegen, attack, attackVelocity , crit , critProbability, velocity, capacity, luck, armor, vision, revive, goldGain, xpGain;
    public TextMeshProUGUI coinsText;
    public Menu menu;
    public void OnEnable()
    {
        StartSetup();
    }
    public void StartSetup()
    {
        coinsText.text = GameManager.coins.ToString();
        health.GetComponent<UpgradesData>().Setup(StatsPlayerManager.healthlv, (int)Mathf.Pow(2f, StatsPlayerManager.healthlv) * 20);
        healthRegen.GetComponent<UpgradesData>().Setup(StatsPlayerManager.healthRegenlv, (int)(Mathf.Pow(2f, StatsPlayerManager.healthRegenlv) * 20));
        stamina.GetComponent<UpgradesData>().Setup(StatsPlayerManager.staminalv, (int)Mathf.Pow(2f, StatsPlayerManager.staminalv) * 20);
        staminaRegen.GetComponent<UpgradesData>().Setup(StatsPlayerManager.staminaRegenlv, (int)(Mathf.Pow(2f, StatsPlayerManager.staminaRegenlv) * 20));
        attack.GetComponent<UpgradesData>().Setup(StatsPlayerManager.attacklv, (int)Mathf.Pow(2f, StatsPlayerManager.attacklv) * 20);
        attackVelocity.GetComponent<UpgradesData>().Setup(StatsPlayerManager.attackVelocitylv, (int)(Mathf.Pow(2f, StatsPlayerManager.attackVelocitylv) * 20));
        crit.GetComponent<UpgradesData>().Setup(StatsPlayerManager.critlv, (int)(Mathf.Pow(2f, StatsPlayerManager.critlv) * 20));
        critProbability.GetComponent<UpgradesData>().Setup(StatsPlayerManager.critProbabilitylv, (int)(Mathf.Pow(2f, StatsPlayerManager.critProbabilitylv) * 20));
        velocity.GetComponent<UpgradesData>().Setup(StatsPlayerManager.velocitylv, (int)(Mathf.Pow(2f, StatsPlayerManager.velocitylv) * 20));
        capacity.GetComponent<UpgradesData>().Setup(StatsPlayerManager.capacitylv, (int)(Mathf.Pow(2f, StatsPlayerManager.capacitylv) * 20));
        luck.GetComponent<UpgradesData>().Setup(StatsPlayerManager.lucklv, (int)(Mathf.Pow(2f, StatsPlayerManager.lucklv) * 20));
        armor.GetComponent<UpgradesData>().Setup(StatsPlayerManager.armorlv, (int)(Mathf.Pow(2f, StatsPlayerManager.armorlv) * 20));
        vision.GetComponent<UpgradesData>().Setup(StatsPlayerManager.visionlv, (int)(Mathf.Pow(2f, StatsPlayerManager.visionlv) * 20));
        revive.GetComponent<UpgradesData>().Setup(StatsPlayerManager.revivelv, 500);
        goldGain.GetComponent<UpgradesData>().Setup(StatsPlayerManager.goldGainlv, (int)(Mathf.Pow(2f, StatsPlayerManager.goldGainlv) * 20));
        xpGain.GetComponent<UpgradesData>().Setup(StatsPlayerManager.xpGainlv, (int)(Mathf.Pow(2f, StatsPlayerManager.xpGainlv) * 20));
    }
    public void UpgradeHealth()
    {
        if (GameManager.coins >= 20 *  Mathf.Pow(2f, StatsPlayerManager.healthlv) && StatsPlayerManager.healthlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.healthlv);
            StatsPlayerManager.health += 20f;
            StatsPlayerManager.healthlv++;
            health.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.healthlv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.healthlv) * 20));
            menu.ChangeCoinsValue();
            if(!(StatsPlayerManager.healthlv + 1 <= 5))
            {
                health.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeHealthRegen()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.healthRegenlv) && StatsPlayerManager.healthRegenlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.healthRegenlv);
            StatsPlayerManager.healthRegen += 0.05f;
            StatsPlayerManager.healthRegenlv++;
            healthRegen.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.healthRegenlv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.healthRegenlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.healthRegenlv + 1 <= 5))
            {
                healthRegen.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeStamina()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.staminalv) && StatsPlayerManager.staminalv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.staminalv);
            StatsPlayerManager.stamina += 20f;
            StatsPlayerManager.staminalv++;
            stamina.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.staminalv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.staminalv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.staminalv + 1 <= 5))
            {
                stamina.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }

    }
    public void UpgradeStaminaRegen()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.staminaRegenlv) && StatsPlayerManager.staminaRegenlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.staminaRegenlv);
            StatsPlayerManager.staminaRegen += 0.1f;
            StatsPlayerManager.staminaRegenlv++;
            staminaRegen.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.staminaRegenlv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.staminaRegenlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.staminaRegenlv + 1 <= 5))
            {
                staminaRegen.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeAttack()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.attacklv) && StatsPlayerManager.attacklv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.attacklv);
            StatsPlayerManager.attack += 1f;
            StatsPlayerManager.attacklv++;
            attack.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.attacklv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.attacklv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.attacklv + 1 <= 5))            
            {
                attack.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeAttackVelocity()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.attackVelocitylv) && StatsPlayerManager.attackVelocitylv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.attackVelocitylv);
            StatsPlayerManager.attackVelocity += 0.1f;
            StatsPlayerManager.attackVelocitylv++;
            attackVelocity.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.attackVelocitylv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.attackVelocitylv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.attackVelocitylv + 1 <= 5))
            {
                attackVelocity.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeCrit()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.critlv) && StatsPlayerManager.critlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.critlv);
            StatsPlayerManager.crit += 0.1f;
            StatsPlayerManager.critlv++;
            crit.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.critlv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.critlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.critlv + 1 <= 5))
            {
                crit.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeCritProbability()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.critProbabilitylv) && StatsPlayerManager.critProbabilitylv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.critProbabilitylv);
            StatsPlayerManager.critProbability += 0.1f;
            StatsPlayerManager.critProbabilitylv++;
            critProbability.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.critProbabilitylv - 1 , (int)(Mathf.Pow(2f, StatsPlayerManager.critProbabilitylv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.critProbabilitylv + 1 <= 5))
            {
                critProbability.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeVelocity()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.velocitylv) && StatsPlayerManager.velocitylv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.velocitylv);
            StatsPlayerManager.velocity += 0.1f;
            StatsPlayerManager.velocitylv++;
            velocity.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.velocitylv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.velocitylv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.velocitylv + 1 <= 5))
            {
                velocity.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeCapacity()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.capacitylv) && StatsPlayerManager.capacitylv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.capacitylv);
            StatsPlayerManager.capacity += 2f;
            StatsPlayerManager.capacitylv++;
            capacity.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.capacitylv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.capacitylv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.capacitylv + 1 <= 5))
            {
                capacity.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }

    }
    public void UpgradeLuck()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.lucklv) && StatsPlayerManager.lucklv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.lucklv);
            StatsPlayerManager.luck += 0.1f;
            StatsPlayerManager.lucklv++;
            luck.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.lucklv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.lucklv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.lucklv + 1 <= 5))
            {
                luck.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }

    } 
    public void UpgradeArmor()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.armorlv) && StatsPlayerManager.armorlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.armorlv);
            StatsPlayerManager.armor += 1f;
            StatsPlayerManager.armorlv++;
            armor.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.armorlv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.armorlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.armorlv + 1 <= 5))
            {
                armor.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }

    }
    public void UpgradeVision()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.visionlv) && StatsPlayerManager.visionlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.visionlv);
            StatsPlayerManager.vision += 0.2f;
            StatsPlayerManager.visionlv++;
            vision.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.visionlv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.visionlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.visionlv + 1 <= 5))
            {
                vision.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeRevive()
    {
        if (GameManager.coins >= 500 && StatsPlayerManager.revivelv + 1 <= 1)
        {
            GameManager.coins -= 500;
            StatsPlayerManager.revive += 1f;
            StatsPlayerManager.revivelv++;
            revive.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.revivelv - 1, 500);
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.revivelv + 1 <= 1))
            {
                revive.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeGoldGain()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.goldGainlv) && StatsPlayerManager.goldGainlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.goldGainlv);
            StatsPlayerManager.goldGain += 0.1f;
            StatsPlayerManager.goldGainlv++;
            goldGain.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.goldGainlv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.goldGainlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.goldGainlv + 1 <= 5))
            {
                goldGain.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void UpgradeXpGain()
    {
        if (GameManager.coins >= 20 * Mathf.Pow(2f, StatsPlayerManager.xpGainlv) && StatsPlayerManager.xpGainlv + 1 <= 5)
        {
            GameManager.coins -= 20 * Mathf.Pow(2f, StatsPlayerManager.xpGainlv);
            StatsPlayerManager.xpGain += 0.1f;
            StatsPlayerManager.xpGainlv++;
            xpGain.GetComponent<UpgradesData>().UpdateLevels(StatsPlayerManager.xpGainlv - 1, (int)(Mathf.Pow(2f, StatsPlayerManager.xpGainlv) * 20));
            coinsText.text = GameManager.coins.ToString();
            if(!(StatsPlayerManager.xpGainlv + 1 <= 5))
            {
                xpGain.GetComponent<UpgradesData>().MaxLevel();
            }
            SaveManager.GuardarPartida();
        }
        else
        {
            Debug.Log("Not enough coins or max level reached");
        }
    }
    public void ResetStats()
    {
        SaveManager.ResetearPartida();
    }
}