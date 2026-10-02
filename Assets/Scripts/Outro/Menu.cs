using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;


public class Menu : NetworkBehaviour
{
    public Maze_Generator script;
    public TMP_InputField Height_text;
    public TMP_InputField Width_text;
    public TMP_InputField Center_text;
    public TMP_InputField Enemies_text, Seed_text , Special_text;
    public Animator tinyAnim, smallAnim, mediumAnim, largeAnim, hugeAnim, colossalAnim;
    public Animator veryEasyAnim, easyAnim, mediumAnimD, hardAnim, veryHardAnim, impossibleAnim, nightmareAnim;
    public GameObject chainTiny, chainSmall, chainMedium, chainLarge, chainHuge, chainColossal;
    public GameObject chainVeryEasy, chainMediumD, chainHard, chainVeryHard, chainImpossible, chainNightmare;
    public int _width;
    public int _height;
    public int _center;
    public GameObject sizeMenu, difficultMenu, waitPanel , menu , configurationMultiplayerPanel;
    public TextMeshProUGUI keysText, keysText2, goldText;
    public TextMeshProUGUI[] goldTexts;
    public TextMeshProUGUI difficultyText; 
    public TextMeshProUGUI sizeText; 
    public LobbyBrowserManager lobbyBrowserManager;
    public CargadorDeEscenas cargadorDeEscenas;
    public SetupRunStats setup;
    public MenuAjustes menuAjustes;
    public void Awake()
    {
        SaveManager.CargarPartida();
    }
    private void Start()
    {
        menuAjustes.Setup();
        script = FindObjectOfType<Maze_Generator>();
        Time.timeScale = 1f;
        keysText.text = GameManager.keys.ToString();
        keysText2.text = GameManager.keys.ToString();
        ChangeCoinsValue();
        for (int i = 0; i < GameManager.sizes; i++)
        {
            if (GameManager.unlocked[i, 0] == true)
            {
                switch (i)
                {
                    case 1:
                        chainTiny.SetActive(false);
                        break;
                    case 2:
                        chainSmall.SetActive(false);
                        break;
                    case 3:
                        chainMedium.SetActive(false);
                        break;
                    case 4:
                        chainLarge.SetActive(false);
                        break;
                    case 5:
                        chainHuge.SetActive(false);
                        break;
                    case 6:
                        chainColossal.SetActive(false);
                        break;
                }
            }
        }
        foreach (Transform item in this.transform)
        {
            item.gameObject.SetActive(false);
        }
        menu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ChangeCoinsValue()
    {
        foreach (TextMeshProUGUI text in goldTexts)
        {
            text.text = GameManager.coins.ToString();
        }
    }
    public void Play()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        foreach (Transform item in this.transform)
        {
            item.gameObject.SetActive(false);
        }
        cargadorDeEscenas.ActivarPantallaCarga(); 
        if(IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("PrincipalScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
        response.Position = Vector3.zero;
        response.Rotation = Quaternion.identity;
        response.Pending = false;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Change_Height()
    {
        if (int.TryParse(Height_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Clamp(valorIngresado, 0, 240);
            GameManager.height = valorLimitado;
            Height_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.height = 30;
            Height_text.text = "";
        }
    }

    public void Change_Width()
    {
        if (int.TryParse(Width_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Clamp(valorIngresado, 0, 240);
            GameManager.width = valorLimitado;
            Width_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.width = 30;
            Width_text.text = "";
        }
    }

    public void Change_Center()
    {
        if (int.TryParse(Center_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Clamp(valorIngresado, 0, Math.Min(GameManager.width , GameManager.height));
            GameManager.center = valorLimitado;
            Center_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.center = 5;
            Center_text.text = "";
        }
    }
    public void Change_Enemies()
    {
        if (int.TryParse(Enemies_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Max(1, valorIngresado);
            GameManager.enemies = valorLimitado;
            Enemies_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.enemies = 1;
            Enemies_text.text = "";
        }
        GameManager.changedEnemies = true;
    }
    public void Change_SpecialZones()
    {
        if (int.TryParse(Special_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Max(1, valorIngresado);
            GameManager.specialZones = valorLimitado;
            Special_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.specialZones = 1;
            Special_text.text = "";
        }
        GameManager.changedSpecialZones = true;
    }
    public void Change_Seed()
    {
        if (int.TryParse(Seed_text.text, out int valorIngresado))
        {
            int valorLimitado = Math.Clamp(valorIngresado, 0, 9999999);
            GameManager.seed = valorLimitado;
            Seed_text.text = valorLimitado.ToString();
        }
        else
        {
            GameManager.seed = 1111111;
            Seed_text.text = "";
        }
        GameManager.changedSeed = true;
    }
    public void ResetChanges()
    {
        GameManager.changedSeed = false;
        GameManager.changedEnemies = false;
        GameManager.changedSpecialZones = false;
    }
    public void Micro()
    {
        GameManager.width = 10;
        GameManager.height = 10;
        GameManager.center = 2;
        GameManager.size = 0;
        sizeText.text = "Micro";
        DifficultMenu();
        return;
    }
    public void Tiny()
    {
        if (GameManager.unlocked[1, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[1, 0] = true;
                GameManager.keys -= 1;
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                chainTiny.SetActive(false);
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                tinyAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 25;
            GameManager.height = 25;
            GameManager.center = 3;
            GameManager.size = 1;
            sizeText.text = "Tiny";
            DifficultMenu();
            return;
        }
    }
    public void Small()
    {
        if (GameManager.unlocked[2, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[2, 0] = true;
                GameManager.keys -= 1;
                chainSmall.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                smallAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 50;
            GameManager.height = 50;
            GameManager.center = 5;
            GameManager.size = 2;
            sizeText.text = "Small";
            DifficultMenu();
            return;
        }
    }
    public void Medium()
    {
        if (GameManager.unlocked[3, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[3, 0] = true;
                GameManager.keys -= 1;
                chainMedium.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                mediumAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 80;
            GameManager.height = 80;
            GameManager.center = 7;
            GameManager.size = 3;
            sizeText.text = "Medium";
            DifficultMenu();
            return;
        }
    }
    public void Large()
    {
        if (GameManager.unlocked[4, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[4, 0] = true;
                GameManager.keys -= 1;
                chainLarge.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                largeAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 120;
            GameManager.height = 120;
            GameManager.center = 11;
            GameManager.size = 4;
            sizeText.text = "Large";
            DifficultMenu();
            return;
        }
    }
    public void Huge()
    {
        if (GameManager.unlocked[5, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[5, 0] = true;
                GameManager.keys -= 1;
                chainHuge.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                hugeAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 180;
            GameManager.height = 180;
            GameManager.center = 15;
            GameManager.size = 5;
            sizeText.text = "Huge";
            DifficultMenu();
            return;
        }
    }
    public void Colossal()
    {
        if (GameManager.unlocked[6, 0] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[6, 0] = true;
                GameManager.keys -= 1;
                chainColossal.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                colossalAnim.SetTrigger("Block");
                return;
            }
        }
        else
        {
            GameManager.width = 220;
            GameManager.height = 220;
            GameManager.center = 20;
            GameManager.size = 6;
            sizeText.text = "Colossal";
            DifficultMenu();
            return;
        }
    }
    public void VeryEasy()
    {
        if (GameManager.unlocked[GameManager.size, 1] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 1] = true;
                GameManager.keys -= 1;
                chainVeryEasy.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                veryEasyAnim.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 0.8f;
        GameManager.difficult = 1;
        difficultyText.text = "Very Easy";
        ComprobarMultiplayer();
        return;
    }
    public void Easy()
    {
        GameManager.difficulty = 1f;
        GameManager.difficult = 2;
        difficultyText.text = "Easy";
        ComprobarMultiplayer();
        return;
    }
    public void Medium_Difficulty()
    {
        if (GameManager.unlocked[GameManager.size, 3] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 3] = true;
                GameManager.keys -= 1;
                chainMediumD.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                mediumAnimD.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 1.2f;
        GameManager.difficult = 3;
        difficultyText.text = "Medium";
        ComprobarMultiplayer();
    }
    public void Hard()
    {
        if (GameManager.unlocked[GameManager.size, 4] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 4] = true;
                GameManager.keys -= 1;
                chainHard.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
            }
            else
            {
                hardAnim.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 1.5f;
        GameManager.difficult = 4;
        difficultyText.text = "Hard";
        ComprobarMultiplayer();
    }
    public void VeryHard()
    {
        if (GameManager.unlocked[GameManager.size, 5] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 5] = true;
                GameManager.keys -= 1;
                chainVeryHard.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                veryHardAnim.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 1.8f;
        GameManager.difficult = 5;
        difficultyText.text = "Very Hard";
        ComprobarMultiplayer();
        return;
    }
    public void Impossible()
    {
        if (GameManager.unlocked[GameManager.size, 6] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 6] = true;
                GameManager.keys -= 1;
                chainImpossible.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                impossibleAnim.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 2.2f;
        GameManager.difficult = 6;
        difficultyText.text = "Impossible";
        ComprobarMultiplayer();
        return;
    }
    public void Nightmare()
    {
        if (GameManager.unlocked[GameManager.size, 7] == false)
        {
            if (GameManager.keys >= 1)
            {
                GameManager.unlocked[GameManager.size, 7] = true;
                GameManager.keys -= 1;
                chainNightmare.SetActive(false);
                keysText.text = GameManager.keys.ToString();
                keysText2.text = GameManager.keys.ToString();
                SaveManager.GuardarPartida();
                return;
            }
            else
            {
                nightmareAnim.SetTrigger("Block");
                return;
            }
        }
        GameManager.difficulty = 3f;
        GameManager.difficult = 7;
        difficultyText.text = "Nightmare";
        ComprobarMultiplayer();
        return;
    }
    public void DifficultMenu()
    {
        difficultMenu.SetActive(true);
        sizeMenu.SetActive(false);
        Debug.Log(GameManager.size + " size selected");
        chainVeryEasy.SetActive(true);
        chainMediumD.SetActive(true);
        chainHard.SetActive(true);
        chainVeryHard.SetActive(true);
        chainImpossible.SetActive(true);
        chainNightmare.SetActive(true);
        for (int i = 1; i < GameManager.difficulties - 1; i++)
        {
            if (GameManager.unlocked[GameManager.size, i] == true)
            {
                switch (i)
                {
                    case 1:
                        chainVeryEasy.SetActive(false);
                        break;
                    case 2:
                        break;
                    case 3:
                        chainMedium.SetActive(false);
                        break;
                    case 4:
                        chainHard.SetActive(false);
                        break;
                    case 5:
                        chainVeryHard.SetActive(false);
                        break;
                    case 6:
                        chainImpossible.SetActive(false);
                        break;
                    case 7:
                        chainNightmare.SetActive(false);
                        break;
                }
            }
        }
    }
    public void Multiplayer()
    {
        GameManager.multiplayer = true;
        return;
    }
    public void Singleplayer()
    {
        GameManager.multiplayer = false;
        return;
    }
    public void PlayMultiplayer()
    {
        configurationMultiplayerPanel.SetActive(true);
    }
    public void ComprobarMultiplayer()
    {
        if (GameManager.multiplayer == true)
        {
            PlayMultiplayer();
            difficultMenu.SetActive(false);
            return;
        }
        else
        { 
            setup.SetupStats();
            Play();
        }
    }
}
