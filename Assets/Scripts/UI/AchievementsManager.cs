using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hệ thống Thành Tựu (Achievements) chuẩn Vampire Survivors
/// Ghi nhận cột mốc sinh tồn, quái tiêu diệt, boss hạ gục và thưởng vàng
/// </summary>
public class AchievementsManager : MonoBehaviour
{
    public static AchievementsManager instance;

    [System.Serializable]
    public class Achievement
    {
        public string id;
        public string title;
        public string description;
        public int targetValue;
        public int rewardCoins;
    }

    private static readonly List<Achievement> achievements = new List<Achievement>
    {
        new Achievement { id = "kill_1", title = "First Blood", description = "Defeat your first monster in battle.", targetValue = 1, rewardCoins = 50 },
        new Achievement { id = "kill_200", title = "Monster Hunter", description = "Slay 200 monsters across your runs.", targetValue = 200, rewardCoins = 150 },
        new Achievement { id = "kill_1000", title = "Extinction Event", description = "Slay 1,000 monsters.", targetValue = 1000, rewardCoins = 350 },
        new Achievement { id = "survive_3m", title = "Survivor Novice", description = "Survive for 3 minutes in a single battle.", targetValue = 180, rewardCoins = 100 },
        new Achievement { id = "survive_10m", title = "Night Stalker", description = "Survive for 10 minutes in a single battle.", targetValue = 600, rewardCoins = 300 },
        new Achievement { id = "level_10", title = "Mystic Ascension", description = "Reach Character Level 10.", targetValue = 10, rewardCoins = 150 },
        new Achievement { id = "level_25", title = "Supreme Master", description = "Reach Character Level 25 in a single run.", targetValue = 25, rewardCoins = 400 },
        new Achievement { id = "boss_slain", title = "Regicide", description = "Slay Brood Queen or any Stage Boss.", targetValue = 1, rewardCoins = 500 },
        new Achievement { id = "ultimate_cast", title = "Unleashed Fury", description = "Unleash your Character Ultimate Skill (Key E).", targetValue = 1, rewardCoins = 100 },
        new Achievement { id = "coins_500", title = "Gold Rush", description = "Amass 500 gold coins.", targetValue = 500, rewardCoins = 250 },
        new Achievement { id = "collect_10", title = "Avid Collector", description = "Discover at least 10 items in the Collection.", targetValue = 10, rewardCoins = 300 }
    };

    public static int GetCurrentValue(string id)
    {
        switch (id)
        {
            case "kill_1":
            case "kill_200":
            case "kill_1000":
                return PlayerPrefs.GetInt("TotalKills", 0);
            case "survive_3m":
            case "survive_10m":
                return Mathf.FloorToInt(PlayerPrefs.GetFloat("BestSurvivalTime", 0f));
            case "level_10":
            case "level_25":
                return PlayerPrefs.GetInt("BestPlayerLevel", 1);
            case "boss_slain":
                return PlayerPrefs.GetInt("TotalBossesSlain", 0);
            case "ultimate_cast":
                return PlayerPrefs.GetInt("TotalUltimatesCast", 0);
            case "coins_500":
                return PlayerPrefs.GetInt("Coins", 0);
            case "collect_10":
                return CollectionManager.GetUnlockedCount();
            default:
                return PlayerPrefs.GetInt("Ach_Prog_" + id, 0);
        }
    }

    public static bool IsUnlocked(string id)
    {
        return PlayerPrefs.GetInt("Ach_Completed_" + id, 0) == 1;
    }

    public static void UnlockAchievement(string id)
    {
        PlayerPrefs.SetInt("Ach_Completed_" + id, 1);
        PlayerPrefs.Save();
    }

    public static void OpenPanel(Transform canvasParent, Action onBack)
    {
        GameObject go = new GameObject("AchievementsScreenPanel", typeof(RectTransform));
        go.transform.SetParent(canvasParent, false);
        AchievementsManager am = go.AddComponent<AchievementsManager>();
        am.BuildUI(onBack);
    }

    private void BuildUI(Action onBack)
    {
        RectTransform rootRt = GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // 1. Backdrop
        GameObject bg = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(transform, false);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero;
        bgrt.offsetMax = Vector2.zero;
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0.04f, 0.02f, 0.05f, 0.92f);

        // 2. Top Coin Capsule
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (CoinController.instance != null) currentCoins = CoinController.instance.currentCoins;

