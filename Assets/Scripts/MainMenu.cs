using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string firstLevelName; // The first level name. GK

    private TMP_FontAsset retroPixelFont;
    private RectTransform titleRect;
    private RectTransform titleShadowRect;
    private Vector2 titleBasePos;
    private Vector2 shadowBasePos;
    private GameObject currentModal = null;

    void Start()
    {
        GameSettings.ApplyAll();
        CacheRetroFont();
        CreateGothicAtmosphereOverlay();
        CreateGothicEmbersOverlay();
        ShowMainMenuFirst();
        SetupTitle();
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
        img.color = new Color(0.04f, 0.02f, 0.06f, 0.45f); // Lớp sương mù ma mị
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
        embersGO.transform.SetSiblingIndex(1); // Nằm trên nền lâu đài, dưới các nút bấm

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

        foreach (string buttonName in new[] { "Start Game", "Quit Game", "Continue Button", "How To Play Button", "Settings Button", "Power Up Button", "Codex Button", "TreasuryPill" })
        {
            Transform t = transform.Find(buttonName);
            if (t != null) t.gameObject.SetActive(true);
        }
    }

    // Title: Phong cách Gothic cổ điển với sắc Đỏ Huyết Dụ ma mị và Vàng Cổ Hoàng Gia
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

        // Bóng đen hắc ám sâu thẳm
        ConfigureTitle(shadow, "<color=#070104>ALIEN\nSURVIVORS</color>", new Vector2(8f, -8f));

        // Tiêu đề: Đỏ thẫm máu ma mị kết hợp vàng hoàng kim cổ kính
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
        srt.anchoredPosition = new Vector2(0f, 65f);

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

    // Nút bấm phong cách Gothic Retro cổ kính với viền vàng son và đá quý chạm khắc
    void SetupMenuButtons()
    {
        Transform start = transform.Find("Start Game");
        Transform quit = transform.Find("Quit Game");
        if (start == null || quit == null) return;

        RunSaveData saved = RunSave.Load();
        bool canContinue = saved != null;

        Transform cont = transform.Find("Continue Button");
        if (canContinue && cont == null)
        {
            cont = Instantiate(start.gameObject, start.parent).transform;
            cont.name = "Continue Button";
            cont.SetSiblingIndex(start.GetSiblingIndex() + 1);
            Button b = cont.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(ContinueGame);
        }
        if (cont != null) cont.gameObject.SetActive(canContinue);

        // Tạo các nút 9-slice Hoàng Gia Gothic với viền vàng kim, góc đính đá quý Ruby/Emerald/Sapphire
        Sprite goldButton = CreateRoyalGothicButton(
            new Color(0.08f, 0.09f, 0.16f), // Đáy nhung Dạ Lam tối
            new Color(0.18f, 0.20f, 0.32f), // Đỉnh nhung ánh tím xanh
            new Color(0.86f, 0.68f, 0.22f), // Viền vàng hoàng kim
            new Color(1.0f, 0.92f, 0.48f),  // Ánh kim rực sáng
            new Color(0.90f, 0.18f, 0.22f)   // Ngọc Ruby góc
        );

        Sprite emeraldButton = CreateRoyalGothicButton(
            new Color(0.04f, 0.18f, 0.09f), // Đáy nhung Lục Bảo sâu thẳm
            new Color(0.10f, 0.36f, 0.18f), // Đỉnh nhung ngọc bích
            new Color(0.88f, 0.74f, 0.25f), // Viền vàng hoàng gia
            new Color(1.0f, 0.96f, 0.55f),  // Ánh kim chói rạng
            new Color(0.20f, 0.98f, 0.45f)  // Ngọc Lục Bảo góc
        );

        Sprite crimsonButton = CreateRoyalGothicButton(
            new Color(0.22f, 0.05f, 0.07f), // Đáy nhung Đỏ Huyết Tế
            new Color(0.44f, 0.10f, 0.14f), // Đỉnh nhung đỏ rực
            new Color(0.82f, 0.62f, 0.20f), // Viền vàng cổ
            new Color(0.98f, 0.84f, 0.42f), // Ánh kim lấp lánh
            new Color(0.95f, 0.22f, 0.22f)  // Ngọc Hồng Ngọc góc
        );

        float[] y = canContinue ? new[] { -90f, -210f, -330f } : new[] { 0f, -120f, -240f };
        if (canContinue) StyleButton(cont, emeraldButton, "✦  CONTINUE RUN  ✦\n<size=52%>" + RunSave.Describe(saved) + "</size>", y[0], new Color(0.95f, 1f, 0.92f));
        StyleButton(start, goldButton, canContinue ? "✦  NEW GAME  ✦" : "✦  START GAME  ✦", y[1], new Color(1f, 0.94f, 0.58f));
        StyleButton(quit, crimsonButton, "✦  QUIT GAME  ✦", y[2], new Color(1f, 0.88f, 0.88f));

        SetupAuxiliaryButtons(start, canContinue);
    }

    private void SetupAuxiliaryButtons(Transform templateBtn, bool canContinue)
    {
        Sprite sapphireButton = CreateRoyalGothicButton(
            new Color(0.06f, 0.10f, 0.24f),
            new Color(0.12f, 0.24f, 0.48f),
            new Color(0.85f, 0.70f, 0.25f),
            new Color(1.0f, 0.95f, 0.55f),
            new Color(0.25f, 0.60f, 1.0f)
        );

        Sprite obsidianButton = CreateRoyalGothicButton(
            new Color(0.10f, 0.10f, 0.14f),
            new Color(0.20f, 0.22f, 0.28f),
            new Color(0.85f, 0.70f, 0.25f),
            new Color(1.0f, 0.95f, 0.55f),
            new Color(0.70f, 0.80f, 0.95f)
        );

        Sprite amberButton = CreateRoyalGothicButton(
            new Color(0.22f, 0.14f, 0.04f),
            new Color(0.44f, 0.28f, 0.08f),
            new Color(1.0f, 0.82f, 0.25f),
            new Color(1.0f, 0.95f, 0.60f),
            new Color(1.0f, 0.70f, 0.10f)
        );

        Sprite amethystButton = CreateRoyalGothicButton(
            new Color(0.18f, 0.06f, 0.24f),
            new Color(0.36f, 0.12f, 0.48f),
            new Color(0.85f, 0.70f, 0.25f),
            new Color(1.0f, 0.95f, 0.55f),
            new Color(0.75f, 0.30f, 1.0f)
        );

        Transform howToPlay = transform.Find("How To Play Button");
        if (howToPlay == null)
        {
            howToPlay = Instantiate(templateBtn.gameObject, templateBtn.parent).transform;
            howToPlay.name = "How To Play Button";
            Button b = howToPlay.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(OpenHowToPlayModal);
        }
        howToPlay.gameObject.SetActive(true);

        Transform settings = transform.Find("Settings Button");
        if (settings == null)
        {
            settings = Instantiate(templateBtn.gameObject, templateBtn.parent).transform;
            settings.name = "Settings Button";
            Button b = settings.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(OpenSettingsModal);
        }
        settings.gameObject.SetActive(true);

        Transform powerUp = transform.Find("Power Up Button");
        if (powerUp == null)
        {
            powerUp = Instantiate(templateBtn.gameObject, templateBtn.parent).transform;
            powerUp.name = "Power Up Button";
            Button b = powerUp.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(OpenPowerUpModal);
        }
        powerUp.gameObject.SetActive(true);

        Transform codex = transform.Find("Codex Button");
        if (codex == null)
        {
            codex = Instantiate(templateBtn.gameObject, templateBtn.parent).transform;
            codex.name = "Codex Button";
            Button b = codex.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(OpenCodexModal);
        }
        codex.gameObject.SetActive(true);

        float auxY = canContinue ? -415f : -345f;
        StyleAuxButton(howToPlay, sapphireButton, "✦  HOW TO PLAY  ✦", -135f, auxY, new Color(0.88f, 0.95f, 1f));
        StyleAuxButton(settings, obsidianButton, "✦  SETTINGS  ✦", 135f, auxY, new Color(1f, 0.95f, 0.85f));

        StyleAuxButton(powerUp, amberButton, "✦  POWER UP  ✦", -135f, auxY - 68f, new Color(1f, 0.92f, 0.50f));
        StyleAuxButton(codex, amethystButton, "✦  CODEX & FEATS  ✦", 135f, auxY - 68f, new Color(0.95f, 0.85f, 1f));

        Transform stageBtn = transform.Find("Stage Select Button");
        if (stageBtn == null)
        {
            stageBtn = Instantiate(templateBtn.gameObject, templateBtn.parent).transform;
            stageBtn.name = "Stage Select Button";
            Button b = stageBtn.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(OnStageToggleClicked);
        }
        stageBtn.gameObject.SetActive(true);
        UpdateStageButtonVisual(stageBtn);

        CreateTreasuryPill();
    }

    private Transform stageSelectBtnTransform;

    private void UpdateStageButtonVisual(Transform stageBtn = null)
    {
        if (stageBtn == null) stageBtn = stageSelectBtnTransform ?? transform.Find("Stage Select Button");
        if (stageBtn == null) return;
        stageSelectBtnTransform = stageBtn;

        bool isInferno = (MapSelectionManager.CurrentStage == MapStage.InfernoPurgatory);

        Sprite btnSprite = isInferno ?
            CreateRoyalGothicButton(
                new Color(0.35f, 0.05f, 0.05f),
                new Color(0.65f, 0.15f, 0.12f),
                new Color(1.0f, 0.75f, 0.20f),
                new Color(1.0f, 0.95f, 0.60f),
                new Color(1.0f, 0.35f, 0.15f)
            ) :
            CreateRoyalGothicButton(
                new Color(0.04f, 0.18f, 0.14f),
                new Color(0.08f, 0.32f, 0.24f),
                new Color(0.65f, 0.95f, 0.70f),
                new Color(0.90f, 1.0f, 0.95f),
                new Color(0.20f, 0.85f, 0.55f)
            );

        string label = isInferno ? "âœ¦ MAP: INFERNO PURGATORY (HARD) âœ¦" : "âœ¦ MAP: CATACOMBS (NORMAL) âœ¦";
        Color textColor = isInferno ? new Color(1.0f, 0.85f, 0.45f) : new Color(0.85f, 1.0f, 0.90f);

        RectTransform rt = (RectTransform)stageBtn;
        rt.sizeDelta = new Vector2(510f, 62f);
        bool canContinue = PlayerPrefs.HasKey("CurrentLevel") && PlayerPrefs.GetInt("CurrentLevel") > 0;
        float auxY = canContinue ? -415f : -345f;
        rt.anchoredPosition = new Vector2(0f, auxY - 134f);

        Image img = stageBtn.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = btnSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_Text text = stageBtn.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            if (retroPixelFont != null) text.font = retroPixelFont;
            text.enableAutoSizing = false;
            text.fontSize = 21f;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 2f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = textColor;
            text.text = label;
        }

        if (stageBtn.GetComponent<GothicMenuButtonEffects>() == null)
        {
            stageBtn.gameObject.AddComponent<GothicMenuButtonEffects>();
        }
    }

    public void OnStageToggleClicked()
    {
        MapSelectionManager.ToggleStage();
        UpdateStageButtonVisual();
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        ProceduralSFX.PlayLevelUp();
    }

    private void StyleAuxButton(Transform t, Sprite sprite, string label, float x, float y, Color textColor)
    {
        RectTransform rt = (RectTransform)t;
        rt.sizeDelta = new Vector2(240f, 65f);
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
            text.fontSize = 24f;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 2.5f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = textColor;
            text.text = label;
        }

        if (t.GetComponent<GothicMenuButtonEffects>() == null)
        {
            t.gameObject.AddComponent<GothicMenuButtonEffects>();
        }
    }

    private void StyleButton(Transform t, Sprite sprite, string label, float y, Color textColor)
    {
        RectTransform rt = (RectTransform)t;
        rt.sizeDelta = new Vector2(510f, 105f);
        rt.anchoredPosition = new Vector2(0f, y);

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
            text.fontSize = 44f;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 3.5f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = textColor;
            text.text = label;
        }

        // Thêm hiệu ứng tương tác Hover mượt mà, phóng to nhẹ và âm thanh phản hồi
        GothicMenuButtonEffects effects = t.GetComponent<GothicMenuButtonEffects>();
        if (effects == null)
        {
            effects = t.gameObject.AddComponent<GothicMenuButtonEffects>();
        }
    }

    // =========================================================================
    // MODAL DIALOGS: HOW TO PLAY & SETTINGS
    // =========================================================================

    public void CloseCurrentModal()
    {
        if (currentModal != null)
        {
            Destroy(currentModal);
            currentModal = null;
        }
    }

    public void OpenHowToPlayModal()
    {
        CloseCurrentModal();

        GameObject overlay = new GameObject("HowToPlayModalOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();
        RectTransform ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.sizeDelta = Vector2.zero;
        Image oimg = overlay.GetComponent<Image>();
        oimg.color = new Color(0f, 0f, 0f, 0.82f);

        GameObject win = new GameObject("HowToPlayWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        win.transform.SetParent(overlay.transform, false);
        RectTransform wrt = win.GetComponent<RectTransform>();
        wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(860f, 640f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "✦ HOW TO PLAY & COMBAT GUIDE ✦", new Vector2(0f, 260f), 32f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "✦ MASTER THE ART OF NIGHTMARE SURVIVAL ✦", new Vector2(0f, 226f), 18f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal);

        string body = "<b>🎮 DI CHUYỂN:</b> <color=#FFE040>[W, A, S, D]</color> hoặc <color=#FFE040>[Phím Mũi Tên]</color>\n" +
                      "<b>💨 LƯỚT NÉ ĐÒN (DASH):</b> <color=#FFE040>[Phím Space]</color> (Tăng tốc tức thì & bất tử i-Frames né đòn)\n" +
                      "<b>⚡ CHIÊU CUỐI (ULTIMATE):</b> <color=#FFE040>[Phím E]</color> hoặc <color=#FFE040>[Chuột Phải]</color> (Kích hoạt khi đầy 100% Nộ)\n" +
                      "<b>⏸ TẠM DỪNG & LƯU TRẬN:</b> <color=#FFE040>[Phím Esc]</color> (Lưu ván đấu hiện tại để chơi tiếp sau)\n" +
                      "<b>🏛 ĐÀN TẾ CỔ (SHRINES):</b> Bước vào đàn tế để nhận chúc phúc (Đóng băng, Mưa vàng, Cuồng nộ)\n" +
                      "<b>🏺 CHƯỚNG NGẠI VẬT:</b> Đánh vỡ bia mộ, bình gốm để nhặt Vàng và Thịt nướng hồi máu\n" +
                      "<b>🔥 TIẾN HÓA THẦN THOẠI (RECIPES):</b>\n" +
                      "   • <color=#66FFAA>Sword + Damage Multiplier</color> ➔ <color=#FF5566>Thousand Blades (Kiếm Bão)</color>\n" +
                      "   • <color=#66FFAA>Fireball + Pickup Range</color> ➔ <color=#FFD700>Solar Flare Ring (Vành Hào Quang)</color>\n" +
                      "<b>🛠 BẢNG DEMO BÁO CÁO:</b> <color=#00FFAA>[Phím F1]</color> hoặc <color=#00FFAA>[Phím ~]</color> để gọi nhanh Boss, God Mode, Vàng...";

        GameObject textGO = new GameObject("GuideText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(win.transform, false);
        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(780f, 380f);
        trt.anchoredPosition = new Vector2(0f, 15f);
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = body;
        tmp.fontSize = 20f;
        tmp.lineSpacing = 12f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        CreateModalCloseButton(win.transform, new Vector2(0f, -255f), "✦  CLOSE  ✦");

        currentModal = overlay;
    }

    public void OpenSettingsModal()
    {
        CloseCurrentModal();

        GameObject overlay = new GameObject("SettingsModalOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();
        RectTransform ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.sizeDelta = Vector2.zero;
        Image oimg = overlay.GetComponent<Image>();
        oimg.color = new Color(0f, 0f, 0f, 0.82f);

        GameObject win = new GameObject("SettingsWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        win.transform.SetParent(overlay.transform, false);
        RectTransform wrt = win.GetComponent<RectTransform>();
        wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(760f, 540f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "✦ AUDIO & DISPLAY SETTINGS ✦", new Vector2(0f, 215f), 32f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "✦ CONFIGURATION PREFERENCES ✦", new Vector2(0f, 182f), 18f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal);

        CreateVolumeRow(win.transform, "MASTER VOLUME", 100f, () => GameSettings.MasterVolume, v => GameSettings.MasterVolume = v);
        CreateVolumeRow(win.transform, "BGM MUSIC", 30f, () => GameSettings.BGMVolume, v => GameSettings.BGMVolume = v);
        CreateVolumeRow(win.transform, "SFX SOUNDS", -40f, () => GameSettings.SFXVolume, v => GameSettings.SFXVolume = v);

        CreateDisplayToggleRow(win.transform, -115f);

        CreateModalCloseButton(win.transform, new Vector2(0f, -205f), "✦  SAVE & CLOSE  ✦");

        currentModal = overlay;
    }

    private void CreateModalText(Transform parent, string text, Vector2 pos, float fontSize, Color color, FontStyles style)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(700f, 40f);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) tmp.font = retroPixelFont;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private void CreateVolumeRow(Transform parent, string title, float y, System.Func<float> getter, System.Action<float> setter)
    {
        GameObject row = new GameObject("Row_" + title, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        RectTransform rrt = row.GetComponent<RectTransform>();
        rrt.anchorMin = rrt.anchorMax = rrt.pivot = new Vector2(0.5f, 0.5f);
        rrt.sizeDelta = new Vector2(650f, 50f);
        rrt.anchoredPosition = new Vector2(0f, y);

        GameObject lbl = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        lbl.transform.SetParent(row.transform, false);
        RectTransform lrt = lbl.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.5f);
        lrt.anchorMax = new Vector2(0f, 0.5f);
        lrt.pivot = new Vector2(0f, 0.5f);
        lrt.anchoredPosition = new Vector2(30f, 0f);
        lrt.sizeDelta = new Vector2(250f, 40f);
        TextMeshProUGUI ltmp = lbl.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) ltmp.font = retroPixelFont;
        ltmp.text = title;
        ltmp.fontSize = 24f;
        ltmp.color = new Color(0.95f, 0.88f, 0.65f);
        ltmp.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject valGO = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
        valGO.transform.SetParent(row.transform, false);
        RectTransform vrt = valGO.GetComponent<RectTransform>();
        vrt.anchorMin = vrt.anchorMax = vrt.pivot = new Vector2(0.5f, 0.5f);
        vrt.anchoredPosition = new Vector2(100f, 0f);
        vrt.sizeDelta = new Vector2(100f, 40f);
        TextMeshProUGUI vtmp = valGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) vtmp.font = retroPixelFont;
        vtmp.fontSize = 26f;
        vtmp.fontStyle = FontStyles.Bold;
        vtmp.color = Color.white;
        vtmp.alignment = TextAlignmentOptions.Center;
        vtmp.text = $"{(int)(getter() * 100)}%";

        Sprite btnSpr = CreateRoyalGothicButton(
            new Color(0.12f, 0.14f, 0.22f), new Color(0.22f, 0.26f, 0.38f),
            new Color(0.85f, 0.68f, 0.22f), new Color(1.0f, 0.92f, 0.48f),
            new Color(0.2f, 0.6f, 1f)
        );

        CreateSmallRowButton(row.transform, new Vector2(35f, 0f), "−", btnSpr, () => {
            float val = Mathf.Clamp01(getter() - 0.1f);
            setter(val);
            vtmp.text = $"{(int)(val * 100)}%";
        });

        CreateSmallRowButton(row.transform, new Vector2(165f, 0f), "+", btnSpr, () => {
            float val = Mathf.Clamp01(getter() + 0.1f);
            setter(val);
            vtmp.text = $"{(int)(val * 100)}%";
        });
    }

    private void CreateSmallRowButton(Transform parent, Vector2 pos, string label, Sprite sprite, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(45f, 45f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;

        Button b = go.GetComponent<Button>();
        b.onClick.AddListener(action);

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        go.AddComponent<GothicMenuButtonEffects>();
    }

    private void CreateDisplayToggleRow(Transform parent, float y)
    {
        Sprite btnSpr = CreateRoyalGothicButton(
            new Color(0.10f, 0.12f, 0.20f), new Color(0.20f, 0.24f, 0.35f),
            new Color(0.85f, 0.68f, 0.22f), new Color(1.0f, 0.92f, 0.48f),
            new Color(0.85f, 0.35f, 0.15f)
        );

        GameObject go = new GameObject("Btn_Fullscreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
        rt.sizeDelta = new Vector2(420f, 55f);

        Image img = go.GetComponent<Image>();
        img.sprite = btnSpr;
        img.type = Image.Type.Sliced;

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) tmp.font = retroPixelFont;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.95f, 0.95f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;

        void UpdateLabel()
        {
            tmp.text = Screen.fullScreen ? "🖥 DISPLAY MODE: FULLSCREEN" : "🪟 DISPLAY MODE: WINDOWED";
        }
        UpdateLabel();

        Button b = go.GetComponent<Button>();
        b.onClick.AddListener(() => {
            Screen.fullScreen = !Screen.fullScreen;
            UpdateLabel();
        });

        go.AddComponent<GothicMenuButtonEffects>();
    }

    private void CreateModalCloseButton(Transform parent, Vector2 pos, string label)
    {
        Sprite crimsonButton = CreateRoyalGothicButton(
            new Color(0.22f, 0.05f, 0.07f), new Color(0.44f, 0.10f, 0.14f),
            new Color(0.82f, 0.62f, 0.20f), new Color(0.98f, 0.84f, 0.42f),
            new Color(0.95f, 0.22f, 0.22f)
        );

        GameObject go = new GameObject("Btn_Close", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(260f, 55f);

        Image img = go.GetComponent<Image>();
        img.sprite = crimsonButton;
        img.type = Image.Type.Sliced;

        Button b = go.GetComponent<Button>();
        b.onClick.AddListener(CloseCurrentModal);

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) tmp.font = retroPixelFont;
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.92f, 0.92f);
        tmp.alignment = TextAlignmentOptions.Center;

        go.AddComponent<GothicMenuButtonEffects>();
    }

    private void CreateCustomModalButton(Transform parent, Vector2 pos, Vector2 size, string label, Sprite sprite, UnityEngine.Events.UnityAction action)
    {
        GameObject go = new GameObject("Btn_" + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;

        Button b = go.GetComponent<Button>();
        b.onClick.AddListener(action);

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(go.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) tmp.font = retroPixelFont;
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        go.AddComponent<GothicMenuButtonEffects>();
    }

    private void CreateTreasuryPill()
    {
        Transform existing = transform.Find("TreasuryPill");
        if (existing != null) Destroy(existing.gameObject);

        int coins = PlayerPrefs.GetInt("SavedCoins", 0);

        GameObject pill = new GameObject("TreasuryPill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        pill.transform.SetParent(transform, false);

        RectTransform rt = pill.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-28f, -24f);
        rt.sizeDelta = new Vector2(230f, 52f);

        Image img = pill.GetComponent<Image>();
        img.sprite = CreateRoyalGothicButton(
            new Color(0.18f, 0.12f, 0.04f), new Color(0.36f, 0.24f, 0.08f),
            new Color(1f, 0.82f, 0.25f), new Color(1f, 0.95f, 0.60f),
            new Color(1f, 0.70f, 0.10f)
        );
        img.type = Image.Type.Sliced;

        Button b = pill.GetComponent<Button>();
        b.onClick.AddListener(OpenPowerUpModal);

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(pill.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) tmp.font = retroPixelFont;
        tmp.text = $"🪙 {coins} GOLD";
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.92f, 0.45f);
        tmp.alignment = TextAlignmentOptions.Center;

        pill.AddComponent<GothicMenuButtonEffects>();
    }

    public void OpenPowerUpModal()
    {
        CloseCurrentModal();

        GameObject overlay = new GameObject("PowerUpModalOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();
        RectTransform ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.sizeDelta = Vector2.zero;
        Image oimg = overlay.GetComponent<Image>();
        oimg.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject win = new GameObject("PowerUpWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        win.transform.SetParent(overlay.transform, false);
        RectTransform wrt = win.GetComponent<RectTransform>();
        wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(920f, 660f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "✦ POWER-UP SANCTUARY ✦", new Vector2(0f, 280f), 32f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "✦ PERMANENT ENHANCEMENTS FOR ALL CHARACTERS ✦", new Vector2(0f, 248f), 17f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal);

        GameObject treasuryGO = new GameObject("TreasuryText", typeof(RectTransform), typeof(TextMeshProUGUI));
        treasuryGO.transform.SetParent(win.transform, false);
        RectTransform trt = treasuryGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.anchoredPosition = new Vector2(0f, 215f);
        trt.sizeDelta = new Vector2(700f, 32f);
        TextMeshProUGUI treasuryTmp = treasuryGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) treasuryTmp.font = retroPixelFont;
        treasuryTmp.fontSize = 22f;
        treasuryTmp.fontStyle = FontStyles.Bold;
        treasuryTmp.color = new Color(1f, 0.92f, 0.45f);
        treasuryTmp.alignment = TextAlignmentOptions.Center;

        GameObject gridContainer = new GameObject("StatGrid", typeof(RectTransform));
        gridContainer.transform.SetParent(win.transform, false);
        RectTransform grt = gridContainer.GetComponent<RectTransform>();
        grt.anchorMin = grt.anchorMax = grt.pivot = new Vector2(0.5f, 0.5f);
        grt.sizeDelta = new Vector2(880f, 400f);
        grt.anchoredPosition = new Vector2(0f, 15f);

        void RefreshShopUI()
        {
            int currentCoins = PlayerPrefs.GetInt("SavedCoins", 0);
            treasuryTmp.text = $"✦ TREASURY BALANCE: <color=#FFE259>{currentCoins}</color> GOLD ✦";

            for (int i = gridContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContainer.transform.GetChild(i).gameObject);
            }

            MetaStat[] stats = (MetaStat[])System.Enum.GetValues(typeof(MetaStat));
            float[] colX = new float[] { -220f, 220f };
            float[] rowY = new float[] { 135f, 50f, -35f, -120f };

            for (int i = 0; i < stats.Length; i++)
            {
                MetaStat stat = stats[i];
                if (!MetaProgression.StatDatabase.TryGetValue(stat, out MetaStatInfo info)) continue;

                int cIdx = i / 4;
                int rIdx = i % 4;
                Vector2 cardPos = new Vector2(colX[cIdx], rowY[rIdx]);

                CreateMetaStatCard(gridContainer.transform, stat, info, cardPos, currentCoins, RefreshShopUI);
            }

            CreateTreasuryPill();
        }

        RefreshShopUI();

        Sprite obsidianBtn = CreateRoyalGothicButton(
            new Color(0.12f, 0.12f, 0.16f), new Color(0.24f, 0.24f, 0.32f),
            new Color(0.85f, 0.70f, 0.25f), new Color(1.0f, 0.95f, 0.55f),
            new Color(0.70f, 0.80f, 0.95f)
        );
        CreateCustomModalButton(win.transform, new Vector2(-150f, -270f), new Vector2(240f, 52f), "✦  REFUND ALL  ✦", obsidianBtn, () => {
            MetaProgression.RefundAll();
            ProceduralSFX.PlayLevelUp();
            RefreshShopUI();
        });

        CreateModalCloseButton(win.transform, new Vector2(150f, -270f), "✦  CLOSE  ✦");

        currentModal = overlay;
    }

    private void CreateMetaStatCard(Transform parent, MetaStat stat, MetaStatInfo info, Vector2 pos, int currentCoins, System.Action onPurchased)
    {
        int lvl = MetaProgression.GetLevel(stat);
        bool isMax = lvl >= info.maxLevel;
        int cost = MetaProgression.GetCost(stat);
        bool canAfford = currentCoins >= cost && !isMax;

        GameObject card = new GameObject("Card_" + stat.ToString(), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform crt = card.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = pos;
        crt.sizeDelta = new Vector2(420f, 74f);

        Image cimg = card.GetComponent<Image>();
        cimg.sprite = CreateRoyalGothicButton(
            new Color(0.06f, 0.06f, 0.09f), new Color(0.12f, 0.13f, 0.18f),
            new Color(0.65f, 0.52f, 0.20f), new Color(0.92f, 0.82f, 0.45f),
            canAfford ? new Color(0.2f, 0.9f, 0.4f) : new Color(0.5f, 0.5f, 0.5f)
        );
        cimg.type = Image.Type.Sliced;

        string pips = "";
        for (int p = 0; p < info.maxLevel; p++)
        {
            pips += (p < lvl) ? "<color=#FFD700>■</color>" : "<color=#4A4A5A>□</color>";
        }

        GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(card.transform, false);
        RectTransform trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.5f);
        trt.anchorMax = new Vector2(0f, 0.5f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(16f, 13f);
        trt.sizeDelta = new Vector2(275f, 30f);
        TextMeshProUGUI ttmp = titleGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) ttmp.font = retroPixelFont;
        ttmp.fontSize = 19f;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.color = new Color(1f, 0.90f, 0.55f);
        ttmp.text = $"{info.displayName}  {pips}";

        GameObject descGO = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGO.transform.SetParent(card.transform, false);
        RectTransform drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0f, 0.5f);
        drt.anchorMax = new Vector2(0f, 0.5f);
        drt.pivot = new Vector2(0f, 0.5f);
        drt.anchoredPosition = new Vector2(16f, -14f);
        drt.sizeDelta = new Vector2(275f, 24f);
        TextMeshProUGUI dtmp = descGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) dtmp.font = retroPixelFont;
        dtmp.fontSize = 15.5f;
        dtmp.color = new Color(0.80f, 0.85f, 0.90f, 0.9f);
        dtmp.text = isMax ? $"<color=#FFE066>Cực Đại (Max Level {lvl}/{info.maxLevel})</color>" : $"{info.bonusPerLevel}";

        GameObject btnGO = new GameObject("BuyBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(card.transform, false);
        RectTransform brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1f, 0.5f);
        brt.anchorMax = new Vector2(1f, 0.5f);
        brt.pivot = new Vector2(1f, 0.5f);
        brt.anchoredPosition = new Vector2(-12f, 0f);
        brt.sizeDelta = new Vector2(105f, 44f);

        Image bimg = btnGO.GetComponent<Image>();
        Sprite btnSprite = isMax ?
            CreateRoyalGothicButton(new Color(0.1f, 0.1f, 0.12f), new Color(0.18f, 0.18f, 0.22f), new Color(0.4f, 0.4f, 0.45f), new Color(0.6f, 0.6f, 0.65f), Color.gray) :
            (canAfford ?
                CreateRoyalGothicButton(new Color(0.04f, 0.18f, 0.08f), new Color(0.10f, 0.36f, 0.16f), new Color(0.85f, 0.70f, 0.25f), new Color(1f, 0.95f, 0.55f), new Color(0.2f, 0.95f, 0.4f)) :
                CreateRoyalGothicButton(new Color(0.16f, 0.06f, 0.08f), new Color(0.28f, 0.10f, 0.14f), new Color(0.5f, 0.35f, 0.2f), new Color(0.7f, 0.5f, 0.3f), Color.red));

        bimg.sprite = btnSprite;
        bimg.type = Image.Type.Sliced;

        Button btn = btnGO.GetComponent<Button>();
        btn.interactable = canAfford;
        if (canAfford)
        {
            btn.onClick.AddListener(() => {
                if (MetaProgression.PurchaseUpgrade(stat))
                {
                    ProceduralSFX.PlayChestOpen();
                    onPurchased?.Invoke();
                }
            });
            btnGO.AddComponent<GothicMenuButtonEffects>();
        }

        GameObject btnTxtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTxtGO.transform.SetParent(btnGO.transform, false);
        RectTransform btrt = btnTxtGO.GetComponent<RectTransform>();
        btrt.anchorMin = Vector2.zero;
        btrt.anchorMax = Vector2.one;
        btrt.sizeDelta = Vector2.zero;
        TextMeshProUGUI btmp = btnTxtGO.GetComponent<TextMeshProUGUI>();
        if (retroPixelFont != null) btmp.font = retroPixelFont;
        btmp.fontSize = 17f;
        btmp.fontStyle = FontStyles.Bold;
        btmp.alignment = TextAlignmentOptions.Center;
        btmp.color = isMax ? new Color(0.7f, 0.7f, 0.7f) : (canAfford ? new Color(1f, 0.95f, 0.6f) : new Color(0.8f, 0.45f, 0.45f));
        btmp.text = isMax ? "MAX" : $"{cost} G";
    }

    public void OpenCodexModal()
    {
        CloseCurrentModal();

        GameObject overlay = new GameObject("CodexModalOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();
        RectTransform ort = overlay.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.sizeDelta = Vector2.zero;
        Image oimg = overlay.GetComponent<Image>();
        oimg.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject win = new GameObject("CodexWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        win.transform.SetParent(overlay.transform, false);
        RectTransform wrt = win.GetComponent<RectTransform>();
        wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(940f, 660f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "✦ WEAPON EVOLUTION CODEX & FEATS ✦", new Vector2(0f, 280f), 30f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "✦ GRIMOIRE OF FORBIDDEN ALCHEMY & SURVIVAL ACHIEVEMENTS ✦", new Vector2(0f, 248f), 17f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal);

        string codexText =
            "<color=#FFD700><b>🔥 HUYỀN THOẠI TIẾN HÓA VŨ KHÍ (EVOLUTION RECIPES):</b></color>\n" +
            " • ⚔️ <color=#66FFBB>Giant Sword (Lvl Max)</color> + <color=#FF8866>Might</color> ➔ <color=#FF4466><b>Thousand Blades (Vạn Kiếm Quy Tông)</b></color>\n" +
            "   <i>Bắn bão kiếm xoay tròn 360 độ xé nát toàn bộ kẻ thù trong tầm mắt.</i>\n\n" +
            " • 🔥 <color=#66FFBB>Magic Fireball (Lvl Max)</color> + <color=#FF8866>Magnet</color> ➔ <color=#FFAA00><b>Solar Flare Ring (Nhật Luân Hào Quang)</b></color>\n" +
            "   <i>Quả cầu lửa hợp nhất thành vành đai mặt trời thiêu rụi toàn diện xung quanh.</i>\n\n" +
            " • 🪓 <color=#66FFBB>Throwing Axes (Lvl Max)</color> + <color=#FF8866>Swiftness</color> ➔ <color=#00E5FF><b>Spiral Cyclone Axes (Cuồng Phong Phì Phủ)</b></color>\n" +
            "   <i>Bão rìu cuồng nộ xoay tròn liên hồi không ngừng nghỉ, đẩy lùi đàn quái.</i>\n\n" +
            " • 🏹 <color=#66FFBB>Forest Bow (Lvl Max)</color> + <color=#FF8866>Critical</color> ➔ <color=#E066FF><b>Celestial Piercing Arrow (Thần Tiễn Xuyên Không)</b></color>\n" +
            "   <i>Tên ánh sáng xuyên thấu toàn màn hình gây sát thương chí mạng cực đại.</i>\n\n" +
            " • 🕊️ <color=#66FFBB>Bright Zone (Lvl Max)</color> + <color=#FF8866>Vitality</color> ➔ <color=#FFFF66><b>Sanctuary of Light (Thánh Địa Trấn Hồn)</b></color>\n" +
            "   <i>Vòng hào quang cực đại liên tục hồi phục sinh lực và giáng sấm sét hủy diệt.</i>\n\n" +
            "<color=#FFD700><b>🏆 CHIẾN TÍCH & KỶ LỤC CÁ NHÂN (SURVIVAL FEATS):</b></color>\n";

        float bestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f);
        int minutes = Mathf.FloorToInt(bestTime / 60f);
        int seconds = Mathf.FloorToInt(bestTime % 60f);
        string timeStr = bestTime > 0 ? $"{minutes:00}:{seconds:00}" : "--:--";
        int coins = PlayerPrefs.GetInt("SavedCoins", 0);
        int mightLvl = MetaProgression.GetLevel(MetaStat.Might);
        int revivalLvl = MetaProgression.GetLevel(MetaStat.Revival);

        string featsText =
            $" • ⏱️ <b>Thời Gian Sống Sót Kỷ Lục:</b> <color=#FFE040>{timeStr}</color> " + (bestTime >= 900f ? "✦ <color=#00FF99>[SURVIVOR MASTER]</color>" : "") + "\n" +
            $" • 💰 <b>Kho Báu Tích Lũy:</b> <color=#FFE040>{coins} Vàng</color> " + (coins >= 1000 ? "✦ <color=#00FF99>[GOLDEN HOARDER]</color>" : "") + "\n" +
            $" • ⚔️ <b>Chiến Tướng Sát Thần:</b> Might Cấp {mightLvl}/5 " + (mightLvl >= 5 ? "✦ <color=#00FF99>[GOD OF WAR]</color>" : "") + "\n" +
            $" • 🕊️ <b>Hồi Sinh Phượng Hoàng:</b> " + (revivalLvl > 0 ? "<color=#00FF99>ĐÃ MỞ KHÓA [PHOENIX REBIRTH]</color>" : "<color=#888888>Chưa mở khóa</color>") + "\n" +
            $" • 🐉 <b>Đại Chiến Hỏa Long:</b> Sống sót 15 phút triệu hồi và đánh bại Trùm Cuối Pyroclast!";

        GameObject textGO = new GameObject("CodexText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(win.transform, false);
        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(860f, 440f);
        trt.anchoredPosition = new Vector2(0f, 15f);
        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = codexText + featsText;
        tmp.fontSize = 17.5f;
        tmp.lineSpacing = 8f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.TopLeft;

        CreateModalCloseButton(win.transform, new Vector2(0f, -270f), "✦  CLOSE  ✦");

        currentModal = overlay;
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
                // 1. Viền ngoài sắt đen Dark Iron
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    p[y * w + x] = darkIron;
                }
                // 2. Viền vàng kim vát 3D (Golden Bevel)
                else if (x <= 3 || x >= w - 4 || y <= 3 || y >= h - 4)
                {
                    // Đính ngọc quý 4 góc
                    bool isCornerGem = (x >= 2 && x <= 4 && y >= 2 && y <= 4) ||
                                       (x >= w - 5 && x <= w - 3 && y >= 2 && y <= 4) ||
                                       (x >= 2 && x <= 4 && y >= h - 5 && y <= h - 3) ||
                                       (x >= w - 5 && x <= w - 3 && y >= h - 5 && y <= h - 3);

                    if (isCornerGem)
                    {
                        // Điểm sáng trắng trung tâm ngọc
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
                        p[y * w + x] = highlightGold; // Cạnh trên & trái đón ánh sáng
                    }
                    else if (y <= 2 || x >= w - 3)
                    {
                        p[y * w + x] = shadowBronze; // Cạnh dưới & phải đổ bóng
                    }
                    else
                    {
                        p[y * w + x] = borderGold;
                    }
                }
                // 3. Đường chỉ vàng thanh tao bên trong (Inner Filigree Hairline)
                else if (x == 5 || x == w - 6 || y == 5 || y == h - 6)
                {
                    p[y * w + x] = borderGold * 0.72f;
                }
                // 4. Lớp nền nhung sang trọng
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
            new Color(1.0f, 0.42f, 0.15f), // Cam lửa
            new Color(1.0f, 0.22f, 0.28f), // Đỏ huyết tế
            new Color(1.0f, 0.88f, 0.35f), // Vàng hoàng kim
            new Color(0.95f, 0.12f, 0.18f)  // Đỏ rực
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

            // Tái sinh tàn lửa khi bay vượt màn hình
            if (pos.y > 580f)
            {
                pos.y = -580f;
                pos.x = Random.Range(-920f, 920f);
            }

            ep.rt.anchoredPosition = pos;

            // Chớp nháy lấp lánh tự nhiên
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
