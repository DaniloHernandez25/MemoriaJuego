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
        // 1) chequear nivel actual vs. lo ya cargado en NivelManager
        int nivelActual = SceneManager.GetActiveScene().buildIndex;
        if (nivelActual <= NivelManager.MaxNivelesCompletados)
        {
            Debug.Log("🔒 Ya completado según NivelManager. No actualizo BD.");
            yield break;
        }

        // 2) Preparar datos
        string nombre = PlayerPrefs.GetString("nombreJugador", "");
        string fecha  = PlayerPrefs.GetString("fechaSeleccionada", "");
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha))
        {
            Debug.LogWarning("⚠️ Faltan nombre o fecha en PlayerPrefs");
            yield break;
        }

        // 3) Enviar delta=1 al servidor
        var form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("fecha",  fecha);
        form.AddField("delta",  1); // siempre sumamos 1

        using var www = UnityWebRequest.Post(
            "http://localhost/EduardoDragon/actualizar_niveles.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ BD incrementada en 1 nivel");
            // 4) Actualizar el máximo en NivelManager para esta sesión
            NivelManager.RegistrarNivelCompletado(nivelActual);
        }
        else
        {
            Debug.LogError("❌ Error guardando progreso: " + www.error);
        }
    }
}
