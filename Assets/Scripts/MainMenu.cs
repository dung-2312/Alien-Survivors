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
    private static TMP_FontAsset smoothFont;
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

    private static Dictionary<MetaStat, Sprite> cachedStatSprites = new Dictionary<MetaStat, Sprite>();
    private static Dictionary<int, Sprite> cachedCodexSprites = new Dictionary<int, Sprite>();

    public static TMP_FontAsset GetSmoothFont()
    {
        if (smoothFont == null)
        {
            smoothFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (smoothFont == null) smoothFont = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
            if (smoothFont == null) smoothFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");
            if (smoothFont == null)
            {
                var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                foreach (var f in fonts)
                {
                    if (f != null && (f.name.Contains("LiberationSans") || f.name.Contains("Roboto")))
                    {
                        smoothFont = f;
                        break;
                    }
                }
            }
        }
        return smoothFont;
    }

    public static Sprite GetGothicStatIconSprite(MetaStat stat)
    {
        if (cachedStatSprites.TryGetValue(stat, out Sprite s) && s != null) return s;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        int mid = size / 2;

        switch (stat)
        {
            case MetaStat.Might: // Two crossed silver swords with golden hilts
                for (int i = 8; i < 40; i++)
                {
                    p[i * size + i] = Color.white;
                    if (i > 8 && i < 39) { p[i * size + i - 1] = new Color(0.7f, 0.8f, 0.95f); p[(i - 1) * size + i] = new Color(0.7f, 0.8f, 0.95f); }
                    int j = 47 - i;
                    p[i * size + j] = Color.white;
                    if (i > 8 && i < 39) { p[i * size + j - 1] = new Color(0.7f, 0.8f, 0.95f); p[(i - 1) * size + j] = new Color(0.7f, 0.8f, 0.95f); }
                }
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        if (Mathf.Abs(dx) + Mathf.Abs(dy) <= 2)
                        {
                            p[(12 + dy) * size + (12 + dx)] = new Color(1f, 0.85f, 0.2f);
                            p[(12 + dy) * size + (35 + dx)] = new Color(1f, 0.85f, 0.2f);
                        }
                    }
                }
                p[mid * size + mid] = new Color(1f, 0.3f, 0.3f);
                break;

            case MetaStat.Health: // Radiant Red Heart
                for (int y = 8; y < 40; y++)
                {
                    for (int x = 8; x < 40; x++)
                    {
                        float nx = (x - 24f) / 14f;
                        float ny = (y - 22f) / 14f;
                        float a = nx * nx + ny * ny - 1f;
                        if (a * a * a - nx * nx * ny * ny * ny <= 0.05f)
                        {
                            if (nx < -0.3f && ny > 0.3f && nx > -0.7f && ny < 0.7f)
                                p[y * size + x] = Color.white;
                            else
                                p[y * size + x] = new Color(0.92f, 0.15f, 0.25f);
                        }
                        else if (a * a * a - nx * nx * ny * ny * ny <= 0.35f)
                        {
                            p[y * size + x] = new Color(0.4f, 0.05f, 0.1f);
                        }
                    }
                }
                break;

            case MetaStat.Recovery: // Emerald Potion Bottle
                for (int y = 8; y < 38; y++)
                {
                    for (int x = 12; x < 36; x++)
                    {
                        float distCenter = Mathf.Abs(x - 24f);
                        if (y <= 24)
                        {
                            float r = Mathf.Sqrt(distCenter * distCenter + (y - 20) * (y - 20));
                            if (r <= 12f)
                            {
                                if (r <= 10f)
                                {
                                    if (distCenter < 3f && y == 20) p[y * size + x] = Color.white;
                                    else p[y * size + x] = new Color(0.2f, 0.95f, 0.45f);
                                }
                                else p[y * size + x] = new Color(0.7f, 1f, 0.85f);
                            }
                        }
                        else if (y <= 33 && distCenter <= 4f)
                        {
                            p[y * size + x] = (distCenter <= 3f) ? new Color(0.6f, 0.95f, 0.8f) : new Color(0.2f, 0.4f, 0.3f);
                        }
                        else if (y > 33 && distCenter <= 5f)
                        {
                            p[y * size + x] = new Color(0.85f, 0.6f, 0.25f);
                        }
                    }
                }
                break;

            case MetaStat.Swiftness: // Winged Golden Boot
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        if (y >= 12 && y <= 16 && x >= 12 && x <= 36) p[y * size + x] = new Color(0.95f, 0.80f, 0.2f);
                        else if (y >= 16 && y <= 28 && x >= 22 && x <= 32) p[y * size + x] = new Color(1f, 0.90f, 0.3f);
                        else if (y >= 20 && y <= 35 && x >= 10 && x <= 26 && (x + y) % 3 == 0) p[y * size + x] = new Color(0.3f, 0.9f, 1f);
                    }
                }
                break;

            case MetaStat.Magnet: // Red & Silver Horseshoe Magnet
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 12; x < 36; x++)
                    {
                        float dx = Mathf.Abs(x - 24f);
                        if (dx >= 6f && dx <= 11f && y >= 14 && y <= 36)
                        {
                            if (y >= 28) p[y * size + x] = new Color(0.88f, 0.92f, 0.98f);
                            else p[y * size + x] = new Color(0.92f, 0.18f, 0.22f);
                        }
                        else if (y <= 14 && y >= 9 && dx <= 11f)
                        {
                            p[y * size + x] = new Color(0.92f, 0.18f, 0.22f);
                        }
                    }
                }
                p[33 * size + 24] = new Color(1f, 0.9f, 0.2f);
                p[31 * size + 24] = new Color(1f, 0.9f, 0.2f);
                break;

            case MetaStat.Greed: // Golden Coin with Star
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(24, 24));
                        if (dist <= 13f)
                        {
                            if (dist > 11f) p[y * size + x] = new Color(0.72f, 0.52f, 0.12f);
                            else if (dist <= 4f) p[y * size + x] = new Color(1f, 0.98f, 0.75f);
                            else p[y * size + x] = new Color(1f, 0.85f, 0.25f);
                        }
                    }
                }
                break;

            case MetaStat.Crit: // Radiant Lightning Bolt
                int[] lx = { 28, 26, 24, 22, 28, 26, 24, 22, 20, 18, 16 };
                int[] ly = { 36, 33, 30, 27, 27, 24, 21, 18, 15, 12,  9 };
                for (int k = 0; k < lx.Length; k++)
                {
                    for (int ox = -2; ox <= 2; ox++)
                    {
                        for (int oy = -2; oy <= 2; oy++)
                        {
                            int px = lx[k] + ox;
                            int py = ly[k] + oy;
                            if (px >= 0 && px < size && py >= 0 && py < size)
                            {
                                if (Mathf.Abs(ox) <= 1 && Mathf.Abs(oy) <= 1)
                                    p[py * size + px] = new Color(1f, 0.98f, 0.6f);
                                else
                                    p[py * size + px] = new Color(1f, 0.65f, 0.1f);
                            }
                        }
                    }
                }
                break;

            case MetaStat.Revival: // Golden Wings / Holy Halo
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dx = Mathf.Abs(x - 24f);
                        if ((dx <= 2f && y >= 14 && y <= 34) || (Mathf.Abs(y - 26f) <= 2f && dx <= 12f))
                        {
                            p[y * size + x] = new Color(1f, 0.92f, 0.45f);
                        }
                        float haloDist = Vector2.Distance(new Vector2(x, y), new Vector2(24, 32));
                        if (haloDist >= 4f && haloDist <= 6f)
                        {
                            p[y * size + x] = new Color(1f, 1f, 0.7f);
                        }
                    }
                }
                break;
        }

        tex.SetPixels(p);
        tex.Apply();
        Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        cachedStatSprites[stat] = spr;
        return spr;
    }

    public static Sprite GetCodexIconSprite(int index)
    {
        if (cachedCodexSprites.TryGetValue(index, out Sprite s) && s != null) return s;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        switch (index)
        {
            case 0: // Thousand Blades: Greatsword
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 20; x <= 28; x++)
                    {
                        if (y >= 14 && y <= 34 && Mathf.Abs(x - 24) <= 2) p[y * size + x] = Color.white;
                        else if (y == 14 && Mathf.Abs(x - 24) <= 5) p[y * size + x] = new Color(1f, 0.85f, 0.2f);
                        else if (y <= 13 && x == 24) p[y * size + x] = new Color(0.7f, 0.4f, 0.2f);
                    }
                }
                break;

            case 1: // Solar Flare Ring: Sunburst
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(24, 24));
                        if (dist <= 8f) p[y * size + x] = new Color(1f, 0.95f, 0.4f);
                        else if (dist <= 13f && ((x + y) % 4 == 0 || (x - y) % 4 == 0)) p[y * size + x] = new Color(1f, 0.55f, 0.1f);
                    }
                }
                break;

            case 2: // Spiral Cyclone Axes: Battleaxe
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 12; x < 36; x++)
                    {
                        if (x == 24 && y >= 10 && y <= 36) p[y * size + x] = new Color(0.7f, 0.5f, 0.3f);
                        else if (y >= 22 && y <= 34 && (Mathf.Abs(x - 24) >= 4 && Mathf.Abs(x - 24) <= 10)) p[y * size + x] = new Color(0.3f, 0.85f, 1f);
                    }
                }
                break;

            case 3: // Celestial Arrow: Bow & Arrow
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 24));
                        if (dist >= 11f && dist <= 13f && x >= 16) p[y * size + x] = new Color(1f, 0.85f, 0.25f);
                        if (y == 24 && x >= 16 && x <= 34) p[y * size + x] = new Color(0.4f, 1f, 0.8f);
                    }
                }
                break;

            case 4: // Sanctuary of Light: Divine Star
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float distCenter = Vector2.Distance(new Vector2(x, y), new Vector2(24, 24));
                        if (distCenter <= 4f) p[y * size + x] = Color.white;
                        else if (Mathf.Abs(x - 24) <= 2 && y >= 12 && y <= 36) p[y * size + x] = new Color(1f, 0.95f, 0.4f);
                        else if (Mathf.Abs(y - 24) <= 2 && x >= 12 && x <= 36) p[y * size + x] = new Color(1f, 0.95f, 0.4f);
                    }
                }
                break;

            case 5: // Survival Time: Stopwatch
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(24, 23));
                        if (dist <= 11f && dist >= 9f) p[y * size + x] = new Color(1f, 0.85f, 0.25f);
                        else if (y == 23 && x >= 24 && x <= 29) p[y * size + x] = Color.white;
                        else if (x == 24 && y >= 23 && y <= 29) p[y * size + x] = Color.white;
                        else if (y >= 33 && y <= 36 && Mathf.Abs(x - 24) <= 2) p[y * size + x] = new Color(0.8f, 0.8f, 0.8f);
                    }
                }
                break;

            case 6: // Treasury Gold: Gold Chest
                for (int y = 12; y <= 32; y++)
                {
                    for (int x = 12; x <= 36; x++)
                    {
                        if (y == 12 || y == 32 || x == 12 || x == 36) p[y * size + x] = new Color(1f, 0.85f, 0.2f);
                        else if (y == 22) p[y * size + x] = new Color(0.8f, 0.6f, 0.15f);
                        else p[y * size + x] = new Color(0.55f, 0.32f, 0.12f);
                    }
                }
                break;

            case 7: // Might
                return GetGothicStatIconSprite(MetaStat.Might);

            case 8: // Revival
                return GetGothicStatIconSprite(MetaStat.Revival);

            default: // Dragon / Boss
                for (int y = 10; y < 38; y++)
                {
                    for (int x = 10; x < 38; x++)
                    {
                        float dx = Mathf.Abs(x - 24);
                        if (y >= 14 && y <= 28 && dx <= (y - 12)) p[y * size + x] = new Color(0.9f, 0.2f, 0.2f);
                        if (y == 22 && (x == 20 || x == 28)) p[y * size + x] = new Color(1f, 0.95f, 0.2f);
                        if (y >= 26 && dx >= 6 && dx <= 12) p[y * size + x] = new Color(0.3f, 0.1f, 0.1f);
                    }
                }
                break;
        }

        tex.SetPixels(p);
        tex.Apply();
        Sprite spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        cachedCodexSprites[index] = spr;
        return spr;
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
        GetSmoothFont();
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
        stmp.color = new Color(0.95f, 0.86f, 0.58f, 0.95f);
        stmp.text = "A GOTHIC SURVIVAL NIGHTMARE";
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
        if (canContinue) StyleButton(cont, emeraldButton, "CONTINUE RUN\n<size=52%>" + RunSave.Describe(saved) + "</size>", y[0], new Color(0.95f, 1f, 0.92f));
        StyleButton(start, goldButton, canContinue ? "NEW GAME" : "START GAME", y[1], new Color(1f, 0.94f, 0.58f));
        StyleButton(quit, crimsonButton, "QUIT GAME", y[2], new Color(1f, 0.88f, 0.88f));

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
        StyleAuxButton(howToPlay, sapphireButton, "HOW TO PLAY", -135f, auxY, new Color(0.88f, 0.95f, 1f));
        StyleAuxButton(settings, obsidianButton, "SETTINGS", 135f, auxY, new Color(1f, 0.95f, 0.85f));

        StyleAuxButton(powerUp, amberButton, "POWER UP", -135f, auxY - 68f, new Color(1f, 0.92f, 0.50f));
        StyleAuxButton(codex, amethystButton, "CODEX & FEATS", 135f, auxY - 68f, new Color(0.95f, 0.85f, 1f));

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
        oimg.color = new Color(0f, 0f, 0f, 0.85f);

        GameObject win = new GameObject("HowToPlayWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        win.transform.SetParent(overlay.transform, false);
        RectTransform wrt = win.GetComponent<RectTransform>();
        wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.sizeDelta = new Vector2(980f, 680f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "HOW TO PLAY & COMBAT GUIDE", new Vector2(0f, 290f), 30f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "BÍ KÍP ĐIỀU KHIỂN & CƠ CHẾ SINH TỒN ĐÊM ÁC MỘNG", new Vector2(0f, 258f), 16f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal, true);
        CreateGothicDividerLine(win.transform, new Vector2(0f, 236f), 900f);

        // Cột Trái: BỘ PHÍM ĐIỀU KHIỂN & SINH TỒN
        CreateSectionHeader(win.transform, new Vector2(-232f, 212f), "BỘ PHÍM ĐIỀU KHIỂN & SINH TỒN");

        var controls = new (string key, string title, string desc)[]
        {
            ("[W, A, S, D] / [MŨI TÊN]", "DI CHUYỂN", "Di chuyển nhân vật né tránh đàn quái vật bao vây, nhặt ngọc kinh nghiệm và giữ cự ly sinh tồn."),
            ("[PHÍM SPACE]", "LƯỚT NÉ ĐÒN (DASH)", "Lướt nhanh tức thì kèm thời gian Bất Tử (i-Frames), né đòn đánh chí tử và thoát khỏi vòng vây."),
            ("[PHÍM E] / [CHUỘT PHẢI]", "CHIÊU CUỐI (ULTIMATE)", "Kích hoạt khi thanh Nộ Khí tích tụ đầy 100%. Quét sạch kẻ thù diện rộng và tạo khoảng trống an toàn!"),
            ("[PHÍM ESC]", "TẠM DỪNG & LƯU GAME", "Tạm dừng trận đấu, kiểm tra thông số vũ khí và lưu lại tiến trình ván đấu hiện tại để chơi tiếp sau."),
            ("[ĐÀN TẾ & VẬT CẢN]", "TẾ ĐÀN & KHO BÁU BÍ ẨN", "Vào Tế đàn nhận chúc phúc (Đóng băng, Mưa vàng, Cuồng nộ). Đánh vỡ bia mộ, bình gốm nhặt Vàng & Thịt hồi HP.")
        };

        float[] cardYs = new float[] { 152f, 68f, -16f, -100f, -184f };
        for (int i = 0; i < controls.Length; i++)
        {
            CreateGuideCard(win.transform, new Vector2(-232f, cardYs[i]), controls[i].key, controls[i].title, controls[i].desc, new Color(0.35f, 0.85f, 1f));
        }

        // Cột Phải: TIẾN HÓA VŨ KHÍ & MẸO CHIẾN ĐẤU
        CreateSectionHeader(win.transform, new Vector2(232f, 212f), "TIẾN HÓA VŨ KHÍ & MẸO CHIẾN ĐẤU");

        var evolutions = new (string key, string title, string desc)[]
        {
            ("SWORD + MIGHT", "VẠN KIẾM QUY TÔNG", "Bắn bão kiếm xoay tròn 360 độ xé nát toàn bộ kẻ thù trong tầm mắt. Sát thương diện rộng cực lớn."),
            ("FIREBALL + MAGNET", "NHẬT LUÂN HÀO QUANG", "Quả cầu lửa hợp nhất thành vành đai mặt trời thiêu rụi kẻ địch tiếp cận liên tục không ngừng nghỉ."),
            ("AXES + SWIFTNESS", "CUỒNG PHONG PHI PHỦ", "Bão rìu cuồng nộ xoay liên hồi không ngừng, tạo tường chắn đẩy lùi và băm nhỏ đàn quái vật áp sát."),
            ("ZONE + VITALITY", "THÁNH ĐỊA TRẤN HỒN", "Hào quang thần thánh mở rộng tối đa, liên tục hồi sinh lực cho chủ nhân và giáng sấm sét hủy diệt."),
            ("[PHÍM F1] / [PHÍM ~]", "BẢNG BÁO CÁO & DEBUG", "Mở menu thử nghiệm tính năng nhanh: Triệu hồi Boss, Bất tử God Mode, Thêm Vàng và Nâng cấp vũ khí.")
        };

        for (int i = 0; i < evolutions.Length; i++)
        {
            CreateGuideCard(win.transform, new Vector2(232f, cardYs[i]), evolutions[i].key, evolutions[i].title, evolutions[i].desc, new Color(1f, 0.75f, 0.2f));
        }

        CreateModalCloseButton(win.transform, new Vector2(0f, -288f), "CLOSE (ĐÓNG)");

        currentModal = overlay;
    }

    private void CreateGuideCard(Transform parent, Vector2 pos, string keyBadge, string title, string desc, Color badgeColor)
    {
        GameObject card = new GameObject("Card_" + title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform crt = card.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = pos;
        crt.sizeDelta = new Vector2(445f, 78f);

        Image cimg = card.GetComponent<Image>();
        cimg.sprite = LevelUpSellectionButton.GetGothicCardSprite(false);
        cimg.type = Image.Type.Sliced;

        TMP_FontAsset font = GetSmoothFont();

        // Key Badge Pill (Góc trên trái)
        GameObject badgeGO = new GameObject("Badge", typeof(RectTransform), typeof(TextMeshProUGUI));
        badgeGO.transform.SetParent(card.transform, false);
        RectTransform brt = badgeGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 1f);
        brt.anchorMax = new Vector2(0f, 1f);
        brt.pivot = new Vector2(0f, 1f);
        brt.anchoredPosition = new Vector2(14f, -8f);
        brt.sizeDelta = new Vector2(210f, 22f);
        TextMeshProUGUI btmp = badgeGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { btmp.font = font; if (font.material != null) btmp.fontSharedMaterial = font.material; }
        btmp.fontSize = 13.5f;
        btmp.fontStyle = FontStyles.Bold;
        btmp.color = badgeColor;
        btmp.alignment = TextAlignmentOptions.MidlineLeft;
        btmp.text = keyBadge;

        // Title (Góc trên phải)
        GameObject titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(card.transform, false);
        RectTransform trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(1f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(1f, 1f);
        trt.anchoredPosition = new Vector2(-14f, -8f);
        trt.sizeDelta = new Vector2(200f, 22f);
        TextMeshProUGUI ttmp = titleGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { ttmp.font = font; if (font.material != null) ttmp.fontSharedMaterial = font.material; }
        ttmp.fontSize = 14f;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.color = new Color(1f, 0.90f, 0.55f);
        ttmp.alignment = TextAlignmentOptions.MidlineRight;
        ttmp.text = title;

        // Description Body (Hàng dưới độc lập hoàn toàn, không đè lên hàng trên)
        GameObject descGO = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGO.transform.SetParent(card.transform, false);
        RectTransform drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0f, 0f);
        drt.anchorMax = new Vector2(1f, 0f);
        drt.pivot = new Vector2(0f, 0f);
        drt.anchoredPosition = new Vector2(14f, 8f);
        drt.sizeDelta = new Vector2(-28f, 38f);
        TextMeshProUGUI dtmp = descGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { dtmp.font = font; if (font.material != null) dtmp.fontSharedMaterial = font.material; }
        dtmp.fontSize = 12.5f;
        dtmp.lineSpacing = 1.5f;
        dtmp.color = new Color(0.85f, 0.88f, 0.94f, 0.95f);
        dtmp.alignment = TextAlignmentOptions.TopLeft;
        dtmp.textWrappingMode = TextWrappingModes.Normal;
        dtmp.text = desc;
    }

    private void CreateSectionHeader(Transform parent, Vector2 pos, string title)
    {
        GameObject go = new GameObject("Header_" + title, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(445f, 28f);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = GetSmoothFont();
        if (font != null) { tmp.font = font; if (font.material != null) tmp.fontSharedMaterial = font.material; }
        tmp.text = title;
        tmp.fontSize = 16.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.88f, 0.35f);
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private void CreateGothicDividerLine(Transform parent, Vector2 pos, float width)
    {
        GameObject line = new GameObject("DividerLine", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        line.transform.SetParent(parent, false);
        RectTransform rt = line.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(width, 2f);
        Image img = line.GetComponent<Image>();
        img.color = new Color(0.85f, 0.70f, 0.25f, 0.45f);
        img.raycastTarget = false;
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

        CreateModalText(win.transform, "AUDIO & DISPLAY SETTINGS", new Vector2(0f, 215f), 30f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "TÙY CHỈNH ÂM THANH & CHẾ ĐỘ HIỂN THỊ", new Vector2(0f, 182f), 16f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal, true);

        CreateVolumeRow(win.transform, "MASTER VOLUME", 100f, () => GameSettings.MasterVolume, v => GameSettings.MasterVolume = v);
        CreateVolumeRow(win.transform, "BGM MUSIC", 30f, () => GameSettings.BGMVolume, v => GameSettings.BGMVolume = v);
        CreateVolumeRow(win.transform, "SFX SOUNDS", -40f, () => GameSettings.SFXVolume, v => GameSettings.SFXVolume = v);

        CreateDisplayToggleRow(win.transform, -115f);

        CreateModalCloseButton(win.transform, new Vector2(0f, -205f), "SAVE & CLOSE");

        currentModal = overlay;
    }

    private TextMeshProUGUI CreateModalText(Transform parent, string text, Vector2 pos, float fontSize, Color color, FontStyles style, bool useSmooth = false)
    {
        GameObject go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(900f, 40f);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset fontToUse = (useSmooth && smoothFont != null) ? smoothFont : (retroPixelFont ?? smoothFont);
        if (fontToUse != null)
        {
            tmp.font = fontToUse;
            if (fontToUse.material != null) tmp.fontSharedMaterial = fontToUse.material;
        }
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
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
        TMP_FontAsset sFont = GetSmoothFont() ?? retroPixelFont;
        if (sFont != null) { ltmp.font = sFont; if (sFont.material != null) ltmp.fontSharedMaterial = sFont.material; }
        ltmp.text = title;
        ltmp.fontSize = 20f;
        ltmp.fontStyle = FontStyles.Bold;
        ltmp.color = new Color(0.95f, 0.88f, 0.65f);
        ltmp.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject valGO = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
        valGO.transform.SetParent(row.transform, false);
        RectTransform vrt = valGO.GetComponent<RectTransform>();
        vrt.anchorMin = vrt.anchorMax = vrt.pivot = new Vector2(0.5f, 0.5f);
        vrt.anchoredPosition = new Vector2(100f, 0f);
        vrt.sizeDelta = new Vector2(100f, 40f);
        TextMeshProUGUI vtmp = valGO.GetComponent<TextMeshProUGUI>();
        if (sFont != null) { vtmp.font = sFont; if (sFont.material != null) vtmp.fontSharedMaterial = sFont.material; }
        vtmp.fontSize = 20f;
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
        TMP_FontAsset sFont = GetSmoothFont() ?? retroPixelFont;
        if (sFont != null) { tmp.font = sFont; if (sFont.material != null) tmp.fontSharedMaterial = sFont.material; }
        tmp.text = label;
        tmp.fontSize = 22f;
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
        TMP_FontAsset sFont = GetSmoothFont() ?? retroPixelFont;
        if (sFont != null) { tmp.font = sFont; if (sFont.material != null) tmp.fontSharedMaterial = sFont.material; }
        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(0.95f, 0.95f, 1f);
        tmp.alignment = TextAlignmentOptions.Center;

        void UpdateLabel()
        {
            tmp.text = Screen.fullScreen ? "DISPLAY MODE: FULLSCREEN" : "DISPLAY MODE: WINDOWED";
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
        TMP_FontAsset font = GetSmoothFont();
        if (font != null) { tmp.font = font; if (font.material != null) tmp.fontSharedMaterial = font.material; }
        tmp.text = label;
        tmp.fontSize = 20f;
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
        TMP_FontAsset font = GetSmoothFont();
        if (font != null) { tmp.font = font; if (font.material != null) tmp.fontSharedMaterial = font.material; }
        tmp.text = label;
        tmp.fontSize = 19f;
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
        rt.sizeDelta = new Vector2(240f, 52f);

        Image img = pill.GetComponent<Image>();
        img.sprite = CreateRoyalGothicButton(
            new Color(0.18f, 0.12f, 0.04f), new Color(0.36f, 0.24f, 0.08f),
            new Color(1f, 0.82f, 0.25f), new Color(1f, 0.95f, 0.60f),
            new Color(1f, 0.70f, 0.10f)
        );
        img.type = Image.Type.Sliced;

        Button b = pill.GetComponent<Button>();
        b.onClick.AddListener(OpenPowerUpModal);

        GameObject icoGO = new GameObject("CoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icoGO.transform.SetParent(pill.transform, false);
        RectTransform icort = icoGO.GetComponent<RectTransform>();
        icort.anchorMin = icort.anchorMax = icort.pivot = new Vector2(0f, 0.5f);
        icort.anchoredPosition = new Vector2(16f, 0f);
        icort.sizeDelta = new Vector2(30f, 30f);
        Image icoImg = icoGO.GetComponent<Image>();
        icoImg.sprite = GetGothicStatIconSprite(MetaStat.Greed);
        icoImg.preserveAspect = true;
        icoImg.raycastTarget = false;

        GameObject txt = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txt.transform.SetParent(pill.transform, false);
        RectTransform trt = txt.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(50f, 0f);
        trt.offsetMax = new Vector2(-12f, 0f);
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset font = GetSmoothFont();
        if (font != null) { tmp.font = font; if (font.material != null) tmp.fontSharedMaterial = font.material; }
        tmp.text = $"{coins:N0} VÀNG";
        tmp.fontSize = 20f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = new Color(1f, 0.92f, 0.45f);
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

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
        wrt.sizeDelta = new Vector2(980f, 700f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "POWER-UP SANCTUARY", new Vector2(0f, 298f), 30f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "THÁNH ĐIỆN NÂNG CẤP VĨNH VIỄN CHO TẤT CẢ TƯỚNG", new Vector2(0f, 266f), 16f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal, true);

        // Treasury Balance Container Pill
        GameObject treasuryPill = new GameObject("TreasuryPillContainer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        treasuryPill.transform.SetParent(win.transform, false);
        RectTransform tprt = treasuryPill.GetComponent<RectTransform>();
        tprt.anchorMin = tprt.anchorMax = tprt.pivot = new Vector2(0.5f, 0.5f);
        tprt.anchoredPosition = new Vector2(0f, 226f);
        tprt.sizeDelta = new Vector2(480f, 40f);
        Image tpImg = treasuryPill.GetComponent<Image>();
        tpImg.sprite = CreateRoyalGothicButton(
            new Color(0.18f, 0.12f, 0.04f), new Color(0.36f, 0.24f, 0.08f),
            new Color(1f, 0.82f, 0.25f), new Color(1f, 0.95f, 0.60f),
            new Color(1f, 0.70f, 0.10f)
        );
        tpImg.type = Image.Type.Sliced;

        GameObject tpIco = new GameObject("CoinIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tpIco.transform.SetParent(treasuryPill.transform, false);
        RectTransform tpIcoRt = tpIco.GetComponent<RectTransform>();
        tpIcoRt.anchorMin = tpIcoRt.anchorMax = tpIcoRt.pivot = new Vector2(0f, 0.5f);
        tpIcoRt.anchoredPosition = new Vector2(18f, 0f);
        tpIcoRt.sizeDelta = new Vector2(24f, 24f);
        Image tpIcoImg = tpIco.GetComponent<Image>();
        tpIcoImg.sprite = GetGothicStatIconSprite(MetaStat.Greed);
        tpIcoImg.preserveAspect = true;
        tpIcoImg.raycastTarget = false;

        GameObject treasuryGO = new GameObject("TreasuryText", typeof(RectTransform), typeof(TextMeshProUGUI));
        treasuryGO.transform.SetParent(treasuryPill.transform, false);
        RectTransform trt = treasuryGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(46f, 0f);
        trt.offsetMax = new Vector2(-16f, 0f);
        TextMeshProUGUI treasuryTmp = treasuryGO.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset smooth = GetSmoothFont();
        if (smooth != null) { treasuryTmp.font = smooth; if (smooth.material != null) treasuryTmp.fontSharedMaterial = smooth.material; }
        treasuryTmp.fontSize = 16f;
        treasuryTmp.fontStyle = FontStyles.Bold;
        treasuryTmp.color = new Color(1f, 0.92f, 0.45f);
        treasuryTmp.alignment = TextAlignmentOptions.MidlineLeft;

        CreateGothicDividerLine(win.transform, new Vector2(0f, 200f), 900f);

        GameObject gridContainer = new GameObject("StatGrid", typeof(RectTransform));
        gridContainer.transform.SetParent(win.transform, false);
        RectTransform grt = gridContainer.GetComponent<RectTransform>();
        grt.anchorMin = grt.anchorMax = grt.pivot = new Vector2(0.5f, 0.5f);
        grt.sizeDelta = new Vector2(930f, 365f);
        grt.anchoredPosition = new Vector2(0f, 6f);

        void RefreshShopUI()
        {
            int currentCoins = PlayerPrefs.GetInt("SavedCoins", 0);
            treasuryTmp.text = $"KHO BÁU HIỆN CÓ: <color=#FFE040><b>{currentCoins:N0}</b></color> VÀNG";

            for (int i = gridContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(gridContainer.transform.GetChild(i).gameObject);
            }

            MetaStat[] stats = (MetaStat[])System.Enum.GetValues(typeof(MetaStat));
            float[] colX = new float[] { -232f, 232f };
            float[] rowY = new float[] { 126f, 42f, -42f, -126f };

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
        CreateCustomModalButton(win.transform, new Vector2(-160f, -288f), new Vector2(250f, 50f), "REFUND ALL (TẨY ĐIỂM)", obsidianBtn, () => {
            MetaProgression.RefundAll();
            ProceduralSFX.PlayLevelUp();
            RefreshShopUI();
        });

        CreateModalCloseButton(win.transform, new Vector2(160f, -288f), "CLOSE (ĐÓNG)");

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
        crt.sizeDelta = new Vector2(445f, 76f);

        Image cimg = card.GetComponent<Image>();
        cimg.sprite = LevelUpSellectionButton.GetGothicCardSprite(canAfford);
        cimg.type = Image.Type.Sliced;

        TMP_FontAsset font = GetSmoothFont();

        // 1. Icon Slot bên trái với ảnh pixel sắc nét
        GameObject iconSlot = new GameObject("IconSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconSlot.transform.SetParent(card.transform, false);
        RectTransform irt = iconSlot.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0f, 0.5f);
        irt.anchoredPosition = new Vector2(14f, 0f);
        irt.sizeDelta = new Vector2(48f, 48f);
        Image isImg = iconSlot.GetComponent<Image>();
        isImg.sprite = LevelUpSellectionButton.GetGothicIconSlotSprite();
        isImg.type = Image.Type.Sliced;

        GameObject icoGO = new GameObject("StatIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icoGO.transform.SetParent(iconSlot.transform, false);
        RectTransform icoRt = icoGO.GetComponent<RectTransform>();
        icoRt.anchorMin = Vector2.zero;
        icoRt.anchorMax = Vector2.one;
        icoRt.offsetMin = new Vector2(6f, 6f);
        icoRt.offsetMax = new Vector2(-6f, -6f);
        Image icoImg = icoGO.GetComponent<Image>();
        icoImg.sprite = GetGothicStatIconSprite(stat);
        icoImg.preserveAspect = true;

        // 2. Khu vực thông tin ở giữa (Stat Name + Diamond Pips + Bonus text)
        GameObject infoGO = new GameObject("InfoContainer", typeof(RectTransform));
        infoGO.transform.SetParent(card.transform, false);
        RectTransform infort = infoGO.GetComponent<RectTransform>();
        infort.anchorMin = new Vector2(0f, 0f);
        infort.anchorMax = new Vector2(1f, 1f);
        infort.pivot = new Vector2(0f, 0.5f);
        infort.offsetMin = new Vector2(70f, 6f);
        infort.offsetMax = new Vector2(-114f, -6f);

        // Hàng trên: Display Name (height 20px)
        GameObject titleGO = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(infoGO.transform, false);
        RectTransform trt = titleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0f, 1f);
        trt.anchoredPosition = new Vector2(0f, 0f);
        trt.sizeDelta = new Vector2(0f, 20f);
        TextMeshProUGUI ttmp = titleGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { ttmp.font = font; if (font.material != null) ttmp.fontSharedMaterial = font.material; }
        ttmp.fontSize = 14f;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.color = new Color(1f, 0.90f, 0.55f);
        ttmp.alignment = TextAlignmentOptions.MidlineLeft;
        ttmp.text = info.displayName;

        // Hàng giữa: Visual Diamond Pips (height 16px)
        GameObject pipsGO = new GameObject("DiamondPips", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        pipsGO.transform.SetParent(infoGO.transform, false);
        RectTransform prt = pipsGO.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0f, 0.5f);
        prt.anchorMax = new Vector2(1f, 0.5f);
        prt.pivot = new Vector2(0f, 0.5f);
        prt.anchoredPosition = new Vector2(0f, -2f);
        prt.sizeDelta = new Vector2(0f, 16f);

        HorizontalLayoutGroup hlg = pipsGO.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.spacing = 4f;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        for (int p = 0; p < info.maxLevel; p++)
        {
            GameObject pip = new GameObject($"Pip_{p}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            pip.transform.SetParent(pipsGO.transform, false);
            RectTransform pipRt = pip.GetComponent<RectTransform>();
            pipRt.sizeDelta = new Vector2(16f, 16f);
            Image pImg = pip.GetComponent<Image>();
            bool filled = p < lvl;
            pImg.sprite = LevelUpSellectionButton.GetPipDiamondSprite(filled);
            pImg.color = filled ? Color.white : new Color(0.6f, 0.6f, 0.75f, 0.7f);
            pImg.raycastTarget = false;
        }

        // Hàng dưới: Description / Bonus (height 18px)
        GameObject descGO = new GameObject("DescText", typeof(RectTransform), typeof(TextMeshProUGUI));
        descGO.transform.SetParent(infoGO.transform, false);
        RectTransform drt = descGO.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0f, 0f);
        drt.anchorMax = new Vector2(1f, 0f);
        drt.pivot = new Vector2(0f, 0f);
        drt.anchoredPosition = new Vector2(0f, 0f);
        drt.sizeDelta = new Vector2(0f, 18f);
        TextMeshProUGUI dtmp = descGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { dtmp.font = font; if (font.material != null) dtmp.fontSharedMaterial = font.material; }
        dtmp.fontSize = 12f;
        dtmp.alignment = TextAlignmentOptions.MidlineLeft;
        dtmp.text = isMax ? $"<color=#55FF99>MAX LEVEL ({lvl}/{info.maxLevel})</color>" : $"<color=#99E5FF>{info.bonusPerLevel}</color>";

        // 3. Nút mua bên phải (Buy Button)
        GameObject btnGO = new GameObject("BuyBtn", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(card.transform, false);
        RectTransform brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(1f, 0.5f);
        brt.anchorMax = new Vector2(1f, 0.5f);
        brt.pivot = new Vector2(1f, 0.5f);
        brt.anchoredPosition = new Vector2(-12f, 0f);
        brt.sizeDelta = new Vector2(96f, 44f);

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
        if (font != null) { btmp.font = font; if (font.material != null) btmp.fontSharedMaterial = font.material; }
        btmp.fontSize = 15f;
        btmp.fontStyle = FontStyles.Bold;
        btmp.alignment = TextAlignmentOptions.Center;
        btmp.color = isMax ? new Color(0.7f, 0.7f, 0.7f) : (canAfford ? new Color(1f, 0.95f, 0.6f) : new Color(0.85f, 0.5f, 0.5f));
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
        wrt.sizeDelta = new Vector2(980f, 680f);
        wrt.anchoredPosition = Vector2.zero;
        Image wimg = win.GetComponent<Image>();
        wimg.sprite = UIController.GetGothicWindowSprite();
        wimg.type = Image.Type.Sliced;

        CreateModalText(win.transform, "WEAPON EVOLUTION CODEX & FEATS", new Vector2(0f, 290f), 30f, new Color(1f, 0.88f, 0.25f), FontStyles.Bold);
        CreateModalText(win.transform, "KINH ĐIỂN TIẾN HÓA VŨ KHÍ & CHIẾN TÍCH SINH TỒN", new Vector2(0f, 258f), 16f, new Color(0.92f, 0.82f, 0.55f), FontStyles.Normal, true);
        CreateGothicDividerLine(win.transform, new Vector2(0f, 236f), 900f);

        // Cột Trái: CÔNG THỨC TIẾN HÓA VŨ KHÍ
        CreateSectionHeader(win.transform, new Vector2(-232f, 212f), "HUYỀN THOẠI TIẾN HÓA VŨ KHÍ");

        float[] cardYs = new float[] { 152f, 68f, -16f, -100f, -184f };
        var recipes = new (int iconIdx, string title, string formula, string desc)[]
        {
            (0, "Thousand Blades", "<color=#66FFBB>Sword</color> + <color=#FFAA55>Might</color>", "Bắn bão kiếm xoay 360° xé nát quái vật trong tầm mắt. Sát thương cực lớn."),
            (1, "Solar Flare Ring", "<color=#66FFBB>Fireball</color> + <color=#FFAA55>Magnet</color>", "Quả cầu lửa hợp nhất thành vành đai mặt trời thiêu rụi toàn diện xung quanh."),
            (2, "Spiral Cyclone Axes", "<color=#66FFBB>Axes</color> + <color=#FFAA55>Swiftness</color>", "Bão rìu cuồng nộ xoay liên hồi không ngừng, tạo tường chắn đẩy lùi đàn quái vật."),
            (3, "Celestial Arrow", "<color=#66FFBB>Forest Bow</color> + <color=#FFAA55>Critical</color>", "Tên ánh sáng xuyên thấu toàn màn hình gây sát thương chí mạng cực đại."),
            (4, "Sanctuary of Light", "<color=#66FFBB>Bright Zone</color> + <color=#FFAA55>Vitality</color>", "Vòng hào quang cực đại hồi phục sinh lực và giáng sấm sét trừng phạt.")
        };

        for (int i = 0; i < recipes.Length; i++)
        {
            CreateCodexCard(win.transform, new Vector2(-232f, cardYs[i]), recipes[i].iconIdx, recipes[i].title, recipes[i].formula, recipes[i].desc);
        }

        // Cột Phải: CHIẾN TÍCH & KỶ LỤC CÁ NHÂN
        CreateSectionHeader(win.transform, new Vector2(232f, 212f), "CHIẾN TÍCH & KỶ LỤC CÁ NHÂN");

        float bestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f);
        int minutes = Mathf.FloorToInt(bestTime / 60f);
        int seconds = Mathf.FloorToInt(bestTime % 60f);
        string timeStr = bestTime > 0 ? $"{minutes:00}:{seconds:00}" : "--:--";
        int coins = PlayerPrefs.GetInt("SavedCoins", 0);
        int mightLvl = MetaProgression.GetLevel(MetaStat.Might);
        int revivalLvl = MetaProgression.GetLevel(MetaStat.Revival);

        var feats = new (int iconIdx, string title, string status, string desc)[]
        {
            (5, "Thời Gian Sống Sót", $"<color=#FFE040><b>{timeStr}</b></color>", bestTime >= 900f ? "<color=#00FF99>[ĐÃ ĐẠT: SURVIVOR MASTER (15 PHÚT)]</color>" : "<color=#8899AA>Mục tiêu: Sống sót 15:00 để đối đầu Hỏa Long</color>"),
            (6, "Kho Báu Tích Lũy", $"<color=#FFE040><b>{coins} Vàng</b></color>", coins >= 1000 ? "<color=#00FF99>[ĐÃ ĐẠT: GOLDEN HOARDER (1,000+ VÀNG)]</color>" : "<color=#8899AA>Mục tiêu: Tích lũy 1,000 Vàng trong Thánh Điện</color>"),
            (7, "Sức Mạnh Sát Thần", $"<color=#FFE040><b>Might Cấp {mightLvl}/5</b></color>", mightLvl >= 5 ? "<color=#00FF99>[ĐÃ ĐẠT: GOD OF WAR (SÁT THƯƠNG TỐI ĐA)]</color>" : "<color=#8899AA>Nâng cấp Might cấp 5 trong Thánh Điện (+25% Dmg)</color>"),
            (8, "Hồi Sinh Phượng Hoàng", revivalLvl > 0 ? "<color=#00FF99><b>ĐÃ MỞ KHÓA</b></color>" : "<color=#FF6666><b>CHƯA MỞ KHÓA</b></color>", revivalLvl > 0 ? "<color=#00FF99>Tự động hồi sinh với 50% máu khi gục ngã</color>" : "<color=#8899AA>Mở khóa trong Thánh Điện để nhận 1 mạng hồi sinh</color>"),
            (9, "Đại Chiến Hỏa Long", "<color=#FF5544><b>PYROCLAST</b></color>", "<color=#FFAA44>Sống sót đến phút 15:00 để triệu hồi và tiêu diệt Hỏa Long!</color>")
        };

        for (int i = 0; i < feats.Length; i++)
        {
            CreateCodexCard(win.transform, new Vector2(232f, cardYs[i]), feats[i].iconIdx, feats[i].title, feats[i].status, feats[i].desc);
        }

        CreateModalCloseButton(win.transform, new Vector2(0f, -288f), "CLOSE (ĐÓNG)");

        currentModal = overlay;
    }

    private void CreateCodexCard(Transform parent, Vector2 pos, int iconIndex, string title, string formulaOrBadge, string desc)
    {
        GameObject card = new GameObject("Card_" + title, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.transform.SetParent(parent, false);
        RectTransform crt = card.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.anchoredPosition = pos;
        crt.sizeDelta = new Vector2(445f, 76f);

        Image cimg = card.GetComponent<Image>();
        cimg.sprite = LevelUpSellectionButton.GetGothicCardSprite(false);
        cimg.type = Image.Type.Sliced;

        TMP_FontAsset font = GetSmoothFont();

        // 1. Icon slot ở bên trái với icon pixel sắc nét
        GameObject iconSlot = new GameObject("IconSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconSlot.transform.SetParent(card.transform, false);
        RectTransform irt = iconSlot.GetComponent<RectTransform>();
        irt.anchorMin = irt.anchorMax = irt.pivot = new Vector2(0f, 0.5f);
        irt.anchoredPosition = new Vector2(14f, 0f);
        irt.sizeDelta = new Vector2(48f, 48f);
        Image isImg = iconSlot.GetComponent<Image>();
        isImg.sprite = LevelUpSellectionButton.GetGothicIconSlotSprite();
        isImg.type = Image.Type.Sliced;

        GameObject icoGO = new GameObject("CodexIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        icoGO.transform.SetParent(iconSlot.transform, false);
        RectTransform icoRt = icoGO.GetComponent<RectTransform>();
        icoRt.anchorMin = Vector2.zero;
        icoRt.anchorMax = Vector2.one;
        icoRt.offsetMin = new Vector2(6f, 6f);
        icoRt.offsetMax = new Vector2(-6f, -6f);
        Image icoImg = icoGO.GetComponent<Image>();
        icoImg.sprite = GetCodexIconSprite(iconIndex);
        icoImg.preserveAspect = true;

        // 2. Content bên phải icon: 2 dòng sắc nét độc lập, tuyệt đối không đè nhau
        GameObject contentGO = new GameObject("Content", typeof(RectTransform));
        contentGO.transform.SetParent(card.transform, false);
        RectTransform cntRt = contentGO.GetComponent<RectTransform>();
        cntRt.anchorMin = Vector2.zero;
        cntRt.anchorMax = Vector2.one;
        cntRt.offsetMin = new Vector2(70f, 6f);
        cntRt.offsetMax = new Vector2(-14f, -6f);

        // Hàng trên: Title + Formula/Badge (height 22px)
        GameObject topGO = new GameObject("TopRow", typeof(RectTransform), typeof(TextMeshProUGUI));
        topGO.transform.SetParent(contentGO.transform, false);
        RectTransform trt = topGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 1f);
        trt.anchorMax = new Vector2(1f, 1f);
        trt.pivot = new Vector2(0f, 1f);
        trt.anchoredPosition = new Vector2(0f, 0f);
        trt.sizeDelta = new Vector2(0f, 22f);
        TextMeshProUGUI ttmp = topGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { ttmp.font = font; if (font.material != null) ttmp.fontSharedMaterial = font.material; }
        ttmp.fontSize = 13.5f;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.color = new Color(1f, 0.90f, 0.55f);
        ttmp.alignment = TextAlignmentOptions.MidlineLeft;
        ttmp.text = $"<b>{title}</b>  <color=#888888>|</color>  {formulaOrBadge}";

        // Hàng dưới: Description (height 36px, zero overlap)
        GameObject btmGO = new GameObject("BottomRow", typeof(RectTransform), typeof(TextMeshProUGUI));
        btmGO.transform.SetParent(contentGO.transform, false);
        RectTransform brt = btmGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0f);
        brt.anchorMax = new Vector2(1f, 0f);
        brt.pivot = new Vector2(0f, 0f);
        brt.anchoredPosition = new Vector2(0f, 0f);
        brt.sizeDelta = new Vector2(0f, 36f);
        TextMeshProUGUI btmp = btmGO.GetComponent<TextMeshProUGUI>();
        if (font != null) { btmp.font = font; if (font.material != null) btmp.fontSharedMaterial = font.material; }
        btmp.fontSize = 12f;
        btmp.lineSpacing = 1.5f;
        btmp.color = new Color(0.85f, 0.88f, 0.94f, 0.95f);
        btmp.alignment = TextAlignmentOptions.TopLeft;
        btmp.textWrappingMode = TextWrappingModes.Normal;
        btmp.text = desc;
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
