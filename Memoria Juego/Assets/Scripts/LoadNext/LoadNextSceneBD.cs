using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
//C:\Users\Usuario\AppData\LocalLow\DefaultCompany\Memoria Juego\jugadores.csv

public class LoadNextSceneBD : MonoBehaviour
{
    [Header("Datos del jugador")]
    public TMP_InputField inputNombre;
    public SelectorDeImagen selector;

    [Header("Configuración de escena")]
    [SerializeField] private int sceneIndexToLoad = -1;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.interactable = true;
    }

    public void GuardarYContinuar()
    {
        string nombre = inputNombre.text;
        string urlImagen = selector.ObtenerURLActual();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(urlImagen))
        {
            return;
        }

        // Guardar el nombre para usarlo en la siguiente escena
        PlayerPrefs.SetString("nombreJugador", nombre);
        PlayerPrefs.Save();

        GuardarEnCSV(nombre, urlImagen); // Guardar en CSV
        CargarEscena(); // Cambiar de escena
    }

    private void GuardarEnCSV(string nombre, string urlImagen)
    {
        string path = Application.persistentDataPath + "/jugadores.csv";
        bool fileExists = File.Exists(path);
        bool nombreExistente = false;

        // Crear una lista para guardar las líneas modificadas
        List<string> lineasModificadas = new List<string>();

        // Verificar si el archivo ya existe
        if (fileExists)
        {
            // Leer todas las líneas del archivo CSV
            string[] lineas = File.ReadAllLines(path);

            foreach (var linea in lineas)
            {
                string[] columnas = linea.Split(',');

                if (columnas.Length >= 2 && columnas[0] == nombre)
                {
                    // Si el nombre ya existe, actualizar el perfil
                    lineasModificadas.Add($"{nombre},{urlImagen}");
                    nombreExistente = true;
                }
                else
                {
                    // Si el nombre no existe, mantener la línea tal como está
                    lineasModificadas.Add(linea);
                }
            }

            // Si el nombre no existe, agregar una nueva entrada
            if (!nombreExistente)
            {
                lineasModificadas.Add($"{nombre},{urlImagen}");
            }

            // Sobrescribir el archivo con las líneas modificadas
            File.WriteAllLines(path, lineasModificadas.ToArray());
        }
        else
        {
            // Si el archivo no existe, crear uno nuevo
            using (StreamWriter writer = new StreamWriter(path, false)) // "false" sobrescribe el archivo
            {
                writer.WriteLine("Nombre,Perfil"); // Escribir las cabeceras
                writer.WriteLine($"{nombre},{urlImagen}"); // Escribir la nueva entrada
            }
        }

    }

    private void CargarEscena()
    {
        if (sceneIndexToLoad >= 0)
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }
}