using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NusantaraHeritage.Splash
{
    /// <summary>
    /// Alur splash: Fade-in -> Batik reveal -> Logo pop -> Judul + tagline -> Loading bar (preload scene Home)
    /// -> Tombol MULAI muncul (pulse) -> klik -> fade out -> masuk Home.
    /// Animasi memakai DOTween. Semua tween memakai SetUpdate(true) (tidak terpengaruh Time.timeScale).
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("Scene Tujuan")]
        [SerializeField] private string nextSceneName = "Home";
        [Tooltip("Durasi minimum bar loading terisi penuh (detik).")]
        [SerializeField, Min(0.5f)] private float minLoadingTime = 2.4f;

        [Header("Background")]
        [SerializeField] private CanvasGroup batikGroup;
        [SerializeField] private RectTransform batikRect;
        [SerializeField, Range(0f, 1f)] private float batikMaxAlpha = 0.30f;

        [Header("Logo & Teks")]
        [SerializeField] private RectTransform logoRect;
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private RectTransform glowRect;
        [SerializeField] private CanvasGroup glowGroup;
        [SerializeField] private RectTransform titleRect;
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private CanvasGroup taglineGroup;
        [SerializeField] private CanvasGroup footerGroup;

        [Header("Loading")]
        [SerializeField] private CanvasGroup loadingGroup;
        [SerializeField] private Image loadingFill;
        [SerializeField] private TMP_Text loadingLabel;

        [Header("Tombol Mulai")]
        [SerializeField] private Button startButton;
        [SerializeField] private CanvasGroup startGroup;
        [SerializeField] private RectTransform startRect;

        [Header("Transisi")]
        [SerializeField] private CanvasGroup fadeOverlay;

        [Header("Audio (opsional)")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioClip clickClip;

        private AsyncOperation _sceneOp;
        private Tween _pulse, _float, _glowPulse;
        private float _titleBaseY, _logoBaseY;
        private bool _clicked;

        private void Awake()
        {
            Application.targetFrameRate = 60;

            _titleBaseY = titleRect.anchoredPosition.y;
            _logoBaseY = logoRect.anchoredPosition.y;

            // State awal (semua tersembunyi)
            batikGroup.alpha = 0f;
            batikRect.localScale = Vector3.one * 1.25f;

            logoGroup.alpha = 0f;
            logoRect.localScale = Vector3.zero;
            logoRect.localRotation = Quaternion.Euler(0f, 0f, -30f);
            glowGroup.alpha = 0f;

            titleGroup.alpha = 0f;
            titleRect.anchoredPosition = new Vector2(titleRect.anchoredPosition.x, _titleBaseY - 60f);
            taglineGroup.alpha = 0f;
            footerGroup.alpha = 0f;

            loadingGroup.alpha = 0f;
            loadingFill.fillAmount = 0f;
            loadingLabel.text = "Memuat 0%";

            startGroup.alpha = 0f;
            startGroup.interactable = false;
            startGroup.blocksRaycasts = false;

            fadeOverlay.alpha = 1f;
            fadeOverlay.blocksRaycasts = false;

            startButton.onClick.AddListener(OnStartClicked);
        }

        private void Start() => StartCoroutine(Run());

        private void OnDestroy()
        {
            _pulse?.Kill();
            _float?.Kill();
            _glowPulse?.Kill();
            if (startButton != null) startButton.onClick.RemoveListener(OnStartClicked);
        }

        // ------------------------------------------------------------------ Alur utama
        private IEnumerator Run()
        {
            PlayIntro();
            yield return null;

            // Preload scene tujuan di belakang layar (tidak aktif sebelum tombol Mulai ditekan)
            if (Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                _sceneOp = SceneManager.LoadSceneAsync(nextSceneName);
                _sceneOp.allowSceneActivation = false;
            }
            else
            {
                Debug.LogWarning($"[Splash] Scene '{nextSceneName}' belum ada di Build Settings. " +
                                 "Tombol Mulai tidak akan pindah scene sampai scene ditambahkan.");
            }

            // Tunggu bar loading muncul bersama animasi intro
            yield return new WaitForSecondsRealtime(1.2f);

            float elapsed = 0f, shown = 0f;
            while (shown < 0.999f)
            {
                elapsed += Time.unscaledDeltaTime;
                float timeP = Mathf.Clamp01(elapsed / minLoadingTime);
                float loadP = _sceneOp != null ? Mathf.Clamp01(_sceneOp.progress / 0.9f) : 1f;
                float target = Mathf.Min(timeP, loadP);
                shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 0.9f);

                loadingFill.fillAmount = shown;
                loadingLabel.text = $"Memuat {Mathf.RoundToInt(shown * 100f)}%";
                yield return null;
            }

            loadingFill.fillAmount = 1f;
            loadingLabel.text = "Siap!";
            yield return new WaitForSecondsRealtime(0.35f);

            ShowStartButton();
        }

        // ------------------------------------------------------------------ Animasi intro
        private void PlayIntro()
        {
            var s = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

            // Fade dari gelap
            s.Insert(0f, fadeOverlay.DOFade(0f, 0.6f).SetEase(Ease.OutQuad));

            // Batik reveal: muncul pelan + zoom-out halus
            s.Insert(0.2f, batikGroup.DOFade(batikMaxAlpha, 1.8f).SetEase(Ease.OutQuad));
            s.Insert(0.2f, batikRect.DOScale(1f, 2.0f).SetEase(Ease.OutCubic));

            // Logo pop (scale + putar kecil) + glow
            s.Insert(0.7f, logoGroup.DOFade(1f, 0.5f));
            s.Insert(0.7f, logoRect.DOScale(1f, 0.95f).SetEase(Ease.OutBack, 1.6f));
            s.Insert(0.7f, logoRect.DOLocalRotate(Vector3.zero, 0.95f).SetEase(Ease.OutCubic));
            s.Insert(0.9f, glowGroup.DOFade(1f, 0.9f));

            // Judul naik + fade
            s.Insert(1.05f, titleGroup.DOFade(1f, 0.7f));
            s.Insert(1.05f, titleRect.DOAnchorPosY(_titleBaseY, 0.8f).SetEase(Ease.OutCubic));

            // Tagline, loading, footer
            s.Insert(1.45f, taglineGroup.DOFade(1f, 0.6f));
            s.Insert(1.2f, loadingGroup.DOFade(1f, 0.5f));
            s.Insert(1.6f, footerGroup.DOFade(1f, 0.5f));

            s.OnComplete(StartIdleLoops);
        }

        private void StartIdleLoops()
        {
            _float = logoRect.DOAnchorPosY(_logoBaseY + 16f, 2.2f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true).SetLink(gameObject);

            _glowPulse = glowGroup.DOFade(0.55f, 1.8f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true).SetLink(gameObject);
        }

        // ------------------------------------------------------------------ Tombol Mulai
        private void ShowStartButton()
        {
            startRect.localScale = Vector3.one * 0.7f;

            var s = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);
            s.Append(loadingGroup.DOFade(0f, 0.25f));
            s.Append(startGroup.DOFade(1f, 0.35f));
            s.Join(startRect.DOScale(1f, 0.55f).SetEase(Ease.OutBack, 1.8f));
            s.OnComplete(() =>
            {
                startGroup.interactable = true;
                startGroup.blocksRaycasts = true;
                StartPulse();
            });
        }

        private void StartPulse()
        {
            _pulse?.Kill();
            startRect.localScale = Vector3.one;
            _pulse = startRect.DOScale(1.05f, 0.9f)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true).SetLink(gameObject);
        }

        private void OnStartClicked()
        {
            if (_clicked) return;
            StartCoroutine(GoNext());
        }

        private IEnumerator GoNext()
        {
            _clicked = true;
            startGroup.interactable = false;
            _pulse?.Kill();
            startRect.localScale = Vector3.one;

            if (sfxSource != null && clickClip != null) sfxSource.PlayOneShot(clickClip);

            yield return startRect.DOPunchScale(Vector3.one * -0.12f, 0.25f, 8, 0.8f)
                .SetUpdate(true).WaitForCompletion();

            if (_sceneOp == null)
            {
                Debug.LogError($"[Splash] Tidak bisa pindah: scene '{nextSceneName}' belum ada di Build Settings " +
                               "(File > Build Profiles > Scene List).");
                startGroup.interactable = true;
                _clicked = false;
                StartPulse();
                yield break;
            }

            fadeOverlay.blocksRaycasts = true;
            yield return fadeOverlay.DOFade(1f, 0.45f).SetEase(Ease.InOutQuad)
                .SetUpdate(true).WaitForCompletion();

            _sceneOp.allowSceneActivation = true;
        }
    }
}
