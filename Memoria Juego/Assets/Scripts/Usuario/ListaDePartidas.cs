using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        // Obtener el nombre del jugador desde PlayerPrefs
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            Debug.LogWarning("No se encontró el nombre del jugador");
            return;
        }

        CargarPartidasDesdeCSV(); // Cargar las partidas desde el archivo CSV
    }

    public void CargarPartidasDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            Debug.LogWarning("No se encontró el archivo CSV de partidas.");
            return;
        }

        List<Partida> partidas = new List<Partida>();

        // Leer las líneas del archivo CSV
        string[] lineas = File.ReadAllLines(path);

        // Saltar la cabecera
        bool primeraLinea = true;

        foreach (string linea in lineas)
        {
            if (primeraLinea)
            {
                primeraLinea = false;
                continue; // Saltar la cabecera
            }

            string[] columnas = linea.Split(',');

            if (columnas.Length >= 4 && columnas[0] == nombreJugador) // Filtrar solo las partidas del jugador
            {
                Partida partida = new Partida();
                partida.fecha = columnas[1];  // La fecha está en la segunda columna
                partidas.Add(partida);
            }
        }

        MostrarPartidas(partidas);
    }

    private void MostrarPartidas(List<Partida> partidas)
    {
        // Limpiar los botones existentes
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Crear un botón para cada partida
        foreach (Partida p in partidas)
        {
            GameObject boton = Instantiate(botonPrefab, contentPanel);
            PartidaButton script = boton.GetComponent<PartidaButton>();

            // Configuramos el botón con la fecha de la partida
            script.Configurar(p.fecha, escenaFases);
        }
    }
}
