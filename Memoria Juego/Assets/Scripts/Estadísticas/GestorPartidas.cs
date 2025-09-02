using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GestorPartidas : MonoBehaviour
{
    [Header("UI")]
    public GameObject botonNombrePrefab;       // Prefab con botón y texto para nombre
    public GameObject filaDetallePrefab;       // Prefab con texto para cada partida
    public Transform contentNombres;           // Content del ScrollView de nombres
    public Transform contentDetalles;          // Content del ScrollView de detalles

    private List<Partida> todasLasPartidas = new List<Partida>();

    [System.Serializable]
    public class Partida
    {
        public string nombre;
        public string fecha;
        public int fases;
        public int niveles;
    }

    void Start()
    {
        CargarDatosDesdeCSV();
        MostrarNombresUnicos();
    }

    void CargarDatosDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            return;
        }

        todasLasPartidas.Clear(); // 💡 Evita duplicados si se recarga

        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(fs))
        {
            bool primeraLinea = true;

            while (!reader.EndOfStream)
            {
                string linea = reader.ReadLine();

                if (primeraLinea)
                {
                    primeraLinea = false;
                    continue;
                }

                string[] columnas = linea.Split(',');
                if (columnas.Length < 4) continue;

                Partida p = new Partida
                {
                    nombre = columnas[0].Trim().Replace("\uFEFF", ""),
                    fecha = columnas[1],
                    fases = int.TryParse(columnas[2], out int f) ? f : 0,
                    niveles = int.TryParse(columnas[3], out int n) ? n : 0
                };

                todasLasPartidas.Add(p);
            }
        }
    }

    void MostrarNombresUnicos()
    {
        foreach (Transform hijo in contentNombres)
            Destroy(hijo.gameObject);

        var nombresUnicos = todasLasPartidas
            .Select(p => p.nombre.Trim().Replace("\uFEFF", ""))
            .Distinct()
            .OrderBy(n => n);

        foreach (string nombre in nombresUnicos)
        {
            string nombreCapturado = nombre;

            GameObject boton = Instantiate(botonNombrePrefab, contentNombres);
            boton.GetComponentInChildren<TextMeshProUGUI>().text = nombreCapturado;

            boton.GetComponent<Button>().onClick.RemoveAllListeners();
            boton.GetComponent<Button>().onClick.AddListener(() =>
            {
                MostrarDetallesDe(nombreCapturado);
            });
        }
    }

    void MostrarDetallesDe(string nombre)
    {
        foreach (Transform hijo in contentDetalles)
            Destroy(hijo.gameObject);

        var partidasJugador = todasLasPartidas
            .Where(p => p.nombre.Trim().Replace("\uFEFF", "") == nombre.Trim().Replace("\uFEFF", ""))
            .OrderByDescending(p => p.fecha);

        foreach (var p in partidasJugador)
        {
            GameObject fila = Instantiate(filaDetallePrefab, contentDetalles);
            fila.GetComponentInChildren<TextMeshProUGUI>().text = $"Fecha: {p.fecha} | Fases: {p.fases} | Niveles: {p.niveles}";
        }
    }
}
