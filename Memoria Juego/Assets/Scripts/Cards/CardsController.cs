// CardsControllerCSV.cs
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
        string nombre    = PlayerPrefs.GetString("nombreJugador", "");
        string fecha     = PlayerPrefs.GetString("fechaSeleccionada", "");
        int prefNivel    = PlayerPrefs.GetInt("nivelSeleccionado", -1);

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || prefNivel < 0)
        {
            yield break;
        }

        string path = Application.persistentDataPath + "/progreso.csv";

        // Crear archivo si no existe
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Nombre,Fecha,FasesCompletadas,NivelesCompletados\n");
        }

        string[] lineas = File.ReadAllLines(path);
        bool encontrado = false;

        for (int i = 0; i < lineas.Length; i++)
        {
            if (lineas[i].StartsWith("Nombre,")) continue;

            string[] columnas = lineas[i].Split(',');
            if (columnas.Length < 4) continue;

            if (columnas[0] == nombre && columnas[1] == fecha)
            {
                // Bloqueo si nivelSeleccionado < niveles_completados
                if (!int.TryParse(columnas[3], out int nivelesActuales))
                    nivelesActuales = 0;

                if (prefNivel < nivelesActuales)
                {
                    yield break;
                }

                // Incrementar niveles_completados en 1
                nivelesActuales += 1;
                columnas[3] = nivelesActuales.ToString();
                lineas[i] = string.Join(",", columnas);
                encontrado = true;

                NivelManager.RegistrarNivelCompletado(prefNivel);
                break;
            }
        }

        // Si no existe registro, lo creamos
        if (!encontrado)
        {
            string nuevaLinea = $"{nombre},{fecha},0,1"; // FasesCompletadas = 0, NivelesCompletados = 1
            List<string> lineasList = new List<string>(lineas) { nuevaLinea };
            lineas = lineasList.ToArray();
        }

        File.WriteAllLines(path, lineas);

        yield return null;
    }
}
