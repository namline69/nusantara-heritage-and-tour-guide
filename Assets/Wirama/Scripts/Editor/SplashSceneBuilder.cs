using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wirama.Splash;

namespace Wirama.SplashEditor
{
    /// <summary>
    /// Tools > Wirama > 1. Build Splash Scene
    /// Builds Splash.unity + Home.unity (placeholder), wires every reference, sets portrait orientation and Build Settings.
    /// Safe to run again: it rebuilds both scenes from the data in WiramaCatalog.
    /// </summary>
    public static class SplashSceneBuilder
    {
        const string Root = "Assets/Wirama";
        const string Sprites = Root + "/Art/Sprites";
        const string Audio = Root + "/Audio";
        const string SceneDir = Root + "/Scenes";
        const string SplashPath = SceneDir + "/Splash.unity";
        const string HomePath = SceneDir + "/Home.unity";

        public const string AppName = "Wirama Nusantara";
        const string Tagline = "Jelajahi Budaya & Pesona Indonesia";

        static readonly Vector2 RefRes = new Vector2(1284f, 2778f);   // iPhone 13 Pro Max

        // Palette A "Tropis Laut"
        static readonly Color Bg     = Hex("0B3D4F");
        static readonly Color Deep   = Hex("072632");
        static readonly Color Panel  = Hex("0F4A5E");
        static readonly Color Gold   = Hex("F2B134");
        static readonly Color Tosca  = Hex("2EC4B6");
        static readonly Color Cream  = Hex("FFF8E7");

        // popup / layout numbers shared between builder and controller
        const float PopupBottom = 340f, PopupHeight = 460f, ContainerBottom = 330f, ContainerTop = 830f;

        // ------------------------------------------------------------------ menu

        [MenuItem("Tools/Wirama/1. Build Splash Scene")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            if (TMP_Settings.instance == null || TMP_Settings.defaultFontAsset == null)
            {
                EditorUtility.DisplayDialog("TextMeshPro belum siap",
                    "Import dulu TMP Essentials:\nWindow > TextMeshPro > Import TMP Essential Resources\n\nLalu jalankan menu ini lagi.", "OK");
                return;
            }

            try
            {
                ReimportArt();
                Directory.CreateDirectory(SceneDir);
                AssetDatabase.Refresh();

                BuildHomeScene();
                BuildSplashScene();
                ConfigureProject();

                Debug.Log("[Wirama] Selesai. Buka Assets/Wirama/Scenes/Splash.unity lalu tekan Play (Game view 1284x2778).");
                EditorUtility.DisplayDialog("Wirama Nusantara", "Scene Splash & Home berhasil dibuat.\nTekan Play di scene Splash.", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[Wirama] Gagal membangun scene: " + e);
                EditorUtility.DisplayDialog("Gagal", e.Message, "OK");
            }
        }

        [MenuItem("Tools/Wirama/2. Reimport Art")]
        public static void ReimportArt()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Wirama/Art" });
            foreach (string g in guids)
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();
        }

        // ------------------------------------------------------------------ project settings

        static void ConfigureProject()
        {
            PlayerSettings.productName = AppName;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            var list = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(SplashPath, true),
                new EditorBuildSettingsScene(HomePath, true),
            };
            foreach (var s in EditorBuildSettings.scenes)
                if (s.path != SplashPath && s.path != HomePath) list.Add(s);
            EditorBuildSettings.scenes = list.ToArray();
        }

        // ------------------------------------------------------------------ Home placeholder

        static void BuildHomeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera();
            AddEventSystem();

            GameObject canvasGo; RectTransform canvasRt;
            MakeCanvas("HomeCanvas", out canvasGo, out canvasRt);

            var bg = NewImage("Background", canvasRt, Spr("bg_gradient"), Color.white);
            Stretch(bg.rectTransform, 0, 0, 0, 0);

            var content = NewRect("Content", canvasRt);
            Stretch(content, 0, 0, 0, 0);
            var group = content.gameObject.AddComponent<CanvasGroup>();

            var logo = NewImage("Logo", content, Spr("logo"), Color.white);
            Place(logo.rectTransform, new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.62f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(360, 360));

            var title = NewText("Title", content, "HOME", 110, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(1100, 140));
            title.characterSpacing = 10;

