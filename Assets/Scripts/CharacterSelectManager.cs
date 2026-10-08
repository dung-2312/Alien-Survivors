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

    [Header("Alien Survivors Style Sprites")]
    [SerializeField] private Sprite windowFrameSprite;
    [SerializeField] private Sprite cardNormalSprite;
    [SerializeField] private Sprite cardSelectedSprite;
    [SerializeField] private Sprite confirmButtonSprite;
    [SerializeField] private Sprite backButtonSprite;
    [SerializeField] private Sprite subPanelSprite;
    [SerializeField] private Sprite coinBoxSprite;
    [SerializeField] private Sprite coinIconSprite;
    [SerializeField] private TMP_FontAsset fontAsset;

    private int currentSelectedIndex = 0;
    private readonly List<GameObject> slotObjects = new List<GameObject>();
    private readonly List<GameObject> hiddenMainMenuObjects = new List<GameObject>();
    private TMP_Text coinText;
    private GameObject coinBoxObj;

    private void Awake()
    {
        LoadStyleResources();
        ResolveReferences();

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        // Khởi tạo trước cấu trúc và giao diện ngay từ Awake để khi người dùng ấn Start lần đầu tiên
        // mọi thành phần đã được xếp đặt hoàn hảo và hiển thị tức thì.
        ApplyThemeStyles();
        SetupCharacterSlots();
    }

    private void OnEnable()
    {
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

        // Buộc Unity Canvas và LayoutRebuilder tính toán lại vị trí ngay lập tức trong frame 0
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

    private void LoadStyleResources()
    {
        if (windowFrameSprite == null) windowFrameSprite = Resources.Load<Sprite>("UI/UI_WindowFrame_Gold") ?? CreateFallbackBorder(48, 48, new Color(0.17f, 0.20f, 0.29f), new Color(0.55f, 0.40f, 0.11f), new Color(0.96f, 0.81f, 0.29f), 3, new Vector4(12, 12, 12, 12));
        if (cardNormalSprite == null) cardNormalSprite = Resources.Load<Sprite>("UI/UI_Card_Normal") ?? CreateFallbackBorder(36, 36, new Color(0.22f, 0.25f, 0.36f), new Color(0.09f, 0.11f, 0.16f), new Color(0.47f, 0.37f, 0.16f), 2, new Vector4(8, 8, 8, 8));
        if (cardSelectedSprite == null) cardSelectedSprite = Resources.Load<Sprite>("UI/UI_Card_Selected") ?? CreateFallbackBorder(36, 36, new Color(0.22f, 0.25f, 0.36f), new Color(1.0f, 0.88f, 0.16f), new Color(1.0f, 0.97f, 0.55f), 3, new Vector4(10, 10, 10, 10));
        if (confirmButtonSprite == null) confirmButtonSprite = Resources.Load<Sprite>("UI/UI_Button_Confirm") ?? CreateFallbackBorder(36, 24, new Color(0.18f, 0.55f, 0.24f), new Color(0.11f, 0.33f, 0.14f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (backButtonSprite == null) backButtonSprite = Resources.Load<Sprite>("UI/UI_Button_Back") ?? CreateFallbackBorder(36, 24, new Color(0.66f, 0.12f, 0.12f), new Color(0.37f, 0.05f, 0.05f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (subPanelSprite == null) subPanelSprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold") ?? CreateFallbackBorder(36, 36, new Color(0.15f, 0.17f, 0.25f), new Color(0.07f, 0.08f, 0.11f), new Color(0.72f, 0.58f, 0.24f), 2, new Vector4(8, 8, 8, 8));
        if (coinBoxSprite == null) coinBoxSprite = Resources.Load<Sprite>("UI/UI_Coin_Box") ?? CreateFallbackBorder(36, 24, new Color(0.08f, 0.09f, 0.12f), new Color(0.04f, 0.04f, 0.06f), new Color(0.96f, 0.81f, 0.29f), 2, new Vector4(7, 7, 7, 7));
        if (coinIconSprite == null) coinIconSprite = Resources.Load<Sprite>("UI/UI_Coin_Icon");
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
            if (weaponPreview == null)
            {
                Transform t = bottomDetailPanel.Find("WeaponPreview");
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
        // 1. Nền tối nhẹ để tôn lên khung WindowBox vàng gothic, giữ logo MM và bầu không khí game
        Image bgImg = GetComponent<Image>();
        if (bgImg != null)
        {
            bgImg.color = new Color(0.06f, 0.06f, 0.10f, 0.88f);
        }

        // 2. Khung viền WindowBox chính kiểu Vampire Survivors
        if (windowBox != null)
        {
            windowBox.anchorMin = new Vector2(0.5f, 0.5f);
            windowBox.anchorMax = new Vector2(0.5f, 0.5f);
            windowBox.pivot = new Vector2(0.5f, 0.5f);
            windowBox.sizeDelta = new Vector2(850f, 690f);
            windowBox.anchoredPosition = new Vector2(0f, -20f);

            Image wImg = windowBox.GetComponent<Image>();
            if (wImg != null)
            {
                wImg.sprite = windowFrameSprite;
                wImg.type = Image.Type.Sliced;
                wImg.color = Color.white;
            }
        }

        // 3. Tiêu đề Character Selection
        if (titleText != null)
        {
            RectTransform tr = titleText.rectTransform;
            tr.anchorMin = new Vector2(0.5f, 1f);
            tr.anchorMax = new Vector2(0.5f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -20f);
            tr.sizeDelta = new Vector2(600f, 45f);

            titleText.text = "Character Selection";
            titleText.fontSize = 38f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            ApplyFont(titleText);
        }

        // 4. Thanh Top Bar (Nút Back & Ô Coin)
        SetupTopBar();

        // 5. Khung Bottom Detail Panel
        if (bottomDetailPanel != null)
        {
            bottomDetailPanel.anchorMin = new Vector2(0.5f, 0f);
            bottomDetailPanel.anchorMax = new Vector2(0.5f, 0f);
            bottomDetailPanel.pivot = new Vector2(0.5f, 0f);
            bottomDetailPanel.sizeDelta = new Vector2(790f, 185f);
            bottomDetailPanel.anchoredPosition = new Vector2(0f, 25f);

            Image pImg = bottomDetailPanel.GetComponent<Image>();
            if (pImg != null)
            {
                pImg.sprite = subPanelSprite;
                pImg.type = Image.Type.Sliced;
                pImg.color = Color.white;
            }

            // Tên nhân vật (trên cùng bên trái ô dưới)
            if (nameText != null)
            {
                RectTransform nTr = nameText.rectTransform;
                nTr.anchorMin = new Vector2(0f, 1f);
                nTr.anchorMax = new Vector2(1f, 1f);
                nTr.pivot = new Vector2(0f, 1f);
                nTr.anchoredPosition = new Vector2(25f, -12f);
                nTr.sizeDelta = new Vector2(-50f, 32f);

                nameText.fontSize = 26f;
                nameText.fontStyle = FontStyles.Bold;
                nameText.color = new Color(1f, 0.88f, 0.40f); // Vàng hoàng gia
                nameText.alignment = TextAlignmentOptions.Left;
                ApplyFont(nameText);
            }

            // Ảnh avatar xem trước (kích thước lớn, sắc nét, rõ ràng)
            if (characterPreview != null)
            {
                RectTransform avTr = characterPreview.rectTransform;
                avTr.anchorMin = new Vector2(0f, 0.5f);
                avTr.anchorMax = new Vector2(0f, 0.5f);
                avTr.pivot = new Vector2(0.5f, 0.5f);
                avTr.anchoredPosition = new Vector2(75f, -16f);
                avTr.sizeDelta = new Vector2(96f, 96f);
                characterPreview.preserveAspect = true;
            }

            // Khung và Ảnh vũ khí xem trước
            if (weaponPreview != null)
            {
                RectTransform wpTr = weaponPreview.rectTransform;
                wpTr.anchorMin = new Vector2(0f, 0.5f);
                wpTr.anchorMax = new Vector2(0f, 0.5f);
                wpTr.pivot = new Vector2(0.5f, 0.5f);
                wpTr.anchoredPosition = new Vector2(155f, -16f);
                wpTr.sizeDelta = new Vector2(46f, 46f);
                weaponPreview.preserveAspect = true;
            }

            // Mô tả nội tại nhân vật
            if (descriptionText != null)
            {
                RectTransform dTr = descriptionText.rectTransform;
                dTr.anchorMin = new Vector2(0f, 0.5f);
                dTr.anchorMax = new Vector2(1f, 0.5f);
                dTr.pivot = new Vector2(0f, 0.5f);
                dTr.anchoredPosition = new Vector2(215f, -16f);
                dTr.sizeDelta = new Vector2(-375f, 105f);

                descriptionText.fontSize = 19f;
                descriptionText.color = Color.white;
                descriptionText.textWrappingMode = TextWrappingModes.Normal;
                descriptionText.alignment = TextAlignmentOptions.MidlineLeft;
                ApplyFont(descriptionText);
            }

            // Nút Confirm (màu xanh lá viền vàng phong cách Vampire Survivors)
            if (confirmButton != null)
            {
                RectTransform cTr = confirmButton.GetComponent<RectTransform>();
                cTr.anchorMin = new Vector2(1f, 0.5f);
                cTr.anchorMax = new Vector2(1f, 0.5f);
                cTr.pivot = new Vector2(1f, 0.5f);
                cTr.anchoredPosition = new Vector2(-25f, -16f);
                cTr.sizeDelta = new Vector2(135f, 52f);

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
        // 1. Nút BACK màu đỏ viền vàng ở góc trên bên phải
        if (backButton != null)
        {
            RectTransform bTr = backButton.GetComponent<RectTransform>();
            bTr.anchorMin = new Vector2(0.5f, 0.5f);
            bTr.anchorMax = new Vector2(0.5f, 0.5f);
            bTr.pivot = new Vector2(0.5f, 0.5f);
            bTr.anchoredPosition = new Vector2(360f, 375f);
            bTr.sizeDelta = new Vector2(115f, 44f);

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
                bTxt.fontSize = 22f;
                bTxt.fontStyle = FontStyles.Bold;
                bTxt.color = Color.white;
                ApplyFont(bTxt);
            }
        }

        // 2. Ô hiển thị số Coin giữa màn hình
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
        cbTr.anchoredPosition = new Vector2(0f, 375f);
        cbTr.sizeDelta = new Vector2(150f, 44f);

        Image cbImg = coinBoxObj.GetComponent<Image>();
        cbImg.sprite = coinBoxSprite;
        cbImg.type = Image.Type.Sliced;
        cbImg.color = Color.white;

        // Icon Coin
        Transform iconT = coinBoxObj.transform.Find("CoinIcon");
        GameObject iconObj = iconT != null ? iconT.gameObject : new GameObject("CoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(coinBoxObj.transform, false);
        RectTransform iconTr = iconObj.GetComponent<RectTransform>();
        iconTr.anchorMin = new Vector2(0f, 0.5f);
        iconTr.anchorMax = new Vector2(0f, 0.5f);
        iconTr.pivot = new Vector2(0f, 0.5f);
        iconTr.anchoredPosition = new Vector2(10f, 0f);
        iconTr.sizeDelta = new Vector2(28f, 28f);

        Image iconImg = iconObj.GetComponent<Image>();
        if (coinIconSprite != null)
        {
            iconImg.sprite = coinIconSprite;
            iconImg.color = Color.white;
            iconImg.preserveAspect = true;
        }

        // Text Coin
        Transform textT = coinBoxObj.transform.Find("CoinText");
        GameObject textObj = textT != null ? textT.gameObject : new GameObject("CoinText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(coinBoxObj.transform, false);
        RectTransform textTr = textObj.GetComponent<RectTransform>();
        textTr.anchorMin = new Vector2(0f, 0f);
        textTr.anchorMax = new Vector2(1f, 1f);
        textTr.pivot = new Vector2(1f, 0.5f);
        textTr.anchoredPosition = new Vector2(-15f, 0f);
        textTr.sizeDelta = new Vector2(-50f, 0f);

        coinText = textObj.GetComponent<TMP_Text>();
        coinText.fontSize = 24f;
        coinText.fontStyle = FontStyles.Bold;
        coinText.color = Color.white;
        coinText.alignment = TextAlignmentOptions.MidlineRight;
        ApplyFont(coinText);
    }

    private void UpdateCoinDisplay()
    {
        if (coinText != null)
        {
            int savedCoins = PlayerPrefs.GetInt("SavedCoins", 0);
            coinText.text = savedCoins.ToString();
        }
    }

    private void SetupCharacterSlots()
    {
        if (gridTransform == null || characters == null || characters.Length == 0) return;

        // Xóa sạch HorizontalLayoutGroup nếu còn tồn tại
        var hlg = gridTransform.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            DestroyImmediate(hlg);
        }

        GridLayoutGroup glg = gridTransform.GetComponent<GridLayoutGroup>();
        if (glg == null)
        {
            glg = gridTransform.gameObject.AddComponent<GridLayoutGroup>();
        }

        glg.cellSize = new Vector2(168f, 195f);
        glg.spacing = new Vector2(18f, 16f);
        glg.childAlignment = TextAnchor.UpperCenter;
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        glg.constraintCount = 4;

        gridTransform.anchorMin = new Vector2(0.5f, 1f);
        gridTransform.anchorMax = new Vector2(0.5f, 1f);
        gridTransform.pivot = new Vector2(0.5f, 1f);
        gridTransform.anchoredPosition = new Vector2(0f, -75f);
        gridTransform.sizeDelta = new Vector2(760f, 420f);

        slotObjects.Clear();

        // Định dạng và hiển thị từng thẻ tướng
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

        // Ẩn các child dư thừa nếu có
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

        // Background Image & Button
        Image cardBg = slot.GetComponent<Image>();
        if (cardBg == null) cardBg = slot.AddComponent<Image>();
        cardBg.type = Image.Type.Sliced;
        cardBg.color = Color.white;
        cardBg.sprite = (index == currentSelectedIndex) ? cardSelectedSprite : cardNormalSprite;

        Button btn = slot.GetComponent<Button>();
        if (btn == null) btn = slot.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => SelectCharacter(index));

        // 1. Nhãn tên tướng ở đỉnh thẻ (Top Label)
        Transform labelT = slot.transform.Find("NameLabel");
        GameObject labelObj = labelT != null ? labelT.gameObject : null;
        if (labelObj == null)
        {
            TMP_Text existingText = slot.GetComponentInChildren<TMP_Text>();
            if (existingText != null && existingText.transform.parent == slot.transform)
            {
                labelObj = existingText.gameObject;
                labelObj.name = "NameLabel";
            }
            else
            {
                labelObj = new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(slot.transform, false);
            }
        }

        RectTransform lTr = labelObj.GetComponent<RectTransform>();
        lTr.anchorMin = new Vector2(0f, 1f);
        lTr.anchorMax = new Vector2(1f, 1f);
        lTr.pivot = new Vector2(0.5f, 1f);
        lTr.anchoredPosition = new Vector2(0f, -8f);
        lTr.sizeDelta = new Vector2(-16f, 26f);

        TMP_Text lText = labelObj.GetComponent<TMP_Text>();
        string displayName = data.characterName;
        int parenIdx = displayName.IndexOf('(');
        if (parenIdx > 0) displayName = displayName.Substring(0, parenIdx).Trim();
        lText.text = displayName;
        lText.fontSize = 20f;
        lText.fontStyle = FontStyles.Bold;
        lText.alignment = TextAlignmentOptions.Center;
        lText.color = Color.white;
        ApplyFont(lText);

        // 2. Sprite Avatar nhân vật ở chính giữa thẻ (to rõ ràng, cân đối)
        Transform avatarT = slot.transform.Find("Avatar");
        GameObject avatarObj = avatarT != null ? avatarT.gameObject : null;
        if (avatarObj == null)
        {
            for (int c = 0; c < slot.transform.childCount; c++)
            {
                Transform ch = slot.transform.GetChild(c);
                if (ch != labelObj.transform && ch.GetComponent<Image>() != null)
                {
                    avatarObj = ch.gameObject;
                    avatarObj.name = "Avatar";
                    break;
                }
            }
            if (avatarObj == null)
            {
                avatarObj = new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                avatarObj.transform.SetParent(slot.transform, false);
            }
        }

        RectTransform avTr = avatarObj.GetComponent<RectTransform>();
        avTr.anchorMin = new Vector2(0.5f, 0.5f);
        avTr.anchorMax = new Vector2(0.5f, 0.5f);
        avTr.pivot = new Vector2(0.5f, 0.5f);
        avTr.anchoredPosition = new Vector2(0f, -6f);
        avTr.sizeDelta = new Vector2(92f, 92f);

        Image avImg = avatarObj.GetComponent<Image>();
        avImg.sprite = data.characterSprite;
        avImg.color = Color.white;
        avImg.preserveAspect = true;

        // 3. Khung icon vũ khí khởi đầu ở góc dưới bên phải thẻ
        Transform wpBoxT = slot.transform.Find("WeaponBox");
        GameObject wpBoxObj = wpBoxT != null ? wpBoxT.gameObject : null;
        if (wpBoxObj == null)
        {
            wpBoxObj = new GameObject("WeaponBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wpBoxObj.transform.SetParent(slot.transform, false);
        }

        RectTransform wpBoxTr = wpBoxObj.GetComponent<RectTransform>();
        wpBoxTr.anchorMin = new Vector2(1f, 0f);
        wpBoxTr.anchorMax = new Vector2(1f, 0f);
        wpBoxTr.pivot = new Vector2(1f, 0f);
        wpBoxTr.anchoredPosition = new Vector2(-10f, 10f);
        wpBoxTr.sizeDelta = new Vector2(34f, 34f);

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
        wpIconImg.color = Color.white;
        wpIconImg.preserveAspect = true;
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

        if (nameText != null)
        {
            nameText.text = data.characterName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.passiveDescription;
        }

        if (characterPreview != null)
        {
            characterPreview.sprite = data.characterSprite;
            characterPreview.color = Color.white;
            characterPreview.preserveAspect = true;
        }

        if (weaponPreview != null)
        {
            weaponPreview.sprite = data.weaponSprite;
            weaponPreview.color = Color.white;
            weaponPreview.preserveAspect = true;
        }
    }

    private void OnConfirmClicked()
    {
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentSelectedIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main");
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);

        // Khôi phục hiển thị cho menu chính
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