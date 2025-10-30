using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class GestorPartidasFiltro : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject panelBusqueda;
    public GameObject panelResultados;

    [Header("Botones")]
    public Button botonVolver;

    [Header("UI Búsqueda")]
    public TMP_Dropdown dropdownNombre;
    public TMP_InputField inputFechaInicio;
    public TMP_InputField inputFechaFin;
    public TMP_Dropdown dropdownFase;
    public Button botonBuscar;

    [Header("UI Tabla Resultados")]
    public Transform tablaContenido;
    public GameObject filaPrefab;

    [Header("UI Resultados")]
    public GameObject prefabSinRegistro;
    public Transform spawnPrefabSinRegistro;

    private List<RegistroTiempo> todosLosTiempos = new List<RegistroTiempo>();
    private List<string> todosLosNombres = new List<string>();

    [System.Serializable]
    public class RegistroTiempo
    {
        public string nombre;
        public string fecha;
        public Dictionary<string, string> tiempos = new Dictionary<string, string>();
    }

    void Start()
    {
        CargarTiemposDesdeCSV();
        InicializarDropdownNombres();
        InicializarDropdownFases();

        dropdownNombre.onValueChanged.AddListener(delegate { /* no necesario actualizar fechas */ });

        if (botonBuscar != null)
            botonBuscar.onClick.AddListener(EjecutarBusqueda);

        if (botonVolver != null)
            botonVolver.onClick.AddListener(VolverPanelBusqueda);

        MostrarPanelBusqueda();
    }

    void CargarTiemposDesdeCSV()
    {
        string path = Application.persistentDataPath + "/progreso_tiempos.csv";
        if (!File.Exists(path)) return;

        todosLosTiempos.Clear();
        todosLosNombres.Clear();

        string[] lineas;
        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (var sr = new StreamReader(fs))
        {
            var contenido = sr.ReadToEnd();
            lineas = contenido.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        if (lineas.Length < 2) return;

        string[] headers = lineas[0].Split(',');

        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            if (columnas.Length < 2) continue;

            RegistroTiempo reg = new RegistroTiempo
            {
                nombre = columnas[0].Trim().Replace("\uFEFF", ""),
                fecha = columnas[1].Trim()
            };

            for (int j = 2; j < columnas.Length && j < headers.Length; j++)
            {
                string valor = columnas[j].Trim();
                if (!string.IsNullOrEmpty(valor))
                {
                    if (valor.Contains(":")) // formato hh:mm:ss
                        reg.tiempos[headers[j]] = valor;
                }
            }

            todosLosTiempos.Add(reg);
            todosLosNombres.Add(reg.nombre);
        }

        todosLosNombres = todosLosNombres.Distinct().OrderBy(n => n).ToList();
    }

    void InicializarDropdownNombres()
    {
        dropdownNombre.ClearOptions();
        if (todosLosNombres.Count == 0)
            dropdownNombre.AddOptions(new List<string> { "Sin registro" });
        else
            dropdownNombre.AddOptions(todosLosNombres);
    }

    void InicializarDropdownFases()
    {
        dropdownFase.ClearOptions();
        dropdownFase.AddOptions(new List<string> { "Fase 1", "Fase 2", "Fase 3" });
    }

    void EjecutarBusqueda()
    {
        string nombreSeleccionado = dropdownNombre.options[dropdownNombre.value].text;
        string fechaInicioStr = inputFechaInicio.text.Trim();
        string fechaFinStr = inputFechaFin.text.Trim();
        int faseSeleccionada = dropdownFase.value + 1;

        if (nombreSeleccionado == "Sin registro" || string.IsNullOrEmpty(fechaInicioStr) || string.IsNullOrEmpty(fechaFinStr))
        {
            MostrarPrefabSinRegistro();
            return;
        }

        // Intentar parsear las fechas de entrada
        if (!DateTime.TryParse(fechaInicioStr, out DateTime fechaInicio) ||
            !DateTime.TryParse(fechaFinStr, out DateTime fechaFin))
        {
            Debug.LogWarning("Formato de fecha inválido. Usa el formato YYYY-MM-DD");
            MostrarPrefabSinRegistro();
            return;
        }

        // Filtrado ignorando horas
        var filtrados = todosLosTiempos
            .Where(r => r.nombre == nombreSeleccionado)
            .Where(r =>
            {
                if (DateTime.TryParse(r.fecha, out DateTime f))
                {
                    DateTime soloFecha = f.Date; // Ignora la hora
                    return soloFecha >= fechaInicio.Date && soloFecha <= fechaFin.Date;
                }
                return false;
            })
            .OrderBy(r => r.fecha)
            .ToList();

        if (filtrados.Count == 0)
        {
            MostrarPrefabSinRegistro();
            return;
        }

        ConstruirTabla(filtrados, faseSeleccionada);
        MostrarPanelResultados();
    }

    void ConstruirTabla(List<RegistroTiempo> registros, int fase)
    {
        foreach (Transform hijo in tablaContenido) Destroy(hijo.gameObject);

        foreach (var reg in registros)
        {
            GameObject fila = Instantiate(filaPrefab, tablaContenido);
            var txt = fila.GetComponentInChildren<TextMeshProUGUI>();

            string linea;

            if (DateTime.TryParse(reg.fecha, out DateTime fechaParsed))
                linea = $"Fecha: {fechaParsed:yyyy-MM-dd HH:mm:ss} | ";
            else
                linea = $"Fecha: {reg.fecha} | ";

            // Mostrar todos los niveles de esa fase
            for (int n = 1; n <= 10; n++)
            {
                string clave = $"nivel{n}F{fase}";
                string valor = reg.tiempos.ContainsKey(clave) ? reg.tiempos[clave] : "--:--:--";
                linea += $"N{n}: {valor} | ";
            }

            if (linea.EndsWith(" | "))
                linea = linea.Substring(0, linea.Length - 3);

            txt.text = linea;
        }
    }

    void MostrarPrefabSinRegistro()
    {
        if (prefabSinRegistro != null)
        {
            GameObject instanciado = Instantiate(prefabSinRegistro, spawnPrefabSinRegistro.position, Quaternion.identity, panelBusqueda.transform);
            StartCoroutine(DesaparecerPrefab(instanciado, 3f));
        }
    }

    private System.Collections.IEnumerator DesaparecerPrefab(GameObject obj, float segundos)
    {
        yield return new WaitForSeconds(segundos);
        if (obj != null)
            Destroy(obj);
    }

    void MostrarPanelBusqueda()
    {
        panelBusqueda.SetActive(true);
        panelResultados.SetActive(false);
    }

    void MostrarPanelResultados()
    {
        panelBusqueda.SetActive(false);
        panelResultados.SetActive(true);
    }

    void VolverPanelBusqueda()
    {
        panelBusqueda.SetActive(true);
        panelResultados.SetActive(false);
    }
}
