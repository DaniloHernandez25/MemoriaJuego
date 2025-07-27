// NivelManager.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class NivelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelButton
    {
        public Button button;
        public Image  bgImage;
    }

    [Header("Botones y fondos")]
    public LevelButton[] niveles;
    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    // Ahora el setter es private; uso RegistrarNivel para cambiarlo
    public static int MaxNivelesCompletados { get; private set; } = -1;

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida  = PlayerPrefs.GetString("fechaSeleccionada", "");
        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("⚠️ Falta nombre o fecha en PlayerPrefs");
            return;
        }
        StartCoroutine(ObtenerYAplicarProgreso());
    }

    private IEnumerator ObtenerYAplicarProgreso()
    {
        var form = new WWWForm();
        form.AddField("nombre", nombreJugador);
        form.AddField("fecha",  fechaPartida);

        using var www = UnityWebRequest.Post(
            "http://localhost/EduardoDragon/obtener_niveles.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al obtener niveles: " + www.error);
            yield break;
        }

        var datos = JsonUtility.FromJson<NivelData>(www.downloadHandler.text.Trim());
        MaxNivelesCompletados = datos.niveles_completados;
        Debug.Log($"🌟 NivelManager: max niveles = {MaxNivelesCompletados}");

        for (int i = 0; i < niveles.Length; i++)
        {
            bool ok = i <= MaxNivelesCompletados;
            niveles[i].button.interactable = ok;
            niveles[i].bgImage.sprite     = ok ? unlockedSprite : lockedSprite;
            niveles[i].bgImage.SetNativeSize();
        }
    }

    // Llamar desde fuera para actualizar el máximo
    public static void RegistrarNivelCompletado(int nivel)
    {
        if (nivel > MaxNivelesCompletados)
            MaxNivelesCompletados = nivel;
    }

    [System.Serializable]
    private class NivelData
    {
        public int niveles_completados;
    }
}
