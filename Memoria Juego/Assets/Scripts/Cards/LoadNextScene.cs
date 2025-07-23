using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class LoadNextScene : MonoBehaviour
{
    [SerializeField] public int sceneIndexToLoad = -1;
    [SerializeField] public bool canLoad = false;

    [Header("GameObjects de imagen completa")]
    [SerializeField] public GameObject imagenDesbloqueado;
    [SerializeField] public GameObject imagenBloqueado;
    [SerializeField] public bool ignorarProgreso = false;


    private Button button;


    public void LoadNext()
    {
        if (!canLoad) return;

        if (sceneIndexToLoad >= 0)
        {
            SceneManager.LoadScene(sceneIndexToLoad);
        }
    }
}
