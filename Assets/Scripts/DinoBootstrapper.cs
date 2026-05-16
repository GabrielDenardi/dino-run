using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class DinoBootstrapper : MonoBehaviour
{
    private static DinoBootstrapper instance;

    public const float GroundCenterY = -3f;
    public const float GroundTopY = -2.5f;
    public const float PlayerStartX = -6.5f;
    public const float PlayerStartY = -1.9f;
    public const float PlayerMinX = -8.2f;
    public const float PlayerMaxX = 4.0f;
    public const float SpawnX = 11.5f;

    public static Material GroundMaterial { get; private set; }
    public static Material PlayerMaterial { get; private set; }
    public static Material ObstacleMaterial { get; private set; }

    public static Material CreateTintedMaterial(Color color)
    {
        return CreateMaterial(color);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBootstrapperExists()
    {
        if (Object.FindAnyObjectByType<DinoBootstrapper>() != null)
        {
            return;
        }

        new GameObject(nameof(DinoBootstrapper)).AddComponent<DinoBootstrapper>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Physics.gravity = new Vector3(0f, -25f, 0f);

        EnsureMaterials();
        BuildLighting();
        BuildCamera();
        BuildBackdrop();
        BuildWorld();
        BuildGameplay();
        BuildUI();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    private static void EnsureMaterials()
    {
        if (GroundMaterial != null && PlayerMaterial != null && ObstacleMaterial != null)
        {
            return;
        }

        GroundMaterial = CreateMaterial(new Color(0.36f, 0.25f, 0.14f));
        PlayerMaterial = CreateMaterial(new Color(0.18f, 0.34f, 0.2f));
        ObstacleMaterial = CreateMaterial(new Color(0.22f, 0.22f, 0.24f));
    }

    private static Material CreateMaterial(Color color)
    {
        var shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        var material = new Material(shader);
        material.color = color;
        return material;
    }

    private static void BuildLighting()
    {
        if (Object.FindAnyObjectByType<Light>() != null)
        {
            return;
        }

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(1f, 0.98f, 0.94f);
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static void BuildCamera()
    {
        if (Camera.main != null)
        {
            return;
        }

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.backgroundColor = new Color(0.94f, 0.96f, 0.98f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 50f;

        cameraObject.transform.position = new Vector3(0f, 2f, -10f);
    }

    private static void BuildWorld()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.tag = "Ground";
        ground.transform.position = new Vector3(0f, GroundCenterY, 0f);
        ground.transform.localScale = new Vector3(80f, 1f, 1f);
        ground.GetComponent<Renderer>().enabled = false;

        var groundVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        groundVisual.name = "GroundBody";
        groundVisual.transform.SetParent(ground.transform, false);
        groundVisual.transform.localPosition = Vector3.zero;
        groundVisual.transform.localScale = new Vector3(80f, 1f, 1f);
        groundVisual.GetComponent<Collider>().enabled = false;
        groundVisual.GetComponent<Renderer>().sharedMaterial = GroundMaterial;

        var grassVisual = new GameObject("GroundGrass");
        grassVisual.transform.SetParent(ground.transform, false);
        grassVisual.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        grassVisual.transform.localScale = Vector3.one;

        var groundSprite = DinoSpriteLibrary.GetTextureSprite("Art/External/Platformer32GrassDirt/0_0_trim", 32f);
        var groundRenderer = grassVisual.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = groundSprite;
        groundRenderer.drawMode = SpriteDrawMode.Simple;
        groundRenderer.sortingOrder = 2;

        if (groundSprite != null)
        {
            var scaleX = 80f / Mathf.Max(0.01f, groundSprite.bounds.size.x);
            grassVisual.transform.localScale = new Vector3(scaleX, 0.7f, 1f);
        }
    }

    private static void BuildBackdrop()
    {
        var backdrop = new GameObject("Backdrop");

        CreateBackdropLayer(backdrop.transform, "Summer8Layer1", "Art/Backgrounds/Summer8_Layer1", new Vector3(0f, 1.9f, 12f), -30, 1.0f);
        CreateBackdropLayer(backdrop.transform, "Summer8Layer2", "Art/Backgrounds/Summer8_Layer2", new Vector3(0f, 2.1f, 11.5f), -29, 1.02f);
        CreateBackdropLayer(backdrop.transform, "Summer8Layer3", "Art/Backgrounds/Summer8_Layer3", new Vector3(0f, 2.4f, 11f), -28, 1.04f);
    }

    private static void CreateBackdropLayer(
        Transform parent,
        string name,
        string resourcePath,
        Vector3 position,
        int sortingOrder,
        float scaleMultiplier)
    {
        var layer = new GameObject(name);
        layer.transform.SetParent(parent, false);
        layer.transform.position = position;

        var renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateBackdropSprite(resourcePath);
        renderer.sortingOrder = sortingOrder;

        if (renderer.sprite != null)
        {
            var camera = Camera.main;
            var height = camera != null ? camera.orthographicSize * 2f : 11f;
            var width = height * (camera != null ? camera.aspect : 16f / 9f);
            var spriteWidth = renderer.sprite.bounds.size.x;
            var spriteHeight = renderer.sprite.bounds.size.y;
            var scale = Mathf.Max(width / Mathf.Max(0.01f, spriteWidth), height / Mathf.Max(0.01f, spriteHeight));
            layer.transform.localScale = Vector3.one * (scale * scaleMultiplier);
        }
    }

    private static Sprite CreateBackdropSprite(string resourcePath)
    {
        var texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static void BuildGameplay()
    {
        var gameManager = new GameObject("GameManager");
        gameManager.AddComponent<DinoGameManager>();

        var player = new GameObject("Dino");
        player.name = "Dino";
        player.tag = "Untagged";
        player.transform.position = new Vector3(PlayerStartX, PlayerStartY, 0f);

        var body = player.AddComponent<Rigidbody>();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var collider = player.AddComponent<BoxCollider>();
        collider.size = new Vector3(0.95f, 1.15f, 0.9f);
        collider.center = new Vector3(0f, 0.1f, 0f);

        var dinoSprite = DinoSpriteLibrary.GetAnySprite("Art/External/FreeDinoSprite/png");
        var targetVisualHeight = 1.72f;
        var spriteHeight = dinoSprite != null ? dinoSprite.bounds.size.y : 1f;
        var visualScale = targetVisualHeight / Mathf.Max(0.01f, spriteHeight);

        var visualRoot = new GameObject("Visual");
        visualRoot.transform.SetParent(player.transform, false);
        visualRoot.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        visualRoot.transform.localScale = new Vector3(visualScale, visualScale, 1f);

        var spriteRenderer = visualRoot.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = 20;
        spriteRenderer.sprite = dinoSprite;
        spriteRenderer.color = Color.white;

        player.AddComponent<DinoController>();
        player.AddComponent<DinoVisualAnimator>();

        var spawner = new GameObject("ObstacleSpawner");
        spawner.AddComponent<ObstacleSpawner>();
    }

    private static void BuildUI()
    {
        var canvasObject = new GameObject("Canvas");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var hudObject = new GameObject("HUD", typeof(RectTransform));
        hudObject.transform.SetParent(canvasObject.transform, false);
        var hudRect = hudObject.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        var hud = hudObject.AddComponent<DinoUI>();

        var topBarObject = new GameObject("TopBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        topBarObject.transform.SetParent(hudObject.transform, false);

        var topBarRect = topBarObject.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0f, 1f);
        topBarRect.anchorMax = new Vector2(1f, 1f);
        topBarRect.pivot = new Vector2(0.5f, 1f);
        topBarRect.anchoredPosition = Vector2.zero;
        topBarRect.sizeDelta = new Vector2(0f, 96f);

        var topBarImage = topBarObject.GetComponent<Image>();
        topBarImage.color = new Color(0.08f, 0.1f, 0.16f, 0.82f);

        var bestText = CreateText(topBarObject.transform, "BestText", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), 28, TextAnchor.MiddleLeft);
        var messageText = CreateText(topBarObject.transform, "MessageText", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), 22, TextAnchor.MiddleCenter);
        var scoreText = CreateText(topBarObject.transform, "ScoreText", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-28f, 0f), 34, TextAnchor.MiddleRight);

        bestText.rectTransform.sizeDelta = new Vector2(320f, 96f);
        messageText.rectTransform.sizeDelta = new Vector2(760f, 96f);
        scoreText.rectTransform.sizeDelta = new Vector2(320f, 96f);

        hud.Bind(scoreText, bestText, messageText);
    }

    private static Text CreateText(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        int fontSize,
        TextAnchor alignment)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(760f, 140f);

        var text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.02f, 0.03f, 0.05f, 0.95f);
        outline.effectDistance = new Vector2(2f, -2f);

        var shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.35f);
        shadow.effectDistance = new Vector2(0f, -3f);

        return text;
    }
}
