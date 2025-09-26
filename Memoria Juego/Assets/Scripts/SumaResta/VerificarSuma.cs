using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class VerificarSuma : MonoBehaviour
{
    [Header("Referencias")]
    public Arrastrable sumaScript;                   // Referencia al script anterior
    public TextMeshProUGUI objetivoText;      // Muestra el número objetivo
    public TextMeshProUGUI aciertosText;      // Texto para mostrar aciertos
    [Header("Contador de imágenes")]
    public TextMeshProUGUI contadorImagenesText;
    public Transform spawnArea;               // Área donde aparecen las imágenes
    public GameObject imagenPrefab;           // Prefab de la imagen que se clona
    public Button verificarButton;            // Botón de verificar
    public GameObject ganastePrefab;          // Prefab que aparece al ganar
    public GameObject perfectoPrefab;        // 👈 Prefab que muestra el mensaje de "Perfecto"
    
    public GameObject intentaDeNuevoPrefab;   // 👈 Prefab que muestra el mensaje de error
    public static event System.Action OnNivelCompletado;
    [Header("Configuración de Escena")]
    public int indiceEscenaAlGanar = -1;


    private int objetivoActual = 0;
    private int ultimoObjetivo = 0;
    private int aciertos = 0;

    private List<GameObject> spawnedImages = new List<GameObject>();

    void Start()
    {
        // Generar primer objetivo
        GenerarNuevoObjetivo();

        // Mostrar aciertos iniciales
        ActualizarAciertosText();

        // Listener del botón
        verificarButton.onClick.AddListener(VerificarRespuesta);
    }

    void GenerarNuevoObjetivo()
    {
        // Borrar imágenes anteriores
        foreach (var img in spawnedImages)
            Destroy(img);
        spawnedImages.Clear();

        // Generar número aleatorio del 1 al 15, distinto al último
        int nuevoObjetivo;
        do
        {
            nuevoObjetivo = Random.Range(1, 16); // 1 a 15
        } while (nuevoObjetivo == ultimoObjetivo);

        objetivoActual = nuevoObjetivo;
        ultimoObjetivo = objetivoActual;

        // Generar esa cantidad de imágenes
        for (int i = 0; i < objetivoActual; i++)
        {
            GameObject nuevaImg = Instantiate(imagenPrefab, spawnArea);
            spawnedImages.Add(nuevaImg);
        }

        // 👇 Mostrar solo el número en el texto
        if (contadorImagenesText != null)
            contadorImagenesText.text = objetivoActual.ToString();
    }
    void VerificarRespuesta()
    {
        if (sumaScript == null) return;

        int totalJugador = sumaScript.GetCurrentTotal();

        if (totalJugador == objetivoActual)
        {
            Debug.Log("✅ Acierto!");
            aciertos++;
            ActualizarAciertosText();
            sumaScript.LimpiarSlots();

            if (aciertos >= 5)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                Instantiate(ganastePrefab, canvas.transform, false);
                Debug.Log("🎉 ¡Ganaste!");

                OnNivelCompletado?.Invoke();  // 👈 Notifica al NivelTimer
                StartCoroutine(GuardarProgresoEnCSV());

                // 👇 Corutina inline usando enumerador anónimo
                StartCoroutine(EsperarYCargar());

                System.Collections.IEnumerator EsperarYCargar()
                {
                    yield return new WaitForSeconds(2f);
                    UnityEngine.SceneManagement.SceneManager.LoadScene(indiceEscenaAlGanar);
                }
            }

            else
            {
                // Mostrar mensaje "Perfecto" solo si NO ganó
                StartCoroutine(MostrarMensajePerfecto());

                // Generar nuevo objetivo
                GenerarNuevoObjetivo();
            }
        }
        else
        {
            Debug.Log("❌ Fallo, intenta de nuevo");
            StartCoroutine(MostrarMensajeTemporal());
        }
    }

    private IEnumerator MostrarMensajePerfecto()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject mensajePerfecto = Instantiate(perfectoPrefab, canvas.transform, false);

        yield return new WaitForSeconds(1f);

        Destroy(mensajePerfecto);
    }


    private IEnumerator MostrarMensajeTemporal()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject mensaje = Instantiate(intentaDeNuevoPrefab, canvas.transform, false);

        yield return new WaitForSeconds(2f);

        Destroy(mensaje);
    }

    private void ActualizarAciertosText()
    {
        if (aciertosText != null)
            aciertosText.text = "Aciertos: " + aciertos;
    }

    private IEnumerator GuardarProgresoEnCSV()
    {
        string nombre = PlayerPrefs.GetString("nombreJugador", "");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", "");
        int nivelSeleccionadoRaw = PlayerPrefs.GetInt("nivelSeleccionado", -1);

        int nivelCompletado;
        if (nivelSeleccionadoRaw >= 0 && nivelSeleccionadoRaw < 30)
        {
            nivelCompletado = nivelSeleccionadoRaw + 1;
            Debug.Log($"⚠️ CONVERSIÓN: Nivel raw {nivelSeleccionadoRaw} convertido a {nivelCompletado}");
        }
        else
        {
            nivelCompletado = nivelSeleccionadoRaw;
        }

        Debug.Log($"Guardando progreso: Nombre={nombre}, Fecha={fecha}, Nivel={nivelCompletado}");

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || nivelCompletado < 1)
        {
            Debug.LogError("Datos incompletos para guardar progreso en VerificarSuma");
            yield break;
        }

        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Nombre,Fecha,Fases Completadas,Niveles Completados\n");
            Debug.Log("Archivo CSV creado");
        }

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;

            string[] columnas = lineas[i].Split(',');
            for (int j = 0; j < columnas.Length; j++) columnas[j] = columnas[j].Trim();

            if (columnas.Length < 4) continue;

            if (columnas[0] == nombre && columnas[1] == fecha)
            {
                if (!int.TryParse(columnas[3], out int nivelesActuales)) nivelesActuales = 0;

                if (nivelCompletado == nivelesActuales)
                {
                    int nuevoProgreso = nivelesActuales + 1;
                    columnas[3] = nuevoProgreso.ToString();
                    lineas[i] = string.Join(",", columnas);
                    encontrado = true;

                    Debug.Log($"¡Nivel completado! Progreso: {nivelesActuales} -> {nuevoProgreso}");
                    NivelManager.RegistrarNivelCompletado(nuevoProgreso);
                }
                else
                {
                    encontrado = true;
                    Debug.Log($"Nivel {nivelCompletado} no coincide con el disponible {nivelesActuales}");
                }
                break;
            }
        }

        if (!encontrado)
        {
            Debug.Log("No se encontró registro existente, creando uno nuevo");
        }

        try
        {
            File.WriteAllLines(path, lineas);
            Debug.Log("CSV guardado exitosamente desde VerificarSuma");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV en VerificarSuma: {e.Message}");
        }

        yield return null;
    }
}
