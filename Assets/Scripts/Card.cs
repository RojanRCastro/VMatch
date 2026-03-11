using UnityEngine;
using UnityEngine.UI;
using PrimeTween;
public class Card : MonoBehaviour
{
    [SerializeField] Image iconImage;

    public Sprite hideIconSprite;
    public Sprite IconSprite;

    public bool isSelected;

    public CardManager controller;


    public void OnCardClick()
    {
        controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        IconSprite = sp;
    }


    public void Show()
    {
        controller.PlayFlipSound();   // play flip sound

        Tween.Rotation(transform, //Target
            new Vector3(0f, 180f, 0f), //Rotate 180 on y axis
            0.2f); // in 0.2 seconds
        Tween.Delay(0.1f, () => iconImage.sprite = IconSprite);
        isSelected = true;
    }

    public void Hide()
    {
        controller.PlayFlipSound();   // play flip sound

        Tween.Rotation(transform, //Target
            new Vector3(0f, 0f, 0f), //Rotate 180 on y axis
            0.2f); // in 2seconds
        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = hideIconSprite;
            isSelected = false;
        });

    }
}
