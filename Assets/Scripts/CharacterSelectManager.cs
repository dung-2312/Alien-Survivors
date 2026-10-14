using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Danh sách dữ liệu nhân vật")]
    [SerializeField] private CharacterData[] characters;

    [Header("UI Chi tiết bên dưới")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image characterPreview;
    [SerializeField] private Image weaponPreview;
    [SerializeField] private Button confirmButton;

    [Header("Nút quay lại")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject mainMenuElements;

    [Header("UI Components")]
    [SerializeField] private RectTransform windowBox;
    [SerializeField] private RectTransform gridTransform;
    [SerializeField] private RectTransform bottomDetailPanel;
    [SerializeField] private TMP_Text titleText;

    [Header("Vampire Survivors Style Sprites")]
    [SerializeField] private Sprite windowFrameSprite;
    [SerializeField] private Sprite cardNormalSprite;
    [SerializeField] private Sprite cardSelectedSprite;
    [SerializeField] private Sprite confirmButtonSprite;
    [SerializeField] private Sprite backButtonSprite;
    [SerializeField] private Sprite subPanelSprite;
    [SerializeField] private Sprite coinBoxSprite;
    [SerializeField] private Sprite coinIconSprite;
    [SerializeField] private Sprite coinSackSprite;
    [SerializeField] private Sprite padlockSprite;
    [SerializeField] private Sprite badgeUnlockedSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    private int currentSelectedIndex = 0;
    private readonly List<GameObject> slotObjects = new List<GameObject>();
    private readonly List<GameObject> hiddenMainMenuObjects = new List<GameObject>();
    private TMP_Text coinText;
    private GameObject coinBoxObj;

    private void Awake()
    {
        InitializeCoins();
        LoadStyleResources();
        ResolveReferences();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmOrUnlockClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        ApplyThemeStyles();
        SetupCharacterSlots();
    }

    private void OnEnable()
    {
        InitializeCoins();
        HideMainMenuButtons();
        LoadStyleResources();
        ResolveReferences();
        ApplyThemeStyles();
        SetupCharacterSlots();

        if (characters != null && characters.Length > 0)
        {
            if (currentSelectedIndex < 0 || currentSelectedIndex >= characters.Length)
            {
                currentSelectedIndex = 0;
            }
            SelectCharacter(currentSelectedIndex);
        }

        UpdateCoinDisplay();

        Canvas.ForceUpdateCanvases();
        if (gridTransform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridTransform);
        }
        if (bottomDetailPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(bottomDetailPanel);
        }
    }

    private void InitializeCoins()
    {
        // Mặc định khởi tạo 5,250 vàng theo thiết kế giao diện nếu chưa có dữ liệu lưu
        if (!PlayerPrefs.HasKey("SavedCoins"))
        {
            PlayerPrefs.SetInt("SavedCoins", 5250);
            PlayerPrefs.Save();
        }
    }

    private void LoadStyleResources()
    {
        if (windowFrameSprite == null) windowFrameSprite = Resources.Load<Sprite>("UI/UI_WindowFrame_Gold") ?? CreateFallbackBorder(48, 48, new Color(0.17f, 0.20f, 0.29f), new Color(0.55f, 0.40f, 0.11f), new Color(0.96f, 0.81f, 0.29f), 3, new Vector4(12, 12, 12, 12));
        if (cardNormalSprite == null) cardNormalSprite = Resources.Load<Sprite>("UI/UI_Card_Normal") ?? CreateFallbackBorder(36, 36, new Color(0.22f, 0.25f, 0.36f), new Color(0.09f, 0.11f, 0.16f), new Color(0.47f, 0.37f, 0.16f), 2, new Vector4(8, 8, 8, 8));
        if (cardSelectedSprite == null) cardSelectedSprite = Resources.Load<Sprite>("UI/UI_Card_Selected") ?? CreateFallbackBorder(48, 48, new Color(0.22f, 0.25f, 0.36f), new Color(1.0f, 0.88f, 0.16f), new Color(1.0f, 0.97f, 0.55f), 3, new Vector4(12, 12, 12, 12));
        if (confirmButtonSprite == null) confirmButtonSprite = Resources.Load<Sprite>("UI/UI_Button_Confirm") ?? CreateFallbackBorder(36, 24, new Color(0.18f, 0.55f, 0.24f), new Color(0.11f, 0.33f, 0.14f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (backButtonSprite == null) backButtonSprite = Resources.Load<Sprite>("UI/UI_Button_Back") ?? CreateFallbackBorder(36, 24, new Color(0.66f, 0.12f, 0.12f), new Color(0.37f, 0.05f, 0.05f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (subPanelSprite == null) subPanelSprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold") ?? CreateFallbackBorder(36, 36, new Color(0.15f, 0.17f, 0.25f), new Color(0.07f, 0.08f, 0.11f), new Color(0.72f, 0.58f, 0.24f), 2, new Vector4(8, 8, 8, 8));
        if (coinBoxSprite == null) coinBoxSprite = Resources.Load<Sprite>("UI/UI_Coin_Box") ?? CreateFallbackBorder(36, 24, new Color(0.08f, 0.09f, 0.12f), new Color(0.04f, 0.04f, 0.06f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (coinIconSprite == null) coinIconSprite = Resources.Load<Sprite>("UI/UI_Coin_Icon");
        if (coinSackSprite == null) coinSackSprite = Resources.Load<Sprite>("UI/UI_Icon_CoinSack");
        if (padlockSprite == null) padlockSprite = Resources.Load<Sprite>("UI/UI_Icon_Padlock");
        if (badgeUnlockedSprite == null) badgeUnlockedSprite = Resources.Load<Sprite>("UI/UI_Badge_Unlocked");
    }

    private void ResolveReferences()
    {
        if (windowBox == null)
        {
            Transform wb = transform.Find("WindowBox");
            if (wb != null) windowBox = wb as RectTransform;
        }

        if (gridTransform == null)
        {
            if (windowBox != null)
            {
                Transform g = windowBox.Find("CharacterGrid");
                if (g != null) gridTransform = g as RectTransform;
            }
            if (gridTransform == null)
            {
                Transform g = transform.Find("CharacterGrid");
                if (g != null) gridTransform = g as RectTransform;
            }
        }

        if (bottomDetailPanel == null)
        {
            if (windowBox != null)
            {
                Transform b = windowBox.Find("BottomDetailPanel");
                if (b != null) bottomDetailPanel = b as RectTransform;
            }
            if (bottomDetailPanel == null)
            {
                Transform b = transform.Find("BottomDetailPanel");
                if (b != null) bottomDetailPanel = b as RectTransform;
            }
        }

        if (backButton == null)
        {
            Transform b = transform.Find("BackButton") ?? (windowBox != null ? windowBox.Find("BackButton") : null);
            if (b != null) backButton = b.GetComponent<Button>();
        }

        if (bottomDetailPanel != null)
        {
            if (confirmButton == null)
            {
                Transform c = bottomDetailPanel.Find("ConfirmButton");
                if (c != null) confirmButton = c.GetComponent<Button>();
            }
            if (nameText == null)
            {
                Transform t = bottomDetailPanel.Find("NameText");
                if (t != null) nameText = t.GetComponent<TMP_Text>();
            }
            if (descriptionText == null)
            {
                Transform t = bottomDetailPanel.Find("DescText");
                if (t != null) descriptionText = t.GetComponent<TMP_Text>();
            }
            if (characterPreview == null)
            {
                Transform t = bottomDetailPanel.Find("AvatarPreview");
                if (t != null) characterPreview = t.GetComponent<Image>();
            }
            Transform wpBox = bottomDetailPanel.Find("WeaponPreviewBox");
            Transform directWp = bottomDetailPanel.Find("WeaponPreview");
            if (wpBox != null && directWp != null && directWp.parent == bottomDetailPanel)
            {
                directWp.gameObject.SetActive(false);
            }

            if (weaponPreview == null)
            {
                if (wpBox != null)
                {
                    Transform t = wpBox.Find("WeaponPreview");
                    if (t != null) weaponPreview = t.GetComponent<Image>();
                }
                if (weaponPreview == null && directWp != null)
                {
                    weaponPreview = directWp.GetComponent<Image>();
                }
            }
            else if (wpBox != null && weaponPreview.transform.parent == bottomDetailPanel)
            {
                Transform t = wpBox.Find("WeaponPreview");
                if (t != null) weaponPreview = t.GetComponent<Image>();
            }
        }

        if (titleText == null && windowBox != null)
        {
            Transform t = windowBox.Find("Title");
            if (t != null) titleText = t.GetComponent<TMP_Text>();
        }

        if (fontAsset == null && nameText != null)
        {
            fontAsset = nameText.font;
        }
    }

    private void ApplyFont(TMP_Text txt)
    {
        if (txt == null) return;
        if (fontAsset != null)
        {
            txt.font = fontAsset;
            if (fontAsset.material != null)
            {
                txt.fontSharedMaterial = fontAsset.material;
            }
        }
    }

    private void HideMainMenuButtons()
    {
        hiddenMainMenuObjects.Clear();
        if (mainMenuElements != null)
        {
            mainMenuElements.SetActive(false);
            hiddenMainMenuObjects.Add(mainMenuElements);
        }

        Transform canvas = transform.parent;
        if (canvas != null)
        {
            for (int i = 0; i < canvas.childCount; i++)
            {
                Transform child = canvas.GetChild(i);
                if (child == transform) continue;
                if (child.name.Contains("Start") || child.name.Contains("Quit") || child.name.Contains("Button"))
                {
                    if (child.gameObject.activeSelf)
                    {
                        child.gameObject.SetActive(false);
                        hiddenMainMenuObjects.Add(child.gameObject);
                    }
                }
            }
        }
    }

    private void ApplyThemeStyles()
    {
        // 1. Lớp nền tối mờ huyền bí
        Image bgImg = GetComponent<Image>();
        if (bgImg != null)
        {
            bgImg.color = new Color(0.04f, 0.04f, 0.07f, 0.92f);
        }

        // 2. Khung viền WindowBox tỷ lệ landscape (880x960) phong cách Vampire Survivors
        if (windowBox != null)
        {
            windowBox.anchorMin = new Vector2(0.5f, 0.5f);
            windowBox.anchorMax = new Vector2(0.5f, 0.5f);
            windowBox.pivot = new Vector2(0.5f, 0.5f);
            windowBox.sizeDelta = new Vector2(880f, 960f);
            windowBox.anchoredPosition = new Vector2(0f, -40f);

            Image wImg = windowBox.GetComponent<Image>();
            if (wImg != null)
            {
                wImg.sprite = windowFrameSprite;
                wImg.type = Image.Type.Sliced;
                wImg.color = Color.white;
            }
        }

        // 3. Tiêu đề "Character Selection" font pixel trắng
        if (titleText != null)
        {
            RectTransform tr = titleText.rectTransform;
            tr.anchorMin = new Vector2(0.5f, 1f);
            tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -18f);
            tr.sizeDelta = new Vector2(800f, 48f);

            titleText.text = "Character Selection";
            titleText.fontSize = 36f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            ApplyFont(titleText);
        }

        // 4. Thanh Top Bar: Ô tiền tệ có túi tiền màu xanh (bên trái) và nút BACK màu đỏ viền vàng (bên phải)
        SetupTopBar();

        // 5. Khung thông tin nhân vật chi tiết phía dưới (Bottom Detail Panel)
        if (bottomDetailPanel != null)
        {
            bottomDetailPanel.anchorMin = new Vector2(0.5f, 0f);
            bottomDetailPanel.anchorMax = new Vector2(0.5f, 0f);
            bottomDetailPanel.pivot = new Vector2(0.5f, 0f);
            bottomDetailPanel.sizeDelta = new Vector2(830f, 220f);
            bottomDetailPanel.anchoredPosition = new Vector2(0f, 18f);

            Image pImg = bottomDetailPanel.GetComponent<Image>();
            if (pImg != null)
            {
                pImg.sprite = subPanelSprite;
                pImg.type = Image.Type.Sliced;
                pImg.color = Color.white;
            }

            // Tên tướng ở góc trên bên trái
            if (nameText != null)
            {
                RectTransform nTr = nameText.rectTransform;
                nTr.anchorMin = new Vector2(0f, 1f);
                nTr.anchorMax = new Vector2(1f, 1f);
                nTr.pivot = new Vector2(0f, 1f);
                nTr.anchoredPosition = new Vector2(24f, -12f);
                nTr.sizeDelta = new Vector2(500f, 44f);

                nameText.fontSize = 32f;
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = new Color(1f, 0.95f, 0.85f);
                nameText.alignment = TextAlignmentOptions.Left;
                ApplyFont(nameText);
            }

            // Avatar tướng xem trước
            if (characterPreview != null)
            {
                RectTransform avTr = characterPreview.rectTransform;
                avTr.anchorMin = new Vector2(0f, 0.5f);
                avTr.anchorMax = new Vector2(0f, 0.5f);
                avTr.pivot = new Vector2(0.5f, 0.5f);
                avTr.anchoredPosition = new Vector2(65f, -14f);
                avTr.sizeDelta = new Vector2(90f, 90f);
                characterPreview.preserveAspect = true;
            }

            // Ô khung vũ khí xem trước
            Transform wpBoxTr = bottomDetailPanel.Find("WeaponPreviewBox");
            if (wpBoxTr != null)
            {
                RectTransform boxRt = wpBoxTr.GetComponent<RectTransform>();
                if (boxRt != null)
                {
                    boxRt.anchorMin = new Vector2(0f, 0.5f);
                    boxRt.anchorMax = new Vector2(0f, 0.5f);
                    boxRt.pivot = new Vector2(0.5f, 0.5f);
                    boxRt.anchoredPosition = new Vector2(145f, -14f);
                    boxRt.sizeDelta = new Vector2(46f, 46f);
                }
                Image boxImg = wpBoxTr.GetComponent<Image>();
                if (boxImg != null)
                {
                    boxImg.sprite = subPanelSprite;
                    boxImg.type = Image.Type.Sliced;
                    boxImg.color = Color.white;
                }
            }

            // Ô vũ khí khởi đầu xem trước
            if (weaponPreview != null)
            {
                RectTransform wpTr = weaponPreview.rectTransform;
                if (weaponPreview.transform.parent != null && weaponPreview.transform.parent == bottomDetailPanel)
                {
                    wpTr.anchorMin = new Vector2(0f, 0.5f);
                    wpTr.anchorMax = new Vector2(0f, 0.5f);
                    wpTr.pivot = new Vector2(0.5f, 0.5f);
                    wpTr.anchoredPosition = new Vector2(145f, -14f);
                    wpTr.sizeDelta = new Vector2(46f, 46f);
                }
                else
                {
                    wpTr.anchorMin = Vector2.zero;
                    wpTr.anchorMax = Vector2.one;
                    wpTr.pivot = new Vector2(0.5f, 0.5f);
                    wpTr.anchoredPosition = Vector2.zero;
                    wpTr.sizeDelta = new Vector2(-8f, -8f);
                }
                weaponPreview.preserveAspect = true;
            }

            // Nội dung mô tả / lore / yêu cầu mở khóa
            if (descriptionText != null)
            {
                RectTransform dTr = descriptionText.rectTransform;
                dTr.anchorMin = new Vector2(0f, 0.5f);
                dTr.anchorMax = new Vector2(1f, 0.5f);
                dTr.pivot = new Vector2(0f, 0.5f);
                dTr.anchoredPosition = new Vector2(205f, -14f);
                dTr.sizeDelta = new Vector2(-385f, 130f);

                descriptionText.fontSize = 24f;
                descriptionText.color = Color.white;
                descriptionText.textWrappingMode = TextWrappingModes.Normal;
                descriptionText.alignment = TextAlignmentOptions.MidlineLeft;
                ApplyFont(descriptionText);
            }

            // Nút Confirm / Unlock màu xanh lá viền vàng
            if (confirmButton != null)
            {
                RectTransform cTr = confirmButton.GetComponent<RectTransform>();
                cTr.anchorMin = new Vector2(1f, 0.5f);
                cTr.anchorMax = new Vector2(1f, 0.5f);
                cTr.pivot = new Vector2(1f, 0.5f);
                cTr.anchoredPosition = new Vector2(-25f, -14f);
                cTr.sizeDelta = new Vector2(145f, 56f);

                Image cImg = confirmButton.GetComponent<Image>();
                if (cImg != null)
                {
                    cImg.sprite = confirmButtonSprite;
                    cImg.type = Image.Type.Sliced;
                    cImg.color = Color.white;
                }

                TMP_Text cTxt = confirmButton.GetComponentInChildren<TMP_Text>();
                if (cTxt != null)
                {
                    cTxt.text = "Confirm";
                    cTxt.fontSize = 24f;
                    cTxt.fontStyle = FontStyles.Bold;
                    cTxt.color = Color.white;
                    ApplyFont(cTxt);
                }
            }
        }
    }

    private void SetupTopBar()
    {
        // 1. Ô hiển thị số Coin với túi tiền xanh ở phía trên bên trái
        if (coinBoxObj == null)
        {
            Transform existingBox = transform.Find("TopCoinBox");
            if (existingBox != null)
            {
                coinBoxObj = existingBox.gameObject;
            }
            else
            {
                coinBoxObj = new GameObject("TopCoinBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                coinBoxObj.transform.SetParent(transform, false);
            }
        }

        RectTransform cbTr = coinBoxObj.GetComponent<RectTransform>();
        cbTr.anchorMin = new Vector2(0.5f, 0.5f);
        cbTr.anchorMax = new Vector2(0.5f, 0.5f);
        cbTr.pivot = new Vector2(0.5f, 0.5f);
        cbTr.anchoredPosition = new Vector2(-40f, 478f);
        cbTr.sizeDelta = new Vector2(240f, 58f);

        Image cbImg = coinBoxObj.GetComponent<Image>();
        cbImg.sprite = coinBoxSprite;
        cbImg.type = Image.Type.Sliced;
        cbImg.color = Color.white;

        // Icon Túi tiền xanh (Green Coin Sack)
        Transform sackT = coinBoxObj.transform.Find("CoinSackIcon");
        GameObject sackObj = sackT != null ? sackT.gameObject : new GameObject("CoinSackIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        sackObj.transform.SetParent(coinBoxObj.transform, false);
        RectTransform sackTr = sackObj.GetComponent<RectTransform>();
        sackTr.anchorMin = new Vector2(0f, 0.5f);
        sackTr.anchorMax = new Vector2(0f, 0.5f);
        sackTr.pivot = new Vector2(0f, 0.5f);
        sackTr.anchoredPosition = new Vector2(10f, 0f);
        sackTr.sizeDelta = new Vector2(42f, 42f);

        Image sackImg = sackObj.GetComponent<Image>();
        if (coinSackSprite != null)
        {
            sackImg.sprite = coinSackSprite;
        }
        else if (coinIconSprite != null)
        {
            sackImg.sprite = coinIconSprite;
        }
        sackImg.color = Color.white;
        sackImg.preserveAspect = true;

        // Số tiền
        Transform textT = coinBoxObj.transform.Find("CoinText");
        GameObject textObj = textT != null ? textT.gameObject : new GameObject("CoinText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(coinBoxObj.transform, false);
        RectTransform textTr = textObj.GetComponent<RectTransform>();
        textTr.anchorMin = new Vector2(0f, 0f);
        textTr.anchorMax = new Vector2(1f, 1f);
        textTr.pivot = new Vector2(1f, 0.5f);
        textTr.anchoredPosition = new Vector2(-15f, 0f);
        textTr.sizeDelta = new Vector2(-60f, 0f);

        coinText = textObj.GetComponent<TMP_Text>();
        coinText.fontSize = 24f;
        coinText.fontStyle = FontStyles.Bold;
        coinText.color = Color.white;
        coinText.alignment = TextAlignmentOptions.MidlineRight;
        ApplyFont(coinText);

        // 2. Nút BACK màu đỏ viền vàng ở phía trên bên phải
        if (backButton != null)
        {
            RectTransform bTr = backButton.GetComponent<RectTransform>();
            bTr.anchorMin = new Vector2(0.5f, 0.5f);
            bTr.anchorMax = new Vector2(0.5f, 0.5f);
            bTr.pivot = new Vector2(0.5f, 0.5f);
            bTr.anchoredPosition = new Vector2(260f, 478f);
            bTr.sizeDelta = new Vector2(150f, 56f);

            Image bImg = backButton.GetComponent<Image>();
            if (bImg != null)
            {
                bImg.sprite = backButtonSprite;
                bImg.type = Image.Type.Sliced;
                bImg.color = Color.white;
            }

            TMP_Text bTxt = backButton.GetComponentInChildren<TMP_Text>();
            if (bTxt != null)
            {
                bTxt.text = "BACK";
                bTxt.fontSize = 24f;
                bTxt.fontStyle = FontStyles.Bold;
                bTxt.color = Color.white;
                ApplyFont(bTxt);
            }
        }
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            int savedCoins = PlayerPrefs.GetInt("SavedCoins", 5250);
            coinText.text = savedCoins.ToString("N0");
        }
    }

    private static readonly string[] characterDisplayNames = new string[]
    {
        "Rowan",
        "Kaelen",
        "Valerius",
        "Seraphina",
        "Gorgon",
        "Morvath"
    };

    public string GetCharacterDisplayName(int index)
    {
        if (index >= 0 && index < characterDisplayNames.Length)
        {
            return characterDisplayNames[index];
        }
        if (characters != null && index >= 0 && index < characters.Length && characters[index] != null)
        {
            return characters[index].characterName;
        }
        return $"CH {index + 1:D2}";
    }

    public int GetCharacterCost(int index)
    {
        if (characters != null && index >= 0 && index < characters.Length && characters[index] != null)
        {
            if (characters[index].unlockCost > 0) return characters[index].unlockCost;
        }

        // Bảng giá chuẩn khớp 100% với giao diện mẫu của người dùng:
        // Slot 1 (Rowan): 0 (UNLOCKED)
        // Slot 2 (Kaelen): 0 (UNLOCKED)
        // Slot 3 (Valerius): 100 Coins
        // Slot 4 (Seraphina): 250 Coins
        // Slot 5 (Gorgon): 500 Coins
        // Slot 6 (Morvath): 800 Coins
        switch (index)
        {
            case 0: return 0;
            case 1: return 0;
            case 2: return 100;
            case 3: return 250;
            case 4: return 500;
            case 5: return 800;
            default: return (index + 1) * 300;
        }
    }

    public bool IsCharacterUnlocked(int index)
    {
        int cost = GetCharacterCost(index);
        if (cost <= 0) return true; // Tướng miễn phí mặc định đã mở
        return PlayerPrefs.GetInt($"Character_Unlocked_{index}", 0) == 1;
    }

    private void SetupCharacterSlots()
    {
        if (gridTransform == null || characters == null || characters.Length == 0) return;

        // Cấu hình lưới 2x3 (3 cột x 2 dòng) cho 6 tướng
        var hlg = gridTransform.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) DestroyImmediate(hlg);

        GridLayoutGroup glg = gridTransform.GetComponent<GridLayoutGroup>();
        if (glg == null) glg = gridTransform.gameObject.AddComponent<GridLayoutGroup>();

        // Kích thước mỗi thẻ 250 x 280, khoảng cách 18 x 16
        glg.cellSize = new Vector2(250f, 280f);
        glg.spacing = new Vector2(18f, 16f);
        glg.childAlignment = TextAnchor.UpperCenter;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 3; // Đúng 3 cột x 2 hàng

        gridTransform.anchorMin = new Vector2(0.5f, 1f);
        gridTransform.anchorMax = new Vector2(0.5f, 1f);
        gridTransform.pivot = new Vector2(0.5f, 1f);
        gridTransform.anchoredPosition = new Vector2(0f, -70f);
        gridTransform.sizeDelta = new Vector2(820f, 600f);

        slotObjects.Clear();

        for (int i = 0; i < characters.Length; i++)
        {
            int index = i;
            GameObject slot;

            if (i < gridTransform.childCount)
            {
                slot = gridTransform.GetChild(i).gameObject;
            }
            else
            {
                slot = Instantiate(gridTransform.GetChild(0).gameObject, gridTransform);
            }

            slot.name = $"Slot_{index}";
            slot.SetActive(true);
            slotObjects.Add(slot);

            FormatCardVisual(slot, index);
        }

        // Ẩn bớt nếu số child vượt quá danh sách tướng
        for (int i = characters.Length; i < gridTransform.childCount; i++)
        {
            gridTransform.GetChild(i).gameObject.SetActive(false);
        }

        UpdateCardSelectionHighlight();
    }

    private void FormatCardVisual(GameObject slot, int index)
    {
        CharacterData data = characters[index];
        if (data == null) return;

        bool isUnlocked = IsCharacterUnlocked(index);
        int cost = GetCharacterCost(index);

        // 1. Ảnh viền thẻ và Button Click
        Image cardBg = slot.GetComponent<Image>();
        if (cardBg == null) cardBg = slot.AddComponent<Image>();
        cardBg.type = Image.Type.Sliced;
        cardBg.color = Color.white;
        cardBg.sprite = (index == currentSelectedIndex) ? cardSelectedSprite : cardNormalSprite;

        Button btn = slot.GetComponent<Button>();
        if (btn == null) btn = slot.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SelectCharacter(index));

        // 2. Nhãn tên ở góc trên bên trái: Rowan, Kaelen, Valerius...
        Transform labelT = slot.transform.Find("NameLabel");
        GameObject labelObj = labelT != null ? labelT.gameObject : null;
        if (labelObj == null)
        {
            labelObj = new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(slot.transform, false);
        }

        RectTransform lTr = labelObj.GetComponent<RectTransform>();
        lTr.anchorMin = new Vector2(0f, 1f);
        lTr.anchorMax = new Vector2(0f, 1f);
        lTr.pivot = new Vector2(0f, 1f);
        lTr.anchoredPosition = new Vector2(12f, -8f);
        lTr.sizeDelta = new Vector2(185f, 38f);

        TMP_Text lText = labelObj.GetComponent<TMP_Text>();
        lText.text = GetCharacterDisplayName(index);
        lText.fontSize = 30f;
        lText.fontStyle = FontStyles.Bold;
        lText.alignment = TextAlignmentOptions.TopLeft;
        lText.textWrappingMode = TextWrappingModes.NoWrap;
        lText.color = Color.white;
        ApplyFont(lText);

        // 3. Icon Ổ khóa vàng ở góc trên bên phải nếu chưa mở khóa (Locked Padlock)
        Transform lockT = slot.transform.Find("PadlockIcon");
        GameObject lockObj = lockT != null ? lockT.gameObject : null;
        if (lockObj == null)
        {
            lockObj = new GameObject("PadlockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lockObj.transform.SetParent(slot.transform, false);
        }
        RectTransform lockTr = lockObj.GetComponent<RectTransform>();
        lockTr.anchorMin = new Vector2(1f, 1f);
        lockTr.anchorMax = new Vector2(1f, 1f);
        lockTr.pivot = new Vector2(1f, 1f);
        lockTr.anchoredPosition = new Vector2(-10f, -8f);
        lockTr.sizeDelta = new Vector2(26f, 26f);

        Image lockImg = lockObj.GetComponent<Image>();
        if (padlockSprite != null) lockImg.sprite = padlockSprite;
        lockImg.color = Color.white;
        lockImg.preserveAspect = true;
        lockObj.SetActive(!isUnlocked);

        // 4. Sprite Avatar nhân vật ở chính giữa thẻ
        Transform avatarT = slot.transform.Find("Avatar");
        GameObject avatarObj = avatarT != null ? avatarT.gameObject : null;
        if (avatarObj == null)
        {
            avatarObj = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            avatarObj.transform.SetParent(slot.transform, false);
        }

        RectTransform avTr = avatarObj.GetComponent<RectTransform>();
        avTr.anchorMin = new Vector2(0.5f, 0.5f);
        avTr.anchorMax = new Vector2(0.5f, 0.5f);
        avTr.pivot = new Vector2(0.5f, 0.5f);
        avTr.anchoredPosition = new Vector2(-22f, 8f);
        avTr.sizeDelta = new Vector2(96f, 96f);

        Image avImg = avatarObj.GetComponent<Image>();
        avImg.sprite = data.characterSprite;
        // Nếu đã mở khóa: hiển thị màu gốc rực rỡ; nếu chưa mở khóa: bóng đen bí ẩn (silhouette)
        avImg.color = isUnlocked ? Color.white : new Color(0.10f, 0.12f, 0.16f, 1f);
        avImg.preserveAspect = true;

        // 5. Khung ô vũ khí khởi đầu bên cạnh nhân vật
        Transform wpBoxT = slot.transform.Find("WeaponBox");
        GameObject wpBoxObj = wpBoxT != null ? wpBoxT.gameObject : null;
        if (wpBoxObj == null)
        {
            wpBoxObj = new GameObject("WeaponBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wpBoxObj.transform.SetParent(slot.transform, false);
        }

        RectTransform wpBoxTr = wpBoxObj.GetComponent<RectTransform>();
        wpBoxTr.anchorMin = new Vector2(0.5f, 0.5f);
        wpBoxTr.anchorMax = new Vector2(0.5f, 0.5f);
        wpBoxTr.pivot = new Vector2(0f, 0.5f);
        wpBoxTr.anchoredPosition = new Vector2(42f, 8f);
        wpBoxTr.sizeDelta = new Vector2(42f, 42f);

        Image wpBoxImg = wpBoxObj.GetComponent<Image>();
        wpBoxImg.sprite = subPanelSprite;
        wpBoxImg.type = Image.Type.Sliced;
        wpBoxImg.color = Color.white;

        Transform wpIconT = wpBoxObj.transform.Find("WeaponIcon");
        GameObject wpIconObj = wpIconT != null ? wpIconT.gameObject : null;
        if (wpIconObj == null)
        {
            wpIconObj = new GameObject("WeaponIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wpIconObj.transform.SetParent(wpBoxObj.transform, false);
        }

        RectTransform wpIconTr = wpIconObj.GetComponent<RectTransform>();
        wpIconTr.anchorMin = new Vector2(0f, 0f);
        wpIconTr.anchorMax = new Vector2(1f, 1f);
        wpIconTr.sizeDelta = new Vector2(-6f, -6f);
        wpIconTr.anchoredPosition = Vector2.zero;

        Image wpIconImg = wpIconObj.GetComponent<Image>();
        wpIconImg.sprite = data.weaponSprite;
        wpIconImg.color = isUnlocked ? Color.white : new Color(0.10f, 0.12f, 0.16f, 0.9f);
        wpIconImg.preserveAspect = true;

        // 6. Thanh trạng thái ở đáy thẻ: Nhãn "UNLOCKED" xanh lá HOẶC Giá tiền "X,XXX" kèm đồng xu vàng
        Transform statusT = slot.transform.Find("BottomStatus");
        GameObject statusObj = statusT != null ? statusT.gameObject : null;
        if (statusObj == null)
        {
            statusObj = new GameObject("BottomStatus", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            statusObj.transform.SetParent(slot.transform, false);
        }

        RectTransform stTr = statusObj.GetComponent<RectTransform>();
        stTr.anchorMin = new Vector2(0.5f, 0f);
        stTr.anchorMax = new Vector2(0.5f, 0f);
        stTr.pivot = new Vector2(0.5f, 0f);
        stTr.anchoredPosition = new Vector2(0f, 12f);
        stTr.sizeDelta = new Vector2(180f, 32f);

        Image stImg = statusObj.GetComponent<Image>();

        // Text trạng thái
        Transform stTextT = statusObj.transform.Find("StatusText");
        GameObject stTextObj = stTextT != null ? stTextT.gameObject : new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        stTextObj.transform.SetParent(statusObj.transform, false);
        RectTransform stTextTr = stTextObj.GetComponent<RectTransform>();
        stTextTr.anchorMin = new Vector2(0f, 0f);
        stTextTr.anchorMax = new Vector2(1f, 1f);
        stTextTr.pivot = new Vector2(0.5f, 0.5f);
        stTextTr.anchoredPosition = Vector2.zero;
        stTextTr.sizeDelta = Vector2.zero;

        TMP_Text stText = stTextObj.GetComponent<TMP_Text>();
        stText.fontStyle = FontStyles.Bold;
        stText.alignment = TextAlignmentOptions.Center;
        ApplyFont(stText);

        // Icon đồng xu trong thẻ
        Transform stCoinT = statusObj.transform.Find("StatusCoinIcon");
        GameObject stCoinObj = stCoinT != null ? stCoinT.gameObject : new GameObject("StatusCoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        stCoinObj.transform.SetParent(statusObj.transform, false);
        RectTransform stCoinTr = stCoinObj.GetComponent<RectTransform>();
        stCoinTr.anchorMin = new Vector2(1f, 0.5f);
        stCoinTr.anchorMax = new Vector2(1f, 0.5f);
        stCoinTr.pivot = new Vector2(1f, 0.5f);
        stCoinTr.anchoredPosition = new Vector2(-10f, 0f);
        stCoinTr.sizeDelta = new Vector2(20f, 20f);

        Image stCoinImg = stCoinObj.GetComponent<Image>();
        if (coinIconSprite != null) stCoinImg.sprite = coinIconSprite;
        stCoinImg.color = Color.white;
        stCoinImg.preserveAspect = true;

        if (isUnlocked)
        {
            // Trạng thái ĐÃ MỞ KHÓA: Badge xanh lá "UNLOCKED"
            if (badgeUnlockedSprite != null)
            {
                stImg.sprite = badgeUnlockedSprite;
                stImg.type = Image.Type.Sliced;
                stImg.color = Color.white;
            }
            else
            {
                stImg.color = new Color(0.12f, 0.55f, 0.22f, 1f);
            }

            stText.text = "UNLOCKED";
            stText.fontSize = 17f;
            stText.color = Color.white;
            stTextTr.anchoredPosition = Vector2.zero;

            stCoinObj.SetActive(false);
        }
        else
        {
            // Trạng thái KHÓA: Hiển thị giá coin (ví dụ "1,000", "2,500", "5,000", "8,000")
            stImg.color = new Color(0f, 0f, 0f, 0.35f); // Nền tối nhẹ để tôn số tiền

            stText.text = cost.ToString("N0");
            stText.fontSize = 19f;
            stText.color = new Color(1f, 0.92f, 0.70f); // Vàng nhạt hoàng gia
            stTextTr.anchoredPosition = new Vector2(-12f, 0f);

            stCoinObj.SetActive(true);
        }
    }

    private void UpdateCardSelectionHighlight()
    {
        for (int i = 0; i < slotObjects.Count; i++)
        {
            if (slotObjects[i] == null) continue;
            Image img = slotObjects[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i == currentSelectedIndex) ? cardSelectedSprite : cardNormalSprite;
            }
        }
    }

    public void SelectCharacter(int index)
    {
        if (characters == null || index < 0 || index >= characters.Length) return;
        currentSelectedIndex = index;

        UpdateCardSelectionHighlight();

        CharacterData data = characters[currentSelectedIndex];
        if (data == null) return;

        bool isUnlocked = IsCharacterUnlocked(currentSelectedIndex);
        int cost = GetCharacterCost(currentSelectedIndex);
        int currentCoins = PlayerPrefs.GetInt("SavedCoins", 5250);

        // Cập nhật Tên tướng ở Bottom Panel
        if (nameText != null)
        {
            if (isUnlocked)
            {
                nameText.text = GetCharacterDisplayName(currentSelectedIndex);
                nameText.color = new Color(1f, 0.95f, 0.85f);
            }
            else
            {
                nameText.text = $"{GetCharacterDisplayName(currentSelectedIndex)} (Locked)";
                nameText.color = new Color(0.8f, 0.8f, 0.85f);
            }
        }

        // Cập nhật Mô tả / Hướng dẫn mở khóa
        if (descriptionText != null)
        {
            if (isUnlocked)
            {
                descriptionText.text = !string.IsNullOrEmpty(data.passiveDescription)
                    ? data.passiveDescription
                    : "Select a character to view stats and unlock or Generic stats & lore details...";
            }
            else
            {
                if (currentCoins >= cost)
                {
                    descriptionText.text = $"Unlock Cost: <color=#FFD700>{cost:N0} Coins</color>\n<color=#66FF66>You have {currentCoins:N0} Coins. Click Unlock to play with this character!</color>";
                }
                else
                {
                    descriptionText.text = $"Unlock Cost: <color=#FFD700>{cost:N0} Coins</color>\n<color=#FF7777>You need {cost - currentCoins:N0} more Coins to unlock.</color>";
                }
            }
        }

        // Cập nhật ảnh đại diện preview
        if (characterPreview != null)
        {
            characterPreview.sprite = data.characterSprite;
            characterPreview.color = isUnlocked ? Color.white : new Color(0.15f, 0.17f, 0.22f, 1f);
            characterPreview.preserveAspect = true;
        }

        // Cập nhật vũ khí preview
        if (weaponPreview != null)
        {
            weaponPreview.sprite = data.weaponSprite;
            weaponPreview.color = isUnlocked ? Color.white : new Color(0.15f, 0.17f, 0.22f, 0.9f);
            weaponPreview.preserveAspect = true;
        }

        // Cập nhật nút Confirm / Unlock
        if (confirmButton != null)
        {
            TMP_Text cTxt = confirmButton.GetComponentInChildren<TMP_Text>();
            Image cImg = confirmButton.GetComponent<Image>();

            if (isUnlocked)
            {
                if (cTxt != null) cTxt.text = "Confirm";
                confirmButton.interactable = true;
                if (cImg != null) cImg.color = Color.white;
            }
            else
            {
                if (currentCoins >= cost)
                {
                    if (cTxt != null) cTxt.text = "Unlock";
                    confirmButton.interactable = true;
                    if (cImg != null) cImg.color = Color.white;
                }
                else
                {
                    if (cTxt != null) cTxt.text = "Locked";
                    confirmButton.interactable = false;
                    if (cImg != null) cImg.color = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                }
            }
        }
    }

    private void OnConfirmOrUnlockClicked()
    {
        bool isUnlocked = IsCharacterUnlocked(currentSelectedIndex);
        if (isUnlocked)
        {
            // Vào trận đấu với tướng đã chọn
            PlayerPrefs.SetInt("SelectedCharacterIndex", currentSelectedIndex);
            PlayerPrefs.Save();
            SceneManager.LoadScene("Main");
        }
        else
        {
            // Mở khóa tướng bằng vàng
            int cost = GetCharacterCost(currentSelectedIndex);
            int currentCoins = PlayerPrefs.GetInt("SavedCoins", 5250);

            if (currentCoins >= cost)
            {
                currentCoins -= cost;
                PlayerPrefs.SetInt("SavedCoins", currentCoins);
                PlayerPrefs.SetInt($"Character_Unlocked_{currentSelectedIndex}", 1);
                PlayerPrefs.Save();

                UpdateCoinDisplay();
                SetupCharacterSlots();
                SelectCharacter(currentSelectedIndex);
            }
        }
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);

        foreach (var obj in hiddenMainMenuObjects)
        {
            if (obj != null) obj.SetActive(true);
        }
        hiddenMainMenuObjects.Clear();

        if (mainMenuElements != null)
        {
            mainMenuElements.SetActive(true);
        }
    }

    private Sprite CreateFallbackBorder(int width, int height, Color fill, Color border, Color highlight, int borderWidth, Vector4 sliceBorder)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isOuter = (x == 0 || x == width - 1 || y == 0 || y == height - 1);
                bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                bool isHighlight = (x == 1 || x == width - 2 || y == 1 || y == height - 2);

                if (isOuter)
                    cols[y * width + x] = border;
                else if (isHighlight)
                    cols[y * width + x] = highlight;
                else if (isBorder)
                    cols[y * width + x] = border;
                else
                    cols[y * width + x] = fill;
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, sliceBorder);
    }
}