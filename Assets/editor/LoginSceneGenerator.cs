// Simpan di: Assets/Editor/LoginSceneGenerator.cs
// Butuh: LoginController.cs (Assets/Scripts) + TextMeshPro Essentials
//
// Opsional (otomatis dipakai kalau ada), taruh di Assets/UI/Custom/:
//   hero.jpg / hero.png  -> foto panel kiri
//   logo.png, mail.png, lock.png, eye.png, eye_off.png, arrow.png, shield.png
//   google.png, apple.png, github.png  -> logo resmi tombol sosial
// Kalau tidak ada, script membuat ikon placeholder sendiri.
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

public static class LoginSceneGenerator
{
    // ====== TINGGAL UBAH DI SINI ======
    const string BrandName = "NEXORA";
    const string Copyright = "© 2024 Nexora. All rights reserved.";

    static readonly Color BgTop       = Hex("#151518");
    static readonly Color BgBottom    = Hex("#09090A");
    static readonly Color CardColor   = Hex("#18181B");
    static readonly Color CardBorder  = Hex("#2A2A2F");
    static readonly Color FieldColor  = Hex("#1D1D20");
    static readonly Color FieldBorder = Hex("#2E2E33");
    static readonly Color Accent      = Hex("#F0B87F");
    static readonly Color TextMain    = Hex("#F4F4F5");
    static readonly Color TextMuted   = Hex("#8C8C93");
    static readonly Color HintColor   = Hex("#6B6B72");
    static readonly Color ErrorColor  = Hex("#FF6B6B");
    // ==================================

    // Referensi 1080x1620 (rasio sama dengan desain 736x1104)
    static readonly Vector2 RefResolution = new Vector2(1080, 1620);

    const string CustomDir = "Assets/UI/Custom";
    const string GenDir = "Assets/UI/Generated";

    static Sprite rounded, circle;
    static readonly Dictionary<string, Sprite> iconCache = new Dictionary<string, Sprite>();

