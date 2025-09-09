using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.Networking;

public class CargarImagenJugador : MonoBehaviour
{
    private Image imagenUI;

    void Start()
    {
        imagenUI = GetComponent<Image>();

        // Leer la ruta/URL guardada en PlayerPrefs
        string rutaImagen = PlayerPrefs.GetString("imagenJugador", "");

        if (!string.IsNullOrEmpty(rutaImagen))
        {
            StartCoroutine(CargarImagen(rutaImagen));
        }
        else
        {
            Debug.LogWarning("⚠ No se encontró ninguna imagen guardada en PlayerPrefs.");
        }
    }

    private IEnumerator CargarImagen(string ruta)
    {
        // Caso 1: Si es un archivo local (ruta en disco)
        if (File.Exists(ruta))
        {
            byte[] bytes = File.ReadAllBytes(ruta);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            imagenUI.sprite = sprite;
        }
        else
        {
            // Caso 2: Si es un link web (http/https) usando UnityWebRequest
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(ruta))
            {
                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (request.result != UnityWebRequest.Result.Success)
#else
                if (request.isNetworkError || request.isHttpError)
#endif
                {
                    Debug.LogError("❌ Error cargando imagen desde URL: " + request.error);
                }
                else
                {
                    Texture2D tex = DownloadHandlerTexture.GetContent(request);
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    imagenUI.sprite = sprite;
                }
            }
        }
    }
}
