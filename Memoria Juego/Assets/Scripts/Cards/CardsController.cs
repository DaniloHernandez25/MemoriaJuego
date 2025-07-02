using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CardsController : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;

    private List<Sprite> spritePairs;

    Card firstSelected;
    Card secondSelected;

    int matchCounts;

    private void Start()
    {
        PrepareSprites();
        CreateCards();
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++)
        {
            //añade sprites 2 veces para que haya pares
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        ShuffleSprites(spritePairs);
    }
    void CreateCards() {
        {
            for (int i = 0; i < spritePairs.Count; i++)
            {
                Card card = Instantiate(cardPrefab, gridTransform);
                card.SetIconSprite(spritePairs[i]);
                card.controller = this;
            }
        }
    }
    public void SetSelected(Card card)
    {
        if (card.isSelected == false)
        {
            card.Show();

            if (firstSelected == null)
            {
                firstSelected = card;
                return;
            }

            if (secondSelected == null)
            {
                secondSelected = card;
                StartCoroutine(CheckMatching(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }

    IEnumerator CheckMatching(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);
        if (a.iconSprite == b.iconSprite)
        {
            matchCounts++;
            if (matchCounts >= spritePairs.Count / 2)
            {
                // Nivel completado: animación y luego cambio de escena
                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.2f, ease: PrimeTween.Ease.InOutCubic))
                    .ChainDelay(0.3f) // Pequeña pausa si deseas un efecto más limpio
                    .ChainCallback(() =>
                    {
                        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                        SceneManager.LoadScene(nextSceneIndex);
                    });
            }

    
        }
        else
        {
            // Dar vuelta a las cartas
            a.Hide();
            b.Hide();
        }
    }

    void ShuffleSprites(List<Sprite> spritelist)
    {
        for (int i = spritelist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            // Swap elements at i and randomIndex
            Sprite temp = spritelist[i];
            spritelist[i] = spritelist[randomIndex];
            spritelist[randomIndex] = temp;
        }
    }

    internal void SetSelected(CardHard cardHard)
    {
        throw new System.NotImplementedException();
    }

    public static implicit operator CardsController(CardsControllerHard v)
    {
        throw new System.NotImplementedException();
    }
}