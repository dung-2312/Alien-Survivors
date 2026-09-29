namespace AlienSurvivors.Upgrades
{
    public enum UpgradeType
    {
        UnlockWeapon,       // Mở khóa vũ khí mới vào slot trống
        UpgradeWeapon,      // Tăng cấp độ vũ khí hiện có
        UpgradeStat,        // Tăng chỉ số nhân vật (Passive Stat)
        WeaponEvolution     // Tiến hóa vũ khí khi đạt đủ điều kiện Synergy
    }

    public enum StatUpgradeType
    {
        MoveSpeed,          // Tốc độ di chuyển
        PickupRange,        // Tầm hút kinh nghiệm / vật phẩm
        MaxHealth,          // Máu tối đa
        MaxWeapons,         // Giới hạn số vũ khí mang theo
        DamageMultiplier,   // Hệ số sát thương toàn diện
        CooldownReduction   // Giảm thời gian hồi chiêu
    }
}
