using UnityEngine;
using UnityEngine.UI;

public class SelectorDeImagen : MonoBehaviour
{
    [SerializeField] private Image imagenPrincipal;

    public void CambiarImagen(Sprite nuevaImagen)
    {
        imagenPrincipal.sprite = nuevaImagen;
    }
}
