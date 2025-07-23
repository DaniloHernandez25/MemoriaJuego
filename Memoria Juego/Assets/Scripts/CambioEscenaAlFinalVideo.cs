using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CambioEscenaAlFinalVideo : MonoBehaviour
{
    public VideoPlayer videoPlayer;      // Asigna el VideoPlayer desde el inspector

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        videoPlayer.loopPointReached += CambiarEscena; // Evento cuando termina el video
    }

    void CambiarEscena(VideoPlayer vp)
    {
        SceneManager.LoadScene(2);
    }
}
