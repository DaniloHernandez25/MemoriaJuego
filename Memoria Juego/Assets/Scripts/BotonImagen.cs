using UnityEngine;

public class BotonImagen : MonoBehaviour
{
    [SerializeField] private string urlImagen; // Aquí va el link
    [SerializeField] private SelectorDeImagen selector;

    public void AlHacerClick()
    {
        selector.CambiarImagenDesdeURL(urlImagen);
    }

}
