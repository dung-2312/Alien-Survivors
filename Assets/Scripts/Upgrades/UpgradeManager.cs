using System.Collections.Generic;
using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager instance;

        [Header("Upgrade Database (ScriptableObjects)")]
        [Tooltip("Kéo các file UpgradeData (ScriptableObject) vào đây")]
        public List<UpgradeData> upgradeDatabase = new List<UpgradeData>();

        [Header("Settings")]
        [Tooltip("Tự động tạo thẻ nâng cấp cho các vũ khí đang có trên Player nếu chưa gán ScriptableObject")]
        public bool autoGenerateMissingCards = true;

        [Tooltip("Ưu tiên hiển thị thẻ tiến hóa (Evolution) nếu đủ điều kiện")]
        public bool prioritizeEvolution = true;

        private bool isInitialized = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                EnsureInitialized();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            EnsureInitialized();
        }

        public void EnsureInitialized()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (upgradeDatabase == null)
            {
                upgradeDatabase = new List<UpgradeData>();
            }

            // 1. Tự động nạp toàn bộ thẻ từ thư mục Resources/DataUpgrades nếu Inspector chưa gán
            if (upgradeDatabase.Count == 0)
            {
                UpgradeData[] loadedCards = Resources.LoadAll<UpgradeData>("DataUpgrades");
                if (loadedCards != null && loadedCards.Length > 0)
                {
                    upgradeDatabase.AddRange(loadedCards);
                }

                // Fallback nạp ScriptableObject nếu generic polymorphic query trả về rỗng
                if (upgradeDatabase.Count == 0)
                {
                    ScriptableObject[] allObjects = Resources.LoadAll<ScriptableObject>("DataUpgrades");
                    if (allObjects != null && allObjects.Length > 0)
                    {
                        foreach (var obj in allObjects)
                        {
                            if (obj is UpgradeData ud && !upgradeDatabase.Contains(ud))
                            {
                                upgradeDatabase.Add(ud);
                            }
                        }
                    }
                }

                Debug.Log($"[UpgradeManager] Loaded {upgradeDatabase.Count} upgrade cards from Resources/DataUpgrades.");
            }

            ResetRunState();

            if (autoGenerateMissingCards)
            {
                GenerateFallbackCardsIfNeeded();
            }
        }

        /// <summary>
        /// Khởi động lại trạng thái của các thẻ nâng cấp khi bắt đầu lượt chơi mới.
        /// </summary>
        public void ResetRunState()
        {
            foreach (var card in upgradeDatabase)
            {
                if (card != null)
                {
                    card.ResetState();
                }
            }
        }

        /// <summary>
        /// Lọc toàn bộ danh sách thẻ hợp lệ (chưa max cấp, đủ slot, thỏa điều kiện tiến hóa).
        /// </summary>
        public List<UpgradeData> GetAvailableUpgrades()
        {
            EnsureInitialized();
            List<UpgradeData> available = new List<UpgradeData>();

            for (int i = 0; i < upgradeDatabase.Count; i++)
            {
                UpgradeData card = upgradeDatabase[i];
                if (card != null && card.IsAvailable())
                {
                    available.Add(card);
                }
            }

            return available;
        }

        /// <summary>
        /// Lấy ngẫu nhiên 'count' thẻ nâng cấp không trùng nhau để hiển thị cho người chơi.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count = 3)
        {
            EnsureInitialized();
            List<UpgradeData> available = GetAvailableUpgrades();

            // Nếu vì lý do nào đó không có thẻ khả dụng, tự động sinh thẻ từ Player
            if (available.Count == 0 && autoGenerateMissingCards)
            {
                GenerateFallbackCardsIfNeeded();
                available = GetAvailableUpgrades();
            }

            List<UpgradeData> result = new List<UpgradeData>();

            if (available.Count == 0)
            {
                return result;
            }

            // 1. Nếu có thẻ tiến hóa vũ khí và bật ưu tiên
            if (prioritizeEvolution)
            {
                for (int i = 0; i < available.Count; i++)
                {
                    if (available[i] is WeaponEvolutionUpgradeData)
                    {
                        result.Add(available[i]);
                        available.RemoveAt(i);
                        break;
                    }
                }
            }

            // 2. Thuật toán Fisher-Yates / bốc ngẫu nhiên không trùng lặp
            while (result.Count < count && available.Count > 0)
            {
                int randomIndex = Random.Range(0, available.Count);
                result.Add(available[randomIndex]);
                available.RemoveAt(randomIndex);
            }

            return result;
        }

        /// <summary>
        /// Tự động sinh các thẻ nâng cấp cơ bản trong bộ nhớ nếu Database chưa được gán đầy đủ trong Inspector.
        /// Giúp game chạy mượt mà ngay lập tức mà không sợ thiếu thẻ.
        /// </summary>
        private void GenerateFallbackCardsIfNeeded()
        {
            if (PlayerController.instance == null) return;

            // Thêm các thẻ Passive Stat mặc định nếu chưa có
            EnsureStatCardExists(StatUpgradeType.MoveSpeed, "Speed Boots", "Increases movement speed by 10%", 0.5f);
            EnsureStatCardExists(StatUpgradeType.PickupRange, "Magnet Orb", "Increases collection range by +1", 1.0f);
            EnsureStatCardExists(StatUpgradeType.MaxHealth, "Heart Amulet", "Increases max health by +20", 20f);

            // Tự động thêm thẻ cho các vũ khí có trên Player
            List<Weapon> allWeapons = new List<Weapon>();
            allWeapons.AddRange(PlayerController.instance.assignedWeapons);
            allWeapons.AddRange(PlayerController.instance.unassignedWeapons);

            foreach (var w in allWeapons)
            {
                if (w == null) continue;

                string wName = w.name.Replace("(Clone)", "").Trim();

                // Tạo thẻ Unlock nếu chưa có
                if (!HasUnlockCardFor(wName))
                {
                    UnlockWeaponUpgradeData unlockCard = ScriptableObject.CreateInstance<UnlockWeaponUpgradeData>();
                    unlockCard.name = $"Unlock_{wName}";
                    unlockCard.upgradeName = wName;
                    unlockCard.targetWeaponName = wName;
                    unlockCard.icon = w.icon;
                    unlockCard.baseDescription = $"Unlocks and equips {wName}.";
                    upgradeDatabase.Add(unlockCard);
                }

                // Tạo thẻ Upgrade Level nếu chưa có
                if (!HasUpgradeCardFor(wName))
                {
                    UpgradeWeaponUpgradeData upgradeCard = ScriptableObject.CreateInstance<UpgradeWeaponUpgradeData>();
                    upgradeCard.name = $"Upgrade_{wName}";
                    upgradeCard.upgradeName = wName;
                    upgradeCard.targetWeaponName = wName;
                    upgradeCard.icon = w.icon;
                    upgradeCard.baseDescription = $"Upgrades damage and efficiency of {wName}.";
                    upgradeDatabase.Add(upgradeCard);
                }
            }
        }

        private void EnsureStatCardExists(StatUpgradeType type, string title, string desc, float value)
        {
            foreach (var card in upgradeDatabase)
            {
                if (card is UpgradeStatUpgradeData statCard && statCard.statType == type)
                {
                    return; // Đã tồn tại
                }
            }

            UpgradeStatUpgradeData newStatCard = ScriptableObject.CreateInstance<UpgradeStatUpgradeData>();
            newStatCard.name = $"Stat_{type}";
            newStatCard.upgradeName = title;
            newStatCard.statType = type;
            newStatCard.valuePerLevel = value;
            newStatCard.baseDescription = desc;
            upgradeDatabase.Add(newStatCard);
        }

        private bool HasUnlockCardFor(string weaponName)
        {
            foreach (var card in upgradeDatabase)
            {
                if (card is UnlockWeaponUpgradeData unlockCard && unlockCard.targetWeaponName == weaponName)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasUpgradeCardFor(string weaponName)
        {
            foreach (var card in upgradeDatabase)
            {
                if (card is UpgradeWeaponUpgradeData upCard && upCard.targetWeaponName == weaponName)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
