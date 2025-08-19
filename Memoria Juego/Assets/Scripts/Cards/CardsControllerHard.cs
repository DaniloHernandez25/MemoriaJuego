using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.IO;

[System.Serializable]
public class SpritePair
{
    public Sprite spriteA;
    public Sprite spriteB;
}

public class CardsControllerHard : MonoBehaviour
{
    [SerializeField] CardHard cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] private List<SpritePair> spritePairsData;
    [SerializeField] GameObject nivelCompletadoPrefab;

    private List<Sprite> spritePool;
    private Dictionary<Sprite, Sprite> matchMap;

    private CardHard firstSelected;
    private CardHard secondSelected;
    private int matchCounts;

    // Para deserializar el GET de niveles
    [System.Serializable]
    private class NivelData { public int niveles_completados; }

    private void Start()
    {
        PrepareSprites();
        PrepareMatchMap();
        CreateCards();
    }

    private void PrepareSprites()
    {
        spritePool = new List<Sprite>();
        foreach (var pair in spritePairsData)
        {
            spritePool.Add(pair.spriteA);
            spritePool.Add(pair.spriteB);
        }
        ShuffleSprites(spritePool);
    }

    private void PrepareMatchMap()
    {
        matchMap = new Dictionary<Sprite, Sprite>();
        foreach (var pair in spritePairsData)
        {
            matchMap[pair.spriteA] = pair.spriteB;
            matchMap[pair.spriteB] = pair.spriteA;
        }
    }

    void CreateCards()
    {
        foreach (var s in spritePool)
        {
            var card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(s);
            card.controller = this;
        }
    }

    public void SetSelected(CardHard cardHard)
    {
        if (cardHard.isSelected) return;
        cardHard.Show();

        if (firstSelected == null)
        {
            firstSelected = cardHard;
        }
        else if (secondSelected == null)
        {
            secondSelected = cardHard;
            StartCoroutine(CheckMatching(firstSelected, secondSelected));
            firstSelected = secondSelected = null;
        }
    }

    IEnumerator CheckMatching(CardHard a, CardHard b)
    {
        yield return new WaitForSeconds(0.3f);

        if (matchMap.ContainsKey(a.iconSprite) && matchMap[a.iconSprite] == b.iconSprite)
        {
            matchCounts++;
            if (matchCounts >= spritePairsData.Count)
            {
                // Animación
                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one,      0.2f, ease: PrimeTween.Ease.InOutCubic))
                    .ChainDelay(0.3f)
                    .ChainCallback(() =>
                    {
                        // Desbloquea siguiente nivel en memoria
                        if (LevelProgress.Instance != null)
                            LevelProgress.Instance.DesbloquearNivel(SceneManager.GetActiveScene().buildIndex + 1);

                        // Popup
                        var canvas = GameObject.Find("Canvas");
                        if (canvas != null && nivelCompletadoPrefab != null)
                            Instantiate(nivelCompletadoPrefab, canvas.transform);

                        // Guarda progreso
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
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }

    private IEnumerator GuardarProgresoEnCSV()
    {
        string nombre    = PlayerPrefs.GetString("nombreJugador", "");
        string fecha     = PlayerPrefs.GetString("fechaSeleccionada", "");
        int prefNivel    = PlayerPrefs.GetInt("nivelSeleccionado", -1);

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || prefNivel < 0)
        {
            Debug.LogWarning("❌ Faltan datos para guardar progreso");
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
                    Debug.Log("🔒 El nivel seleccionado es menor al ya almacenado. No actualizo CSV.");
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
        Debug.Log("✅ Progreso guardado en CSV correctamente");

        yield return null;
    }
}
