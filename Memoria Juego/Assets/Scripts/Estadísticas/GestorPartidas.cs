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
    public TMP_Dropdown dropdownFechaInicio;
    public TMP_Dropdown dropdownFechaFin;
    public TMP_Dropdown dropdownFase;
    public Button botonBuscar;

    [Header("UI Tabla Resultados")]
    public Transform tablaContenido;
    public GameObject filaPrefab;

    [Header("UI Resultados")]
    public GameObject prefabSinRegistro;
    public Transform spawnPrefabSinRegistro; // Lugar donde se instanciará sobresaliendo

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

        dropdownNombre.onValueChanged.AddListener(delegate { ActualizarFechasPorNombre(); });

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
                fecha = columnas[1]
            };

            for (int j = 2; j < columnas.Length && j < headers.Length; j++)
            {
                string valor = columnas[j].Trim();
                if (!string.IsNullOrEmpty(valor))
                {
                    // Si es formato hh:mm:ss lo guardamos directo
                    if (valor.Contains(":"))
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

        ActualizarFechasPorNombre();
    }

    void ActualizarFechasPorNombre()
    {
        dropdownFechaInicio.ClearOptions();
        dropdownFechaFin.ClearOptions();

        if (dropdownNombre.options.Count == 0 || dropdownNombre.options[dropdownNombre.value].text == "Sin registro")
        {
            dropdownFechaInicio.AddOptions(new List<string> { "Sin registro" });
            dropdownFechaFin.AddOptions(new List<string> { "Sin registro" });
            return;
        }

        string nombreSeleccionado = dropdownNombre.options[dropdownNombre.value].text;

        List<string> fechasFiltradas = todosLosTiempos
            .Where(r => r.nombre == nombreSeleccionado)
            .Select(r => r.fecha)
            .Distinct()
            .OrderBy(f => DateTime.Parse(f))
            .ToList();

        if (fechasFiltradas.Count == 0)
        {
            dropdownFechaInicio.AddOptions(new List<string> { "Sin registro" });
            dropdownFechaFin.AddOptions(new List<string> { "Sin registro" });
            return;
        }

        dropdownFechaInicio.AddOptions(fechasFiltradas);
        dropdownFechaFin.AddOptions(fechasFiltradas);

        dropdownFechaInicio.value = 0;
        dropdownFechaFin.value = fechasFiltradas.Count - 1;
    }

    void InicializarDropdownFases()
    {
        dropdownFase.ClearOptions();
        dropdownFase.AddOptions(new List<string> { "Fase 1", "Fase 2", "Fase 3" });
    }

    void EjecutarBusqueda()
    {
        string nombreSeleccionado = dropdownNombre.options[dropdownNombre.value].text;
        string fechaInicioStr = dropdownFechaInicio.options[dropdownFechaInicio.value].text;
        string fechaFinStr = dropdownFechaFin.options[dropdownFechaFin.value].text;
        int faseSeleccionada = dropdownFase.value + 1;

        // Si no hay registros
        if (nombreSeleccionado == "Sin registro" || fechaInicioStr == "Sin registro" || fechaFinStr == "Sin registro")
        {
            MostrarPrefabSinRegistro();
            return;
        }

        DateTime.TryParse(fechaInicioStr, out DateTime fechaInicio);
        DateTime.TryParse(fechaFinStr, out DateTime fechaFin);

        var filtrados = todosLosTiempos.Where(r =>
        {
            if (r.nombre != nombreSeleccionado) return false;

            if (DateTime.TryParse(r.fecha, out DateTime f))
            {
                if (fechaInicio != DateTime.MinValue && f < fechaInicio) return false;
                if (fechaFin != DateTime.MinValue && f > fechaFin) return false;
            }

            return true;
        }).OrderBy(r => r.fecha).ToList();

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
                linea = $"Fecha: {fechaParsed:yyyy-MM-dd} | ";
            else
                linea = $"Fecha: {reg.fecha} | ";

            for (int n = 1; n <= 10; n++)
            {
                string clave = $"nivel{n}F{fase}";
                string valor = reg.tiempos.ContainsKey(clave) ? reg.tiempos[clave] : "00:00:00";
                linea += $"N{n}: {valor} | ";
            }


            if (linea.EndsWith(" | "))
                linea = linea.Substring(0, linea.Length - 3);

            txt.text = linea;
        }
    }

   void MostrarPrefabSinRegistro()
    {
        // Instancia el prefab sobresaliendo sobre el canvas principal
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
