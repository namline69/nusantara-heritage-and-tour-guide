using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wirama.Splash
{
    /// <summary>One island group of the map (a PNG cut from id.svg). Tap = zoom into it.</summary>
    [RequireComponent(typeof(Image))]
    public class IslandLayer : MonoBehaviour, IPointerClickHandler
    {
        public string islandId;

        Image image;
        RectTransform rt;
        SplashController controller;

        public IslandInfo Info { get { return WiramaCatalog.FindIsland(islandId); } }

        void Awake()
        {
            image = GetComponent<Image>();
            rt = (RectTransform)transform;
            controller = GetComponentInParent<SplashController>();
        }

        public void HideInstant()
        {
            image.DOKill();
            image.color = new Color(1f, 1f, 1f, 0f);
            rt.localScale = Vector3.one * 0.92f;
        }

        /// <summary>Fade + gentle scale-up used by the intro sequence.</summary>
        public Tween BuildReveal()
        {
            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.Append(image.DOFade(1f, 0.6f).SetEase(Ease.OutQuad));
            s.Join(rt.DOScale(1f, 0.75f).SetEase(Ease.OutCubic));
            return s;
        }

        public void SetDim(bool dim)
        {
            image.DOKill();
            image.DOFade(dim ? 0.38f : 1f, 0.35f).SetLink(gameObject);
        }

        public void Pulse()
        {
            rt.DOKill(true);
            rt.localScale = Vector3.one;
            rt.DOPunchScale(Vector3.one * 0.035f, 0.45f, 6, 0.5f).SetLink(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller != null) controller.OnIslandTapped(this);
        }
    }
}
