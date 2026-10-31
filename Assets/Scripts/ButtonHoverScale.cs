using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Smoothly grows a button on hover and shrinks it slightly when pressed.</summary>
public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressedScale = 0.95f;
    [SerializeField] private float speed = 14f;

    private Vector3 baseScale;
    private float target = 1f;
    private bool hovering;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        transform.localScale = baseScale;
        target = 1f;
        hovering = false;
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, t);
    }

    public void OnPointerEnter(PointerEventData eventData) { hovering = true; target = hoverScale; }
    public void OnPointerExit(PointerEventData eventData) { hovering = false; target = 1f; }
    public void OnPointerDown(PointerEventData eventData) { target = pressedScale; }
    public void OnPointerUp(PointerEventData eventData) { target = hovering ? hoverScale : 1f; }
}