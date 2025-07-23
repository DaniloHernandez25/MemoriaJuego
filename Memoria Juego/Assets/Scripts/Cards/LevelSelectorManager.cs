using UnityEngine;
using System.Collections;

public class LevelSelectorManager : MonoBehaviour
{
    [Header("Botones de niveles")]
    public LoadNextScene[] botonesDeNivel;

    private void Start()
    {
        StartCoroutine(EsperarYActualizar());
    }

    private IEnumerator EsperarYActualizar()
    {
        // ⏳ Espera que LevelProgress termine de cargar los datos
        yield return new WaitUntil(() => LevelProgress.Instance != null && LevelProgress.Instance.nivelesDesbloqueados.Count > 0);

        foreach (LoadNextScene boton in botonesDeNivel)
        {
            if (boton.ignorarProgreso)
            {
                boton.canLoad = true;
                boton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                if (boton.imagenDesbloqueado) boton.imagenDesbloqueado.SetActive(true);
                if (boton.imagenBloqueado) boton.imagenBloqueado.SetActive(false);
            }
            else if (LevelProgress.Instance.EstaDesbloqueado(boton.sceneIndexToLoad))
            {
                boton.canLoad = true;
                boton.GetComponent<UnityEngine.UI.Button>().interactable = true;
                if (boton.imagenDesbloqueado) boton.imagenDesbloqueado.SetActive(true);
                if (boton.imagenBloqueado) boton.imagenBloqueado.SetActive(false);
            }
            else
            {
                boton.canLoad = false;
                boton.GetComponent<UnityEngine.UI.Button>().interactable = false;
                if (boton.imagenDesbloqueado) boton.imagenDesbloqueado.SetActive(false);
                if (boton.imagenBloqueado) boton.imagenBloqueado.SetActive(true);
            }
        }
    }
}
