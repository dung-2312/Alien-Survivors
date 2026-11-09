using System;
using System.Collections.Generic;
using UnityEngine;

public enum MetaStat
{
    Might,      // +5% Sát thương / cấp (Max 5: +25%)
    Health,     // +15 Máu tối đa / cấp (Max 5: +75 HP)
    Recovery,   // +0.4 HP hồi máu mỗi giây (Max 5: +2.0 HP/s)
    Swiftness,  // +5% Tốc độ chạy / cấp (Max 5: +25%)
    Magnet,     // +0.6 Tầm hút ngọc & vàng / cấp (Max 5: +3.0)
    Greed,      // +10% Vàng thu được / cấp (Max 5: +50%)
    Crit,       // +3% Tỷ lệ bạo kích / cấp (Max 5: +15%)
    Revival     // 1 Mạng hồi sinh khi hết máu với 50% HP (Max 1)
}

[Serializable]
public class MetaStatInfo
{
    public MetaStat stat;
    public string displayName;
    public string description;
    public int maxLevel;
    public int[] costs;
    public string bonusPerLevel;
}

/// <summary>
/// Quản lý hệ thống nâng cấp vĩnh viễn (Power-Up Meta Shop) giữa các lượt chơi.
/// Dùng số vàng tích lũy được lưu trong PlayerPrefs để tăng cường sức mạnh vĩnh viễn.
/// </summary>
public static class MetaProgression
{
    private const string PREFIX = "MetaUpgrade_";
    private const string TOTAL_SPENT_KEY = "MetaTotalSpentGold";

    public static readonly Dictionary<MetaStat, MetaStatInfo> StatDatabase = new Dictionary<MetaStat, MetaStatInfo>
    {
        {
            MetaStat.Might,
            new MetaStatInfo
            {
                stat = MetaStat.Might,
                displayName = "MIGHT (SỨC MẠNH)",
                description = "Tăng toàn bộ sát thương vũ khí gây ra",
                maxLevel = 5,
                costs = new int[] { 200, 450, 900, 1600, 2800 },
                bonusPerLevel = "+5% Sát thương / cấp"
            }
        },
        {
            MetaStat.Health,
            new MetaStatInfo
            {
                stat = MetaStat.Health,
                displayName = "VITALITY (SINH MỆNH)",
                description = "Tăng thanh máu tối đa của nhân vật",
                maxLevel = 5,
                costs = new int[] { 150, 350, 750, 1400, 2400 },
                bonusPerLevel = "+15 Max HP / cấp"
            }
        },
        {
            MetaStat.Recovery,
            new MetaStatInfo
            {
                stat = MetaStat.Recovery,
                displayName = "RECOVERY (HỒI PHỤC)",
                description = "Tự động tái tạo sinh lực mỗi giây",
                maxLevel = 5,
                costs = new int[] { 250, 550, 1100, 2000, 3200 },
                bonusPerLevel = "+0.4 HP / giây / cấp"
            }
        },
        {
            MetaStat.Swiftness,
            new MetaStatInfo
            {
                stat = MetaStat.Swiftness,
                displayName = "SWIFTNESS (NHANH NHẸN)",
                description = "Tăng tốc độ di chuyển cơ bản",
                maxLevel = 5,
                costs = new int[] { 180, 400, 850, 1500, 2600 },
                bonusPerLevel = "+5% Tốc độ chạy / cấp"
            }
        },
        {
            MetaStat.Magnet,
            new MetaStatInfo
            {
                stat = MetaStat.Magnet,
                displayName = "MAGNET (HÀO QUANG)",
                description = "Mở rộng tầm hút ngọc kinh nghiệm và vàng",
                maxLevel = 5,
                costs = new int[] { 120, 300, 650, 1200, 2200 },
                bonusPerLevel = "+0.6 Tầm nhặt / cấp"
            }
        },
        {
            MetaStat.Greed,
            new MetaStatInfo
            {
                stat = MetaStat.Greed,
                displayName = "GREED (HOÀNG KIM PHÚC)",
                description = "Tăng số vàng rơi ra từ quái vật và hòm kho báu",
                maxLevel = 5,
                costs = new int[] { 200, 500, 1000, 1800, 3000 },
                bonusPerLevel = "+10% Vàng thu được / cấp"
            }
        },
        {
            MetaStat.Crit,
            new MetaStatInfo
            {
                stat = MetaStat.Crit,
                displayName = "CRITICAL (BẠO KÍCH)",
                description = "Tăng tỷ lệ đánh đòn chí mạng gây x2 sát thương",
                maxLevel = 5,
                costs = new int[] { 220, 500, 1050, 1900, 3100 },
                bonusPerLevel = "+3% Tỷ lệ Crit / cấp"
            }
        },
        {
            MetaStat.Revival,
            new MetaStatInfo
            {
                stat = MetaStat.Revival,
                displayName = "REVIVAL (HỒI SINH)",
                description = "Hồi sinh ngay tại chỗ với 50% HP khi cạn máu",
                maxLevel = 1,
                costs = new int[] { 3500 },
                bonusPerLevel = "1 Lần Hồi Sinh / trận đấu"
            }
        }
    };

