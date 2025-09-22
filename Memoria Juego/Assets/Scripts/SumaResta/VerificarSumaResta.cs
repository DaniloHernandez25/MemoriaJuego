using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class VerificarSumaResta : MonoBehaviour
{
    [Header("Referencias")]
    public Arrastrable sumaScript;                   
    public TextMeshProUGUI objetivoText;      // Muestra el número objetivo (para suma)
    public TextMeshProUGUI restaText;         // Para mostrar la operación de resta
    public TextMeshProUGUI preguntaText;      // Pregunta para resta (vacío para suma)
    public TextMeshProUGUI aciertosText;      
    public Transform spawnArea;               
    public GameObject imagenPrefab;           
    public Button verificarButton;            
    public GameObject ganastePrefab;          
    public GameObject intentaDeNuevoPrefab;   

    private int aciertos = 0;
    private bool tuveSuma = false;
    private bool tuveResta = false;

    // Variables para suma
    private int objetivoActualSuma = 0;
    private int ultimoObjetivoSuma = 0;

    // Variables para resta
    private int numeroSpawn = 0;              
    private int numeroObjetivo = 0;           
    private int cantidadARestar = 0;

    // Variables generales
    private bool esSuma = true; // true = suma, false = resta
    private List<GameObject> spawnedImages = new List<GameObject>();

    void Start()
    {
        GenerarNuevoEjercicio();
        ActualizarAciertosText();
        verificarButton.onClick.AddListener(VerificarRespuesta);
    }

    void GenerarNuevoEjercicio()
    {
        // Borrar imágenes anteriores
        foreach (var img in spawnedImages)
            Destroy(img);
        spawnedImages.Clear();

        // Determinar si será suma o resta
        if (aciertos == 0)
        {
            // Primer intento: aleatorio entre suma y resta
            esSuma = Random.Range(0, 2) == 0;
        }
        else if (aciertos == 1)
        {
            // Segundo intento: si no hemos tenido suma o resta, forzar el que falta
            if (!tuveSuma && !tuveResta)
            {
                // Esto no debería pasar, pero por seguridad
                esSuma = Random.Range(0, 2) == 0;
            }
            else if (!tuveSuma)
            {
                esSuma = true;
            }
            else if (!tuveResta)
            {
                esSuma = false;
            }
            else
            {
                // Ya tuvimos ambos, aleatorio
                esSuma = Random.Range(0, 2) == 0;
            }
        }
        else
        {
            // Tercer intento: aleatorio
            esSuma = Random.Range(0, 2) == 0;
        }

        if (esSuma)
        {
            GenerarEjercicioSuma();
            tuveSuma = true;
        }
        else
        {
            GenerarEjercicioResta();
            tuveResta = true;
        }
    }

    void GenerarEjercicioSuma()
    {
        // Limpiar textos de resta
        if (restaText != null)
            restaText.text = "";
        if (preguntaText != null)
            preguntaText.text = "";

        // Generar número aleatorio del 1 al 15, distinto al último
        int nuevoObjetivo;
        do
        {
            nuevoObjetivo = Random.Range(1, 16); // 1 a 15
        } while (nuevoObjetivo == ultimoObjetivoSuma);

        objetivoActualSuma = nuevoObjetivo;
        ultimoObjetivoSuma = objetivoActualSuma;

        // Mostrar objetivo
        if (objetivoText != null)
            objetivoText.text = "Objetivo: " + objetivoActualSuma;

        // Generar esa cantidad de imágenes
        for (int i = 0; i < objetivoActualSuma; i++)
        {
            GameObject nuevaImg = Instantiate(imagenPrefab, spawnArea);
            spawnedImages.Add(nuevaImg);
        }
    }

    void GenerarEjercicioResta()
    {
        // Limpiar texto de suma
        if (objetivoText != null)
            objetivoText.text = "";

        numeroSpawn = Random.Range(2, 16); 
        numeroObjetivo = Random.Range(1, numeroSpawn);
        cantidadARestar = numeroSpawn - numeroObjetivo;

        for (int i = 0; i < numeroSpawn; i++)
        {
            GameObject nuevaImg = Instantiate(imagenPrefab, spawnArea);
            spawnedImages.Add(nuevaImg);
        }

        if (preguntaText != null)
            preguntaText.text = $"¿Cuánto debo restar de {numeroSpawn} para tener {numeroObjetivo}?";

    }

    void VerificarRespuesta()
    {
        if (sumaScript == null) return;

        int respuestaJugador = sumaScript.GetCurrentTotal();
        bool esCorrecta = false;

        if (esSuma)
        {
            esCorrecta = (respuestaJugador == objetivoActualSuma);
        }
        else
        {
            esCorrecta = (respuestaJugador == cantidadARestar);
        }

        if (esCorrecta)
        {
            Debug.Log("✅ Acierto!");
            aciertos++;
            ActualizarAciertosText();

            if (aciertos >= 3)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                Instantiate(ganastePrefab, canvas.transform, false);
                Debug.Log("🎉 ¡Ganaste!");
                StartCoroutine(GuardarProgresoEnCSV());
            }
            else
            {
                GenerarNuevoEjercicio();
            }
        }
        else
        {
            Debug.Log("❌ Fallo, intenta de nuevo");
            StartCoroutine(MostrarMensajeTemporal());
        }
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
            Debug.LogError("Datos incompletos para guardar progreso en VerificarSumaResta");
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
            string nuevaLinea = $"{nombre},{fecha},0,{nivelCompletado}";
            List<string> lineasList = new List<string>(lineas) { nuevaLinea };
            lineas = lineasList.ToArray();

            Debug.Log($"Nuevo registro creado: {nuevaLinea}");
            NivelManager.RegistrarNivelCompletado(nivelCompletado);
        }

        try
        {
            File.WriteAllLines(path, lineas);
            Debug.Log("CSV guardado exitosamente desde VerificarSumaResta");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV en VerificarSumaResta: {e.Message}");
        }

        yield return null;
    }
}