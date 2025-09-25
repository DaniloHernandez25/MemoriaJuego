using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIScaler : MonoBehaviour
{
    [Header("Resolución base de referencia")]
    public Vector2 resolucionReferencia = new Vector2(1920, 1080);

    private void Awake()
    {
        // Solo permitir una instancia (evita duplicados en DontDestroyOnLoad)
        if (Object.FindObjectsByType<UIScaler>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        AjustarEscala();

        // Suscribirse al evento de carga de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Evitar fugas de memoria
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AjustarEscala();
    }

    void AjustarEscala()
    {
        // Reemplaza la búsqueda de CanvasScaler
        CanvasScaler[] escaladores = Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None);

        foreach (CanvasScaler scaler in escaladores)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = resolucionReferencia;
            scaler.matchWidthOrHeight = 0.5f; // 0 = ancho, 1 = alto, 0.5 = mezcla
        }
    }
}
