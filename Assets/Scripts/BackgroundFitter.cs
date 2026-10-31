using UnityEngine;

/// <summary>Scales the background sprite to always cover the camera view, at any aspect ratio.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private SpriteRenderer sprite;
    private Camera cam;

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null || sprite.sprite == null) return;

        float viewHeight = cam.orthographicSize * 2f;
        float viewWidth = viewHeight * cam.aspect;
        Vector2 size = sprite.sprite.bounds.size;
        float scale = Mathf.Max(viewWidth / size.x, viewHeight / size.y);

        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
    }
}