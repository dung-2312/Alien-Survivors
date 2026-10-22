using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string firstLevelName; // The first level name. GK

    private TMP_FontAsset retroPixelFont;

    void Start()
    {
        CacheRetroFont();
        CreateGothicAtmosphereOverlay();
        ShowMainMenuFirst();
        SetupTitle();
        SetupMenuButtons();
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

    // The scene is saved with the Character Selection panel switched on, so every load (including coming back from
    // Save & Exit) opened on it. Start on the menu itself; Start Game / NEW GAME opens character selection.
    void ShowMainMenuFirst()
    {
        Transform panel = transform.Find("CharacterSelectionPanel");
        if (panel != null) panel.gameObject.SetActive(false);

        foreach (string buttonName in new[] { "Start Game", "Quit Game", "Continue Button" })
        {
            Transform t = transform.Find(buttonName);
            if (t != null) t.gameObject.SetActive(true); // Character select hid them when it enabled
        }
    }

    // Title: Phong cách Gothic cổ điển với sắc Đỏ Huyết Dụ ma mị và Vàng Cổ Hoàng Gia
    void SetupTitle()
    {
        Transform logo = transform.Find("VS_Image");
        if (logo != null) logo.gameObject.SetActive(false);

        Transform title = transform.Find("Title"); // Existing (hidden) title text that already uses the pixel title font
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
        ConfigureTitle(title, "<color=#C41E3A>ALIEN</color>\n<color=#D4AF37>SURVIVORS</color>", new Vector2(0f, 0f));
        shadow.SetSiblingIndex(title.GetSiblingIndex()); // Shadow draws behind the title

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
        stmp.characterSpacing = 4f;
        stmp.color = new Color(0.85f, 0.76f, 0.52f, 0.9f);
        stmp.text = "* A GOTHIC SURVIVAL NIGHTMARE *";
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

    public void StartGame() // Start the game. GK
    {
        SceneManager.LoadScene(firstLevelName); // Load the first level. GK
    }

    public void ContinueGame() // Load the run saved by "Save & Exit". GK
    {
        RunSaveData saved = RunSave.Load();
        if (saved == null) return;

        PlayerPrefs.SetInt("SelectedCharacterIndex", saved.characterIndex); // So the same hero is spawned
        PlayerPrefs.Save();
        RunSave.continueRequested = true;
        SceneManager.LoadScene(string.IsNullOrEmpty(firstLevelName) ? "Main" : firstLevelName);
    }

    public void QuitGame() // Quit the game. GK
    {
        Application.Quit(); // Quit the game. GK
        Debug.Log("I'm Quitting");
    }

    // Nút bấm phong cách Gothic Retro cổ kính với viền đá vàng son
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

        Sprite green = Resources.Load<Sprite>("UI/VS_Button_PowerUp_Green") ?? Resources.Load<Sprite>("UI/UI_Button_Confirm");
        Sprite gothicNavy = Resources.Load<Sprite>("UI/VS_Button_Start_Blue") ?? Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        Sprite red = Resources.Load<Sprite>("UI/VS_Button_Quit_Red") ?? Resources.Load<Sprite>("UI/UI_Button_Back");

        float[] y = canContinue ? new[] { -100f, -225f, -350f } : new[] { -10f, -145f, -270f };
        if (canContinue) StyleButton(cont, green, "CONTINUE\n<size=50%>" + RunSave.Describe(saved) + "</size>", y[0], new Color(0.9f, 1f, 0.9f));
        StyleButton(start, gothicNavy, canContinue ? "NEW GAME" : "START GAME", y[1], new Color(1f, 0.92f, 0.55f));
        StyleButton(quit, red, "QUIT", y[2], new Color(1f, 0.85f, 0.85f));
    }

    private void StyleButton(Transform t, Sprite sprite, string label, float y, Color textColor)
    {
        RectTransform rt = (RectTransform)t;
        rt.sizeDelta = new Vector2(490f, 105f);
        rt.anchoredPosition = new Vector2(0f, y);

        Image img = t.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_Text text = t.GetComponentInChildren<TMP_Text>(true);
        if (text == null) return;

        if (retroPixelFont != null)
        {
            text.font = retroPixelFont;
            if (retroPixelFont.material != null)
            {
                text.fontSharedMaterial = retroPixelFont.material;
            }
        }

        text.enableAutoSizing = false;
        text.fontSize = 46f;
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 4f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = textColor;
        text.text = label;
    }
}
