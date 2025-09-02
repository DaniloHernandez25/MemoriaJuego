using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadNextSceneNormal : MonoBehaviour
{
    [SerializeField] private int sceneIndexToLoad = -1;

    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();
        button.interactable = true;
    }

    public void LoadNext()
    {
        if (sceneIndexToLoad >= 0)
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }


    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
