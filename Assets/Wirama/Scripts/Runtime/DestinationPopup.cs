using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wirama.Splash
{
    /// <summary>Card that slides up with the featured destination's name, region and description.</summary>
    public class DestinationPopup : MonoBehaviour
    {
        public TMP_Text regionText;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public Button closeButton;

        /// <summary>Raised only when the player presses the X button.</summary>
        public event Action Closed;

        public bool IsOpen { get; private set; }

        RectTransform rt;
        CanvasGroup group;
        Vector2 shownPos;
        Tween tween;

        void Awake()
        {
            rt = (RectTransform)transform;
            group = GetComponent<CanvasGroup>();
            if (group == null) group = gameObject.AddComponent<CanvasGroup>();
            shownPos = rt.anchoredPosition;

            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void Show(DestinationInfo info, string islandName)
        {
            if (info == null) return;

            regionText.text = info.region.ToUpperInvariant();
            titleText.text = info.name;
            descriptionText.text = info.description;

            if (tween != null) tween.Kill();
            if (!IsOpen)
            {
                rt.anchoredPosition = shownPos + new Vector2(0f, -90f);
                rt.localScale = Vector3.one * 0.94f;
            }
            IsOpen = true;
            group.blocksRaycasts = true;
            group.interactable = true;

            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.Append(group.DOFade(1f, 0.28f));
            s.Join(rt.DOAnchorPos(shownPos, 0.5f).SetEase(Ease.OutBack));
            s.Join(rt.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            tween = s;
        }

        public void Hide()
        {
            if (!IsOpen) return;
            IsOpen = false;
            group.blocksRaycasts = false;
            group.interactable = false;

            if (tween != null) tween.Kill();
            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.Append(group.DOFade(0f, 0.2f));
            s.Join(rt.DOAnchorPos(shownPos + new Vector2(0f, -70f), 0.25f).SetEase(Ease.InCubic));
            tween = s;
        }

        void OnCloseClicked()
        {
            Hide();
            if (Closed != null) Closed();
        }
    }
}
