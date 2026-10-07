using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NusantaraHeritage.Splash.EditorTools
{
    /// <summary>
    /// Menu: Nusantara > Build Splash Scene
    /// Membuat scene Splash lengkap (Canvas, layout, komponen, referensi) + scene Home placeholder
    /// jika belum ada, lalu mendaftarkannya ke Build Settings.
    /// </summary>
    public static class SplashSceneBuilder
    {
        private const string Art = "Assets/NusantaraSplash/Art/";
        private const string SplashPath = "Assets/Scenes/Splash.unity";
        private const string HomePath = "Assets/Scenes/Home.unity";

        // Palet: Indigo (tenang), Emas lembut (aksen), Krem (teks)
        private static readonly Color Indigo = new Color32(0x1C, 0x2B, 0x4A, 255);
        private static readonly Color IndigoDeep = new Color32(0x0B, 0x14, 0x26, 255);
        private static readonly Color Gold = new Color32(0xE2, 0xB6, 0x59, 255);
        private static readonly Color Cream = new Color32(0xF6, 0xEB, 0xD9, 255);

        [MenuItem("Nusantara/Build Splash Scene")]
        public static void Build()
        {
            if (TMP_Settings.instance == null)
            {
                EditorUtility.DisplayDialog("TextMeshPro belum siap",
                    "Import dulu: Window > TextMeshPro > Import TMP Essential Resources, lalu jalankan menu ini lagi.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            AssetDatabase.Refresh();
            ConfigureImports();
            if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");

            EnsureHomePlaceholder();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            var controller = BuildCanvas();
            CreateEventSystem();
            EditorSceneManager.SaveScene(scene, SplashPath);

            RegisterBuildScenes();
            Selection.activeGameObject = controller.gameObject;
            Debug.Log("[Splash] Selesai. Atur Game view ke iPhone 13 Pro Max (Simulator) lalu tekan Play.");
        }

        // ------------------------------------------------------------------ Import settings
        private static void ConfigureImports()
        {
            SetupTexture("batik_kawung.png", isSprite: false, repeat: true, mips: true);
            SetupTexture("logo_nusantara.png", true, false, false);
            SetupTexture("glow_soft.png", true, false, false);
            SetupTexture("vignette.png", true, false, false);
            SetupTexture("bg_gradient.png", true, false, false);
            SetupTexture("white_px.png", true, false, false);
            SetupTexture("ui_rounded.png", true, false, false, new Vector4(63, 63, 63, 63));
        }

        private static void SetupTexture(string file, bool isSprite, bool repeat, bool mips, Vector4? border = null)
        {
            string path = Art + file;
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) { Debug.LogError("[Splash] Aset tidak ditemukan: " + path); return; }

            ti.textureType = isSprite ? TextureImporterType.Sprite : TextureImporterType.Default;
            if (isSprite) ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = mips;
            ti.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            ti.filterMode = FilterMode.Bilinear;
            ti.npotScale = TextureImporterNPOTScale.None;
            if (border.HasValue) ti.spriteBorder = border.Value;
            ti.SaveAndReimport();
        }

        // ------------------------------------------------------------------ Scene objects
        private static void CreateCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = IndigoDeep;
            go.AddComponent<AudioListener>();
        }

        private static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
            // Unity 6 default memakai Input System package. Pakai modul UI yang sesuai.
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null) es.AddComponent(t);
            else es.AddComponent<StandaloneInputModule>();
        }

        private static SplashController BuildCanvas()
        {
            var canvasGo = new GameObject("SplashCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1284f, 2778f);   // iPhone 13 Pro Max
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.transform;
            var ctrl = canvasGo.AddComponent<SplashController>();
            var sfx = canvasGo.AddComponent<AudioSource>();
            sfx.playOnAwake = false;

            // 1. Background gradient (full-bleed)
            var bg = NewRect("Background", root); Stretch(bg);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.sprite = Load<Sprite>("bg_gradient.png"); bgImg.raycastTarget = false;

            // 2. Pola batik (full-bleed, tile merata)
            var batik = NewRect("BatikLayer", root); Stretch(batik);
            var raw = batik.gameObject.AddComponent<RawImage>();
            raw.texture = Load<Texture2D>("batik_kawung.png"); raw.raycastTarget = false;
            var batikCg = batik.gameObject.AddComponent<CanvasGroup>();
            batik.gameObject.AddComponent<BatikScroller>();

            // 3. Vignette
            var vig = NewRect("Vignette", root); Stretch(vig);
            var vigImg = vig.gameObject.AddComponent<Image>();
            vigImg.sprite = Load<Sprite>("vignette.png"); vigImg.raycastTarget = false;

            // 4. Safe Area (semua konten penting di sini)
            var safe = NewRect("SafeArea", root); Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            // --- Logo area
            var logoArea = NewRect("LogoArea", safe);
            Anchor(logoArea, new Vector2(0.5f, 0.5f), new Vector2(0, 380), new Vector2(1000, 1000));

            var glow = NewRect("Glow", logoArea);
            Anchor(glow, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 1000));
            var glowImg = glow.gameObject.AddComponent<Image>();
            glowImg.sprite = Load<Sprite>("glow_soft.png"); glowImg.raycastTarget = false;
            var glowCg = glow.gameObject.AddComponent<CanvasGroup>();

            var logo = NewRect("Logo", logoArea);
            Anchor(logo, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560, 560));
            var logoImg = logo.gameObject.AddComponent<Image>();
            logoImg.sprite = Load<Sprite>("logo_nusantara.png"); logoImg.raycastTarget = false;
            logoImg.preserveAspect = true;
            var logoCg = logo.gameObject.AddComponent<CanvasGroup>();

            // --- Judul (Auto-Layout vertikal)
            var titleGroup = NewRect("TitleGroup", safe);
            Anchor(titleGroup, new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(1150, 460));
            var titleCg = titleGroup.gameObject.AddComponent<CanvasGroup>();
            var vlg = titleGroup.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            vlg.spacing = 14;

            NewText("Title", titleGroup, "NUSANTARA", 128, Cream, FontStyles.Bold, 8f);
            NewText("Subtitle", titleGroup, "HERITAGE & TOUR GUIDE", 46, Gold, FontStyles.Normal, 14f);
            var spacer = NewRect("Spacer", titleGroup);
            spacer.gameObject.AddComponent<LayoutElement>().minHeight = 26;
            var tagline = NewText("Tagline", titleGroup, "Jelajahi Budaya, Temukan Indonesia", 44,
                new Color(Cream.r, Cream.g, Cream.b, 0.88f), FontStyles.Italic, 1f);
            var taglineCg = tagline.gameObject.AddComponent<CanvasGroup>();

            // --- Loading
            var loading = NewRect("LoadingGroup", safe);
            Anchor(loading, new Vector2(0.5f, 0f), new Vector2(0, 380), new Vector2(760, 140));
            var loadingCg = loading.gameObject.AddComponent<CanvasGroup>();

            var barBg = NewRect("BarBackground", loading);
            Anchor(barBg, new Vector2(0.5f, 1f), new Vector2(0, 0), new Vector2(760, 22));
            var barBgImg = barBg.gameObject.AddComponent<Image>();
            barBgImg.sprite = Load<Sprite>("ui_rounded.png"); barBgImg.type = Image.Type.Sliced;
            barBgImg.color = new Color(Cream.r, Cream.g, Cream.b, 0.18f); barBgImg.raycastTarget = false;
            barBg.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            var fill = NewRect("BarFill", barBg); Stretch(fill);
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.sprite = Load<Sprite>("white_px.png");
            fillImg.type = Image.Type.Filled; fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left; fillImg.fillAmount = 0f;
            fillImg.color = Gold; fillImg.raycastTarget = false;

            var label = NewText("LoadingLabel", loading, "Memuat 0%", 38,
                new Color(Cream.r, Cream.g, Cream.b, 0.85f), FontStyles.Normal, 2f);
            Anchor(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(760, 60));

            // --- Tombol Mulai
            var btn = NewRect("StartButton", safe);
            Anchor(btn, new Vector2(0.5f, 0f), new Vector2(0, 340), new Vector2(640, 150));
            var btnImg = btn.gameObject.AddComponent<Image>();
            btnImg.sprite = Load<Sprite>("ui_rounded.png"); btnImg.type = Image.Type.Sliced; btnImg.color = Gold;
            var button = btn.gameObject.AddComponent<Button>();
            button.targetGraphic = btnImg;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.5f);
            button.colors = colors;
            var shadow = btn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.35f); shadow.effectDistance = new Vector2(0, -8);
            var btnCg = btn.gameObject.AddComponent<CanvasGroup>();

            var btnLabel = NewText("Label", btn, "MULAI", 62, Indigo, FontStyles.Bold, 10f);
            Stretch(btnLabel.rectTransform);
            btnLabel.alignment = TextAlignmentOptions.Center;

            // --- Footer
            var footer = NewText("Footer", safe, "v1.0.0   •   Kelompok 2", 32,
                new Color(Cream.r, Cream.g, Cream.b, 0.6f), FontStyles.Normal, 2f);
            Anchor(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(1000, 50));
            var footerCg = footer.gameObject.AddComponent<CanvasGroup>();

            // 5. Fade overlay (paling atas)
            var ov = NewRect("FadeOverlay", root); Stretch(ov);
            var ovImg = ov.gameObject.AddComponent<Image>();
            ovImg.color = IndigoDeep; ovImg.raycastTarget = false;
            var ovCg = ov.gameObject.AddComponent<CanvasGroup>();

            // --- Wiring referensi ke SplashController
            var so = new SerializedObject(ctrl);
            Wire(so, "batikGroup", batikCg);
            Wire(so, "batikRect", batik);
            Wire(so, "logoRect", logo);
            Wire(so, "logoGroup", logoCg);
            Wire(so, "glowRect", glow);
            Wire(so, "glowGroup", glowCg);
            Wire(so, "titleRect", titleGroup);
            Wire(so, "titleGroup", titleCg);
            Wire(so, "taglineGroup", taglineCg);
            Wire(so, "footerGroup", footerCg);
            Wire(so, "loadingGroup", loadingCg);
            Wire(so, "loadingFill", fillImg);
            Wire(so, "loadingLabel", label);
            Wire(so, "startButton", button);
            Wire(so, "startGroup", btnCg);
            Wire(so, "startRect", btn);
            Wire(so, "fadeOverlay", ovCg);
            Wire(so, "sfxSource", sfx);
            so.ApplyModifiedPropertiesWithoutUndo();

            return ctrl;
        }

        // ------------------------------------------------------------------ Home placeholder
        private static void EnsureHomePlaceholder()
        {
            if (File.Exists(HomePath)) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var sc = canvasGo.GetComponent<CanvasScaler>();
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(1284f, 2778f);
            sc.matchWidthOrHeight = 0.5f;

            var t = NewText("PlaceholderText", canvasGo.transform, "HOME\n(placeholder)", 90, Cream, FontStyles.Bold, 4f);
            Stretch(t.rectTransform);
            CreateEventSystem();

            EditorSceneManager.SaveScene(scene, HomePath);
        }

        private static void RegisterBuildScenes()
        {
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == SplashPath);
            list.Insert(0, new EditorBuildSettingsScene(SplashPath, true));       // Splash selalu index 0
            if (!list.Exists(s => s.path == HomePath) && File.Exists(HomePath))
                list.Add(new EditorBuildSettingsScene(HomePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }

        // ------------------------------------------------------------------ Helpers
        private static T Load<T>(string file) where T : Object
        {
            var o = AssetDatabase.LoadAssetAtPath<T>(Art + file);
            if (o == null) Debug.LogError("[Splash] Gagal memuat " + Art + file);
            return o;
        }

        private static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform r, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = anchor;
            r.anchoredPosition = pos; r.sizeDelta = size;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string text, float size,
            Color color, FontStyles style, float spacing)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text; t.fontSize = size; t.color = color; t.fontStyle = style;
            t.characterSpacing = spacing;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.raycastTarget = false;
            return t;
        }

        private static void Wire(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Splash] Field tidak ditemukan: " + field); return; }
            p.objectReferenceValue = value;
        }
    }
}
