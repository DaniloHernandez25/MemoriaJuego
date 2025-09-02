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

        if (string.IsNullOrEmpty(nombreJugador) || string.IsNullOrEmpty(fechaPartida))
        {
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
            return;
        }

        string[] lineas = File.ReadAllLines(path);
        bool jugadorEncontrado = false;

        foreach (string linea in lineas)
        {
            // Ignorar la cabecera
            if (linea.StartsWith("Nombre,")) continue;

            string[] columnas = linea.Split(',');

            if (columnas.Length >= 4 && columnas[0] == nombre)
            {
                // Si encontramos el jugador, obtenemos las fases completadas
                int fasesCompletadas = 0;

                if (int.TryParse(columnas[2].Trim(), out fasesCompletadas))
                {
                }

                DesbloquearBotones(fasesCompletadas);
                jugadorEncontrado = true;
                break;
            }
        }

        if (!jugadorEncontrado)
        {
            DesbloquearBotones(0); // Si no encontramos el jugador, no desbloqueamos ninguna fase
        }
    }

    private void DesbloquearBotones(int fasesCompletadas)
    {
        for (int i = 0; i < botonesDeFase.Length; i++)
        {
            bool desbloqueado;

            if (fasesCompletadas <= 0)
            {
                // Solo el primer botón activo si no hay progreso
                desbloqueado = (i == 0);
            }
            else
            {
                // Desbloquea desde el primero hasta el número indicado
                desbloqueado = (i < fasesCompletadas);
            }

            botonesDeFase[i].interactable = desbloqueado;

            // Cambiar la imagen según estado
            Image img = botonesDeFase[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = desbloqueado ? spriteDesbloqueado : spriteBloqueado;
            }
        }
    }
}