    public static int GetLevel(MetaStat stat)
    {
        return PlayerPrefs.GetInt(PREFIX + stat.ToString(), 0);
    }

    public static int GetCost(MetaStat stat)
    {
        int lvl = GetLevel(stat);
        if (!StatDatabase.TryGetValue(stat, out MetaStatInfo info)) return 999999;
        if (lvl >= info.maxLevel) return 0; // Max level
        return info.costs[Mathf.Clamp(lvl, 0, info.costs.Length - 1)];
    }

    public static bool CanUpgrade(MetaStat stat, int currentCoins)
    {
        int lvl = GetLevel(stat);
        if (!StatDatabase.TryGetValue(stat, out MetaStatInfo info)) return false;
        if (lvl >= info.maxLevel) return false;
        return currentCoins >= GetCost(stat);
    }

    public static bool PurchaseUpgrade(MetaStat stat)
    {
        int coins = PlayerPrefs.GetInt("SavedCoins", 0);
        int cost = GetCost(stat);
        if (!CanUpgrade(stat, coins)) return false;

        int lvl = GetLevel(stat);
        PlayerPrefs.SetInt(PREFIX + stat.ToString(), lvl + 1);
        PlayerPrefs.SetInt("SavedCoins", coins - cost);

        int totalSpent = PlayerPrefs.GetInt(TOTAL_SPENT_KEY, 0);
        PlayerPrefs.SetInt(TOTAL_SPENT_KEY, totalSpent + cost);

        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// Hoàn trả 100% số vàng đã tiêu vào Meta Upgrades để người chơi thoải mái thử nghiệm
    /// </summary>
    public static void RefundAll()
    {
        int totalSpent = PlayerPrefs.GetInt(TOTAL_SPENT_KEY, 0);
        int coins = PlayerPrefs.GetInt("SavedCoins", 0);

        PlayerPrefs.SetInt("SavedCoins", coins + totalSpent);
        PlayerPrefs.SetInt(TOTAL_SPENT_KEY, 0);

        foreach (MetaStat stat in Enum.GetValues(typeof(MetaStat)))
        {
            PlayerPrefs.DeleteKey(PREFIX + stat.ToString());
        }

        PlayerPrefs.Save();
    }

    // =========================================================================
    // STAT GETTERS (Áp dụng vào trận đấu)
    // =========================================================================

    public static float GetDamageMultiplier()
    {
        // Cấp 1 -> 5: +5% -> +25%
        return 1f + (GetLevel(MetaStat.Might) * 0.05f);
    }

    public static float GetBonusHealth()
    {
        // Cấp 1 -> 5: +15 -> +75 HP
        return GetLevel(MetaStat.Health) * 15f;
    }

    public static float GetBonusRegenPerSecond()
    {
        // Cấp 1 -> 5: +0.4 -> +2.0 HP/s
        return GetLevel(MetaStat.Recovery) * 0.4f;
    }

    public static float GetBonusSpeed()
    {
        // Cấp 1 -> 5: +5% -> +25%
        return GetLevel(MetaStat.Swiftness) * 0.05f;
    }

    public static float GetBonusPickupRange()
    {
        // Cấp 1 -> 5: +0.6 -> +3.0 units
        return GetLevel(MetaStat.Magnet) * 0.6f;
    }

    public static float GetGreedMultiplier()
    {
        // Cấp 1 -> 5: +10% -> +50% vàng
        return 1f + (GetLevel(MetaStat.Greed) * 0.10f);
    }

    public static float GetBonusCritChance()
    {
        // Cấp 1 -> 5: +3% -> +15%
        return GetLevel(MetaStat.Crit) * 0.03f;
    }

    public static bool HasRevivalAvailable(out int currentCharges)
    {
        currentCharges = GetLevel(MetaStat.Revival);
        return currentCharges > 0;
    }
}
