using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    [CreateAssetMenu(fileName = "UnlockWeapon_New", menuName = "Survivors/Upgrades/Unlock Weapon")]
    public class UnlockWeaponUpgradeData : UpgradeData
    {
        [Header("Weapon Config")]
        [Tooltip("Tên GameObject hoặc tên hiển thị của vũ khí tương ứng trong Player")]
        public string targetWeaponName;

        [Tooltip("Prefab tùy chọn nếu muốn sinh vũ khí mới thay vì lấy từ unassignedWeapons")]
        public Weapon weaponPrefab;

        private void Awake()
        {
            upgradeType = UpgradeType.UnlockWeapon;
        }

        public override bool IsAvailable()
        {
            if (PlayerController.instance == null) return false;

            // 1. Kiểm tra giới hạn số lượng vũ khí tối đa của nhân vật
            int currentSlots = PlayerController.instance.assignedWeapons.Count + PlayerController.instance.fullyLevelledWeapons.Count;
            if (currentSlots >= PlayerController.instance.maxWeapons)
            {
                return false;
            }

            // 2. Không mở khóa nếu người chơi đã sở hữu vũ khí này rồi
            if (IsWeaponAlreadyEquipped(targetWeaponName))
            {
                return false;
            }

            // 3. Phải có sẵn trong unassignedWeapons hoặc có prefab để sinh mới
            return FindUnassignedWeapon(targetWeaponName) != null || weaponPrefab != null;
        }

        public override void ApplyUpgrade()
        {
            if (PlayerController.instance == null) return;

            // Tìm trong danh sách unassignedWeapons trên nhân vật
            Weapon unassigned = FindUnassignedWeapon(targetWeaponName);
            if (unassigned != null)
            {
                PlayerController.instance.AddWeapon(unassigned);
                return;
            }

            // Nếu dùng prefab sinh mới
            if (weaponPrefab != null)
            {
                Weapon newWeapon = Instantiate(weaponPrefab, PlayerController.instance.transform);
                newWeapon.name = weaponPrefab.name;
                PlayerController.instance.AddWeapon(newWeapon);
            }
        }

        public override string GetDisplayName()
        {
            return $"Unlock: {upgradeName}";
        }

        public override string GetDescription()
        {
            if (!string.IsNullOrEmpty(baseDescription)) return baseDescription;
            return $"Acquires the {upgradeName} to attack surrounding enemies.";
        }

        public override Sprite GetIcon()
        {
            if (icon != null) return icon;
            Weapon w = FindUnassignedWeapon(targetWeaponName);
            if (w != null && w.icon != null) return w.icon;
            if (weaponPrefab != null && weaponPrefab.icon != null) return weaponPrefab.icon;
            return null;
        }

        private bool IsWeaponAlreadyEquipped(string wName)
        {
            if (string.IsNullOrEmpty(wName)) return false;

            foreach (var w in PlayerController.instance.assignedWeapons)
            {
                if (w != null && (w.name.Contains(wName) || wName.Contains(w.name))) return true;
            }

            foreach (var w in PlayerController.instance.fullyLevelledWeapons)
            {
                if (w != null && (w.name.Contains(wName) || wName.Contains(w.name))) return true;
            }

            return false;
        }

        private Weapon FindUnassignedWeapon(string wName)
        {
            if (string.IsNullOrEmpty(wName) || PlayerController.instance == null) return null;

            foreach (var w in PlayerController.instance.unassignedWeapons)
            {
                if (w != null && (w.name.Contains(wName) || wName.Contains(w.name)))
                {
                    return w;
                }
            }
            return null;
        }
    }
}
