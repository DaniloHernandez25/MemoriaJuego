using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class SelectorDeImagen : MonoBehaviour
{
    [SerializeField] private Image imagenPrincipal;
    private string urlActual;

    public void CambiarImagenDesdeURL(string url)
    {
        urlActual = url;
        StartCoroutine(CargarImagen(url));
    }

    private IEnumerator CargarImagen(string url)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar imagen: " + www.error);
        }
        else
        {
            Texture2D textura = DownloadHandlerTexture.GetContent(www);
            Sprite sprite = Sprite.Create(textura, new Rect(0, 0, textura.width, textura.height), new Vector2(0.5f, 0.5f));
            imagenPrincipal.sprite = sprite;
        }
    }

    public string ObtenerURLActual()
    {
        return urlActual;
    }
}
