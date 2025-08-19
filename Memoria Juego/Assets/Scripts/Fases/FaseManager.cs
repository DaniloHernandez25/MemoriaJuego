using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class FaseManager : MonoBehaviour
{
    public Button[] botonesDeFase; // Asignar los botones en el Inspector

    private string nombreJugador;
    private string fechaPartida;

    void Start()
    {
        nombreJugador = PlayerPrefs.GetString("nombreJugador", "");
        fechaPartida = PlayerPrefs.GetString("fechaSeleccionada", "");

        Debug.Log("💡 Nombre en FaseManager: " + nombreJugador);
        Debug.Log("💡 Fecha en FaseManager: " + fechaPartida);

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
            Debug.LogWarning("⚠️ Faltan datos en PlayerPrefs");
            return;
        }

        LeerFasesDesdeCSV(nombreJugador); // Leer las fases directamente desde el archivo CSV
    }

    // Función para leer las fases completadas desde el archivo CSV
    private void LeerFasesDesdeCSV(string nombre)
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            Debug.LogWarning("⚠️ No se encontró el archivo CSV de progreso.");
            return;
        }

        string[] lineas = File.ReadAllLines(path);
        bool jugadorEncontrado = false;

        foreach (string linea in lineas)
        {
            // Ignorar la cabecera
            if (linea.StartsWith("Nombre,"))
                continue;

            string[] columnas = linea.Split(',');

            if (columnas.Length >= 4 && columnas[0] == nombre)
            {
                // Si encontramos el jugador, obtenemos las fases completadas
                int fasesCompletadas = 0;

                // Intentar convertir la fase completada a un número
                if (int.TryParse(columnas[2].Trim(), out fasesCompletadas)) 
                {
                    Debug.Log("✅ Fases completadas para " + nombre + ": " + fasesCompletadas);
                }
                else
                {
                    Debug.LogWarning("⚠️ No se pudo convertir fases_completadas a int para " + nombre);
                }

                DesbloquearBotones(fasesCompletadas);
                jugadorEncontrado = true;
                break;
            }
        }

        if (!jugadorEncontrado)
        {
            Debug.LogWarning("⚠️ No se encontraron fases completadas para el jugador " + nombre);
            DesbloquearBotones(0); // Si no encontramos el jugador, no desbloqueamos ninguna fase
        }
    }

    private void DesbloquearBotones(int fasesCompletadas)
    {
        for (int i = 0; i < botonesDeFase.Length; i++)
        {
            if (fasesCompletadas <= 0)
            {
                // Solo el primer botón activo si no hay progreso
                botonesDeFase[i].interactable = (i == 0);
            }
            else
            {
                // Desbloquea desde el primero hasta el número indicado
                botonesDeFase[i].interactable = (i < fasesCompletadas);
            }

            Debug.Log($"🔓 Botón {i}: {(botonesDeFase[i].interactable ? "Desbloqueado" : "Bloqueado")}");
        }
    }
}
