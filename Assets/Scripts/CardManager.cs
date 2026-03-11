using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardManager : MonoBehaviour
{
    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;
    [SerializeField] RectTransform gridContainer;
    [SerializeField] UnityEngine.UI.GridLayoutGroup gridLayout;

    [SerializeField] AudioSource audioSource;

    [SerializeField] AudioClip flipSound;
    [SerializeField] AudioClip matchSound;
    [SerializeField] AudioClip wrongSound;
    [SerializeField] AudioClip winSound;

    [SerializeField] TMP_Text movesText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text bestScoreText;
    [SerializeField] TMP_Text levelText;

    //private List<Sprite> spritePairs;

    List<Card> cards = new List<Card>();
    bool canPlay = false;

    Card firstSelected, secondSelected;

    [SerializeField] int rows = 3;
    [SerializeField] int columns = 4;

    int matchCount;
    int moves = 0;
    int score = 0;
    int bestScore;
    int level = 1;

    private void Start()
    {
        LoadProgress();

        movesText.text = "Moves: 0";
        scoreText.text = "Score: 0";
        levelText.text = "Level 1";

        StartCoroutine(InitializeBoard());
    }

    IEnumerator InitializeBoard()
    {
        yield return new WaitForEndOfFrame();

        int totalCards = rows * columns;

        if (totalCards % 2 != 0)
        {
            Debug.LogError("Card layout must contain an even number of cards!");
            yield break;
        }

        UpdateGridLayout();
        CreateCards();
    }

    void LoadProgress()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);

        bestScoreText.text = "Best: " + bestScore;
    }

    void UpdateGridLayout()
    {
        float width = gridContainer.rect.width;
        float height = gridContainer.rect.height;

        float spacingX = gridLayout.spacing.x;
        float spacingY = gridLayout.spacing.y;

        // available space inside container
        float availableWidth = width - (columns - 1) * spacingX;
        float availableHeight = height - (rows - 1) * spacingY;

        float cardWidth = availableWidth / columns;
        float cardHeight = availableHeight / rows;

        // pick the smaller value so cards always fit
        float size = Mathf.Min(cardWidth, cardHeight);

        gridLayout.cellSize = new Vector2(size, size);
        gridLayout.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    void CreateCards()
    {
        int totalCards = rows * columns;

        List<Sprite> cardSprites = new List<Sprite>();

        int pairCount = totalCards / 2;

        for (int i = 0; i < pairCount; i++)
        {
            Sprite sp = sprites[i % sprites.Length];

            cardSprites.Add(sp);
            cardSprites.Add(sp);
        }

        ShuffleSprites(cardSprites);

        for (int i = 0; i < totalCards; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);

            card.SetIconSprite(cardSprites[i]);
            card.controller = this;

            cards.Add(card);
        }

        StartCoroutine(ShowCardsAtStart());
    }

    public void SetSelected(Card card)
    {
        if (!canPlay) return;

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

                moves++;
                movesText.text = "Moves: " + moves;

                canPlay = false;   // prevent extra clicks

                StartCoroutine(CheckMatch(firstSelected, secondSelected));
                firstSelected = null;
                secondSelected = null;
            }
        }
    }

    IEnumerator ShowCardsAtStart()
    {
        // wait for UI/layout to settle
        yield return new WaitForSeconds(0.05f);

        // reveal cards one by one
        foreach (Card card in cards)
        {
            card.Show();
            yield return new WaitForSeconds(0.1f);
        }

        // wait for player to memorize
        yield return new WaitForSeconds(2f);

        // hide cards one by one
        foreach (Card card in cards)
        {
            card.Hide();
            yield return new WaitForSeconds(0.05f);
        }

        canPlay = true;
    }

    public void PlayFlipSound()
    {
        audioSource.PlayOneShot(flipSound);
    }

    IEnumerator CheckMatch(Card a, Card b)
    {
        yield return new WaitForSeconds(0.3f);
        if (a.IconSprite == b.IconSprite)
        {
            //Matched
            audioSource.PlayOneShot(matchSound);

            //Matched
            matchCount++;

            score += 100;
            scoreText.text = "Score: " + score;

            if (score > bestScore)
            {
                bestScore = score;
                PlayerPrefs.SetInt("BestScore", bestScore);

                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(bestScoreText.transform, Vector3.one * 1.2f, 0.15f))
                    .Chain(PrimeTween.Tween.Scale(bestScoreText.transform, Vector3.one, 0.15f));

                PlayerPrefs.Save();
                bestScoreText.text = "Best: " + bestScore;
            }

            if (matchCount >= (rows * columns) / 2)
            {

                

                audioSource.PlayOneShot(winSound);

                StartCoroutine(NextLevel());

                PrimeTween.Sequence.Create()
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
                    .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.1f));
            }
        }


        else
        {
            audioSource.PlayOneShot(wrongSound);

            //flip back
            a.Hide();
            b.Hide();
        }

        canPlay = true;
    }

    IEnumerator NextLevel()
    {
        canPlay = false;

        // small win animation
        PrimeTween.Sequence.Create()
            .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one * 1.2f, 0.2f, ease: PrimeTween.Ease.OutBack))
            .Chain(PrimeTween.Tween.Scale(gridTransform, Vector3.one, 0.1f));

        yield return new WaitForSeconds(1f);

        level++;
        levelText.text = "Level " + level;

        // reset level values
        matchCount = 0;

        // destroy old cards
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        cards.Clear();

        CreateCards();
    }

    void ShuffleSprites(List<Sprite> spriteList)
    {
        for (int i = spriteList.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            //Swap the elements at i and randomIndex
            //Sprite temp = spriteList[i];
            //spriteList[i] = spriteList[randomIndex];
            //spriteList[randomIndex] = temp;
            (spriteList[i], spriteList[randomIndex]) = (spriteList[randomIndex], spriteList[i]);
        }
    }
}
