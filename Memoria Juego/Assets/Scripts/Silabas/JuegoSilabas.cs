using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
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
    
    public static event System.Action OnNivelCompletado;
    
    public int indiceEscenaAlGanar = -1;


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
            OnNivelCompletado?.Invoke();

            // 👇 Llamar a la corutina local
            StartCoroutine(EsperarYCargar());

            IEnumerator EsperarYCargar()
            {
                yield return new WaitForSeconds(2f);
                UnityEngine.SceneManagement.SceneManager.LoadScene(indiceEscenaAlGanar);
            }
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
        string nombre = PlayerPrefs.GetString("nombreJugador", "");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", "");
        int nivelSeleccionadoRaw = PlayerPrefs.GetInt("nivelSeleccionado", -1);

        int nivelCompletado;
        if (nivelSeleccionadoRaw >= 0 && nivelSeleccionadoRaw < 30)
        {
            nivelCompletado = nivelSeleccionadoRaw + 1;
            Debug.Log($"⚠️ CONVERSIÓN: Nivel raw {nivelSeleccionadoRaw} convertido a {nivelCompletado}");
        }
        else
        {
            nivelCompletado = nivelSeleccionadoRaw;
        }

        Debug.Log($"Guardando progreso: Nombre={nombre}, Fecha={fecha}, Nivel={nivelCompletado}");

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || nivelCompletado < 1)
        {
            Debug.LogError("Datos incompletos para guardar progreso en VerificarSuma");
            yield break;
        }

        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Nombre,Fecha,Fases Completadas,Niveles Completados\n");
            Debug.Log("Archivo CSV creado");
        }

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            for (int j = 0; j < columnas.Length; j++) columnas[j] = columnas[j].Trim();

            if (columnas.Length < 4) continue;

            if (columnas[0] == nombre && columnas[1] == fecha)
            {
                if (!int.TryParse(columnas[3], out int nivelesActuales)) nivelesActuales = 0;

                if (nivelCompletado == nivelesActuales)
                {
                    int nuevoProgreso = nivelesActuales + 1;
                    columnas[3] = nuevoProgreso.ToString();
                    lineas[i] = string.Join(",", columnas);
                    encontrado = true;

                    Debug.Log($"¡Nivel completado! Progreso: {nivelesActuales} -> {nuevoProgreso}");
                    NivelManager.RegistrarNivelCompletado(nuevoProgreso);
                }
                else
                {
                    encontrado = true;
                    Debug.Log($"Nivel {nivelCompletado} no coincide con el disponible {nivelesActuales}");
                }
                break;
            }
        }

        if (!encontrado)
        {
            string nuevaLinea = $"{nombre},{fecha},0,{nivelCompletado}";
            List<string> lineasList = new List<string>(lineas) { nuevaLinea };
            lineas = lineasList.ToArray();

            Debug.Log($"Nuevo registro creado: {nuevaLinea}");
            NivelManager.RegistrarNivelCompletado(nivelCompletado);
        }

        try
        {
            File.WriteAllLines(path, lineas);
            Debug.Log("CSV guardado exitosamente desde VerificarSuma");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV en VerificarSuma: {e.Message}");
        }

        yield return null;
    }
}
