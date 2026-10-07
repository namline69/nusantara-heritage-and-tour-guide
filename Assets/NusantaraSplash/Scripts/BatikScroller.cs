using UnityEngine;
using UnityEngine.UI;

namespace NusantaraHeritage.Splash
{
    /// <summary>
    /// Membuat pola batik pada RawImage tile secara merata (tidak melar) di rasio layar apa pun,
    /// lalu menggesernya sangat pelan agar terasa "hidup".
    /// Texture harus Wrap Mode = Repeat (diatur otomatis oleh SplashSceneBuilder).
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class BatikScroller : MonoBehaviour
    {
        [Tooltip("Ukuran 1 tile pada canvas (unit referensi, bukan pixel layar).")]
        [SerializeField] private float tileSize = 380f;
        [Tooltip("Kecepatan geser (tile per detik).")]
        [SerializeField] private Vector2 speed = new Vector2(0.012f, -0.008f);

        private RawImage _img;
        private RectTransform _rt;
        private Vector2 _offset;

        private void Cache()
        {
            if (_img == null) _img = GetComponent<RawImage>();
            if (_rt == null) _rt = transform as RectTransform;
        }

        private void OnEnable() => Refresh();
        private void OnRectTransformDimensionsChange() => Refresh();

        private void Update()
        {
            _offset += speed * Time.unscaledDeltaTime;
            _offset.x %= 1f;
            _offset.y %= 1f;
            Refresh();
        }

        private void Refresh()
        {
            Cache();
            if (_img == null || _rt == null) return;
            Rect r = _rt.rect;
            if (r.width <= 0f || r.height <= 0f) return;
            _img.uvRect = new Rect(_offset.x, _offset.y, r.width / tileSize, r.height / tileSize);
        }
    }
}
