using UnityEngine;

namespace Wirama.Splash
{
    /// <summary>
    /// Fits this RectTransform to Screen.safeArea so nothing sits under the notch / Dynamic Island
    /// or the iPhone home indicator. Put it on a full-stretch child of the Canvas.
    /// </summary>
    [DisallowMultipleComponent]
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rt;
        Rect lastSafe;
        int lastW, lastH;

        void Awake()
        {
            rt = (RectTransform)transform;
            Apply();
        }

        void OnEnable()
        {
            if (rt == null) rt = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != lastSafe || Screen.width != lastW || Screen.height != lastH)
                Apply();
        }

        void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect sa = Screen.safeArea;
            lastSafe = sa; lastW = Screen.width; lastH = Screen.height;

            Vector2 min = sa.position;
            Vector2 max = sa.position + sa.size;
            min.x /= Screen.width;  min.y /= Screen.height;
            max.x /= Screen.width;  max.y /= Screen.height;

            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
