using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DinoSpriteLibrary
{
    private const float DefaultPixelsPerUnit = 250f;

    private static readonly Dictionary<string, Sprite[]> SequenceCache = new();
    private static readonly Dictionary<string, Sprite> SingleCache = new();
    private static readonly Dictionary<string, Sprite> TextureSpriteCache = new();

    public static Sprite[] GetSequence(string resourcePath)
    {
        if (SequenceCache.TryGetValue(resourcePath, out var cached))
        {
            return cached;
        }

        var sprites = Resources.LoadAll<Sprite>(resourcePath);
        var frames = sprites
            .Select(sprite => new { sprite, frame = ExtractFrameNumber(sprite.name) })
            .OrderBy(entry => entry.frame)
            .ThenBy(entry => entry.sprite.name)
            .Select(entry => entry.sprite)
            .Where(sprite => sprite != null)
            .ToArray();

        SequenceCache[resourcePath] = frames;
        return frames;
    }

    public static Sprite GetSingle(string resourcePath)
    {
        if (SingleCache.TryGetValue(resourcePath, out var cached))
        {
            return cached;
        }

        var sprite = Resources.Load<Sprite>(resourcePath);
        SingleCache[resourcePath] = sprite;
        return sprite;
    }

    public static Sprite GetAnySprite(string resourcePath)
    {
        var sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites != null && sprites.Length > 0)
        {
            return sprites[0];
        }

        Debug.LogWarning($"No sprites found at Resources/{resourcePath}");
        return CreateFallbackSprite();
    }

    public static Sprite GetTextureSprite(string resourcePath, float pixelsPerUnit = 16f)
    {
        var cacheKey = $"{resourcePath}|ppu={pixelsPerUnit:0.###}";
        if (TextureSpriteCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"No texture found at Resources/{resourcePath}");
            return CreateFallbackSprite();
        }

        var sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);

        TextureSpriteCache[cacheKey] = sprite;
        return sprite;
    }

    public static Sprite GetTextureSpriteSlice(string resourcePath, int y, int height, float pixelsPerUnit = 16f)
    {
        var cacheKey = $"{resourcePath}|y={y}|h={height}|ppu={pixelsPerUnit:0.###}";
        if (TextureSpriteCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"No texture found at Resources/{resourcePath}");
            return CreateFallbackSprite();
        }

        var rect = new Rect(0f, y, texture.width, height);
        var sprite = Sprite.Create(
            texture,
            rect,
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);

        TextureSpriteCache[cacheKey] = sprite;
        return sprite;
    }

    public static bool TryGetOpaqueBounds(Sprite sprite, out Bounds bounds, byte alphaThreshold = 16)
    {
        bounds = default;
        if (sprite == null || sprite.texture == null)
        {
            return false;
        }

        var rect = sprite.textureRect;
        var startX = Mathf.FloorToInt(rect.x);
        var startY = Mathf.FloorToInt(rect.y);
        var width = Mathf.FloorToInt(rect.width);
        var height = Mathf.FloorToInt(rect.height);
        if (width <= 0 || height <= 0)
        {
            return false;
        }

        Color32[] pixels;
        try
        {
            pixels = sprite.texture.GetPixels32();
        }
        catch
        {
            return false;
        }

        var textureWidth = sprite.texture.width;
        var minX = width;
        var minY = height;
        var maxX = -1;
        var maxY = -1;

        for (var y = 0; y < height; y++)
        {
            var textureY = startY + y;
            var rowOffset = textureY * textureWidth;
            for (var x = 0; x < width; x++)
            {
                var pixel = pixels[rowOffset + startX + x];
                if (pixel.a < alphaThreshold)
                {
                    continue;
                }

                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0 || maxY < 0)
        {
            return false;
        }

        var ppu = sprite.pixelsPerUnit;
        var opaqueWidth = maxX - minX + 1;
        var opaqueHeight = maxY - minY + 1;
        var centerX = ((minX + maxX + 1) * 0.5f - sprite.pivot.x) / ppu;
        var centerY = ((minY + maxY + 1) * 0.5f - sprite.pivot.y) / ppu;

        bounds = new Bounds(
            new Vector3(centerX, centerY, 0f),
            new Vector3(opaqueWidth / ppu, opaqueHeight / ppu, 0f));
        return true;
    }

    private static Sprite CreateFallbackSprite()
    {
        var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        var pixels = new Color32[32 * 32];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(70, 160, 90, 255);
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
    }

    private static int ExtractFrameNumber(string name)
    {
        var match = Regex.Match(name, @"\((\d+)\)$");
        return match.Success && int.TryParse(match.Groups[1].Value, out var value) ? value : int.MaxValue;
    }
}