        GameObject coinCap = new GameObject("TopCoinCapsule", typeof(RectTransform), typeof(Image));
        coinCap.transform.SetParent(transform, false);
        RectTransform ccrt = coinCap.GetComponent<RectTransform>();
        ccrt.anchorMin = new Vector2(0.5f, 1f);
        ccrt.anchorMax = new Vector2(0.5f, 1f);
        ccrt.pivot = new Vector2(0.5f, 1f);
        ccrt.anchoredPosition = new Vector2(0f, -18f);
        ccrt.sizeDelta = new Vector2(220f, 48f);
        Image ccImg = coinCap.GetComponent<Image>();
        ccImg.sprite = UIController.GetGothicBanishButtonSprite();
        ccImg.type = Image.Type.Sliced;
        ccImg.color = new Color(0.18f, 0.14f, 0.10f, 0.95f);

        GameObject coinTxtObj = new GameObject("CoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinTxtObj.transform.SetParent(coinCap.transform, false);
        RectTransform ctrt = coinTxtObj.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        TextMeshProUGUI ctTmp = coinTxtObj.GetComponent<TextMeshProUGUI>();
        ctTmp.text = $"💰 <color=#FFD700><b>{currentCoins}</b></color>";
        ctTmp.fontSize = 26f;
        ctTmp.alignment = TextAlignmentOptions.Center;

        // 3. Back Button
        GameObject backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backBtnObj.transform.SetParent(transform, false);
        RectTransform bbrt = backBtnObj.GetComponent<RectTransform>();
        bbrt.anchorMin = new Vector2(0.5f, 1f);
        bbrt.anchorMax = new Vector2(0.5f, 1f);
        bbrt.pivot = new Vector2(0.5f, 1f);
        bbrt.anchoredPosition = new Vector2(210f, -18f);
        bbrt.sizeDelta = new Vector2(140f, 48f);
        Image bbImg = backBtnObj.GetComponent<Image>();
        bbImg.sprite = UIController.GetGothicBanishButtonSprite();
        bbImg.type = Image.Type.Sliced;
        bbImg.color = Color.white;
        Button bBtn = backBtnObj.GetComponent<Button>();
        bBtn.onClick.AddListener(() =>
        {
            if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
            Destroy(gameObject);
            onBack?.Invoke();
        });

        GameObject bbTxtObj = new GameObject("BackText", typeof(RectTransform), typeof(TextMeshProUGUI));
        bbTxtObj.transform.SetParent(backBtnObj.transform, false);
        RectTransform bbtRt = bbTxtObj.GetComponent<RectTransform>();
        bbtRt.anchorMin = Vector2.zero;
        bbtRt.anchorMax = Vector2.one;
        TextMeshProUGUI bbTmp = bbTxtObj.GetComponent<TextMeshProUGUI>();
        bbTmp.text = "<b>BACK</b>";
        bbTmp.fontSize = 24f;
        bbTmp.alignment = TextAlignmentOptions.Center;
        bbTmp.color = new Color(1f, 0.94f, 0.88f);

        // 4. Main Window
        GameObject win = new GameObject("AchievementsWindow", typeof(RectTransform), typeof(Image));
        win.transform.SetParent(transform, false);
        RectTransform winRt = win.GetComponent<RectTransform>();
        winRt.anchorMin = new Vector2(0.5f, 0.5f);
        winRt.anchorMax = new Vector2(0.5f, 0.5f);
        winRt.pivot = new Vector2(0.5f, 0.5f);
        winRt.anchoredPosition = new Vector2(0f, -15f);
        winRt.sizeDelta = new Vector2(920f, 620f);
        Image winImg = win.GetComponent<Image>();
        winImg.sprite = UIController.GetMysticAscensionWindowSprite();
        winImg.type = Image.Type.Sliced;
        winImg.color = Color.white;

        // Title
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(win.transform, false);
        RectTransform trt = titleObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -22f);
        trt.sizeDelta = new Vector2(600f, 45f);
        TextMeshProUGUI tTmp = titleObj.GetComponent<TextMeshProUGUI>();
        tTmp.text = "✦  A C H I E V E M E N T S  ✦";
        tTmp.fontSize = 34f;
        tTmp.alignment = TextAlignmentOptions.Center;
        tTmp.color = new Color(1f, 0.88f, 0.32f);
        tTmp.characterSpacing = 3f;

