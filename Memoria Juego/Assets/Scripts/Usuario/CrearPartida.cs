using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class CrearPartida : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = -1;

    public void AlCrearPartida()
    {
        string nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            Debug.LogWarning("No hay nombre guardado en PlayerPrefs.");
            return;
        }

        // 🔹 Generar una nueva fecha para esta partida
        string fechaPartida = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 🔸 Guardar esa fecha en PlayerPrefs
        PlayerPrefs.SetString("fechaSeleccionada", fechaPartida);
        PlayerPrefs.Save();
        Debug.Log("Fecha de partida guardada localmente: " + fechaPartida);

        // Crear nueva partida con 0 niveles
        StartCoroutine(EnviarProgreso(nombreJugador, fechaPartida, 0));
    }

    private IEnumerator EnviarProgreso(string nombre, string fecha, int niveles)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fecha",  fecha);
        form.AddField("niveles", niveles);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/crear_partida.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al guardar progreso: " + www.error);
            }
            else
            {
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
                // 👇 Cambiar de escena una vez confirmada la creación
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
