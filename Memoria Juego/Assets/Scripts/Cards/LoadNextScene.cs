using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    public void LoadNext()
    {
        // Carga la siguiente escena según el índice en Build Settings
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }

    // Opcional: Si quieres cargar por nombre, usa esto en lugar de LoadNext:
    // public void LoadSceneByName(string sceneName)
    // {
    //     SceneManager.LoadScene(sceneName);
    // }
}