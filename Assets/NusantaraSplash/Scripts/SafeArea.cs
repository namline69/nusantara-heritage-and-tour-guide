using UnityEngine;

namespace NusantaraHeritage.Splash
{
    /// <summary>
    /// Menyesuaikan RectTransform dengan Screen.safeArea (notch / Dynamic Island / home indicator iPhone).
    /// Pasang pada panel yang membungkus semua konten penting. Background tetap full-screen di luar panel ini.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafe;
        private Vector2Int _lastScreen;

        private void OnEnable()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Murah: hanya menghitung ulang jika safe area / orientasi / resolusi berubah.
            if (_lastSafe != Screen.safeArea || _lastScreen.x != Screen.width || _lastScreen.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Rect safe = Screen.safeArea;
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width;  min.y /= Screen.height;
            max.x /= Screen.width;  max.y /= Screen.height;

            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;

            _lastSafe = safe;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
        }
    }
}
