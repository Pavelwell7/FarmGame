using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Создаёт временные картинки персонажа (цветные прямоугольники с белой меткой стороны взгляда)
/// и готовый PlayerSpriteSet. Нужно только чтобы проверить систему, пока нет настоящих рисунков.
/// Меню: Farm -> Generate Placeholder Player Sprites.
/// Файл должен лежать в папке Assets/Editor.
/// </summary>
public static class PlaceholderSpriteGenerator
{
    private const int Width = 16;
    private const int Height = 24;
    private const int PixelsPerUnit = 32; // 16x24 пикселей = 0.5 x 0.75 клетки

    private const string Folder = "Assets/PlaceholderPlayerSprites";

    [MenuItem("Farm/Generate Placeholder Player Sprites")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Folder))
            AssetDatabase.CreateFolder("Assets", "PlaceholderPlayerSprites");

        var set = ScriptableObject.CreateInstance<PlayerSpriteSet>();
        set.emptyHands = MakeSet("EmptyHands", new Color(0.75f, 0.75f, 0.75f));
        set.hoe = MakeSet("Hoe", new Color(0.55f, 0.35f, 0.20f));
        set.seeds = MakeSet("Seeds", new Color(0.45f, 0.70f, 0.25f));
        set.wateringCan = MakeSet("WateringCan", new Color(0.25f, 0.50f, 0.85f));
        set.shovel = MakeSet("Shovel", new Color(0.45f, 0.45f, 0.50f));
        set.carrot = MakeSet("Carrot", new Color(0.95f, 0.55f, 0.10f));

        string setPath = Folder + "/PlayerSpriteSet.asset";
        AssetDatabase.DeleteAsset(setPath);
        AssetDatabase.CreateAsset(set, setPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = set;
        EditorGUIUtility.PingObject(set);
        Debug.Log("Временные картинки созданы: " + Folder);
    }

    private static PlayerSpriteSet.DirectionalSprites MakeSet(string name, Color bodyColor)
    {
        return new PlayerSpriteSet.DirectionalSprites
        {
            up = MakeSprite(name + "_Up", bodyColor, Direction4.Up),
            down = MakeSprite(name + "_Down", bodyColor, Direction4.Down),
            left = MakeSprite(name + "_Left", bodyColor, Direction4.Left),
            right = MakeSprite(name + "_Right", bodyColor, Direction4.Right)
        };
    }

    private static Sprite MakeSprite(string fileName, Color bodyColor, Direction4 facing)
    {
        string assetPath = Folder + "/" + fileName + ".png";

        var pixels = new Color32[Width * Height];
        Color32 body = bodyColor;
        Color32 border = Color.Lerp(bodyColor, Color.black, 0.5f);
        Color32 marker = Color.white;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                bool edge = x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
                pixels[y * Width + x] = edge ? border : body;
            }
        }

        // Белая метка показывает, куда смотрит персонаж (y = 0 это низ картинки).
        const int m = 5;
        int mx = 0, my = 0;
        switch (facing)
        {
            case Direction4.Up:    mx = (Width - m) / 2; my = Height - m - 2; break;
            case Direction4.Down:  mx = (Width - m) / 2; my = 2; break;
            case Direction4.Left:  mx = 2;               my = (Height - m) / 2; break;
            case Direction4.Right: mx = Width - m - 2;   my = (Height - m) / 2; break;
        }

        for (int y = my; y < my + m; y++)
            for (int x = mx; x < mx + m; x++)
                pixels[y * Width + x] = marker;

        var texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();

        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;

        // Pivot у ног: нижний центр картинки.
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }
}
