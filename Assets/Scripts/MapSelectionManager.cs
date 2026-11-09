using System;
using UnityEngine;

public enum MapStage
{
    HauntedCatacombs = 0, // Nghĩa địa rừng thẳm (Normal Mode)
    InfernoPurgatory = 1  // Hỏa ngục huyết nguyệt (Hard Mode / Cursed Lava)
}

[Serializable]
public class MapStageInfo
{
    public MapStage stage;
    public string displayName;
    public string subtitle;
    public string description;
    public Color backgroundTint;
    public Color vignetteColor;
    public Color emberColor;
    public float enemySpeedMultiplier;
    public float enemyHealthMultiplier;
    public float goldDropMultiplier;
}

/// <summary>
/// Quản lý hệ thống Chọn Bản Đồ (Stage / Map Selection) và độ khó tương ứng.
/// Cho phép người chơi trải nghiệm nhiều chiều không gian chiến đấu với hiệu ứng thị giác và chỉ số quái biến đổi.
/// </summary>
public static class MapSelectionManager
{
    private const string MAP_KEY = "SelectedMapStageIndex";

    public static readonly MapStageInfo[] Stages = new MapStageInfo[]
    {
        new MapStageInfo
        {
            stage = MapStage.HauntedCatacombs,
            displayName = "THE HAUNTED CATACOMBS",
            subtitle = "✦ STAGE I: CATACOMBS (NORMAL MODE) ✦",
            description = "Nghĩa địa rêu phong cổ kính. Quái vật tiêu chuẩn, nhịp độ cân bằng.",
            backgroundTint = new Color(0.85f, 0.92f, 0.90f, 1f),
            vignetteColor = new Color(0.02f, 0.04f, 0.07f, 0.45f),
            emberColor = new Color(0.35f, 0.95f, 0.65f, 0.65f),
            enemySpeedMultiplier = 1.0f,
            enemyHealthMultiplier = 1.0f,
            goldDropMultiplier = 1.0f
        },
        new MapStageInfo
        {
            stage = MapStage.InfernoPurgatory,
            displayName = "INFERNO PURGATORY",
            subtitle = "✦ STAGE II: INFERNO PURGATORY (HARD MODE • +50% GOLD) ✦",
            description = "Vùng đất dung nham huyết tế rực lửa. Quái chạy nhanh +25%, máu +30%, rơi vàng gấp x1.5!",
            backgroundTint = new Color(1.0f, 0.45f, 0.38f, 1f),
            vignetteColor = new Color(0.25f, 0.02f, 0.04f, 0.58f),
            emberColor = new Color(1.0f, 0.35f, 0.15f, 0.85f),
            enemySpeedMultiplier = 1.25f,
            enemyHealthMultiplier = 1.30f,
            goldDropMultiplier = 1.50f
        }
    };

    public static MapStage CurrentStage
    {
        get => (MapStage)PlayerPrefs.GetInt(MAP_KEY, 0);
        set
        {
            PlayerPrefs.SetInt(MAP_KEY, (int)value);
            PlayerPrefs.Save();
        }
    }

    public static MapStageInfo CurrentStageInfo
    {
        get
        {
            int idx = Mathf.Clamp((int)CurrentStage, 0, Stages.Length - 1);
            return Stages[idx];
        }
    }

    public static void ToggleStage()
    {
        CurrentStage = (CurrentStage == MapStage.HauntedCatacombs) ? MapStage.InfernoPurgatory : MapStage.HauntedCatacombs;
    }

    public static float GetEnemySpeedMultiplier() => CurrentStageInfo.enemySpeedMultiplier;
    public static float GetEnemyHealthMultiplier() => CurrentStageInfo.enemyHealthMultiplier;
    public static float GetGoldMultiplier() => CurrentStageInfo.goldDropMultiplier;

    /// <summary>
    /// Áp dụng bảng màu nền, sương mù và tàn lửa theo Map đang chọn vào thế giới
    /// </summary>
    public static void ApplyMapVisuals(SpriteRenderer bgSr, SpriteRenderer vignetteSr)
    {
        MapStageInfo info = CurrentStageInfo;
        if (bgSr != null)
        {
            bgSr.color = info.backgroundTint;
        }
        if (vignetteSr != null)
        {
            vignetteSr.color = info.vignetteColor;
        }
    }
}
