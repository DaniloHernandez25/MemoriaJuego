// NivelManagerCSV.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using TMPro;

public class NivelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelButton
    {
        public Button button;          // tu botón
        public Image bgImage;          // su fondo
        public TextMeshProUGUI label;  // TextMeshPro para el número/texto
        [Tooltip("Texto que se mostrará cuando esté desbloqueado")]
        public string unlockedText;    // editable en Unity
    }

    [Header("Botones y fondos")]
    public LevelButton[] niveles;

    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    public static int MaxNivelesCompletados { get; private set; } = -1;

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");
        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("⚠️ Falta nombre o fecha en PlayerPrefs");
            return;
        }

        LeerProgresoDesdeCSV();
        AplicarProgresoALosBotones();
    }

    private void LeerProgresoDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ No se encontró el archivo CSV de progreso.");
            MaxNivelesCompletados = 0;
            return;
        }

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        foreach (string linea in lineas)
        {
            if (linea.StartsWith("Nombre,")) continue; // Saltar cabecera

            string[] columnas = linea.Split(',');

            // Columnas: Nombre, Fecha, FasesCompletadas, NivelesCompletados
            if (columnas.Length >= 4 && columnas[0] == nombreJugador && columnas[1] == fechaPartida)
            {
                if (!int.TryParse(columnas[3], out int nivelesCompletados))
                {
                    Debug.LogWarning("⚠️ Error al leer niveles completados en CSV");
                    nivelesCompletados = 0;
                }

                MaxNivelesCompletados = nivelesCompletados;
                encontrado = true;
                Debug.Log($"🌟 NivelManagerCSV: max niveles = {MaxNivelesCompletados}");
                break;
            }
        }

        if (!encontrado)
        {
            MaxNivelesCompletados = 0;
            Debug.LogWarning("⚠️ No se encontró progreso para el jugador/fecha");
        }
    }

    private void AplicarProgresoALosBotones()
    {
        for (int i = 0; i < niveles.Length; i++)
        {
            bool desbloqueado = i <= MaxNivelesCompletados;

            var lvl = niveles[i];

            lvl.button.interactable = desbloqueado;
            lvl.bgImage.sprite = desbloqueado ? unlockedSprite : lockedSprite;
            lvl.bgImage.SetNativeSize();

            if (lvl.label != null)
            {
                lvl.label.gameObject.SetActive(desbloqueado);
                if (desbloqueado)
                    lvl.label.text = lvl.unlockedText;
            }
        }
    }

    public static void RegistrarNivelCompletado(int nivel)
    {
        if (nivel > MaxNivelesCompletados)
            MaxNivelesCompletados = nivel;
    }

    void Awake()
    {
        foreach (var lvl in niveles)
            if (lvl.label != null)
                lvl.label.gameObject.SetActive(false);
    }
}
