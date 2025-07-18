using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextScene : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad = -1;
    [SerializeField] public bool canLoad = false;

    [Header("GameObjects de imagen completa")]
    [SerializeField] private GameObject imagenDesbloqueado;
    [SerializeField] private GameObject imagenBloqueado;
    [SerializeField] private bool ignorarProgreso = false;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        if (ignorarProgreso || (LevelProgress.Instance != null && LevelProgress.Instance.EstaDesbloqueado(sceneIndexToLoad)))
        {
            canLoad = true;
            button.interactable = true;

            if (imagenDesbloqueado != null) imagenDesbloqueado.SetActive(true);
            if (imagenBloqueado != null) imagenBloqueado.SetActive(false);
        }
        else
        {
            canLoad = false;
            button.interactable = false;

            if (imagenDesbloqueado != null) imagenDesbloqueado.SetActive(false);
            if (imagenBloqueado != null) imagenBloqueado.SetActive(true);
        }
    }


    public void LoadNext()
    {
        if (!canLoad) return;

        if (sceneIndexToLoad >= 0)
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }
}
