using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class NivelManager : MonoBehaviour
{
    [System.Serializable]
    public struct LevelButton
    {
        public Button button;
        public Image bgImage;
        public TextMeshProUGUI label;
        [Tooltip("Texto que se mostrará cuando esté desbloqueado")]
        public string unlockedText;
    }

    [Header("Botones y fondos")]
    public LevelButton[] niveles;

    [Header("Sprites")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    [Header("Configuración de escena")]
    [Tooltip("Nivel inicial de esta escena (ej. 1 para fase 1, 11 para fase 2, 21 para fase 3...)")]
    public int nivelInicial = 1;

    [Tooltip("Nivel final de esta escena (ej. 10 para fase 1, 20 para fase 2, 30 para fase 3...)")]
    public int nivelFinal = 10;

    public static int MaxNivelesCompletados { get; private set; } = -1;

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");
        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
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
            MaxNivelesCompletados = 0;
            return;
        }

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        foreach (string linea in lineas)
        {
            if (linea.StartsWith("Nombre,")) continue;

            string[] columnas = linea.Split(',');

            if (columnas.Length >= 4 && columnas[0] == nombreJugador && columnas[1] == fechaPartida)
            {
                if (!int.TryParse(columnas[3], out int nivelesCompletados))
                {
                    nivelesCompletados = 0;
                }

                MaxNivelesCompletados = nivelesCompletados;
                encontrado = true;
                break;
            }
        }

        if (!encontrado)
        {
            MaxNivelesCompletados = 0;
        }
    }

    private void AplicarProgresoALosBotones()
    {
        for (int i = 0; i < niveles.Length; i++)
        {
            int numeroNivel = nivelInicial + i; // 👈 nivel real que representa este botón
            bool desbloqueado = numeroNivel <= MaxNivelesCompletados || (MaxNivelesCompletados == 0 && numeroNivel == nivelInicial);

            var lvl = niveles[i];

            lvl.button.interactable = desbloqueado;
            lvl.bgImage.sprite = desbloqueado ? unlockedSprite : lockedSprite;
            RectTransform imageRect = lvl.bgImage.GetComponent<RectTransform>();
            RectTransform buttonRect = lvl.button.GetComponent<RectTransform>();

            imageRect.sizeDelta = buttonRect.sizeDelta; // Ajusta la imagen al tamaño del botón

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
