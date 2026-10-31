using UnityEngine;

/// <summary>Spawns coins above the screen. Faster speed and shorter delay as difficulty rises.</summary>
public class CoinSpawner : MonoBehaviour
{
    [SerializeField] private Coin coinPrefab;
    [SerializeField] private float baseFallSpeed = 4f;
    [SerializeField] private float startInterval = 0.9f;
    [SerializeField] private float minInterval = 0.3f;
    [SerializeField] private float horizontalMargin = 0.7f;
    [SerializeField] private float spawnHeightOffset = 1f;

    private Camera cam;
    private float timer;

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

        Coin coin = Instantiate(coinPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        float speed = baseFallSpeed * GameManager.Instance.Difficulty * Random.Range(0.9f, 1.15f);
        coin.Launch(speed);
    }

    private void ClearCoins()
    {
        foreach (Coin c in FindObjectsByType<Coin>(FindObjectsSortMode.None))
            Destroy(c.gameObject);
    }
}
