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

    public TMP_Text timeText, levelText, coinsText, bestText, killsText; // The run stats.
    public GameObject newBestBadge; // Shown only when the run beat the best time.

    public float introDuration = 0.4f, countDuration = 0.9f;

    private bool pulseBadge;

    public void Show(float survivedTime, float bestTime, bool isNewBest, int level, int coins)
    {
        gameObject.SetActive(true);
        BossController.HideAllBossUI();
        if (UIController.instance != null) UIController.instance.HideWarningBanner();

        // Add gothic floating embers if not already present
        if (GetComponentInChildren<GothicMenuEmbers>() == null)
        {
            GameObject embersGO = new GameObject("GameOver Embers Layer", typeof(RectTransform));
            embersGO.transform.SetParent(transform, false);
            embersGO.transform.SetSiblingIndex(0);
            RectTransform ert = embersGO.GetComponent<RectTransform>();
            ert.anchorMin = Vector2.zero;
            ert.anchorMax = Vector2.one;
            ert.offsetMin = ert.offsetMax = Vector2.zero;
            embersGO.AddComponent<GothicMenuEmbers>();
        }

        EnsureVSElements();
        UseSmoothFontForSmallText();
        StopAllCoroutines();
        StartCoroutine(ShowCo(survivedTime, bestTime, isNewBest, level, coins));
    }

    void EnsureVSElements()
    {
        if (window == null) return;

        // 1. Ornate Royal Gothic Window Frame & Dimensions
        UnityEngine.UI.Image winImg = window.GetComponent<UnityEngine.UI.Image>();
        if (winImg != null)
        {
            winImg.sprite = UIController.GetGothicWindowSprite();
            winImg.type = UnityEngine.UI.Image.Type.Sliced;
            winImg.color = Color.white;
        }
        window.sizeDelta = new Vector2(860f, 680f);
        window.anchoredPosition = new Vector2(0f, -30f);

        // 2. Dynamic Title based on whether player survived
        bool survived = PlayerHealthController.instance != null && PlayerHealthController.instance.currentHealth > 0;
        if (title != null)
        {
            title.anchoredPosition = new Vector2(0f, 335f);
            title.sizeDelta = new Vector2(1000f, 130f);

            TMP_Text[] titleTexts = title.GetComponentsInChildren<TMP_Text>(true);
            foreach (var tt in titleTexts)
            {
                if (tt.name == "Title Shadow")
                {
                    tt.text = survived ? "❖  VICTORY SURVIVED  ❖" : "☠   GAME OVER   ☠";
                    tt.color = survived ? new Color(0.20f, 0.14f, 0.02f) : new Color(0.12f, 0.01f, 0.02f);
                }
                else if (tt.name == "Title Text")
                {
                    tt.text = survived ? "❖  VICTORY SURVIVED  ❖" : "☠   GAME OVER   ☠";
                    tt.color = survived ? new Color(1f, 0.88f, 0.25f) : new Color(1f, 0.16f, 0.24f);
                }
            }
        }

        // Subtitle
        Transform subT = window.Find("Subtitle");
        if (subT != null)
        {
            TMP_Text subTmp = subT.GetComponent<TMP_Text>();
            if (subTmp != null)
            {
                subTmp.text = survived ? "✦ THOU HAST CONQUERED THE INFERNAL ABYSS ✦" : "✦ THE SHADOWS CLAIM THY VALIANT SOUL ✦";
                subTmp.color = new Color(0.92f, 0.82f, 0.55f, 0.95f);
                subTmp.fontSize = 26f;
            }
        }

        // Stats Panel
        Transform stats = window.Find("Stats Panel");
        if (stats != null)
        {
            RectTransform stRt = stats.GetComponent<RectTransform>();
            stRt.sizeDelta = new Vector2(760f, 150f);
            stRt.anchoredPosition = new Vector2(0f, -315f);
            UnityEngine.UI.Image stImg = stats.GetComponent<UnityEngine.UI.Image>();
            if (stImg != null)
            {
                stImg.sprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
                stImg.type = UnityEngine.UI.Image.Type.Sliced;
                stImg.color = new Color(0.08f, 0.07f, 0.12f, 0.95f);
            }
        }

        // 3. Final Weapons Row inside window
        Transform wrT = window.Find("Final Weapons Row");
        GameObject wrGO = wrT != null ? wrT.gameObject : new GameObject("Final Weapons Row", typeof(RectTransform));
        if (wrT == null) wrGO.transform.SetParent(window, false);
        RectTransform wrRT = wrGO.GetComponent<RectTransform>();
        wrRT.anchorMin = new Vector2(0.5f, 0f);
        wrRT.anchorMax = new Vector2(0.5f, 0f);
        wrRT.pivot = new Vector2(0.5f, 0f);
        wrRT.anchoredPosition = new Vector2(0f, 130f);
        wrRT.sizeDelta = new Vector2(760f, 58f);

        System.Collections.Generic.List<Weapon> activeWeapons = new System.Collections.Generic.List<Weapon>();
        if (PlayerController.instance != null)
        {
            if (PlayerController.instance.assignedWeapons != null) activeWeapons.AddRange(PlayerController.instance.assignedWeapons);
            if (PlayerController.instance.fullyLevelledWeapons != null) activeWeapons.AddRange(PlayerController.instance.fullyLevelledWeapons);
        }

        Sprite slotSprite = Resources.Load<Sprite>("UI/VS_HUD_Slot");
        TMP_FontAsset retroFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        float slotSize = 52f;
        float spacing = 14f;
        int activeCount = Mathf.Max(activeWeapons.Count, 1);
        float startX = -((activeCount * slotSize + (activeCount - 1) * spacing) * 0.5f) + (slotSize * 0.5f);

        for (int i = 0; i < 6; i++)
        {
            string sName = "FinalSlot_" + i;
            Transform sT = wrGO.transform.Find(sName);
            GameObject sGO = sT != null ? sT.gameObject : new GameObject(sName, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            if (sT == null) sGO.transform.SetParent(wrGO.transform, false);
            RectTransform sRT = sGO.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(startX + i * (slotSize + spacing), 0f);
            sRT.sizeDelta = new Vector2(slotSize, slotSize);

            UnityEngine.UI.Image sImg = sGO.GetComponent<UnityEngine.UI.Image>();
            if (slotSprite != null) sImg.sprite = slotSprite;
            sImg.type = UnityEngine.UI.Image.Type.Sliced;
            sImg.color = Color.white;

            Transform icT = sGO.transform.Find("Icon");
            GameObject icGO = icT != null ? icT.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            if (icT == null) icGO.transform.SetParent(sGO.transform, false);
            RectTransform icRT = icGO.GetComponent<RectTransform>();
            icRT.anchorMin = Vector2.zero;
            icRT.anchorMax = Vector2.one;
            icRT.offsetMin = new Vector2(4f, 4f);
            icRT.offsetMax = new Vector2(-4f, -4f);
            UnityEngine.UI.Image icImg = icGO.GetComponent<UnityEngine.UI.Image>();
            icImg.preserveAspect = true;

            Transform lvlT = sGO.transform.Find("Level");
            GameObject lvlGO = lvlT != null ? lvlT.gameObject : new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (lvlT == null) lvlGO.transform.SetParent(sGO.transform, false);
            RectTransform lvlRT = lvlGO.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(0f, 0f);
            lvlRT.anchorMax = new Vector2(1f, 0f);
            lvlRT.pivot = new Vector2(0.5f, 1f);
            lvlRT.anchoredPosition = new Vector2(0f, -2f);
            lvlRT.sizeDelta = new Vector2(60f, 16f);
            TextMeshProUGUI lTMP = lvlGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) lTMP.font = retroFont;
            lTMP.fontSize = 14f;
            lTMP.alignment = TextAlignmentOptions.Center;

            if (i < activeWeapons.Count && activeWeapons[i] != null && activeWeapons[i].icon != null)
            {
                sGO.SetActive(true);
                icImg.sprite = activeWeapons[i].icon;
                icImg.enabled = true;
                int lvl = activeWeapons[i].weaponLevel + 1;
                bool isMax = activeWeapons[i].stats != null && lvl >= activeWeapons[i].stats.Count;
                lTMP.text = isMax ? "<color=#FF4444>MAX</color>" : $"<color=#FFE040>LV {lvl}</color>";
                lTMP.enabled = true;
            }
            else
            {
                sGO.SetActive(false);
            }
        }

        // 4. Buttons Styling with 9-Slice Royal Gothic Buttons
        Sprite emeraldButton = MainMenu.CreateRoyalGothicButton(
            new Color(0.04f, 0.18f, 0.09f),
            new Color(0.10f, 0.36f, 0.18f),
            new Color(0.88f, 0.74f, 0.25f),
            new Color(1.0f, 0.96f, 0.55f),
            new Color(0.20f, 0.98f, 0.45f)
        );

        Sprite crimsonButton = MainMenu.CreateRoyalGothicButton(
            new Color(0.22f, 0.05f, 0.07f),
            new Color(0.44f, 0.10f, 0.14f),
            new Color(0.82f, 0.62f, 0.20f),
            new Color(0.98f, 0.84f, 0.42f),
            new Color(0.95f, 0.22f, 0.22f)
        );

        Transform restart = window.Find("Restart");
        if (restart != null)
        {
            RectTransform rRT = (RectTransform)restart;
            rRT.sizeDelta = new Vector2(310f, 70f);
            rRT.anchoredPosition = new Vector2(-165f, 42f);
            UnityEngine.UI.Image rImg = restart.GetComponent<UnityEngine.UI.Image>();
            if (rImg != null) { rImg.sprite = emeraldButton; rImg.type = UnityEngine.UI.Image.Type.Sliced; rImg.color = Color.white; }
            TMP_Text rText = restart.GetComponentInChildren<TMP_Text>(true);
            if (rText != null) { rText.text = "✦  TRY AGAIN  ✦"; rText.color = new Color(0.94f, 1f, 0.95f); rText.fontSize = 32f; }
            if (restart.GetComponent<GothicMenuButtonEffects>() == null) restart.gameObject.AddComponent<GothicMenuButtonEffects>();
        }

        Transform menu = window.Find("Main Menu");
        if (menu != null)
        {
            RectTransform mRT = (RectTransform)menu;
            mRT.sizeDelta = new Vector2(310f, 70f);
            mRT.anchoredPosition = new Vector2(165f, 42f);
            UnityEngine.UI.Image mImg = menu.GetComponent<UnityEngine.UI.Image>();
            if (mImg != null) { mImg.sprite = crimsonButton; mImg.type = UnityEngine.UI.Image.Type.Sliced; mImg.color = Color.white; }
            TMP_Text mText = menu.GetComponentInChildren<TMP_Text>(true);
            if (mText != null) { mText.text = "✦  MAIN MENU  ✦"; mText.color = new Color(1f, 0.92f, 0.92f); mText.fontSize = 32f; }
            if (menu.GetComponent<GothicMenuButtonEffects>() == null) menu.gameObject.AddComponent<GothicMenuButtonEffects>();
        }
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

        int totalKills = UIController.instance != null ? UIController.instance.killCount : 0;
        for (float t = 0f; t < countDuration; t += Time.unscaledDeltaTime)
        {
            float p = EaseOutCubic(t / countDuration);
            SetStats(survivedTime * p, Mathf.RoundToInt(Mathf.Lerp(1f, level, p)), Mathf.RoundToInt(coins * p), Mathf.RoundToInt(totalKills * p));
            yield return null;
        }
        SetStats(survivedTime, level, coins, totalKills);

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

    void SetStats(float time, int level, int coins, int kills = -1)
    {
        if (timeText != null) timeText.text = FormatTime(time);
        if (levelText != null) levelText.text = level.ToString();
        if (coinsText != null) coinsText.text = coins.ToString();
        if (killsText != null)
        {
            int k = kills >= 0 ? kills : (UIController.instance != null ? UIController.instance.killCount : 0);
            killsText.text = k.ToString();
        }
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
