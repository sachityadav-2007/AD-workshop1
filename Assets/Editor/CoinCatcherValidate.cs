using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CoinCatcherValidate
{
    private static T First<T>() where T : Object
    {
        T[] all = Object.FindObjectsByType<T>();
        return all.Length > 0 ? all[0] : null;
    }

    private static bool HasRef(Object owner, string field)
    {
        var p = new SerializedObject(owner).FindProperty(field);
        return p != null && p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue != null;
    }

    public static void Run()
    {
        var sb = new StringBuilder();
        bool ok = true;
        void Fail(string msg) { ok = false; sb.AppendLine("FAIL: " + msg); }

        const string scenePath = "Assets/Scenes/GameScene.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError("VALIDATION RESULT: FAIL - GameScene.unity does not exist");
            EditorApplication.Exit(1);
            return;
        }
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var gm = First<GameManager>();
        var audio = First<AudioManager>();
        var spawner = First<CoinSpawner>();
        var ui = First<UIManager>();
        var player = First<PlayerController>();
        if (gm == null) Fail("GameManager missing in scene");
        if (audio == null) Fail("AudioManager missing in scene");
        if (spawner == null) Fail("CoinSpawner missing in scene");
        if (ui == null) Fail("UIManager missing in scene");
        if (player == null) Fail("PlayerController (PlayerBasket) missing in scene");
        else if (!player.CompareTag("Player")) Fail("PlayerBasket is not tagged Player");

        if (spawner != null)
        {
            if (!HasRef(spawner, "coinPrefab")) Fail("CoinSpawner.coinPrefab is EMPTY");
            if (!HasRef(spawner, "bombPrefab")) Fail("CoinSpawner.bombPrefab is EMPTY (this is why bombs never spawn)");
            else
            {
                var bombPrefab = (Bomb)new SerializedObject(spawner).FindProperty("bombPrefab").objectReferenceValue;
                var rb = bombPrefab.GetComponent<Rigidbody2D>();
                var col = bombPrefab.GetComponent<Collider2D>();
                if (rb == null) Fail("Bomb prefab has no Rigidbody2D");
                else
                {
                    if (rb.bodyType != RigidbodyType2D.Dynamic) Fail("Bomb Rigidbody2D is not Dynamic");
                    if (rb.gravityScale != 0f) sb.AppendLine("NOTE: Bomb gravityScale = " + rb.gravityScale);
                }
                if (col == null || !col.isTrigger) Fail("Bomb collider missing or not a trigger");
                if (bombPrefab.GetComponentInChildren<SpriteRenderer>() == null ||
                    bombPrefab.GetComponentInChildren<SpriteRenderer>().sprite == null)
                    Fail("Bomb prefab has no sprite");
                if (!HasRef(bombPrefab, "visual")) Fail("Bomb.visual is EMPTY");
                if (!HasRef(bombPrefab, "explosionEffectPrefab")) Fail("Bomb.explosionEffectPrefab is EMPTY");
            }
            var so = new SerializedObject(spawner);
            sb.AppendLine("INFO: bombChance=" + so.FindProperty("bombChance").floatValue +
                          " bombStartDelay=" + so.FindProperty("bombStartDelay").floatValue);
        }

        if (ui != null)
        {
            string[] fields = { "menuPanel", "hudPanel", "gameOverPanel", "scoreText", "timerText",
                "finalScoreText", "bestScoreText", "startButton", "restartButton", "mainMenuButton",
                "bombsText", "gameOverReasonText" };
            foreach (string f in fields)
                if (!HasRef(ui, f)) Fail("UIManager." + f + " is EMPTY");
        }

        bool inBuild = false;
        foreach (var s in EditorBuildSettings.scenes)
            if (s.path == scenePath && s.enabled) inBuild = true;
        if (!inBuild) Fail("GameScene is not enabled in Build Settings");

        Debug.Log("VALIDATION DETAILS:\n" + sb);
        Debug.Log(ok ? "VALIDATION RESULT: OK" : "VALIDATION RESULT: FAIL");
        EditorApplication.Exit(ok ? 0 : 1);
    }
}
