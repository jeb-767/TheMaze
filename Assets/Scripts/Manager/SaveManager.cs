using UnityEngine;
using System.IO;

// 1. Clase contenedora que JsonUtility sí puede leer
[System.Serializable]
public class DatosGuardado
{
    // Datos del GameManager
    public float coins;
    public int keys;
    // Matrices aplanadas
    public bool[] unlockedFlat;
    public int[] completedFlat;

    // StatsPlayerManager (Floats)
    public float health, healthRegen, stamina, staminaRegen, attack, attackVelocity, crit, critProbability, velocity, capacity, luck, armor, vision, revive, goldGain, xpGain , reloads;
    public bool hasCompass;
    // StatsPlayerManager (Ints/Niveles)
    public int healthlv, healthRegenlv, staminalv, staminaRegenlv, attacklv, attackVelocitylv, critlv, critProbabilitylv, velocitylv, capacitylv, lucklv, armorlv, visionlv, revivelv, goldGainlv, xpGainlv, mapLv;
    //Ajustes
    public int resIndex, qualIndex, audioGen, audioBGM, audioSFX;
    public bool fullScreen;
}

public static class SaveManager
{
    // Ruta donde se guardará el archivo en el dispositivo
    private static string rutaArchivo = Application.persistentDataPath + "/playerProgress.json";

    public static void GuardarPartida()
    {
        DatosGuardado datos = new DatosGuardado();

        // --- GUARDAR GAMEMANAGER ---
        datos.coins = GameManager.coins;
        datos.keys = GameManager.keys;

        int sizes = GameManager.sizes;
        int diffs = GameManager.difficulties;
        
        // Inicializamos los arreglos aplanados
        datos.unlockedFlat = new bool[sizes * diffs];
        datos.completedFlat = new int[sizes * diffs];

        // Aplanamos las matrices bidimensionales [,] a unidimensionales []
        for (int i = 0; i < sizes; i++)
        {
            for (int j = 0; j < diffs; j++)
            {
                int index = (i * diffs) + j;
                datos.unlockedFlat[index] = GameManager.unlocked[i, j];
                datos.completedFlat[index] = GameManager.completed[i, j];
            }
        }

        // --- GUARDAR STATS PLAYER MANAGER ---
        datos.health = StatsPlayerManager.health;
        datos.healthRegen = StatsPlayerManager.healthRegen;
        datos.stamina = StatsPlayerManager.stamina;
        datos.staminaRegen = StatsPlayerManager.staminaRegen;
        datos.attack = StatsPlayerManager.attack;
        datos.attackVelocity = StatsPlayerManager.attackVelocity;
        datos.crit = StatsPlayerManager.crit;
        datos.critProbability = StatsPlayerManager.critProbability;
        datos.velocity = StatsPlayerManager.velocity;
        datos.capacity = StatsPlayerManager.capacity;
        datos.luck = StatsPlayerManager.luck;
        datos.armor = StatsPlayerManager.armor;
        datos.vision = StatsPlayerManager.vision;
        datos.revive = StatsPlayerManager.revive;
        datos.goldGain = StatsPlayerManager.goldGain;
        datos.xpGain = StatsPlayerManager.xpGain;
        datos.reloads = StatsPlayerManager.reloads;

        datos.healthlv = StatsPlayerManager.healthlv;
        datos.healthRegenlv = StatsPlayerManager.healthRegenlv;
        datos.staminalv = StatsPlayerManager.staminalv;
        datos.staminaRegenlv = StatsPlayerManager.staminaRegenlv;
        datos.attacklv = StatsPlayerManager.attacklv;
        datos.attackVelocitylv = StatsPlayerManager.attackVelocitylv;
        datos.critlv = StatsPlayerManager.critlv;
        datos.critProbabilitylv = StatsPlayerManager.critProbabilitylv;
        datos.velocitylv = StatsPlayerManager.velocitylv;
        datos.capacitylv = StatsPlayerManager.capacitylv;
        datos.lucklv = StatsPlayerManager.lucklv;
        datos.armorlv = StatsPlayerManager.armorlv;
        datos.visionlv = StatsPlayerManager.visionlv;
        datos.revivelv = StatsPlayerManager.revivelv;
        datos.goldGainlv = StatsPlayerManager.goldGainlv;
        datos.xpGainlv = StatsPlayerManager.xpGainlv;
        datos.mapLv = StatsPlayerManager.mapLv;
        datos.hasCompass = StatsPlayerManager.hasCompass;

        datos.resIndex = GameManager.resIndex;
        datos.qualIndex = GameManager.qualIndex;
        datos.audioGen = GameManager.audioGen;
        datos.audioBGM = GameManager.audioBGM;
        datos.audioSFX = GameManager.audioSFX;
        datos.fullScreen = GameManager.fullScreen;
        // Convertir a texto JSON (el 'true' hace que el JSON sea legible si lo abres con bloc de notas)
        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(rutaArchivo, json);
        
        Debug.Log("Partida guardada exitosamente en: " + rutaArchivo);
    }

