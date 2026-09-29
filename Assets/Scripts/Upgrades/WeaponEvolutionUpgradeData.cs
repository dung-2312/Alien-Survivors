using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    [CreateAssetMenu(fileName = "Evolution_New", menuName = "Survivors/Upgrades/Weapon Evolution")]
    public class WeaponEvolutionUpgradeData : UpgradeData
    {
        [Header("Synergy Condition (Điều kiện kết hợp)")]
        [Tooltip("Tên vũ khí yêu cầu để tiến hóa")]
        public string requiredWeaponName;

        [Tooltip("-1 nghĩa là yêu cầu đạt cấp tối đa (Max Level)")]
        public int requiredWeaponLevel = -1;

        [Tooltip("Thẻ chỉ số phụ trợ (Passive Item) bắt buộc đi kèm")]
        public UpgradeStatUpgradeData requiredPassiveStat;

        [Tooltip("Cấp độ tối thiểu của Passive Item cần có để kích hoạt")]
        public int requiredPassiveLevel = 1;

        [Header("Evolved Weapon Result (Vũ khí sau tiến hóa)")]
        public string evolvedWeaponName;
        public GameObject evolvedWeaponPrefab;

        [System.NonSerialized]
        public bool isEvolved = false;

        private void Awake()
        {
            upgradeType = UpgradeType.WeaponEvolution;
        }

        public override bool IsAvailable()
        {
            if (isEvolved || PlayerController.instance == null) return false;

            // 1. Kiểm tra vũ khí cơ bản đã đạt cấp yêu cầu (hoặc Max Level) chưa
            bool weaponConditionMet = false;

            // Kiểm tra trong danh sách vũ khí đã đạt max level
            foreach (var w in PlayerController.instance.fullyLevelledWeapons)
            {
                if (w != null && (w.name.Contains(requiredWeaponName) || requiredWeaponName.Contains(w.name)))
                {
                    weaponConditionMet = true;
                    break;
                }
            }

            // Hoặc kiểm tra trong danh sách đang trang bị nếu yêu cầu level cụ thể
            if (!weaponConditionMet)
            {
                foreach (var w in PlayerController.instance.assignedWeapons)
                {
                    if (w != null && (w.name.Contains(requiredWeaponName) || requiredWeaponName.Contains(w.name)))
                    {
                        if (requiredWeaponLevel == -1)
                        {
                            if (w.weaponLevel >= w.stats.Count - 1) weaponConditionMet = true;
                        }
                        else
                        {
                            if (w.weaponLevel >= requiredWeaponLevel) weaponConditionMet = true;
                        }
                        break;
                    }
                }
            }

            if (!weaponConditionMet) return false;

            // 2. Kiểm tra chỉ số phụ trợ (Passive Stat) đã đạt cấp tối thiểu chưa
            if (requiredPassiveStat != null && requiredPassiveStat.currentLevel < requiredPassiveLevel)
            {
                return false;
            }

            return true;
        }

        public override void ApplyUpgrade()
        {
            if (PlayerController.instance == null) return;

            isEvolved = true;

            // 1. Gỡ bỏ hoặc vô hiệu hóa vũ khí cũ
            Weapon oldWeapon = null;
            for (int i = PlayerController.instance.assignedWeapons.Count - 1; i >= 0; i--)
            {
                Weapon w = PlayerController.instance.assignedWeapons[i];
                if (w != null && (w.name.Contains(requiredWeaponName) || requiredWeaponName.Contains(w.name)))
                {
                    oldWeapon = w;
                    PlayerController.instance.assignedWeapons.RemoveAt(i);
                    break;
                }
            }

            if (oldWeapon == null)
            {
                for (int i = PlayerController.instance.fullyLevelledWeapons.Count - 1; i >= 0; i--)
                {
                    Weapon w = PlayerController.instance.fullyLevelledWeapons[i];
                    if (w != null && (w.name.Contains(requiredWeaponName) || requiredWeaponName.Contains(w.name)))
                    {
                        oldWeapon = w;
                        PlayerController.instance.fullyLevelledWeapons.RemoveAt(i);
                        break;
                    }
                }
            }

            if (oldWeapon != null)
            {
                oldWeapon.gameObject.SetActive(false);
            }

            // 2. Sinh hoặc trang bị vũ khí tiến hóa mới
            if (evolvedWeaponPrefab != null)
            {
                GameObject newObj = Instantiate(evolvedWeaponPrefab, PlayerController.instance.transform);
                newObj.name = evolvedWeaponName;
                Weapon evolvedWeapon = newObj.GetComponent<Weapon>();
                if (evolvedWeapon != null)
                {
                    PlayerController.instance.AddWeapon(evolvedWeapon);
                }
            }

            Debug.Log($"<color=yellow>[EVOLUTION] Thành công tiến hóa: {requiredWeaponName} -> {evolvedWeaponName}!</color>");
        }

        public override string GetDisplayName()
        {
            return $"★ EVOLUTION: {evolvedWeaponName}";
        }

        public override string GetDescription()
        {
            if (!string.IsNullOrEmpty(baseDescription)) return baseDescription;
            string passiveName = requiredPassiveStat != null ? requiredPassiveStat.upgradeName : "Synergy";
            return $"Combines max-level {requiredWeaponName} + {passiveName} into the ultimate {evolvedWeaponName}!";
        }

        public override void ResetState()
        {
            isEvolved = false;
        }
    }
}
