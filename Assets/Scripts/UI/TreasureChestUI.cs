using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AlienSurvivors.Upgrades;

public class TreasureChestUI : MonoBehaviour
{
    public static TreasureChestUI instance;

    [Header("UI References")]
    private GameObject chestModalRoot;
    private GameObject windowGO;
    private Transform sunburstTransform;
    private TMP_Text titleText;
    private TMP_Text coinBonusText;
    private Image coinIconImg;
    private Transform itemsContainer;
    private Button collectButton;
    private TMP_Text collectButtonText;
    private TMP_FontAsset retroFont;
    private TMP_FontAsset smoothFont;

    private Coroutine cardsAnimationCo;
    private List<GameObject> spawnedCards = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        EnsureUIExists();
    }

    private void Update()
    {
        // Xoay tia sáng hoàng kim trong nền khi hòm mở
        if (chestModalRoot != null && chestModalRoot.activeSelf && sunburstTransform != null)
        {
            sunburstTransform.Rotate(0f, 0f, -22f * Time.unscaledDeltaTime);
        }

        // Hiệu ứng thở nhẹ của nút Collect để kích thích bấm
        if (chestModalRoot != null && chestModalRoot.activeSelf && collectButton != null)
        {
            float pulse = 1f + 0.035f * Mathf.Sin(Time.unscaledTime * 5f);
            collectButton.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }

    public void EnsureUIExists()
    {
        if (chestModalRoot != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindAnyObjectByType<Canvas>();
        }
        if (canvas == null) return;

        // Tải Font retro đồng bộ
        if (retroFont == null)
        {
            retroFont = Resources.Load<TMP_FontAsset>("Fonts/Kenney Pixel SDF - Outline");
            if (retroFont == null)
            {
                TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
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
        smoothFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        Sprite windowFrame = Resources.Load<Sprite>("UI/VS_Window_Gold_Frame") ?? Resources.Load<Sprite>("UI/UI_WindowFrame_Gold");
        Sprite buttonGreen = Resources.Load<Sprite>("UI/VS_Button_PowerUp_Green") ?? Resources.Load<Sprite>("UI/UI_Button_Confirm");
        Sprite coinCapSprite = Resources.Load<Sprite>("UI/VS_Coin_Capsule_Bar") ?? Resources.Load<Sprite>("UI/UI_Coin_Box");
        Sprite coinIcon = Resources.Load<Sprite>("UI/UI_Coin_Icon");

        // 1. Overlay nền mờ che phủ toàn màn hình
        chestModalRoot = new GameObject("Treasure Chest Modal", typeof(RectTransform), typeof(Image));
        chestModalRoot.transform.SetParent(canvas.transform, false);
        chestModalRoot.transform.SetAsLastSibling();

        RectTransform rootRT = chestModalRoot.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;

        Image rootBg = chestModalRoot.GetComponent<Image>();
        rootBg.color = new Color(0.015f, 0.02f, 0.05f, 0.92f);
        rootBg.raycastTarget = true;

        // 2. Cửa sổ nội dung chính (Kích thước lớn 1060 x 760 chuẩn Vampire Survivors)
        windowGO = new GameObject("Chest Window", typeof(RectTransform), typeof(Image));
        windowGO.transform.SetParent(chestModalRoot.transform, false);

        RectTransform winRT = windowGO.GetComponent<RectTransform>();
        winRT.anchorMin = new Vector2(0.5f, 0.5f);
        winRT.anchorMax = new Vector2(0.5f, 0.5f);
        winRT.pivot = new Vector2(0.5f, 0.5f);
        winRT.anchoredPosition = Vector2.zero;
        winRT.sizeDelta = new Vector2(1060f, 760f);

        Image winImg = windowGO.GetComponent<Image>();
        winImg.sprite = UIController.GetGothicWindowSprite();
        winImg.type = Image.Type.Sliced;
        winImg.color = Color.white;

        // 2B. Nền hoa văn / Tia sáng mặt trời tỏa hào quang (Rotating Sunburst Rays)
        GameObject sunburstGO = new GameObject("Sunburst Rays", typeof(RectTransform), typeof(Image));
        sunburstGO.transform.SetParent(windowGO.transform, false);
        sunburstTransform = sunburstGO.transform;
        sunburstTransform.SetAsFirstSibling();

        RectTransform sbRT = sunburstGO.GetComponent<RectTransform>();
        sbRT.anchorMin = sbRT.anchorMax = sbRT.pivot = new Vector2(0.5f, 0.5f);
        sbRT.anchoredPosition = new Vector2(0f, 15f);
        sbRT.sizeDelta = new Vector2(950f, 950f);

        Image sbImg = sunburstGO.GetComponent<Image>();
        sbImg.sprite = CreateProceduralSunburstSprite();
        sbImg.color = new Color(1f, 0.85f, 0.25f, 0.08f);
        sbImg.raycastTarget = false;

        // 3. Tiêu đề Hòm báu (Vàng kim rực rỡ, chữ to đậm)
        GameObject titleGO = new GameObject("Title Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(windowGO.transform, false);

        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -28f);
        titleRT.sizeDelta = new Vector2(980f, 70f);

        titleText = titleGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) titleText.font = retroFont;
        titleText.fontSize = 54;
        titleText.fontStyle = FontStyles.Bold;
        titleText.characterSpacing = 5f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.88f, 0.25f);
        titleText.text = "✦  ROYAL TREASURE CHEST!  ✦";

        // 4. Bảng viên nang hiển thị Coin thưởng (Gold Capsule Placard)
        GameObject coinCapGO = new GameObject("Coin Capsule", typeof(RectTransform), typeof(Image));
        coinCapGO.transform.SetParent(windowGO.transform, false);

        RectTransform coinCapRT = coinCapGO.GetComponent<RectTransform>();
        coinCapRT.anchorMin = new Vector2(0.5f, 1f);
        coinCapRT.anchorMax = new Vector2(0.5f, 1f);
        coinCapRT.pivot = new Vector2(0.5f, 1f);
        coinCapRT.anchoredPosition = new Vector2(0f, -98f);
        coinCapRT.sizeDelta = new Vector2(460f, 48f);

        Image coinCapImg = coinCapGO.GetComponent<Image>();
        if (coinCapSprite != null)
        {
            coinCapImg.sprite = coinCapSprite;
            coinCapImg.type = Image.Type.Sliced;
        }
        coinCapImg.color = new Color(0.95f, 0.75f, 0.15f, 0.95f);

        // Icon đồng tiền vàng trong viên nang
        GameObject coinIconGO = new GameObject("Coin Icon", typeof(RectTransform), typeof(Image));
        coinIconGO.transform.SetParent(coinCapGO.transform, false);
        RectTransform ciRT = coinIconGO.GetComponent<RectTransform>();
        ciRT.anchorMin = new Vector2(0f, 0.5f);
        ciRT.anchorMax = new Vector2(0f, 0.5f);
        ciRT.pivot = new Vector2(0f, 0.5f);
        ciRT.anchoredPosition = new Vector2(16f, 0f);
        ciRT.sizeDelta = new Vector2(30f, 30f);
        coinIconImg = coinIconGO.GetComponent<Image>();
        if (coinIcon != null) coinIconImg.sprite = coinIcon;
        coinIconImg.preserveAspect = true;

        // Dòng chữ số tiền thưởng
        GameObject coinTextGO = new GameObject("Coin Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinTextGO.transform.SetParent(coinCapGO.transform, false);
        RectTransform ctRT = coinTextGO.GetComponent<RectTransform>();
        ctRT.anchorMin = Vector2.zero;
        ctRT.anchorMax = Vector2.one;
        ctRT.offsetMin = new Vector2(54f, 0f);
        ctRT.offsetMax = new Vector2(-16f, 0f);
        coinBonusText = coinTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) coinBonusText.font = retroFont;
        coinBonusText.fontSize = 28;
        coinBonusText.fontStyle = FontStyles.Bold;
        coinBonusText.alignment = TextAlignmentOptions.Center;
        coinBonusText.color = new Color(1f, 0.98f, 0.65f);
        coinBonusText.text = "+250 GOLD COINS AWARDED!";

        // 5. Container danh sách các thẻ phần thưởng (Kích thước rộng 980 x 440)
        GameObject itemsGO = new GameObject("Items Container", typeof(RectTransform));
        itemsGO.transform.SetParent(windowGO.transform, false);

        RectTransform itemsRT = itemsGO.GetComponent<RectTransform>();
        itemsRT.anchorMin = new Vector2(0.5f, 0.5f);
        itemsRT.anchorMax = new Vector2(0.5f, 0.5f);
        itemsRT.pivot = new Vector2(0.5f, 0.5f);
        itemsRT.anchoredPosition = new Vector2(0f, -8f);
        itemsRT.sizeDelta = new Vector2(980f, 440f);
        itemsContainer = itemsGO.transform;

        // 6. Nút Collect & Resume lớn, hoành tráng (Size 480 x 78)
        GameObject btnGO = new GameObject("Collect Button", typeof(RectTransform), typeof(Button), typeof(Image));
        btnGO.transform.SetParent(windowGO.transform, false);

        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f);
        btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.anchoredPosition = new Vector2(0f, 38f);
        btnRT.sizeDelta = new Vector2(480f, 78f);

        Image btnImg = btnGO.GetComponent<Image>();
        btnImg.sprite = MainMenu.CreateRoyalGothicButton(
            new Color(0.04f, 0.18f, 0.09f),
            new Color(0.10f, 0.36f, 0.18f),
            new Color(0.88f, 0.74f, 0.25f),
            new Color(1.0f, 0.96f, 0.55f),
            new Color(0.20f, 0.98f, 0.45f)
        );
        btnImg.type = Image.Type.Sliced;
        btnImg.color = Color.white;

        collectButton = btnGO.GetComponent<Button>();
        collectButton.onClick.RemoveAllListeners();
        collectButton.onClick.AddListener(CloseChest);

        if (btnGO.GetComponent<GothicMenuButtonEffects>() == null)
        {
            btnGO.AddComponent<GothicMenuButtonEffects>();
        }

        GameObject btnTextGO = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextGO.transform.SetParent(btnGO.transform, false);

        collectButtonText = btnTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) collectButtonText.font = retroFont;
        collectButtonText.fontSize = 32;
        collectButtonText.fontStyle = FontStyles.Bold;
        collectButtonText.characterSpacing = 3.5f;
        collectButtonText.alignment = TextAlignmentOptions.Center;
        collectButtonText.textWrappingMode = TextWrappingModes.NoWrap;
        collectButtonText.color = new Color(0.95f, 1f, 0.95f);
        collectButtonText.text = "✦  COLLECT REWARDS  ✦";

        RectTransform btRT = btnTextGO.GetComponent<RectTransform>();
        btRT.anchorMin = Vector2.zero;
        btRT.anchorMax = Vector2.one;
        btRT.offsetMin = Vector2.zero;
        btRT.offsetMax = Vector2.zero;

        chestModalRoot.SetActive(false);
    }

    /// <summary>
    /// Mở Hòm Kho Báu với hiệu ứng âm thanh, danh sách phần thưởng to lớn, rõ ràng.
    /// </summary>
    public void OpenChest(int chestLevel)
    {
        EnsureUIExists();
        if (chestModalRoot == null) return;

        // Tạm dừng thời gian gameplay
        Time.timeScale = 0f;

        int itemCount = 1;
        int coinsBonus = 50;

        if (chestLevel == 1) // Mini-Boss Chest
        {
            float roll = Random.value;
            if (roll < 0.65f)
            {
                itemCount = 1;
                titleText.text = "✦  SILVER TREASURE CHEST!  ✦";
                titleText.color = new Color(0.85f, 0.95f, 1f);
                coinsBonus = Random.Range(70, 140);
            }
            else
            {
                itemCount = 3;
                titleText.text = "✦  GOLDEN ROYAL CHEST!  ✦";
                titleText.color = new Color(1f, 0.88f, 0.25f);
                coinsBonus = Random.Range(180, 280);
            }
        }
        else // Major Boss Chest
        {
            float roll = Random.value;
            if (roll < 0.50f)
            {
                itemCount = 3;
                titleText.text = "★  EPIC BOSS CHEST!  ★";
                titleText.color = new Color(1f, 0.82f, 0.2f);
                coinsBonus = Random.Range(240, 380);
            }
            else
            {
                itemCount = 5;
                titleText.text = "★★★  JACKPOT SUPER CHEST!  ★★★";
                titleText.color = new Color(1f, 0.45f, 0.95f);
                coinsBonus = Random.Range(450, 700);
            }
        }

        coinBonusText.text = $"+{coinsBonus} GOLD COINS AWARDED!";

        // Thêm vàng vào ngân khố
        if (CoinController.instance != null)
        {
            CoinController.instance.AddCoins(coinsBonus);
        }

        // Lấy danh sách nâng cấp hợp lệ
        List<UpgradeData> awardedCards = new List<UpgradeData>();
        if (UpgradeManager.instance != null)
        {
            awardedCards = UpgradeManager.instance.GetRandomUpgrades(itemCount);
            foreach (var card in awardedCards)
            {
                if (card != null)
                {
                    card.ApplyUpgrade();
                }
            }
        }

        // Cập nhật lại thanh kho đồ HUD ngay khi mở hòm
        if (UIController.instance != null)
        {
            UIController.instance.UpdateWeaponSlots();
            UIController.instance.UpdatePassiveSlots();
        }

        // Dọn sạch danh sách thẻ cũ
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedCards.Clear();

        // Kích hoạt giao diện
        chestModalRoot.SetActive(true);
        chestModalRoot.transform.SetAsLastSibling();

        if (windowGO != null)
        {
            windowGO.transform.localScale = Vector3.one * 0.85f;
        }

        // Chạy animation mở hòm xuất hiện thẻ
        if (cardsAnimationCo != null) StopCoroutine(cardsAnimationCo);
        cardsAnimationCo = StartCoroutine(AnimateChestOpen(awardedCards));

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(2);
        }
    }

    private IEnumerator AnimateChestOpen(List<UpgradeData> awardedCards)
    {
        // 1. Zoom in mượt mà cửa sổ hòm báu
        float introTime = 0.22f;
        float elapsed = 0f;
        while (elapsed < introTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / introTime);
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            if (windowGO != null)
            {
                windowGO.transform.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, ease);
            }
            yield return null;
        }
        if (windowGO != null) windowGO.transform.localScale = Vector3.one;

        // 2. Kích thước thẻ lớn, rõ ràng
        Sprite cardBgSprite = Resources.Load<Sprite>("UI/VS_Card_Stone_Slot") ?? Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        Sprite slotFrame = Resources.Load<Sprite>("UI/VS_HUD_Slot");

        int total = awardedCards != null ? awardedCards.Count : 0;
        if (total == 0)
        {
            SpawnMaxedCardsMessage();
            yield break;
        }

        // Điều chỉnh chiều rộng thẻ theo số lượng (1 thẻ: 320px, 3 thẻ: 260px, 5 thẻ: 180px)
        float cardWidth = total == 1 ? 320f : (total <= 3 ? 260f : 180f);
        float cardHeight = 360f;
        float spacing = total <= 3 ? 32f : 16f;
        float startX = -((total - 1) * (cardWidth + spacing)) / 2f;

        for (int i = 0; i < total; i++)
        {
            UpgradeData card = awardedCards[i];
            if (card == null) continue;

            GameObject cardGO = new GameObject("ChestCard_" + i, typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(itemsContainer, false);
            spawnedCards.Add(cardGO);

            RectTransform cRT = cardGO.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0f);
            cRT.sizeDelta = new Vector2(cardWidth, cardHeight);
            cRT.localScale = Vector3.zero; // Bắt đầu từ 0 để phóng to

            Image cImg = cardGO.GetComponent<Image>();
            cImg.sprite = cardBgSprite != null ? cardBgSprite : Resources.Load<Sprite>("UI/UI_Card_Normal");
            cImg.type = Image.Type.Sliced;
            cImg.color = Color.white;

            // Inner gold hairline border for gothic elevation
            GameObject innerBorder = new GameObject("CardInnerGlow", typeof(RectTransform), typeof(Image));
            innerBorder.transform.SetParent(cardGO.transform, false);
            RectTransform ibRT = innerBorder.GetComponent<RectTransform>();
            ibRT.anchorMin = Vector2.zero;
            ibRT.anchorMax = Vector2.one;
            ibRT.offsetMin = new Vector2(4f, 4f);
            ibRT.offsetMax = new Vector2(-4f, -4f);
            Image ibImg = innerBorder.GetComponent<Image>();
            ibImg.sprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
            ibImg.type = Image.Type.Sliced;
            ibImg.color = new Color(1f, 0.88f, 0.35f, 0.35f);
            ibImg.raycastTarget = false;

            // Khung Icon to đẹp
            float iconBoxSize = total <= 3 ? 108f : 86f;
            GameObject iconFrameGO = new GameObject("IconFrame", typeof(RectTransform), typeof(Image));
            iconFrameGO.transform.SetParent(cardGO.transform, false);
            RectTransform ifRT = iconFrameGO.GetComponent<RectTransform>();
            ifRT.anchorMin = new Vector2(0.5f, 1f);
            ifRT.anchorMax = new Vector2(0.5f, 1f);
            ifRT.pivot = new Vector2(0.5f, 1f);
            ifRT.anchoredPosition = new Vector2(0f, -22f);
            ifRT.sizeDelta = new Vector2(iconBoxSize, iconBoxSize);

            Image ifImg = iconFrameGO.GetComponent<Image>();
            if (slotFrame != null)
            {
                ifImg.sprite = slotFrame;
                ifImg.type = Image.Type.Sliced;
            }
            ifImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

            // Icon món đồ
            if (card.icon != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(iconFrameGO.transform, false);

                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = Vector2.zero;
                iconRT.anchorMax = Vector2.one;
                iconRT.offsetMin = new Vector2(8f, 8f);
                iconRT.offsetMax = new Vector2(-8f, -8f);

                Image iconImg = iconGO.GetComponent<Image>();
                iconImg.sprite = card.icon;
                iconImg.preserveAspect = true;
            }

            // Tên món đồ (Vàng hoàng kim nổi bật)
            GameObject nameGO = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(cardGO.transform, false);
            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0.44f);
            nameRT.anchorMax = new Vector2(1f, 0.64f);
            nameRT.offsetMin = new Vector2(12f, 0f);
            nameRT.offsetMax = new Vector2(-12f, 0f);

            TextMeshProUGUI nameTMP = nameGO.GetComponent<TextMeshProUGUI>();
            if (smoothFont != null) nameTMP.font = smoothFont;
            nameTMP.fontSize = total <= 3 ? 24 : 19;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = new Color(1f, 0.90f, 0.35f);
            nameTMP.text = card.upgradeName.ToUpper();
            nameTMP.textWrappingMode = TextWrappingModes.Normal;

            // Huy hiệu Cấp độ & Level Pips
            GameObject badgeGO = new GameObject("BadgeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            badgeGO.transform.SetParent(cardGO.transform, false);
            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0f, 0.28f);
            badgeRT.anchorMax = new Vector2(1f, 0.44f);
            badgeRT.offsetMin = new Vector2(10f, 0f);
            badgeRT.offsetMax = new Vector2(-10f, 0f);

            TextMeshProUGUI badgeTMP = badgeGO.GetComponent<TextMeshProUGUI>();
            if (smoothFont != null) badgeTMP.font = smoothFont;
            badgeTMP.fontSize = total <= 3 ? 20 : 16;
            badgeTMP.fontStyle = FontStyles.Bold;
            badgeTMP.alignment = TextAlignmentOptions.Center;

            int curLvl = 1;
            int maxLvl = 8;
            if (card is UpgradeStatUpgradeData statUp)
            {
                curLvl = statUp.currentLevel;
                maxLvl = statUp.maxLevel;
            }
            string pips = LevelUpSellectionButton.BuildLevelPips(curLvl, maxLvl);
            badgeTMP.text = $"<color=#00FFAA><b>[ +1 LEVEL ]</b></color>\n<size=85%>{pips}</size>";

            // Mô tả hiệu ứng ngắn
            GameObject descGO = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
            descGO.transform.SetParent(cardGO.transform, false);
            RectTransform descRT = descGO.GetComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0f, 0.05f);
            descRT.anchorMax = new Vector2(1f, 0.28f);
            descRT.offsetMin = new Vector2(12f, 0f);
            descRT.offsetMax = new Vector2(-12f, 0f);

            TextMeshProUGUI descTMP = descGO.GetComponent<TextMeshProUGUI>();
            if (smoothFont != null) descTMP.font = smoothFont;
            descTMP.fontSize = total <= 3 ? 17 : 14;
            descTMP.alignment = TextAlignmentOptions.Top;
            descTMP.color = new Color(0.9f, 0.92f, 0.98f);
            descTMP.text = card.GetDescription();
            descTMP.textWrappingMode = TextWrappingModes.Normal;

            // Animation từng thẻ nảy ra (Staggered Pop-In)
            float popElapsed = 0f;
            float popDuration = 0.24f;
            while (popElapsed < popDuration)
            {
                popElapsed += Time.unscaledDeltaTime;
                float pt = Mathf.Clamp01(popElapsed / popDuration);
                float scale = EaseOutBack(pt);
                cRT.localScale = Vector3.one * scale;
                yield return null;
            }
            cRT.localScale = Vector3.one;

            if (SFXManager.instance != null)
            {
                SFXManager.instance.PlaySFXPitched(0);
            }

            yield return new WaitForSecondsRealtime(0.06f);
        }
    }

    private void SpawnMaxedCardsMessage()
    {
        GameObject emptyGO = new GameObject("MaxedText", typeof(RectTransform), typeof(TextMeshProUGUI));
        emptyGO.transform.SetParent(itemsContainer, false);
        RectTransform eRT = emptyGO.GetComponent<RectTransform>();
        eRT.anchorMin = Vector2.zero;
        eRT.anchorMax = Vector2.one;
        eRT.offsetMin = Vector2.zero;
        eRT.offsetMax = Vector2.zero;

        TextMeshProUGUI eTMP = emptyGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) eTMP.font = retroFont;
        eTMP.fontSize = 32;
        eTMP.alignment = TextAlignmentOptions.Center;
        eTMP.color = new Color(0.9f, 0.9f, 0.9f);
        eTMP.text = "<color=#00FFAA>★ ALL UPGRADES MAXED OUT! ★</color>\n\n<color=#FFE040>+500 EXTRA ROYAL COINS AWARDED!</color>";

        if (CoinController.instance != null) CoinController.instance.AddCoins(500);
    }

    private static Sprite CreateProceduralSunburstSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] p = new Color[size * size];

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.48f;
        int rayCount = 12;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 diff = new Vector2(x, y) - center;
                float dist = diff.magnitude;
                if (dist <= radius)
                {
                    float angle = Mathf.Atan2(diff.y, diff.x);
                    float rayPattern = Mathf.Cos(angle * rayCount);
                    float rayAlpha = Mathf.Clamp01(rayPattern * 0.5f + 0.5f);
                    float falloff = 1f - (dist / radius);
                    p[y * size + x] = new Color(1f, 0.9f, 0.4f, rayAlpha * falloff * 0.85f);
                }
                else
                {
                    p[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    private static Sprite CreateProceduralEmeraldButtonSprite()
    {
        int w = 64;
        int h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color darkGreen = new Color(0.04f, 0.22f, 0.12f, 1f);
        Color midGreen = new Color(0.08f, 0.45f, 0.25f, 1f);
        Color lightGreen = new Color(0.15f, 0.70f, 0.38f, 1f);
        Color highlight = new Color(0.40f, 0.95f, 0.60f, 1f);
        Color goldBorder = new Color(0.95f, 0.80f, 0.25f, 1f);
        Color darkGold = new Color(0.45f, 0.32f, 0.08f, 1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    p[y * w + x] = darkGold;
                }
                else if (x == 1 || x == w - 2 || y == 1 || y == h - 2)
                {
                    p[y * w + x] = goldBorder;
                }
                else if (y >= h - 4)
                {
                    p[y * w + x] = highlight;
                }
                else if (y >= h / 2)
                {
                    p[y * w + x] = lightGreen;
                }
                else if (y >= 4)
                {
                    p[y * w + x] = midGreen;
                }
                else
                {
                    p[y * w + x] = darkGreen;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
    }

    private static float EaseOutBack(float p)
    {
        p = Mathf.Clamp01(p);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(p - 1f, 3f) + c1 * Mathf.Pow(p - 1f, 2f);
    }

    public void CloseChest()
    {
        if (cardsAnimationCo != null) StopCoroutine(cardsAnimationCo);

        if (chestModalRoot != null)
        {
            chestModalRoot.SetActive(false);
        }

        Time.timeScale = 1f; // Tiếp tục ván đấu

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(1);
        }
    }
}
