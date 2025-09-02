using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance;

    public List<int> nivelesDesbloqueados = new List<int>();

    private string nombreJugador;
    private string fechaSeleccionada;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
            fechaSeleccionada = PlayerPrefs.GetString("fechaSeleccionada", "");

            if (!string.IsNullOrEmpty(nombreJugador) && !string.IsNullOrEmpty(fechaSeleccionada))
            {
                StartCoroutine(CargarProgresoDesdeBD(nombreJugador, fechaSeleccionada));
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator CargarProgresoDesdeBD(string nombre, string fecha)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fecha", fecha);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/obtener_progreso.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener progreso: " + www.error);
        }
        else
        {
            string json = www.downloadHandler.text;
            ProgresoDatos progreso = JsonUtility.FromJson<ProgresoDatos>(json);

            for (int i = 1; i <= progreso.niveles_completados; i++)
            {
                nivelesDesbloqueados.Add(i);
            }

        }
    }


    [System.Serializable]
    public class ProgresoDatos
    {
        public int fases_completadas;
        public int niveles_completados;
    }

    public bool EstaDesbloqueado(int sceneIndex)
    {
        return nivelesDesbloqueados.Contains(sceneIndex);
    }

    public void DesbloquearNivel(int sceneIndex)
    {
        if (!nivelesDesbloqueados.Contains(sceneIndex))
        {
            nivelesDesbloqueados.Add(sceneIndex);
        }
    }
}
