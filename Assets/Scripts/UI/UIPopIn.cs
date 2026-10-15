using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Fades a modal in and pops its window from slightly small to full size whenever it is enabled. GK
// Runs on unscaled time so it works on the pause screen while Time.timeScale is 0.
public class UIPopIn : MonoBehaviour
{
    public RectTransform target; // The window that scales in.
    public CanvasGroup canvasGroup; // Optional fade for the whole modal.
    public float duration = 0.22f, startScale = 0.85f;

    void OnEnable()
    {
        CleanUpText();
        StartCoroutine(PopCo());
    }

    // Pause title: drop the offset shadow copy (it leaves a stray glyph). Buttons: large smooth text instead of tiny pixel text.
    void CleanUpText()
    {
        TMP_FontAsset smooth = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.name == "Title Shadow")
            {
                t.gameObject.SetActive(false);
            }
            else if (t.name == "Title Text")
            {
                CenterOnDrawnGlyphs(t);
            }
            else if (t.GetComponentInParent<Button>() != null && smooth != null)
            {
                t.font = smooth;
                t.fontSharedMaterial = smooth.material;
                t.enableAutoSizing = false;
                t.fontSize = 44f;
                t.fontStyle = FontStyles.Bold;
                t.characterSpacing = 2f;
            }
        }
    }

    // Centre the title on the glyphs actually drawn, ignoring trailing spacing that pulls it off-centre.
    static void CenterOnDrawnGlyphs(TMP_Text t)
    {
        RectTransform rt = t.rectTransform;
        rt.anchoredPosition = Vector2.zero;
        t.ForceMeshUpdate();
        TMP_TextInfo info = t.textInfo;
        if (info.characterCount == 0) return;
        // Use the visible left edge of the first letter and right edge of the last, not the advance-width bounds.
        float left = info.characterInfo[0].bottomLeft.x;
        float right = info.characterInfo[info.characterCount - 1].topRight.x;
        rt.anchoredPosition = new Vector2(-(left + right) * 0.5f, 0f);
    }

    IEnumerator PopCo()
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            if (canvasGroup != null) canvasGroup.alpha = eased;
            if (target != null) target.localScale = Vector3.one * Mathf.Lerp(startScale, 1f, eased);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (target != null) target.localScale = Vector3.one;
    }
}
