using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    private List<Image> weaponSlotIcons = new List<Image>();
    private int lastWeaponCount = -1;
    private float targetExpValue = 0f;
    private int currentDisplayLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        ApplySciFiLevelUpTheme();

        if (RunSave.continueRequested) // Came from the Main Menu's Continue button
        {
            RunSave.continueRequested = false;
            RunSaveData saved = RunSave.Load();
            if (saved != null) StartCoroutine(RunSave.Restore(saved));
        }
    }

    // Vampire-Survivors style level-up: a framed window with the title on top, three wide rows inside it
    // and a red Skip button beside it. Built at runtime from the existing panel objects.
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
                panelImg.color = new Color(0.02f, 0.03f, 0.07f, 0.7f); // Dim the game behind the window
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
            windowRt.sizeDelta = new Vector2(900f, 780f);
            Image windowImg = windowT.GetComponent<Image>();
            windowImg.sprite = Resources.Load<Sprite>("UI/UI_WindowFrame_Gold");
            windowImg.type = Image.Type.Sliced;
            windowImg.color = Color.white;

            TMP_Text[] allTexts = levelUpPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in allTexts)
            {
                if (t.name == "Upgrade Text")
                {
                    t.text = "Level Up!";
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.fontSize = 72;
                    t.characterSpacing = 4f;
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.alignment = TextAlignmentOptions.Center;
                    t.color = new Color(1f, 0.88f, 0.25f);
                    t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    t.rectTransform.sizeDelta = new Vector2(800f, 100f);
                    t.rectTransform.anchoredPosition = new Vector2(0f, 320f);
                }
                else if (t.transform.parent != null && t.transform.parent.name == "Skip Level Up")
                {
                    t.text = "SKIP";
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.fontSize = 48;
                    t.color = Color.white;
                }
            }

            // Red Skip button on the right of the window
            Transform skip = levelUpPanel.transform.Find("Skip Level Up");
            if (skip != null)
            {
                RectTransform skipRt = (RectTransform)skip;
                skipRt.anchorMin = skipRt.anchorMax = skipRt.pivot = new Vector2(0.5f, 0.5f);
                skipRt.sizeDelta = new Vector2(280f, 92f);
                skipRt.anchoredPosition = new Vector2(640f, 140f);
                Image skipImg = skip.GetComponent<Image>();
                Sprite redButton = Resources.Load<Sprite>("UI/UI_Button_Back");
                if (skipImg != null && redButton != null)
                {
                    skipImg.sprite = redButton;
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
                    levelUpButtons[i].SetLayout(new Vector2(820f, 178f), new Vector2(0f, 135f - i * 192f));
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
                    fillImg.color = Color.white;
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
            lvlRT.sizeDelta = new Vector2(100f, 22f);

            if (retroFont != null) explvlText.font = retroFont;
            explvlText.fontSize = 22;
            explvlText.alignment = TextAlignmentOptions.Right;
            explvlText.color = Color.white;
            explvlText.fontStyle = FontStyles.Bold;
            explvlText.enableAutoSizing = false;
            explvlText.margin = Vector4.zero;
            explvlText.text = "LV " + (ExperienceLevelController.instance != null ? ExperienceLevelController.instance.currentLevel : 1);
        }

        // 5. Timer Text ("0:05" centered on HUD row)
        if (timeText != null)
        {
            RectTransform timeRT = timeText.GetComponent<RectTransform>();
            timeRT.anchorMin = new Vector2(0.5f, 1f);
            timeRT.anchorMax = new Vector2(0.5f, 1f);
            timeRT.pivot = new Vector2(0.5f, 0.5f);
            timeRT.anchoredPosition = new Vector2(0f, -54f);
            timeRT.sizeDelta = new Vector2(180f, 38f);

            if (retroFont != null) timeText.font = retroFont;
            timeText.fontSize = 42;
            timeText.alignment = TextAlignmentOptions.Center;
            timeText.color = Color.white;
            timeText.fontStyle = FontStyles.Bold;
            timeText.enableAutoSizing = false;
            timeText.margin = Vector4.zero;
            timeText.text = "0:00";
        }

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

        // 7. Coin Display (Icon + Number to the left of Pause Button)
        if (coinImage == null && coinText != null)
        {
            Transform imgT = coinText.transform.parent.Find("Coin Image");
            if (imgT != null) coinImage = imgT.GetComponent<Image>();
        }

        if (coinImage != null)
        {
            RectTransform cImgRT = coinImage.GetComponent<RectTransform>();
            cImgRT.anchorMin = new Vector2(1f, 1f);
            cImgRT.anchorMax = new Vector2(1f, 1f);
            cImgRT.pivot = new Vector2(1f, 0.5f);
            cImgRT.anchoredPosition = new Vector2(-48f, -54f);
            cImgRT.sizeDelta = new Vector2(20f, 20f);

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
            cTextRT.anchoredPosition = new Vector2(-74f, -54f);
            cTextRT.sizeDelta = new Vector2(85f, 30f);

            if (retroFont != null) coinText.font = retroFont;
            coinText.fontSize = 30;
            coinText.alignment = TextAlignmentOptions.Right;
            coinText.color = Color.white;
            coinText.fontStyle = FontStyles.Bold;
            coinText.enableAutoSizing = false;
            coinText.margin = Vector4.zero;
            coinText.text = CoinController.instance != null ? CoinController.instance.currentCoins.ToString() : "0";
        }

        // 8. Monster Kill Counter (Skull Icon + Number to the left of Coins)
        Transform existingKillCounter = canvasT.Find("Kill Counter Container");
        GameObject killContainer;
        if (existingKillCounter == null)
        {
            killContainer = new GameObject("Kill Counter Container", typeof(RectTransform));
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
        kcRT.anchoredPosition = new Vector2(-172f, -54f);
        kcRT.sizeDelta = new Vector2(115f, 30f);

        Transform skullChild = killContainer.transform.Find("Skull Icon");
        GameObject skullGO = skullChild != null ? skullChild.gameObject : new GameObject("Skull Icon", typeof(RectTransform), typeof(Image));
        if (skullChild == null) skullGO.transform.SetParent(killContainer.transform, false);

        killImage = skullGO.GetComponent<Image>();
        if (skullIcon != null) killImage.sprite = skullIcon;
        killImage.preserveAspect = true;
        RectTransform skullRT = skullGO.GetComponent<RectTransform>();
        skullRT.anchorMin = new Vector2(1f, 0.5f);
        skullRT.anchorMax = new Vector2(1f, 0.5f);
        skullRT.pivot = new Vector2(1f, 0.5f);
        skullRT.anchoredPosition = Vector2.zero;
        skullRT.sizeDelta = new Vector2(20f, 20f);

        Transform killTextChild = killContainer.transform.Find("Kill Text");
        GameObject killTextGO = killTextChild != null ? killTextChild.gameObject : new GameObject("Kill Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (killTextChild == null) killTextGO.transform.SetParent(killContainer.transform, false);

        killText = killTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) killText.font = retroFont;
        killText.fontSize = 30;
        killText.alignment = TextAlignmentOptions.Right;
        killText.color = Color.white;
        killText.fontStyle = FontStyles.Bold;
        killText.enableAutoSizing = false;
        killText.margin = Vector4.zero;
        killText.text = killCount.ToString();

        RectTransform killTextRT = killTextGO.GetComponent<RectTransform>();
        killTextRT.anchorMin = Vector2.zero;
        killTextRT.anchorMax = Vector2.one;
        killTextRT.offsetMin = Vector2.zero;
        killTextRT.offsetMax = new Vector2(-26f, 0f);

        // 9. Weapon / Item Inventory Slots (Top-Left under top bar)
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
        scRT.anchoredPosition = new Vector2(12f, -54f);
        scRT.sizeDelta = new Vector2(210f, 32f);

        weaponSlotIcons.Clear();
        int maxSlots = 6;
        float slotSize = 30f;
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
            slotFrame.color = new Color(1f, 1f, 1f, 0.45f);

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
            iconRT.sizeDelta = new Vector2(22f, 22f);

            Image iconImg = iconGO.GetComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.enabled = false;

            weaponSlotIcons.Add(iconImg);
        }

        // 10. Bring popups (LevelUp, Pause, End) on top of HUD
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
            }
            else
            {
                weaponSlotIcons[i].enabled = false;
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
            pauseScreen.SetActive(true); // Set the pause screen to the opposite of itself. GK
            Time.timeScale = 0f; // Set the time scale to 0. GK
        }
        else
        {
            pauseScreen.SetActive(false); // Set the pause screen to the opposite of itself. GK
            if (levelUpPanel.activeSelf == false) // If the level up panel is not active. GK
            {
                Time.timeScale = 1f; // Set the time scale to 1. GK 

            }
        }
    }
}