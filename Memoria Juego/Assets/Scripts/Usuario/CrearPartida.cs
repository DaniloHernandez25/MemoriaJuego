using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class CrearPartida : MonoBehaviour
{
    [SerializeField] private int sceneToLoad = -1;

    public void AlCrearPartida()
    {
        string nombreJugador = PlayerPrefs.GetString("nombreJugador", "");

        if (string.IsNullOrEmpty(nombreJugador))
        {
            return;
        }

        // 🔹 Generar una nueva fecha para esta partida
        string fechaPartida = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 🔸 Guardar esa fecha en PlayerPrefs
        PlayerPrefs.SetString("fechaSeleccionada", fechaPartida);
        PlayerPrefs.Save();

        // Crear nueva partida con 0 niveles en el archivo CSV
        CrearPartidaEnCSV(nombreJugador, fechaPartida);

        // 👇 Cambiar de escena una vez confirmada la creación
        SceneManager.LoadScene(sceneToLoad);
    }

    // Función para crear la partida en el archivo CSV
    private void CrearPartidaEnCSV(string nombre, string fecha)
    {
        string path = Application.persistentDataPath + "/progreso.csv";

        // Verificar si el archivo CSV ya existe
        bool fileExists = File.Exists(path);

        // Si el archivo no existe, lo creamos y escribimos la cabecera
        if (!fileExists)
        {
            using (StreamWriter writer = new StreamWriter(path, false)) // "false" para sobrescribir el archivo
            {
                writer.WriteLine("Nombre,Fecha,Fases Completadas,Niveles Completados"); // Cabecera
            }
        }

        // Ahora agregamos la nueva partida (si el archivo existe o lo acabamos de crear)
        using (StreamWriter writer = new StreamWriter(path, true)) // "true" para agregar al final del archivo
        {
            // Fases y niveles siempre serán 0 en la creación de una nueva partida
            writer.WriteLine($"{nombre},{fecha},1,1"); // Escribir la nueva partida con valores predeterminados
        }
    }
}
