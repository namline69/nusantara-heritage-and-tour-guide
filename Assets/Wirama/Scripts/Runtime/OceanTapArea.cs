using UnityEngine;
using UnityEngine.EventSystems;

namespace Wirama.Splash
{
    /// <summary>
    /// Invisible full-size target behind the map. Tapping empty sea closes the popup or zooms back out,
    /// and during the intro it skips the animation.
    /// </summary>
    public class OceanTapArea : MonoBehaviour, IPointerClickHandler
    {
        SplashController controller;

        void Awake()
        {
            controller = GetComponentInParent<SplashController>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (controller != null) controller.OnOceanTapped();
        }
    }
}
