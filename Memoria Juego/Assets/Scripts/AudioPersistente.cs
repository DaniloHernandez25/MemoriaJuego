using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPersistente : MonoBehaviour
{
    private static AudioPersistente instancia;
    private AudioSource audioSource;

    void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);

            // Obtener el AudioSource y configurarlo
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