        // Scroll View / List of Achievements
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(win.transform, false);
        RectTransform srt = scrollObj.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0.5f);
        srt.anchorMax = new Vector2(0.5f, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.anchoredPosition = new Vector2(0f, -28f);
        srt.sizeDelta = new Vector2(840f, 480f);

        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollObj.transform, false);
        RectTransform vprt = viewport.GetComponent<RectTransform>();
        vprt.anchorMin = Vector2.zero;
        vprt.anchorMax = Vector2.one;
        vprt.offsetMin = Vector2.zero;
        vprt.offsetMax = Vector2.zero;
        sr.viewport = vprt;

        // Content
        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0f, 1f);
        crt.anchorMax = new Vector2(1f, 1f);
        crt.pivot = new Vector2(0.5f, 1f);
        crt.sizeDelta = new Vector2(0f, 0f);
        sr.content = crt;

        VerticalLayoutGroup vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Populate Achievement Cards
        for (int i = 0; i < achievements.Count; i++)
        {
            Achievement ach = achievements[i];
            int currentVal = GetCurrentValue(ach.id);
            bool unlocked = IsUnlocked(ach.id) || currentVal >= ach.targetValue;
            if (unlocked && !IsUnlocked(ach.id)) UnlockAchievement(ach.id);

            GameObject card = new GameObject("AchCard_" + i, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(content.transform, false);
            RectTransform cardRt = card.GetComponent<RectTransform>();
            cardRt.sizeDelta = new Vector2(810f, 75f);
            Image cardImg = card.GetComponent<Image>();
            cardImg.sprite = LevelUpSellectionButton.GetGothicCardSprite(unlocked);
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;

            // Icon Trophy / Star
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(card.transform, false);
            RectTransform irt = iconObj.GetComponent<RectTransform>();
            irt.anchorMin = new Vector2(0f, 0.5f);
            irt.anchorMax = new Vector2(0f, 0.5f);
            irt.pivot = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = new Vector2(40f, 0f);
            irt.sizeDelta = new Vector2(46f, 46f);
            Image iImg = iconObj.GetComponent<Image>();
            iImg.sprite = LevelUpSellectionButton.GetPipDiamondSprite(unlocked);
            iImg.color = unlocked ? new Color(1f, 0.88f, 0.25f) : new Color(0.45f, 0.45f, 0.55f);

            // Title & Description
            GameObject textObj = new GameObject("TextHolder", typeof(RectTransform));
            textObj.transform.SetParent(card.transform, false);
            RectTransform tr = textObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0.11f, 0f);
            tr.anchorMax = new Vector2(0.72f, 1f);
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;

            GameObject titleT = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleT.transform.SetParent(textObj.transform, false);
            RectTransform titlert = titleT.GetComponent<RectTransform>();
            titlert.anchorMin = new Vector2(0f, 0.52f);
            titlert.anchorMax = new Vector2(1f, 0.95f);
            titlert.offsetMin = Vector2.zero;
            titlert.offsetMax = Vector2.zero;
            TextMeshProUGUI titleTmp = titleT.GetComponent<TextMeshProUGUI>();
            titleTmp.text = unlocked ? $"<color=#FFD700><b>{ach.title}</b></color>" : $"<color=#A0AAB8><b>{ach.title}</b></color>";
            titleTmp.fontSize = 22f;

            GameObject descT = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            descT.transform.SetParent(textObj.transform, false);
            RectTransform descrt = descT.GetComponent<RectTransform>();
            descrt.anchorMin = new Vector2(0f, 0.05f);
            descrt.anchorMax = new Vector2(1f, 0.52f);
            descrt.offsetMin = Vector2.zero;
            descrt.offsetMax = Vector2.zero;
            TextMeshProUGUI descTmp = descT.GetComponent<TextMeshProUGUI>();
            descTmp.text = ach.description;
            descTmp.fontSize = 16f;
            descTmp.color = new Color(0.85f, 0.85f, 0.9f);

            // Status Badge / Reward
            GameObject badgeObj = new GameObject("Badge", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeObj.transform.SetParent(card.transform, false);
            RectTransform bdrt = badgeObj.GetComponent<RectTransform>();
            bdrt.anchorMin = new Vector2(0.74f, 0f);
            bdrt.anchorMax = new Vector2(0.98f, 1f);
            bdrt.offsetMin = Vector2.zero;
            bdrt.offsetMax = Vector2.zero;
            TextMeshProUGUI bdTmp = badgeObj.GetComponent<TextMeshProUGUI>();
            bdTmp.alignment = TextAlignmentOptions.MidlineRight;

            if (unlocked)
            {
                bdTmp.text = $"<color=#00FFAA><b>✓ COMPLETED</b></color>\n<size=80%><color=#FFD700>+{ach.rewardCoins} COINS</color></size>";
            }
            else
            {
                int showVal = Mathf.Min(currentVal, ach.targetValue);
                bdTmp.text = $"<color=#FFAA00>[ {showVal} / {ach.targetValue} ]</color>\n<size=80%><color=#FFD700>+{ach.rewardCoins} COINS</color></size>";
            }
        }
    }
}
