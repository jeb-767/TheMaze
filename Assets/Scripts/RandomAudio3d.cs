using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudio3D : MonoBehaviour
{
    [Header("Configuración de Audio")]
    [Tooltip("Arrastra aquí tus clips de audio")]
    public AudioClip[] audioClips;

    [Tooltip("Variación de tono para evitar repetición robótica")]
    [Range(0.1f, 0.5f)]
    public float pitchVariation = 0.1f;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        
        // Aseguramos por código que sea 3D, por si se te olvida en el inspector
        source.spatialBlend = 1.0f; 
    }

    public void PlayRandomSound()
    {
        if (audioClips.Length == 0) return;

        // 1. Elegir un clip aleatorio
        int index = Random.Range(0, audioClips.Length);
        AudioClip clipToPlay = audioClips[index];

        // 2. (Opcional) Variar ligeramente el pitch para realismo
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);

        // 3. Reproducir usando PlayOneShot
        source.PlayOneShot(clipToPlay);
    }
}