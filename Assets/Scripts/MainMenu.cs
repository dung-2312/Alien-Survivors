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

    void Start()
    {
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

        foreach (string buttonName in new[] { "Start Game", "Quit Game", "Continue Button" })
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

        float[] y = canContinue ? new[] { -100f, -225f, -350f } : new[] { -10f, -145f, -270f };
        if (canContinue) StyleButton(cont, emeraldButton, "✦  CONTINUE RUN  ✦\n<size=52%>" + RunSave.Describe(saved) + "</size>", y[0], new Color(0.95f, 1f, 0.92f));
        StyleButton(start, goldButton, canContinue ? "✦  NEW GAME  ✦" : "✦  START GAME  ✦", y[1], new Color(1f, 0.94f, 0.58f));
        StyleButton(quit, crimsonButton, "✦  QUIT GAME  ✦", y[2], new Color(1f, 0.88f, 0.88f));
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
