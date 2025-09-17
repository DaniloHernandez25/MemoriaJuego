using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GestorPartidas : MonoBehaviour
{
    [Header("UI Paneles")]
    public GameObject panelPartidas;           // Panel que contiene los ScrollViews de nombres y contenido
    public GameObject panelNivelesTiempo;      // Panel que contiene los ScrollViews de fases y niveles
    
    [Header("UI Partidas")]
    public GameObject botonNombrePrefab;       // Prefab con botón y texto para nombre
    public GameObject filaDetallePrefab;       // Prefab con texto para cada partida (ahora botón)
    public Transform contentNombres;           // Content del ScrollView de nombres
    public Transform contentDetalles;          // Content del ScrollView de detalles
    
    [Header("UI Niveles y Tiempos")]
    public GameObject botonFasePrefab;         // Prefab para botones de fases
    public GameObject filaNivelPrefab;         // Prefab para mostrar nivel y tiempo
    public Transform contentFases;             // Content del ScrollView de fases
    public Transform contentNiveles;           // Content del ScrollView de niveles
    public Button botonVolver;                 // Botón para volver al panel de partidas
    [Header("UI Mensajes")]
    public GameObject mensajeNoPartidasPrefab;

    private List<Partida> todasLasPartidas = new List<Partida>();
    private List<RegistroTiempo> todosLosTiempos = new List<RegistroTiempo>();
    private RegistroTiempo registroSeleccionado;

    [System.Serializable]
    public class Partida
    {
        public string nombre;
        public string fecha;
        public int fases;
        public int niveles;
    }
    
    [System.Serializable]
    public class RegistroTiempo
    {
        public string nombre;
        public string fecha;
        public Dictionary<string, float> tiempos = new Dictionary<string, float>();
    }

    void Start()
    {
        CargarDatosDesdeCSV();
        CargarTiemposDesdeCSV();
        MostrarNombresUnicos();
        ConfigurarBotonVolver();
        
        // Mostrar panel de partidas por defecto
        MostrarPanelPartidas();
    }

    void CargarDatosDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            return;
        }

        todasLasPartidas.Clear();

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
    
    void CargarTiemposDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso_tiempos.csv";

        if (!File.Exists(path))
        {
            return;
        }

        todosLosTiempos.Clear();

        using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (StreamReader reader = new StreamReader(fs))
        {
            string[] headers = null;
            bool primeraLinea = true;

            while (!reader.EndOfStream)
            {
                string linea = reader.ReadLine();

                if (primeraLinea)
                {
                    headers = linea.Split(',');
                    primeraLinea = false;
                    continue;
                }

                string[] columnas = linea.Split(',');
                if (columnas.Length < 2) continue;

                RegistroTiempo registro = new RegistroTiempo
                {
                    nombre = columnas[0].Trim().Replace("\uFEFF", ""),
                    fecha = columnas[1]
                };

                // Procesar los tiempos de cada nivel
                for (int i = 2; i < columnas.Length && i < headers.Length; i++)
                {
                    if (float.TryParse(columnas[i], out float tiempo) && tiempo > 0)
                    {
                        registro.tiempos[headers[i]] = tiempo;
                    }
                }

                todosLosTiempos.Add(registro);
            }
        }
    }

    void MostrarNombresUnicos()
    {
        // Limpiar solo los hijos del Content, no el Content mismo
        for (int i = contentNombres.childCount - 1; i >= 0; i--)
        {
            Destroy(contentNombres.GetChild(i).gameObject);
        }

        var nombresUnicos = todasLasPartidas
            .Select(p => p.nombre.Trim().Replace("\uFEFF", ""))
            .Distinct()
            .OrderBy(n => n);

        if (!nombresUnicos.Any())
        {
            // No hay partidas, mostrar mensaje durante 3 segundos
            if (mensajeNoPartidasPrefab != null)
            {
                StartCoroutine(MostrarMensajeTemporal(mensajeNoPartidasPrefab, 3f));
            }
            return;
        }

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

    private IEnumerator MostrarMensajeTemporal(GameObject prefab, float duracion)
    {
        GameObject mensaje = Instantiate(prefab, panelPartidas.transform); // se instancia como hijo del panel
        mensaje.SetActive(true);
        yield return new WaitForSeconds(duracion);
        Destroy(mensaje);
    }


    void MostrarDetallesDe(string nombre)
    {
        // Limpiar solo los hijos del Content, no el Content mismo
        for (int i = contentDetalles.childCount - 1; i >= 0; i--)
        {
            Destroy(contentDetalles.GetChild(i).gameObject);
        }

        var partidasJugador = todasLasPartidas
            .Where(p => p.nombre.Trim().Replace("\uFEFF", "") == nombre.Trim().Replace("\uFEFF", ""))
            .OrderByDescending(p => p.fecha);

        foreach (var p in partidasJugador)
        {
            GameObject fila = Instantiate(filaDetallePrefab, contentDetalles);
            fila.GetComponentInChildren<TextMeshProUGUI>().text = $"Fecha: {p.fecha} | Fases: {p.fases} | Niveles: {p.niveles}";
            
            // Convertir la fila en botón
            Button botonFila = fila.GetComponent<Button>();
            if (botonFila == null)
                botonFila = fila.AddComponent<Button>();
            
            // Capturar variables para el closure
            string fechaCapturada = p.fecha;
            string nombreCapturado = p.nombre;
            
            botonFila.onClick.RemoveAllListeners();
            botonFila.onClick.AddListener(() =>
            {
                MostrarPanelTiempos(nombreCapturado, fechaCapturada);
            });
        }
    }
    
    void MostrarPanelTiempos(string nombre, string fecha)
    {
        // Buscar el registro de tiempos correspondiente
        registroSeleccionado = todosLosTiempos
            .FirstOrDefault(r => r.nombre.Trim().Replace("\uFEFF", "") == nombre.Trim().Replace("\uFEFF", "") 
                               && r.fecha == fecha);
        
        if (registroSeleccionado == null)
        {
            Debug.LogWarning($"No se encontró registro de tiempos para {nombre} en fecha {fecha}");
            
            // Mostrar mensaje temporal de "no se encontró registro de tiempos"
            if (mensajeNoPartidasPrefab != null)
            {
                StartCoroutine(MostrarMensajeTemporal(mensajeNoPartidasPrefab, 3f));
            }
            return;
        }
        
        // Cambiar a panel de tiempos
        MostrarPanelNivelesTiempo();
        MostrarFases();
        MostrarNivelesDeFase(1); // Mostrar fase 1 por defecto
    }
    
    void MostrarFases()
    {
        foreach (Transform hijo in contentFases)
            Destroy(hijo.gameObject);
        
        // Siempre mostrar las 3 fases
        for (int fase = 1; fase <= 3; fase++)
        {
            int faseCapturada = fase;
            
            GameObject botonFase = Instantiate(botonFasePrefab, contentFases);
            botonFase.GetComponentInChildren<TextMeshProUGUI>().text = $"Fase {fase}";
            
            Button btn = botonFase.GetComponent<Button>();
            if (btn == null)
                btn = botonFase.AddComponent<Button>();
                
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                MostrarNivelesDeFase(faseCapturada);
            });
        }
    }
    
    void MostrarNivelesDeFase(int fase)
    {
        foreach (Transform hijo in contentNiveles)
            Destroy(hijo.gameObject);
        
        if (registroSeleccionado == null) return;
        
        // Mostrar niveles del 1 al 10 para la fase seleccionada
        for (int nivel = 1; nivel <= 10; nivel++)
        {
            string claveNivel = $"nivel{nivel}F{fase}";
            
            GameObject filaNivel = Instantiate(filaNivelPrefab, contentNiveles);
            TextMeshProUGUI textoNivel = filaNivel.GetComponentInChildren<TextMeshProUGUI>();
            
            if (registroSeleccionado.tiempos.ContainsKey(claveNivel))
            {
                float tiempo = registroSeleccionado.tiempos[claveNivel];
                textoNivel.text = $"Nivel {nivel} | Tiempo: {tiempo:F2}s";
            }
            else
            {
                textoNivel.text = $"Nivel {nivel} | Tiempo: --";
            }
        }
    }
    
    void ConfigurarBotonVolver()
    {
        if (botonVolver != null)
        {
            botonVolver.onClick.RemoveAllListeners();
            botonVolver.onClick.AddListener(MostrarPanelPartidas);
        }
    }
    
    void MostrarPanelPartidas()
    {
        panelPartidas.SetActive(true);
        panelNivelesTiempo.SetActive(false);
    }
    
    void MostrarPanelNivelesTiempo()
    {
        panelPartidas.SetActive(false);
        panelNivelesTiempo.SetActive(true);
    }
}