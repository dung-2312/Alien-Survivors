using UnityEngine;
using UnityEngine.EventSystems;

// Chunky retro hover: the button grows slightly while pointed at or selected. GK
// Uses unscaled time so it still responds on the pause screen.
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public float hoverScale = 1.06f, speed = 16f;

    private float targetScale = 1f;

    void OnDisable()
    {
        targetScale = 1f;
        transform.localScale = Vector3.one;
    }

    void Update()
    {
        float s = Mathf.Lerp(transform.localScale.x, targetScale, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
        transform.localScale = new Vector3(s, s, 1f);
    }

    public void OnPointerEnter(PointerEventData eventData) { targetScale = hoverScale; }
    public void OnPointerExit(PointerEventData eventData) { targetScale = 1f; }
    public void OnSelect(BaseEventData eventData) { targetScale = hoverScale; }
    public void OnDeselect(BaseEventData eventData) { targetScale = 1f; }
}