    [MenuItem("Tools/Generate Login Scene")]
    public static void Generate()
    {
        Directory.CreateDirectory(GenDir);
        Directory.CreateDirectory(CustomDir);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var cam = Camera.main;
        if (cam != null) { cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = BgBottom; }
        var light = GameObject.Find("Directional Light");
        if (light != null) Object.DestroyImmediate(light);

        rounded = MakeRoundedSprite();
        circle = Icon("circle");

        // ---------- Canvas ----------
        var canvasGO = new GameObject("LoginCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = RefResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        var ctrl = canvasGO.AddComponent<LoginController>();
        var canvasT = canvasGO.transform;

        // ---------- Background ----------
        var bg = Img("Background", canvasT, MakeGradient("bg_gradient", BgTop, BgBottom), Color.white);
        Stretch(bg.rectTransform);

        // ---------- Panel kiri ----------
        BuildLeftPanel(canvasT);

        // ---------- Card login ----------
        var card = Panel("LoginCard", canvasT, rounded, CardColor, CardBorder, 38f);
        Set(card, new Vector2(0, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(725, 0), new Vector2(597, 1224));

        // Logo bulat
        var logoRT = Panel("LogoCircle", card, circle, Hex("#1A1A1D"), Hex("#3B2F22"), 0f);
        Top(logoRT, 163, 117, 117);
        var logoIcon = Img("Logo", logoRT, Icon("logo"), Accent);
        Set(logoIcon.rectTransform, Mid, Mid, Vector2.zero, new Vector2(62, 62));
        logoIcon.preserveAspect = true;

        var title = Txt("Title", card, "Login", 46, TextMain, FontStyles.Bold);
        Top(title.rectTransform, 273, 490, 64);
        var subtitle = Txt("Subtitle", card, "Login to your account to continue", 23, TextMuted);
        Top(subtitle.rectTransform, 323, 520, 40);

        // Email
        var emailLabel = Txt("EmailLabel", card, "Email Address", 24, TextMain, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        TopLeft(emailLabel.rectTransform, 53, 405, 400, 34);
        var email = MakeField(card, "EmailField", 468, "you@example.com", Icon("mail"), 24, TMP_InputField.ContentType.EmailAddress);
        var emailError = Txt("EmailError", card, "", 20, ErrorColor, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        TopLeft(emailError.rectTransform, 53, 523, 490, 28);

        // Password
        var pwLabel = Txt("PasswordLabel", card, "Password", 24, TextMain, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        TopLeft(pwLabel.rectTransform, 53, 557, 400, 34);
        var pw = MakeField(card, "PasswordField", 619, "••••••••", Icon("lock"), 110, TMP_InputField.ContentType.Password);

        var toggle = Img("Btn_TogglePassword", pw.transform, null, new Color(0, 0, 0, 0));
        toggle.raycastTarget = true;
        Set(toggle.rectTransform, new Vector2(1, 0.5f), Mid, new Vector2(-52, 0), new Vector2(72, 72));
        var toggleBtn = toggle.gameObject.AddComponent<Button>();
        toggleBtn.transition = Selectable.Transition.None;
        toggleBtn.targetGraphic = toggle;
        var eyeIcon = Img("Icon", toggle.transform, Icon("eye_off"), TextMuted);
        Set(eyeIcon.rectTransform, Mid, Mid, Vector2.zero, new Vector2(38, 38));
        eyeIcon.preserveAspect = true;

        var pwError = Txt("PasswordError", card, "", 20, ErrorColor, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        TopLeft(pwError.rectTransform, 53, 692, 260, 28);

        var forgot = Txt("Btn_ForgotPassword", card, "Forgot Password?", 22, Accent, FontStyles.Normal, TextAlignmentOptions.MidlineRight);
        forgot.raycastTarget = true;
        TopRight(forgot.rectTransform, 53, 692, 230, 34);
        var forgotBtn = forgot.gameObject.AddComponent<Button>();
        forgotBtn.targetGraphic = forgot;

        // Tombol Login
        var login = Img("Btn_Login", card, rounded, Accent, 22f);
        login.raycastTarget = true;
        Top(login.rectTransform, 777, 490, 79);
        var glow = login.gameObject.AddComponent<Shadow>();
        glow.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.28f);
        glow.effectDistance = new Vector2(0, -12);
        var loginBtn = login.gameObject.AddComponent<Button>();
        loginBtn.targetGraphic = login;
        var loginLabel = Txt("Label", login.transform, "Login", 27, Hex("#1A1208"), FontStyles.Bold);
        Stretch(loginLabel.rectTransform);
        var arrow = Img("Arrow", login.transform, Icon("arrow"), Hex("#1A1208"));
        Set(arrow.rectTransform, new Vector2(1, 0.5f), Mid, new Vector2(-52, 0), new Vector2(36, 36));
        arrow.preserveAspect = true;

        // Status (error umum / sukses)
        var status = Txt("StatusText", card, "", 22, ErrorColor);
        Top(status.rectTransform, 849, 490, 34);

        // Divider
        var lineL = Img("Line_L", card, null, FieldBorder);
        Top(lineL.rectTransform, 895, 150, 2, -171);
        var lineR = Img("Line_R", card, null, FieldBorder);
        Top(lineR.rectTransform, 895, 150, 2, 171);
        var or = Txt("OrText", card, "or continue with", 22, TextMuted);
        Top(or.rectTransform, 895, 220, 34);

        // Social
        var google = MakeSocial(card, "Btn_Google", "google", -137);
        var apple = MakeSocial(card, "Btn_Apple", "apple", 0);
        var github = MakeSocial(card, "Btn_GitHub", "github", 137);

        // Sign up
        var signRow = Row("SignUpRow", card, 12);
        Set(signRow, new Vector2(0.5f, 1), Mid, new Vector2(0, -1118), new Vector2(400, 40));
        var noAcc = Txt("Text", signRow, "Don't have an account?", 24, TextMuted);
        var signUp = Txt("Btn_SignUp", signRow, "Sign up", 24, Accent, FontStyles.Bold);
        signUp.raycastTarget = true;
        var signUpBtn = signUp.gameObject.AddComponent<Button>();
        signUpBtn.targetGraphic = signUp;
        LayoutRebuilder.ForceRebuildLayoutImmediate(signRow);

        // Footer (di bawah card)
        var footer = Row("Footer", canvasT, 12);
        Set(footer, new Vector2(0, 0.5f), Mid, new Vector2(725, -673), new Vector2(400, 36));
        var shield = Img("ShieldIcon", footer, Icon("shield"), TextMuted);
        shield.preserveAspect = true;
        FixedSize(shield.gameObject, 28, 28);
        Txt("Text", footer, "Your data is secure with us", 20, TextMuted);
        LayoutRebuilder.ForceRebuildLayoutImmediate(footer);

        // ---------- Hubungkan ke controller ----------
        ctrl.emailInput = email;
        ctrl.passwordInput = pw;
        ctrl.emailError = emailError;
        ctrl.passwordError = pwError;
        ctrl.statusText = status;
        ctrl.loginButton = loginBtn;
        ctrl.passwordToggleButton = toggleBtn;
        ctrl.forgotPasswordButton = forgotBtn;
        ctrl.signUpButton = signUpBtn;
        ctrl.googleButton = google;
        ctrl.appleButton = apple;
        ctrl.githubButton = github;
        ctrl.loginLabel = loginLabel;
        ctrl.loginArrow = arrow.gameObject;
        ctrl.passwordToggleIcon = eyeIcon;
        ctrl.eyeSprite = Icon("eye");
        ctrl.eyeOffSprite = Icon("eye_off");

        // ---------- EventSystem ----------
        var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        es.AddComponent<InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif

        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/LoginScene.unity");
        AssetDatabase.Refresh();
        Selection.activeGameObject = canvasGO;
        Debug.Log("Login scene dibuat di Assets/Scenes/LoginScene.unity");
    }

    // ================= Panel kiri =================

    static void BuildLeftPanel(Transform canvasT)
    {
        var left = new GameObject("LeftPanel", typeof(RectTransform), typeof(RectMask2D));
        left.transform.SetParent(canvasT, false);
        var lrt = left.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0, 0);
        lrt.anchorMax = new Vector2(0, 1);
        lrt.pivot = new Vector2(0, 0.5f);
        lrt.anchoredPosition = Vector2.zero;
        lrt.sizeDelta = new Vector2(350, 0);

        // Foto (kalau ada) atau gradient hangat
        var heroSprite = LoadCustom("hero");
        var hero = Img("Hero", lrt, heroSprite != null ? heroSprite : MakeGradient("hero_fallback", Hex("#3A2E22"), Hex("#0E0D0C")), Color.white);
        Stretch(hero.rectTransform);
        if (heroSprite != null)
        {
            var arf = hero.gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            arf.aspectRatio = heroSprite.rect.width / heroSprite.rect.height;
        }

        var overlay = Img("Overlay", lrt, MakeGradient("overlay_gradient", new Color(0, 0, 0, 0.10f), new Color(0, 0, 0, 0.92f)), Color.white);
        Stretch(overlay.rectTransform);

        // Brand
        var brand = Row("Brand", lrt, 14);
        brand.anchorMin = brand.anchorMax = new Vector2(0, 1);
        brand.pivot = new Vector2(0, 0.5f);
        brand.anchoredPosition = new Vector2(48, -103);
        var bIcon = Img("Logo", brand, Icon("logo"), Color.white);
        bIcon.preserveAspect = true;
        FixedSize(bIcon.gameObject, 40, 40);
        var bText = Txt("Name", brand, BrandName, 30, Color.white, FontStyles.Normal);
        bText.characterSpacing = 8;
        LayoutRebuilder.ForceRebuildLayoutImmediate(brand);

        // Welcome Back
        var bar = Img("AccentBar", lrt, null, Accent);
        Set(bar.rectTransform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(48, 399), new Vector2(5, 106));
        var wb = Txt("WelcomeTitle", lrt, "Welcome\nBack", 46, Hex("#F4F4F5"), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Set(wb.rectTransform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(76, 399), new Vector2(270, 120));
        var body = Txt("WelcomeBody", lrt, "Glad to see you again.\nLet's continue where you left off.", 17, new Color(1, 1, 1, 0.65f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Set(body.rectTransform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(48, 298), new Vector2(300, 56));
        var copy = Txt("Copyright", lrt, Copyright, 16, new Color(1, 1, 1, 0.5f), FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Set(copy.rectTransform, new Vector2(0, 0), new Vector2(0, 0.5f), new Vector2(48, 85), new Vector2(300, 30));
    }

    // ================= Komponen UI =================

    static readonly Vector2 Mid = new Vector2(0.5f, 0.5f);

    static Image Img(string name, Transform parent, Sprite sprite, Color color, float radius = 0f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.raycastTarget = false;
        if (radius > 0f && sprite == rounded)
        {
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 32f / radius; // radius sudut ~ radius px
        }
        return img;
    }

    static TextMeshProUGUI Txt(string name, Transform parent, string text, float size, Color color,
        FontStyles style = FontStyles.Normal, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    // Panel dengan border: objek luar = warna border, anak "Fill" = warna isi
    static RectTransform Panel(string name, Transform parent, Sprite sprite, Color fill, Color border, float radius, float bw = 2f)
    {
        var outer = Img(name, parent, sprite, border, radius);
        var inner = Img("Fill", outer.transform, sprite, fill, radius > 0 ? Mathf.Max(radius - bw, 1f) : 0f);
        Stretch(inner.rectTransform, bw, bw, bw, bw);
        return outer.rectTransform;
    }

    static Image FillOf(RectTransform panel) { return panel.GetChild(0).GetComponent<Image>(); }

    static TMP_InputField MakeField(Transform parent, string name, float y, string hint, Sprite icon, float rightPad, TMP_InputField.ContentType type)
    {
        var rt = Panel(name, parent, rounded, FieldColor, FieldBorder, 20f);
        Top(rt, y, 490, 73);
        var outerImg = rt.GetComponent<Image>();
        outerImg.raycastTarget = true;

        var ic = Img("Icon", rt, icon, TextMuted);
        Set(ic.rectTransform, new Vector2(0, 0.5f), Mid, new Vector2(52, 0), new Vector2(36, 36));
        ic.preserveAspect = true;

        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(rt, false);
        var areaRT = area.GetComponent<RectTransform>();
        Stretch(areaRT, 96, rightPad, 6, 6);

        var ph = Txt("Placeholder", areaRT, hint, 24, HintColor, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Stretch(ph.rectTransform);
        var tx = Txt("Text", areaRT, "", 24, TextMain, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
        Stretch(tx.rectTransform);

        var input = rt.gameObject.AddComponent<TMP_InputField>();
        input.targetGraphic = outerImg;
        input.transition = Selectable.Transition.None;
        input.textViewport = areaRT;
        input.textComponent = tx;
        input.placeholder = ph;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.contentType = type;
        input.customCaretColor = true;
        input.caretColor = Accent;
        input.caretWidth = 3;
        input.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f);
        return input;
    }

    static Button MakeSocial(Transform parent, string name, string iconName, float x)
    {
        var rt = Panel(name, parent, circle, FieldColor, FieldBorder, 0f);
        Top(rt, 997, 88, 88, x);
        rt.GetComponent<Image>().raycastTarget = true;
        var btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = FillOf(rt);
        var ic = Img("Icon", rt, Icon(iconName), Color.white);
        Set(ic.rectTransform, Mid, Mid, Vector2.zero, new Vector2(44, 44));
        ic.preserveAspect = true;
        return btn;
    }

    static RectTransform Row(string name, Transform parent, float spacing)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        go.transform.SetParent(parent, false);
        var h = go.GetComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childAlignment = TextAnchor.MiddleCenter;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = false;
        var f = go.GetComponent<ContentSizeFitter>();
        f.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        f.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        return go.GetComponent<RectTransform>();
    }

    static void FixedSize(GameObject go, float w, float h)
    {
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        le.preferredHeight = h;
    }

    // ================= Rect helpers =================

    static void Set(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    // y = jarak dari sisi atas parent ke titik tengah elemen
    static void Top(RectTransform rt, float y, float w, float h, float x = 0)
    {
        Set(rt, new Vector2(0.5f, 1), Mid, new Vector2(x, -y), new Vector2(w, h));
    }

    static void TopLeft(RectTransform rt, float x, float y, float w, float h)
    {
        Set(rt, new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(x, -y), new Vector2(w, h));
    }

    static void TopRight(RectTransform rt, float x, float y, float w, float h)
    {
        Set(rt, new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(-x, -y), new Vector2(w, h));
    }

    // ================= Asset procedural =================

    static Sprite LoadCustom(string name)
    {
        foreach (var ext in new[] { "png", "jpg", "jpeg" })
        {
            string path = CustomDir + "/" + name + "." + ext;
            if (!File.Exists(path)) continue;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp != null && imp.textureType != TextureImporterType.Sprite)
            {
                imp.textureType = TextureImporterType.Sprite;
                imp.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        return null;
    }

    static Sprite Icon(string name)
    {
        var custom = LoadCustom(name);
        if (custom != null) return custom;
        Sprite s;
        if (iconCache.TryGetValue(name, out s) && s != null) return s;
        s = SaveSprite("icon_" + name, BuildIconTexture(name), Vector4.zero);
        iconCache[name] = s;
        return s;
    }

    static Sprite SaveSprite(string file, Texture2D tex, Vector4 border)
    {
        string path = GenDir + "/" + file + ".png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType = TextureImporterType.Sprite;
        imp.spriteImportMode = SpriteImportMode.Single;
        imp.mipmapEnabled = false;
        imp.alphaIsTransparency = true;
        imp.wrapMode = TextureWrapMode.Clamp;
        imp.textureCompression = TextureImporterCompression.Uncompressed;
        imp.spriteBorder = border;
        imp.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Sprite MakeGradient(string file, Color top, Color bottom)
    {
        const int h = 256;
        var tex = new Texture2D(4, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            var c = Color.Lerp(bottom, top, y / (float)(h - 1));
            for (int x = 0; x < 4; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply();
        return SaveSprite(file, tex, Vector4.zero);
    }

    static Sprite MakeRoundedSprite()
    {
        const int S = 96;
        const float R = 32f;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color[S * S];
        var c = new Vector2(S / 2f, S / 2f);
        float half = S / 2f - R;
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f) - c;
                var q = new Vector2(Mathf.Abs(p.x) - half, Mathf.Abs(p.y) - half);
                float sd = new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude
                           + Mathf.Min(Mathf.Max(q.x, q.y), 0) - R;
                px[y * S + x] = new Color(1, 1, 1, Mathf.Clamp01(0.5f - sd));
            }
        tex.SetPixels(px);
        tex.Apply();
        return SaveSprite("rounded", tex, new Vector4(R, R, R, R));
    }

    // ---------- Ikon garis procedural (128x128, putih) ----------

    static Vector2 V(float x, float y) { return new Vector2(x, y); }

    static Vector2[] Arc(Vector2 c, float rx, float ry, float a0, float a1, int n)
    {
        var pts = new Vector2[n + 1];
        for (int i = 0; i <= n; i++)
        {
            float a = Mathf.Deg2Rad * Mathf.Lerp(a0, a1, i / (float)n);
            pts[i] = c + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
        }
        return pts;
    }

    static Vector2[] Poly(Vector2 c, float r, int sides, float startDeg)
    {
        var pts = new Vector2[sides];
        for (int i = 0; i < sides; i++)
        {
            float a = Mathf.Deg2Rad * (startDeg + 360f * i / sides);
            pts[i] = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
        }
        return pts;
    }

    static float SegDist(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
        return Vector2.Distance(p, a + ab * t);
    }

    static Texture2D BuildIconTexture(string name)
    {
        var lines = new List<(Vector2[] pts, bool closed)>();
        var fills = new List<(Vector2 c, float r)>();
        float stroke = 7f;

        switch (name)
        {
            case "circle":
                fills.Add((V(64, 64), 63.5f));
                break;
            case "logo":
                stroke = 8f;
                lines.Add((Poly(V(64, 64), 54, 6, 30), true));
                lines.Add((new[] { V(44, 42), V(44, 86), V(84, 42), V(84, 86) }, false));
                break;
            case "mail":
                lines.Add((new[] { V(16, 34), V(112, 34), V(112, 94), V(16, 94) }, true));
                lines.Add((new[] { V(16, 94), V(64, 60), V(112, 94) }, false));
                break;
            case "lock":
                lines.Add((new[] { V(30, 20), V(98, 20), V(98, 70), V(30, 70) }, true));
                lines.Add((Arc(V(64, 70), 20, 28, 0, 180, 24), false));
                fills.Add((V(64, 45), 6f));
                break;
            case "eye":
                lines.Add((Arc(V(64, 64), 52, 30, 0, 360, 48), true));
                fills.Add((V(64, 64), 13f));
                break;
            case "eye_off":
                lines.Add((Arc(V(64, 64), 52, 30, 0, 360, 48), true));
                fills.Add((V(64, 64), 13f));
                lines.Add((new[] { V(24, 24), V(104, 104) }, false));
                break;
            case "arrow":
                lines.Add((new[] { V(16, 64), V(108, 64) }, false));
                lines.Add((new[] { V(70, 26), V(108, 64), V(70, 102) }, false));
                break;
            case "shield":
                lines.Add((new[] { V(64, 114), V(108, 98), V(108, 62), V(64, 14), V(20, 62), V(20, 98) }, true));
                lines.Add((new[] { V(44, 62), V(58, 48), V(84, 80) }, false));
                break;
            // Placeholder logo sosial -> ganti dengan logo resmi di Assets/UI/Custom/
            case "google":
                stroke = 13f;
                lines.Add((Arc(V(64, 64), 42, 42, 40, 330, 40), false));
                lines.Add((new[] { V(66, 64), V(106, 64) }, false));
                break;
            case "apple":
                stroke = 8f;
                fills.Add((V(50, 56), 30f));
                fills.Add((V(78, 56), 30f));
                fills.Add((V(64, 44), 28f));
                lines.Add((new[] { V(64, 88), V(70, 104), V(84, 112) }, false));
                break;
            case "github":
                stroke = 8f;
                lines.Add((Arc(V(64, 64), 50, 50, 0, 360, 48), true));
                fills.Add((V(64, 62), 24f));
                break;
        }
        return DrawIcon(lines, fills, stroke);
    }

    static Texture2D DrawIcon(List<(Vector2[] pts, bool closed)> lines, List<(Vector2 c, float r)> fills, float stroke)
    {
        const int S = 128;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color[S * S];
        float half = stroke * 0.5f;
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = 1000f;
                foreach (var l in lines)
                {
                    int n = l.pts.Length;
                    int count = l.closed ? n : n - 1;
                    for (int i = 0; i < count; i++)
                        d = Mathf.Min(d, SegDist(p, l.pts[i], l.pts[(i + 1) % n]));
                }
                float a = Mathf.Clamp01(half + 0.5f - d);
                foreach (var f in fills)
                    a = Mathf.Max(a, Mathf.Clamp01(f.r + 0.5f - Vector2.Distance(p, f.c)));
                px[y * S + x] = new Color(1, 1, 1, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    static Color Hex(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
