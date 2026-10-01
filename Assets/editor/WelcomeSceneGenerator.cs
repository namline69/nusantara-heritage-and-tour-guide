// Simpan di: Assets/Editor/WelcomeSceneGenerator.cs
// Butuh: WelcomeController.cs (Assets/Scripts) + TextMeshPro (Window > TextMeshPro > Import TMP Essential Resources)
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

public static class WelcomeSceneGenerator
{
    // ====== TINGGAL UBAH DI SINI ======
    const string AppName      = "NamaAppKamu";
    const string Title        = "Selamat Datang";
    const string Subtitle     = "Mulai perjalananmu bersama kami.\nMudah, cepat, dan menyenangkan.";
    const string PrimaryText  = "Mulai Sekarang";
    const string SecondaryText = "Saya Sudah Punya Akun";

    static readonly Color TopColor     = Hex("#7C6CF0");
    static readonly Color BottomColor  = Hex("#2B1A6B");
    static readonly Color AccentColor  = Hex("#FFFFFF");
    static readonly Color PrimaryBtnBg = Hex("#FFFFFF");
    static readonly Color PrimaryBtnTx = Hex("#4B3BC4");

    static readonly Vector2 RefResolution = new Vector2(1080, 1920); // portrait mobile
    // ==================================

    [MenuItem("Tools/Generate Welcome Scene")]
    public static void Generate()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Kamera: warna solid, lampu tidak dibutuhkan untuk UI
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = BottomColor;
        }
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null) Object.DestroyImmediate(light.gameObject);

        var roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var circleSprite  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        var gradientSprite = CreateGradientSprite();

        // ---------- Canvas ----------
        var canvasGO = new GameObject("WelcomeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = RefResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var controller = canvasGO.AddComponent<WelcomeController>();

        // ---------- Background ----------
        var bg = MakeImage("Background", canvasGO.transform, gradientSprite, Color.white);
        Stretch(bg.rectTransform);

        // Dekorasi lingkaran transparan
        var c1 = MakeImage("Deco_Circle_1", canvasGO.transform, circleSprite, new Color(1, 1, 1, 0.07f));
        Place(c1.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(380, 760), new Vector2(700, 700));
        var c2 = MakeImage("Deco_Circle_2", canvasGO.transform, circleSprite, new Color(1, 1, 1, 0.05f));
        Place(c2.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-400, -200), new Vector2(560, 560));

        // ---------- Logo placeholder ----------
        var logo = MakeImage("Logo", canvasGO.transform, roundedSprite, AccentColor);
        logo.type = Image.Type.Sliced;
        logo.pixelsPerUnitMultiplier = 0.35f;
        Place(logo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 460), new Vector2(280, 280));
        var logoText = MakeText("LogoText", logo.transform, AppName.Substring(0, 1).ToUpper(), 150, PrimaryBtnTx, FontStyles.Bold);
        Stretch(logoText.rectTransform);

        // ---------- Title & subtitle ----------
        var title = MakeText("Title", canvasGO.transform, Title, 86, Color.white, FontStyles.Bold);
        Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(920, 120));

        var sub = MakeText("Subtitle", canvasGO.transform, Subtitle, 44, new Color(1, 1, 1, 0.8f), FontStyles.Normal);
        Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(880, 200));

        // ---------- Page indicator (dots) ----------
        for (int i = 0; i < 3; i++)
        {
            var dot = MakeImage("Dot_" + (i + 1), canvasGO.transform, circleSprite,
                new Color(1, 1, 1, i == 0 ? 1f : 0.35f));
            Place(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2((i - 1) * 40, -220), new Vector2(22, 22));
        }

        // ---------- Buttons (anchor bawah) ----------
        var primary = MakeButton("Btn_GetStarted", canvasGO.transform, roundedSprite, PrimaryBtnBg,
            PrimaryText, PrimaryBtnTx, true);
        PlaceBottom(primary.GetComponent<RectTransform>(), 340, new Vector2(880, 150));
        UnityEventTools.AddVoidPersistentListener(primary.onClick, controller.OnGetStarted);

        var secondary = MakeButton("Btn_Login", canvasGO.transform, roundedSprite, new Color(1, 1, 1, 0.16f),
            SecondaryText, Color.white, false);
        PlaceBottom(secondary.GetComponent<RectTransform>(), 160, new Vector2(880, 150));
        UnityEventTools.AddVoidPersistentListener(secondary.onClick, controller.OnLogin);

        var footer = MakeText("Footer", canvasGO.transform, "Dengan melanjutkan, kamu menyetujui Syarat & Ketentuan",
            30, new Color(1, 1, 1, 0.55f), FontStyles.Normal);
        PlaceBottom(footer.rectTransform, 60, new Vector2(900, 60));

        // ---------- EventSystem ----------
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        // ---------- Simpan scene ----------
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/WelcomeScene.unity");
        AssetDatabase.Refresh();
        Selection.activeGameObject = canvasGO;
        Debug.Log("Welcome scene dibuat di Assets/Scenes/WelcomeScene.unity");
    }

    // ================= Helpers =================

    static Image MakeImage(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, Color color, FontStyles style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    static Button MakeButton(string name, Transform parent, Sprite sprite, Color bg, string label, Color labelColor, bool bold)
    {
        var img = MakeImage(name, parent, sprite, bg);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 0.25f;
        img.raycastTarget = true;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var t = MakeText("Label", img.transform, label, 48, labelColor, bold ? FontStyles.Bold : FontStyles.Normal);
        Stretch(t.rectTransform);
        return btn;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void PlaceBottom(RectTransform rt, float yFromBottom, Vector2 size)
    {
        Place(rt, new Vector2(0.5f, 0f), new Vector2(0, yFromBottom), size);
    }

    static Sprite CreateGradientSprite()
    {
        const string dir = "Assets/UI/Generated";
        const string path = dir + "/WelcomeGradient.png";
        Directory.CreateDirectory(dir);

        const int h = 256;
        var tex = new Texture2D(4, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            var c = Color.Lerp(BottomColor, TopColor, y / (float)(h - 1));
            for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out var c);
        return c;
    }
}
