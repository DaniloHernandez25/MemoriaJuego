using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class CrearPartida : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = -1; // 

    public void AlCrearPartida()
    {
        string nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            Debug.LogWarning("No hay nombre guardado en PlayerPrefs.");
            return;
        }

        // Crea nueva partida con 0 fases y 0 niveles
        StartCoroutine(EnviarProgreso(nombreJugador, 0, 0));
    }

    private IEnumerator EnviarProgreso(string nombre, int fases, int niveles)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fases", fases);
        form.AddField("niveles", niveles);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/guardar_progreso.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al guardar progreso: " + www.error);
        }
        else
        {
            Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
            SceneManager.LoadScene(sceneToLoad); // Cambia de escena si fue exitoso
        }
    }
}
