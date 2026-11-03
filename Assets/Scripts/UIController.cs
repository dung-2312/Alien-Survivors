using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using AlienSurvivors.Upgrades;

public class UIController : MonoBehaviour
{
    public static UIController instance; // Create a singleton instance of the UIController. GK

    public void Awake()
    {
        instance = this; // Set the instance to this object. GK
        ApplyVampireSurvivorsHUDTheme();
    }

    public Slider explvlSlider; // The experience level slider. GK
    public TMP_Text explvlText; // The experience level text. GK

    public LevelUpSellectionButton[] levelUpButtons; // The level up buttons. GK

    public GameObject levelUpPanel; // The level up panel. GK
    public TMP_Text coinText; // The coin text. D
    public GameObject levelEndScreen; // The level end script. GK
    public TMP_Text endTimeText; // The end time text. GK

    public PlayerStatUpgradeDisplay moveSpeedUpgradeDisplay,
        healthUpgradeDisplay,
        pickupRangeUpgradeDisplay,
        maxWeaponsUpgradeDisplay; // The player stat upgrade display. GK

    public TMP_Text timeText; // The time text. GK
    public string mainMenuName; // The main menu scene. GK

    public GameObject pauseScreen; // The pause screen. GK

    [Header("Vampire Survivors HUD Theme")]
    public TMP_FontAsset retroFont;
    public Image coinImage;
    public TMP_Text killText;
    public Image killImage;
    [HideInInspector]
    public int killCount = 0;
    private TMP_Text dashStatusText;
    private TMP_Text ultStatusText;
    private Image ultWidgetBg;

