using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>Moves the basket with Arrow Keys / A / D, smoothly, and keeps it inside the screen.</summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float acceleration = 100f;
    [Tooltip("Gap between the basket and the bottom edge of the screen.")]
    [SerializeField] private float bottomMargin = 0.3f;
    [SerializeField] private float edgePadding = 0.05f;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Camera cam;
    private float halfWidth;
    private float halfHeight;
    private float velocityX;
    private Vector3 baseScale;
    private Coroutine catchRoutine;
    private Coroutine hitRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        cam = Camera.main;
        baseScale = transform.localScale;

        if (sprite != null)
        {
            Bounds b = sprite.bounds;
            halfWidth = b.extents.x;
            halfHeight = b.extents.y;
        }
        else
        {
            halfWidth = 0.5f;
            halfHeight = 0.5f;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged += OnStateChanged;
        ResetPosition();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
        if (state == GameState.Playing) ResetPosition();
    }

    private void Update()
    {
        bool playing = GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;
        float input = playing ? ReadHorizontalInput() : 0f;
        velocityX = Mathf.MoveTowards(velocityX, input * maxSpeed, acceleration * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || rb == null) return;

        float camHalfWidth = cam.orthographicSize * cam.aspect;
        float minX = cam.transform.position.x - camHalfWidth + halfWidth + edgePadding;
        float maxX = cam.transform.position.x + camHalfWidth - halfWidth - edgePadding;

        float newX = Mathf.Clamp(rb.position.x + velocityX * Time.fixedDeltaTime, minX, maxX);

        // Stop pushing against a wall so the basket reacts instantly when you reverse.
        if ((newX <= minX && velocityX < 0f) || (newX >= maxX && velocityX > 0f))
            velocityX = 0f;

        rb.MovePosition(new Vector2(newX, rb.position.y));
    }

    private static float ReadHorizontalInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        if (kb == null) return 0f;
        float x = 0f;
        if (kb.leftArrowKey.isPressed || kb.aKey.isPressed) x -= 1f;
        if (kb.rightArrowKey.isPressed || kb.dKey.isPressed) x += 1f;
        return x;
#else
        return Input.GetAxisRaw("Horizontal"); // Arrow keys + A/D by default
#endif
    }

    public void ResetPosition()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null || rb == null) return;

        float y = cam.transform.position.y - cam.orthographicSize + bottomMargin + halfHeight;
        Vector2 pos = new Vector2(cam.transform.position.x, y);
        transform.position = pos;
        rb.position = pos;
        velocityX = 0f;
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        if (sprite != null) sprite.color = Color.white;
    }

    /// <summary>Small squash-and-stretch when a coin is caught.</summary>
    public void PlayCatchAnimation()
    {
        if (catchRoutine != null) StopCoroutine(catchRoutine);
        transform.localScale = baseScale;
        catchRoutine = StartCoroutine(CatchRoutine());
    }

    private IEnumerator CatchRoutine()
    {
        const float duration = 0.15f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            transform.localScale = new Vector3(baseScale.x * (1f + 0.12f * k), baseScale.y * (1f - 0.12f * k), baseScale.z);
            yield return null;
        }
        transform.localScale = baseScale;
    }

    /// <summary>Flashes the basket red when a bomb is caught.</summary>
    public void PlayHitAnimation()
    {
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        if (sprite == null) yield break;
        Color original = Color.white;
        Color hitColor = new Color(1f, 0.25f, 0.25f);
        const float duration = 0.35f;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float k = Mathf.Sin(t / duration * Mathf.PI);
            sprite.color = Color.Lerp(original, hitColor, k);
            yield return null;
        }
        sprite.color = original;
    }
}