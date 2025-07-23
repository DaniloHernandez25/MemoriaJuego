using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class FaseManager : MonoBehaviour
{
    public Button[] botonesDeFase; // Asignar los botones en el Inspector

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");

        Debug.Log("💡 Nombre en FaseManager: " + nombreJugador);
        Debug.Log("💡 Fecha en FaseManager: " + fechaPartida);

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("⚠️ Faltan datos en PlayerPrefs");
            return;
        }

        StartCoroutine(ObtenerFases(nombreJugador, fechaPartida));
    }

    private IEnumerator ObtenerFases(string nombre, string fecha)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fecha", fecha);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/obtener_fases.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error al obtener fases: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text.Trim();
            Debug.Log("📦 JSON recibido (raw): " + json);

            if (json.StartsWith("{") && json.Contains("fases_completadas"))
            {
                FaseData datos = JsonUtility.FromJson<FaseData>(json);
                Debug.Log("✅ Fases completadas (parseado): " + datos.fases_completadas);
                DesbloquearBotones(datos.fases_completadas);
            }
            else
            {
                Debug.LogWarning("⚠️ JSON inválido o campos faltantes.");
                DesbloquearBotones(0);
            }
        }
    }

    private void DesbloquearBotones(int fasesCompletadas)
    {
        for (int i = 0; i < botonesDeFase.Length; i++)
        {
            if (fasesCompletadas <= 0)
            {
                // Solo el primer botón activo si no hay progreso
                botonesDeFase[i].interactable = (i == 0);
            }
            else
            {
                // Desbloquea desde el primero hasta el número indicado
                botonesDeFase[i].interactable = (i < fasesCompletadas);
            }

            Debug.Log($"🔓 Botón {i}: {(botonesDeFase[i].interactable ? "Desbloqueado" : "Bloqueado")}");
        }
    }


    [System.Serializable]
    public class FaseData
    {
        public int fases_completadas;
    }
}
