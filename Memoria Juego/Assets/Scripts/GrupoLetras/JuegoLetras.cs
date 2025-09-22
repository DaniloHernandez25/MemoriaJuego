using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class JuegoLetras : MonoBehaviour
{
    [Header("UI Letras Grupo 1 (5 TMP_Texts)")]
    public List<TMP_Text> grupo1UI;

    [Header("UI Letras Grupo 2 (5 TMP_Texts)")]
    public List<TMP_Text> grupo2UI;

    [Header("Pregunta")]
    public TMP_Text preguntaTexto;

    [Header("Botones")]
    public Button botonGrupo1;
    public Button botonGrupo2;

    [Header("Prefabs Resultado")]
    public GameObject prefabGanaste;
    public GameObject prefabIntentalo;

    private List<char> grupo1Letras = new List<char>();
    private List<char> grupo2Letras = new List<char>();
    private List<char> letrasUsadas = new List<char>(); // Letras ya preguntadas

    private char letraActual;
    private int grupoCorrecto; // 1 o 2
    private int aciertos = 0;
    private int aciertosNecesarios = 5;
    private bool juegoTerminado = false;

    void Start()
    {
        GenerarLetras();

        // Eventos botones
        botonGrupo1.onClick.AddListener(() => VerificarRespuesta(1));
        botonGrupo2.onClick.AddListener(() => VerificarRespuesta(2));

        NuevaPregunta();
    }

    void GenerarLetras()
    {
        List<char> letrasDisponibles = new List<char>("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());

        // Mezclar aleatoriamente
        for (int i = letrasDisponibles.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            char temp = letrasDisponibles[i];
            letrasDisponibles[i] = letrasDisponibles[j];
            letrasDisponibles[j] = temp;
        }

        // Primeros 5 letras para grupo 1
        for (int i = 0; i < 5; i++)
        {
            grupo1Letras.Add(letrasDisponibles[i]);
            grupo1UI[i].text = letrasDisponibles[i].ToString();
        }

        // Siguientes 5 letras para grupo 2
        for (int i = 5; i < 10; i++)
        {
            grupo2Letras.Add(letrasDisponibles[i]);
            grupo2UI[i - 5].text = letrasDisponibles[i].ToString();
        }
    }

    void NuevaPregunta()
    {
        if (juegoTerminado) return;

        List<char> posiblesLetras;
        int grupoElegido = Random.Range(1, 3);

        posiblesLetras = grupoElegido == 1 ? new List<char>(grupo1Letras) : new List<char>(grupo2Letras);
        posiblesLetras.RemoveAll(l => letrasUsadas.Contains(l));

        if (posiblesLetras.Count == 0)
        {
            // Cambiar grupo si no quedan letras
            grupoElegido = grupoElegido == 1 ? 2 : 1;
            posiblesLetras = grupoElegido == 1 ? new List<char>(grupo1Letras) : new List<char>(grupo2Letras);
            posiblesLetras.RemoveAll(l => letrasUsadas.Contains(l));
        }

        if (posiblesLetras.Count == 0)
        {
            // No quedan letras → fin del juego
            GanarJuego();
            return;
        }

        grupoCorrecto = grupoElegido;
        letraActual = posiblesLetras[Random.Range(0, posiblesLetras.Count)];
        letrasUsadas.Add(letraActual);

        preguntaTexto.text = $"¿En qué grupo está la letra <b>{letraActual}</b>?";
    }

    void VerificarRespuesta(int grupoElegido)
    {
        if (juegoTerminado) return;

        if (grupoElegido == grupoCorrecto)
        {
            aciertos++;
            Debug.Log($"✅ Correcto. Aciertos: {aciertos}");

            if (aciertos >= aciertosNecesarios)
            {
                GanarJuego();
            }
            else
            {
                NuevaPregunta();
            }
        }
        else
        {
            Debug.Log("❌ Incorrecto");
            StartCoroutine(MostrarMensajeTemporal());
        }
    }

    void GanarJuego()
    {
        if (juegoTerminado) return;

        juegoTerminado = true;

        // Bloquear botones
        botonGrupo1.interactable = false;
        botonGrupo2.interactable = false;

        Instantiate(prefabGanaste, FindFirstObjectByType<Canvas>().transform, false);
        StartCoroutine(GuardarProgresoEnCSV());
        Debug.Log("🎉 ¡Ganaste el juego de letras!");
    }

    private IEnumerator MostrarMensajeTemporal()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject mensaje = Instantiate(prefabIntentalo, canvas.transform, false);

        yield return new WaitForSeconds(2f);

        Destroy(mensaje);
    }

    private IEnumerator GuardarProgresoEnCSV()
    {
        string nombre = PlayerPrefs.GetString("nombreJugador", "Jugador");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", System.DateTime.Now.ToString("yyyy-MM-dd"));
        int nivelSeleccionadoRaw = PlayerPrefs.GetInt("nivelSeleccionado", 0);

        int nivelCompletado = nivelSeleccionadoRaw + 1;

        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
            File.WriteAllText(path, "Nombre,Fecha,Niveles Completados\n");

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            if (columnas.Length < 3) continue;

            if (columnas[0] == nombre && columnas[1] == fecha)
            {
                if (!int.TryParse(columnas[2], out int nivelesActuales)) nivelesActuales = 0;
                int nuevoProgreso = Mathf.Max(nivelesActuales, nivelCompletado);

                columnas[2] = nuevoProgreso.ToString();
                lineas[i] = string.Join(",", columnas);
                encontrado = true;
                break;
            }
        }

        if (!encontrado)
        {
            string nuevaLinea = $"{nombre},{fecha},{nivelCompletado}";
            List<string> lineasList = new List<string>(lineas) { nuevaLinea };
            lineas = lineasList.ToArray();
        }

        try
        {
            File.WriteAllLines(path, lineas);
            Debug.Log("CSV guardado exitosamente ✅");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV: {e.Message}");
        }

        yield return null;
    }
}
