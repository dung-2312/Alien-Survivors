using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradeWeapon_New", menuName = "Survivors/Upgrades/Upgrade Weapon")]
    public class UpgradeWeaponUpgradeData : UpgradeData
    {
        [Header("Target Weapon")]
        [Tooltip("Tên vũ khí tương ứng trong danh sách assignedWeapons của Player")]
        public string targetWeaponName;

        private void Awake()
        {
            upgradeType = UpgradeType.UpgradeWeapon;
        }

        public override bool IsAvailable()
        {
            if (PlayerController.instance == null) return false;

            Weapon weapon = FindAssignedWeapon();
            if (weapon == null) return false;

            // Kiểm tra xem vũ khí đã max cấp chưa
            return weapon.weaponLevel < weapon.stats.Count - 1;
        }

        public override void ApplyUpgrade()
        {
            Weapon weapon = FindAssignedWeapon();
            if (weapon != null)
            {
                weapon.LevelUp();
            }
        }

        public override string GetDisplayName()
        {
            Weapon weapon = FindAssignedWeapon();
            if (weapon != null)
            {
                return $"{upgradeName} - Lvl {weapon.weaponLevel + 1}";
            }
            return $"{upgradeName} - Upgrade";
        }

        public override string GetDescription()
        {
            Weapon weapon = FindAssignedWeapon();
            if (weapon != null && weapon.stats != null && weapon.weaponLevel < weapon.stats.Count)
            {
                string text = weapon.stats[weapon.weaponLevel].upgradeText;
                if (!string.IsNullOrEmpty(text)) return text;
            }

            if (!string.IsNullOrEmpty(baseDescription)) return baseDescription;
            return $"Enhances damage, speed, or range of {upgradeName}.";
        }

        public override Sprite GetIcon()
        {
            if (icon != null) return icon;
            Weapon weapon = FindAssignedWeapon();
            if (weapon != null && weapon.icon != null) return weapon.icon;
            return null;
        }

        private Weapon FindAssignedWeapon()
        {
            if (string.IsNullOrEmpty(targetWeaponName) || PlayerController.instance == null) return null;

            foreach (var w in PlayerController.instance.assignedWeapons)
            {
                if (w != null && (w.name.Contains(targetWeaponName) || targetWeaponName.Contains(w.name)))
                {
                    return w;
                }
            }
            return null;
        }
    }
}
