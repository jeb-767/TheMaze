using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;


public class ExitTrigger : MonoBehaviour
{
    public Volume postProcessingVolume;
    public Vignette vignette;
    public GameObject winScreen;
    public Canvas_Controller canvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvas = FindObjectOfType<Canvas_Controller>();
        postProcessingVolume = FindObjectOfType<Volume>();
        postProcessingVolume.profile.TryGet(out vignette);
        winScreen = GameObject.Find("FinishPanel");
        winScreen.SetActive(false);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(Finish());
        }
    }
    public IEnumerator Finish()
    {
        MissionManager.Instance.AddProgress(11, 1);
        yield return new WaitForSeconds(0.5f);
        float vigneteValue = vignette.intensity.value;
        float vignetteSmoothness = vignette.smoothness.value;
        Debug.Log("Finish");
        if (vignette != null)
        {
            Debug.Log("Start Finish");
            vignette.rounded.value = true;
            for (int i = 0; i < 10; i++)
            {
                vignette.intensity.value += 0.1f;
                vignette.smoothness.value += 0.1f;
                yield return new WaitForSeconds(0.15f);
            }
            yield return new WaitForSeconds(0.5f);
            vignette.intensity.value = vigneteValue;
            vignette.smoothness.value = vignetteSmoothness;
            canvas.Finish();
        }
    }
}
