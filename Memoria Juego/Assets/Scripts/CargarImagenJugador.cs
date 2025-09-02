using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

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
            // Caso 2: Si es un link web (http/https)
            using (WWW www = new WWW(ruta))
            {
                yield return www;

                if (string.IsNullOrEmpty(www.error))
                {
                    Texture2D tex = www.texture;
                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    imagenUI.sprite = sprite;
                }
                else
                {
                    Debug.LogError("❌ Error cargando imagen desde URL: " + www.error);
                }
            }
        }
    }
}
