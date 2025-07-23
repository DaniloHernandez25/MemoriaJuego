using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

public class ListaDePartidas : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject botonPrefab;
    public Transform contentPanel;
    private string nombreJugador;
    public int escenaFases = 5;


    [System.Serializable]
    public class Partida
    {
        public string fecha;
    }

    private void Start()
    {
        // 🔹 Obtener el nombre del jugador desde PlayerPrefs
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            Debug.LogWarning("No se encontró el nombre del jugador");
            return;
        }

        CargarPartidas();
    }


    public void CargarPartidas()
    {
        StartCoroutine(ObtenerFechas(nombreJugador));
    }

    IEnumerator ObtenerFechas(string nombre)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/obtener_fechas.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
            yield break;
        }

        Partida[] partidas = JsonHelper.FromJson<Partida>(JsonHelper.FixJsonArray(www.downloadHandler.text));

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
        foreach (Partida p in partidas)
        {
            GameObject boton = Instantiate(botonPrefab, contentPanel);
            PartidaButton script = boton.GetComponent<PartidaButton>();

            // ✅ configuramos la fecha y la escena que queremos cargar
            script.Configurar(p.fecha, escenaFases);
        }

    }
}
