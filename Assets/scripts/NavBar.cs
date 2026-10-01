using UnityEngine;
using UnityEngine.UI;

public class NavBar : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Sprite outlineSprite;
    [SerializeField] Sprite filledSprite;

    public void SetActive(bool active)
    {
        icon.sprite = active ? filledSprite : outlineSprite;
    }
}