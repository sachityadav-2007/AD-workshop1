using UnityEngine;

/// <summary>Spawns coins above the screen. Faster speed and shorter delay as difficulty rises.</summary>
public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private float baseFallSpeed = 4f;
    [SerializeField] private float startInterval = 0.9f;
    [SerializeField] private float minInterval = 0.3f;
    [SerializeField] private float horizontalMargin = 0.7f;
    [SerializeField] private float spawnHeightOffset = 1f;
    [SerializeField, Range(0f, 0.5f)] private float bombChance = 0.25f;
    [SerializeField] private float bombStartDelay = 3f;

    private Camera cam;
    private float timer;
    private bool lastSpawnedBomb;
    private int spawnedBombCount;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged += OnStateChanged;
            OnStateChanged(GameManager.Instance.State);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        ClearCoins();
        lastSpawnedBomb = false;
        spawnedBombCount = 0;
        if (state == GameState.Playing) timer = 0.5f; // short pause before the first coin
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Spawn();
            float difficulty = GameManager.Instance.Difficulty;
            timer = Mathf.Max(minInterval, startInterval / Mathf.Sqrt(difficulty));
        }
    }

    private void Spawn()
    {
        float halfWidth = cam.orthographicSize * cam.aspect;
        float margin = Mathf.Min(horizontalMargin, halfWidth * 0.5f);
        float x = Random.Range(cam.transform.position.x - halfWidth + margin,
                               cam.transform.position.x + halfWidth - margin);
        float y = cam.transform.position.y + cam.orthographicSize + spawnHeightOffset;
        Vector3 pos = new Vector3(x, y, 0f);

        float speed = baseFallSpeed * GameManager.Instance.Difficulty * Random.Range(0.9f, 1.15f);

        // Bomb fairness rules:
        // - no bombs in the first 4 seconds (bombStartDelay)
        // - never two bombs in a row
        // - bomb chance starts at 15% and rises to 30% as difficulty increases
        bool canSpawnBomb = bombPrefab != null
                            && !lastSpawnedBomb
                            && GameManager.Instance.ElapsedTime >= bombStartDelay;

        float difficultyFactor = Mathf.Clamp01((GameManager.Instance.Difficulty - 1f) / 1.5f);
        float currentBombChance = Mathf.Lerp(bombChance, 0.30f, difficultyFactor);

        if (canSpawnBomb && Random.value < currentBombChance)
        {
            Bomb bomb = Instantiate(bombPrefab, pos, Quaternion.identity);
            bomb.Launch(speed);
            lastSpawnedBomb = true;
            spawnedBombCount++;
            if (spawnedBombCount <= 3)
                Debug.Log("[CoinCatcher] Bomb spawned at x=" + pos.x.ToString("0.0"));
        }
        else
        {
            Coin coin = Instantiate(coinPrefab, pos, Quaternion.identity);
            coin.Launch(speed);
            lastSpawnedBomb = false;
        }
    }

    private void ClearCoins()
    {
        foreach (Coin c in FindObjectsByType<Coin>())
            Destroy(c.gameObject);
        foreach (Bomb b in FindObjectsByType<Bomb>())
            Destroy(b.gameObject);
    }
}
