using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static MainMenu instance;
    public string firstLevelName; // The first level name. GK

    private TMP_FontAsset retroPixelFont;
    private RectTransform titleRect;
    private RectTransform titleShadowRect;
    private Vector2 titleBasePos;
    private Vector2 shadowBasePos;
    private TextMeshProUGUI coinCountText;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CacheRetroFont();
        CreateGothicAtmosphereOverlay();
        CreateGothicEmbersOverlay();
        ShowMainMenuFirst();
        SetupTitle();
        SetupTopCoinCapsule();
        SetupMenuButtons();
    }

    void Update()
    {
        // Hiệu ứng nhịp thở bồng bềnh thanh thoát cho Logo Tiêu Đề Gothic (Title Breathing Float)
        if (titleRect != null)
        {
            float bob = Mathf.Sin(Time.time * 1.8f) * 6f;
            titleRect.anchoredPosition = new Vector2(titleBasePos.x, titleBasePos.y + bob);
            if (titleShadowRect != null)
            {
                titleShadowRect.anchoredPosition = new Vector2(shadowBasePos.x, shadowBasePos.y + bob);
            }
        }
    }

    private void CacheRetroFont()
    {
        Transform title = transform.Find("Title");
        if (title != null)
        {
            TMP_Text tmp = title.GetComponent<TMP_Text>();
            if (tmp != null && tmp.font != null)
            {
                retroPixelFont = tmp.font;
            }
        }
    }

    /// <summary>
    /// Tạo hiệu ứng viền tối ma mị (Eerie Gothic Vignette) bao quanh màn hình
    /// </summary>
    private void CreateGothicAtmosphereOverlay()
    {
        Transform existingVignette = transform.Find("Gothic Vignette");
        if (existingVignette != null) return;

        GameObject vignetteGO = new GameObject("Gothic Vignette", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        vignetteGO.transform.SetParent(transform, false);
        vignetteGO.transform.SetAsFirstSibling();

        RectTransform rt = vignetteGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = vignetteGO.GetComponent<Image>();
        img.color = new Color(0.04f, 0.02f, 0.06f, 0.45f);
        img.raycastTarget = false;
    }

    /// <summary>
    /// Tạo bầy tàn lửa huyết sắc bay lượn ma mị trong màn hình chờ (Blood-Red Gothic Embers)
    /// </summary>
    private void CreateGothicEmbersOverlay()
    {
        Transform existing = transform.Find("Gothic Embers Layer");
        if (existing != null) return;

        GameObject embersGO = new GameObject("Gothic Embers Layer", typeof(RectTransform));
        embersGO.transform.SetParent(transform, false);
        embersGO.transform.SetSiblingIndex(1);

        RectTransform rt = embersGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        embersGO.AddComponent<GothicMenuEmbers>();
    }

    void ShowMainMenuFirst()
    {
        Transform panel = transform.Find("CharacterSelectionPanel");
        if (panel != null) panel.gameObject.SetActive(false);

        SetMainMenuVisible(true);
    }

    // Tiêu đề: Đỏ Thẫm Huyết Dụ kết hợp Vàng Kim Hoàng Gia
    void SetupTitle()
    {
        Transform logo = transform.Find("VS_Image");
        if (logo != null) logo.gameObject.SetActive(false);

        Transform title = transform.Find("Title");
        if (title == null) return;

        Transform shadow = transform.Find("Title Shadow");
        if (shadow == null)
        {
            shadow = Instantiate(title.gameObject, title.parent).transform;
            shadow.name = "Title Shadow";
        }

        ConfigureTitle(shadow, "<color=#070104>ALIEN\nSURVIVORS</color>", new Vector2(8f, -8f));
        ConfigureTitle(title, "<color=#E8254A>ALIEN</color>\n<color=#FFD700>SURVIVORS</color>", new Vector2(0f, 0f));
        shadow.SetSiblingIndex(title.GetSiblingIndex());

        titleRect = (RectTransform)title;
        titleBasePos = titleRect.anchoredPosition;
        titleShadowRect = (RectTransform)shadow;
        shadowBasePos = titleShadowRect.anchoredPosition;

        // Phụ đề Gothic huyền bí
        Transform subtitle = transform.Find("Title Subtitle");
        if (subtitle == null)
        {
            GameObject subGO = new GameObject("Title Subtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            subGO.transform.SetParent(transform, false);
            subtitle = subGO.transform;
        }

        RectTransform srt = (RectTransform)subtitle;
        srt.anchorMin = srt.anchorMax = srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(900f, 50f);
        srt.anchoredPosition = new Vector2(0f, 75f);

        TextMeshProUGUI stmp = subtitle.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) stmp.font = retroPixelFont;
        stmp.enableAutoSizing = false;
        stmp.fontSize = 28f;
        stmp.alignment = TextAlignmentOptions.Center;
        stmp.characterSpacing = 4.5f;
        stmp.color = new Color(0.95f, 0.86f, 0.58f, 0.95f);
        stmp.text = "✦  A GOTHIC SURVIVAL NIGHTMARE  ✦";
        stmp.raycastTarget = false;
    }

    static void ConfigureTitle(Transform t, string text, Vector2 position)
    {
        t.gameObject.SetActive(true);
        RectTransform rt = (RectTransform)t;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1300f, 400f);
        rt.anchoredPosition = new Vector2(0f, 220f);

        TMP_Text tmp = t.GetComponent<TMP_Text>();
        tmp.enableAutoSizing = false;
        tmp.fontSize = 150f;
        tmp.lineSpacing = -20f;
        tmp.characterSpacing = 6f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.color = Color.white;
        tmp.text = text;

        tmp.ForceMeshUpdate();
        TMP_TextInfo info = tmp.textInfo;
        float left = float.MaxValue, right = float.MinValue;
        for (int i = 0; i < info.characterCount; i++)
        {
            if (!info.characterInfo[i].isVisible) continue;
            left = Mathf.Min(left, info.characterInfo[i].bottomLeft.x);
            right = Mathf.Max(right, info.characterInfo[i].topRight.x);
        }
        float centerX = left <= right ? (left + right) * 0.5f : 0f;
        rt.anchoredPosition = new Vector2(-centerX + position.x, 220f + position.y);
    }

    /// <summary>
    /// Thanh hiển thị Tiền Vàng ở đỉnh giữa màn hình (Ảnh 3)
    /// </summary>
    private void SetupTopCoinCapsule()
    {
        Transform capT = transform.Find("TopCoinCapsule");
        GameObject capGO = capT != null ? capT.gameObject : new GameObject("TopCoinCapsule", typeof(RectTransform), typeof(Image));
        if (capT == null) capGO.transform.SetParent(transform, false);

        RectTransform ccrt = capGO.GetComponent<RectTransform>();
        ccrt.anchorMin = new Vector2(0.5f, 1f);
        ccrt.anchorMax = new Vector2(0.5f, 1f);
        ccrt.pivot = new Vector2(0.5f, 1f);
        ccrt.anchoredPosition = new Vector2(0f, -20f);
        ccrt.sizeDelta = new Vector2(230f, 50f);

        Image ccImg = capGO.GetComponent<Image>();
        ccImg.sprite = UIController.GetGothicBanishButtonSprite();
        ccImg.type = Image.Type.Sliced;
        ccImg.color = new Color(0.12f, 0.10f, 0.15f, 0.95f);

        Transform txtT = capGO.transform.Find("CoinText");
        GameObject txtGO = txtT != null ? txtT.gameObject : new GameObject("CoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (txtT == null) txtGO.transform.SetParent(capGO.transform, false);

        RectTransform ctrt = txtGO.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.offsetMin = Vector2.zero;
        ctrt.offsetMax = Vector2.zero;

        coinCountText = txtGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) coinCountText.font = retroPixelFont;
        coinCountText.fontSize = 28f;
        coinCountText.alignment = TextAlignmentOptions.Center;
        UpdateCoinCapsule();

        // Nút OPTIONS ở góc trên bên phải
        Transform optT = transform.Find("OptionsButton");
        GameObject optGO = optT != null ? optT.gameObject : new GameObject("OptionsButton", typeof(RectTransform), typeof(Image), typeof(Button));
        if (optT == null) optGO.transform.SetParent(transform, false);

        RectTransform oprt = optGO.GetComponent<RectTransform>();
        oprt.anchorMin = new Vector2(0.5f, 1f);
        oprt.anchorMax = new Vector2(0.5f, 1f);
        oprt.pivot = new Vector2(0.5f, 1f);
        oprt.anchoredPosition = new Vector2(210f, -20f);
        oprt.sizeDelta = new Vector2(130f, 50f);

        Image opImg = optGO.GetComponent<Image>();
        opImg.sprite = UIController.GetGothicBanishButtonSprite();
        opImg.type = Image.Type.Sliced;

        Transform optTxtT = optGO.transform.Find("Text");
        GameObject optTxtGO = optTxtT != null ? optTxtT.gameObject : new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        if (optTxtT == null) optTxtGO.transform.SetParent(optGO.transform, false);
        RectTransform ort = optTxtGO.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        TextMeshProUGUI optTmp = optTxtGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) optTmp.font = retroPixelFont;
        optTmp.text = "OPTIONS";
        optTmp.fontSize = 22f;
        optTmp.alignment = TextAlignmentOptions.Center;
        optTmp.color = new Color(1f, 0.92f, 0.85f);
    }

    public void UpdateCoinCapsule()
    {
        if (coinCountText == null) return;
        int coins = PlayerPrefs.GetInt("SavedCoins", PlayerPrefs.GetInt("Coins", 0));
        if (CoinController.instance != null) coins = CoinController.instance.currentCoins;
        coinCountText.text = $"💰 <color=#FFD700><b>{coins}</b></color>";
    }

    // ====================================================================================
    // HỆ THỐNG NÚT BẤM CHUẨN VAMPIRE SURVIVORS (ẢNH 3):
    // START ở giữa -> Hàng 3 nút [COLLECTION | POWER UP | ACHIEVEMENTS] -> QUIT GAME
    // ====================================================================================
    void SetupMenuButtons()
    {
        Transform start = transform.Find("Start Game");
        Transform quit = transform.Find("Quit Game");
        if (start == null || quit == null) return;

        RunSaveData saved = RunSave.Load();
        bool canContinue = saved != null;

        // 1. Nút CONTINUE RUN (nếu có save)
        Transform cont = transform.Find("Continue Button");
        if (canContinue && cont == null)
        {
            cont = Instantiate(start.gameObject, start.parent).transform;
            cont.name = "Continue Button";
            cont.SetSiblingIndex(start.GetSiblingIndex());
            Button b = cont.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(ContinueGame);
        }
        if (cont != null) cont.gameObject.SetActive(canContinue);

        // Nút xanh lam hoàng gia
        Sprite blueButton = CreateRoyalGothicButton(
            new Color(0.08f, 0.12f, 0.28f),
            new Color(0.16f, 0.24f, 0.48f),
            new Color(0.88f, 0.72f, 0.25f),
            new Color(1.0f, 0.95f, 0.65f),
            new Color(0.35f, 0.75f, 1.0f)
        );

        // Nút xanh lục bảo
        Sprite emeraldButton = CreateRoyalGothicButton(
            new Color(0.04f, 0.18f, 0.09f),
            new Color(0.10f, 0.36f, 0.18f),
            new Color(0.88f, 0.74f, 0.25f),
            new Color(1.0f, 0.96f, 0.55f),
            new Color(0.20f, 0.98f, 0.45f)
        );

        // Nút đỏ huyết tế
        Sprite crimsonButton = CreateRoyalGothicButton(
            new Color(0.22f, 0.05f, 0.07f),
            new Color(0.44f, 0.10f, 0.14f),
            new Color(0.82f, 0.62f, 0.20f),
            new Color(0.98f, 0.84f, 0.42f),
            new Color(0.95f, 0.22f, 0.22f)
        );

        // 2. Nút START GAME (Ảnh 3: Nút lớn ở giữa)
        float startY = canContinue ? -45f : -35f;
        if (canContinue && cont != null)
        {
            StyleButton(cont, emeraldButton, "✦  CONTINUE RUN  ✦\n<size=52%>" + RunSave.Describe(saved) + "</size>", 40f, new Color(0.95f, 1f, 0.92f), new Vector2(480f, 85f), 38f);
        }

        StyleButton(start, blueButton, "✦  S T A R T  ✦", startY, new Color(1f, 0.94f, 0.58f), new Vector2(400f, 85f), 46f);
        Button sBtn = start.GetComponent<Button>();
        sBtn.onClick.RemoveAllListeners();
        sBtn.onClick.AddListener(OpenCharacterSelection);

        // 3. Hàng 3 nút: [ COLLECTION | POWER UP | ACHIEVEMENTS ] (Ảnh 3)
        float rowY = startY - 95f;

        // Collection Button
        Transform collT = transform.Find("Collection Button");
        if (collT == null)
        {
            collT = Instantiate(start.gameObject, start.parent).transform;
            collT.name = "Collection Button";
        }
        StyleButton(collT, blueButton, "COLLECTION", rowY, new Color(1f, 0.92f, 0.85f), new Vector2(210f, 65f), 24f, -220f);
        Button collBtn = collT.GetComponent<Button>();
        collBtn.onClick.RemoveAllListeners();
        collBtn.onClick.AddListener(OpenCollection);

        // Power Up Button
        Transform pwrT = transform.Find("PowerUp Button");
        if (pwrT == null)
        {
            pwrT = Instantiate(start.gameObject, start.parent).transform;
            pwrT.name = "PowerUp Button";
        }
        StyleButton(pwrT, emeraldButton, "POWER UP", rowY, new Color(0.95f, 1f, 0.92f), new Vector2(210f, 65f), 24f, 0f);
        Button pwrBtn = pwrT.GetComponent<Button>();
        pwrBtn.onClick.RemoveAllListeners();
        pwrBtn.onClick.AddListener(OpenCharacterSelection);

        // Achievements Button
        Transform achT = transform.Find("Achievements Button");
        if (achT == null)
        {
            achT = Instantiate(start.gameObject, start.parent).transform;
            achT.name = "Achievements Button";
        }
        StyleButton(achT, blueButton, "ACHIEVEMENTS", rowY, new Color(1f, 0.92f, 0.85f), new Vector2(210f, 65f), 24f, 220f);
        Button achBtn = achT.GetComponent<Button>();
        achBtn.onClick.RemoveAllListeners();
        achBtn.onClick.AddListener(OpenAchievements);

        // 4. Nút QUIT GAME
        StyleButton(quit, crimsonButton, "QUIT GAME", rowY - 85f, new Color(1f, 0.88f, 0.88f), new Vector2(260f, 52f), 24f, 0f);
        Button qBtn = quit.GetComponent<Button>();
        qBtn.onClick.RemoveAllListeners();
        qBtn.onClick.AddListener(QuitGame);
    }

    private void StyleButton(Transform t, Sprite sprite, string label, float y, Color textColor, Vector2 size, float fontSize = 40f, float x = 0f)
    {
        t.gameObject.SetActive(true);
        RectTransform rt = (RectTransform)t;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(x, y);

        Image img = t.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_Text text = t.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            if (retroPixelFont != null)
            {
                text.font = retroPixelFont;
                if (retroPixelFont.material != null)
                {
                    text.fontSharedMaterial = retroPixelFont.material;
                }
            }

            text.enableAutoSizing = false;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 3f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = textColor;
            text.text = label;
        }

        GothicMenuButtonEffects effects = t.GetComponent<GothicMenuButtonEffects>();
        if (effects == null)
        {
            effects = t.gameObject.AddComponent<GothicMenuButtonEffects>();
        }
    }

    public void OpenCharacterSelection()
    {
        Transform panel = transform.Find("CharacterSelectionPanel");
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            SetMainMenuVisible(false);
        }
        else
        {
            StartGame();
        }
    }

    public void OpenCollection()
    {
        SetMainMenuVisible(false);
        CollectionManager.OpenPanel(transform, () => SetMainMenuVisible(true));
    }

    public void OpenAchievements()
    {
        SetMainMenuVisible(false);
        AchievementsManager.OpenPanel(transform, () => SetMainMenuVisible(true));
    }

    public void SetMainMenuVisible(bool visible)
    {
        foreach (string n in new[] { "Start Game", "Quit Game", "Continue Button", "Collection Button", "PowerUp Button", "Achievements Button", "Title", "Title Shadow", "Title Subtitle", "TopCoinCapsule", "OptionsButton" })
        {
            Transform t = transform.Find(n);
            if (t != null)
            {
                // Continue chỉ hiện nếu có save
                if (n == "Continue Button" && visible)
                {
                    RunSaveData saved = RunSave.Load();
                    t.gameObject.SetActive(saved != null);
                }
                else
                {
                    t.gameObject.SetActive(visible);
                }
            }
        }

        if (visible) UpdateCoinCapsule();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(firstLevelName);
    }

    public void ContinueGame()
    {
        RunSaveData saved = RunSave.Load();
        if (saved == null) return;

        PlayerPrefs.SetInt("SelectedCharacterIndex", saved.characterIndex);
        PlayerPrefs.Save();
        RunSave.continueRequested = true;
        SceneManager.LoadScene(string.IsNullOrEmpty(firstLevelName) ? "Main" : firstLevelName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("I'm Quitting");
    }

    /// <summary>
    /// Tạo Sprite nút bấm 9-Slice Hoàng Gia Gothic với viền vàng vát 3D và ngọc quý 4 góc
    /// </summary>
    public static Sprite CreateRoyalGothicButton(Color innerDark, Color innerLight, Color borderGold, Color highlightGold, Color jewelColor)
    {
        int w = 64;
        int h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color darkIron = new Color(0.10f, 0.10f, 0.12f, 1f);
        Color shadowBronze = borderGold * 0.52f;
        shadowBronze.a = 1f;

        for (int y = 0; y < h; y++)
        {
            float tY = (float)y / (h - 1);
            Color currentInner = Color.Lerp(innerDark, innerLight, tY);

            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    p[y * w + x] = darkIron;
                }
                else if (x <= 3 || x >= w - 4 || y <= 3 || y >= h - 4)
                {
                    bool isCornerGem = (x >= 2 && x <= 4 && y >= 2 && y <= 4) ||
                                       (x >= w - 5 && x <= w - 3 && y >= 2 && y <= 4) ||
                                       (x >= 2 && x <= 4 && y >= h - 5 && y <= h - 3) ||
                                       (x >= w - 5 && x <= w - 3 && y >= h - 5 && y <= h - 3);

                    if (isCornerGem)
                    {
                        if ((x == 3 && y == 3) || (x == w - 4 && y == 3) || (x == 3 && y == h - 4) || (x == w - 4 && y == h - 4))
                        {
                            p[y * w + x] = Color.white;
                        }
                        else
                        {
                            p[y * w + x] = jewelColor;
                        }
                    }
                    else if (y >= h - 3 || x <= 2)
                    {
                        p[y * w + x] = highlightGold;
                    }
                    else if (y <= 2 || x >= w - 3)
                    {
                        p[y * w + x] = shadowBronze;
                    }
                    else
                    {
                        p[y * w + x] = borderGold;
                    }
                }
                else if (x == 5 || x == w - 6 || y == 5 || y == h - 6)
                {
                    p[y * w + x] = borderGold * 0.72f;
                }
                else
                {
                    p[y * w + x] = currentInner;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
    }
}

/// <summary>
/// Hiệu ứng tương tác phóng to thu nhỏ và âm thanh khi rê chuột vào Nút Menu
/// </summary>
public class GothicMenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rt;
    private Vector3 baseScale = Vector3.one;
    private float targetScale = 1.0f;
    private float currentScale = 1.0f;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        if (rt != null) baseScale = rt.localScale;
    }

    void Update()
    {
        currentScale = Mathf.Lerp(currentScale, targetScale, Time.unscaledDeltaTime * 14f);
        if (rt != null)
        {
            rt.localScale = baseScale * currentScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = 1.045f;
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = 1.0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = 0.96f;
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(1);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = 1.045f;
    }
}

