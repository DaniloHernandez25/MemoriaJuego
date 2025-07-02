using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
using System;


public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;
    public Sprite iconSprite;
    public bool isSelected;

    public CardsController controller;

    public void OnCardClick()
    {
        controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite = sp;
    }
    public void Show()
    {
        Tween.Rotation(transform,
            new Vector3(0, 180, 0),
            0.2f);
        Tween.Delay(0.1f, () => iconImage.sprite = iconSprite);
        isSelected = true;
    }
    public void Hide()
    {
        Tween.Rotation(transform,
            new Vector3(0, 0, 0),
            0.2f);
        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            isSelected = false;
        });
    }

    public static implicit operator Card(CardHard v)
    {
        throw new NotImplementedException();
    }
}