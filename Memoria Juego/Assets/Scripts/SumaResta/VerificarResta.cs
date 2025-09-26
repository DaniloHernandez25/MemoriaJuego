using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class VerificarResta : MonoBehaviour
{
    [Header("Referencias")]
    public Arrastrable sumaScript;                   
    public TextMeshProUGUI restaText;         
    public TextMeshProUGUI preguntaText;      
    public TextMeshProUGUI aciertosText;      
    public Transform spawnArea;               
    public GameObject imagenPrefab;           
    public Button verificarButton;            
    public GameObject ganastePrefab;
    public GameObject perfectoPrefab;          
    public GameObject intentaDeNuevoPrefab;
    public static event System.Action OnNivelCompletado;
    [Header("Configuración de Escena")]
    public int indiceEscenaAlGanar = -1;  

    private int numeroSpawn = 0;              
    private int numeroObjetivo = 0;           
    private int cantidadARestar = 0;          
    private int aciertos = 0;

    private List<GameObject> spawnedImages = new List<GameObject>();

    void Start()
    {
        verificarButton.onClick.AddListener(VerificarRespuesta);
        GenerarNuevoEjercicio();
        ActualizarAciertosText();
    }

    void GenerarNuevoEjercicio()
    {
        // Limpiar imágenes anteriores
        foreach (var img in spawnedImages)
            Destroy(img);
        spawnedImages.Clear();

        // Generar números
        numeroSpawn = Random.Range(2, 16);          // Número mayor
        numeroObjetivo = Random.Range(1, numeroSpawn);  // Número menor
        cantidadARestar = numeroSpawn - numeroObjetivo;

        // Mostrar total inicial en Arrastrable
        if (sumaScript != null)
            sumaScript.SetInitialTotal(numeroSpawn);

        // Generar imágenes
        for (int i = 0; i < numeroSpawn; i++)
        {
            GameObject nuevaImg = Instantiate(imagenPrefab, spawnArea);
            spawnedImages.Add(nuevaImg);
        }

        // Actualizar textos
        if (preguntaText != null)
            preguntaText.text = $"¿Cuánto debo restar de {numeroSpawn} para tener {numeroObjetivo}?";

        if (restaText != null)
            restaText.text = "Total: " + sumaScript.GetCurrentTotal();

    }

    void VerificarRespuesta()
    {
        if (sumaScript == null) return;

        int respuestaJugador = sumaScript.GetCurrentTotal();

        if (respuestaJugador == numeroObjetivo)
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
                // Mostrar Perfecto solo si no ganó
                StartCoroutine(MostrarMensajePerfecto());
                GenerarNuevoEjercicio();
            }
        }
        else
        {
            Debug.Log($"❌ Fallo, quitó {respuestaJugador} pero debía quitar {cantidadARestar}");
            StartCoroutine(MostrarIntentoFallido());
        }
    }

    private IEnumerator MostrarMensajePerfecto()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject mensajePerfecto = Instantiate(perfectoPrefab, canvas.transform, false);

        yield return new WaitForSeconds(1f);

        Destroy(mensajePerfecto);
    }


    private IEnumerator MostrarIntentoFallido()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject aviso = Instantiate(intentaDeNuevoPrefab, canvas.transform, false);

        yield return new WaitForSeconds(2f);

        Destroy(aviso);
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
            Debug.LogError("Datos incompletos para guardar progreso en VerificarResta");
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
            Debug.Log("CSV guardado exitosamente desde VerificarResta");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV en VerificarResta: {e.Message}");
        }

        yield return null;
    }
}
