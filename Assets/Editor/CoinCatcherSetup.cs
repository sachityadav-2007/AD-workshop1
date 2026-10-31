using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

/// <summary>Menu: Coin Catcher > Build Game Scene. Generates sprites, prefabs, the scene and the UI.</summary>
public static class CoinCatcherSetup
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";

    [MenuItem("Coin Catcher/Build Game Scene")]
    public static void Build()
    {
        ExecuteBuild(true);
    }

    public static void ExecuteBuild(bool showDialog = false)
    {
        if (TMP_Settings.defaultFontAsset == null)
        {
            var fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (fallbackFont == null)
            {
                if (showDialog)
                    EditorUtility.DisplayDialog("TextMeshPro needed",
                        "Please import TMP Essentials first:\nWindow > TextMeshPro > Import TMP Essential Resources.\nThen run this menu again.",
                        "OK");
                else
                    Debug.LogWarning("TextMeshPro essentials needed before building GameScene.");
                return;
            }
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        foreach (string folder in new[] { "Sprites", "Prefabs", "Materials", "Scenes", "Audio" })
            if (!AssetDatabase.IsValidFolder("Assets/" + folder))
                AssetDatabase.CreateFolder("Assets", folder);

        // ---------- Sprites ----------
        Sprite coinSprite = SaveSprite(MakeCoinTexture(), "Assets/Sprites/Coin.png", 128f, Vector4.zero);
        Sprite bombSprite = SaveSprite(MakeBombTexture(), "Assets/Sprites/Bomb.png", 128f, Vector4.zero);
        Sprite basketSprite = SaveSprite(MakeBasketTexture(), "Assets/Sprites/Basket.png", 128f, Vector4.zero);
        Sprite backgroundSprite = SaveSprite(MakeBackgroundTexture(), "Assets/Sprites/Background.png", 100f, Vector4.zero);
        Sprite particleSprite = SaveSprite(MakeParticleTexture(), "Assets/Sprites/Particle.png", 32f, Vector4.zero);
        Sprite roundedSprite = SaveSprite(MakeRoundedTexture(), "Assets/Sprites/RoundedRect.png", 100f, new Vector4(24, 24, 24, 24));

        // ---------- Materials (unlit, so no 2D light is required) ----------
        Material spriteMat = MakeMaterial("Assets/Materials/SpriteUnlit.mat", null);
        Material particleMat = MakeMaterial("Assets/Materials/ParticleUnlit.mat",
            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Particle.png"));

        // ---------- Prefabs ----------
        GameObject effectPrefab = BuildEffectPrefab(particleMat);
        GameObject bombEffectPrefab = BuildBombExplosionPrefab(particleMat);
        GameObject coinPrefab = BuildCoinPrefab(coinSprite, spriteMat, effectPrefab);
        GameObject bombPrefab = BuildBombPrefab(bombSprite, spriteMat, bombEffectPrefab);
        GameObject basketPrefab = BuildBasketPrefab(basketSprite, spriteMat);

        // ---------- Scene ----------
        Scene_Build(backgroundSprite, spriteMat, roundedSprite, coinPrefab, bombPrefab, basketPrefab);

        if (showDialog)
            EditorUtility.DisplayDialog("Coin Catcher", "Game scene built and saved to " + ScenePath + ".\nPress Play!", "OK");
        else
            Debug.Log("Coin Catcher: Game scene built and saved to " + ScenePath);
    }

    // =====================================================================
    // Scene
    // =====================================================================
    private static void Scene_Build(Sprite bg, Material spriteMat, Sprite rounded, GameObject coinPrefab, GameObject bombPrefab, GameObject basketPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        GameObject light = GameObject.Find("Directional Light");
        if (light != null) Object.DestroyImmediate(light);

        Camera cam = Camera.main;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.1f, 0.07f, 0.36f);

        // Background
        var bgGo = new GameObject("Background");
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = bg;
        bgSr.sharedMaterial = spriteMat;
        bgSr.sortingOrder = -10;
        bgGo.AddComponent<BackgroundFitter>();

        // Player
        var basket = (GameObject)PrefabUtility.InstantiatePrefab(basketPrefab);
        basket.transform.position = new Vector3(0f, -4f, 0f);

        // Managers
        var gmGo = new GameObject("GameManager");
        gmGo.AddComponent<GameManager>();

        var audioGo = new GameObject("AudioManager");
        audioGo.AddComponent<AudioManager>();

        var spawnerGo = new GameObject("CoinSpawner");
        var spawner = spawnerGo.AddComponent<CoinSpawner>();
        SetRef(spawner, "coinPrefab", coinPrefab.GetComponent<Coin>());
        SetRef(spawner, "bombPrefab", bombPrefab.GetComponent<Bomb>());

        // UI
        BuildUI(rounded);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Selection.activeGameObject = gmGo;
    }

    private static void BuildUI(Sprite rounded)
    {
        // Canvas
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // EventSystem (needed for button clicks)
        var esGo = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        var module = esGo.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
#else
        esGo.AddComponent<StandaloneInputModule>();
#endif

        var ui = canvasGo.AddComponent<UIManager>();
        Color gold = new Color(1f, 0.85f, 0.2f);
        Vector2 center = new Vector2(0.5f, 0.5f);

        // ----- Main menu -----
        GameObject menu = MakePanel(canvasGo.transform, "MainMenuPanel", new Color(0.04f, 0.02f, 0.18f, 0.55f));
        MakeCard(menu.transform, rounded, new Vector2(1200f, 820f), new Color(0.1f, 0.05f, 0.3f, 0.85f));
        var title = MakeText(menu.transform, "TitleText", "COIN CATCHER", 150f, gold, FontStyles.Bold);
        Anchor(title.rectTransform, center, new Vector2(0f, 230f), new Vector2(1600f, 220f));
        var subtitle = MakeText(menu.transform, "SubtitleText", "Catch the falling coins!\nMove with  < >  or  A / D", 44f, new Color(1f, 1f, 1f, 0.9f), FontStyles.Normal);
        Anchor(subtitle.rectTransform, center, new Vector2(0f, 30f), new Vector2(1200f, 160f));
        Button startButton = MakeButton(menu.transform, "StartButton", "START GAME", new Vector2(0f, -190f), new Vector2(560f, 150f), new Color(0.2f, 0.8f, 0.4f), rounded);

        // ----- HUD -----
        GameObject hud = MakePanel(canvasGo.transform, "HUDPanel", null);
        var score = MakeText(hud.transform, "ScoreText", "Score: 0", 70f, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);
        Anchor(score.rectTransform, new Vector2(0f, 1f), new Vector2(50f, -30f), new Vector2(700f, 110f));
        var timer = MakeText(hud.transform, "TimerText", "Time: 60", 70f, Color.white, FontStyles.Bold, TextAlignmentOptions.Right);
        Anchor(timer.rectTransform, new Vector2(1f, 1f), new Vector2(-50f, -30f), new Vector2(700f, 110f));
        var bombs = MakeText(hud.transform, "BombsText", "Bombs: 0/3", 64f, new Color(1f, 0.4f, 0.4f), FontStyles.Bold, TextAlignmentOptions.Center);
        Anchor(bombs.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(500f, 110f));

        // ----- Game over -----
        GameObject over = MakePanel(canvasGo.transform, "GameOverPanel", new Color(0f, 0f, 0f, 0.65f));
        MakeCard(over.transform, rounded, new Vector2(1100f, 960f), new Color(0.12f, 0.07f, 0.3f, 0.92f));
        var overTitle = MakeText(over.transform, "GameOverTitle", "GAME OVER", 130f, new Color(1f, 0.35f, 0.35f), FontStyles.Bold);
        Anchor(overTitle.rectTransform, center, new Vector2(0f, 330f), new Vector2(1000f, 200f));
        var reason = MakeText(over.transform, "ReasonText", "Time's up!", 50f, new Color(1f, 0.85f, 0.3f), FontStyles.Normal);
        Anchor(reason.rectTransform, center, new Vector2(0f, 230f), new Vector2(1000f, 80f));
        var finalScore = MakeText(over.transform, "FinalScoreText", "Final Score: 0", 80f, Color.white, FontStyles.Bold);
        Anchor(finalScore.rectTransform, center, new Vector2(0f, 135f), new Vector2(1000f, 120f));
        var best = MakeText(over.transform, "BestScoreText", "Best: 0", 54f, gold, FontStyles.Normal);
        Anchor(best.rectTransform, center, new Vector2(0f, 35f), new Vector2(1000f, 90f));
        Button restartButton = MakeButton(over.transform, "RestartButton", "RESTART", new Vector2(0f, -140f), new Vector2(560f, 130f), new Color(0.2f, 0.8f, 0.4f), rounded);
        Button menuButton = MakeButton(over.transform, "MainMenuButton", "MAIN MENU", new Vector2(0f, -300f), new Vector2(560f, 130f), new Color(0.25f, 0.55f, 1f), rounded);

        hud.SetActive(false);
        over.SetActive(false);

        // Connect everything to UIManager
        SetRef(ui, "menuPanel", menu);
        SetRef(ui, "hudPanel", hud);
        SetRef(ui, "gameOverPanel", over);
        SetRef(ui, "scoreText", score);
        SetRef(ui, "timerText", timer);
        SetRef(ui, "bombsText", bombs);
        SetRef(ui, "finalScoreText", finalScore);
        SetRef(ui, "bestScoreText", best);
        SetRef(ui, "gameOverReasonText", reason);
        SetRef(ui, "startButton", startButton);
        SetRef(ui, "restartButton", restartButton);
        SetRef(ui, "mainMenuButton", menuButton);
    }

    // =====================================================================
    // Prefabs
    // =====================================================================
    private static GameObject BuildEffectPrefab(Material particleMat)
    {
        var fx = new GameObject("CoinCollectEffect");
        var ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.6f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.95f, 0.4f), new Color(1f, 0.6f, 0.1f));
        main.gravityModifier = 0.6f;
        main.maxParticles = 50;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 16) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var psr = fx.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = particleMat;
        psr.sortingOrder = 20;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fx, "Assets/Prefabs/CoinCollectEffect.prefab");
        Object.DestroyImmediate(fx);
        return prefab;
    }

    private static GameObject BuildBombExplosionPrefab(Material particleMat)
    {
        var fx = new GameObject("BombExplosionEffect");
        var ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.7f;
        main.loop = false;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.25f, 0.05f), new Color(1f, 0.85f, 0.1f));
        main.gravityModifier = 0.8f;
        main.maxParticles = 60;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.2f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var psr = fx.GetComponent<ParticleSystemRenderer>();
        psr.sharedMaterial = particleMat;
        psr.sortingOrder = 25;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(fx, "Assets/Prefabs/BombExplosionEffect.prefab");
        Object.DestroyImmediate(fx);
        return prefab;
    }

    private static GameObject BuildCoinPrefab(Sprite sprite, Material mat, GameObject effectPrefab)
    {
        var root = new GameObject("Coin");

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f; // the spawner sets the fall speed directly

        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * 0.9f;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = 5;

        var coin = root.AddComponent<Coin>();
        SetRef(coin, "visual", visual.transform);
        SetRef(coin, "collectEffectPrefab", effectPrefab);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Coin.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject BuildBombPrefab(Sprite sprite, Material mat, GameObject effectPrefab)
    {
        var root = new GameObject("Bomb");

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;

        var col = root.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.transform.localScale = Vector3.one * 0.9f;
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = 6;

        var bomb = root.AddComponent<Bomb>();
        SetRef(bomb, "visual", visual.transform);
        SetRef(bomb, "explosionEffectPrefab", effectPrefab);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/Bomb.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject BuildBasketPrefab(Sprite sprite, Material mat)
    {
        var root = new GameObject("PlayerBasket");
        root.tag = "Player";
        root.transform.localScale = Vector3.one * 1.2f;

        var sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sharedMaterial = mat;
        sr.sortingOrder = 10;

        var rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Collider covers only the open top part of the basket.
        Vector2 size = sprite.bounds.size;
        var col = root.AddComponent<BoxCollider2D>();
        col.size = new Vector2(size.x * 0.85f, size.y * 0.35f);
        col.offset = new Vector2(0f, size.y * 0.5f - col.size.y * 0.5f);

        root.AddComponent<PlayerController>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/PlayerBasket.prefab");
        Object.DestroyImmediate(root);
        return prefab;
    }

    // =====================================================================
    // UI helpers
    // =====================================================================
    private static RectTransform NewRect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go.GetComponent<RectTransform>();
    }

    private static void Anchor(RectTransform rt, Vector2 anchor, Vector2 position, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
    }

    private static GameObject MakePanel(Transform parent, string name, Color? overlay)
    {
        RectTransform rt = NewRect(name, parent);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        if (overlay.HasValue)
        {
            var img = rt.gameObject.AddComponent<Image>();
            img.color = overlay.Value;
        }
        rt.gameObject.AddComponent<CanvasGroup>();
        return rt.gameObject;
    }

    private static void MakeCard(Transform parent, Sprite rounded, Vector2 size, Color color)
    {
        RectTransform rt = NewRect("Card", parent);
        Anchor(rt, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = color;
        img.raycastTarget = false;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string text, float size, Color color,
                                            FontStyles style, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        RectTransform rt = NewRect(name, parent);
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = align;
        t.raycastTarget = false;
        return t;
    }

    private static Button MakeButton(Transform parent, string name, string label, Vector2 position, Vector2 size,
                                     Color color, Sprite rounded)
    {
        RectTransform rt = NewRect(name, parent);
        Anchor(rt, new Vector2(0.5f, 0.5f), position, size);

        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = rounded;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = img;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        colors.selectedColor = colors.highlightedColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        button.colors = colors;

        rt.gameObject.AddComponent<ButtonHoverScale>();

        var text = MakeText(rt, "Label", label, 56f, Color.white, FontStyles.Bold);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty prop = so.FindProperty(field);
        if (prop == null)
        {
            Debug.LogError("CoinCatcherSetup: field '" + field + "' not found on " + target.GetType().Name);
            return;
        }
        prop.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // =====================================================================
    // Asset helpers
    // =====================================================================
    private static Material MakeMaterial(string path, Texture texture)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            if (texture != null) existing.mainTexture = texture;
            return existing;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        var mat = new Material(shader);
        if (texture != null) mat.mainTexture = texture;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static Sprite SaveSprite(Texture2D tex, string path, float pixelsPerUnit, Vector4 border)
    {
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Texture2D NewTexture(int w, int h)
    {
        return new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
    }

    // =====================================================================
    // Procedural art
    // =====================================================================
    private static Texture2D MakeCoinTexture()
    {
        const int S = 128;
        Texture2D tex = NewTexture(S, S);
        var px = new Color[S * S];
        float R = S * 0.5f;
        Color groove = new Color(0.85f, 0.52f, 0.05f);

        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float dx = x + 0.5f - R;
                float dy = y + 0.5f - R;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float n = d / R;
                float a = Mathf.Clamp01(R - d);
                if (a <= 0f) { px[y * S + x] = Color.clear; continue; }

                float v = (dy / R + 1f) * 0.5f;
                Color c;
                if (n > 0.84f)
                {
                    c = Color.Lerp(new Color(0.72f, 0.42f, 0.04f), new Color(1f, 0.88f, 0.3f), v);
                }
                else
                {
                    c = Color.Lerp(new Color(1f, 0.70f, 0.10f), new Color(1f, 0.93f, 0.45f), v);
                    if (n > 0.72f && n < 0.78f) c = groove;
                    if (Mathf.Abs(dx) < S * 0.045f && Mathf.Abs(dy) < S * 0.24f) c = groove;
                    float h = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(dx, dy), new Vector2(-S * 0.17f, S * 0.19f)) / (S * 0.2f));
                    c = Color.Lerp(c, Color.white, h * 0.55f);
                }
                c.a = a;
                px[y * S + x] = c;
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeBombTexture()
    {
        const int S = 128;
        Texture2D tex = NewTexture(S, S);
        var px = new Color[S * S];
        float cx = S * 0.5f;
        float cy = S * 0.44f;
        float R = S * 0.36f;

        Color darkBody = new Color(0.12f, 0.12f, 0.16f);
        Color lightBody = new Color(0.28f, 0.28f, 0.35f);
        Color redStripe = new Color(0.95f, 0.18f, 0.22f);
        Color capColor = new Color(0.35f, 0.35f, 0.42f);
        Color fuseColor = new Color(0.85f, 0.72f, 0.55f);
        Color sparkYellow = new Color(1f, 0.95f, 0.25f);
        Color sparkOrange = new Color(1f, 0.45f, 0.05f);

        Vector2 sparkPos = new Vector2(cx + 26f, cy + R + 26f);

        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float px_x = x + 0.5f;
                float px_y = y + 0.5f;
                float dx = px_x - cx;
                float dy = px_y - cy;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                Color c = Color.clear;

                // 1. Bomb Sphere Body
                if (d <= R + 1f)
                {
                    float a = Mathf.Clamp01(R - d + 0.5f);
                    float v = (dy / R + 1f) * 0.5f;
                    Color body = Color.Lerp(darkBody, lightBody, v);

                    // Red stripe around equator
                    if (Mathf.Abs(dy) < R * 0.26f)
                    {
                        float stripeV = Mathf.Clamp01((dy / (R * 0.26f) + 1f) * 0.5f);
                        body = Color.Lerp(redStripe * 0.75f, redStripe, stripeV);
                    }

                    // Rim shadow
                    if (d > R * 0.82f)
                    {
                        body = Color.Lerp(body, Color.black, 0.45f);
                    }

                    // Specular highlight on top-left
                    float highlight = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(dx, dy), new Vector2(-R * 0.38f, R * 0.38f)) / (R * 0.32f));
                    body = Color.Lerp(body, Color.white, highlight * 0.65f);

                    body.a = a;
                    c = body;
                }

                // 2. Collar / Cap on top of sphere
                if (px_y >= cy + R - 3f && px_y <= cy + R + 9f && Mathf.Abs(dx) <= 9f)
                {
                    float capA = Mathf.Clamp01(9f - Mathf.Abs(dx));
                    Color cap = capColor;
                    if (Mathf.Abs(dx) < 6f) cap = Color.Lerp(capColor, Color.white, 0.25f);
                    cap.a = capA;
                    c = Color.Lerp(c, cap, capA);
                }

                // 3. Curved Fuse
                float fuseT = Mathf.Clamp01((px_y - (cy + R + 6f)) / 22f);
                if (px_y >= cy + R + 6f && px_y <= sparkPos.y + 2f)
                {
                    float targetX = cx + fuseT * 26f + Mathf.Sin(fuseT * Mathf.PI) * 4f;
                    float distToFuse = Mathf.Abs(px_x - targetX);
                    if (distToFuse <= 2.2f)
                    {
                        float fuseA = Mathf.Clamp01(2.2f - distToFuse);
                        Color fCol = (y % 4 < 2) ? fuseColor : fuseColor * 0.75f;
                        fCol.a = fuseA;
                        c = Color.Lerp(c, fCol, fuseA);
                    }
                }

                // 4. Spark at the tip of the fuse
                float distSpark = Vector2.Distance(new Vector2(px_x, px_y), sparkPos);
                if (distSpark <= 8f)
                {
                    float sparkA = Mathf.Clamp01((8f - distSpark) / 5f);
                    float angle = Mathf.Atan2(px_y - sparkPos.y, px_x - sparkPos.x);
                    float rays = 0.65f + 0.35f * Mathf.Cos(angle * 6f);
                    sparkA *= rays;
                    Color spk = Color.Lerp(sparkOrange, sparkYellow, Mathf.Clamp01(1f - distSpark / 5f));
                    if (distSpark < 2.5f) spk = Color.white;
                    spk.a = Mathf.Clamp01(sparkA * 1.5f);
                    c = Color.Lerp(c, spk, spk.a);
                }

                px[y * S + x] = c;
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeBasketTexture()
    {
        const int W = 256;
        const int H = 160;
        Texture2D tex = NewTexture(W, H);
        var px = new Color[W * H];
        const float rimStart = 0.82f;
        Color stripeA = new Color(1f, 0.55f, 0.12f);
        Color stripeB = new Color(0.95f, 0.28f, 0.32f);

        for (int y = 0; y < H; y++)
        {
            float ty = (y + 0.5f) / H;
            bool rim = ty >= rimStart;
            float halfW = rim ? W * 0.5f - 2f : Mathf.Lerp(W * 0.30f, W * 0.43f, ty / rimStart);

            for (int x = 0; x < W; x++)
            {
                float dx = Mathf.Abs(x + 0.5f - W * 0.5f);
                float edge = halfW - dx;
                float a = Mathf.Clamp01(edge)
                          * Mathf.Clamp01((y + 0.5f) / 2f)
                          * Mathf.Clamp01((H - (y + 0.5f)) / 2f);
                if (a <= 0f) { px[y * W + x] = Color.clear; continue; }

                Color c;
                if (rim)
                {
                    float k = (ty - rimStart) / (1f - rimStart);
                    c = Color.Lerp(new Color(0.35f, 0.15f, 0.65f), new Color(0.68f, 0.42f, 0.95f), k);
                }
                else
                {
                    c = ((x / 28) % 2 == 0) ? stripeA : stripeB;
                    if (y % 22 < 3) c *= 0.82f;
                    c *= Mathf.Lerp(0.72f, 1f, ty / rimStart);
                    if (ty > rimStart - 0.025f) c *= 0.7f;
                }
                if (edge < 3f) c *= 0.6f;
                c.a = a;
                px[y * W + x] = c;
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeBackgroundTexture()
    {
        const int S = 512;
        Texture2D tex = NewTexture(S, S);
        var px = new Color[S * S];
        Color top = new Color(0.10f, 0.07f, 0.36f);
        Color mid = new Color(0.42f, 0.18f, 0.74f);
        Color bottom = new Color(1f, 0.52f, 0.48f);

        for (int y = 0; y < S; y++)
        {
            float t = 1f - (y + 0.5f) / S; // 0 at top, 1 at bottom
            Color c = t < 0.55f ? Color.Lerp(top, mid, t / 0.55f) : Color.Lerp(mid, bottom, (t - 0.55f) / 0.45f);
            for (int x = 0; x < S; x++) px[y * S + x] = c;
        }

        // Soft translucent bubbles for decoration.
        var rnd = new System.Random(7);
        for (int i = 0; i < 45; i++)
        {
            int cx = rnd.Next(S), cy = rnd.Next(S), r = rnd.Next(8, 46);
            float alpha = 0.05f + (float)rnd.NextDouble() * 0.09f;
            for (int y = Mathf.Max(0, cy - r - 2); y < Mathf.Min(S, cy + r + 2); y++)
            {
                for (int x = Mathf.Max(0, cx - r - 2); x < Mathf.Min(S, cx + r + 2); x++)
                {
                    float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                    if (dist > r) continue;
                    float a = alpha;
                    if (Mathf.Abs(dist - r * 0.92f) < 1.5f) a *= 2.2f;
                    px[y * S + x] = Color.Lerp(px[y * S + x], Color.white, a);
                }
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeParticleTexture()
    {
        const int S = 32;
        Texture2D tex = NewTexture(S, S);
        var px = new Color[S * S];
        float R = S * 0.5f;
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float d = Mathf.Sqrt((x + 0.5f - R) * (x + 0.5f - R) + (y + 0.5f - R) * (y + 0.5f - R)) / R;
                float a = Mathf.Pow(Mathf.Clamp01(1f - d), 1.5f);
                px[y * S + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    private static Texture2D MakeRoundedTexture()
    {
        const int S = 64;
        const float radius = 20f;
        Texture2D tex = NewTexture(S, S);
        var px = new Color[S * S];
        float half = S * 0.5f;
        for (int y = 0; y < S; y++)
        {
            for (int x = 0; x < S; x++)
            {
                float qx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                float qy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                float outside = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) + Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f));
                float dist = outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
                px[y * S + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - dist));
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }
}