    public static void CargarPartida()
    {
        if (File.Exists(rutaArchivo))
        {
            string contenidoJson = File.ReadAllText(rutaArchivo);
            DatosGuardado datosCargados = JsonUtility.FromJson<DatosGuardado>(contenidoJson);

            // --- CARGAR GAMEMANAGER ---
            GameManager.coins = datosCargados.coins;
            GameManager.keys = datosCargados.keys;

            int sizes = GameManager.sizes;
            int diffs = GameManager.difficulties;

            // Reconstruimos las matrices bidimensionales
            for (int i = 0; i < sizes; i++)
            {
                for (int j = 0; j < diffs; j++)
                {
                    int index = (i * diffs) + j;
                    GameManager.unlocked[i, j] = datosCargados.unlockedFlat[index];
                    GameManager.completed[i, j] = datosCargados.completedFlat[index];
                }
            }

            // --- CARGAR STATS PLAYER MANAGER ---
            StatsPlayerManager.health = datosCargados.health;
            StatsPlayerManager.healthRegen = datosCargados.healthRegen;
            StatsPlayerManager.stamina = datosCargados.stamina;
            StatsPlayerManager.staminaRegen = datosCargados.staminaRegen;
            StatsPlayerManager.attack = datosCargados.attack;
            StatsPlayerManager.attackVelocity = datosCargados.attackVelocity;
            StatsPlayerManager.crit = datosCargados.crit;
            StatsPlayerManager.critProbability = datosCargados.critProbability;
            StatsPlayerManager.velocity = datosCargados.velocity;
            StatsPlayerManager.capacity = datosCargados.capacity;
            StatsPlayerManager.luck = datosCargados.luck;
            StatsPlayerManager.armor = datosCargados.armor;
            StatsPlayerManager.vision = datosCargados.vision;
            StatsPlayerManager.revive = datosCargados.revive;
            StatsPlayerManager.goldGain = datosCargados.goldGain;
            StatsPlayerManager.xpGain = datosCargados.xpGain;
            StatsPlayerManager.reloads = datosCargados.reloads;

            StatsPlayerManager.healthlv = datosCargados.healthlv;
            StatsPlayerManager.healthRegenlv = datosCargados.healthRegenlv;
            StatsPlayerManager.staminalv = datosCargados.staminalv;
            StatsPlayerManager.staminaRegenlv = datosCargados.staminaRegenlv;
            StatsPlayerManager.attacklv = datosCargados.attacklv;
            StatsPlayerManager.attackVelocitylv = datosCargados.attackVelocitylv;
            StatsPlayerManager.critlv = datosCargados.critlv;
            StatsPlayerManager.critProbabilitylv = datosCargados.critProbabilitylv;
            StatsPlayerManager.velocitylv = datosCargados.velocitylv;
            StatsPlayerManager.capacitylv = datosCargados.capacitylv;
            StatsPlayerManager.lucklv = datosCargados.lucklv;
            StatsPlayerManager.armorlv = datosCargados.armorlv;
            StatsPlayerManager.visionlv = datosCargados.visionlv;
            StatsPlayerManager.revivelv = datosCargados.revivelv;
            StatsPlayerManager.goldGainlv = datosCargados.goldGainlv;
            StatsPlayerManager.xpGainlv = datosCargados.xpGainlv;
            StatsPlayerManager.mapLv = datosCargados.mapLv;
            StatsPlayerManager.hasCompass = datosCargados.hasCompass;
            // Cargar ajustes
            GameManager.resIndex = datosCargados.resIndex;
            GameManager.qualIndex = datosCargados.qualIndex;
            GameManager.audioGen = datosCargados.audioGen;
            GameManager.audioBGM = datosCargados.audioBGM;
            GameManager.audioSFX = datosCargados.audioSFX;
            GameManager.fullScreen = datosCargados.fullScreen;
            Debug.Log("Partida cargada exitosamente.");
        }
        else
        {
            Debug.LogWarning("No se encontró archivo de guardado. Se usarán los valores por defecto.");
        }
    }
    public static void ResetearPartida()
    {
        // 1. Borramos el archivo físico del disco si existe
        if (File.Exists(rutaArchivo))
        {
            File.Delete(rutaArchivo);
            Debug.Log("Archivo de guardado eliminado de: " + rutaArchivo);
        }
        else
        {
            Debug.Log("No había archivo de guardado previo.");
        }

        // 2. Limpiamos las variables estáticas que están cargadas actualmente en memoria
        GameManager.ResetToDefault();
        StatsPlayerManager.ResetToDefault();
        
        Debug.Log("Partida reseteada por completo a los valores por defecto.");
    }
}