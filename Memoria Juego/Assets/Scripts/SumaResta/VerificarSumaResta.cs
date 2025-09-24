using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.SceneManagement;

public class VerificarSumaResta : MonoBehaviour
{
    [Header("Referencias")]
    public Arrastrable sumaRestaScript;     // Script de arrastrables
    public TextMeshProUGUI aciertosText;    
    [Header("Contador de imágenes")]
    public TextMeshProUGUI contadorImagenesText;
    public Transform spawnArea;             
    public GameObject imagenPrefab;         
    public Button verificarButton;          
    public GameObject ganastePrefab;        
    public GameObject perfectoPrefab;       
    public GameObject intentaDeNuevoPrefab; 
    public GameObject colocaSumaORestaPrefab; // 👈 NUEVO prefab para error de no usar suma/resta
    public static event System.Action OnNivelCompletado;
    [Header("Configuración de Escena")]
    public int indiceEscenaAlGanar = -1;

    private int objetivoActual = 0;
    private int ultimoObjetivo = 0;
    private int aciertos = 0;

    private List<GameObject> spawnedImages = new List<GameObject>();

    void Start()
    {
        GenerarNuevoObjetivo();
        ActualizarAciertosText();
        verificarButton.onClick.AddListener(VerificarRespuesta);
    }

    void GenerarNuevoObjetivo()
    {
        foreach (var img in spawnedImages)
            Destroy(img);
        spawnedImages.Clear();

        int nuevoObjetivo;
        do
        {
            nuevoObjetivo = Random.Range(2, 16); // un poco más grande para usar suma y resta
        } while (nuevoObjetivo == ultimoObjetivo);

        objetivoActual = nuevoObjetivo;
        ultimoObjetivo = objetivoActual;

        for (int i = 0; i < objetivoActual; i++)
        {
            GameObject nuevaImg = Instantiate(imagenPrefab, spawnArea);
            spawnedImages.Add(nuevaImg);
        }

        if (contadorImagenesText != null)
            contadorImagenesText.text = objetivoActual.ToString();
    }

    void VerificarRespuesta()
    {
        if (sumaRestaScript == null) return;

        int totalJugador = sumaRestaScript.GetCurrentTotal();

        // 👇 Validar que haya usado al menos un slot de suma y uno de resta
        bool usoSuma = sumaRestaScript.UsaSlotDeTipo(ModoOperacion.Suma);
        bool usoResta = sumaRestaScript.UsaSlotDeTipo(ModoOperacion.Resta);

        if (!usoSuma || !usoResta)
        {
            Debug.Log("⚠️ Debes usar al menos un número en suma y uno en resta");
            StartCoroutine(MostrarMensajeColocaSumaOResta());
            return;
        }

        if (totalJugador == objetivoActual)
        {
            Debug.Log("✅ Acierto!");
            aciertos++;
            ActualizarAciertosText();
            sumaRestaScript.LimpiarSlots();

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
                StartCoroutine(MostrarMensajePerfecto());
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

    private IEnumerator MostrarMensajeColocaSumaOResta()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        GameObject mensaje = Instantiate(colocaSumaORestaPrefab, canvas.transform, false);
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
            nivelCompletado = nivelSeleccionadoRaw + 1;
        else
            nivelCompletado = nivelSeleccionadoRaw;

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || nivelCompletado < 1)
            yield break;

        string path = Application.persistentDataPath + "/progreso.csv";

        if (!File.Exists(path))
            File.WriteAllText(path, "Nombre,Fecha,Fases Completadas,Niveles Completados\n");

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
                    NivelManager.RegistrarNivelCompletado(nuevoProgreso);
                }
                else
                {
                    encontrado = true;
                }
                break;
            }
        }

        if (!encontrado)
        {
            string nuevaLinea = $"{nombre},{fecha},0,{nivelCompletado}";
            List<string> lineasList = new List<string>(lineas) { nuevaLinea };
            lineas = lineasList.ToArray();
            NivelManager.RegistrarNivelCompletado(nivelCompletado);
        }

        try
        {
            File.WriteAllLines(path, lineas);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV: {e.Message}");
        }

        yield return null;
    }
}
