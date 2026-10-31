using UnityEngine;

/// <summary>A falling coin. Detects the basket, scores, plays sound + particles, destroys itself.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject collectEffectPrefab;
    [SerializeField] private float spinSpeed = 6f;
    [SerializeField] private int value = 1;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 visualBaseScale = Vector3.one;
    private float spinOffset;
    private bool collected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (visual != null) visualBaseScale = visual.localScale;
        spinOffset = Random.value * Mathf.PI * 2f;
    }

    /// <summary>Called by the spawner right after instantiation.</summary>
    public void Launch(float fallSpeed)
    {
        rb.linearVelocity = Vector2.down * fallSpeed; // Unity 6: linearVelocity replaces velocity
    }

    private void Update()
    {
        // Fake "coin flip" spin by squashing the sprite horizontally.
        if (visual != null)
        {
            float s = Mathf.Abs(Mathf.Cos(Time.time * spinSpeed + spinOffset));
            visual.localScale = new Vector3(visualBaseScale.x * Mathf.Lerp(0.25f, 1f, s), visualBaseScale.y, visualBaseScale.z);
        }

        // Missed coin: remove it once it is below the screen.
        if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize - 1f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        if (GameManager.Instance != null) GameManager.Instance.AddScore(value);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayCoin();

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null) player.PlayCatchAnimation();

        if (collectEffectPrefab != null)
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
