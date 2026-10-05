using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    [CreateAssetMenu(fileName = "UpgradeStat_New", menuName = "Survivors/Upgrades/Passive Stat Upgrade")]
    public class UpgradeStatUpgradeData : UpgradeData
    {
        [Header("Stat Configuration")]
        public StatUpgradeType statType;
        [Tooltip("Giá trị cộng thêm vào chỉ số ở mỗi cấp")]
        public float valuePerLevel = 0.5f;
        [Tooltip("Cấp độ tối đa của thẻ chỉ số này trong một lượt chơi")]
        public int maxLevel = 5;

        [System.NonSerialized]
        public int currentLevel = 0;

        private void Awake()
        {
            upgradeType = UpgradeType.UpgradeStat;
        }

        public override bool IsAvailable()
        {
            return currentLevel < maxLevel;
        }

        public override void ApplyUpgrade()
        {
            currentLevel++;

            if (PlayerController.instance == null) return;

            switch (statType)
            {
                case StatUpgradeType.MoveSpeed:
                    PlayerController.instance.moveSpeed += valuePerLevel;
                    break;

                case StatUpgradeType.PickupRange:
                    PlayerController.instance.pickupRange += valuePerLevel;
                    break;

                case StatUpgradeType.MaxHealth:
                    if (PlayerHealthController.instance != null)
                    {
                        PlayerHealthController.instance.maxHealth += valuePerLevel;
                        PlayerHealthController.instance.currentHealth += valuePerLevel;
                        if (PlayerHealthController.instance.healthSlider != null)
                        {
                            PlayerHealthController.instance.healthSlider.maxValue = PlayerHealthController.instance.maxHealth;
                            PlayerHealthController.instance.healthSlider.value = PlayerHealthController.instance.currentHealth;
                        }
                    }
                    break;

                case StatUpgradeType.MaxWeapons:
                    PlayerController.instance.maxWeapons += Mathf.RoundToInt(valuePerLevel);
                    break;

                case StatUpgradeType.DamageMultiplier:
                    if (PlayerController.instance != null)
                    {
                        System.Action<Weapon> applyMult = (w) =>
                        {
                            if (w != null && w.stats != null)
                            {
                                foreach (var s in w.stats)
                                {
                                    s.damage *= (1f + valuePerLevel);
                                }
                                w.statsUpdated = true;
                            }
                        };

                        if (PlayerController.instance.assignedWeapons != null)
                        {
                            foreach (var weapon in PlayerController.instance.assignedWeapons)
                            {
                                applyMult(weapon);
                            }
                        }

                        if (PlayerController.instance.fullyLevelledWeapons != null)
                        {
                            foreach (var weapon in PlayerController.instance.fullyLevelledWeapons)
                            {
                                applyMult(weapon);
                            }
                        }
                    }
                    break;
            }
        }

        public override string GetDisplayName()
        {
            return $"{upgradeName} - Lvl {currentLevel + 1}";
        }

        public override string GetDescription()
        {
            if (!string.IsNullOrEmpty(baseDescription))
            {
                return $"{baseDescription} (+{valuePerLevel})";
            }

            return statType switch
            {
                StatUpgradeType.MoveSpeed => $"Increases player movement speed by +{valuePerLevel}",
                StatUpgradeType.PickupRange => $"Increases orb collection radius by +{valuePerLevel}",
                StatUpgradeType.MaxHealth => $"Increases maximum health by +{valuePerLevel}",
                StatUpgradeType.MaxWeapons => $"Increases maximum weapon capacity by +{valuePerLevel}",
                StatUpgradeType.DamageMultiplier => $"Increases all weapon damage by +{valuePerLevel * 100}%",
                _ => $"Upgrades {upgradeName} to Level {currentLevel + 1}"
            };
        }

        public override void ResetState()
        {
            currentLevel = 0;
        }
    }
}
