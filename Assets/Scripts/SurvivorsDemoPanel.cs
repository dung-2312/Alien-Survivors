using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlienSurvivors.Upgrades;

/// <summary>
/// Quản lý cài đặt âm lượng (Master, BGM, SFX) và hiển thị cho toàn game
/// </summary>
public static class GameSettings
{
    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat("Setting_MasterVolume", 1.0f);
        set
        {
            PlayerPrefs.SetFloat("Setting_MasterVolume", value);
            AudioListener.volume = value;
        }
    }

    public static float BGMVolume
    {
        get => PlayerPrefs.GetFloat("Setting_BGMVolume", 0.8f);
        set
        {
            PlayerPrefs.SetFloat("Setting_BGMVolume", value);
            ApplyBGMVolume(value);
        }
    }

    public static float SFXVolume
    {
        get => PlayerPrefs.GetFloat("Setting_SFXVolume", 0.9f);
        set
        {
            PlayerPrefs.SetFloat("Setting_SFXVolume", value);
            ApplySFXVolume(value);
        }
    }

    public static void ApplyAll()
    {
        AudioListener.volume = MasterVolume;
        ApplyBGMVolume(BGMVolume);
        ApplySFXVolume(SFXVolume);
    }

    public static void ApplyBGMVolume(float vol)
    {
        AudioSource[] sources = Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include);
        foreach (var src in sources)
        {
            if (src != null && src.loop && (SFXManager.instance == null || !System.Array.Exists(SFXManager.instance.soundEffects, s => s == src)))
            {
                src.volume = vol;
            }
        }
    }

    public static void ApplySFXVolume(float vol)
    {
        if (SFXManager.instance != null && SFXManager.instance.soundEffects != null)
        {
            foreach (var src in SFXManager.instance.soundEffects)
            {
                if (src != null) src.volume = vol;
            }
        }
    }
}

/// <summary>
/// Bảng Điều Khiển Demo / Cheat Menu phục vụ báo cáo đồ án kết thúc môn PRU213.
/// Phím tắt bật/tắt: F1 hoặc phím Tilde (~) / Backquote (`).
/// Có nút mở nhanh ở góc trên bên phải màn hình.
/// </summary>
public class SurvivorsDemoPanel : MonoBehaviour
{
    public static SurvivorsDemoPanel instance;

    [Header("Panel Configuration")]
    public bool isVisible = false;
    public KeyCode toggleKey1 = KeyCode.F1;
    public KeyCode toggleKey2 = KeyCode.BackQuote; // Phím ~

    private Rect windowRect = new Rect(20, 40, 530, 640);
    private int selectedTab = 0;
    private readonly string[] tabNames = new string[] { "🎮 Player", "⚔ Weapons", "👑 Bosses", "⚡ Utilities" };

    // GUI Styles
    private GUIStyle windowStyle;
    private GUIStyle headerStyle;
    private GUIStyle tabButtonStyle;
    private GUIStyle activeTabButtonStyle;
    private GUIStyle actionButtonStyle;
    private GUIStyle dangerButtonStyle;
    private GUIStyle successButtonStyle;
    private GUIStyle labelStyle;
    private GUIStyle infoBoxStyle;
    private Texture2D windowBgTex;
    private Texture2D buttonBgTex;
    private Texture2D buttonActiveTex;
    private Texture2D greenTex;
    private Texture2D redTex;
    private bool stylesInitialized = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        GameSettings.ApplyAll();

        if (instance == null)
        {
            GameObject go = new GameObject("SurvivorsDemoPanel");
            instance = go.AddComponent<SurvivorsDemoPanel>();
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GameSettings.ApplyAll();
    }

    private void Update()
    {
        // Nhấn F1 hoặc ~ để bật/tắt bảng Cheat
        if (Input.GetKeyDown(toggleKey1) || Input.GetKeyDown(toggleKey2))
        {
            isVisible = !isVisible;
        }

        // Luôn giữ máu tối đa nếu đang bật God Mode
        if (PlayerHealthController.isGodMode && PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
        }
    }

