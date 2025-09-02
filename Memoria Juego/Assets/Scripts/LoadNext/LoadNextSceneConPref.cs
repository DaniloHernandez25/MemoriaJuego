using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextSceneConPref : MonoBehaviour
{
    [SerializeField] private int nivelIndex;     // Nivel “dinámico” que quieres guardar
    [SerializeField] private int escenaDestino;  // Opcional: si quieres cambiar escena

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        string nombre = PlayerPrefs.GetString("nombreJugador", "");
        string fecha = PlayerPrefs.GetString("fechaSeleccionada", "");

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(fecha))
        {
            return;
        }

        // Guarda el nivel “dinámico”
        PlayerPrefs.SetInt("nivelSeleccionado", nivelIndex);
        PlayerPrefs.Save();

        // Opcional: carga otra escena
        if (escenaDestino >= 0)
            SceneManager.LoadScene(escenaDestino);
    }
    private void OnApplicationQuit() {
        PlayerPrefs.DeleteKey("nivelSeleccionado");
    }

}
