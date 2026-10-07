using UnityEditor;
using UnityEngine;

namespace Wirama.SplashEditor
{
    /// <summary>
    /// Imports every PNG under Assets/Wirama/Art as a clean UI sprite, so no manual import settings are needed.
    /// Island sprites stay readable + uncompressed because the map uses alpha hit-testing on them.
    /// </summary>
    public class WiramaTextureImporter : AssetPostprocessor
    {
        public const string ArtRoot = "Assets/Wirama/Art/";

        // bump to force a re-import of existing art after editing the rules below
        public override uint GetVersion() { return 3; }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;

            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            ti.filterMode = FilterMode.Bilinear;
            ti.maxTextureSize = 2048;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.isReadable = assetPath.Contains("/Islands/");
            ti.wrapMode = assetPath.EndsWith("kawung_tile.png") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;   // required for tiled + sliced images
            ti.SetTextureSettings(settings);

            if (assetPath.EndsWith("/rounded.png"))
                ti.spriteBorder = new Vector4(48f, 48f, 48f, 48f);
        }
    }
}
