using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NivelManager : MonoBehaviour
{
    public Button[] botonesDeNivel; // Asigna los botones de nivel en el Inspector

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");

        Debug.Log("🎮 Nombre en NivelManager: " + nombreJugador);
        Debug.Log("📅 Fecha en NivelManager: " + fechaPartida);

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("⚠️ Faltan datos en PlayerPrefs");
            return;
        }

        StartCoroutine(ObtenerNiveles(nombreJugador, fechaPartida));
    }

    private IEnumerator ObtenerNiveles(string nombre, string fecha)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fecha", fecha);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/obtener_niveles.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error al obtener niveles: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text.Trim();
            Debug.Log("📦 JSON recibido (raw): " + json);

            if (json.StartsWith("{") && json.Contains("niveles_completados"))
            {
                NivelData datos = JsonUtility.FromJson<NivelData>(json);
                Debug.Log("✅ Niveles completados: " + datos.niveles_completados);
                DesbloquearBotones(datos.niveles_completados);
            }
            else
            {
                Debug.LogWarning("⚠️ JSON inválido o campos faltantes.");
                DesbloquearBotones(0);
            }
        }
    }

    private void DesbloquearBotones(int nivelesCompletados)
    {
        for (int i = 0; i < botonesDeNivel.Length; i++)
        {
            bool desbloqueado = i <= nivelesCompletados; // Nivel 0 se desbloquea si niveles_completados = 0
            botonesDeNivel[i].interactable = desbloqueado;
            Debug.Log($"🔓 Botón Nivel {i}: {(desbloqueado ? "Desbloqueado" : "Bloqueado")}");
        }
    }

    [System.Serializable]
    public class NivelData
    {
        public int niveles_completados;
    }
}
