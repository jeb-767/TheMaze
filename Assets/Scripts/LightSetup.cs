using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class LightSetup : MonoBehaviour
{
    public MeshRenderer_Hide meshRendererHide;
    public List<Color> colors = new List<Color>();
    public List<ParticleSystem> particulas = new List<ParticleSystem>();
    public List<Gradient> gradientes = new List<Gradient>();
    public List<ParticleSystem> lights = new List<ParticleSystem>();  
    void Start()
    {
        int num = Random.Range(0, colors.Count);
        foreach(ParticleSystem color in particulas)
        {
            var colorOverLifetime = color.colorOverLifetime;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradientes[num]);
        }
        foreach(ParticleSystem light in lights)
        {
            var lightGrad = light.colorOverLifetime;
            Gradient gradLight = lightGrad.color.gradient;
            GradientColorKey[] colorKeys = gradLight.colorKeys;
            GradientAlphaKey[] alphaKeys = gradLight.alphaKeys;
            colorKeys[1].color = colors[num];
            gradLight.colorKeys = colorKeys;
            gradLight.alphaKeys = alphaKeys;
            lightGrad.color = new ParticleSystem.MinMaxGradient(gradLight);
        }
        
    }
}