    private List<Image> weaponSlotIcons = new List<Image>();
    private List<TextMeshProUGUI> weaponSlotLevels = new List<TextMeshProUGUI>();
    private List<Image> passiveSlotIcons = new List<Image>();
    private List<TextMeshProUGUI> passiveSlotLevels = new List<TextMeshProUGUI>();
    private int lastWeaponCount = -1;
    private int lastPassiveHash = -1;
    private float targetExpValue = 0f;
    private int currentDisplayLevel = 1;
    private TMP_Text stageSubtitleText;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<TreasureChestUI>() == null) gameObject.AddComponent<TreasureChestUI>();
        if (ComboManager.instance == null) new GameObject("ComboManager", typeof(ComboManager));
        if (ShrineManager.instance == null) new GameObject("ShrineManager", typeof(ShrineManager));
        if (EnvironmentDecorManager.instance == null) new GameObject("EnvironmentDecorManager", typeof(EnvironmentDecorManager));
        ApplySciFiLevelUpTheme();

        if (RunSave.continueRequested) // Came from the Main Menu's Continue button
        {
            RunSave.continueRequested = false;
            RunSaveData saved = RunSave.Load();
            if (saved != null) StartCoroutine(RunSave.Restore(saved));
        }
    }

    // Vampire-Survivors style level-up: ornate gold-framed window, iconic "LEVEL UP!" title with drop shadow,
    // subtitle, three wide cards with level pips, and a gothic red BANISH (SKIP) button.
    public void ApplySciFiLevelUpTheme()
    {
        if (levelUpPanel != null)
        {
            RectTransform panelRt = levelUpPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; // Backdrop covers the whole screen.
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Image panelImg = levelUpPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = null;
                panelImg.color = new Color(0.02f, 0.02f, 0.05f, 0.85f); // Dim the game behind the window
            }

            // Gold-framed window behind everything
            Transform windowT = levelUpPanel.transform.Find("Level Up Window");
            if (windowT == null)
            {
                GameObject w = new GameObject("Level Up Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                w.transform.SetParent(levelUpPanel.transform, false);
                windowT = w.transform;
            }
            windowT.SetAsFirstSibling();
            RectTransform windowRt = (RectTransform)windowT;
            windowRt.anchorMin = windowRt.anchorMax = windowRt.pivot = new Vector2(0.5f, 0.5f);
            windowRt.anchoredPosition = Vector2.zero;
            windowRt.sizeDelta = new Vector2(940f, 820f);
            Image windowImg = windowT.GetComponent<Image>();
            windowImg.sprite = GetGothicWindowSprite();
            windowImg.type = Image.Type.Sliced;
            windowImg.color = Color.white;

            TMP_Text[] allTexts = levelUpPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in allTexts)
            {
                if (t.name == "Upgrade Text")
                {
                    t.text = "✦  M Y S T I C   A S C E N S I O N  ✦";
                    t.fontSize = 50;
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.characterSpacing = 4f;
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.alignment = TextAlignmentOptions.Center;
                    t.color = new Color(1f, 0.88f, 0.32f);
                    t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    t.rectTransform.sizeDelta = new Vector2(880f, 70f);
                    t.rectTransform.anchoredPosition = new Vector2(0f, 342f);
                }
                else if (t.transform.parent != null && t.transform.parent.name == "Skip Level Up")
                {
                    t.text = "⚔  BANISH (SKIP)  ⚔";
                    t.fontSize = 26;
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.color = new Color(1f, 0.94f, 0.90f);
                    t.alignment = TextAlignmentOptions.Center;
                }
            }

            // Subtitle "CHOOSE YOUR SACRED BLESSING"
            Transform subT = levelUpPanel.transform.Find("Level Up Subtitle");
            GameObject subGO = subT != null ? subT.gameObject : new GameObject("Level Up Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (subT == null) subGO.transform.SetParent(levelUpPanel.transform, false);
            RectTransform subRT = subGO.GetComponent<RectTransform>();
            subRT.anchorMin = subRT.anchorMax = subRT.pivot = new Vector2(0.5f, 0.5f);
            subRT.sizeDelta = new Vector2(740f, 32f);
            subRT.anchoredPosition = new Vector2(0f, 286f);
            TextMeshProUGUI subTMP = subGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) subTMP.font = retroFont;
            subTMP.text = "✦  CHOOSE YOUR SACRED BLESSING  ✦";
            subTMP.fontSize = 22f;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.color = new Color(0.92f, 0.84f, 0.60f, 0.95f);
            subTMP.characterSpacing = 3f;
            subTMP.raycastTarget = false;

            // Gothic Blood Crimson Skip button at the bottom of the window
            Transform skip = levelUpPanel.transform.Find("Skip Level Up");
            if (skip != null)
            {
                RectTransform skipRt = (RectTransform)skip;
                skipRt.anchorMin = skipRt.anchorMax = skipRt.pivot = new Vector2(0.5f, 0.5f);
                skipRt.sizeDelta = new Vector2(300f, 62f);
                skipRt.anchoredPosition = new Vector2(0f, -342f);
                Image skipImg = skip.GetComponent<Image>();
                if (skipImg != null)
                {
                    skipImg.sprite = GetGothicBanishButtonSprite();
                    skipImg.type = Image.Type.Sliced;
                    skipImg.color = Color.white;
                }
            }
        }

        // The old stat shop buttons are not part of this screen
        if (moveSpeedUpgradeDisplay != null) moveSpeedUpgradeDisplay.gameObject.SetActive(false);
        if (healthUpgradeDisplay != null) healthUpgradeDisplay.gameObject.SetActive(false);
        if (pickupRangeUpgradeDisplay != null) pickupRangeUpgradeDisplay.gameObject.SetActive(false);
        if (maxWeaponsUpgradeDisplay != null) maxWeaponsUpgradeDisplay.gameObject.SetActive(false);

        // Three wide rows stacked inside the window
        if (levelUpButtons != null)
        {
            for (int i = 0; i < levelUpButtons.Length && i < 3; i++)
            {
                if (levelUpButtons[i] != null)
                    levelUpButtons[i].SetLayout(new Vector2(840f, 162f), new Vector2(0f, 155f - i * 175f));
            }
        }
    }

    public void ApplyVampireSurvivorsHUDTheme()
    {
        // 1. Resolve Retro Pixel Font (prefer Outline font)
        if (retroFont == null)
        {
            TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in allFonts)
            {
                if (f != null && f.name.Contains("Kenney Pixel SDF - Outline"))
                {
                    retroFont = f;
                    break;
                }
            }
            if (retroFont == null)
            {
                foreach (var f in allFonts)
                {
                    if (f != null && f.name.Contains("Kenney Pixel"))
                    {
                        retroFont = f;
                        break;
                    }
                }
            }
        }
        if (retroFont == null && timeText != null)
        {
            retroFont = timeText.font;
        }

        // 2. Load custom Vampire Survivors pixel sprites from Resources
        Sprite expFrame = Resources.Load<Sprite>("UI/VS_ExpBar_Frame");
        Sprite expFill = Resources.Load<Sprite>("UI/VS_ExpBar_Fill");
        Sprite hudSlot = Resources.Load<Sprite>("UI/VS_HUD_Slot");
        Sprite skullIcon = Resources.Load<Sprite>("UI/VS_Icon_Skull");
        Sprite coinIcon = Resources.Load<Sprite>("UI/UI_Coin_Icon");

        Transform canvasT = explvlSlider != null ? explvlSlider.transform.parent : transform;

        // 3. Configure Full-Width Top Experience Bar
        if (explvlSlider != null)
        {
            RectTransform sliderRT = explvlSlider.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0f, 1f);
            sliderRT.anchorMax = new Vector2(1f, 1f);
            sliderRT.pivot = new Vector2(0.5f, 1f);
            sliderRT.offsetMin = new Vector2(0f, -22f);
            sliderRT.offsetMax = new Vector2(0f, 0f);

            Transform bgT = explvlSlider.transform.Find("Background");
            if (bgT != null)
            {
                Image bgImg = bgT.GetComponent<Image>();
                if (bgImg != null)
                {
                    if (expFrame != null) bgImg.sprite = expFrame;
                    bgImg.type = Image.Type.Sliced;
                    bgImg.color = Color.white;
                }
                RectTransform bgRT = bgT.GetComponent<RectTransform>();
                bgRT.anchorMin = Vector2.zero;
                bgRT.anchorMax = Vector2.one;
                bgRT.offsetMin = Vector2.zero;
                bgRT.offsetMax = Vector2.zero;
            }

            if (explvlSlider.fillRect != null)
            {
                Image fillImg = explvlSlider.fillRect.GetComponent<Image>();
                if (fillImg != null)
                {
                    if (expFill != null) fillImg.sprite = expFill;
                    fillImg.type = Image.Type.Sliced;
                    fillImg.color = new Color(0.15f, 0.95f, 0.85f, 1f); // Thanh EXP xanh neon ngọc bích rực sáng
                }
                Transform fillAreaT = explvlSlider.fillRect.parent;
                if (fillAreaT != null)
                {
                    RectTransform fillAreaRT = fillAreaT.GetComponent<RectTransform>();
                    fillAreaRT.anchorMin = Vector2.zero;
                    fillAreaRT.anchorMax = Vector2.one;
                    fillAreaRT.offsetMin = new Vector2(2f, 2f);
                    fillAreaRT.offsetMax = new Vector2(-2f, -2f);
                }
            }
        }

        // 4. Experience Level Text ("LV 1" positioned inside the top bar on the far right)
        if (explvlText != null)
        {
            explvlText.transform.SetParent(explvlSlider != null ? explvlSlider.transform : canvasT, false);
            explvlText.transform.SetAsLastSibling();

            RectTransform lvlRT = explvlText.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(1f, 0.5f);
            lvlRT.anchorMax = new Vector2(1f, 0.5f);
            lvlRT.pivot = new Vector2(1f, 0.5f);
            lvlRT.anchoredPosition = new Vector2(-10f, 0f);
            lvlRT.sizeDelta = new Vector2(110f, 22f);

            if (retroFont != null) explvlText.font = retroFont;
            explvlText.fontSize = 24;
            explvlText.alignment = TextAlignmentOptions.Right;
            explvlText.color = new Color(1f, 0.92f, 0.35f); // Hoàng kim óng ánh
            explvlText.fontStyle = FontStyles.Bold;
            explvlText.enableAutoSizing = false;
            explvlText.margin = Vector4.zero;
            explvlText.text = "LV " + (ExperienceLevelController.instance != null ? ExperienceLevelController.instance.currentLevel : 1);
        }

        // 5. Timer Text ("0:05" centered on HUD row with Ornate Placard)
        Transform existingPlacard = canvasT.Find("HUD Timer Placard");
        GameObject placardGO = existingPlacard != null ? existingPlacard.gameObject : new GameObject("HUD Timer Placard", typeof(RectTransform), typeof(Image));
        if (existingPlacard == null) placardGO.transform.SetParent(canvasT, false);

        RectTransform plRT = placardGO.GetComponent<RectTransform>();
        plRT.anchorMin = new Vector2(0.5f, 1f);
        plRT.anchorMax = new Vector2(0.5f, 1f);
        plRT.pivot = new Vector2(0.5f, 0.5f);
        plRT.anchoredPosition = new Vector2(0f, -54f);
        plRT.sizeDelta = new Vector2(170f, 42f);

        Image plImg = placardGO.GetComponent<Image>();
        plImg.sprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold") ?? hudSlot;
        plImg.type = Image.Type.Sliced;
        plImg.color = new Color(0.08f, 0.08f, 0.14f, 0.88f);

        if (timeText != null)
        {
            placardGO.transform.SetSiblingIndex(Mathf.Max(0, timeText.transform.GetSiblingIndex()));
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.5f, 1f);
            timeRT.anchorMax = new Vector2(0.5f, 1f);
            timeRT.pivot = new Vector2(0.5f, 0.5f);
            timeRT.anchoredPosition = new Vector2(0f, -54f);
            timeRT.sizeDelta = new Vector2(160f, 38f);

            if (retroFont != null) timeText.font = retroFont;
            timeText.fontSize = 42;
            timeText.alignment = TextAlignmentOptions.Center;
            timeText.color = new Color(1f, 0.96f, 0.70f); // Ánh kim ấm áp
            timeText.fontStyle = FontStyles.Bold;
            timeText.enableAutoSizing = false;
            timeText.margin = Vector4.zero;
            timeText.text = "0:00";
        }

        // 5b. Stage Name Subtitle ribbon under the timer
        Transform existingStageSub = canvasT.Find("HUD Stage Subtitle");
        GameObject stageSubGO = existingStageSub != null ? existingStageSub.gameObject : new GameObject("HUD Stage Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (existingStageSub == null) stageSubGO.transform.SetParent(canvasT, false);

        RectTransform stageRT = stageSubGO.GetComponent<RectTransform>();
        stageRT.anchorMin = new Vector2(0.5f, 1f);
        stageRT.anchorMax = new Vector2(0.5f, 1f);
        stageRT.pivot = new Vector2(0.5f, 0.5f);
        stageRT.anchoredPosition = new Vector2(0f, -80f);
        stageRT.sizeDelta = new Vector2(500f, 24f);

        stageSubtitleText = stageSubGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) stageSubtitleText.font = retroFont;
        stageSubtitleText.fontSize = 20;
        stageSubtitleText.alignment = TextAlignmentOptions.Center;
        stageSubtitleText.fontStyle = FontStyles.Bold;
        stageSubtitleText.raycastTarget = false;
        stageSubtitleText.enableAutoSizing = false;
        stageSubtitleText.text = "<color=#88FF88>✦ STAGE I: THE TOXIC MEADOW ✦</color>";

        // 6. Retro Pause Button ("||" on far right of HUD row)
        Transform existingPauseBtn = canvasT.Find("HUD Pause Button");
        GameObject pauseBtnGO;
        if (existingPauseBtn == null)
        {
            pauseBtnGO = new GameObject("HUD Pause Button", typeof(RectTransform), typeof(Button), typeof(Image));
            pauseBtnGO.transform.SetParent(canvasT, false);
        }
        else
        {
            pauseBtnGO = existingPauseBtn.gameObject;
        }

        RectTransform pbRT = pauseBtnGO.GetComponent<RectTransform>();
        pbRT.anchorMin = new Vector2(1f, 1f);
        pbRT.anchorMax = new Vector2(1f, 1f);
        pbRT.pivot = new Vector2(1f, 0.5f);
        pbRT.anchoredPosition = new Vector2(-12f, -54f);
        pbRT.sizeDelta = new Vector2(26f, 28f);

        Image pbImg = pauseBtnGO.GetComponent<Image>();
        if (hudSlot != null) pbImg.sprite = hudSlot;
        pbImg.type = Image.Type.Sliced;
        pbImg.color = new Color(1f, 1f, 1f, 0.35f);

        Button pbBtn = pauseBtnGO.GetComponent<Button>();
        pbBtn.onClick.RemoveAllListeners();
        pbBtn.onClick.AddListener(PauseUnpause);

        Transform pbTextChild = pauseBtnGO.transform.Find("Pause Icon Text");
        GameObject pbTextGO = pbTextChild != null ? pbTextChild.gameObject : new GameObject("Pause Icon Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (pbTextChild == null) pbTextGO.transform.SetParent(pauseBtnGO.transform, false);

        TextMeshProUGUI pbTMP = pbTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) pbTMP.font = retroFont;
        pbTMP.text = "||";
        pbTMP.fontSize = 18;
        pbTMP.alignment = TextAlignmentOptions.Center;
        pbTMP.color = new Color(0.95f, 0.88f, 0.65f, 0.85f);

        RectTransform pbtRT = pbTextGO.GetComponent<RectTransform>();
        pbtRT.anchorMin = Vector2.zero;
        pbtRT.anchorMax = Vector2.one;
        pbtRT.offsetMin = Vector2.zero;
        pbtRT.offsetMax = Vector2.zero;

        // 7. Coin Display (Icon + Number to the left of Pause Button with Golden Capsule)
        if (coinImage == null && coinText != null)
        {
            Transform imgT = coinText.transform.parent.Find("Coin Image");
            if (imgT != null) coinImage = imgT.GetComponent<Image>();
        }

        Transform existingCoinCap = canvasT.Find("HUD Coin Capsule");
        GameObject coinCapGO = existingCoinCap != null ? existingCoinCap.gameObject : new GameObject("HUD Coin Capsule", typeof(RectTransform), typeof(Image));
        if (existingCoinCap == null) coinCapGO.transform.SetParent(canvasT, false);

        RectTransform ccRT = coinCapGO.GetComponent<RectTransform>();
        ccRT.anchorMin = new Vector2(1f, 1f);
        ccRT.anchorMax = new Vector2(1f, 1f);
        ccRT.pivot = new Vector2(1f, 0.5f);
        ccRT.anchoredPosition = new Vector2(-46f, -54f);
        ccRT.sizeDelta = new Vector2(125f, 36f);

        Image ccImg = coinCapGO.GetComponent<Image>();
        ccImg.sprite = Resources.Load<Sprite>("UI/VS_Coin_Capsule_Bar") ?? Resources.Load<Sprite>("UI/UI_Coin_Box");
        ccImg.type = Image.Type.Sliced;
        ccImg.color = new Color(0.95f, 0.75f, 0.15f, 0.92f); // Thỏi vàng hoàng gia

        if (coinImage != null)
        {
            coinCapGO.transform.SetSiblingIndex(Mathf.Max(0, coinImage.transform.GetSiblingIndex()));
            RectTransform cImgRT = coinImage.GetComponent<RectTransform>();
            cImgRT.anchorMin = new Vector2(1f, 1f);
            cImgRT.anchorMax = new Vector2(1f, 1f);
            cImgRT.pivot = new Vector2(1f, 0.5f);
            cImgRT.anchoredPosition = new Vector2(-52f, -54f);
            cImgRT.sizeDelta = new Vector2(22f, 22f);

            if (coinIcon != null) coinImage.sprite = coinIcon;
            coinImage.preserveAspect = true;
            coinImage.color = Color.white;
        }

        if (coinText != null)
        {
            RectTransform cTextRT = coinText.GetComponent<RectTransform>();
            cTextRT.anchorMin = new Vector2(1f, 1f);
            cTextRT.anchorMax = new Vector2(1f, 1f);
            cTextRT.pivot = new Vector2(1f, 0.5f);
            cTextRT.anchoredPosition = new Vector2(-80f, -54f);
            cTextRT.sizeDelta = new Vector2(85f, 30f);

            if (retroFont != null) coinText.font = retroFont;
            coinText.fontSize = 30;
            coinText.alignment = TextAlignmentOptions.Right;
            coinText.color = new Color(1f, 0.98f, 0.55f);
            coinText.fontStyle = FontStyles.Bold;
            coinText.enableAutoSizing = false;
            coinText.margin = Vector4.zero;
            coinText.text = CoinController.instance != null ? CoinController.instance.currentCoins.ToString() : "0";
        }

        // 8. Monster Kill Counter (Skull Icon + Number with Crimson Gem Capsule)
        Transform existingKillCounter = canvasT.Find("Kill Counter Container");
        GameObject killContainer;
        if (existingKillCounter == null)
        {
            killContainer = new GameObject("Kill Counter Container", typeof(RectTransform), typeof(Image));
            killContainer.transform.SetParent(canvasT, false);
        }
        else
        {
            killContainer = existingKillCounter.gameObject;
        }

        RectTransform kcRT = killContainer.GetComponent<RectTransform>();
        kcRT.anchorMin = new Vector2(1f, 1f);
        kcRT.anchorMax = new Vector2(1f, 1f);
        kcRT.pivot = new Vector2(1f, 0.5f);
        kcRT.anchoredPosition = new Vector2(-180f, -54f);
        kcRT.sizeDelta = new Vector2(125f, 36f);

        Image kcImg = killContainer.GetComponent<Image>();
        if (kcImg == null) kcImg = killContainer.AddComponent<Image>();
        kcImg.sprite = Resources.Load<Sprite>("UI/VS_Coin_Capsule_Bar") ?? Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        kcImg.type = Image.Type.Sliced;
        kcImg.color = new Color(0.70f, 0.12f, 0.20f, 0.92f); // Huyết ngọc đỏ thắm

        Transform skullChild = killContainer.transform.Find("Skull Icon");
        GameObject skullGO = skullChild != null ? skullChild.gameObject : new GameObject("Skull Icon", typeof(RectTransform), typeof(Image));
        if (skullChild == null) skullGO.transform.SetParent(killContainer.transform, false);

        killImage = skullGO.GetComponent<Image>();
        if (skullIcon != null) killImage.sprite = skullIcon;
        killImage.preserveAspect = true;
        killImage.color = new Color(1f, 0.95f, 0.95f);
        RectTransform skullRT = skullGO.GetComponent<RectTransform>();
        skullRT.anchorMin = new Vector2(1f, 0.5f);
        skullRT.anchorMax = new Vector2(1f, 0.5f);
        skullRT.pivot = new Vector2(1f, 0.5f);
        skullRT.anchoredPosition = new Vector2(-6f, 0f);
        skullRT.sizeDelta = new Vector2(22f, 22f);

        Transform killTextChild = killContainer.transform.Find("Kill Text");
        GameObject killTextGO = killTextChild != null ? killTextChild.gameObject : new GameObject("Kill Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (killTextChild == null) killTextGO.transform.SetParent(killContainer.transform, false);

        killText = killTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) killText.font = retroFont;
        killText.fontSize = 30;
        killText.alignment = TextAlignmentOptions.Right;
        killText.color = new Color(1f, 0.85f, 0.85f);
        killText.fontStyle = FontStyles.Bold;
        killText.enableAutoSizing = false;
        killText.margin = Vector4.zero;
        killText.text = killCount.ToString();

        RectTransform killTextRT = killTextGO.GetComponent<RectTransform>();
        killTextRT.anchorMin = Vector2.zero;
        killTextRT.anchorMax = Vector2.one;
        killTextRT.offsetMin = Vector2.zero;
        killTextRT.offsetMax = new Vector2(-32f, 0f);

        // 9A. Row 1: Weapon Inventory Slots (Top-Left: 6 framed slots with level badge)
        Transform existingSlotsContainer = canvasT.Find("Weapon Slots Container");
        GameObject slotsContainer;
        if (existingSlotsContainer == null)
        {
            slotsContainer = new GameObject("Weapon Slots Container", typeof(RectTransform));
            slotsContainer.transform.SetParent(canvasT, false);
        }
        else
        {
            slotsContainer = existingSlotsContainer.gameObject;
        }

        RectTransform scRT = slotsContainer.GetComponent<RectTransform>();
        scRT.anchorMin = new Vector2(0f, 1f);
        scRT.anchorMax = new Vector2(0f, 1f);
        scRT.pivot = new Vector2(0f, 0.5f);
        scRT.anchoredPosition = new Vector2(12f, -44f);
        scRT.sizeDelta = new Vector2(212f, 32f);

        weaponSlotIcons.Clear();
        weaponSlotLevels.Clear();
        int maxSlots = 6;
        float slotSize = 32f;
        float slotSpacing = 4f;

        for (int i = 0; i < maxSlots; i++)
        {
            string slotName = "WeaponSlot_" + i;
            Transform slotChild = slotsContainer.transform.Find(slotName);
            GameObject slotGO;
            if (slotChild == null)
            {
                slotGO = new GameObject(slotName, typeof(RectTransform), typeof(Image));
                slotGO.transform.SetParent(slotsContainer.transform, false);
            }
            else
            {
                slotGO = slotChild.gameObject;
            }

            RectTransform slotRT = slotGO.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0f, 0.5f);
            slotRT.anchorMax = new Vector2(0f, 0.5f);
            slotRT.pivot = new Vector2(0f, 0.5f);
            slotRT.anchoredPosition = new Vector2(i * (slotSize + slotSpacing), 0f);
            slotRT.sizeDelta = new Vector2(slotSize, slotSize);

            Image slotFrame = slotGO.GetComponent<Image>();
            if (hudSlot != null) slotFrame.sprite = hudSlot;
            slotFrame.type = Image.Type.Sliced;
            slotFrame.color = new Color(1f, 1f, 1f, 0.65f);

            Transform iconChild = slotGO.transform.Find("Icon");
            GameObject iconGO;
            if (iconChild == null)
            {
                iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(slotGO.transform, false);
            }
            else
            {
                iconGO = iconChild.gameObject;
            }

            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = new Vector2(24f, 24f);

            Image iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;
            weaponSlotIcons.Add(iconImg);

            // Level text at bottom-right corner of slot
            Transform lvlChild = slotGO.transform.Find("LevelText");
            GameObject lvlGO = lvlChild != null ? lvlChild.gameObject : new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (lvlChild == null) lvlGO.transform.SetParent(slotGO.transform, false);
            RectTransform lvlRT = lvlGO.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(1f, 0f);
            lvlRT.anchorMax = new Vector2(1f, 0f);
            lvlRT.pivot = new Vector2(1f, 0f);
            lvlRT.anchoredPosition = new Vector2(1f, -1f);
            lvlRT.sizeDelta = new Vector2(20f, 16f);
            TextMeshProUGUI lvlTMP = lvlGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) lvlTMP.font = retroFont;
            lvlTMP.fontSize = 14f;
            lvlTMP.alignment = TextAlignmentOptions.BottomRight;
            lvlTMP.color = new Color(1f, 0.9f, 0.3f);
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.enabled = false;
            lvlTMP.raycastTarget = false;
            weaponSlotLevels.Add(lvlTMP);
        }

        // 9B. Row 2: Passive Item Inventory Slots (Directly under Weapons, matching Vampire Survivors)
        Transform existingPassivesContainer = canvasT.Find("Passive Slots Container");
        GameObject passivesContainer;
        if (existingPassivesContainer == null)
        {
            passivesContainer = new GameObject("Passive Slots Container", typeof(RectTransform));
            passivesContainer.transform.SetParent(canvasT, false);
        }
        else
        {
            passivesContainer = existingPassivesContainer.gameObject;
        }

        RectTransform pcRT = passivesContainer.GetComponent<RectTransform>();
        pcRT.anchorMin = new Vector2(0f, 1f);
        pcRT.anchorMax = new Vector2(0f, 1f);
        pcRT.pivot = new Vector2(0f, 0.5f);
        pcRT.anchoredPosition = new Vector2(12f, -80f);
        pcRT.sizeDelta = new Vector2(212f, 32f);

        passiveSlotIcons.Clear();
        passiveSlotLevels.Clear();

        for (int i = 0; i < maxSlots; i++)
        {
            string slotName = "PassiveSlot_" + i;
            Transform slotChild = passivesContainer.transform.Find(slotName);
            GameObject slotGO;
            if (slotChild == null)
            {
                slotGO = new GameObject(slotName, typeof(RectTransform), typeof(Image));
                slotGO.transform.SetParent(passivesContainer.transform, false);
            }
            else
            {
                slotGO = slotChild.gameObject;
            }

            RectTransform slotRT = slotGO.GetComponent<RectTransform>();
            slotRT.anchorMin = new Vector2(0f, 0.5f);
            slotRT.anchorMax = new Vector2(0f, 0.5f);
            slotRT.pivot = new Vector2(0f, 0.5f);
            slotRT.anchoredPosition = new Vector2(i * (slotSize + slotSpacing), 0f);
            slotRT.sizeDelta = new Vector2(slotSize, slotSize);

            Image slotFrame = slotGO.GetComponent<Image>();
            if (hudSlot != null) slotFrame.sprite = hudSlot;
            slotFrame.type = Image.Type.Sliced;
            slotFrame.color = new Color(0.9f, 0.95f, 1f, 0.55f);

            Transform iconChild = slotGO.transform.Find("Icon");
            GameObject iconGO;
            if (iconChild == null)
            {
                iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(slotGO.transform, false);
            }
            else
            {
                iconGO = iconChild.gameObject;
            }

            RectTransform iconRT = iconGO.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.pivot = new Vector2(0.5f, 0.5f);
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = new Vector2(24f, 24f);

            Image iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;
            passiveSlotIcons.Add(iconImg);

            // Level text at bottom-right corner of passive slot
            Transform lvlChild = slotGO.transform.Find("LevelText");
            GameObject lvlGO = lvlChild != null ? lvlChild.gameObject : new GameObject("LevelText", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (lvlChild == null) lvlGO.transform.SetParent(slotGO.transform, false);
            RectTransform lvlRT = lvlGO.GetComponent<RectTransform>();
            lvlRT.anchorMin = new Vector2(1f, 0f);
            lvlRT.anchorMax = new Vector2(1f, 0f);
            lvlRT.pivot = new Vector2(1f, 0f);
            lvlRT.anchoredPosition = new Vector2(1f, -1f);
            lvlRT.sizeDelta = new Vector2(20f, 16f);
            TextMeshProUGUI lvlTMP = lvlGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) lvlTMP.font = retroFont;
            lvlTMP.fontSize = 14f;
            lvlTMP.alignment = TextAlignmentOptions.BottomRight;
            lvlTMP.color = new Color(0.4f, 1f, 0.7f);
            lvlTMP.fontStyle = FontStyles.Bold;
            lvlTMP.enabled = false;
            lvlTMP.raycastTarget = false;
            passiveSlotLevels.Add(lvlTMP);
        }

        // 10A. Sleek Dash Ability Pill (Placed neatly under the inventory)
        Transform existingDashWidget = canvasT.Find("HUD Dash Widget");
        GameObject dashWidgetGO;
        if (existingDashWidget == null)
        {
            dashWidgetGO = new GameObject("HUD Dash Widget", typeof(RectTransform), typeof(Image));
            dashWidgetGO.transform.SetParent(canvasT, false);
        }
        else
        {
            dashWidgetGO = existingDashWidget.gameObject;
        }

        RectTransform dwRT = dashWidgetGO.GetComponent<RectTransform>();
        dwRT.anchorMin = new Vector2(0f, 1f);
        dwRT.anchorMax = new Vector2(0f, 1f);
        dwRT.pivot = new Vector2(0f, 0.5f);
        dwRT.anchoredPosition = new Vector2(12f, -114f);
        dwRT.sizeDelta = new Vector2(165f, 26f);

        Image dwImg = dashWidgetGO.GetComponent<Image>();
        if (hudSlot != null) dwImg.sprite = hudSlot;
        dwImg.type = Image.Type.Sliced;
        dwImg.color = new Color(0.08f, 0.12f, 0.18f, 0.85f);

        Transform dwTextChild = dashWidgetGO.transform.Find("Dash Text");
        GameObject dwTextGO = dwTextChild != null ? dwTextChild.gameObject : new GameObject("Dash Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (dwTextChild == null) dwTextGO.transform.SetParent(dashWidgetGO.transform, false);

        dashStatusText = dwTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) dashStatusText.font = retroFont;
        dashStatusText.fontSize = 15;
        dashStatusText.alignment = TextAlignmentOptions.Center;
        dashStatusText.color = new Color(0.2f, 1f, 0.6f);
        dashStatusText.text = "SPACE: READY";

        RectTransform dwtRT = dwTextGO.GetComponent<RectTransform>();
        dwtRT.anchorMin = Vector2.zero;
        dwtRT.anchorMax = Vector2.one;
        dwtRT.offsetMin = Vector2.zero;
        dwtRT.offsetMax = Vector2.zero;

        // 10B. Sleek Ultimate Ability Pill (Side-by-side with Dash)
        Transform existingUltWidget = canvasT.Find("HUD Ultimate Widget");
        GameObject ultWidgetGO;
        if (existingUltWidget == null)
        {
            ultWidgetGO = new GameObject("HUD Ultimate Widget", typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
            ultWidgetGO.transform.SetParent(canvasT, false);
        }
        else
        {
            ultWidgetGO = existingUltWidget.gameObject;
        }

        RectTransform uwRT = ultWidgetGO.GetComponent<RectTransform>();
        uwRT.anchorMin = new Vector2(0f, 1f);
        uwRT.anchorMax = new Vector2(0f, 1f);
        uwRT.pivot = new Vector2(0f, 0.5f);
        uwRT.anchoredPosition = new Vector2(182f, -114f);
        uwRT.sizeDelta = new Vector2(180f, 26f);

        ultWidgetBg = ultWidgetGO.GetComponent<Image>();
        if (hudSlot != null) ultWidgetBg.sprite = hudSlot;
        ultWidgetBg.type = Image.Type.Sliced;
        ultWidgetBg.color = new Color(0.18f, 0.12f, 0.16f, 0.85f);

        UnityEngine.UI.Button ultBtn = ultWidgetGO.GetComponent<UnityEngine.UI.Button>();
        ultBtn.onClick.RemoveAllListeners();
        ultBtn.onClick.AddListener(() =>
        {
            if (PlayerUltimateController.instance != null && PlayerUltimateController.instance.IsUltimateReady)
            {
                PlayerUltimateController.instance.ActivateUltimate();
            }
        });

        Transform uwTextChild = ultWidgetGO.transform.Find("Ult Text");
        GameObject uwTextGO = uwTextChild != null ? uwTextChild.gameObject : new GameObject("Ult Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (uwTextChild == null) uwTextGO.transform.SetParent(ultWidgetGO.transform, false);

        ultStatusText = uwTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) ultStatusText.font = retroFont;
        ultStatusText.fontSize = 15;
        ultStatusText.alignment = TextAlignmentOptions.Center;
        ultStatusText.color = new Color(1f, 0.8f, 0.2f);
        ultStatusText.text = "[E] ULT: 0%";

        RectTransform uwtRT = uwTextGO.GetComponent<RectTransform>();
        uwtRT.anchorMin = Vector2.zero;
        uwtRT.anchorMax = Vector2.one;
        uwtRT.offsetMin = Vector2.zero;
        uwtRT.offsetMax = Vector2.zero;

        // 11. Bring popups (LevelUp, Pause, End) on top of HUD
        if (levelUpPanel != null) levelUpPanel.transform.SetAsLastSibling();
        if (pauseScreen != null) pauseScreen.transform.SetAsLastSibling();
        if (levelEndScreen != null) levelEndScreen.transform.SetAsLastSibling();
    }

    public void UpdateWeaponSlots()
    {
        if (weaponSlotIcons == null || weaponSlotIcons.Count == 0 || PlayerController.instance == null)
        {
            return;
        }

        List<Weapon> activeWeapons = new List<Weapon>();
        if (PlayerController.instance.assignedWeapons != null)
        {
            activeWeapons.AddRange(PlayerController.instance.assignedWeapons);
        }
        if (PlayerController.instance.fullyLevelledWeapons != null)
        {
            activeWeapons.AddRange(PlayerController.instance.fullyLevelledWeapons);
        }

        for (int i = 0; i < weaponSlotIcons.Count; i++)
        {
            if (i < activeWeapons.Count && activeWeapons[i] != null && activeWeapons[i].icon != null)
            {
                weaponSlotIcons[i].sprite = activeWeapons[i].icon;
                weaponSlotIcons[i].enabled = true;
                weaponSlotIcons[i].color = Color.white;

                if (i < weaponSlotLevels.Count && weaponSlotLevels[i] != null)
                {
                    int lvl = activeWeapons[i].weaponLevel + 1;
                    bool isMax = activeWeapons[i].stats != null && lvl >= activeWeapons[i].stats.Count;
                    weaponSlotLevels[i].text = isMax ? "MAX" : lvl.ToString();
                    weaponSlotLevels[i].color = isMax ? new Color(1f, 0.4f, 0.4f) : new Color(1f, 0.9f, 0.3f);
                    weaponSlotLevels[i].enabled = true;
                }
            }
            else
            {
                weaponSlotIcons[i].enabled = false;
                if (i < weaponSlotLevels.Count && weaponSlotLevels[i] != null)
                {
                    weaponSlotLevels[i].enabled = false;
                }
            }
        }
    }

    public struct PassiveSlotData
    {
        public string name;
        public Sprite icon;
        public int level;
        public int maxLevel;
    }

    public List<PassiveSlotData> GetActivePassives()
    {
        List<PassiveSlotData> list = new List<PassiveSlotData>();

        if (UpgradeManager.instance != null && UpgradeManager.instance.upgradeDatabase != null)
        {
            foreach (var up in UpgradeManager.instance.upgradeDatabase)
            {
                if (up is UpgradeStatUpgradeData statUp && statUp.currentLevel > 0)
                {
                    list.Add(new PassiveSlotData
                    {
                        name = statUp.upgradeName,
                        icon = statUp.GetIcon(),
                        level = statUp.currentLevel,
                        maxLevel = statUp.maxLevel
                    });
                }
            }
        }

        if (list.Count == 0 && PlayerStatController.instance != null)
        {
            UpgradeStatUpgradeData[] stats = Resources.LoadAll<UpgradeStatUpgradeData>("DataUpgrades");
            Sprite bootsIcon = null, heartIcon = null, magnetIcon = null;
            if (stats != null)
            {
                foreach (var s in stats)
                {
                    if (s.statType == StatUpgradeType.MoveSpeed && s.icon != null) bootsIcon = s.icon;
                    if (s.statType == StatUpgradeType.MaxHealth && s.icon != null) heartIcon = s.icon;
                    if (s.statType == StatUpgradeType.PickupRange && s.icon != null) magnetIcon = s.icon;
                }
            }

            if (PlayerStatController.instance.moveSpeedLevel > 0)
                list.Add(new PassiveSlotData { name = "Wings", icon = bootsIcon, level = PlayerStatController.instance.moveSpeedLevel, maxLevel = 5 });
            if (PlayerStatController.instance.healthLevel > 0)
                list.Add(new PassiveSlotData { name = "Hollow Heart", icon = heartIcon, level = PlayerStatController.instance.healthLevel, maxLevel = 5 });
            if (PlayerStatController.instance.pickupRangeLevel > 0)
                list.Add(new PassiveSlotData { name = "Attractorb", icon = magnetIcon, level = PlayerStatController.instance.pickupRangeLevel, maxLevel = 5 });
        }

        return list;
    }

    public int GetPassiveStateHash()
    {
        int hash = 17;
        if (UpgradeManager.instance != null && UpgradeManager.instance.upgradeDatabase != null)
        {
            foreach (var up in UpgradeManager.instance.upgradeDatabase)
            {
                if (up is UpgradeStatUpgradeData s) hash = hash * 31 + s.currentLevel;
            }
        }
        if (PlayerStatController.instance != null)
        {
            hash = hash * 31 + PlayerStatController.instance.moveSpeedLevel;
            hash = hash * 31 + PlayerStatController.instance.healthLevel;
            hash = hash * 31 + PlayerStatController.instance.pickupRangeLevel;
        }
        return hash;
    }

    public void UpdatePassiveSlots()
    {
        if (passiveSlotIcons == null || passiveSlotIcons.Count == 0) return;

        List<PassiveSlotData> passives = GetActivePassives();

        for (int i = 0; i < passiveSlotIcons.Count; i++)
        {
            if (i < passives.Count && passives[i].icon != null)
            {
                passiveSlotIcons[i].sprite = passives[i].icon;
                passiveSlotIcons[i].enabled = true;
                passiveSlotIcons[i].color = Color.white;

                if (i < passiveSlotLevels.Count && passiveSlotLevels[i] != null)
                {
                    bool isMax = passives[i].level >= passives[i].maxLevel;
                    passiveSlotLevels[i].text = isMax ? "MAX" : passives[i].level.ToString();
                    passiveSlotLevels[i].color = isMax ? new Color(1f, 0.4f, 0.4f) : new Color(0.4f, 1f, 0.7f);
                    passiveSlotLevels[i].enabled = true;
                }
            }
            else
            {
                passiveSlotIcons[i].enabled = false;
                if (i < passiveSlotLevels.Count && passiveSlotLevels[i] != null)
                {
                    passiveSlotLevels[i].enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !levelEndScreen.activeSelf) // If the escape key is pressed and the player isn't dead. GK
        {
            PauseUnpause(); // Pause and unpause the game. GK
        }

        // Cập nhật hiển thị trạng thái Dash & Combo
        if (dashStatusText != null && PlayerController.instance != null)
        {
            string dashPart = "";
            if (PlayerController.instance.isDashing)
            {
                dashPart = "<color=#FFFF00><b>[ SPACE: DASHING! ]</b></color>";
            }
            else if (PlayerController.instance.IsDashReady)
            {
                dashPart = "<color=#00FFAA><b>[ SPACE: DASH READY ]</b></color>";
            }
            else
            {
                float rem = PlayerController.instance.dashCooldown * PlayerController.instance.DashCooldownProgress;
                dashPart = $"<color=#88A0B8>[ SPACE: DASH {rem:0.0}s ]</color>";
            }

            string comboPart = "";
            if (ComboManager.instance != null && ComboManager.instance.currentCombo >= 5)
            {
                string colHex = ComboManager.instance.currentCombo >= 50 ? "#FF2244" : 
                               (ComboManager.instance.currentCombo >= 25 ? "#FFAA00" : "#FFEA00");
                comboPart = $"  |  <color={colHex}><b>x{ComboManager.instance.currentCombo} COMBO!</b></color>";
            }

            dashStatusText.text = dashPart + comboPart;
        }

        // Cập nhật nút bấm Chiêu Cuối (Ultimate Widget)
        if (ultStatusText != null && PlayerUltimateController.instance != null)
        {
            string ultName = PlayerUltimateController.instance.GetUltimateName();
            if (PlayerUltimateController.instance.isCasting)
            {
                ultStatusText.text = $"<color=#FF5500><b>CASTING {ultName}!</b></color>";
                if (ultWidgetBg != null) ultWidgetBg.color = new Color(0.6f, 0.2f, 0.1f, 0.9f);
            }
            else if (PlayerUltimateController.instance.IsUltimateReady)
            {
                float pulse = Mathf.PingPong(Time.unscaledTime * 4f, 1f);
                string glowCol = ColorUtility.ToHtmlStringRGB(Color.Lerp(new Color(1f, 0.85f, 0.1f), new Color(1f, 0.4f, 0.1f), pulse));
                ultStatusText.text = $"<color=#{glowCol}><b>★ [E / CLICK] {ultName}: READY! ★</b></color>";
                if (ultWidgetBg != null) ultWidgetBg.color = Color.Lerp(new Color(0.35f, 0.25f, 0.05f, 0.9f), new Color(0.55f, 0.4f, 0.1f, 0.95f), pulse);
            }
            else
            {
                int pct = Mathf.FloorToInt(PlayerUltimateController.instance.RagePercent * 100f);
                ultStatusText.text = $"<color=#FFAA33>[E / CLICK] {ultName}: {pct}%</color>";
                if (ultWidgetBg != null) ultWidgetBg.color = new Color(0.18f, 0.12f, 0.16f, 0.85f);
            }
        }

        // Smooth EXP bar interpolation
        if (explvlSlider != null && explvlSlider.value != targetExpValue)
        {
            float speed = Mathf.Max((targetExpValue - explvlSlider.value) * 6f, explvlSlider.maxValue * 1.5f);
            explvlSlider.value = Mathf.MoveTowards(explvlSlider.value, targetExpValue, speed * Time.unscaledDeltaTime);
        }

        if (PlayerController.instance != null && PlayerController.instance.assignedWeapons != null)
        {
            int currentCount = PlayerController.instance.assignedWeapons.Count + 
                (PlayerController.instance.fullyLevelledWeapons != null ? PlayerController.instance.fullyLevelledWeapons.Count : 0);
            if (currentCount != lastWeaponCount)
            {
                lastWeaponCount = currentCount;
                UpdateWeaponSlots();
            }
        }

        int passHash = GetPassiveStateHash();
        if (passHash != lastPassiveHash)
        {
            lastPassiveHash = passHash;
            UpdatePassiveSlots();
        }
    }

    public void UpdateExperience(int currentExp, int levelExp, int currentLvl) // Function to update the experience. GK
    {
        if (explvlSlider != null)
        {
            explvlSlider.maxValue = levelExp; // Set the max value of the slider to the level experience. GK
            if (currentLvl != currentDisplayLevel)
            {
                currentDisplayLevel = currentLvl;
                explvlSlider.value = 0f;
            }
            targetExpValue = currentExp;
        }
        if (explvlText != null)
        {
            explvlText.text = "LV " + currentLvl; // Set text matching Vampire Survivors "LV 1" style. GK
        }
        UpdateWeaponSlots();
        UpdatePassiveSlots();
    }

    public void RegisterKill() // Function to track and display defeated enemies.
    {
        killCount++;
        if (killText != null)
        {
            killText.text = killCount.ToString();
        }
    }

    public void SkipLevelUp() // Function to skip the level. GK
    {
        ExperienceLevelController.instance.FinishLevelUp();
    }

    public void UpdateCoins() // Function to update the coins. D
    {
        if (coinText != null && CoinController.instance != null)
        {
            coinText.text = CoinController.instance.currentCoins.ToString(); // Display clean number without "Coins: ". D
        }
    }

    public void PurchaseMoveSpeed() // Function to purchase the move speed. GK
    {
        PlayerStatController.instance.PurchaseMoveSpeed();
        SkipLevelUp();
    }

    public void PurchaseHealth() // Function to purchase the health. GK
    {
        PlayerStatController.instance.PurchaseHealth();
        SkipLevelUp();
    }

    public void PurchasePickupRange() // Function to purchase the pickup range. GK
    {
        PlayerStatController.instance.PurchasePickupRange();
        SkipLevelUp();
    }

    public void PurchaseMaxWeapons() // Function to purchase the max weapons. GK
    {
        PlayerStatController.instance.PurchaseMaxWeapons();
        SkipLevelUp();
    }

    public void UpdateTimer(float time) // Function to update the timer. GK
    {
        float minutes = Mathf.FloorToInt(time / 60); // Set the minutes to the time divided by 60. GK
        float seconds = Mathf.FloorToInt(time % 60); // Set the seconds to the time modulo 60. GK
        if (timeText != null)
        {
            timeText.text = string.Format("{0}:{1:00}", (int)minutes, (int)seconds); // Vampire Survivors retro timer format (e.g. 0:05). GK
        }

        if (stageSubtitleText != null)
        {
            if (time < 120f)
                stageSubtitleText.text = "<color=#88FF88>✦ STAGE I: THE TOXIC MEADOW ✦</color>";
            else if (time < 300f)
                stageSubtitleText.text = "<color=#66DDFF>✦ STAGE II: THE FROSTBITE TUNDRA ✦</color>";
            else if (time < 480f)
                stageSubtitleText.text = "<color=#FF6677>✦ STAGE III: THE CURSED CATACOMBS ✦</color>";
            else if (time < 690f)
                stageSubtitleText.text = "<color=#DD77FF>✦ STAGE IV: THE VOID NETHERWORLD ✦</color>";
            else if (time < 900f)
                stageSubtitleText.text = "<color=#FFAA33>✦ STAGE V: THE INFERNAL CATACLYSM ✦</color>";
            else
                stageSubtitleText.text = "<color=#FFD700>✦ ENDLESS CHAOS OVERLORD ✦</color>";
        }
    }

    public void GoToMainMenu() // Function to go to the main menu. GK
    {
        SceneManager.LoadScene(mainMenuName); // Load the main menu. GK
        Time.timeScale = 1f; // Set the time scale to 1. GK 
    }

    public void SaveAndExit() // Save the run exactly as it is right now, then go back to the main menu.
    {
        RunSave.Save();
        GoToMainMenu();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f; // Set the time scale to 1. GK 
    }

    public void QuitGame() // Function to quit the game. GK
    {
        Application.Quit(); // Quit the application. GK
    }

    public void PauseUnpause() // Function to pause and unpause the game. GK
    {
        if (pauseScreen.activeSelf == false) // If the pause screen is active. GK
        {
            pauseScreen.SetActive(true); // Set the pause screen to active. GK
            Time.timeScale = 0f; // Set the time scale to 0. GK
            FormatAndRefreshPauseScreen();
        }
        else
        {
            pauseScreen.SetActive(false); // Set the pause screen to inactive. GK
            if (levelUpPanel.activeSelf == false) // If the level up panel is not active. GK
            {
                Time.timeScale = 1f; // Set the time scale to 1. GK 
            }
        }
    }

    public void FormatAndRefreshPauseScreen()
    {
        if (pauseScreen == null) return;

        // 1. Dark gothic vignette backdrop
        Image bgImg = pauseScreen.GetComponent<Image>();
        if (bgImg != null)
        {
            bgImg.sprite = null;
            bgImg.color = new Color(0.02f, 0.02f, 0.05f, 0.88f);
            bgImg.raycastTarget = true;
        }

        // 2. Gold-framed Pause Window
        Transform winT = pauseScreen.transform.Find("Window");
        if (winT == null)
        {
            GameObject w = new GameObject("Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            w.transform.SetParent(pauseScreen.transform, false);
            winT = w.transform;
        }
        RectTransform winRT = (RectTransform)winT;
        winRT.anchorMin = winRT.anchorMax = winRT.pivot = new Vector2(0.5f, 0.5f);
        winRT.anchoredPosition = Vector2.zero;
        winRT.sizeDelta = new Vector2(860f, 740f);

        Image winImg = winT.GetComponent<Image>();
        if (winImg != null)
        {
            winImg.sprite = GetGothicWindowSprite();
            winImg.type = Image.Type.Sliced;
            winImg.color = Color.white;
        }

        // 3. Title: "❖   ✦   P A U S E D   ✦   ❖"
        Transform titleT = winT.Find("Title");
        GameObject titleGO = titleT != null ? titleT.gameObject : new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (titleT == null) titleGO.transform.SetParent(winT, false);
        RectTransform tRT = titleGO.GetComponent<RectTransform>();
        tRT.anchorMin = tRT.anchorMax = tRT.pivot = new Vector2(0.5f, 0.5f);
        tRT.anchoredPosition = new Vector2(0f, 324f);
        tRT.sizeDelta = new Vector2(750f, 65f);

        TextMeshProUGUI tTMP = titleGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) tTMP.font = retroFont;
        tTMP.text = "❖   ✦   P A U S E D   ✦   ❖";
        tTMP.fontSize = 62f;
        tTMP.fontStyle = FontStyles.Bold;
        tTMP.characterSpacing = 6f;
        tTMP.alignment = TextAlignmentOptions.Center;
        tTMP.color = new Color(1f, 0.88f, 0.25f);

        // Subtitle under pause title
        Transform subT = winT.Find("PauseSubtitle");
        GameObject subGO = subT != null ? subT.gameObject : new GameObject("PauseSubtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (subT == null) subGO.transform.SetParent(winT, false);
        RectTransform subRT = subGO.GetComponent<RectTransform>();
        subRT.anchorMin = subRT.anchorMax = subRT.pivot = new Vector2(0.5f, 0.5f);
        subRT.anchoredPosition = new Vector2(0f, 275f);
        subRT.sizeDelta = new Vector2(650f, 28f);
        TextMeshProUGUI subTMP = subGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) subTMP.font = retroFont;
        subTMP.text = "✦  THE NIGHT HOLDS ITS BREATH  ✦";
        subTMP.fontSize = 22f;
        subTMP.alignment = TextAlignmentOptions.Center;
        subTMP.color = new Color(0.92f, 0.82f, 0.55f, 0.95f);
        subTMP.characterSpacing = 3f;

        // Hide legacy flat divider if present
        Transform divT = winT.Find("Divider");
        if (divT != null) divT.gameObject.SetActive(false);

        // 4. Equipment Grid Container (Weapons & Passives)
        Transform equipT = winT.Find("PauseEquipmentGrid");
        GameObject equipGO = equipT != null ? equipT.gameObject : new GameObject("PauseEquipmentGrid", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (equipT == null) equipGO.transform.SetParent(winT, false);
        RectTransform eqRT = equipGO.GetComponent<RectTransform>();
        eqRT.anchorMin = eqRT.anchorMax = eqRT.pivot = new Vector2(0.5f, 0.5f);
        eqRT.anchoredPosition = new Vector2(0f, 130f);
        eqRT.sizeDelta = new Vector2(790f, 240f);

        Image eqImg = equipGO.GetComponent<Image>();
        eqImg.sprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        eqImg.type = Image.Type.Sliced;
        eqImg.color = new Color(0.08f, 0.07f, 0.12f, 0.95f);

        // 4A. Weapons Row inside Equipment Grid
        Transform wHeaderT = equipGO.transform.Find("WeaponsHeader");
        GameObject wHeaderGO = wHeaderT != null ? wHeaderT.gameObject : new GameObject("WeaponsHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (wHeaderT == null) wHeaderGO.transform.SetParent(equipGO.transform, false);
        RectTransform whRT = wHeaderGO.GetComponent<RectTransform>();
        whRT.anchorMin = whRT.anchorMax = whRT.pivot = new Vector2(0.5f, 0.5f);
        whRT.anchoredPosition = new Vector2(0f, 85f);
        whRT.sizeDelta = new Vector2(750f, 30f);
        TextMeshProUGUI whTMP = wHeaderGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) whTMP.font = retroFont;
        whTMP.fontSize = 24f;
        whTMP.alignment = TextAlignmentOptions.Center;
        whTMP.color = new Color(1f, 0.85f, 0.3f);
        whTMP.text = "✦  EQUIPPED WEAPONS  ✦";

        // Query active weapons
        List<Weapon> activeWeapons = new List<Weapon>();
        if (PlayerController.instance != null)
        {
            if (PlayerController.instance.assignedWeapons != null) activeWeapons.AddRange(PlayerController.instance.assignedWeapons);
            if (PlayerController.instance.fullyLevelledWeapons != null) activeWeapons.AddRange(PlayerController.instance.fullyLevelledWeapons);
        }

        Sprite slotFrame = Resources.Load<Sprite>("UI/VS_HUD_Slot");
        float slotW = 54f;
        float slotSpacing = 16f;
        float startX = -((6 * slotW + 5 * slotSpacing) * 0.5f) + (slotW * 0.5f);

        for (int i = 0; i < 6; i++)
        {
            string sName = "P_WeaponSlot_" + i;
            Transform sChild = equipGO.transform.Find(sName);
            GameObject sGO = sChild != null ? sChild.gameObject : new GameObject(sName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (sChild == null) sGO.transform.SetParent(equipGO.transform, false);
            RectTransform sRT = sGO.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(startX + i * (slotW + slotSpacing), 32f);
            sRT.sizeDelta = new Vector2(slotW, slotW);

            Image sImg = sGO.GetComponent<Image>();
            sImg.sprite = slotFrame;
            sImg.type = Image.Type.Sliced;
            sImg.color = Color.white;

            // Icon
            Transform iconT = sGO.transform.Find("Icon");
            GameObject iconGO = iconT != null ? iconT.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (iconT == null) iconGO.transform.SetParent(sGO.transform, false);
            RectTransform icRT = iconGO.GetComponent<RectTransform>();
            icRT.anchorMin = Vector2.zero;
            icRT.anchorMax = Vector2.one;
            icRT.offsetMin = new Vector2(6f, 6f);
            icRT.offsetMax = new Vector2(-6f, -6f);
            Image icImg = iconGO.GetComponent<Image>();
            icImg.preserveAspect = true;

            // Level Label
            Transform lvlT = sGO.transform.Find("Level");
            GameObject lvlGO = lvlT != null ? lvlT.gameObject : new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (lvlT == null) lvlGO.transform.SetParent(sGO.transform, false);
            RectTransform lRT = lvlGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0f, 0f);
            lRT.anchorMax = new Vector2(1f, 0f);
            lRT.pivot = new Vector2(0.5f, 1f);
            lRT.anchoredPosition = new Vector2(0f, -4f);
            lRT.sizeDelta = new Vector2(60f, 20f);
            TextMeshProUGUI lTMP = lvlGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) lTMP.font = retroFont;
            lTMP.fontSize = 17f;
            lTMP.alignment = TextAlignmentOptions.Center;

            if (i < activeWeapons.Count && activeWeapons[i] != null && activeWeapons[i].icon != null)
            {
                icImg.sprite = activeWeapons[i].icon;
                icImg.enabled = true;
                icImg.color = Color.white;

                int lvl = activeWeapons[i].weaponLevel + 1;
                bool isMax = activeWeapons[i].stats != null && lvl >= activeWeapons[i].stats.Count;
                lTMP.text = isMax ? "<color=#FF4444>MAX</color>" : $"<color=#FFE040>LV {lvl}</color>";
                lTMP.enabled = true;
            }
            else
            {
                icImg.enabled = false;
                lTMP.enabled = false;
            }
        }

        // 4B. Passives Row inside Equipment Grid
        Transform pHeaderT = equipGO.transform.Find("PassivesHeader");
        GameObject pHeaderGO = pHeaderT != null ? pHeaderT.gameObject : new GameObject("PassivesHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (pHeaderT == null) pHeaderGO.transform.SetParent(equipGO.transform, false);
        RectTransform phRT = pHeaderGO.GetComponent<RectTransform>();
        phRT.anchorMin = phRT.anchorMax = phRT.pivot = new Vector2(0.5f, 0.5f);
        phRT.anchoredPosition = new Vector2(0f, -38f);
        phRT.sizeDelta = new Vector2(750f, 30f);
        TextMeshProUGUI phTMP = pHeaderGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) phTMP.font = retroFont;
        phTMP.fontSize = 24f;
        phTMP.alignment = TextAlignmentOptions.Center;
        phTMP.color = new Color(0.4f, 1f, 0.7f);
        phTMP.text = "✦  PASSIVE ACCESSORIES  ✦";

        List<PassiveSlotData> activePassives = GetActivePassives();

        for (int i = 0; i < 6; i++)
        {
            string sName = "P_PassiveSlot_" + i;
            Transform sChild = equipGO.transform.Find(sName);
            GameObject sGO = sChild != null ? sChild.gameObject : new GameObject(sName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (sChild == null) sGO.transform.SetParent(equipGO.transform, false);
            RectTransform sRT = sGO.GetComponent<RectTransform>();
            sRT.anchorMin = sRT.anchorMax = sRT.pivot = new Vector2(0.5f, 0.5f);
            sRT.anchoredPosition = new Vector2(startX + i * (slotW + slotSpacing), -90f);
            sRT.sizeDelta = new Vector2(slotW, slotW);

            Image sImg = sGO.GetComponent<Image>();
            sImg.sprite = slotFrame;
            sImg.type = Image.Type.Sliced;
            sImg.color = Color.white;

            // Icon
            Transform iconT = sGO.transform.Find("Icon");
            GameObject iconGO = iconT != null ? iconT.gameObject : new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (iconT == null) iconGO.transform.SetParent(sGO.transform, false);
            RectTransform icRT = iconGO.GetComponent<RectTransform>();
            icRT.anchorMin = Vector2.zero;
            icRT.anchorMax = Vector2.one;
            icRT.offsetMin = new Vector2(6f, 6f);
            icRT.offsetMax = new Vector2(-6f, -6f);
            Image icImg = iconGO.GetComponent<Image>();
            icImg.preserveAspect = true;

            // Level Label
            Transform lvlT = sGO.transform.Find("Level");
            GameObject lvlGO = lvlT != null ? lvlT.gameObject : new GameObject("Level", typeof(RectTransform), typeof(TextMeshProUGUI));
            if (lvlT == null) lvlGO.transform.SetParent(sGO.transform, false);
            RectTransform lRT = lvlGO.GetComponent<RectTransform>();
            lRT.anchorMin = new Vector2(0f, 0f);
            lRT.anchorMax = new Vector2(1f, 0f);
            lRT.pivot = new Vector2(0.5f, 1f);
            lRT.anchoredPosition = new Vector2(0f, -4f);
            lRT.sizeDelta = new Vector2(60f, 20f);
            TextMeshProUGUI lTMP = lvlGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) lTMP.font = retroFont;
            lTMP.fontSize = 17f;
            lTMP.alignment = TextAlignmentOptions.Center;

            if (i < activePassives.Count && activePassives[i].icon != null)
            {
                icImg.sprite = activePassives[i].icon;
                icImg.enabled = true;
                icImg.color = Color.white;

                bool isMax = activePassives[i].level >= activePassives[i].maxLevel;
                lTMP.text = isMax ? "<color=#FF4444>MAX</color>" : $"<color=#66FFAA>LV {activePassives[i].level}</color>";
                lTMP.enabled = true;
            }
            else
            {
                icImg.enabled = false;
                lTMP.enabled = false;
            }
        }

        // 5. Player Stats Summary Panel
        Transform statsT = winT.Find("PauseStatsPanel");
        GameObject statsGO = statsT != null ? statsT.gameObject : new GameObject("PauseStatsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (statsT == null) statsGO.transform.SetParent(winT, false);
        RectTransform stRT = statsGO.GetComponent<RectTransform>();
        stRT.anchorMin = stRT.anchorMax = stRT.pivot = new Vector2(0.5f, 0.5f);
        stRT.anchoredPosition = new Vector2(0f, -42f);
        stRT.sizeDelta = new Vector2(790f, 75f);

        Image stImg = statsGO.GetComponent<Image>();
        stImg.sprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        stImg.type = Image.Type.Sliced;
        stImg.color = new Color(0.06f, 0.05f, 0.10f, 0.95f);

        Transform statsTextT = statsGO.transform.Find("StatsText");
        GameObject statsTextGO = statsTextT != null ? statsTextT.gameObject : new GameObject("StatsText", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (statsTextT == null) statsTextGO.transform.SetParent(statsGO.transform, false);
        RectTransform sttRT = statsTextGO.GetComponent<RectTransform>();
        sttRT.anchorMin = Vector2.zero;
        sttRT.anchorMax = Vector2.one;
        sttRT.offsetMin = new Vector2(18f, 6f);
        sttRT.offsetMax = new Vector2(-18f, -6f);
        TextMeshProUGUI sttTMP = statsTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) sttTMP.font = retroFont;
        sttTMP.fontSize = 20f;
        sttTMP.alignment = TextAlignmentOptions.Midline;

        float moveSpd = PlayerController.instance != null ? PlayerController.instance.moveSpeed : 5f;
        float maxHp = PlayerHealthController.instance != null ? PlayerHealthController.instance.maxHealth : 100f;
        float curHp = PlayerHealthController.instance != null ? PlayerHealthController.instance.currentHealth : 100f;
        int coins = CoinController.instance != null ? CoinController.instance.currentCoins : 0;
        int curLvl = ExperienceLevelController.instance != null ? ExperienceLevelController.instance.currentLevel : 1;

        sttTMP.text = $"<color=#FFD700>STAGE LV: {curLvl}</color>    |    <color=#FF6666>HP: {(int)curHp}/{(int)maxHp}</color>    |    <color=#00FFAA>SPEED: {moveSpd:0.0}</color>    |    <color=#FF5566>KILLS: {killCount}</color>    |    <color=#FFE040>GOLD: {coins}</color>";

        // 6. Action Buttons Layout with Royal Gothic 9-Slice Styling
        Sprite btnEmerald = MainMenu.CreateRoyalGothicButton(
            new Color(0.04f, 0.18f, 0.09f),
            new Color(0.10f, 0.36f, 0.18f),
            new Color(0.88f, 0.74f, 0.25f),
            new Color(1.0f, 0.96f, 0.55f),
            new Color(0.20f, 0.98f, 0.45f)
        );

        Sprite btnSapphire = MainMenu.CreateRoyalGothicButton(
            new Color(0.04f, 0.12f, 0.25f),
            new Color(0.09f, 0.24f, 0.45f),
            new Color(0.85f, 0.72f, 0.22f),
            new Color(0.98f, 0.92f, 0.50f),
            new Color(0.25f, 0.75f, 1f)
        );

        Sprite btnAmber = MainMenu.CreateRoyalGothicButton(
            new Color(0.25f, 0.15f, 0.04f),
            new Color(0.45f, 0.28f, 0.08f),
            new Color(0.95f, 0.80f, 0.25f),
            new Color(1.0f, 0.96f, 0.58f),
            new Color(1f, 0.85f, 0.2f)
        );

        Sprite btnCrimson = MainMenu.CreateRoyalGothicButton(
            new Color(0.22f, 0.05f, 0.07f),
            new Color(0.44f, 0.10f, 0.14f),
            new Color(0.82f, 0.62f, 0.20f),
            new Color(0.98f, 0.84f, 0.42f),
            new Color(0.95f, 0.22f, 0.22f)
        );

        SetupPauseButton(winT, "Resume", "✦  RESUME  ✦", new Vector2(-195f, -135f), new Vector2(365f, 60f), btnEmerald, new Color(0.92f, 1f, 0.94f), 32f, PauseUnpause);
        SetupPauseButton(winT, "Save And Exit", "✦  SAVE & EXIT  ✦", new Vector2(195f, -135f), new Vector2(365f, 60f), btnSapphire, new Color(0.92f, 0.96f, 1f), 32f, SaveAndExit);
        SetupPauseButton(winT, "Restart", "✦  RESTART  ✦", new Vector2(-260f, -218f), new Vector2(235f, 56f), btnAmber, new Color(1f, 0.95f, 0.85f), 28f, Restart);
        SetupPauseButton(winT, "Main Menu", "✦  MAIN MENU  ✦", new Vector2(0f, -218f), new Vector2(235f, 56f), btnCrimson, new Color(1f, 0.88f, 0.88f), 28f, GoToMainMenu);
        SetupPauseButton(winT, "Quit Game", "✦  QUIT  ✦", new Vector2(260f, -218f), new Vector2(235f, 56f), btnCrimson, new Color(1f, 0.88f, 0.88f), 28f, QuitGame);
    }

    private void SetupPauseButton(Transform win, string btnName, string label, Vector2 pos, Vector2 size, Sprite sprite, Color textCol, float fontSize, UnityEngine.Events.UnityAction onClickAction)
    {
        Transform bT = win.Find(btnName);
        GameObject bGO;
        if (bT == null)
        {
            bGO = new GameObject(btnName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            bGO.transform.SetParent(win, false);
        }
        else
        {
            bGO = bT.gameObject;
        }

        RectTransform rt = bGO.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = bGO.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        Button btn = bGO.GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(onClickAction);

        if (bGO.GetComponent<GothicMenuButtonEffects>() == null)
        {
            bGO.AddComponent<GothicMenuButtonEffects>();
        }

        Transform tChild = bGO.transform.Find("Text");
        GameObject tGO = tChild != null ? tChild.gameObject : null;
        if (tGO == null)
        {
            TextMeshProUGUI existing = bGO.GetComponentInChildren<TextMeshProUGUI>();
            if (existing != null) tGO = existing.gameObject;
            else
            {
                tGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                tGO.transform.SetParent(bGO.transform, false);
            }
        }

        RectTransform trt = tGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = tGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) tmp.font = retroFont;
        tmp.text = label;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = textCol;
        tmp.characterSpacing = 3f;
        tmp.fontStyle = FontStyles.Bold;
    }

    public void HideWarningBanner()
    {
        if (warningBannerCo != null) StopCoroutine(warningBannerCo);
        if (warningBannerRoot != null) warningBannerRoot.SetActive(false);
    }

    [Header("Warning Banner")]
    private TMP_Text warningBannerText;
    private Coroutine warningBannerCo;

    /// <summary>
    /// Hiá»ƒn thá»‹ thanh cáº£nh bÃ¡o chá»›p nhÃ¡y phong cÃ¡ch retro (vÃ­ dá»¥ khi Mini-Boss xuáº¥t hiá»‡n).
    /// </summary>
    public void ShowWarning(string message, float duration = 2.5f)
    {
        if (warningBannerText == null)
        {
            CreateWarningBanner();
        }

        if (warningBannerText != null)
        {
            if (warningBannerCo != null) StopCoroutine(warningBannerCo);
            warningBannerCo = StartCoroutine(WarningBannerRoutine(message, duration));
        }
    }

    private GameObject warningBannerRoot;

    private void CreateWarningBanner()
    {
        Transform canvasT = explvlSlider != null ? explvlSlider.transform.parent : transform;
        warningBannerRoot = new GameObject("BossWarningBanner", typeof(RectTransform));
        warningBannerRoot.transform.SetParent(canvasT, false);

        RectTransform rt = warningBannerRoot.GetComponent<RectTransform>();
        // Neo Gothic Top-Right Alert Box: leaves the center battlefield completely clear!
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-16f, -96f);
        rt.sizeDelta = new Vector2(490f, 66f);

        // Gothic dark velvet backdrop with ruby & gold border
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(warningBannerRoot.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.06f, 0.03f, 0.08f, 0.94f);
        bgImg.raycastTarget = false;

        // Gold ornamental top line
        GameObject topLine = new GameObject("TopLine", typeof(RectTransform), typeof(Image));
        topLine.transform.SetParent(warningBannerRoot.transform, false);
        RectTransform topRt = topLine.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0f, 1f);
        topRt.anchorMax = new Vector2(1f, 1f);
        topRt.pivot = new Vector2(0.5f, 1f);
        topRt.anchoredPosition = Vector2.zero;
        topRt.sizeDelta = new Vector2(0f, 2.5f);
        topLine.GetComponent<Image>().color = new Color(0.9f, 0.75f, 0.2f, 0.95f);

        // Crimson ornamental bottom line
        GameObject bottomLine = new GameObject("BottomLine", typeof(RectTransform), typeof(Image));
        bottomLine.transform.SetParent(warningBannerRoot.transform, false);
        RectTransform botRt = bottomLine.GetComponent<RectTransform>();
        botRt.anchorMin = new Vector2(0f, 0f);
        botRt.anchorMax = new Vector2(1f, 0f);
        botRt.pivot = new Vector2(0.5f, 0f);
        botRt.anchoredPosition = Vector2.zero;
        botRt.sizeDelta = new Vector2(0f, 2.5f);
        bottomLine.GetComponent<Image>().color = new Color(0.9f, 0.2f, 0.25f, 0.95f);

        // Alert text container
        GameObject textObj = new GameObject("WarningText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(warningBannerRoot.transform, false);
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(14f, 4f);
        textRt.offsetMax = new Vector2(-14f, -4f);

        warningBannerText = textObj.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) warningBannerText.font = retroFont;
        warningBannerText.fontSize = 24f;
        warningBannerText.alignment = TextAlignmentOptions.Center;
        warningBannerText.color = new Color(1f, 0.88f, 0.35f, 1f);
        warningBannerText.fontStyle = FontStyles.Bold;
        warningBannerText.raycastTarget = false;
        warningBannerText.enableAutoSizing = true;
        warningBannerText.fontSizeMin = 18f;
        warningBannerText.fontSizeMax = 28f;

        warningBannerRoot.SetActive(false);
    }

    private IEnumerator WarningBannerRoutine(string message, float duration)
    {
        if (warningBannerRoot == null) yield break;

        warningBannerRoot.SetActive(true);
        if (warningBannerText != null) warningBannerText.text = message;

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        RectTransform rt = warningBannerRoot.GetComponent<RectTransform>();
        Vector2 targetPos = new Vector2(-16f, -96f);
        Vector2 startPos = new Vector2(450f, -96f);

        // Slide in from right edge
        float slideIn = 0.2f;
        float elapsed = 0f;
        while (elapsed < slideIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideIn);
            float ease = 1f - Mathf.Pow(1f - t, 3);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
            yield return null;
        }
        rt.anchoredPosition = targetPos;

        // Pulse color
        float pulseTime = Mathf.Max(0.5f, duration - 0.4f);
        elapsed = 0f;
        while (elapsed < pulseTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.PingPong(elapsed * 6f, 1f);
            if (warningBannerText != null)
            {
                warningBannerText.color = Color.Lerp(new Color(1f, 0.25f, 0.2f, 1f), new Color(1f, 0.92f, 0.35f, 1f), t);
            }
            yield return null;
        }

        // Slide out to right edge
        float slideOut = 0.2f;
        elapsed = 0f;
        while (elapsed < slideOut)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideOut);
            float ease = t * t;
            rt.anchoredPosition = Vector2.Lerp(targetPos, startPos, ease);
            yield return null;
        }

        warningBannerRoot.SetActive(false);
    }

    private static Sprite gothicWindowSprite;
    public static Sprite GetGothicWindowSprite()
    {
        if (gothicWindowSprite != null) return gothicWindowSprite;

        int size = 96;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color darkIron = new Color(0.06f, 0.05f, 0.08f, 1f);
        Color borderGold = new Color(0.85f, 0.68f, 0.22f, 1f);
        Color shadowBronze = new Color(0.28f, 0.18f, 0.06f, 1f);
        Color highlightGold = new Color(1f, 0.94f, 0.65f, 1f);
        Color innerDark = new Color(0.06f, 0.05f, 0.09f, 0.96f);
        Color innerLight = new Color(0.10f, 0.08f, 0.15f, 0.96f);
        Color jewelRuby = new Color(0.92f, 0.15f, 0.25f, 1f);
        Color jewelCore = new Color(1f, 0.85f, 0.85f, 1f);

        for (int y = 0; y < size; y++)
        {
            float tY = (float)y / (size - 1);
            Color currentInner = Color.Lerp(innerDark, innerLight, tY);

            for (int x = 0; x < size; x++)
            {
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    p[y * size + x] = darkIron;
                }
                else if (x <= 4 || x >= size - 5 || y <= 4 || y >= size - 5)
                {
                    bool isCornerGem = (x >= 2 && x <= 6 && y >= 2 && y <= 6) ||
                                       (x >= size - 7 && x <= size - 3 && y >= 2 && y <= 6) ||
                                       (x >= 2 && x <= 6 && y >= size - 7 && y <= size - 3) ||
                                       (x >= size - 7 && x <= size - 3 && y >= size - 7 && y <= size - 3);

                    if (isCornerGem)
                    {
                        bool isCenter = (x == 4 || x == size - 5) && (y == 4 || y == size - 5);
                        p[y * size + x] = isCenter ? jewelCore : jewelRuby;
                    }
                    else
                    {
                        bool isTopOrLeft = (x <= 2 || y >= size - 3);
                        p[y * size + x] = isTopOrLeft ? highlightGold : shadowBronze;
                    }
                }
                else if (x == 7 || x == size - 8 || y == 7 || y == size - 8)
                {
                    p[y * size + x] = borderGold * 0.75f;
                }
                else
                {
                    p[y * size + x] = currentInner;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        gothicWindowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
        return gothicWindowSprite;
    }

    private static Sprite gothicBanishButtonSprite;
    public static Sprite GetGothicBanishButtonSprite()
    {
        if (gothicBanishButtonSprite != null) return gothicBanishButtonSprite;

        int w = 64;
        int h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color darkIron = new Color(0.12f, 0.08f, 0.10f, 1f);
        Color borderGold = new Color(0.85f, 0.68f, 0.22f, 1f);
        Color shadowBronze = new Color(0.32f, 0.18f, 0.06f, 1f);
        Color highlightGold = new Color(1f, 0.92f, 0.60f, 1f);
        Color crimsonDark = new Color(0.38f, 0.06f, 0.10f, 1f);
        Color crimsonLight = new Color(0.68f, 0.12f, 0.18f, 1f);

        for (int y = 0; y < h; y++)
        {
            float tY = (float)y / (h - 1);
            Color currentInner = Color.Lerp(crimsonDark, crimsonLight, tY);

            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    p[y * w + x] = darkIron;
                }
                else if (x <= 3 || x >= w - 4 || y <= 3 || y >= h - 4)
                {
                    bool isTopOrLeft = (x <= 2 || y >= h - 3);
                    p[y * w + x] = isTopOrLeft ? highlightGold : shadowBronze;
                }
                else
                {
                    p[y * w + x] = currentInner;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        gothicBanishButtonSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
        return gothicBanishButtonSprite;
    }

    // ====================================================================================
    // GRAND ROYAL GOTHIC CENTER-SCREEN BOSS ANNOUNCEMENTS & DEFEAT SPLASH
    // ====================================================================================
    private GameObject bossCenterBannerRoot;
    private TMPro.TextMeshProUGUI bossCenterHeaderText;
    private TMPro.TextMeshProUGUI bossCenterNameText;
    private TMPro.TextMeshProUGUI bossCenterSubtitleText;
    private CanvasGroup bossCenterCanvasGroup;
    private RectTransform bossCenterRect;
    private Coroutine bossCenterBannerCo;

    private void CreateBossCenterBanner()
    {
        Transform canvasT = explvlSlider != null ? explvlSlider.transform.parent : transform;
        bossCenterBannerRoot = new GameObject("BossCenterBanner", typeof(RectTransform), typeof(CanvasGroup));
        bossCenterBannerRoot.transform.SetParent(canvasT, false);

        bossCenterRect = bossCenterBannerRoot.GetComponent<RectTransform>();
        bossCenterRect.anchorMin = new Vector2(0.5f, 0.5f);
        bossCenterRect.anchorMax = new Vector2(0.5f, 0.5f);
        bossCenterRect.pivot = new Vector2(0.5f, 0.5f);
        bossCenterRect.anchoredPosition = new Vector2(0f, 65f);
        bossCenterRect.sizeDelta = new Vector2(720f, 155f);

        bossCenterCanvasGroup = bossCenterBannerRoot.GetComponent<CanvasGroup>();
        bossCenterCanvasGroup.blocksRaycasts = false;
        bossCenterCanvasGroup.interactable = false;

        // Khung cá»­a sá»• hoÃ ng gia Gothic 9-slice Promax
        GameObject bgObj = new GameObject("Backdrop", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        bgObj.transform.SetParent(bossCenterBannerRoot.transform, false);
        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        UnityEngine.UI.Image bgImg = bgObj.GetComponent<UnityEngine.UI.Image>();
        bgImg.sprite = GetGothicWindowSprite();
        bgImg.type = UnityEngine.UI.Image.Type.Sliced;
        bgImg.color = new Color(1f, 1f, 1f, 0.98f);
        bgImg.raycastTarget = false;

        // Dáº£i ruy bÄƒng Ä‘en nhung bÃ³ng má» phÃ­a trong
        GameObject innerShade = new GameObject("InnerShade", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        innerShade.transform.SetParent(bossCenterBannerRoot.transform, false);
        RectTransform isRt = innerShade.GetComponent<RectTransform>();
        isRt.anchorMin = new Vector2(0.04f, 0.1f);
        isRt.anchorMax = new Vector2(0.96f, 0.9f);
        isRt.offsetMin = Vector2.zero;
        isRt.offsetMax = Vector2.zero;
        UnityEngine.UI.Image isImg = innerShade.GetComponent<UnityEngine.UI.Image>();
        isImg.color = new Color(0.02f, 0.01f, 0.04f, 0.65f);
        isImg.raycastTarget = false;

        // 1. TiÃªu Ä‘á» nhá» bÃªn trÃªn (Header Banner)
        GameObject headerObj = new GameObject("BossHeader", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        headerObj.transform.SetParent(bossCenterBannerRoot.transform, false);
        RectTransform headRt = headerObj.GetComponent<RectTransform>();
        headRt.anchorMin = new Vector2(0f, 0.68f);
        headRt.anchorMax = new Vector2(1f, 0.95f);
        headRt.offsetMin = Vector2.zero;
        headRt.offsetMax = Vector2.zero;
        bossCenterHeaderText = headerObj.GetComponent<TMPro.TextMeshProUGUI>();
        if (retroFont != null) bossCenterHeaderText.font = retroFont;
        bossCenterHeaderText.fontSize = 20f;
        bossCenterHeaderText.alignment = TMPro.TextAlignmentOptions.Center;
        bossCenterHeaderText.color = new Color(1f, 0.85f, 0.35f, 1f);
        bossCenterHeaderText.fontStyle = TMPro.FontStyles.Bold;
        bossCenterHeaderText.raycastTarget = false;

        // 2. TÃªn Boss to khá»•ng lá»“ rá»±c rá»¡ á»Ÿ giá»¯a (Big Boss Name)
        GameObject nameObj = new GameObject("BossName", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        nameObj.transform.SetParent(bossCenterBannerRoot.transform, false);
        RectTransform nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0f, 0.28f);
        nameRt.anchorMax = new Vector2(1f, 0.72f);
        nameRt.offsetMin = Vector2.zero;
        nameRt.offsetMax = Vector2.zero;
        bossCenterNameText = nameObj.GetComponent<TMPro.TextMeshProUGUI>();
        if (retroFont != null) bossCenterNameText.font = retroFont;
        bossCenterNameText.fontSize = 38f;
        bossCenterNameText.alignment = TMPro.TextAlignmentOptions.Center;
        bossCenterNameText.color = new Color(1f, 0.92f, 0.75f, 1f);
        bossCenterNameText.fontStyle = TMPro.FontStyles.Bold;
        bossCenterNameText.raycastTarget = false;
        bossCenterNameText.enableAutoSizing = true;
        bossCenterNameText.fontSizeMin = 26f;
        bossCenterNameText.fontSizeMax = 44f;

        // 3. Phá»¥ Ä‘á» tÆ°á»›c hiá»‡u Boss bÃªn dÆ°á»›i (Boss Subtitle)
        GameObject subObj = new GameObject("BossSubtitle", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        subObj.transform.SetParent(bossCenterBannerRoot.transform, false);
        RectTransform subRt = subObj.GetComponent<RectTransform>();
        subRt.anchorMin = new Vector2(0f, 0.06f);
        subRt.anchorMax = new Vector2(1f, 0.32f);
        subRt.offsetMin = Vector2.zero;
        subRt.offsetMax = Vector2.zero;
        bossCenterSubtitleText = subObj.GetComponent<TMPro.TextMeshProUGUI>();
        if (retroFont != null) bossCenterSubtitleText.font = retroFont;
        bossCenterSubtitleText.fontSize = 17f;
        bossCenterSubtitleText.alignment = TMPro.TextAlignmentOptions.Center;
        bossCenterSubtitleText.color = new Color(0.75f, 0.88f, 1f, 1f);
        bossCenterSubtitleText.fontStyle = TMPro.FontStyles.Italic;
        bossCenterSubtitleText.raycastTarget = false;

        bossCenterBannerRoot.SetActive(false);
    }

    public void ShowBossIntroBanner(string bossName, string bossSubtitle, Color elementColor)
    {
        if (bossCenterBannerRoot == null) CreateBossCenterBanner();
        if (bossCenterBannerCo != null) StopCoroutine(bossCenterBannerCo);
        bossCenterBannerCo = StartCoroutine(BossIntroBannerRoutine(bossName, bossSubtitle, elementColor));
    }

    private IEnumerator BossIntroBannerRoutine(string bossName, string bossSubtitle, Color elementColor)
    {
        if (bossCenterBannerRoot == null) yield break;

        bossCenterBannerRoot.SetActive(true);
        bossCenterCanvasGroup.alpha = 0f;
        bossCenterRect.localScale = Vector3.one * 0.7f;
        bossCenterRect.anchoredPosition = new Vector2(0f, 65f);

        bossCenterHeaderText.text = "â– â”€â”€â”€ â˜  B O S S   E N C O U N T E R â˜  â”€â”€â”€ â–";
        bossCenterHeaderText.color = new Color(1f, 0.35f, 0.3f, 1f);

        bossCenterNameText.text = bossName.ToUpper();
        bossCenterNameText.color = Color.white;

        bossCenterSubtitleText.text = string.IsNullOrEmpty(bossSubtitle) ? "âœ¦ ANCIENT APEX PREDATOR âœ¦" : bossSubtitle;
        bossCenterSubtitleText.color = new Color(0.85f, 0.92f, 1f, 0.95f);

        // KÃ­ch hoáº¡t hiá»‡u á»©ng Slow-motion ká»‹ch tÃ­nh & rung mÃ n hÃ¬nh
        StartCoroutine(BossSlowMoEffect(0.35f, 0.65f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.55f, 0.45f);
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);

        // Hiá»‡u á»©ng Zoom náº£y vÃ o (Scale Pop-in with Ease Out Back)
        float popInDuration = 0.35f;
        float elapsed = 0f;
        while (elapsed < popInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popInDuration);
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float ease = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
            bossCenterRect.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.0f, ease);
            bossCenterCanvasGroup.alpha = Mathf.Clamp01(t * 2.5f);
            yield return null;
        }

        bossCenterRect.localScale = Vector3.one;
        bossCenterCanvasGroup.alpha = 1f;

        // Giá»¯ thÃ´ng bÃ¡o hiá»ƒn thá»‹ rá»±c rá»¡ vÃ  nháº¥p nhÃ¡y mÃ u nguyÃªn tá»‘ cá»§a boss trong 3.2 giÃ¢y
        float holdDuration = 3.2f;
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = Mathf.PingPong(elapsed * 4.5f, 1f);
            bossCenterNameText.color = Color.Lerp(Color.white, elementColor, pulse);
            yield return null;
        }

        // LÆ°á»›t nháº¹ lÃªn vÃ  tan biáº¿n mÆ°á»£t mÃ  (Fade Out Glide)
        float fadeOutDuration = 0.45f;
        elapsed = 0f;
        Vector2 startPos = bossCenterRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, 40f);
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            bossCenterRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            bossCenterCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        bossCenterBannerRoot.SetActive(false);
    }

    public void ShowBossDefeatedBanner(string bossName, int bonusCoins)
    {
        if (bossCenterBannerRoot == null) CreateBossCenterBanner();
        if (bossCenterBannerCo != null) StopCoroutine(bossCenterBannerCo);
        bossCenterBannerCo = StartCoroutine(BossDefeatedBannerRoutine(bossName, bonusCoins));
    }

    private IEnumerator BossDefeatedBannerRoutine(string bossName, int bonusCoins)
    {
        if (bossCenterBannerRoot == null) yield break;

        bossCenterBannerRoot.SetActive(true);
        bossCenterCanvasGroup.alpha = 0f;
        bossCenterRect.localScale = Vector3.one * 0.7f;
        bossCenterRect.anchoredPosition = new Vector2(0f, 65f);

        bossCenterHeaderText.text = "â– â”€â”€â”€ âš”  B O S S   S L A I N !  âš” â”€â”€â”€ â–";
        bossCenterHeaderText.color = new Color(1f, 0.88f, 0.35f, 1f);

        bossCenterNameText.text = $"{bossName.ToUpper()} VANQUISHED!";
        bossCenterNameText.color = new Color(1f, 0.95f, 0.75f, 1f);

        bossCenterSubtitleText.text = $"âœ¦ +{bonusCoins} ROYAL GOLD  |  âš¡ CELESTIAL FRENZY (20s) âœ¦";
        bossCenterSubtitleText.color = new Color(0.35f, 1f, 0.55f, 1f);

        if (CameraController.instance != null) CameraController.instance.Shake(0.6f, 0.5f);
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(8);

        float popInDuration = 0.35f;
        float elapsed = 0f;
        while (elapsed < popInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popInDuration);
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float ease = 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
            bossCenterRect.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.0f, ease);
            bossCenterCanvasGroup.alpha = Mathf.Clamp01(t * 2.5f);
            yield return null;
        }

        bossCenterRect.localScale = Vector3.one;
        bossCenterCanvasGroup.alpha = 1f;

        float holdDuration = 3.2f;
        elapsed = 0f;
        while (elapsed < holdDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float pulse = Mathf.PingPong(elapsed * 5f, 1f);
            bossCenterNameText.color = Color.Lerp(new Color(1f, 0.95f, 0.75f, 1f), new Color(1f, 0.75f, 0.2f, 1f), pulse);
            yield return null;
        }

        float fadeOutDuration = 0.45f;
        elapsed = 0f;
        Vector2 startPos = bossCenterRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, 40f);
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            bossCenterRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            bossCenterCanvasGroup.alpha = 1f - t;
            yield return null;
        }

        bossCenterBannerRoot.SetActive(false);
    }

    public IEnumerator BossSlowMoEffect(float targetScale, float duration)
    {
        float prevScale = Time.timeScale;
        Time.timeScale = targetScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        float restoreTime = 0.35f;
        elapsed = 0f;
        while (elapsed < restoreTime)
        {
            elapsed += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Lerp(targetScale, 1.0f, elapsed / restoreTime);
            yield return null;
        }
        Time.timeScale = 1.0f;
    }
}