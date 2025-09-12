// CardsControllerCSV.cs - CORREGIDO CON NUEVA LÓGICA
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class CardsController : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;
    [SerializeField] GameObject nivelCompletadoPrefab;

    [System.Serializable]
    private class NivelData
    {
        public int niveles_completados;
    }

    private List<Sprite> spritePairs;
    private Card firstSelected, secondSelected;
    private int matchCounts;

    private void Start()
    {
        PrepareSprites();
        CreateCards();
    }

    void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        foreach (var s in sprites)
        {
            spritePairs.Add(s);
            spritePairs.Add(s);
        }
        ShuffleSprites(spritePairs);
    }

    void CreateCards()
    {
        foreach (var s in spritePairs)
        {
            var card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(s);
            card.controller = this;
        }
    }

    public void SetSelected(Card card)
    {
        if (card.isSelected) return;
        card.Show();
        if (firstSelected == null) { firstSelected = card; return; }
        if (secondSelected == null)
        {
            secondSelected = card;
            StartCoroutine(CheckMatching(firstSelected, secondSelected));
            firstSelected = secondSelected = null;
        }
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);
        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;
            if (matchCounts >= spritePairs.Count / 2)
            {
                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.2f, ease: PrimeTween.Ease.InOutCubic))
                    .ChainDelay(0.3f)
                    .ChainCallback(() =>
                    {
                        // Mostrar popup
                        var canvas = GameObject.Find("Canvas");
                        if (canvas != null && nivelCompletadoPrefab != null)
                            Instantiate(nivelCompletadoPrefab, canvas.transform);

                        StartCoroutine(GuardarProgresoEnCSV());
                    });
            }
        }
        else
        {
            a.Hide();
            b.Hide();
        }
    }

    void ShuffleSprites(List<Sprite> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private IEnumerator GuardarProgresoEnCSV()
    {
        string nombre = PlayerPrefs.GetString("nombreJugador", "");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", "");
        int nivelSeleccionadoRaw = PlayerPrefs.GetInt("nivelSeleccionado", -1);

        // TEMPORAL: Corregir si el nivel viene en base 0
        int nivelCompletado;
        if (nivelSeleccionadoRaw >= 0 && nivelSeleccionadoRaw < 30) // Asumiendo máximo 10 niveles base 0
        {
            nivelCompletado = nivelSeleccionadoRaw + 1; // Convertir de base 0 a base 1
            Debug.Log($"⚠️ CONVERSIÓN: Nivel raw {nivelSeleccionadoRaw} convertido a {nivelCompletado}");
        }
        else
        {
            nivelCompletado = nivelSeleccionadoRaw; // Ya está en base 1
        }

        Debug.Log($"Guardando progreso: Nombre={nombre}, Fecha={fecha}");
        Debug.Log($"Nivel seleccionado guardado: {nivelSeleccionadoRaw}, Nivel real completado: {nivelCompletado}");

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || nivelCompletado < 1)
        {
            Debug.LogError("Datos incompletos para guardar progreso");
            yield break;
        }

        string path = Application.persistentDataPath + "/progreso.csv";

        // Crear archivo si no existe
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
            
            for (int j = 0; j < columnas.Length; j++)
            {
                columnas[j] = columnas[j].Trim();
            }

            if (columnas.Length < 4) 
            {
                Debug.LogWarning($"Línea {i} tiene formato incorrecto: {lineas[i]}");
                continue;
            }

            if (columnas[0] == nombre && columnas[1] == fecha)
            {
                if (!int.TryParse(columnas[3], out int nivelesActuales))
                {
                    nivelesActuales = 0;
                    Debug.LogWarning($"No se pudo parsear niveles completados, iniciando con 0");
                }

                Debug.Log($"Niveles desbloqueados: {nivelesActuales}, Nivel que se completó: {nivelCompletado}");

                // LÓGICA CORRECTA: Si completas el nivel que está desbloqueado, aumenta en 1
                if (nivelCompletado == nivelesActuales)
                {
                    // Completaste el nivel disponible: aumentar progreso en 1
                    int nuevoProgreso = nivelesActuales + 1;
                    columnas[3] = nuevoProgreso.ToString();
                    lineas[i] = string.Join(",", columnas);
                    encontrado = true;

                    Debug.Log($"¡Nivel completado! Progreso: {nivelesActuales} -> {nuevoProgreso}");
                    NivelManager.RegistrarNivelCompletado(nuevoProgreso);
                }
                else
                {
                    // No es el nivel correcto para avanzar
                    encontrado = true;
                    Debug.Log($"Nivel {nivelCompletado} no coincide con el nivel disponible {nivelesActuales}");
                }

                break;
            }
        }

        // Si no existe registro, lo creamos
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
            Debug.Log($"Archivo CSV guardado exitosamente");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al guardar CSV: {e.Message}");
        }

        yield return null;
    }
}