using UnityEngine;

public class BotonImagen : MonoBehaviour
{
    [SerializeField] private Sprite spriteParaMostrar;
    [SerializeField] private SelectorDeImagen selector;

    public void AlHacerClick()
    {
        selector.CambiarImagen(spriteParaMostrar);
    }
}
