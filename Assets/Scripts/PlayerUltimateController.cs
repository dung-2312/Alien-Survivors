using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUltimateController : MonoBehaviour
{
    public static PlayerUltimateController instance;

    [Header("Rage / Energy Stats")]
    public float currentRage = 0f;
    public float maxRage = 100f;
    public float ragePerKill = 1.2f;
    public float passiveRageRegen = 0.8f;

    [Header("Ultimate State")]
    public bool isCasting = false;
    private PlayerController player;

    public float RagePercent => Mathf.Clamp01(currentRage / maxRage);
    public bool IsUltimateReady => currentRage >= maxRage && !isCasting;

    public string GetUltimateName()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        switch (selectedIndex)
        {
            case 0: return "AXE TEMPEST";
            case 1: return "EXCALIBUR STORM";
            case 2: return "SHADOW DANCE";
            case 3: return "METEOR CALAMITY";
            case 4: return "DIVINE SANCTUARY";
            case 5: return "SPIRIT BARRAGE";
            default: return "EXCALIBUR STORM";
        }
    }

    void Awake()
    {
        instance = this;
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Tích nộ bị động chậm theo thời gian
        if (currentRage < maxRage && !isCasting)
        {
            currentRage = Mathf.Min(maxRage, currentRage + passiveRageRegen * Time.deltaTime);
        }

        // Kích hoạt chiêu cuối khi nhấn E hoặc Chuột Phải
        if (IsUltimateReady && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)))
        {
            ActivateUltimate();
        }
    }

    /// <summary>
    /// Thêm điểm nộ khi hạ gục quái vật hoặc đánh trúng boss
    /// </summary>
    public void AddRage(float amount)
    {
        if (isCasting) return;
        currentRage = Mathf.Clamp(currentRage + amount, 0f, maxRage);
    }

    /// <summary>
    /// Kích hoạt Chiêu Cuối theo đặc trưng từng nhân vật
    /// </summary>
    public void ActivateUltimate()
    {
        if (!IsUltimateReady) return;

        currentRage = 0f;
        isCasting = true;

        // Bất tử trong thời gian vận chiêu
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.TriggerInvincibility(1.6f);
        }

        // Rung chấn màn hình
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.35f, 0.45f);
        }

        // Âm thanh uy lực
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
        ProceduralSFX.PlayUltimate();

        // Xác định chiêu cuối theo tên tướng
        string charName = "Antonio";
        if (player != null)
        {
            string objName = player.gameObject.name.ToLowerInvariant();
            if (objName.Contains("knight")) charName = "Hero Knight";
            else if (objName.Contains("wizard")) charName = "Evil Wizard";
            else if (objName.Contains("adventurer")) charName = "Adventurer";
            else if (objName.Contains("bandit")) charName = "Light Bandit";
            else if (objName.Contains("forest") || objName.Contains("archer")) charName = "Red Archer";
            else charName = "Antonio";
        }

        // Lấy tên từ CharacterData nếu có
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        switch (selectedIndex)
        {
            case 0: charName = "Antonio"; break;
            case 1: charName = "Hero Knight"; break;
            case 2: charName = "Light Bandit"; break;
            case 3: charName = "Evil Wizard"; break;
            case 4: charName = "Adventurer"; break;
            case 5: charName = "Red Archer"; break;
        }

        StartCoroutine(ExecuteHeroUltimate(charName));
    }

    private IEnumerator ExecuteHeroUltimate(string charName)
    {
        string ultTitle = "";

        switch (charName)
        {
            case "Hero Knight":
                ultTitle = "[ EXCALIBUR BLADE STORM ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateHeroKnight());
                break;

            case "Evil Wizard":
                ultTitle = "[ METEOR CALAMITY ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateEvilWizard());
                break;

            case "Antonio":
                ultTitle = "[ AXE TEMPEST ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateAntonio());
                break;

            case "Light Bandit":
                ultTitle = "[ SHADOW BLADE DANCE ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateLightBandit());
                break;

            case "Adventurer":
                ultTitle = "[ DIVINE SANCTUARY ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateAdventurer());
                break;

            case "Red Archer":
                ultTitle = "[ SPIRIT ARROW BARRAGE ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateRedArcher());
                break;

            default:
                yield return StartCoroutine(UltimateHeroKnight());
                break;
        }

        isCasting = false;
    }

    // =========================================================================
    // PROCEDURAL SPRITE GENERATORS & VFX CACHE
    // =========================================================================

    private static Sprite magicCircleSprite;
    private static Sprite excaliburBladeSprite;
    private static Sprite meteorSprite;
    private static Sprite scorchedGroundSprite;
    private static Sprite battleAxeSprite;
    private static Sprite shadowSlashSprite;
    private static Sprite holyPillarSprite;
    private static Sprite spiritArrowSprite;
    private static Sprite shockwaveRingSprite;
    private static Sprite sparkleStarSprite;
    private static Material trailMaterial;

    private static Material GetTrailMaterial()
    {
        if (trailMaterial == null)
        {
            Shader s = Shader.Find("Sprites/Default");
            if (s != null) trailMaterial = new Material(s);
        }
        return trailMaterial;
    }

    public static Sprite GetMagicCircleSprite()
    {
        if (magicCircleSprite != null) return magicCircleSprite;

        int size = 96;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        float c = (size - 1) * 0.5f;
        float rOuter = c - 2f;
        float rInner = c - 8f;
        float rMid = c * 0.65f;
        float rCore = c * 0.25f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float dx = x - c;
                float dy = y - c;
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                // 1. Viền ngoài rực sáng
                if (dist <= rOuter && dist >= rInner)
                {
                    float a = 1f - Mathf.Abs(dist - (rInner + rOuter) * 0.5f) / 3f;
                    p[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                }
                // 2. Các mấu khắc cổ tự (Rune teeth)
                else if (dist <= rOuter + 2f && dist >= rOuter - 4f && (Mathf.RoundToInt(angle) % 30 < 6))
                {
                    p[y * size + x] = new Color(1f, 0.95f, 0.7f, 0.95f);
                }
                // 3. Vòng tròn đồng tâm thứ 2
                else if (Mathf.Abs(dist - rMid) <= 1.8f)
                {
                    p[y * size + x] = new Color(1f, 0.9f, 0.6f, 0.85f);
                }
                // 4. Các nan hoa ma pháp 8 hướng
                else if (dist <= rOuter && dist >= rCore && (Mathf.Abs(dx) <= 1.2f || Mathf.Abs(dy) <= 1.2f || Mathf.Abs(dx - dy) <= 1.5f || Mathf.Abs(dx + dy) <= 1.5f))
                {
                    float fade = 1f - (dist / rOuter);
                    p[y * size + x] = new Color(1f, 0.95f, 0.8f, Mathf.Clamp01(fade * 0.85f));
                }
                // 5. Ngôi sao trung tâm
                else if (dist <= rCore)
                {
                    float a = 1f - (dist / rCore);
                    p[y * size + x] = new Color(1f, 1f, 1f, a * a);
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        magicCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return magicCircleSprite;
    }

    public static Sprite GetExcaliburBladeSprite()
    {
        if (excaliburBladeSprite != null) return excaliburBladeSprite;

        int w = 32, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        int midX = w / 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int dx = Mathf.Abs(x - midX);

                // Lưỡi kiếm (y từ 18 đến 60)
                if (y >= 18 && y <= 60)
                {
                    int bladeWidth = Mathf.Clamp(Mathf.RoundToInt((60 - y) * 0.25f) + 3, 1, 6);
                    if (y >= 54) bladeWidth = Mathf.Clamp(60 - y, 0, 4);

                    if (dx == 0)
                    {
                        p[y * w + x] = new Color(0.7f, 0.95f, 1f, 1f); // Rãnh máu xanh lam ma thuật
                    }
                    else if (dx <= bladeWidth)
                    {
                        p[y * w + x] = Color.white; // Lưỡi thép sắc lẹm
                    }
                    else if (dx == bladeWidth + 1)
                    {
                        p[y * w + x] = new Color(1f, 0.88f, 0.35f, 0.9f); // Hào quang vàng hoàng kim
                    }
                }
                // Chuôi kiếm & Đốc kiếm (y = 14 đến 17)
                else if (y >= 14 && y <= 17)
                {
                    if (dx <= 10)
                    {
                        if (dx == 0 && (y == 15 || y == 16)) p[y * w + x] = new Color(0.2f, 0.8f, 1f); // Viên ngọc Sapphire ở giữa
                        else p[y * w + x] = new Color(1f, 0.82f, 0.2f); // Vàng chạm khắc
                    }
                }
                // Tay cầm (y = 4 đến 13)
                else if (y >= 4 && y <= 13)
                {
                    if (dx <= 2)
                    {
                        p[y * w + x] = (y % 2 == 0) ? new Color(0.6f, 0.35f, 0.15f) : new Color(0.85f, 0.65f, 0.25f);
                    }
                }
                // Đuôi kiếm (y = 0 đến 3)
                else if (y <= 3)
                {
                    if (dx <= 4 && (dx + (3 - y) <= 4))
                    {
                        p[y * w + x] = new Color(1f, 0.85f, 0.25f);
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        excaliburBladeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.35f), 32f);
        return excaliburBladeSprite;
    }

    public static Sprite GetMeteorSprite()
    {
        if (meteorSprite != null) return meteorSprite;

        int size = 36;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), c);
                // Tạo viền đá thiên thạch gồ ghề
                float jagged = Mathf.Sin(x * 1.5f) * Mathf.Cos(y * 1.5f) * 2.2f;
                float r = 13f + jagged;

                if (dist <= r)
                {
                    if (dist <= 4.5f)
                    {
                        p[y * size + x] = new Color(1f, 1f, 0.85f); // Lõi dung nham trắng rực
                    }
                    else if (dist <= 9f)
                    {
                        p[y * size + x] = new Color(1f, 0.6f, 0.1f); // Lớp magma cam cháy
                    }
                    else if (dist <= 12f)
                    {
                        p[y * size + x] = new Color(0.85f, 0.18f, 0.1f); // Lớp lửa đỏ
                    }
                    else
                    {
                        p[y * size + x] = new Color(0.25f, 0.12f, 0.12f); // Lớp vỏ đá núi lửa đen
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        meteorSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return meteorSprite;
    }

    public static Sprite GetScorchedGroundSprite()
    {
        if (scorchedGroundSprite != null) return scorchedGroundSprite;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float maxR = size * 0.46f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), c);
                if (dist <= maxR)
                {
                    float norm = dist / maxR;
                    float crack = (Mathf.Sin(x * 0.8f) * Mathf.Cos(y * 0.8f) > 0.35f) ? 1.4f : 0.85f;
                    float alpha = Mathf.Clamp01((1f - norm) * crack);

                    if (norm < 0.35f)
                    {
                        p[y * size + x] = new Color(1f, 0.5f, 0.1f, alpha * 0.85f); // Vết nứt dung nham rực sáng
                    }
                    else
                    {
                        p[y * size + x] = new Color(0.12f, 0.08f, 0.08f, alpha * 0.65f); // Đất cháy đen
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        scorchedGroundSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return scorchedGroundSprite;
    }

    public static Sprite GetBattleAxeSprite()
    {
        if (battleAxeSprite != null) return battleAxeSprite;

        int size = 44;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        int mid = size / 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - mid);

                // Cán búa (x ở giữa, y từ 4 đến 38)
                if (dx <= 2 && y >= 4 && y <= 38)
                {
                    p[y * size + x] = (y >= 6 && y <= 16) ? new Color(0.75f, 0.5f, 0.2f) : new Color(0.45f, 0.28f, 0.15f);
                }

                // Đầu rìu chiến 2 lưỡi (y từ 22 đến 36)
                if (y >= 22 && y <= 36)
                {
                    int dy = Mathf.Abs(y - 29);
                    if (dx >= 3 && dx <= 18)
                    {
                        float bladeCurve = Mathf.Abs(dx - 11f);
                        if (dy <= (dx - 2) * 0.85f)
                        {
                            if (dx >= 16) p[y * size + x] = Color.white; // Lưỡi chém trắng bạc
                            else if (dx >= 12) p[y * size + x] = new Color(0.4f, 0.85f, 1f); // Hào quang băng thép
                            else p[y * size + x] = new Color(0.6f, 0.65f, 0.72f); // Thân thép
                        }
                    }
                    // Mấu vàng trung tâm
                    if (dx <= 4 && dy <= 4)
                    {
                        p[y * size + x] = new Color(1f, 0.82f, 0.25f);
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        battleAxeSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return battleAxeSprite;
    }

    public static Sprite GetShadowSlashSprite()
    {
        if (shadowSlashSprite != null) return shadowSlashSprite;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        Vector2 c = new Vector2(size * 0.35f, size * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), c);
                float dx = x - c.x;
                float dy = y - c.y;
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

                if (dist >= 14f && dist <= 26f && Mathf.Abs(angle) <= 60f)
                {
                    float rCenter = 20f;
                    float thick = 1f - Mathf.Abs(dist - rCenter) / 6f;
                    if (thick > 0.65f)
                    {
                        p[y * size + x] = Color.white; // Lưỡi chém sắc
                    }
                    else if (thick > 0.25f)
                    {
                        p[y * size + x] = new Color(0.2f, 0.95f, 1f, 0.95f); // Lam ngọc bóng ma
                    }
                    else
                    {
                        p[y * size + x] = new Color(0.5f, 0.1f, 0.9f, 0.75f); // Tím hắc ám
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        shadowSlashSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.35f, 0.5f), 32f);
        return shadowSlashSprite;
    }

    public static Sprite GetHolyPillarSprite()
    {
        if (holyPillarSprite != null) return holyPillarSprite;

        int w = 32, h = 96;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] p = new Color[w * h];
        float midX = (w - 1) * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - midX);
                float normX = dx / midX;
                float alpha = Mathf.Clamp01(1f - normX * normX);

                if (normX <= 0.35f)
                {
                    p[y * w + x] = new Color(1f, 1f, 1f, alpha); // Lõi cột sáng trắng tinh khôi
                }
                else
                {
                    p[y * w + x] = new Color(1f, 0.88f, 0.45f, alpha * 0.85f); // Ánh hào quang vàng kim
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        holyPillarSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.15f), 32f);
        return holyPillarSprite;
    }

    public static Sprite GetSpiritArrowSprite()
    {
        if (spiritArrowSprite != null) return spiritArrowSprite;

        int w = 16, h = 48;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        int midX = w / 2;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int dx = Mathf.Abs(x - midX);

                // Mũi tên ánh sáng (y từ 34 đến 46)
                if (y >= 34)
                {
                    int arrowW = Mathf.Clamp(46 - y, 0, 6);
                    if (dx <= arrowW)
                    {
                        p[y * w + x] = (dx <= 1) ? Color.white : new Color(0.2f, 1f, 0.6f); // Xanh ngọc lục bảo
                    }
                }
                // Thân tên (y từ 10 đến 34)
                else if (y >= 10 && y < 34)
                {
                    if (dx == 0) p[y * w + x] = Color.white;
                    else if (dx <= 1) p[y * w + x] = new Color(1f, 0.9f, 0.3f); // Thân vàng óng
                }
                // Lông vũ cánh đuôi (y từ 2 đến 12)
                else if (y <= 12)
                {
                    int featherW = Mathf.Clamp(y, 1, 5);
                    if (dx <= featherW && dx >= 1)
                    {
                        p[y * w + x] = new Color(0.4f, 1f, 0.7f, 0.85f);
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        spiritArrowSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.75f), 32f);
        return spiritArrowSprite;
    }

    public static Sprite GetShockwaveRingSprite()
    {
        if (shockwaveRingSprite != null) return shockwaveRingSprite;

        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] p = new Color[size * size];

        float c = (size - 1) * 0.5f;
        float rOuter = c - 1f;
        float rInner = c - 6f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                if (dist <= rOuter && dist >= rInner)
                {
                    float a = 1f - Mathf.Abs(dist - (rInner + rOuter) * 0.5f) / 2.5f;
                    p[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(a));
                }
                else
                {
                    p[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        shockwaveRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return shockwaveRingSprite;
    }

    public static Sprite GetSparkleStarSprite()
    {
        if (sparkleStarSprite != null) return sparkleStarSprite;

        int size = 20;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        float c = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - c);
                float dy = Mathf.Abs(y - c);
                float star = Mathf.Min(dx * 2.6f + dy * 0.5f, dx * 0.5f + dy * 2.6f);
                if (star <= c)
                {
                    float a = Mathf.Clamp01(1f - (star / c));
                    p[y * size + x] = new Color(1f, 1f, 1f, a);
                }
                else
                {
                    p[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        sparkleStarSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return sparkleStarSprite;
    }

    // =========================================================================
    // VFX HELPER SPAWNERS
    // =========================================================================

    public void SpawnMagicCircle(Vector3 center, float radius, float duration, Color color, float spinSpeed = 90f)
    {
        GameObject circle = new GameObject("ArcaneCircle", typeof(SpriteRenderer));
        circle.transform.position = center;
        circle.transform.localScale = Vector3.zero;

        SpriteRenderer sr = circle.GetComponent<SpriteRenderer>();
        sr.sprite = GetMagicCircleSprite();
        sr.color = color;
        sr.sortingOrder = 3;

        StartCoroutine(AnimateMagicCircle(circle, sr, radius, duration, color, spinSpeed));
    }

    private IEnumerator AnimateMagicCircle(GameObject circle, SpriteRenderer sr, float targetRadius, float duration, Color color, float spinSpeed)
    {
        float elapsed = 0f;
        while (elapsed < duration && circle != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            circle.transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

            // Pop-in nhanh rồi mờ dần
            float scaleProgress = Mathf.Clamp01(elapsed / (duration * 0.2f));
            circle.transform.localScale = Vector3.one * Mathf.Lerp(0f, targetRadius * 2f, Mathf.Sin(scaleProgress * Mathf.PI * 0.5f));

            if (sr != null)
            {
                float alpha = (t < 0.7f) ? color.a : Mathf.Lerp(color.a, 0f, (t - 0.7f) / 0.3f);
                sr.color = new Color(color.r, color.g, color.b, alpha);
            }
            yield return null;
        }

        if (circle != null) Destroy(circle);
    }

    public void SpawnShockwaveRing(Vector3 center, float targetRadius, float duration, Color color)
    {
        GameObject ring = new GameObject("ShockwaveRing", typeof(SpriteRenderer));
        ring.transform.position = center;
        ring.transform.localScale = Vector3.zero;

        SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
        sr.sprite = GetShockwaveRingSprite();
        sr.color = color;
        sr.sortingOrder = 14;

        StartCoroutine(AnimateShockwave(ring, sr, targetRadius, duration, color));
    }

    private IEnumerator AnimateShockwave(GameObject ring, SpriteRenderer sr, float targetRadius, float duration, Color color)
    {
        float elapsed = 0f;
        while (elapsed < duration && ring != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
            ring.transform.localScale = Vector3.one * (targetRadius * 2f * ease);

            if (sr != null)
            {
                sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, t * t));
            }
            yield return null;
        }
        if (ring != null) Destroy(ring);
    }

    public void SpawnSparkleMote(Vector3 pos, Color color, float scale = 0.45f)
    {
        GameObject star = new GameObject("SparkleMote", typeof(SpriteRenderer));
        star.transform.position = pos;
        star.transform.localScale = Vector3.one * scale;
        star.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        SpriteRenderer sr = star.GetComponent<SpriteRenderer>();
        sr.sprite = GetSparkleStarSprite();
        sr.color = color;
        sr.sortingOrder = 18;

        StartCoroutine(AnimateSparkleMote(star, sr, 0.35f));
    }

    private IEnumerator AnimateSparkleMote(GameObject star, SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Vector3 baseScale = star.transform.localScale;
        Color c = sr.color;

        while (elapsed < duration && star != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            star.transform.localScale = Vector3.Lerp(baseScale, Vector3.zero, t);
            if (sr != null)
            {
                sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, t));
            }
            yield return null;
        }
        if (star != null) Destroy(star);
    }

    // =========================================================================
    // 1. HERO KNIGHT: [ EXCALIBUR BLADE STORM ] (VẠN KIẾM QUY TÔNG)
    // =========================================================================

    private IEnumerator UltimateHeroKnight()
    {
        // 1. Triệu hồi vòng tròn ma pháp hoàng kim dưới chân
        SpawnMagicCircle(transform.position, 4.2f, 2.6f, new Color(1f, 0.85f, 0.25f, 0.95f), 120f);
        SpawnShockwaveRing(transform.position, 4.5f, 0.45f, new Color(1f, 0.92f, 0.4f, 0.9f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.35f, 0.4f);

        // 2. Tụ 18 thanh Thánh Kiếm Excalibur xoay tròn xung quanh
        int totalBlades = 18;
        List<GameObject> blades = new List<GameObject>();
        float radius = 2.0f;

        for (int i = 0; i < totalBlades; i++)
        {
            float angle = i * (360f / totalBlades);
            Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.up * radius;

            GameObject b = new GameObject("HoverBlade", typeof(SpriteRenderer));
            b.transform.position = transform.position + offset;
            b.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
            b.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            SpriteRenderer sr = b.GetComponent<SpriteRenderer>();
            sr.sprite = GetExcaliburBladeSprite();
            sr.sortingOrder = 16;

            blades.Add(b);
        }

        // Tụ năng lượng trong 0.35s
        float hoverTimer = 0f;
        while (hoverTimer < 0.35f)
        {
            hoverTimer += Time.deltaTime;
            for (int i = 0; i < blades.Count; i++)
            {
                if (blades[i] == null) continue;
                float curAngle = (i * (360f / totalBlades)) + (hoverTimer * 360f);
                Vector3 off = Quaternion.Euler(0, 0, curAngle) * Vector3.up * (radius + Mathf.Sin(hoverTimer * 10f) * 0.2f);
                blades[i].transform.position = transform.position + off;
                blades[i].transform.rotation = Quaternion.Euler(0, 0, curAngle - 90f);

                if (Random.value < 0.25f)
                {
                    SpawnSparkleMote(blades[i].transform.position, new Color(1f, 0.9f, 0.4f), 0.35f);
                }
            }
            yield return null;
        }

        // 3. Phóng vạn kiếm theo 3 làn bão xoáy rực rỡ
        for (int i = 0; i < blades.Count; i++)
        {
            if (blades[i] != null)
            {
                Vector3 dir = (blades[i].transform.position - transform.position).normalized;
                if (dir == Vector3.zero) dir = Vector3.right;
                LaunchExcaliburBlade(blades[i].transform.position, dir, 46f);
                Destroy(blades[i]);
            }
            if (i % 3 == 0)
            {
                if (CameraController.instance != null) CameraController.instance.Shake(0.12f, 0.2f);
                yield return new WaitForSeconds(0.04f);
            }
        }
    }

    private void LaunchExcaliburBlade(Vector3 origin, Vector3 dir, float damage)
    {
        GameObject blade = new GameObject("LaunchedExcalibur", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager), typeof(Rigidbody2D));
        blade.transform.position = origin;
        blade.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        blade.transform.localScale = new Vector3(2.4f, 2.4f, 1f);

        SpriteRenderer sr = blade.GetComponent<SpriteRenderer>();
        sr.sprite = GetExcaliburBladeSprite();
        sr.sortingOrder = 16;

        // Thêm dải vệt sáng Trail vàng rực rỡ
        TrailRenderer tr = blade.AddComponent<TrailRenderer>();
        tr.material = GetTrailMaterial();
        tr.time = 0.22f;
        tr.minVertexDistance = 0.05f;
        tr.sortingOrder = 15;

        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.5f);
        widthCurve.AddKey(1f, 0.0f);
        tr.widthCurve = widthCurve;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.82f, 0.2f), 0.5f), new GradientColorKey(new Color(1f, 0.4f, 0.1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        tr.colorGradient = grad;

        CircleCollider2D col = blade.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.75f;

        EnemyDamager ed = blade.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false;
        ed.lifeTime = 1.2f;

        Rigidbody2D rb = blade.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = dir * 18f;

        Destroy(blade, 1.2f);
    }

    // =========================================================================
    // 2. EVIL WIZARD: [ METEOR CALAMITY ] (HỎA NGỤC THIÊN THẠCH)
    // =========================================================================

    private IEnumerator UltimateEvilWizard()
    {
        // 1. Triệu hồi ma trận hắc ám tím & đỏ rực dưới chân Pháp Sư
        SpawnMagicCircle(transform.position, 5.0f, 3.2f, new Color(0.85f, 0.15f, 0.45f, 0.95f), -140f);
        SpawnShockwaveRing(transform.position, 5.2f, 0.5f, new Color(1f, 0.25f, 0.1f, 0.85f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.35f, 0.45f);

        // 2. Giáng mưa 16 quả thiên thạch khổng lồ từ không gian
        int meteorCount = 16;
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * Random.Range(2.0f, 9.5f);
            Vector3 impactPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            StartCoroutine(SpawnMeteorRoutine(impactPos));
            yield return new WaitForSeconds(0.11f);
        }
    }

    private IEnumerator SpawnMeteorRoutine(Vector3 impactPos)
    {
        // Điểm đánh dấu cảnh báo mặt đất
        GameObject marker = new GameObject("MeteorTarget", typeof(SpriteRenderer));
        marker.transform.position = impactPos;
        marker.transform.localScale = Vector3.one * 0.5f;
        SpriteRenderer msr = marker.GetComponent<SpriteRenderer>();
        msr.sprite = GetShockwaveRingSprite();
        msr.color = new Color(1f, 0.2f, 0.1f, 0.75f);
        msr.sortingOrder = 4;

        // Thiên thạch lao từ trên trời xuống
        Vector3 fallStart = impactPos + new Vector3(-3.5f, 10f, 0f);
        GameObject meteor = new GameObject("FallingMeteor", typeof(SpriteRenderer));
        meteor.transform.position = fallStart;
        meteor.transform.localScale = Vector3.one * 2.4f;

        SpriteRenderer sr = meteor.GetComponent<SpriteRenderer>();
        sr.sprite = GetMeteorSprite();
        sr.sortingOrder = 17;

        // Vệt lửa đuôi thiên thạch
        TrailRenderer tr = meteor.AddComponent<TrailRenderer>();
        tr.material = GetTrailMaterial();
        tr.time = 0.25f;
        tr.minVertexDistance = 0.05f;
        tr.sortingOrder = 16;

        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.8f);
        widthCurve.AddKey(1f, 0.0f);
        tr.widthCurve = widthCurve;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.55f, 0.1f), 0.4f), new GradientColorKey(new Color(0.85f, 0.1f, 0.05f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        tr.colorGradient = grad;

        float fallDuration = 0.32f;
        float elapsed = 0f;
        while (elapsed < fallDuration && meteor != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            meteor.transform.position = Vector3.Lerp(fallStart, impactPos, t * t);
            meteor.transform.Rotate(0, 0, 360f * Time.deltaTime);

            if (marker != null)
            {
                marker.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 2.6f, t);
            }
            yield return null;
        }

        if (meteor != null) Destroy(meteor);
        if (marker != null) Destroy(marker);

        // Vụ nổ hủy diệt khi thiên thạch chạm đất
        SpawnShockwaveRing(impactPos, 3.2f, 0.4f, new Color(1f, 0.45f, 0.1f, 0.95f));
        SpawnSparkleMote(impactPos, new Color(1f, 0.85f, 0.3f), 0.8f);

        // Vết cháy dung nham trên mặt đất
        GameObject crater = new GameObject("ScorchedCrater", typeof(SpriteRenderer));
        crater.transform.position = impactPos;
        crater.transform.localScale = Vector3.one * 2.6f;
        SpriteRenderer csr = crater.GetComponent<SpriteRenderer>();
        csr.sprite = GetScorchedGroundSprite();
        csr.sortingOrder = 2;
        Destroy(crater, 2.2f);

        // Vùng gây sát thương nổ
        GameObject blast = new GameObject("MeteorBlastZone", typeof(CircleCollider2D), typeof(EnemyDamager));
        blast.transform.position = impactPos;
        CircleCollider2D col = blast.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 2.6f;

        EnemyDamager ed = blast.GetComponent<EnemyDamager>();
        ed.damageAmount = 75f;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.35f;

        if (CameraController.instance != null) CameraController.instance.Shake(0.18f, 0.22f);
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);

        Destroy(blast, 0.35f);
    }

    // =========================================================================
    // 3. ANTONIO: [ AXE TEMPEST ] (CUỒNG PHONG BÚA LỐC)
    // =========================================================================

    private IEnumerator UltimateAntonio()
    {
        // 1. Tiếng thét xung trận & sóng gió đẩy lùi quái
        SpawnMagicCircle(transform.position, 4.5f, 2.8f, new Color(1f, 0.75f, 0.2f, 0.9f), 180f);
        SpawnShockwaveRing(transform.position, 5.5f, 0.45f, new Color(1f, 0.85f, 0.35f, 0.95f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.35f, 0.45f);

        // 2. Tạo bão lốc 20 chiến rìu xoay 2 tầng quanh Antonio
        int axeCount = 20;
        List<GameObject> axes = new List<GameObject>();
        float[] baseRadii = new float[axeCount];
        float[] angles = new float[axeCount];

        for (int i = 0; i < axeCount; i++)
        {
            GameObject axe = new GameObject("TempestAxe", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager));
            axe.transform.localScale = Vector3.one * 2.2f;

            SpriteRenderer sr = axe.GetComponent<SpriteRenderer>();
            sr.sprite = GetBattleAxeSprite();
            sr.sortingOrder = 16;

            TrailRenderer tr = axe.AddComponent<TrailRenderer>();
            tr.material = GetTrailMaterial();
            tr.time = 0.18f;
            tr.minVertexDistance = 0.05f;
            tr.sortingOrder = 15;

            AnimationCurve widthCurve = new AnimationCurve();
            widthCurve.AddKey(0f, 0.45f);
            widthCurve.AddKey(1f, 0.0f);
            tr.widthCurve = widthCurve;

            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.75f, 0.2f), 0.6f), new GradientColorKey(new Color(0.8f, 0.3f, 0.05f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.85f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            tr.colorGradient = grad;

            CircleCollider2D col = axe.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.75f;

            EnemyDamager ed = axe.GetComponent<EnemyDamager>();
            ed.damageAmount = 40f;
            ed.shouldKnockBack = true;
            ed.destroyOnImpact = false;
            ed.lifeTime = 2.4f;

            axes.Add(axe);
            baseRadii[i] = (i % 2 == 0) ? 2.2f : 4.2f;
            angles[i] = i * (360f / axeCount);
        }

        // Bão rìu xoay tròn và nở rộng theo hình xoắn ốc bão táp
        float stormDuration = 2.2f;
        float elapsed = 0f;
        while (elapsed < stormDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / stormDuration;
            Vector3 center = transform.position;

            for (int i = 0; i < axes.Count; i++)
            {
                if (axes[i] == null) continue;
                float rotDir = (i % 2 == 0) ? 1f : -1f;
                angles[i] += rotDir * (480f * Time.deltaTime);

                float curRadius = baseRadii[i] + (progress * 3.8f);
                Vector3 pos = center + (Quaternion.Euler(0, 0, angles[i]) * Vector3.up * curRadius);
                axes[i].transform.position = pos;
                axes[i].transform.Rotate(0, 0, 1200f * Time.deltaTime);
            }

            if (Random.value < 0.25f)
            {
                SpawnSparkleMote(center + (Vector3)(Random.insideUnitCircle * 3.5f), new Color(1f, 0.85f, 0.3f), 0.35f);
            }

            yield return null;
        }

        for (int i = 0; i < axes.Count; i++)
        {
            if (axes[i] != null) Destroy(axes[i]);
        }
    }

    // =========================================================================
    // 4. LIGHT BANDIT: [ SHADOW BLADE DANCE ] (VŨ ĐIỆU BÓNG MA)
    // =========================================================================

    private IEnumerator UltimateLightBandit()
    {
        // 1. Bandit tan biến thành khói đen bóng ma
        SpawnShockwaveRing(transform.position, 3.8f, 0.35f, new Color(0.2f, 0.95f, 1f, 0.9f));
        SpawnSparkleMote(transform.position, new Color(0.3f, 0.9f, 1f), 0.8f);
        if (CameraController.instance != null) CameraController.instance.Shake(0.3f, 0.35f);

        // 2. Chớp nhoáng 20 nhát chém ảo ảnh chữ X lên kẻ thù xung quanh
        int strikeCount = 20;
        for (int i = 0; i < strikeCount; i++)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 12f);
            EnemyController targetEnemy = null;
            float bestWeight = float.MinValue;

            foreach (var h in hits)
            {
                if (h != null && h.TryGetComponent<EnemyController>(out var ec) && ec.gameObject.activeInHierarchy && ec.health > 0)
                {
                    float d = Vector2.Distance(transform.position, ec.transform.position);
                    float weight = (ec.isMiniBoss ? 100f : 10f) - d;
                    if (weight > bestWeight)
                    {
                        bestWeight = weight;
                        targetEnemy = ec;
                    }
                }
            }

            Vector3 strikePos = targetEnemy != null ? targetEnemy.transform.position : transform.position + (Vector3)(Random.insideUnitCircle * 6f);
            SpawnShadowDoubleSlash(strikePos, 55f);

            if (CameraController.instance != null) CameraController.instance.Shake(0.08f, 0.12f);
            if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(1);

            yield return new WaitForSeconds(0.065f);
        }

        // 3. Tái hiện tại vị trí với vòng xung kích hắc ám
        SpawnShockwaveRing(transform.position, 4.8f, 0.4f, new Color(0.4f, 0.95f, 1f, 0.95f));
        SpawnMagicCircle(transform.position, 4.0f, 1.2f, new Color(0.2f, 0.85f, 1f, 0.85f), 240f);
    }

    private void SpawnShadowDoubleSlash(Vector3 pos, float damage)
    {
        // Nhát chém 1 chéo góc 45 độ
        SpawnSingleSlash(pos, 45f, damage);
        // Nhát chém 2 chéo góc -45 độ tạo thành hình chữ X chém đôi quái vật
        SpawnSingleSlash(pos, -45f, damage);

        SpawnSparkleMote(pos, new Color(0.2f, 0.95f, 1f), 0.5f);
    }

    private void SpawnSingleSlash(Vector3 pos, float angle, float damage)
    {
        GameObject slash = new GameObject("ShadowSlash", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager));
        slash.transform.position = pos;
        slash.transform.rotation = Quaternion.Euler(0, 0, angle);
        slash.transform.localScale = Vector3.one * 2.2f;

        SpriteRenderer sr = slash.GetComponent<SpriteRenderer>();
        sr.sprite = GetShadowSlashSprite();
        sr.sortingOrder = 17;

        CircleCollider2D col = slash.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.4f;

        EnemyDamager ed = slash.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.22f;

        StartCoroutine(AnimateSlashFade(slash, sr, 0.22f));
    }

    private IEnumerator AnimateSlashFade(GameObject slash, SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Vector3 baseScale = slash.transform.localScale;
        while (elapsed < duration && slash != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            slash.transform.localScale = Vector3.Lerp(baseScale * 0.8f, baseScale * 1.35f, t);
            if (sr != null)
            {
                sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, t * t));
            }
            yield return null;
        }
        if (slash != null) Destroy(slash);
    }

    // =========================================================================
    // 5. ADVENTURER: [ DIVINE SANCTUARY ] (THÁNH QUANG BẢO HỘ)
    // =========================================================================

    private IEnumerator UltimateAdventurer()
    {
        // 1. Hồi phục 60 HP tức thì cho người chơi
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.Heal(60f);
            PlayerHealthController.instance.TriggerInvincibility(3.0f);
        }

        // 2. Cột sáng thần thánh khổng lồ giáng xuống từ trời cao
        GameObject pillar = new GameObject("DivinePillar", typeof(SpriteRenderer));
        pillar.transform.position = transform.position + Vector3.up * 1.5f;
        pillar.transform.localScale = new Vector3(4.5f, 18f, 1f);

        SpriteRenderer psr = pillar.GetComponent<SpriteRenderer>();
        psr.sprite = GetHolyPillarSprite();
        psr.color = new Color(1f, 0.95f, 0.6f, 0.95f);
        psr.sortingOrder = 17;

        // Vòng tròn thần thánh dưới chân
        SpawnMagicCircle(transform.position, 6.0f, 3.5f, new Color(1f, 0.95f, 0.5f, 0.95f), 60f);
        SpawnShockwaveRing(transform.position, 8.5f, 0.6f, new Color(1f, 0.98f, 0.7f, 0.95f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.4f, 0.5f);

        // 3. Sấm sét thánh quang trừng phạt 8 kẻ thù mạnh nhất
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 12f);
        int smiteCount = 0;
        foreach (var h in hits)
        {
            if (h != null && h.TryGetComponent<EnemyController>(out var ec) && ec.gameObject.activeInHierarchy && ec.health > 0)
            {
                StartCoroutine(SpawnHolySmite(ec.transform.position));
                smiteCount++;
                if (smiteCount >= 8) break;
            }
        }

        // 4. Nam châm vạn năng: Hút TOÀN BỘ ngọc kinh nghiệm và vàng trên toàn bản đồ về người!
        ExperienceLevelController.instance?.SpawnExp(transform.position, 20);
        CoinController.instance?.DropCoin(transform.position, 25);

        ExpPickup[] gems = FindObjectsByType<ExpPickup>(FindObjectsSortMode.None);
        foreach (var g in gems)
        {
            if (g != null)
            {
                StartCoroutine(PullPickupToPlayer(g.gameObject));
            }
        }

        CoinPickup[] coins = FindObjectsByType<CoinPickup>(FindObjectsSortMode.None);
        foreach (var c in coins)
        {
            if (c != null)
            {
                StartCoroutine(PullPickupToPlayer(c.gameObject));
            }
        }

        // Mờ dần cột sáng
        float elapsed = 0f;
        while (elapsed < 1.0f && pillar != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 1.0f;
            psr.color = new Color(1f, 0.95f, 0.6f, Mathf.Lerp(0.95f, 0f, t));
            yield return null;
        }
        if (pillar != null) Destroy(pillar);
    }

    private IEnumerator SpawnHolySmite(Vector3 targetPos)
    {
        yield return new WaitForSeconds(Random.Range(0.05f, 0.25f));

        GameObject smite = new GameObject("HolySmite", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager));
        smite.transform.position = targetPos;
        smite.transform.localScale = new Vector3(1.8f, 10f, 1f);

        SpriteRenderer sr = smite.GetComponent<SpriteRenderer>();
        sr.sprite = GetHolyPillarSprite();
        sr.color = new Color(1f, 1f, 0.7f, 1f);
        sr.sortingOrder = 18;

        CircleCollider2D col = smite.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 2.0f;

        EnemyDamager ed = smite.GetComponent<EnemyDamager>();
        ed.damageAmount = 85f;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.35f;

        SpawnShockwaveRing(targetPos, 2.5f, 0.3f, new Color(1f, 0.95f, 0.4f, 0.95f));
        SpawnSparkleMote(targetPos, Color.white, 0.6f);

        Destroy(smite, 0.35f);
    }

    private IEnumerator PullPickupToPlayer(GameObject pickup)
    {
        float speed = 12f;
        while (pickup != null && player != null)
        {
            pickup.transform.position = Vector3.MoveTowards(pickup.transform.position, player.transform.position, speed * Time.deltaTime);
            speed += 25f * Time.deltaTime;
            if (Vector2.Distance(pickup.transform.position, player.transform.position) < 0.6f)
            {
                break;
            }
            yield return null;
        }
    }

    // =========================================================================
    // 6. RED ARCHER: [ SPIRIT ARROW BARRAGE ] (THIÊN VŨ BÁCH TIỄN)
    // =========================================================================

    private IEnumerator UltimateRedArcher()
    {
        // 1. Khai mở thần cung linh thú: Vòng tròn ma pháp lục bảo xuất hiện
        float baseAngle = player != null && player.facingDirection < 0 ? 180f : 0f;
        Vector3 bowPos = transform.position + Quaternion.Euler(0, 0, baseAngle) * Vector3.right * 0.8f;
        SpawnMagicCircle(bowPos, 3.8f, 2.0f, new Color(0.2f, 1f, 0.55f, 0.95f), 150f);
        SpawnShockwaveRing(transform.position, 4.0f, 0.4f, new Color(0.4f, 1f, 0.7f, 0.9f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.25f, 0.3f);

        // 2. Bắn 6 đợt bão tiễn liên hoàn xòe quạt hình cánh bướm (tổng cộng 60 mũi tên xuyên phá!)
        int waveCount = 6;
        for (int w = 0; w < waveCount; w++)
        {
            int arrowCount = 10;
            float spreadArc = 60f;
            float startAngle = baseAngle - (spreadArc * 0.5f);
            float step = spreadArc / (arrowCount - 1);

            for (int i = 0; i < arrowCount; i++)
            {
                float angle = startAngle + (i * step) + Random.Range(-3f, 3f);
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                SpawnPiercingSpiritArrow(transform.position, dir, 36f);
            }

            if (CameraController.instance != null) CameraController.instance.Shake(0.1f, 0.15f);
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void SpawnPiercingSpiritArrow(Vector3 origin, Vector3 dir, float damage)
    {
        GameObject arrow = new GameObject("SpiritArrow", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager), typeof(Rigidbody2D));
        arrow.transform.position = origin;
        arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        arrow.transform.localScale = new Vector3(2.0f, 2.0f, 1f);

        SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
        sr.sprite = GetSpiritArrowSprite();
        sr.sortingOrder = 16;

        TrailRenderer tr = arrow.AddComponent<TrailRenderer>();
        tr.material = GetTrailMaterial();
        tr.time = 0.2f;
        tr.minVertexDistance = 0.05f;
        tr.sortingOrder = 15;

        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.4f);
        widthCurve.AddKey(1f, 0.0f);
        tr.widthCurve = widthCurve;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.2f, 1f, 0.6f), 0.4f), new GradientColorKey(new Color(1f, 0.9f, 0.3f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        tr.colorGradient = grad;

        CircleCollider2D col = arrow.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.65f;

        EnemyDamager ed = arrow.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false; // Xuyên táo toàn bộ quái
        ed.lifeTime = 1.3f;

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = dir * 20f;

        // Khi bay hết tầm hoặc va chạm, mũi tên phân mảnh thành 2 mảnh linh tinh sao tìm mục tiêu
        StartCoroutine(DetonateArrowShards(arrow, 0.85f));
    }

    private IEnumerator DetonateArrowShards(GameObject arrow, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (arrow != null)
        {
            Vector3 burstPos = arrow.transform.position;
            SpawnSparkleMote(burstPos, new Color(0.3f, 1f, 0.7f), 0.5f);

            // Bắn 2 mảnh linh tinh sao tìm mục tiêu gần nhất
            for (int s = 0; s < 2; s++)
            {
                float randAngle = Random.Range(0f, 360f);
                Vector3 shardDir = Quaternion.Euler(0, 0, randAngle) * Vector3.right;
                SpawnHomingShard(burstPos, shardDir, 20f);
            }

            Destroy(arrow);
        }
    }

    private void SpawnHomingShard(Vector3 pos, Vector3 dir, float damage)
    {
        GameObject shard = new GameObject("HomingShard", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager), typeof(Rigidbody2D));
        shard.transform.position = pos;
        shard.transform.localScale = Vector3.one * 1.2f;

        SpriteRenderer sr = shard.GetComponent<SpriteRenderer>();
        sr.sprite = GetSparkleStarSprite();
        sr.color = new Color(0.4f, 1f, 0.8f, 1f);
        sr.sortingOrder = 17;

        CircleCollider2D col = shard.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        EnemyDamager ed = shard.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = true;
        ed.lifeTime = 0.7f;

        Rigidbody2D rb = shard.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = dir * 12f;

        Destroy(shard, 0.7f);
    }
}
