using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

public class DinoSpriteImportSettings : AssetPostprocessor
{
    private static readonly string[] TargetPrefixes =
    {
        "Assets/Resources/Art/External/FreeDinoSprite/",
        "Assets/Resources/Art/External/pdphotodotorg_barell_cactus-varalpha.png"
    };

    private void OnPreprocessTexture()
    {
        if (!IsTargetTexture(assetPath))
        {
            return;
        }

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Point;
    }

    private static bool IsTargetTexture(string path)
    {
        foreach (var prefix in TargetPrefixes)
        {
            if (path.StartsWith(prefix))
            {
                return true;
            }
        }

        return false;
    }

    [InitializeOnLoadMethod]
    private static void ReimportTargets()
    {
        var assetPaths = AssetDatabase.FindAssets("t:Texture2D", new[]
            {
                "Assets/Resources/Art/External/FreeDinoSprite",
                "Assets/Resources/Art/External"
            })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(IsTargetTexture)
            .Distinct()
            .ToArray();

        if (assetPaths.Length == 0)
        {
            return;
        }

        foreach (var assetPath in assetPaths)
        {
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        }

        Debug.Log($"[DinoSpriteImportSettings] Reimported {assetPaths.Length} target textures");
    }
}
