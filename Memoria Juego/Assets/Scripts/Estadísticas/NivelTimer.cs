using UnityEngine;
using System.IO;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System;

public class NivelTimer : MonoBehaviour
{
    [Header("Datos del nivel")]
    public int numeroNivel = 1;   // 1 a 10
    public int numeroFase = 1;    // 1 a 3

    private float tiempoInicio;
    private bool nivelCompletado = false;
    private string filePath;

    void Start()
    {
        tiempoInicio = Time.time;
        filePath = Path.Combine(Application.persistentDataPath, "progreso_tiempos.csv");

        // Suscribirse a los eventos de finalización de nivel
        CardsController.OnNivelCompletado += RegistrarTiempo;
        CardsControllerHard.OnNivelCompletado += RegistrarTiempo;
        VerificarSuma.OnNivelCompletado += RegistrarTiempo;
        VerificarResta.OnNivelCompletado += RegistrarTiempo;
        VerificarSumaResta.OnNivelCompletado += RegistrarTiempo;
        JuegoSilabas.OnNivelCompletado += RegistrarTiempo;
        JuegoLetras.OnNivelCompletado += RegistrarTiempo;
    }

    private void OnDestroy()
    {
        CardsController.OnNivelCompletado -= RegistrarTiempo;
        CardsControllerHard.OnNivelCompletado -= RegistrarTiempo;
        VerificarSuma.OnNivelCompletado -= RegistrarTiempo;
        VerificarResta.OnNivelCompletado -= RegistrarTiempo;
        VerificarSumaResta.OnNivelCompletado -= RegistrarTiempo;
        JuegoSilabas.OnNivelCompletado -= RegistrarTiempo;
        JuegoLetras.OnNivelCompletado -= RegistrarTiempo;
    }

    void RegistrarTiempo()
    {
        if (nivelCompletado) return;
        nivelCompletado = true;

        float tiempoTranscurridoSegundos = Time.time - tiempoInicio;

        // Convertir a hh:mm:ss
        TimeSpan tiempo = TimeSpan.FromSeconds(tiempoTranscurridoSegundos);
        string tiempoFormateado = tiempo.ToString(@"hh\:mm\:ss");

        string nombre = PlayerPrefs.GetString("nombreJugador", "Jugador");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        // Crear CSV si no existe
        if (!File.Exists(filePath))
        {
            List<string> cabeceras = new List<string> { "Nombre", "Fecha" };
            for (int f = 1; f <= 3; f++)
                for (int n = 1; n <= 10; n++)
                    cabeceras.Add($"nivel{n}F{f}");
            File.WriteAllText(filePath, string.Join(",", cabeceras) + "\n");
        }

        // Leer todas las líneas
        string[] lineas = File.ReadAllLines(filePath);
        List<string> lineasList = new List<string>(lineas);

        // Buscar si ya existe fila para este jugador+fecha
        int filaIndex = -1;
        for (int i = 1; i < lineasList.Count; i++)
        {
            string[] columnas = lineasList[i].Split(',');
            if (columnas.Length >= 2 && columnas[0] == nombre && columnas[1] == fecha)
            {
                filaIndex = i;
                break;
            }
        }

        // Si no existe fila, crear nueva
        if (filaIndex == -1)
        {
            string[] nuevasColumnas = new string[2 + 30]; // 30 niveles (10 x 3 fases)
            nuevasColumnas[0] = nombre;
            nuevasColumnas[1] = fecha;
            for (int i = 2; i < nuevasColumnas.Length; i++) nuevasColumnas[i] = "";
            lineasList.Add(string.Join(",", nuevasColumnas));
            filaIndex = lineasList.Count - 1;
        }

        // Actualizar la columna correspondiente con hh:mm:ss
        string[] fila = lineasList[filaIndex].Split(',');
        int columna = 2 + (numeroFase - 1) * 10 + (numeroNivel - 1);
        fila[columna] = tiempoFormateado;

        lineasList[filaIndex] = string.Join(",", fila);

        try
        {
            File.WriteAllLines(filePath, lineasList);
            Debug.Log($"Tiempo registrado en {tiempoFormateado} para {nombre} - {fecha}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV: {e.Message}");
        }
    }
}
