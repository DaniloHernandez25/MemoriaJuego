using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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
                        StartCoroutine(GuardarProgresoEnBD());
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

    private IEnumerator GuardarProgresoEnBD()
    {
    string nombre    = PlayerPrefs.GetString("nombreJugador", "");
    string fecha     = PlayerPrefs.GetString("fechaSeleccionada", "");
    int    prefNivel = PlayerPrefs.GetInt("nivelSeleccionado", -1);

    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha) || prefNivel < 0)
    {
        Debug.LogWarning("❌ Faltan datos para guardar progreso");
        yield break;
    }

    // 1) Consultar BD
    var formGet = new WWWForm();
    formGet.AddField("nombre", nombre);
    formGet.AddField("fecha",  fecha);
    using (var reqGet = UnityWebRequest.Post("http://localhost/EduardoDragon/obtener_niveles.php", formGet))
    {
        yield return reqGet.SendWebRequest();
        if (reqGet.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ Error al obtener niveles: " + reqGet.error);
            yield break;
        }

        var data = JsonUtility.FromJson<NivelData>(reqGet.downloadHandler.text.Trim());
        Debug.Log($"📦 BD dice niveles_completados = {data.niveles_completados}");

        // 2) Bloqueo si nivelSeleccionado < niveles_completados en BD
        if (prefNivel < data.niveles_completados)
        {
            Debug.Log("🔒 El nivel seleccionado es menor al ya almacenado. No actualizo BD.");
            yield break;
        }
    }

        // 3) Enviar el nuevo valor a la BD (siempre delta = 1)
        var formSet = new WWWForm();
        formSet.AddField("nombre", nombre);
        formSet.AddField("fecha",  fecha);
        // en lugar de prefNivel, siempre mandamos 1 unidad a sumar:
        formSet.AddField("delta", 1);

        using (var reqSet = UnityWebRequest.Post(
                "http://localhost/EduardoDragon/actualizar_niveles.php",
                formSet))
        {
            yield return reqSet.SendWebRequest();
            if (reqSet.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ BD incrementada en 1 nivel (ahora debería ser +1)");
                NivelManager.RegistrarNivelCompletado(prefNivel);
            }
            else
            {
                Debug.LogError("❌ Error guardando progreso: " + reqSet.error);
            }
        }
    }
}