            var sub = NewText("Subtitle", content, "Placeholder - ganti dengan scene Home tim.\nSplash berhasil memindahkan ke sini setelah tombol Mulai.", 40, Cream,
                FontStyles.Normal, TextAlignmentOptions.Center);
            Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1100, 160));

            Image face; Button back;
            MakeButton("BackButton", content, "Kembali ke Splash", 52, new Vector2(0.5f, 0.5f), new Vector2(0, -420), new Vector2(760, 150), out face, out back);

            var hp = canvasGo.AddComponent<HomePlaceholder>();
            hp.backButton = back;
            hp.content = group;
            hp.splashSceneName = "Splash";

            EditorSceneManager.SaveScene(scene, HomePath);
        }

        // ------------------------------------------------------------------ Splash

        static void BuildSplashScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera();
            AddEventSystem();

            GameObject canvasGo; RectTransform canvasRt;
            MakeCanvas("SplashCanvas", out canvasGo, out canvasRt);
            var ctrl = canvasGo.AddComponent<SplashController>();
            var audio = canvasGo.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            ctrl.sfx = audio;
            ctrl.gong = AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "/gong.wav");
            ctrl.ting = AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "/ting.wav");
            ctrl.click = AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "/click.wav");
            ctrl.homeSceneName = "Home";

            // ---- background layers (full screen, ignore the safe area)
            var bg = NewImage("Background", canvasRt, Spr("bg_gradient"), Color.white);
            Stretch(bg.rectTransform, 0, 0, 0, 0);

            var kawung = NewImage("Kawung", canvasRt, Spr("kawung_tile"), new Color(Gold.r, Gold.g, Gold.b, 0.07f), false, Image.Type.Tiled);
            Stretch(kawung.rectTransform, -256, -256, -256, -256);
            ctrl.kawung = kawung.rectTransform;

            var glow = NewImage("OceanGlow", canvasRt, Spr("ocean_glow"), new Color(Tosca.r, Tosca.g, Tosca.b, 0.38f));
            Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 60), new Vector2(1800, 1800));
            ctrl.glow = glow.rectTransform;

            // ---- safe area root
            var safe = NewRect("SafeArea", canvasRt);
            Stretch(safe, 0, 0, 0, 0);
            safe.gameObject.AddComponent<SafeAreaFitter>();

            // ---- header: logo, name, tagline
            var logo = NewImage("Logo", safe, Spr("logo"), Color.white);
            Place(logo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(540, 540));
            ctrl.logo = logo.rectTransform;

            var title = NewText("Title", safe, AppName, 120, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
            Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -560), new Vector2(1220, 150));
            title.characterSpacing = 4;
            title.enableAutoSizing = true; title.fontSizeMin = 60; title.fontSizeMax = 120;
            ctrl.title = title.rectTransform;

            var tag = NewText("Tagline", safe, Tagline, 44, Cream, FontStyles.Normal, TextAlignmentOptions.Center);
            Place(tag.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -720), new Vector2(1180, 70));
            tag.characterSpacing = 2;
            ctrl.tagline = tag.rectTransform;

            // ---- map container (clips the zoomed map)
            var container = NewRect("MapContainer", safe);
            container.anchorMin = Vector2.zero; container.anchorMax = Vector2.one;
            container.offsetMin = new Vector2(0, ContainerBottom);
            container.offsetMax = new Vector2(0, -ContainerTop);
            var mask = container.gameObject.AddComponent<RectMask2D>();
            mask.softness = new Vector2Int(90, 240);   // feather the edges instead of a hard rectangular cut
            ctrl.mapContainer = container;

            var ocean = NewImage("OceanTap", container, null, new Color(0, 0, 0, 0), true);
            Stretch(ocean.rectTransform, 0, 0, 0, 0);
            ocean.gameObject.AddComponent<OceanTapArea>();

            var mapRoot = NewRect("MapRoot", container);
            Place(mapRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(MapMath.MapWidth, MapMath.MapHeight));
            ctrl.mapRoot = mapRoot;

            foreach (IslandInfo island in WiramaCatalog.Islands)
            {
                Sprite sp = Load<Sprite>(island.SpriteAssetPath);
                CheckReadable(sp);
                var img = NewImage("Island_" + island.id, mapRoot, sp, Color.white, true);
                img.alphaHitTestMinimumThreshold = 0.1f;
                Vector2 center = MapMath.SvgToLocal(new Vector2((island.x0 + island.x1) * 0.5f, (island.y0 + island.y1) * 0.5f));
                Place(img.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), center,
                    new Vector2((island.x1 - island.x0) * MapMath.Scale, (island.y1 - island.y0) * MapMath.Scale));
                var layer = img.gameObject.AddComponent<IslandLayer>();
                layer.islandId = island.id;
            }

            var pins = NewRect("Pins", mapRoot);
            Place(pins, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(MapMath.MapWidth, MapMath.MapHeight));

            foreach (IslandInfo island in WiramaCatalog.Islands)
            {
                foreach (DestinationInfo d in island.destinations)
                {
                    var root = NewImage("Pin_" + d.id, pins, null, new Color(1, 1, 1, 0), true);   // touch target
                    Place(root.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f),
                        MapMath.LatLonToLocal(d.lat, d.lon), new Vector2(100, 112));

                    var ring = NewImage("Pulse", root.rectTransform, Spr("ring"), new Color(Gold.r, Gold.g, Gold.b, 0f));
                    Place(ring.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(120, 120));

                    var vis = NewImage("Visual", root.rectTransform, Spr("pin"), Color.white);
                    Place(vis.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(56, 75));

                    var pin = root.gameObject.AddComponent<MapPin>();
                    pin.destinationId = d.id;
                    pin.islandId = island.id;
                    pin.visual = vis.rectTransform;
                    pin.pulseRing = ring;
                }
            }

            // island label + back button sit above the container so the soft edge never fades them
            var label = NewText("IslandLabel", safe, "", 64, Gold, FontStyles.Bold, TextAlignmentOptions.Center);
            Place(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -(ContainerTop + 20f)), new Vector2(800, 90));
            label.characterSpacing = 4;
            ctrl.islandLabel = label;

            var backRt = NewRect("BackButton", safe);
            Place(backRt, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40, -(ContainerTop + 10f)), new Vector2(110, 110));
            var backImg = backRt.gameObject.AddComponent<Image>();
            backImg.sprite = Spr("rounded"); backImg.type = Image.Type.Sliced;
            backImg.color = new Color(1, 1, 1, 0.14f);
            var backBtn = backRt.gameObject.AddComponent<Button>();
            StyleButton(backBtn, backImg);
            var backIcon = NewImage("Icon", backRt, Spr("icon_back"), Cream);
            Place(backIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
            ctrl.backButton = backBtn;

            // ---- hint (sits in the slot the popup card uses)
            var hint = NewText("Hint", safe, "", 40, new Color(Cream.r, Cream.g, Cream.b, 0.8f), FontStyles.Italic, TextAlignmentOptions.Center);
            Place(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, PopupBottom + 150), new Vector2(1100, 140));
            ctrl.hint = hint;

            // ---- popup card
            BuildPopup(safe, ctrl);

            // ---- Mulai button
            var startRoot = NewRect("StartButton", safe);
            Place(startRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(780, 150));
            var shadow = NewImage("Shadow", startRoot, Spr("rounded"), new Color(0, 0, 0, 0.3f), false, Image.Type.Sliced);
            Place(shadow.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0, -10), Vector2.zero);
            var face = NewImage("Face", startRoot, Spr("rounded"), Gold, true, Image.Type.Sliced);
            Stretch(face.rectTransform, 0, 0, 0, 0);
            var startBtn = face.gameObject.AddComponent<Button>();
            StyleButton(startBtn, face);
            var startLabel = NewText("Label", face.rectTransform, "Mulai", 70, Deep, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(startLabel.rectTransform, 0, 0, 0, 0);
            startLabel.characterSpacing = 12;
            ctrl.startRoot = startRoot;
            ctrl.startButton = startBtn;

            // ---- footer
            var footer = NewText("Footer", safe, "Nusantara Heritage & Tour Guide  |  Kelompok 2", 28,
                new Color(Cream.r, Cream.g, Cream.b, 0.5f), FontStyles.Normal, TextAlignmentOptions.Center);
            Place(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(1180, 50));

            // ---- fade overlay (on top of everything)
            var fadeImg = NewImage("Fade", canvasRt, null, Deep, true);
            Stretch(fadeImg.rectTransform, 0, 0, 0, 0);
            var fadeGroup = fadeImg.gameObject.AddComponent<CanvasGroup>();
            fadeGroup.alpha = 1f;
            ctrl.fade = fadeGroup;

            ctrl.popupReserve = PopupBottom + PopupHeight - ContainerBottom + 30f;

            EditorSceneManager.SaveScene(scene, SplashPath);
            Selection.activeGameObject = canvasGo;
        }

        static void BuildPopup(RectTransform parent, SplashController ctrl)
        {
            var popupRt = NewRect("Popup", parent);
            Place(popupRt, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, PopupBottom), new Vector2(1160, PopupHeight));
            var border = popupRt.gameObject.AddComponent<Image>();
            border.sprite = Spr("rounded"); border.type = Image.Type.Sliced;
            border.color = new Color(Gold.r, Gold.g, Gold.b, 0.9f);
            border.raycastTarget = true;   // the card swallows taps so the sea behind does not react

            var inner = NewImage("Inner", popupRt, Spr("rounded"), new Color(Panel.r, Panel.g, Panel.b, 0.98f), false, Image.Type.Sliced);
            Stretch(inner.rectTransform, 4, 4, 4, 4);

            var region = NewText("Region", popupRt, "REGION", 32, Tosca, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            TopStretch(region.rectTransform, 56, 170, 44, 44);
            region.characterSpacing = 6;

            var title = NewText("Title", popupRt, "Nama Destinasi", 64, Gold, FontStyles.Bold, TextAlignmentOptions.TopLeft);
            TopStretch(title.rectTransform, 56, 170, 92, 90);
            title.enableAutoSizing = true; title.fontSizeMin = 38; title.fontSizeMax = 64;

            var desc = NewText("Description", popupRt, "Deskripsi", 38, Cream, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            TopStretch(desc.rectTransform, 56, 56, 192, 240);
            desc.enableAutoSizing = true; desc.fontSizeMin = 28; desc.fontSizeMax = 40;
            desc.lineSpacing = 6;

            var closeRt = NewRect("CloseButton", popupRt);
            Place(closeRt, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-28, -28), new Vector2(110, 110));
            var closeImg = closeRt.gameObject.AddComponent<Image>();
            closeImg.sprite = Spr("rounded"); closeImg.type = Image.Type.Sliced;
            closeImg.color = new Color(1, 1, 1, 0.14f);
            var closeBtn = closeRt.gameObject.AddComponent<Button>();
            StyleButton(closeBtn, closeImg);
            var closeIcon = NewImage("Icon", closeRt, Spr("icon_close"), Cream);
            Place(closeIcon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52, 52));

            var popup = popupRt.gameObject.AddComponent<DestinationPopup>();
            popup.regionText = region;
            popup.titleText = title;
            popup.descriptionText = desc;
            popup.closeButton = closeBtn;
            ctrl.popup = popup;
        }

        // ------------------------------------------------------------------ builders / helpers

        static void MakeCanvas(string name, out GameObject go, out RectTransform rt)
        {
            go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefRes;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            rt = (RectTransform)go.transform;
        }

        static void AddCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Bg;
            cam.cullingMask = 0;
            go.AddComponent<AudioListener>();
        }

        static void AddEventSystem()
        {
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            var t = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (t != null)
            {
                var module = go.AddComponent(t);
                var assign = t.GetMethod("AssignDefaultActions");
                if (assign != null) assign.Invoke(module, null);
                return;
            }
#endif
            go.AddComponent<StandaloneInputModule>();
        }

        static void CheckReadable(Sprite sp)
        {
            if (sp != null && sp.texture != null && !sp.texture.isReadable)
                Debug.LogWarning("[Wirama] Texture " + sp.texture.name + " tidak readable. Jalankan Tools > Wirama > 2. Reimport Art.");
        }

        static T Load<T>(string path) where T : Object
        {
            var a = AssetDatabase.LoadAssetAtPath<T>(path);
            if (a == null) throw new System.Exception("Aset tidak ditemukan: " + path + "\nPastikan folder Assets/Wirama utuh, lalu jalankan Tools > Wirama > 2. Reimport Art.");
            return a;
        }

        static Sprite Spr(string name) { return Load<Sprite>(Sprites + "/" + name + ".png"); }

        static Color Hex(string hex)
        {
            Color c;
            ColorUtility.TryParseHtmlString("#" + hex, out c);
            return c;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static Image NewImage(string name, Transform parent, Sprite sprite, Color color, bool raycast = false, Image.Type type = Image.Type.Simple)
        {
            var rt = NewRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = type;
            img.raycastTarget = raycast;
            return img;
        }

        static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color, FontStyles style, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.alignment = align;
            t.raycastTarget = false;
            return t;
        }

        static void Place(RectTransform r, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            r.anchorMin = aMin; r.anchorMax = aMax; r.pivot = pivot;
            r.anchoredPosition = pos; r.sizeDelta = size;
        }

        static void Stretch(RectTransform r, float left, float bottom, float right, float top)
        {
            r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom);
            r.offsetMax = new Vector2(-right, -top);
        }

        /// <summary>Stretch horizontally, pin to the top with a fixed height.</summary>
        static void TopStretch(RectTransform r, float left, float right, float top, float height)
        {
            r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.offsetMin = new Vector2(left, -(top + height));
            r.offsetMax = new Vector2(-right, -top);
        }

        static void StyleButton(Button b, Graphic target)
        {
            b.targetGraphic = target;
            b.transition = Selectable.Transition.None;
            var nav = b.navigation; nav.mode = Navigation.Mode.None; b.navigation = nav;
        }

        static void MakeButton(string name, Transform parent, string label, float fontSize, Vector2 anchor, Vector2 pos, Vector2 size,
            out Image face, out Button button)
        {
            face = NewImage(name, parent, Spr("rounded"), Gold, true, Image.Type.Sliced);
            Place(face.rectTransform, anchor, anchor, new Vector2(0.5f, 0.5f), pos, size);
            button = face.gameObject.AddComponent<Button>();
            StyleButton(button, face);
            var t = NewText("Label", face.rectTransform, label, fontSize, Deep, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(t.rectTransform, 0, 0, 0, 0);
        }
    }
}
