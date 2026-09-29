using UnityEngine;

namespace AlienSurvivors.Upgrades
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho tất cả các thẻ nâng cấp trong game (Data-Driven).
    /// </summary>
    public abstract class UpgradeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string upgradeName;
        public Sprite icon;
        [TextArea(2, 4)]
        public string baseDescription;
        public UpgradeType upgradeType;

        /// <summary>
        /// Kiểm tra thẻ nâng cấp này có đủ điều kiện xuất hiện trên bảng Level-Up hay không.
        /// </summary>
        public abstract bool IsAvailable();

        /// <summary>
        /// Thực thi hiệu ứng nâng cấp khi người chơi bấm chọn thẻ này.
        /// </summary>
        public abstract void ApplyUpgrade();

        /// <summary>
        /// Tên hiển thị động (VD: "Fireball - Lvl 3", "Speed Boots - Lvl 1").
        /// </summary>
        public virtual string GetDisplayName()
        {
            return upgradeName;
        }

        /// <summary>
        /// Mô tả chi tiết (hiển thị chỉ số cộng thêm của cấp tiếp theo).
        /// </summary>
        public virtual string GetDescription()
        {
            return baseDescription;
        }

        /// <summary>
        /// Icon hiển thị trên thẻ nâng cấp.
        /// </summary>
        public virtual Sprite GetIcon()
        {
            return icon;
        }

        /// <summary>
        /// Reset các biến trạng thái tạm thời trong trận đánh khi bắt đầu ván mới.
        /// </summary>
        public virtual void ResetState()
        {
        }
    }
}
