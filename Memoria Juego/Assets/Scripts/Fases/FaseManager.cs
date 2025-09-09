using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class FaseManager : MonoBehaviour
{
    public Button[] botonesDeFase; // Asignar los botones en el Inspector
    public Sprite spriteBloqueado; // Imagen para botón bloqueado
    public Sprite spriteDesbloqueado; // Imagen para botón desbloqueado

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");

        Debug.Log($"FaseManager - Nombre: '{nombreJugador}', Fecha: '{fechaPartida}'");

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("Datos incompletos en PlayerPrefs");
            DesbloquearBotones(1); // Al menos primera fase activa
            return;
        }

        // AHORA PASAMOS TANTO NOMBRE COMO FECHA
        LeerFasesDesdeCSV(nombreJugador, fechaPartida);
    }

    // Función para leer las fases completadas desde el archivo CSV usando NOMBRE Y FECHA
    private void LeerFasesDesdeCSV(string nombre, string fecha)
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            Debug.LogWarning("Archivo progreso.csv no encontrado");
            DesbloquearBotones(1); // Primera fase activa por defecto
            return;
        }

        string[] lineas = File.ReadAllLines(path);
        bool jugadorEncontrado = false;

        for (int i = 1; i < lineas.Length; i++) // Empezar en 1 para saltar cabecera
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            
            // Limpiar espacios
            for (int j = 0; j < columnas.Length; j++)
            {
                columnas[j] = columnas[j].Trim();
            }

            if (columnas.Length >= 4)
            {
                Debug.Log($"Comparando línea {i}: '{columnas[0]}' vs '{nombre}' | '{columnas[1]}' vs '{fecha}'");

                // BUSCAR POR NOMBRE Y FECHA (ambos deben coincidir)
                if (columnas[0] == nombre && columnas[1] == fecha)
                {
                    int fasesCompletadas = 1; // Por defecto al menos 1 fase activa

                    if (int.TryParse(columnas[2], out int fases))
                    {
                        fasesCompletadas = Mathf.Max(1, fases); // Mínimo 1 fase
                    }

                    Debug.Log($"Jugador encontrado: {fasesCompletadas} fases completadas");
                    DesbloquearBotones(fasesCompletadas);
                    jugadorEncontrado = true;
                    break;
                }
            }
        }

        if (!jugadorEncontrado)
        {
            Debug.Log($"No se encontró registro para {nombre} en fecha {fecha}");
            DesbloquearBotones(1); // Primera fase activa si no hay registro
        }
    }

    private void DesbloquearBotones(int fasesCompletadas)
    {
        Debug.Log($"Desbloqueando {fasesCompletadas} fases");

        for (int i = 0; i < botonesDeFase.Length; i++)
        {
            // Las fases desbloqueadas son: desde 0 hasta fasesCompletadas-1
            // Más la siguiente fase (fasesCompletadas) para poder jugar
            bool desbloqueado = (i < fasesCompletadas);

            botonesDeFase[i].interactable = desbloqueado;

            // Cambiar la imagen según estado
            Image img = botonesDeFase[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = desbloqueado ? spriteDesbloqueado : spriteBloqueado;
            }

            Debug.Log($"Fase {i + 1}: {(desbloqueado ? "DESBLOQUEADA" : "BLOQUEADA")}");
        }
    }

    // Método para actualizar fases cuando se complete una
    public void CompletarFase(int numeroFase)
    {
        ActualizarFasesEnCSV(numeroFase + 1); // +1 porque las fases empiezan en 1
        LeerFasesDesdeCSV(nombreJugador, fechaPartida); // Recargar estado
    }

    private void ActualizarFasesEnCSV(int nuevasFasesCompletadas)
    {
        string path = Application.persistentDataPath + "/progreso.csv";
        
        if (!File.Exists(path)) return;

        string[] lineas = File.ReadAllLines(path);

        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            for (int j = 0; j < columnas.Length; j++)
            {
                columnas[j] = columnas[j].Trim();
            }

            if (columnas.Length >= 4 && columnas[0] == nombreJugador && columnas[1] == fechaPartida)
            {
                int fasesActuales = 0;
                int.TryParse(columnas[2], out fasesActuales);

                // Solo actualizar si es progreso (no retroceso)
                if (nuevasFasesCompletadas > fasesActuales)
                {
                    columnas[2] = nuevasFasesCompletadas.ToString();
                    lineas[i] = string.Join(",", columnas);
                    File.WriteAllLines(path, lineas);
                    Debug.Log($"Fases actualizadas a: {nuevasFasesCompletadas}");
                }
                break;
            }
        }
    }
}