using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class SpritePair
{
    public Sprite spriteA;
    public Sprite spriteB;
}

public class CardsControllerHard : MonoBehaviour
{
    [SerializeField] CardHard cardPrefab;
    [SerializeField] Transform gridTransform;

    [SerializeField] private List<SpritePair> spritePairsData;

    private List<Sprite> spritePool;
    private Dictionary<Sprite, Sprite> matchMap;

    CardHard firstSelected;
    CardHard secondSelected;

    int matchCounts;

    private void Start()
    {
        PrepareSprites();
        PrepareMatchMap();
        CreateCards();
    }

    private void PrepareSprites()
    {
        spritePool = new List<Sprite>();
        foreach (var pair in spritePairsData)
        {
            spritePool.Add(pair.spriteA);
            spritePool.Add(pair.spriteB);
        }
        ShuffleSprites(spritePool);
    }

    private void PrepareMatchMap()
    {
        matchMap = new Dictionary<Sprite, Sprite>();
        foreach (var pair in spritePairsData)
        {
            matchMap[pair.spriteA] = pair.spriteB;
            matchMap[pair.spriteB] = pair.spriteA;
        }
    }

    void CreateCards()
    {
        for (int i = 0; i < spritePool.Count; i++)
        {
            CardHard card = Instantiate(cardPrefab, gridTransform);
            card.SetIconSprite(spritePool[i]);
            card.controller = this;
        }
    }

    public void SetSelected(CardHard cardHard)
    {
        if (cardHard.isSelected == false)
        {
            cardHard.Show();

            if (firstSelected == null)
            {
                firstSelected = cardHard;
                return;
            }

            if (secondSelected == null)
            {
                secondSelected = cardHard;
                StartCoroutine(CheckMatching(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }

    IEnumerator CheckMatching(CardHard a, CardHard b)
    {
        yield return new WaitForSeconds(0.3f);

        if (matchMap.ContainsKey(a.iconSprite) && matchMap[a.iconSprite] == b.iconSprite)
        {
            matchCounts++;
            if (matchCounts >= spritePairsData.Count)
            {
                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.2f, ease: PrimeTween.Ease.InOutCubic))
                    .ChainDelay(0.3f)
                    .ChainCallback(() =>
                    {
                        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                        SceneManager.LoadScene(nextSceneIndex);
                    });
            }
        }
        else
        {
            a.Hide();
            b.Hide();
        }
    }

    void ShuffleSprites(List<Sprite> spritelist)
    {
        for (int i = spritelist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Sprite temp = spritelist[i];
            spritelist[i] = spritelist[randomIndex];
            spritelist[randomIndex] = temp;
        }
    }
}
