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
    private TMP_Text titleText;
    private TMP_Text coinBonusText;
    private Transform itemsContainer;
    private Button collectButton;
    private TMP_FontAsset retroFont;

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

    public void EnsureUIExists()
    {
        if (chestModalRoot != null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
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

        Sprite windowFrame = Resources.Load<Sprite>("UI/VS_Window_Gold_Frame") ?? Resources.Load<Sprite>("UI/UI_WindowFrame_Gold");
        Sprite buttonGreen = Resources.Load<Sprite>("UI/VS_Button_PowerUp_Green") ?? Resources.Load<Sprite>("UI/UI_Button_Confirm");
        Sprite slotFrame = Resources.Load<Sprite>("UI/VS_HUD_Slot");

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
        rootBg.color = new Color(0.02f, 0.04f, 0.08f, 0.88f);

        // 2. Cửa sổ nội dung chính
        GameObject windowGO = new GameObject("Chest Window", typeof(RectTransform), typeof(Image));
        windowGO.transform.SetParent(chestModalRoot.transform, false);

        RectTransform winRT = windowGO.GetComponent<RectTransform>();
        winRT.anchorMin = new Vector2(0.5f, 0.5f);
        winRT.anchorMax = new Vector2(0.5f, 0.5f);
        winRT.pivot = new Vector2(0.5f, 0.5f);
        winRT.anchoredPosition = Vector2.zero;
        winRT.sizeDelta = new Vector2(620f, 480f);

        Image winImg = windowGO.GetComponent<Image>();
        if (windowFrame != null)
        {
            winImg.sprite = windowFrame;
            winImg.type = Image.Type.Sliced;
        }
        winImg.color = new Color(0.12f, 0.14f, 0.22f, 0.98f);

        // 3. Tiêu đề Hòm báu
        GameObject titleGO = new GameObject("Title Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(windowGO.transform, false);

        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.5f, 1f);
        titleRT.anchorMax = new Vector2(0.5f, 1f);
        titleRT.pivot = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -22f);
        titleRT.sizeDelta = new Vector2(580f, 55f);

        titleText = titleGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) titleText.font = retroFont;
        titleText.fontSize = 38;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.88f, 0.25f);
        titleText.text = "★ TREASURE CHEST! ★";

        // 4. Dòng hiển thị Coin thưởng
        GameObject coinGO = new GameObject("Coin Bonus Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinGO.transform.SetParent(windowGO.transform, false);

        RectTransform coinRT = coinGO.GetComponent<RectTransform>();
        coinRT.anchorMin = new Vector2(0.5f, 1f);
        coinRT.anchorMax = new Vector2(0.5f, 1f);
        coinRT.pivot = new Vector2(0.5f, 1f);
        coinRT.anchoredPosition = new Vector2(0f, -75f);
        coinRT.sizeDelta = new Vector2(500f, 32f);

        coinBonusText = coinGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) coinBonusText.font = retroFont;
        coinBonusText.fontSize = 24;
        coinBonusText.alignment = TextAlignmentOptions.Center;
        coinBonusText.color = new Color(1f, 0.95f, 0.65f);
        coinBonusText.text = "+150 GOLD COINS";

        // 5. Container danh sách các món nhận được
        GameObject itemsGO = new GameObject("Items Container", typeof(RectTransform));
        itemsGO.transform.SetParent(windowGO.transform, false);

        RectTransform itemsRT = itemsGO.GetComponent<RectTransform>();
        itemsRT.anchorMin = new Vector2(0.5f, 0.5f);
        itemsRT.anchorMax = new Vector2(0.5f, 0.5f);
        itemsRT.pivot = new Vector2(0.5f, 0.5f);
        itemsRT.anchoredPosition = new Vector2(0f, -8f);
        itemsRT.sizeDelta = new Vector2(540f, 250f);
        itemsContainer = itemsGO.transform;

        // 6. Nút Collect & Resume
        GameObject btnGO = new GameObject("Collect Button", typeof(RectTransform), typeof(Button), typeof(Image));
        btnGO.transform.SetParent(windowGO.transform, false);

        RectTransform btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0f);
        btnRT.anchorMax = new Vector2(0.5f, 0f);
        btnRT.pivot = new Vector2(0.5f, 0f);
        btnRT.anchoredPosition = new Vector2(0f, 24f);
        btnRT.sizeDelta = new Vector2(260f, 54f);

        Image btnImg = btnGO.GetComponent<Image>();
        if (buttonGreen != null)
        {
            btnImg.sprite = buttonGreen;
            btnImg.type = Image.Type.Sliced;
        }
        btnImg.color = new Color(0.18f, 0.65f, 0.28f);

        collectButton = btnGO.GetComponent<Button>();
        collectButton.onClick.AddListener(CloseChest);

        GameObject btnTextGO = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextGO.transform.SetParent(btnGO.transform, false);

        TextMeshProUGUI btnTMP = btnTextGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) btnTMP.font = retroFont;
        btnTMP.fontSize = 28;
        btnTMP.fontStyle = FontStyles.Bold;
        btnTMP.alignment = TextAlignmentOptions.Center;
        btnTMP.color = Color.white;
        btnTMP.text = "COLLECT >>";

        RectTransform btRT = btnTextGO.GetComponent<RectTransform>();
        btRT.anchorMin = Vector2.zero;
        btRT.anchorMax = Vector2.one;
        btRT.offsetMin = Vector2.zero;
        btRT.offsetMax = Vector2.zero;

        chestModalRoot.SetActive(false);
    }

    /// <summary>
    /// Mở Hòm Kho Báu và trao thưởng ngẫu nhiên.
    /// </summary>
    public void OpenChest(int chestLevel)
    {
        EnsureUIExists();
        if (chestModalRoot == null) return;

        // Tạm dừng game
        Time.timeScale = 0f;

        // Xác định số lượng vật phẩm và coin thưởng
        int itemCount = 1;
        int coinsBonus = 50;

        if (chestLevel == 1) // Mini-Boss Chest
        {
            float roll = Random.value;
            if (roll < 0.70f)
            {
                itemCount = 1;
                titleText.text = "★ SILVER CHEST ★";
                titleText.color = new Color(0.85f, 0.95f, 1f);
                coinsBonus = Random.Range(60, 120);
            }
            else
            {
                itemCount = 3;
                titleText.text = "★ GOLDEN CHEST! ★";
                titleText.color = new Color(1f, 0.85f, 0.2f);
                coinsBonus = Random.Range(150, 250);
            }
        }
        else // Major Boss Chest
        {
            float roll = Random.value;
            if (roll < 0.40f)
            {
                itemCount = 3;
                titleText.text = "★ EPIC BOSS CHEST! ★";
                titleText.color = new Color(1f, 0.82f, 0.2f);
                coinsBonus = Random.Range(200, 350);
            }
            else
            {
                itemCount = 5;
                titleText.text = "★★★ JACKPOT SUPER CHEST! ★★★";
                titleText.color = new Color(1f, 0.45f, 0.95f);
                coinsBonus = Random.Range(400, 600);
            }
        }

        coinBonusText.text = $"<color=#FFDF00>+{coinsBonus} GOLD COINS</color>";

        // Cộng coin vào ví
        if (CoinController.instance != null)
        {
            CoinController.instance.AddCoins(coinsBonus);
        }

        // Lấy và áp dụng ngẫu nhiên các nâng cấp hợp lệ
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

        // Dọn sạch danh sách hiển thị cũ
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // Sinh danh sách hiển thị từng món thưởng
        Sprite slotFrame = Resources.Load<Sprite>("UI/VS_HUD_Slot") ?? Resources.Load<Sprite>("UI/UI_Card_Normal");
        float cardWidth = 100f;
        float spacing = 12f;
        int total = awardedCards.Count;
        float startX = -((total - 1) * (cardWidth + spacing)) / 2f;

        for (int i = 0; i < total; i++)
        {
            UpgradeData card = awardedCards[i];
            if (card == null) continue;

            GameObject cardGO = new GameObject("ChestCard_" + i, typeof(RectTransform), typeof(Image));
            cardGO.transform.SetParent(itemsContainer, false);

            RectTransform cRT = cardGO.GetComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.pivot = new Vector2(0.5f, 0.5f);
            cRT.anchoredPosition = new Vector2(startX + i * (cardWidth + spacing), 0f);
            cRT.sizeDelta = new Vector2(cardWidth, 160f);

            Image cImg = cardGO.GetComponent<Image>();
            if (slotFrame != null)
            {
                cImg.sprite = slotFrame;
                cImg.type = Image.Type.Sliced;
            }
            cImg.color = new Color(0.18f, 0.22f, 0.32f, 0.95f);

            // Icon món đồ
            if (card.icon != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
                iconGO.transform.SetParent(cardGO.transform, false);

                RectTransform iconRT = iconGO.GetComponent<RectTransform>();
                iconRT.anchorMin = new Vector2(0.5f, 0.65f);
                iconRT.anchorMax = new Vector2(0.5f, 0.65f);
                iconRT.pivot = new Vector2(0.5f, 0.5f);
                iconRT.anchoredPosition = Vector2.zero;
                iconRT.sizeDelta = new Vector2(50f, 50f);

                Image iconImg = iconGO.GetComponent<Image>();
                iconImg.sprite = card.icon;
                iconImg.preserveAspect = true;
            }

            // Tên món đồ
            GameObject nameGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            nameGO.transform.SetParent(cardGO.transform, false);

            RectTransform nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0f);
            nameRT.anchorMax = new Vector2(1f, 0.35f);
            nameRT.offsetMin = new Vector2(4f, 4f);
            nameRT.offsetMax = new Vector2(-4f, 0f);

            TextMeshProUGUI nameTMP = nameGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) nameTMP.font = retroFont;
            nameTMP.fontSize = 15;
            nameTMP.alignment = TextAlignmentOptions.Center;
            nameTMP.color = Color.white;
            nameTMP.text = $"<b>{card.upgradeName}</b>\n<color=#00FFA3>+1 LEVEL</color>";
        }

        if (total == 0)
        {
            // Nếu đã max toàn bộ vũ khí/thẻ
            GameObject emptyGO = new GameObject("MaxedText", typeof(RectTransform), typeof(TextMeshProUGUI));
            emptyGO.transform.SetParent(itemsContainer, false);
            RectTransform eRT = emptyGO.GetComponent<RectTransform>();
            eRT.anchorMin = Vector2.zero;
            eRT.anchorMax = Vector2.one;
            eRT.offsetMin = Vector2.zero;
            eRT.offsetMax = Vector2.zero;

            TextMeshProUGUI eTMP = emptyGO.GetComponent<TextMeshProUGUI>();
            if (retroFont != null) eTMP.font = retroFont;
            eTMP.fontSize = 24;
            eTMP.alignment = TextAlignmentOptions.Center;
            eTMP.color = new Color(0.9f, 0.9f, 0.9f);
            eTMP.text = "<color=#00FFA3>ALL UPGRADES MAXED OUT!</color>\n<color=#FFDF00>+300 EXTRA BONUS COINS AWARDED!</color>";

            if (CoinController.instance != null) CoinController.instance.AddCoins(300);
        }

        chestModalRoot.SetActive(true);
        chestModalRoot.transform.SetAsLastSibling();

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void CloseChest()
    {
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
