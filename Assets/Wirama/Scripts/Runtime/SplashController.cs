using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wirama.Splash
{
    /// <summary>
    /// Brain of the splash page: intro animation, interactive map (island zoom + pins + popup),
    /// and the "Mulai" button that fades into the Home scene.
    /// All references are wired by Tools > Wirama > 1. Build Splash Scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class SplashController : MonoBehaviour
    {
        [Header("Navigasi")]
        public string homeSceneName = "Home";

        [Header("Latar")]
        public RectTransform kawung;
        public RectTransform glow;

        [Header("Header")]
        public RectTransform logo;
        public RectTransform title;
        public RectTransform tagline;

        [Header("Peta")]
        public RectTransform mapContainer;
        public RectTransform mapRoot;
        public TMP_Text islandLabel;
        public Button backButton;
        public TMP_Text hint;
        [Tooltip("Geser peta overview ke atas sebesar fraksi tinggi container.")]
        public float overviewLift = 0.08f;
        public float maxFocusZoom = 2.8f;
        public float overviewPinScale = 0.9f;
        public float focusPinScale = 1.5f;
        [Tooltip("Tinggi area bawah peta yang tertutup kartu popup (canvas units).")]
        public float popupReserve = 500f;

        [Header("UI")]
        public DestinationPopup popup;
        public RectTransform startRoot;
        public Button startButton;
        public CanvasGroup fade;

        [Header("Audio")]
        public AudioSource sfx;
        public AudioClip gong;
        public AudioClip ting;
        public AudioClip click;

        const string HintOverview = "Ketuk pulau atau pin\nuntuk menjelajah destinasi unggulan";
        const string HintFocused = "Ketuk pin untuk melihat destinasi\nketuk laut untuk kembali";

        IslandLayer[] islands;
        MapPin[] pins;
        IslandLayer focused;
        MapPin selectedPin;

        CanvasGroup logoGroup, titleGroup, taglineGroup, hintGroup, labelGroup, backGroup, startGroup;
        Vector2 titleBase, taglineBase;

        Sequence intro;
        Tween zoomTween, startPulse;
        float zoom = 1f;
        float zoomT;
        Vector2 lastContainerSize;
        bool ready, leaving;

        bool Interactive { get { return ready && !leaving; } }

        // ------------------------------------------------------------------ setup

        void Awake()
        {
            Application.targetFrameRate = 60;           // iOS defaults to 30 fps
            DOTween.SetTweensCapacity(500, 125);

            islands = mapRoot.GetComponentsInChildren<IslandLayer>(true);
            pins = mapRoot.GetComponentsInChildren<MapPin>(true);

            logoGroup = Group(logo);
            titleGroup = Group(title);
            taglineGroup = Group(tagline);
            hintGroup = Group(hint);
            labelGroup = Group(islandLabel);
            backGroup = Group(backButton);
            startGroup = Group(startRoot);

            popup.Closed += HandlePopupClosed;
            startButton.onClick.AddListener(OnStartClicked);
            backButton.onClick.AddListener(ResetView);
        }

        void OnDestroy()
        {
            if (popup != null) popup.Closed -= HandlePopupClosed;
        }

        void Start()
        {
            Canvas.ForceUpdateCanvases();
            PrepareIntro();
            lastContainerSize = mapContainer.rect.size;
            ApplyView(OverviewZoom, OverviewPos);
            PlayIntro();
            StartBackgroundLoops();
        }

        void Update()
        {
            // Rotation / window resize / safe-area change: re-fit the map.
            Vector2 s = mapContainer.rect.size;
            if ((s - lastContainerSize).sqrMagnitude > 0.25f)
            {
                lastContainerSize = s;
                RefreshView();
            }
        }

        static CanvasGroup Group(Component c)
        {
            CanvasGroup g = c.GetComponent<CanvasGroup>();
            if (g == null) g = c.gameObject.AddComponent<CanvasGroup>();
            return g;
        }

        // ------------------------------------------------------------------ intro

        void PrepareIntro()
        {
            logoGroup.alpha = 0f;
            logo.localScale = Vector3.one * 0.4f;

            titleBase = title.anchoredPosition;
            taglineBase = tagline.anchoredPosition;
            titleGroup.alpha = 0f;
            taglineGroup.alpha = 0f;
            title.anchoredPosition = titleBase + new Vector2(0f, -40f);
            tagline.anchoredPosition = taglineBase + new Vector2(0f, -40f);

            hintGroup.alpha = 0f;
            hint.text = HintOverview;
            labelGroup.alpha = 0f;
            backGroup.alpha = 0f;
            backGroup.blocksRaycasts = false;

            startGroup.alpha = 0f;
            startGroup.blocksRaycasts = false;
            startRoot.localScale = Vector3.one * 0.8f;

            foreach (IslandLayer i in islands) i.HideInstant();
            foreach (MapPin p in pins) p.HideInstant();

            fade.alpha = 1f;
            fade.blocksRaycasts = true;
        }

        void PlayIntro()
        {
            intro = DOTween.Sequence().SetLink(gameObject);

            // 0.0 - screen fades in from the background colour
            intro.Insert(0f, fade.DOFade(0f, 0.5f).SetEase(Ease.OutQuad));
            intro.InsertCallback(0.5f, () => fade.blocksRaycasts = false);

            // 0.35 - logo pops in with a gong
            intro.InsertCallback(0.35f, () => Play(gong, 0.8f));
            intro.Insert(0.35f, logo.DOScale(1f, 0.9f).SetEase(Ease.OutBack));
            intro.Insert(0.35f, logoGroup.DOFade(1f, 0.5f));

            // 0.8 / 0.95 - name and tagline slide up
            intro.Insert(0.8f, titleGroup.DOFade(1f, 0.5f));
            intro.Insert(0.8f, title.DOAnchorPos(titleBase, 0.6f).SetEase(Ease.OutCubic));
            intro.Insert(0.95f, taglineGroup.DOFade(1f, 0.5f));
            intro.Insert(0.95f, tagline.DOAnchorPos(taglineBase, 0.6f).SetEase(Ease.OutCubic));

            // 1.15+ - islands appear west -> east
            for (int i = 0; i < islands.Length; i++)
                intro.Insert(1.15f + i * 0.18f, islands[i].BuildReveal());

            // 1.75+ - pins drop onto the map
            for (int k = 0; k < pins.Length; k++)
            {
                float t = 1.75f + k * 0.05f;
                intro.Insert(t, pins[k].BuildDrop());
                if (k % 3 == 0)
                {
                    int idx = k;
                    intro.InsertCallback(t + 0.3f, () => Play(ting, 0.18f, 1f + 0.025f * idx));
                }
            }

            // 3.0 - Mulai button and hint
            intro.Insert(3.0f, startGroup.DOFade(1f, 0.4f));
            intro.Insert(3.0f, startRoot.DOScale(1f, 0.6f).SetEase(Ease.OutBack));
            intro.Insert(3.2f, hintGroup.DOFade(1f, 0.5f));

            intro.OnComplete(OnIntroComplete);
        }

        void OnIntroComplete()
        {
            if (ready) return;
            ready = true;
            fade.blocksRaycasts = false;
            startGroup.alpha = 1f;
            startGroup.blocksRaycasts = true;
            startRoot.localScale = Vector3.one;
            hintGroup.alpha = 1f;

            foreach (MapPin p in pins) p.StartPulse();
            StartButtonPulse();

            logo.DOAnchorPosY(logo.anchoredPosition.y + 12f, 2.4f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        void StartBackgroundLoops()
        {
            // Kawung pattern drifts diagonally by exactly one tile (128) so the loop is seamless.
            if (kawung != null)
            {
                Vector2 p = kawung.anchoredPosition;
                kawung.DOAnchorPos(p + new Vector2(128f, -128f), 16f)
                      .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart).SetLink(gameObject);
            }
            // Ocean glow breathes.
            if (glow != null)
            {
                glow.DOScale(1.12f, 3.2f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
            }
        }

        void StartButtonPulse()
        {
            if (startPulse != null) startPulse.Kill();
            startRoot.localScale = Vector3.one;
            startPulse = startRoot.DOScale(1.045f, 0.9f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(gameObject);
        }

        // ------------------------------------------------------------------ map interaction

        void SkipIntro()
        {
            if (intro != null && intro.IsActive()) intro.Complete();
        }

        public void OnIslandTapped(IslandLayer layer)
        {
            if (!ready) { SkipIntro(); return; }
            if (!Interactive) return;
            Play(ting, 0.35f, 0.9f);
            layer.Pulse();
            if (focused != layer) FocusIsland(layer, null);
        }

        public void OnPinTapped(MapPin pin)
        {
            if (!ready) { SkipIntro(); return; }
            if (!Interactive) return;
            IslandLayer layer = FindLayer(pin.islandId);
            if (layer == null) return;

            if (focused != layer)
            {
                FocusIsland(layer, pin);   // zoom first, then show the popup
                return;
            }
            OpenPin(pin);
        }

        public void OnOceanTapped()
        {
            if (!ready)
            {
                SkipIntro();   // tap skips the intro
                return;
            }
            if (leaving) return;
            if (popup.IsOpen) { ClosePopup(); return; }
            if (focused != null) ResetView();
        }

        void FocusIsland(IslandLayer layer, MapPin thenOpen)
        {
            focused = layer;
            IslandInfo info = layer.Info;

            if (popup.IsOpen && (thenOpen == null || selectedPin != thenOpen)) ClosePopup();
            else if (thenOpen == null) DeselectPin();

            foreach (IslandLayer i in islands) i.SetDim(i != layer);
            foreach (MapPin p in pins) p.SetDim(p.islandId != layer.islandId);

            islandLabel.text = info != null ? info.displayName : layer.islandId;
            FadeGroup(labelGroup, 1f, 0.35f);
            backGroup.blocksRaycasts = true;
            FadeGroup(backGroup, 1f, 0.35f);
            SetHint(HintFocused);

            float z; Vector2 pos;
            ComputeFocus(layer, out z, out pos);
            const float dur = 0.75f;
            ZoomTo(z, pos, dur);

            if (thenOpen != null)
            {
                MapPin target = thenOpen;
                DOVirtual.DelayedCall(dur * 0.85f, () =>
                {
                    if (!leaving && focused == layer) OpenPin(target);
                }).SetLink(gameObject);
            }
        }

        void OpenPin(MapPin pin)
        {
            if (selectedPin != null && selectedPin != pin) selectedPin.SetSelected(false);
            selectedPin = pin;
            pin.transform.SetAsLastSibling();
            pin.SetSelected(true);
            Play(ting, 0.6f, 1.15f);

            IslandInfo island = WiramaCatalog.FindIsland(pin.islandId);
            popup.Show(pin.Info, island != null ? island.displayName : "");
            FadeGroup(hintGroup, 0f, 0.2f);
        }

        void ClosePopup()
        {
            popup.Hide();
            HandlePopupClosed();
        }

        void HandlePopupClosed()
        {
            DeselectPin();
            if (ready && !leaving) FadeGroup(hintGroup, 1f, 0.3f);
        }

        void DeselectPin()
        {
            if (selectedPin != null) selectedPin.SetSelected(false);
            selectedPin = null;
        }

        public void ResetView()
        {
            if (!Interactive) return;
            if (popup.IsOpen) ClosePopup(); else DeselectPin();

            focused = null;
            foreach (IslandLayer i in islands) i.SetDim(false);
            foreach (MapPin p in pins) p.SetDim(false);

            FadeGroup(labelGroup, 0f, 0.25f);
            backGroup.blocksRaycasts = false;
            FadeGroup(backGroup, 0f, 0.25f);
            SetHint(HintOverview);

            ZoomTo(OverviewZoom, OverviewPos, 0.7f);
        }

        IslandLayer FindLayer(string id)
        {
            foreach (IslandLayer i in islands) if (i.islandId == id) return i;
            return null;
        }

        void SetHint(string text)
        {
            hintGroup.DOKill();
            hint.text = text;
            if (!popup.IsOpen) hintGroup.DOFade(1f, 0.3f).SetLink(gameObject);
        }

        static void FadeGroup(CanvasGroup g, float alpha, float duration)
        {
            g.DOKill();
            g.DOFade(alpha, duration).SetLink(g.gameObject);
        }

        // ------------------------------------------------------------------ zoom maths

        float OverviewZoom
        {
            get { return Mathf.Clamp(mapContainer.rect.width * 0.98f / MapMath.MapWidth, 0.3f, 1f); }
        }

        Vector2 OverviewPos
        {
            get { return new Vector2(0f, mapContainer.rect.height * overviewLift); }
        }

        static Rect IslandRect(IslandInfo info)
        {
            Vector2 a = MapMath.SvgToLocal(new Vector2(info.x0, info.y1));   // bottom-left
            Vector2 b = MapMath.SvgToLocal(new Vector2(info.x1, info.y0));   // top-right
            return new Rect(a.x, a.y, b.x - a.x, b.y - a.y);
        }

        void ComputeFocus(IslandLayer layer, out float z, out Vector2 pos)
        {
            Rect r = IslandRect(layer.Info);
            float vw = mapContainer.rect.width * 0.92f;
            float vh = Mathf.Max(200f, (mapContainer.rect.height - popupReserve) * 0.9f);
            float fit = Mathf.Min(vw / (r.width * 1.08f), vh / (r.height * 1.08f));
            z = Mathf.Clamp(fit, OverviewZoom, maxFocusZoom);
            // keep the island centred in the part of the container that the popup does not cover
            pos = new Vector2(0f, popupReserve * 0.5f) - r.center * z;
        }

        void RefreshView()
        {
            if (zoomTween != null) zoomTween.Kill();
            if (focused == null)
            {
                ApplyView(OverviewZoom, OverviewPos);
            }
            else
            {
                float z; Vector2 pos;
                ComputeFocus(focused, out z, out pos);
                ApplyView(z, pos);
            }
        }

        void ZoomTo(float targetZoom, Vector2 targetPos, float duration)
        {
            if (zoomTween != null) zoomTween.Kill();
            float z0 = zoom;
            Vector2 p0 = mapRoot.anchoredPosition;
            zoomT = 0f;
            zoomTween = DOTween.To(() => zoomT, v =>
            {
                zoomT = v;
                float z = z0 * Mathf.Pow(targetZoom / z0, v);       // exponential zoom feels linear
                ApplyView(z, Vector2.Lerp(p0, targetPos, v));
            }, 1f, duration).SetEase(Ease.InOutCubic).SetLink(gameObject);
        }

        void ApplyView(float z, Vector2 pos)
        {
            zoom = z;
            mapRoot.localScale = new Vector3(z, z, 1f);
            mapRoot.anchoredPosition = pos;

            // Pins keep a readable, touch-friendly size regardless of zoom.
            float prog = Mathf.InverseLerp(OverviewZoom, maxFocusZoom, z);
            float s = Mathf.Lerp(overviewPinScale, focusPinScale, prog) / z;
            for (int i = 0; i < pins.Length; i++) pins[i].SetCounterScale(s);
        }

        // ------------------------------------------------------------------ Mulai

        void OnStartClicked()
        {
            if (!Interactive) return;
            leaving = true;
            Play(click, 1f);

            if (zoomTween != null) zoomTween.Complete();
            if (popup.IsOpen) popup.Hide();
            if (startPulse != null) startPulse.Kill();
            startRoot.localScale = Vector3.one;
            fade.blocksRaycasts = true;

            Sequence s = DOTween.Sequence().SetLink(gameObject);
            s.Append(startRoot.DOPunchScale(Vector3.one * -0.1f, 0.25f, 8, 0.8f));
            s.Append(fade.DOFade(1f, 0.5f).SetEase(Ease.InQuad));
            s.Join(logo.DOScale(1.12f, 0.5f).SetEase(Ease.InQuad));
            s.AppendCallback(LoadHome);
        }

        void LoadHome()
        {
            if (Application.CanStreamedLevelBeLoaded(homeSceneName))
            {
                SceneManager.LoadSceneAsync(homeSceneName);
                return;
            }

            Debug.LogWarning("[Wirama] Scene '" + homeSceneName + "' belum ada di Build Settings. " +
                             "Jalankan Tools > Wirama > 1. Build Splash Scene atau tambahkan scene Home secara manual.");
            leaving = false;
            logo.DOScale(1f, 0.3f).SetLink(gameObject);
            fade.DOFade(0f, 0.4f).SetLink(gameObject).OnComplete(() => fade.blocksRaycasts = false);
            StartButtonPulse();
        }

        // ------------------------------------------------------------------ audio

        void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null || sfx == null) return;
            sfx.pitch = pitch;
            sfx.PlayOneShot(clip, volume);
        }
    }
}
