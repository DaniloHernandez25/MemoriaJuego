using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextSceneConPref : MonoBehaviour
{
    [SerializeField] private int nivelIndex; // índice del nivel o escena
    [SerializeField] private int escenaDestino; // índice de la escena a cargar

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
            Debug.LogWarning("⚠️ No se puede continuar: faltan datos en PlayerPrefs.");
            return;
        }

        PlayerPrefs.SetInt("nivelSeleccionado", nivelIndex);
        PlayerPrefs.Save();

        Debug.Log($"✅ Nivel seleccionado: {nivelIndex}");
        Debug.Log($"➡️ Cargando escena {escenaDestino}");
        SceneManager.LoadScene(escenaDestino);
    }
}
