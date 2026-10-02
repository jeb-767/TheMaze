using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LevelUpMenu : MonoBehaviour
{
    public Texture2D[] icons;
    public RawImage iconDisplay;
    public RawImage backgroundDisplay;
    public TextMeshProUGUI descriptionDisplay;
    public TextMeshProUGUI levelDisplay;
    int iconIndex; //health , health regen , stamina , staminaregen , attack , attack velocity , crit , crit probability, velocity, capacity , luck , armor , gold gain , xp gain ,  vision , gold, sword, armor 
    int backgroundIndex; //0: common, 1: uncommon, 2: rare, 3: epic, 4: legendary, 5: mythic
    public Button button;
    public ItemData[] swords;
    public ItemData[] armors;
    public Player player;
    public Inventory inv;
    public InventoryUI invUi;
    public ItemData newItem;
    public float wait;
    public AudioSource audioSource, reloadSource;
    public Animator anim;
    public GenerateNewLevel generator;
    public bool isSpecial;
    public Button reloadButton;
    public void Generate()
    {
        button.onClick.RemoveAllListeners();
        SelectQuality();
        SelectIcon();
        SelectDescription();
        StartCoroutine(Enter(wait));
    }
    public IEnumerator Enter(float delay)
    {
        yield return new WaitForSeconds(delay);
        anim.SetTrigger("Enter");
    }
    public void SelectQuality()
    {
        int probabilities = Random.Range(0,100);
        if (probabilities > 98) //2%
        {
            backgroundIndex = 5;
            backgroundDisplay.color = new Color32(236, 50, 67, 255);
        }
        else if (probabilities > 93) //5%
        {
            backgroundIndex = 4;
            backgroundDisplay.color = new Color32(255, 244, 0, 255);
        }
        else if (probabilities > 85) //8%
        {
            backgroundIndex = 3;
            backgroundDisplay.color = new Color32(161, 0, 255, 255);
        }
        else if (probabilities > 70)//15%
        {
            backgroundIndex = 2;
            backgroundDisplay.color = new Color32(0, 109, 255, 255);
        }
        else if (probabilities > 45)//25%
        {
            backgroundIndex = 1;
            backgroundDisplay.color = new Color32(83, 189, 67, 255);
        }
        else //45%
        {
            backgroundIndex = 0;
            backgroundDisplay.color = new Color32(255, 255, 255, 255);
        }
    }
    public void SelectIcon()
    {
        int randomIndex = Random.Range(0, icons.Length);
        iconIndex = randomIndex;
        iconDisplay.texture = icons[iconIndex];
    }

    // Update is called once per frame
    public void SelectDescription()
    {
        switch (iconIndex) //health , health regen , stamina , staminaregen , attack , attack velocity , crit , crit probability, velocity, capacity , luck , armor , gold gain , xp gain ,  vision , gold, sword, armor 
        {
            case 0:
                descriptionDisplay.text = "Health";
                SelectLevel(3);
                SelectButtonAction(3);
                break;
            case 1:
                descriptionDisplay.text = "Heath Regen";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 2:
                descriptionDisplay.text = "Stamina";
                SelectLevel(3);
                SelectButtonAction(3);
                break;
            case 3:
                descriptionDisplay.text = "Stamina Regen";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 4:
                descriptionDisplay.text = "Attack";
                SelectLevel(0.3f);
                SelectButtonAction(0.3f);
                break;
            case 5:
                descriptionDisplay.text = "Attack Velocity";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 6:
                descriptionDisplay.text = "Critical Damage";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 7:
                descriptionDisplay.text = "Critical Probability";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 8:
                descriptionDisplay.text = "Velocity";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 9:
                descriptionDisplay.text = "Capacity";   
                SelectLevel(1);
                SelectButtonAction(1);
                break;
            case 10:
                descriptionDisplay.text = "Luck";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 11:
                descriptionDisplay.text = "Armor";
                SelectLevel(0.3f);
                SelectButtonAction(0.3f);
                break;
            case 12:
                descriptionDisplay.text = "Gold Gain";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 13:
                descriptionDisplay.text = "XP Gain";
                SelectLevel(0.03f);
                SelectButtonAction(0.03f);
                break;
            case 14:
                descriptionDisplay.text = "Vision";
                SelectLevel(0.06f);
                SelectButtonAction(0.06f);
                break;
            case 15:
                descriptionDisplay.text = "Gold";
                SelectLevel(2);
                SelectButtonAction(2);
                break;
            case 16:   
                descriptionDisplay.text = "Sword";
                SelectItemQuality();
                SelectButtonAction(0);
                break;
            case 17:
                descriptionDisplay.text = "Armor";
                SelectItemQuality();
                SelectButtonAction(0);
                break;
            case 18:
                descriptionDisplay.text = "Instant Heal";
                SelectLevel(3f);
                SelectButtonAction(3);
                break;
            case 19:
                descriptionDisplay.text = "Reload";
                SelectLevel(1f);
                SelectButtonAction(1);
                break;
        }
    }
    public void SelectLevel(float quantity)
    {
        if(quantity < 0.1f)
        {
            levelDisplay.text = "+" + (quantity * (backgroundIndex + 1) * 100).ToString() + "%";
        }
        else
        {
            levelDisplay.text = "+" + (quantity * (backgroundIndex + 1)).ToString();
        }
    }
    public void SelectItemQuality()
    {
        switch(backgroundIndex)
        {
            case 0:
                levelDisplay.text = " Common";
                break;
            case 1:
                levelDisplay.text = " Uncommon";
                break;
            case 2:
                levelDisplay.text = " Rare";
                break;
            case 3:
                levelDisplay.text = " Epic";
                break;
            case 4:
                levelDisplay.text = " Legendary";
                break;
            case 5:
                levelDisplay.text = " Mythic";
                break;
        }
    }
    public void SelectButtonAction(float quantity)
    {
        //health , health regen , stamina , staminaregen , attack , attack velocity , crit , crit probability, velocity, capacity , luck , armor , gold gain , xp gain ,  vision , gold, sword, armor
        switch (iconIndex)
        {
            case 0:
                button.onClick.AddListener(() => AddHEalth(quantity));
                break;
            case 1:
                button.onClick.AddListener(() => AddHealthRegen(quantity));
                break;
            case 2:
                button.onClick.AddListener(() => AddStamina(quantity));
                break;
            case 3:
                button.onClick.AddListener(() => AddStaminaRegen(quantity));
                break;
            case 4:
                button.onClick.AddListener(() => AddAttack(quantity));
                break;
            case 5:
                button.onClick.AddListener(() => AddAttackVelocity(quantity));
                break;
            case 6:
                button.onClick.AddListener(() => AddCrit(quantity));
                break;
            case 7:
                button.onClick.AddListener(() => AddCritProbability(quantity));
                break;
            case 8:
                button.onClick.AddListener(() => AddVelocity(quantity));
                break;
            case 9:
                button.onClick.AddListener(() => AddCapacity(quantity));
                break;
            case 10:
                button.onClick.AddListener(() => AddLuck(quantity));
                break;
            case 11:
                button.onClick.AddListener(() => AddArmor(quantity));
                break;
            case 12:
                button.onClick.AddListener(() => AddGoldGain(quantity));
                break;
            case 13:
                button.onClick.AddListener(() => AddXPGain(quantity));
                break;
            case 14:
                button.onClick.AddListener(() => AddVision(quantity));
                break;
            case 15:
                button.onClick.AddListener(() => AddGold(quantity));
                break;
            case 16:   
                button.onClick.AddListener(() => AddSword());
                break;
            case 17:
                button.onClick.AddListener(() => AddArmor());
                break;
            case 18:
                button.onClick.AddListener(() => RestoreHealth(quantity));
                break;
            case 19:
                button.onClick.AddListener(() => AddReload(quantity));
                break;
        }
        button.onClick.AddListener(() => CerrarMenu());

        Debug.Log("Button action added");
    }
    public void AddHEalth(float quantity)
    {
        RunManager.health += quantity * (backgroundIndex + 1);
        player.maxHealthBase += quantity * (backgroundIndex + 1);
        player.HealthBar.GetComponent<Slider>().maxValue = player.maxHealthBase;
        player.StartResetHealth();
    }
    public void AddHealthRegen(float quantity)
    {
        RunManager.healthRegen += quantity * (backgroundIndex + 1);
    }
    public void AddStamina(float quantity)
    {
        RunManager.stamina += quantity * (backgroundIndex + 1);
        player.MaxStamina += quantity * (backgroundIndex + 1);
        player.StaminaBar.GetComponent<Slider>().maxValue = player.MaxStamina;
        player.StartResetStamina();
    }
    public void AddStaminaRegen(float quantity)
    {
        RunManager.staminaRegen += quantity * (backgroundIndex + 1);
    }
    public void AddAttack(float quantity)
    {
        RunManager.attack += quantity * (backgroundIndex + 1);
    }
    public void AddAttackVelocity(float quantity)
    {
        RunManager.attackVelocity += quantity * (backgroundIndex + 1);
        player.attackVelocity = player.attackVelocity + 1 * RunManager.attackVelocity;
        player.anim.SetFloat("AttackVelocity", player.attackVelocity);
    }
    public void AddCrit(float quantity)
    {
        RunManager.crit += quantity * (backgroundIndex + 1);
    }
    public void AddCritProbability(float quantity)
    {
        RunManager.critProbability += quantity * (backgroundIndex + 1);
    }
    public void AddVelocity(float quantity)
    {
        RunManager.velocity += quantity * (backgroundIndex + 1);
        player.SetupVelocity();
    }
    public void AddCapacity(float quantity)
    {
        RunManager.capacity += quantity * (backgroundIndex + 1);
        player.inventory.maxCapacity += (int)(quantity * (backgroundIndex + 1));
        player.canvas.capacity2.text = player.inventory.maxCapacity.ToString();
    }
    public void AddLuck(float quantity)
    {
        RunManager.luck += quantity * (backgroundIndex + 1);
    }
    public void AddArmor(float quantity)
    {
        RunManager.armor += quantity * (backgroundIndex + 1);
        player.armor += quantity * (backgroundIndex + 1);
        player.canvas.armorText.text = player.armor.ToString("F2");
    }
    public void AddGoldGain(float quantity)
    {
        RunManager.goldGain += quantity * (backgroundIndex + 1);
    }
    public void AddXPGain(float quantity)
    {
        RunManager.xpGain += quantity * (backgroundIndex + 1);
    }
    public void AddVision(float quantity)
    {
        RunManager.vision += quantity * (backgroundIndex + 1);
        player.lightRange += quantity * (backgroundIndex + 1);
        player.lighting.intensity = player.lightRange;
    }
    public void AddGold(float quantity)
    {
        RunManager.Gold += quantity * (backgroundIndex + 1);
    }
    public void AddSword()
    {
        GenerateItem(swords[Random.Range(0, swords.Length)]);
    }
    public void AddArmor ()
    {
        GenerateItem(armors[Random.Range(0, armors.Length)]);
    }
    public void GenerateItem(ItemData item)
    {
        float rarityMult = 0;
        newItem = ScriptableObject.CreateInstance<ItemData>();
        newItem.id = item.id;
        newItem.itemName = item.itemName;
        newItem.description = item.description;
        newItem.prefab = item.prefab;
        newItem.sizeX = item.sizeX;
        newItem.sizeY = item.sizeY;
        newItem.tipoEquipo = item.tipoEquipo;
        newItem.equipable = item.equipable;
        switch (backgroundIndex)
        {
            case 0:
                newItem.rarezaEquipo = Rareza.Common;
                rarityMult = 1.0f;
                break;
            case 1:
                newItem.rarezaEquipo = Rareza.Rare;
                rarityMult = 1.2f;
                break;
            case 2:
                newItem.rarezaEquipo = Rareza.Super_rare;
                rarityMult = 1.4f;
                break;
            case 3:
                newItem.rarezaEquipo = Rareza.Epic;
                rarityMult = 1.6f;
                break;
            case 4:
                newItem.rarezaEquipo = Rareza.Legendary;
                rarityMult = 1.8f;
                break;
            case 5:
                newItem.rarezaEquipo = Rareza.Mhytic;
                rarityMult = 2.0f;
                break;

        }
        if (item.tipoEquipo == TipoEquipo.Weapon)
        {
            newItem.Daño = Mathf.Round(((float)(Random.Range(6, 12)) * rarityMult) * 100f) / 100f;
        }
        else
        {
            newItem.Armadura = Mathf.Round(((float)(Random.Range(2, 7) / 10f) * rarityMult) * 100f) / 100f;
        }
        newItem.durabilidad = Mathf.Round(((int)(Random.Range(50, 75)) * rarityMult) * 100f) / 100f;
        newItem.currentDurability = newItem.durabilidad;
        newItem.icon = null;
        AddItemToInventory(newItem);
    }
    public void AddItemToInventory(ItemData newItem)
    {
        if (inv != null && invUi != null)
        {
            if (newItem.icon == null && newItem.prefab != null)
            {
                ItemIconGenerator generator = FindObjectOfType<ItemIconGenerator>();
                newItem.icon = generator.GenerateIcon(newItem.prefab, newItem);
            }
            inv.FillSpace(newItem);
            invUi.PlaceItem(newItem);
        }
    }
    public void RestoreHealth(float quantity)
    {
        player.InstantHeal(quantity * (backgroundIndex + 1));
    }
    public void AddReload(float quantity)
    {
        RunManager.reloads += quantity * (backgroundIndex + 1);
        generator.enableReload();
    }
    public void Reload()
    {
        if(RunManager.reloads > 0)
        {
            this.Generate();
            RunManager.reloads -= 1;
            reloadSource.Play();
        }
        if(RunManager.reloads <= 0)
        {
            generator.disableReload();
        }
        generator.reloadText.text = RunManager.reloads.ToString();
    }
    public void CerrarMenu()
    {
        button.onClick.RemoveAllListeners();
        audioSource.Play();
        anim.SetBool("Selected" , true);
        if(isSpecial)
        {
            generator.CloseS();
        }
        else
        {
            generator.CloseN();
        }
    }
}
