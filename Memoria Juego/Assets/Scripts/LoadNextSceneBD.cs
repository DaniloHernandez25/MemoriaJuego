using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;

public class LoadNextSceneBD : MonoBehaviour
{
    [Header("Datos del jugador")]
    public TMP_InputField inputNombre;
    public SelectorDeImagen selector;

    [Header("Configuración de escena")]
    [SerializeField] private int sceneIndexToLoad = -1;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.interactable = true;
    }

    public void GuardarYContinuar()
    {
        string nombre = inputNombre.text;
        string urlImagen = selector.ObtenerURLActual();

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(urlImagen))
        {
            Debug.LogWarning("Nombre o imagen no seleccionados.");
            return;
        }

        // 🔸 Guardar el nombre para usarlo en la siguiente escena
        PlayerPrefs.SetString("nombreJugador", nombre);
        PlayerPrefs.Save();

        StartCoroutine(GuardarEnBD(nombre, urlImagen));
    }


    private IEnumerator GuardarEnBD(string nombre, string urlImagen)
    {
        WWWForm form = new WWWForm();
        form.AddField("nombre", nombre);
        form.AddField("perfil", urlImagen);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/EduardoDragon/guardar_perfil.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al guardar: " + www.error);
        }
        else
        {
            Debug.Log("Guardado exitoso: " + www.downloadHandler.text);
            CargarEscena();
        }
    }

    private void CargarEscena()
    {
        if (sceneIndexToLoad >= 0)
        {
            Debug.Log("Cambiando a la escena: " + sceneIndexToLoad);
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }
}
