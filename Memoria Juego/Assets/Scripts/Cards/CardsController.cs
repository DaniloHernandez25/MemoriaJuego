// CardsController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

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
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one,      0.2f, ease: PrimeTween.Ease.InOutCubic))
                    .ChainDelay(0.3f)
                    .ChainCallback(() =>
                    {
                        // Mostrar popup
                        var canvas = GameObject.Find("Canvas");
                        if (canvas != null && nivelCompletadoPrefab != null)
                            Instantiate(nivelCompletadoPrefab, canvas.transform);
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
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
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
