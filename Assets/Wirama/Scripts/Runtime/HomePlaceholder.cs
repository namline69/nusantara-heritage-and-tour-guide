using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Wirama.Splash
{
    /// <summary>
    /// Stand-in for the real Home scene so the splash flow can be tested end to end.
    /// Replace the "Home" scene with your team's Home scene (keep the scene name, or change
    /// SplashController.homeSceneName).
    /// </summary>
    public class HomePlaceholder : MonoBehaviour
    {
        public Button backButton;
        public CanvasGroup content;
        public string splashSceneName = "Splash";

        void Start()
        {
            if (content != null)
            {
                content.alpha = 0f;
                content.DOFade(1f, 0.6f).SetLink(gameObject);
            }
            if (backButton != null)
                backButton.onClick.AddListener(() => SceneManager.LoadScene(splashSceneName));
        }
    }
}
