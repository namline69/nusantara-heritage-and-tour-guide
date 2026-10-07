using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wirama.Splash
{
    /// <summary>
    /// A destination pin. The root RectTransform is the (generous) touch target and gets counter-scaled
    /// when the map zooms; the "visual" child is what bounces / drops in.
    /// Pivot of the root is bottom-centre, so the pin tip sits exactly on the coordinate.
    /// </summary>
    public class MapPin : MonoBehaviour, IPointerClickHandler
    {
        public string destinationId;
        public string islandId;
        public RectTransform visual;   // pin sprite
        public Image pulseRing;        // soft ring drawn at the pin tip

        const float DropHeight = 180f;

        SplashController controller;
        CanvasGroup visualGroup;
        Tween delayTween, pulseTween, scaleTween, dimTween, bounceTween;
        bool selected;

        public DestinationInfo Info { get { return WiramaCatalog.FindDestination(destinationId); } }

        void Awake()
        {
            controller = GetComponentInParent<SplashController>();
            visualGroup = visual.GetComponent<CanvasGroup>();
            if (visualGroup == null) visualGroup = visual.gameObject.AddComponent<CanvasGroup>();
            visualGroup.blocksRaycasts = false;   // the root image receives the taps
        }

        public void SetCounterScale(float s)
        {
            transform.localScale = new Vector3(s, s, 1f);
        }

        public void HideInstant()
        {
            visualGroup.alpha = 0f;
            visual.anchoredPosition = new Vector2(0f, DropHeight);
            visual.localScale = Vector3.one * 0.5f;
            Color c = pulseRing.color; c.a = 0f; pulseRing.color = c;
        }

        /// <summary>Pin falls from above and bounces on the map.</summary>
        public Tween BuildDrop()
        {
            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.Append(visualGroup.DOFade(1f, 0.15f));
            s.Join(visual.DOAnchorPosY(0f, 0.65f).SetEase(Ease.OutBounce));
            s.Join(visual.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
            return s;
        }

        /// <summary>Endless soft ripple at the pin tip, with a random offset so pins don't pulse in sync.</summary>
        public void StartPulse()
        {
            StopPulse();
            delayTween = DOVirtual.DelayedCall(Random.Range(0f, 2.5f), BeginPulseLoop).SetLink(gameObject);
        }

        void BeginPulseLoop()
        {
            RectTransform ring = pulseRing.rectTransform;
            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.AppendCallback(() =>
            {
                ring.localScale = Vector3.one * 0.3f;
                Color c = pulseRing.color; c.a = 0.75f; pulseRing.color = c;
            });
            s.Append(ring.DOScale(1.5f, 1.5f).SetEase(Ease.OutQuad));
            s.Join(pulseRing.DOFade(0f, 1.5f).SetEase(Ease.InQuad));
            s.AppendInterval(Random.Range(0.9f, 2.4f));
            s.SetLoops(-1, LoopType.Restart);
            pulseTween = s;
        }

        public void StopPulse()
        {
            if (delayTween != null) delayTween.Kill();
            if (pulseTween != null) pulseTween.Kill();
        }

        public void SetSelected(bool value)
        {
            selected = value;
            if (scaleTween != null) scaleTween.Kill();
            scaleTween = visual.DOScale(value ? 1.3f : 1f, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
            if (value) Bounce();
        }

        public bool IsSelected { get { return selected; } }

        void Bounce()
        {
            // small hop of the pin visual (position only, so it doesn't fight the scale tween)
            if (bounceTween != null) bounceTween.Kill();
            visual.anchoredPosition = Vector2.zero;
            bounceTween = visual.DOPunchAnchorPos(new Vector2(0f, 26f), 0.4f, 6, 0.6f).SetLink(gameObject);
        }

        public void SetDim(bool dim)
        {
            if (dimTween != null) dimTween.Kill();
            dimTween = visualGroup.DOFade(dim ? 0.4f : 1f, 0.3f).SetLink(gameObject);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller != null) controller.OnPinTapped(this);
        }
    }
}