    private void OnGUI()
    {
        InitStylesIfNeeded();

        // 1. Nút nhỏ trên màn hình để bật/tắt (tiện cho việc dùng chuột hoặc máy laptop không có F1)
        float btnW = 185f;
        float btnH = 34f;
        Rect toggleBtnRect = new Rect(Screen.width - btnW - 15f, 12f, btnW, btnH);
        string toggleLabel = isVisible ? "✕ CLOSE DEMO [F1]" : "🛠 DEMO MENU [F1]";
        GUIStyle tStyle = isVisible ? dangerButtonStyle : successButtonStyle;

        if (GUI.Button(toggleBtnRect, toggleLabel, tStyle))
        {
            isVisible = !isVisible;
        }

        // 2. Cửa sổ điều khiển chính
        if (isVisible)
        {
            windowRect = GUI.Window(9999, windowRect, DrawWindowContent, "⚔ PRU213 - PRESENTATION CHEAT & DEMO PANEL ⚔", windowStyle);
        }
    }

    private void DrawWindowContent(int windowID)
    {
        GUILayout.Space(6);

        // Header ghi chú
        GUILayout.Label("Công cụ hỗ trợ Giảng viên & Nhóm kiểm thử tính năng tức thì", infoBoxStyle);
        GUILayout.Space(4);

        // Thanh chọn Tab
        GUILayout.BeginHorizontal();
        for (int i = 0; i < tabNames.Length; i++)
        {
            GUIStyle btnStyle = (selectedTab == i) ? activeTabButtonStyle : tabButtonStyle;
            if (GUILayout.Button(tabNames[i], btnStyle, GUILayout.Height(34)))
            {
                selectedTab = i;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        // Nội dung theo Tab
        switch (selectedTab)
        {
            case 0:
                DrawPlayerTab();
                break;
            case 1:
                DrawWeaponsTab();
                break;
            case 2:
                DrawBossesTab();
                break;
            case 3:
                DrawUtilitiesTab();
                break;
        }

        GUILayout.FlexibleSpace();

        // Nút đóng ở chân bảng
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("✕ ĐÓNG BẢNG CHEAT (F1)", dangerButtonStyle, GUILayout.Height(32)))
        {
            isVisible = false;
        }
        GUILayout.EndHorizontal();

        // Cho phép kéo thả cửa sổ tự do
        GUI.DragWindow(new Rect(0, 0, windowRect.width, 30));
    }

    // =========================================================================
    // TAB 0: PLAYER & STATS
    // =========================================================================
    private void DrawPlayerTab()
    {
        GUILayout.Label("✦ TRẠNG THÁI & CHỈ SỐ NHÂN VẬT ✦", headerStyle);

        // God Mode Toggle
        string godLabel = PlayerHealthController.isGodMode ? "🛡 GOD MODE: ĐANG BẬT (BẤT TỬ)" : "🛡 BẬT GOD MODE (BẤT TỬ)";
        GUIStyle godStyle = PlayerHealthController.isGodMode ? successButtonStyle : actionButtonStyle;
        if (GUILayout.Button(godLabel, godStyle, GUILayout.Height(36)))
        {
            PlayerHealthController.isGodMode = !PlayerHealthController.isGodMode;
            if (PlayerHealthController.isGodMode && PlayerHealthController.instance != null)
            {
                PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
                PlayerHealthController.instance.UpdateHealthUI();
            }
        }

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("❤️ Hồi Đầy Máu (Full HP)", actionButtonStyle, GUILayout.Height(32)))
        {
            if (PlayerHealthController.instance != null)
            {
                PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
                PlayerHealthController.instance.UpdateHealthUI();
            }
        }
        if (GUILayout.Button("⚡ Hồi 100% Nộ Chiêu Cuối", actionButtonStyle, GUILayout.Height(32)))
        {
            if (PlayerUltimateController.instance != null)
            {
                PlayerUltimateController.instance.AddRage(PlayerUltimateController.instance.maxRage);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("💰 +1,000 Vàng", actionButtonStyle, GUILayout.Height(32)))
        {
            if (CoinController.instance != null) CoinController.instance.AddCoins(1000);
        }
        if (GUILayout.Button("💰 +10,000 Vàng (Test Unlock)", successButtonStyle, GUILayout.Height(32)))
        {
            if (CoinController.instance != null) CoinController.instance.AddCoins(10000);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("⭐ Lên +1 Level (Bật Thẻ Chọn)", actionButtonStyle, GUILayout.Height(32)))
        {
            AddLevels(1);
        }
        if (GUILayout.Button("⭐ Lên +5 Level", actionButtonStyle, GUILayout.Height(32)))
        {
            AddLevels(5);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("💨 Reset Cooldown Dash (Lướt ngay)", actionButtonStyle, GUILayout.Height(32)))
        {
            if (PlayerController.instance != null) PlayerController.instance.ResetDashCooldown();
        }
        if (GUILayout.Button("👟 +50% Tốc Độ Di Chuyển", actionButtonStyle, GUILayout.Height(32)))
        {
            if (PlayerController.instance != null) PlayerController.instance.AddMoveSpeedBonus(1.8f);
        }
        GUILayout.EndHorizontal();
    }

    // =========================================================================
    // TAB 1: WEAPONS & EVOLUTIONS
    // =========================================================================
    private void DrawWeaponsTab()
    {
        GUILayout.Label("✦ VŨ KHÍ & TIẾN HÓA THẦN THOẠI ✦", headerStyle);

        if (GUILayout.Button("⚡ MAX CẤP TẤT CẢ VŨ KHÍ ĐANG TRANG BỊ", successButtonStyle, GUILayout.Height(36)))
        {
            MaxAllEquippedWeapons();
        }

        GUILayout.Space(4);

        if (GUILayout.Button("⚔ MỞ KHÓA TOÀN BỘ VŨ KHÍ CHƯA DÙNG", actionButtonStyle, GUILayout.Height(34)))
        {
            EquipAllAvailableWeapons();
        }

        GUILayout.Space(8);
        GUILayout.Label("✦ TIẾN HÓA VŨ KHÍ TỨC THÌ (DEMO RECIPES) ✦", headerStyle);

        if (GUILayout.Button("🔥 KÍCH HOẠT TIẾN HÓA THẺ ĐẦU TIÊN KHẢ DỤNG", successButtonStyle, GUILayout.Height(36)))
        {
            TriggerFirstAvailableEvolution();
        }

        GUILayout.Space(4);

        GUILayout.Label("Công thức tiến hóa có sẵn trong game:\n• Sword + Sát Thương -> Thousand Blades (Kiếm Bão)\n• Fireball + Tầm Hút -> Solar Flare Ring (Vành Hào Quang)", infoBoxStyle);
    }

    // =========================================================================
    // TAB 2: SUMMON BOSSES & EVENTS
    // =========================================================================
    private void DrawBossesTab()
    {
        GUILayout.Label("✦ TRIỆU HỒI TRỰC TIẾP 5 GIAI ĐOẠN BOSS ✦", headerStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🐝 Boss 1: Brood Queen\n<size=11>(Stage 1 - 2:00)</size>", actionButtonStyle, GUILayout.Height(42)))
        {
            SpawnBoss(BossType.BroodQueen);
        }
        if (GUILayout.Button("🐺 Boss 2: Fenrir\n<size=11>(Stage 2 - 5:00)</size>", actionButtonStyle, GUILayout.Height(42)))
        {
            SpawnBoss(BossType.FrostfangFenrir);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("💀 Boss 3: Skeleton Warlord\n<size=11>(Stage 3 - 8:00)</size>", actionButtonStyle, GUILayout.Height(42)))
        {
            SpawnBoss(BossType.SkeletonWarlord);
        }
        if (GUILayout.Button("👻 Boss 4: Death Phantom\n<size=11>(Stage 4 - 11:30)</size>", actionButtonStyle, GUILayout.Height(42)))
        {
            SpawnBoss(BossType.DeathPhantom);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        if (GUILayout.Button("🐉 BOSS CUỐI: GOLDEN DRAGON (15:00)\n<size=11>(Tiêu diệt để xem Màn hình Chiến Thắng Victory)</size>", successButtonStyle, GUILayout.Height(46)))
        {
            SpawnBoss(BossType.GoldenDragon);
        }

        GUILayout.Space(8);
        GUILayout.Label("✦ SỰ KIỆN ĐỘT BIẾN & ĐÀN TẾ CỔ ✦", headerStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("👹 Triệu hồi Mini-Boss", actionButtonStyle, GUILayout.Height(34)))
        {
            if (EnemySpawner.instance != null) EnemySpawner.instance.SpawnMiniBossEvent();
        }
        if (GUILayout.Button("🏛 Xuất hiện Đàn Tế (Shrine)", actionButtonStyle, GUILayout.Height(34)))
        {
            if (ShrineManager.instance != null) ShrineManager.instance.SpawnRandomShrine();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🌊 Bầy Quái Ùa Vào (Horde)", actionButtonStyle, GUILayout.Height(34)))
        {
            if (EnemySpawner.instance != null) EnemySpawner.instance.TriggerHordeRush();
        }
        if (GUILayout.Button("⭕ Vòng Vây Bao Bọc (Ambush)", actionButtonStyle, GUILayout.Height(34)))
        {
            if (EnemySpawner.instance != null) EnemySpawner.instance.TriggerCircleAmbush();
        }
        GUILayout.EndHorizontal();
    }

    // =========================================================================
    // TAB 3: GAME FLOW, UTILITIES & SETTINGS
    // =========================================================================
    private void DrawUtilitiesTab()
    {
        GUILayout.Label("✦ ĐIỀU CHỈNH TỐC ĐỘ GAME ✦", headerStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(Time.timeScale == 1f ? "▶ Chuẩn 1x" : "1x", Time.timeScale == 1f ? successButtonStyle : actionButtonStyle, GUILayout.Height(32)))
        {
            Time.timeScale = 1f;
        }
        if (GUILayout.Button(Time.timeScale == 2f ? "▶ Nhanh 2x" : "2x", Time.timeScale == 2f ? successButtonStyle : actionButtonStyle, GUILayout.Height(32)))
        {
            Time.timeScale = 2f;
        }
        if (GUILayout.Button(Time.timeScale == 3f ? "▶ Cực Nhanh 3x" : "3x", Time.timeScale == 3f ? successButtonStyle : actionButtonStyle, GUILayout.Height(32)))
        {
            Time.timeScale = 3f;
        }
        if (GUILayout.Button(Time.timeScale == 5f ? "▶ Siêu Tốc 5x" : "5x", Time.timeScale == 5f ? successButtonStyle : actionButtonStyle, GUILayout.Height(32)))
        {
            Time.timeScale = 5f;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("✦ DỌN DẸP & THU THẬP ✦", headerStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("💥 QUÉT SẠCH QUÁI (NUKE SCREEN)", dangerButtonStyle, GUILayout.Height(34)))
        {
            if (EnemySpawner.instance != null) EnemySpawner.instance.KillAllEnemies(true);
        }
        if (GUILayout.Button("🧲 HÚT TOÀN BỘ NGỌC & VÀNG", successButtonStyle, GUILayout.Height(34)))
        {
            if (ExpManager.instance != null) ExpManager.instance.AttractAllGems();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("✦ KẾT QUẢ TRẬN ĐẤU (TEST SCREENS) ✦", headerStyle);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("🏆 BẬT MÀN HÌNH VICTORY (THẮNG)", successButtonStyle, GUILayout.Height(36)))
        {
            if (LevelManager.instance != null) LevelManager.instance.TriggerVictory(0.2f);
        }
        if (GUILayout.Button("☠ BẬT MÀN HÌNH GAME OVER (THUA)", dangerButtonStyle, GUILayout.Height(36)))
        {
            if (LevelManager.instance != null) LevelManager.instance.EndLevel();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("✦ CÀI ĐẶT ÂM LƯỢNG & MÀN HÌNH ✦", headerStyle);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Âm lượng Tổng: {(int)(GameSettings.MasterVolume * 100)}%", GUILayout.Width(155));
        float newMaster = GUILayout.HorizontalSlider(GameSettings.MasterVolume, 0f, 1f);
        if (Mathf.Abs(newMaster - GameSettings.MasterVolume) > 0.01f)
        {
            GameSettings.MasterVolume = newMaster;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Nhạc Nền (BGM): {(int)(GameSettings.BGMVolume * 100)}%", GUILayout.Width(155));
        float newBgm = GUILayout.HorizontalSlider(GameSettings.BGMVolume, 0f, 1f);
        if (Mathf.Abs(newBgm - GameSettings.BGMVolume) > 0.01f)
        {
            GameSettings.BGMVolume = newBgm;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Hiệu Ứng (SFX): {(int)(GameSettings.SFXVolume * 100)}%", GUILayout.Width(155));
        float newSfx = GUILayout.HorizontalSlider(GameSettings.SFXVolume, 0f, 1f);
        if (Mathf.Abs(newSfx - GameSettings.SFXVolume) > 0.01f)
        {
            GameSettings.SFXVolume = newSfx;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        string fsLabel = Screen.fullScreen ? "🖥 Chế độ: Toàn Màn Hình [ON]" : "🪟 Chế độ: Cửa Sổ [Windowed]";
        if (GUILayout.Button(fsLabel, actionButtonStyle, GUILayout.Height(30)))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        if (GUILayout.Button("💾 Lưu Trận Tức Thì", actionButtonStyle, GUILayout.Height(30)))
        {
            RunSave.Save();
            if (UIController.instance != null) UIController.instance.ShowWarning("[ RUN PROGRESS SAVED! ]", 2.0f);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);
        GUILayout.Label("âœ¦ CHá»ŒN Báº¢N Äá»’ & Äá»˜ KHÃ“ (STAGE SELECTION) âœ¦", headerStyle);

        string mapLabel = (MapSelectionManager.CurrentStage == MapStage.HauntedCatacombs)
            ? "ðŸ—ºï¸ Báº¢N Äá»’: CATACOMBS (NORMAL) -> Báº¤M Äá»”I SANG INFERNO (HARD)"
            : "ðŸŒ‹ Báº¢N Äá»’: INFERNO PURGATORY (HARD) -> Báº¤M Äá»”I SANG CATACOMBS";
        GUIStyle mapBtnStyle = (MapSelectionManager.CurrentStage == MapStage.HauntedCatacombs) ? actionButtonStyle : dangerButtonStyle;
        if (GUILayout.Button(mapLabel, mapBtnStyle, GUILayout.Height(34)))
        {
            MapSelectionManager.ToggleStage();
            InfiniteBackground bg = FindAnyObjectByType<InfiniteBackground>();
            if (bg != null)
            {
                SpriteRenderer bgSr = bg.GetComponent<SpriteRenderer>();
                Transform vignette = Camera.main != null ? Camera.main.transform.Find("Atmospheric_Vignette") : null;
                SpriteRenderer vsr = vignette != null ? vignette.GetComponent<SpriteRenderer>() : null;
                MapSelectionManager.ApplyMapVisuals(bgSr, vsr);
            }
            if (UIController.instance != null)
            {
                UIController.instance.ShowWarning($"[ STAGE SWITCHED: {MapSelectionManager.CurrentStageInfo.displayName} ]", 2.2f);
            }
        }

        GUILayout.Space(6);
        GUILayout.Label("âœ¦ KIá»‚M THá»¬ HIá»†U á»¨NG VFX & THá»Š GIÃC âœ¦", headerStyle);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("âœ¨ Bá»¥i Sao (Sparkles)", actionButtonStyle, GUILayout.Height(30)))
        {
            Vector3 p = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
            VFXImpactManager.PlayPickupSparkles(p, new Color(1f, 0.88f, 0.25f), 12);
        }
        if (GUILayout.Button("ðŸ’¥ Máº£nh Vá»¥n (Debris)", actionButtonStyle, GUILayout.Height(30)))
        {
            Vector3 p = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
            VFXImpactManager.PlayDebrisExplosion(p, new Color(0.75f, 0.65f, 0.5f), 14);
        }
        if (GUILayout.Button("ðŸ’« SÃ³ng KÃ­ch (Shockwave)", actionButtonStyle, GUILayout.Height(30)))
        {
            Vector3 p = PlayerController.instance != null ? PlayerController.instance.transform.position : Vector3.zero;
            VFXImpactManager.PlayShockwaveRing(p, new Color(0.2f, 0.9f, 1f), 3.5f, 0.4f);
        }
        GUILayout.EndHorizontal();
    }

    // =========================================================================
    // LOGIC HELPER METHODS
    // =========================================================================

    private void AddLevels(int amount)
    {
        if (ExperienceLevelController.instance == null) return;

        for (int i = 0; i < amount; i++)
        {
            int curLvl = ExperienceLevelController.instance.currentLevel;
            var list = ExperienceLevelController.instance.expLevels;
            if (list != null && curLvl < list.Count)
            {
                int needed = list[curLvl] - ExperienceLevelController.instance.currentExperience + 1;
                ExperienceLevelController.instance.GetExp(Mathf.Max(needed, 10));
            }
        }
    }

    private void MaxAllEquippedWeapons()
    {
        if (PlayerController.instance == null) return;

        List<Weapon> copy = new List<Weapon>(PlayerController.instance.assignedWeapons);
        foreach (var w in copy)
        {
            if (w != null && w.stats != null)
            {
                while (w.weaponLevel < w.stats.Count - 1)
                {
                    w.LevelUp();
                }
            }
        }

        if (UIController.instance != null)
        {
            UIController.instance.UpdateWeaponSlots();
            UIController.instance.ShowWarning("[ ALL WEAPONS REACHED MAX LEVEL! ]", 2.0f);
        }
    }

    private void EquipAllAvailableWeapons()
    {
        if (PlayerController.instance == null) return;

        while (PlayerController.instance.unassignedWeapons != null && PlayerController.instance.unassignedWeapons.Count > 0)
        {
            PlayerController.instance.AddWeapon(0);
        }

        if (UIController.instance != null)
        {
            UIController.instance.UpdateWeaponSlots();
        }
    }

    private void TriggerFirstAvailableEvolution()
    {
        if (UpgradeManager.instance == null) return;

        // Ưu tiên nạp và tìm thẻ tiến hóa
        UpgradeManager.instance.EnsureInitialized();
        foreach (var card in UpgradeManager.instance.upgradeDatabase)
        {
            if (card is WeaponEvolutionUpgradeData evo && !evo.isEvolved)
            {
                // Tự động thỏa mãn điều kiện cấp tối đa cho vũ khí yêu cầu
                if (PlayerController.instance != null)
                {
                    foreach (var w in PlayerController.instance.assignedWeapons)
                    {
                        if (w != null && (w.name.Contains(evo.requiredWeaponName) || evo.requiredWeaponName.Contains(w.name)))
                        {
                            while (w.weaponLevel < w.stats.Count - 1) w.LevelUp();
                        }
                    }
                }

                // Tiến hóa
                evo.ApplyUpgrade();
                if (UIController.instance != null)
                {
                    UIController.instance.ShowWarning($"✦ EVOLUTION UNLOCKED: {evo.upgradeName}! ✦", 3.0f);
                    UIController.instance.UpdateWeaponSlots();
                }
                return;
            }
        }

        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("Không tìm thấy thẻ tiến hóa phù hợp với vũ khí hiện có!", 2.0f);
        }
    }

    private void SpawnBoss(BossType type)
    {
        if (EnemySpawner.instance != null)
        {
            EnemySpawner.instance.SpawnBossDirectly(type);
        }
        else
        {
            Debug.LogWarning("[DemoPanel] EnemySpawner instance is null!");
        }
    }

    // =========================================================================
    // GUI STYLES INITIALIZATION
    // =========================================================================

    private void InitStylesIfNeeded()
    {
        if (stylesInitialized) return;

        windowBgTex = MakeColorTex(1, 1, new Color(0.08f, 0.08f, 0.12f, 0.96f));
        buttonBgTex = MakeColorTex(1, 1, new Color(0.18f, 0.20f, 0.28f, 1f));
        buttonActiveTex = MakeColorTex(1, 1, new Color(0.85f, 0.68f, 0.20f, 1f));
        greenTex = MakeColorTex(1, 1, new Color(0.12f, 0.45f, 0.22f, 1f));
        redTex = MakeColorTex(1, 1, new Color(0.55f, 0.12f, 0.15f, 1f));

        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = windowBgTex;
        windowStyle.normal.textColor = new Color(1f, 0.90f, 0.45f);
        windowStyle.fontSize = 14;
        windowStyle.fontStyle = FontStyle.Bold;
        windowStyle.alignment = TextAnchor.UpperCenter;
        windowStyle.padding = new RectOffset(14, 14, 28, 14);

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.normal.textColor = new Color(1f, 0.85f, 0.25f);
        headerStyle.fontSize = 12;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;

        tabButtonStyle = new GUIStyle(GUI.skin.button);
        tabButtonStyle.normal.background = buttonBgTex;
        tabButtonStyle.normal.textColor = new Color(0.85f, 0.85f, 0.90f);
        tabButtonStyle.fontSize = 12;
        tabButtonStyle.fontStyle = FontStyle.Bold;

        activeTabButtonStyle = new GUIStyle(GUI.skin.button);
        activeTabButtonStyle.normal.background = buttonActiveTex;
        activeTabButtonStyle.normal.textColor = new Color(0.1f, 0.08f, 0.02f);
        activeTabButtonStyle.fontSize = 12;
        activeTabButtonStyle.fontStyle = FontStyle.Bold;

        actionButtonStyle = new GUIStyle(GUI.skin.button);
        actionButtonStyle.normal.background = buttonBgTex;
        actionButtonStyle.normal.textColor = Color.white;
        actionButtonStyle.fontSize = 11;
        actionButtonStyle.fontStyle = FontStyle.Bold;

        successButtonStyle = new GUIStyle(GUI.skin.button);
        successButtonStyle.normal.background = greenTex;
        successButtonStyle.normal.textColor = Color.white;
        successButtonStyle.fontSize = 11;
        successButtonStyle.fontStyle = FontStyle.Bold;

        dangerButtonStyle = new GUIStyle(GUI.skin.button);
        dangerButtonStyle.normal.background = redTex;
        dangerButtonStyle.normal.textColor = Color.white;
        dangerButtonStyle.fontSize = 11;
        dangerButtonStyle.fontStyle = FontStyle.Bold;

        infoBoxStyle = new GUIStyle(GUI.skin.box);
        infoBoxStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f);
        infoBoxStyle.fontSize = 11;
        infoBoxStyle.alignment = TextAnchor.MiddleCenter;
        infoBoxStyle.wordWrap = true;

        stylesInitialized = true;
    }

    private Texture2D MakeColorTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
