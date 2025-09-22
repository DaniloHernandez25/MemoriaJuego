using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO; // 👈 Necesario para File

public class JuegoSilabas : MonoBehaviour
{
    [Header("Listas de palabras")]
    public List<string> palabras1Silaba;
    public List<string> palabras2Silabas;
    public List<string> palabras3Silabas;

    [Header("UI")]
    public TMP_Text palabraTexto;
    public Button boton1Silaba;
    public Button boton2Silabas;
    public Button boton3Silabas;

    [Header("Prefabs Resultado")]
    public GameObject prefabGanaste;
    public GameObject prefabIntentalo;

    private string palabraActual;
    private int respuestaCorrecta; // 1, 2 o 3 según la lista
    private int intentosMaximos = 5;
    private int intentosRealizados = 0;
    private int aciertos = 0;
    private bool juegoTerminado = false; // 👈 bandera de control

    void Start()
    {
        // Asignar funciones a los botones
        boton1Silaba.onClick.AddListener(() => VerificarRespuesta(1));
        boton2Silabas.onClick.AddListener(() => VerificarRespuesta(2));
        boton3Silabas.onClick.AddListener(() => VerificarRespuesta(3));

        GenerarNuevaPalabra();
    }

    void GenerarNuevaPalabra()
    {
        if (intentosRealizados >= intentosMaximos || juegoTerminado)
            return;

        int listaElegida = Random.Range(1, 4); // 1, 2 o 3
        switch (listaElegida)
        {
            case 1:
                palabraActual = palabras1Silaba[Random.Range(0, palabras1Silaba.Count)];
                respuestaCorrecta = 1;
                break;
            case 2:
                palabraActual = palabras2Silabas[Random.Range(0, palabras2Silabas.Count)];
                respuestaCorrecta = 2;
                break;
            case 3:
                palabraActual = palabras3Silabas[Random.Range(0, palabras3Silabas.Count)];
                respuestaCorrecta = 3;
                break;
        }

        palabraTexto.text = palabraActual;
        intentosRealizados++;
    }

    void VerificarRespuesta(int respuestaJugador)
    {
        if (juegoTerminado) return; // 👈 evita que siga procesando tras ganar/perder

        if (respuestaJugador == respuestaCorrecta)
        {
            aciertos++;
            if (aciertos >= intentosMaximos)
            {
                FinDelJuego();
            }
            else
            {
                GenerarNuevaPalabra();
            }
        }
        else
        {
            Debug.Log("❌ Fallo, intenta de nuevo");
            StartCoroutine(MostrarMensajeTemporal());
        }
    }

    void FinDelJuego()
    {
        if (juegoTerminado) return; // 👈 evita múltiples ejecuciones
        juegoTerminado = true;

        // Desactiva botones para evitar más clics
        boton1Silaba.interactable = false;
        boton2Silabas.interactable = false;
        boton3Silabas.interactable = false;

        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (aciertos >= intentosMaximos)
        {
            Instantiate(prefabGanaste, canvas.transform, false);
            Debug.Log("🎉 ¡Ganaste!");
            StartCoroutine(GuardarProgresoEnCSV()); // 👈 GUARDAR PROGRESO AQUÍ
        }
        else
        {
            StartCoroutine(MostrarMensajeTemporal());
        }
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
        {
            File.WriteAllText(path, "Nombre,Fecha,Niveles Completados\n");
        }

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
