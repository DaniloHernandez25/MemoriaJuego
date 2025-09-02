using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;


public class CardHard : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite { get; private set; }
    public bool isSelected;

    public CardsControllerHard controller;

    public void OnCardClick()
{
    if (controller != null)
    {
        controller.SetSelected(this);
    }
}


    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
        iconImage.sprite = hiddenIconSprite; // Inicia oculta
    }

    public void Show()
    {
        isSelected = true;
        Sequence.Create()
            .Chain(Tween.Rotation(transform, new Vector3(0, 90, 0), 0.1f))
            .ChainCallback(() => iconImage.sprite = iconSprite)
            .Chain(Tween.Rotation(transform, new Vector3(0, 180, 0), 0.1f));
    }

    public void Hide()
    {
        isSelected = false;
        Sequence.Create()
            .Chain(Tween.Rotation(transform, new Vector3(0, 90, 0), 0.1f))
            .ChainCallback(() => iconImage.sprite = hiddenIconSprite)
            .Chain(Tween.Rotation(transform, new Vector3(0, 0, 0), 0.1f));
    }
}