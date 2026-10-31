using UnityEngine;

/// <summary>A falling bomb hazard. Explodes on contact with the player basket.</summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Bomb : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float wobbleSpeed = 7f;

    private Rigidbody2D rb;
    private Camera cam;
    private Vector3 visualBaseScale = Vector3.one;
    private float wobbleOffset;
    private bool exploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        if (visual != null) visualBaseScale = visual.localScale;
        wobbleOffset = Random.value * Mathf.PI * 2f;
    }

    /// <summary>Called by the spawner right after instantiation.</summary>
    public void Launch(float fallSpeed)
    {
        rb.linearVelocity = Vector2.down * fallSpeed; // Unity 6: linearVelocity replaces velocity
    }

    private void Update()
    {
        // Slight wobble as it falls
        if (visual != null)
        {
            float w = Mathf.Sin(Time.time * wobbleSpeed + wobbleOffset);
            visual.localScale = new Vector3(visualBaseScale.x * (1f + 0.08f * w), visualBaseScale.y * (1f - 0.08f * w), visualBaseScale.z);
        }

        // Missed bomb: remove it once it is below the screen. Missing does nothing.
        if (cam != null && transform.position.y < cam.transform.position.y - cam.orthographicSize - 1f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded || !other.CompareTag("Player")) return;
        exploded = true;

        if (AudioManager.Instance != null) AudioManager.Instance.PlayExplosion();

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null) player.PlayHitAnimation();

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        if (GameManager.Instance != null) GameManager.Instance.HitBomb();

        Destroy(gameObject);
    }
}
