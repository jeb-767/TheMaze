using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; // Usamos TextMeshPro para mejor calidad de texto

public class MenuAjustes : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    
    [Header("UI References")]
    public TMP_Dropdown resolucionDropdown;
    public TMP_Dropdown calidadDropdown;
    public Toggle pantallaCompletaToggle;
    public Slider sliderMaster;
    public Slider sliderBGM;
    public Slider sliderVFX;
    public TMP_InputField inputFieldMaster;
    public TMP_InputField inputFieldBGM;
    public TMP_InputField inputFieldVFX;

    private Resolution[] resoluciones;

    public void Setup()
    {
        // 1. Cargar las resoluciones disponibles del monitor del usuario
        resoluciones = Screen.resolutions;
        resolucionDropdown.ClearOptions();

        List<string> opciones = new List<string>();
        int resolucionActualIndex = 0;

        for (int i = 0; i < resoluciones.Length; i++)
        {
            string opcion = resoluciones[i].width + " x " + resoluciones[i].height;
            opciones.Add(opcion);

            // Verificar si esta es la resolución que estamos usando ahora mismo
            if (resoluciones[i].width == Screen.currentResolution.width &&
                resoluciones[i].height == Screen.currentResolution.height)
            {
                resolucionActualIndex = i;
            }
        }

        resolucionDropdown.AddOptions(opciones);
        resolucionDropdown.value = resolucionActualIndex;
        resolucionDropdown.RefreshShownValue();

        // 2. Inicializar los valores de la UI con los ajustes actuales
        pantallaCompletaToggle.isOn = Screen.fullScreen;
        // --- 3. Cargar las calidades gráficas automáticamente ---
        calidadDropdown.ClearOptions(); // Limpiamos lo que haya por defecto
        
        // QualitySettings.names nos da un arreglo con los nombres ("Very Low", "High", etc.)
        string[] opcionesCalidad = QualitySettings.names; 
        
        // Convertimos el arreglo a una lista y la añadimos al dropdown
        calidadDropdown.AddOptions(new List<string>(opcionesCalidad)); 
        
        // Seleccionamos la calidad que está activa en este momento
        calidadDropdown.value = QualitySettings.GetQualityLevel();
        calidadDropdown.RefreshShownValue();

        //3. Inicializar los sliders de audio con los valores guardados en GameManager (Guardados previamente)
        resolucionDropdown.value = GameManager.resIndex;
        calidadDropdown.value = GameManager.qualIndex;
        pantallaCompletaToggle.isOn = GameManager.fullScreen;
        sliderMaster.value = GameManager.audioGen;
        sliderBGM.value = GameManager.audioBGM;
        sliderVFX.value = GameManager.audioSFX;
        SetVolumenMaster(sliderMaster.value);
        SetVolumenBGM(sliderBGM.value);
        SetVolumenVFX(sliderVFX.value);
        SetCalidadGrafica(calidadDropdown.value);
        SetPantallaCompleta(pantallaCompletaToggle.isOn);
        SetResolucion(resolucionDropdown.value);
        // (Opcional) Aquí podrías cargar los valores de los sliders desde PlayerPrefs si ya los habías guardado antes
    }

    // --- MÉTODOS DE AUDIO ---
    // Usamos Mathf.Log10 porque el AudioMixer usa decibelios (escala logarítmica)
    public void SetVolumenMaster(float volumen)
    {
        if (volumen <= 0.001f) {
            audioMixer.SetFloat("MasterVolume", -80f); // Silencio total
        } else {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volumen) * 20);
        }
        GameManager.audioGen = (int)volumen; // Guardamos el valor en GameManager
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
        inputFieldMaster.text = ((int)(volumen * 100)).ToString(); // Actualizamos el InputField
    }

    public void SetVolumenBGM(float volumen)
    {
        if (volumen <= 0.001f) {
            audioMixer.SetFloat("BGMVolume", -80f); // Silencio total
        } else {
            audioMixer.SetFloat("BGMVolume", Mathf.Log10(volumen) * 20);
        }
        GameManager.audioBGM = (int)volumen;
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
        inputFieldBGM.text = ((int)(volumen * 100)).ToString(); // Actualizamos el InputField
    }

    public void SetVolumenVFX(float volumen)
    {
        if (volumen <= 0.001f) {
            audioMixer.SetFloat("VFXVolume", -80f); // Silencio total
        } else {
            audioMixer.SetFloat("VFXVolume", Mathf.Log10(volumen) * 20);
        }
        GameManager.audioSFX = (int)volumen;
        inputFieldVFX.text = ((int)(volumen * 100)).ToString(); // Actualizamos el InputField
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
    }
    public void SetVolumenMasterInput(string input)
    {
        if (float.TryParse(input, out float volumen))
        {
            volumen = Mathf.Clamp(volumen, 0f, 100f);
            SetVolumenMaster(volumen / 100f);
            sliderMaster.value = volumen / 100f; // Actualizamos el slider
        }
        else
        {
            SetVolumenMaster(sliderMaster.value); // Restaurar el valor del slider si la entrada no es válida
            inputFieldMaster.text = ((int)(sliderMaster.value * 100)).ToString();
        }
    }
    public void SetVolumenBGMInput(string input)
    {
        if (float.TryParse(input, out float volumen))
        {
            volumen = Mathf.Clamp(volumen, 0f, 100f);
            SetVolumenBGM(volumen / 100f);
            sliderBGM.value = volumen / 100f; // Actualizamos el slider
        }
        else
        {
            SetVolumenBGM(sliderBGM.value); // Restaurar el valor del slider si la entrada no es válida
            inputFieldBGM.text = ((int)(sliderBGM.value * 100)).ToString();
        }
    }
    public void SetVolumenVFXInput(string input)
    {
        if (float.TryParse(input, out float volumen))
        {
            volumen = Mathf.Clamp(volumen, 0f, 100f);
            SetVolumenVFX(volumen / 100f);
            sliderVFX.value = volumen / 100f; // Actualizamos el slider
        }
        else
        {
            SetVolumenVFX(sliderVFX.value); // Restaurar el valor del slider si la entrada no es válida
            inputFieldVFX.text = ((int)(sliderVFX.value * 100)).ToString();
        }
    }
    // --- MÉTODOS DE GRÁFICOS Y PANTALLA ---
    public void SetCalidadGrafica(int indexCalidad)
    {
        // El index coincide con los niveles definidos en Edit > Project Settings > Quality
        QualitySettings.SetQualityLevel(indexCalidad);
        GameManager.qualIndex = indexCalidad; // Guardamos el valor en GameManager
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
    }

    public void SetPantallaCompleta(bool esPantallaCompleta)
    {
        Screen.fullScreen = esPantallaCompleta;
        GameManager.fullScreen = esPantallaCompleta; // Guardamos el valor en GameManager
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
    }

    public void SetResolucion(int resolucionIndex)
    {
        Resolution resolucion = resoluciones[resolucionIndex];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
        GameManager.resIndex = resolucionIndex; // Guardamos el valor en GameManager
        SaveManager.GuardarPartida(); // Guardamos los cambios inmediatamente
    }
}