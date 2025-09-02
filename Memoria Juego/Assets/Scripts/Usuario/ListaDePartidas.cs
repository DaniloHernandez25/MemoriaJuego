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
        public int fases;
        public int niveles;
    }

    private void Start()
    {
        // Obtener el nombre del jugador desde PlayerPrefs
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            return;
        }

        CargarPartidasDesdeCSV();
    }

    public void CargarPartidasDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            return;
        }

        List<Partida> partidas = new List<Partida>();
        string[] lineas;

        // ✅ Lectura segura que evita IOException
        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(fs))
        {
            List<string> tempLineas = new List<string>();
            while (!reader.EndOfStream)
            {
                tempLineas.Add(reader.ReadLine());
            }
            lineas = tempLineas.ToArray();
        }

        bool primeraLinea = true;
        foreach (string linea in lineas)
        {
            if (primeraLinea)
            {
                primeraLinea = false;
                continue; // Saltar cabecera
            }

            string[] columnas = linea.Split(',');

            if (columnas.Length >= 4 && columnas[0] == nombreJugador)
            {
                Partida partida = new Partida();
                partida.fecha = columnas[1];

                int.TryParse(columnas[2], out partida.fases);
                int.TryParse(columnas[3], out partida.niveles);

                partidas.Add(partida);
            }
        }

        MostrarPartidas(partidas);
    }


    private void MostrarPartidas(List<Partida> partidas)
    {
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (Partida p in partidas)
        {
            GameObject boton = Instantiate(botonPrefab, contentPanel);
            PartidaButton script = boton.GetComponent<PartidaButton>();

            // ✅ Configurar botón con fecha, fases y niveles
            script.Configurar(p.fecha, p.fases, p.niveles, escenaFases);
        }
    }
}
