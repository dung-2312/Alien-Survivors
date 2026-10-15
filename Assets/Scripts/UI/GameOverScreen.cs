using System.Collections;
using TMPro;
using UnityEngine;

// Drives the retro gold-framed Game Over screen: fades it in, slams the title, counts up the run stats. GK
// The layout itself is built by Tools > Survivors > Generate Game Over & Pause UI.
public class GameOverScreen : MonoBehaviour
{
    public CanvasGroup canvasGroup; // Fades the whole screen in.
    public RectTransform window; // The gold window that pops in.
    public RectTransform title; // The GAME OVER title that slams down.

    public TMP_Text timeText, levelText, coinsText, bestText; // The run stats.
    public GameObject newBestBadge; // Shown only when the run beat the best time.

    public float introDuration = 0.4f, countDuration = 0.9f;

    private bool pulseBadge;

    public void Show(float survivedTime, float bestTime, bool isNewBest, int level, int coins)
    {
        gameObject.SetActive(true);
        UseSmoothFontForSmallText();
        StopAllCoroutines();
        StartCoroutine(ShowCo(survivedTime, bestTime, isNewBest, level, coins));
    }

    // Small labels use the smooth Liberation font; the pixel font only stays on the big headings and numbers.
    void UseSmoothFontForSmallText()
    {
        TMP_FontAsset smooth = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (smooth == null) return;
        foreach (TMP_Text t in GetComponentsInChildren<TMP_Text>(true))
        {
            if (t.fontSize <= 40f)
            {
                t.font = smooth;
                t.fontSharedMaterial = smooth.material;
            }
        }
    }

    IEnumerator ShowCo(float survivedTime, float bestTime, bool isNewBest, int level, int coins)
    {
        pulseBadge = false;
        newBestBadge.SetActive(false);
        newBestBadge.transform.localScale = Vector3.one;
        bestText.text = FormatTime(bestTime);
        SetStats(0f, 1, 0);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // Unscaled time so the screen still animates if the game is paused underneath it.
        for (float t = 0f; t < introDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / introDuration;
            canvasGroup.alpha = EaseOutCubic(p);
            window.localScale = Vector3.one * Mathf.LerpUnclamped(0.8f, 1f, EaseOutBack(p));
            title.localScale = Vector3.one * Mathf.Lerp(1.8f, 1f, EaseOutCubic(p));
            yield return null;
        }
        canvasGroup.alpha = 1f;
        window.localScale = Vector3.one;
        title.localScale = Vector3.one;
        canvasGroup.interactable = true;

        for (float t = 0f; t < countDuration; t += Time.unscaledDeltaTime)
        {
            float p = EaseOutCubic(t / countDuration);
            SetStats(survivedTime * p, Mathf.RoundToInt(Mathf.Lerp(1f, level, p)), Mathf.RoundToInt(coins * p));
            yield return null;
        }
        SetStats(survivedTime, level, coins);

        if (isNewBest)
        {
            newBestBadge.SetActive(true);
            pulseBadge = true;
        }
    }

    void Update()
    {
        if (pulseBadge) // Gentle heartbeat on the NEW BEST badge.
        {
            float s = 1f + 0.06f * Mathf.Sin(Time.unscaledTime * 6f);
            newBestBadge.transform.localScale = new Vector3(s, s, 1f);
        }
    }

    void SetStats(float time, int level, int coins)
    {
        timeText.text = FormatTime(time);
        levelText.text = level.ToString();
        coinsText.text = coins.ToString();
    }

    public static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    static float EaseOutCubic(float p)
    {
        p = Mathf.Clamp01(p);
        return 1f - Mathf.Pow(1f - p, 3f);
    }

    static float EaseOutBack(float p)
    {
        p = Mathf.Clamp01(p);
        const float c1 = 1.70158f, c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(p - 1f, 3f) + c1 * Mathf.Pow(p - 1f, 2f);
    }
}