/// <summary>
/// Tạo hiệu ứng bầy tàn lửa ma mị bay lượn trên màn hình nền lâu đài đỏ
/// </summary>
public class GothicMenuEmbers : MonoBehaviour
{
    private struct EmberParticle
    {
        public RectTransform rt;
        public Image img;
        public float speedY;
        public float driftSpeedX;
        public float driftOffset;
        public float size;
        public float baseAlpha;
    }

    private readonly List<EmberParticle> embers = new List<EmberParticle>();
    private const int EMBER_COUNT = 36;
    private static Sprite emberDotSprite;

    void Start()
    {
        CreateEmberSprite();
        SpawnEmbers();
    }

    private void CreateEmberSprite()
    {
        if (emberDotSprite != null) return;
        int s = 16;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        Color[] p = new Color[s * s];
        Vector2 c = new Vector2(s * 0.5f, s * 0.5f);
        float r = s * 0.48f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                if (d <= r)
                {
                    float a = Mathf.Clamp01(1f - (d / r));
                    p[y * s + x] = new Color(1f, 1f, 1f, a * a);
                }
                else
                {
                    p[y * s + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(p);
        tex.Apply();
        emberDotSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
    }

    private void SpawnEmbers()
    {
        Color[] emberColors = new[]
        {
            new Color(1.0f, 0.42f, 0.15f),
            new Color(1.0f, 0.22f, 0.28f),
            new Color(1.0f, 0.88f, 0.35f),
            new Color(0.95f, 0.12f, 0.18f)
        };

        for (int i = 0; i < EMBER_COUNT; i++)
        {
            GameObject go = new GameObject("Ember_" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            float startX = Random.Range(-920f, 920f);
            float startY = Random.Range(-540f, 540f);
            rt.anchoredPosition = new Vector2(startX, startY);

            float size = Random.Range(5f, 13f);
            rt.sizeDelta = new Vector2(size, size);

            Image img = go.GetComponent<Image>();
            img.sprite = emberDotSprite;
            img.color = emberColors[Random.Range(0, emberColors.Length)];
            img.raycastTarget = false;

            embers.Add(new EmberParticle
            {
                rt = rt,
                img = img,
                speedY = Random.Range(35f, 85f),
                driftSpeedX = Random.Range(1.2f, 2.5f),
                driftOffset = Random.Range(0f, Mathf.PI * 2f),
                size = size,
                baseAlpha = Random.Range(0.45f, 0.85f)
            });
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        for (int i = 0; i < embers.Count; i++)
        {
            EmberParticle ep = embers[i];
            if (ep.rt == null) continue;

            Vector2 pos = ep.rt.anchoredPosition;
            pos.y += ep.speedY * dt;
            pos.x += Mathf.Sin(Time.time * ep.driftSpeedX + ep.driftOffset) * (dt * 18f);

            if (pos.y > 580f)
            {
                pos.y = -580f;
                pos.x = Random.Range(-920f, 920f);
            }

            ep.rt.anchoredPosition = pos;

            if (ep.img != null)
            {
                float shimmer = Mathf.Sin(Time.time * 4f + ep.driftOffset) * 0.2f;
                Color c = ep.img.color;
                c.a = Mathf.Clamp01(ep.baseAlpha + shimmer);
                ep.img.color = c;
            }
        }
    }
}
