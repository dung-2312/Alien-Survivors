using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using AlienSurvivors.Upgrades;

public class UpgradeCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Header Elements")]
    public TMP_Text titleText;
    public TMP_Text badgeText;
    public Image badgeBackground;

    [Header("Icon Frame")]
    public Image iconImage;
    public Image iconGlowBorder;
    public GameObject newTagBadge;

    [Header("Body Content")]
    public TMP_Text descriptionText;

    [Header("Footer & Level Progression")]
    public TMP_Text levelProgressText;
    public Transform levelPipsContainer;
    public Image[] levelPipImages; // Các chấm hiển thị cấp độ (thường là 5 chấm)

    [Header("Card Frame & Components")]
    public Image cardBackground;
    public Image cardBorderGlow;
    public CanvasGroup canvasGroup;
    public RectTransform rectTransform;
    public Button selectButton;

    [Header("Sci-Fi Theme Colors")]
    public Color badgeWeaponColor = new Color(0.2f, 0.65f, 1f, 1f);     // Neon Blue
    public Color badgeStatColor = new Color(0f, 0.95f, 0.65f, 1f);       // Neon Mint
    public Color badgeEvoColor = new Color(0.95f, 0.25f, 0.5f, 1f);      // Radiant Crimson
    public Color badgeNewColor = new Color(1f, 0.85f, 0.15f, 1f);        // Solar Gold

    public Color pipFilledColor = new Color(0f, 1f, 0.64f, 1f);         // #00FFA3 (Đã đạt)
    public Color pipNextColor = new Color(1f, 0.84f, 0f, 1f);           // #FFD700 (Sắp lên cấp)
    public Color pipEmptyColor = new Color(0.15f, 0.22f, 0.32f, 0.6f);  // Chưa đạt

    public Color borderNormalColor = new Color(0.15f, 0.25f, 0.38f, 0.6f);
    public Color borderHoverColor = new Color(0f, 1f, 0.64f, 1f);

    [Header("Animation Settings")]
    public float hoverScale = 1.05f;
    public float scaleSpeed = 14f;

    private UpgradeData currentUpgrade;
    private Vector3 initialScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private bool isInitialized = false;

    private void Awake()
    {
        InitComponents();
    }

    private void InitComponents()
    {
        if (isInitialized) return;

        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (selectButton == null) selectButton = GetComponent<Button>();

        initialScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
        targetScale = initialScale;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(SelectThisUpgrade);
        }

        isInitialized = true;
    }

    private void Update()
    {
        // Chạy animation scale bằng unscaledDeltaTime để mượt mà ngay cả khi Time.timeScale = 0
        if (rectTransform != null && rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, scaleSpeed * Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// Nạp dữ liệu UpgradeData (ScriptableObject) vào thẻ và định dạng UI Sci-Fi.
    /// </summary>
    public void SetCardData(UpgradeData upgrade)
    {
        InitComponents();
        currentUpgrade = upgrade;

        if (upgrade == null) return;

        // 1. Tên nâng cấp
        if (titleText != null)
        {
            titleText.text = upgrade.upgradeName;
        }

        // 2. Icon hình ảnh
        if (iconImage != null)
        {
            Sprite icon = upgrade.GetIcon();
            iconImage.sprite = icon;
            iconImage.gameObject.SetActive(icon != null);
        }

        // 3. Phân loại Badge & Cấp độ tiến trình (Level Pips)
        ConfigureBadgeAndPips(upgrade);

        // 4. Mô tả chỉ số với highlight số liệu (Neon Green #00FFA3)
        if (descriptionText != null)
        {
            string rawDesc = upgrade.GetDescription();
            // Tự động tìm các số liệu (+20%, -15%, x1.5, +25) và bọc màu neon nổi bật
            string formattedDesc = Regex.Replace(rawDesc, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#00FFA3><b>$1</b></color>");
            descriptionText.text = formattedDesc;
        }

        // 5. Khôi phục trạng thái viền bình thường
        if (cardBorderGlow != null)
        {
            cardBorderGlow.color = borderNormalColor;
        }
        targetScale = initialScale;
        if (rectTransform != null) rectTransform.localScale = initialScale;
    }

    private void ConfigureBadgeAndPips(UpgradeData upgrade)
    {
        int currentLevel = 0;
        int maxLevel = 5;
        bool isNew = false;

        switch (upgrade.upgradeType)
        {
            case UpgradeType.UnlockWeapon:
                isNew = true;
                currentLevel = 0;
                maxLevel = 5;
                SetBadge("NEW WEAPON", badgeNewColor);
                if (levelProgressText != null) levelProgressText.text = "LEVEL 1 / 5";
                break;

            case UpgradeType.UpgradeWeapon:
                isNew = false;
                // Tìm cấp hiện tại của vũ khí trên nhân vật
                Weapon w = FindWeaponOnPlayer(upgrade);
                if (w != null)
                {
                    currentLevel = w.weaponLevel + 1; // 1-indexed
                    maxLevel = w.stats.Count;
                }
                else
                {
                    currentLevel = 1;
                    maxLevel = 5;
                }
                SetBadge("WEAPON", badgeWeaponColor);
                if (levelProgressText != null) levelProgressText.text = $"LEVEL {currentLevel + 1} / {maxLevel}";
                break;

            case UpgradeType.UpgradeStat:
                if (upgrade is UpgradeStatUpgradeData statData)
                {
                    currentLevel = statData.currentLevel;
                    maxLevel = statData.maxLevel;
                    isNew = currentLevel == 0;
                }
                SetBadge(isNew ? "NEW PASSIVE" : "PASSIVE STAT", isNew ? badgeNewColor : badgeStatColor);
                if (levelProgressText != null) levelProgressText.text = $"LEVEL {currentLevel + 1} / {maxLevel}";
                break;

            case UpgradeType.WeaponEvolution:
                isNew = true;
                currentLevel = maxLevel = 5;
                SetBadge("★ EVOLUTION", badgeEvoColor);
                if (levelProgressText != null) levelProgressText.text = "MAX EVOLVED";
                break;
        }

        // Tag "NEW" góc thẻ
        if (newTagBadge != null)
        {
            newTagBadge.SetActive(isNew);
        }

        // Cập nhật các chấm cấp độ (Pips)
        UpdateLevelPips(currentLevel, maxLevel, isNew);
    }

    private void SetBadge(string text, Color bgColor)
    {
        if (badgeText != null) badgeText.text = text;
        if (badgeBackground != null) badgeBackground.color = bgColor;
    }

    private void UpdateLevelPips(int currentLevel, int maxLevel, bool isNew)
    {
        if (levelPipImages == null || levelPipImages.Length == 0) return;

        int totalPips = levelPipImages.Length;
        int nextLevel = isNew ? 1 : currentLevel + 1;

        for (int i = 0; i < totalPips; i++)
        {
            if (i >= maxLevel)
            {
                levelPipImages[i].gameObject.SetActive(false);
                continue;
            }

            levelPipImages[i].gameObject.SetActive(true);

            if (i < currentLevel)
            {
                levelPipImages[i].color = pipFilledColor; // Cấp đã đạt
            }
            else if (i == currentLevel)
            {
                levelPipImages[i].color = pipNextColor;   // Cấp sẽ đạt sau khi chọn thẻ này
            }
            else
            {
                levelPipImages[i].color = pipEmptyColor;  // Cấp chưa đạt
            }
        }
    }

    private Weapon FindWeaponOnPlayer(UpgradeData upgrade)
    {
        if (PlayerController.instance == null) return null;

        string targetName = upgrade.upgradeName;
        if (upgrade is UpgradeWeaponUpgradeData upWeapon)
        {
            targetName = upWeapon.targetWeaponName;
        }

        foreach (var w in PlayerController.instance.assignedWeapons)
        {
            if (w != null && (w.name.Contains(targetName) || targetName.Contains(w.name)))
            {
                return w;
            }
        }
        return null;
    }

    // --- SỰ KIỆN TƯƠNG TÁC (JUICE & FEEDBACK) ---

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale * hoverScale;

        if (cardBorderGlow != null)
        {
            cardBorderGlow.color = borderHoverColor;
        }

        // Âm thanh lướt nhẹ khi hover qua thẻ
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;

        if (cardBorderGlow != null)
        {
            cardBorderGlow.color = borderNormalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectThisUpgrade();
    }

    public void SelectThisUpgrade()
    {
        if (currentUpgrade == null) return;

        // Âm thanh chọn thẻ
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(2);
        }

        // Áp dụng nâng cấp
        currentUpgrade.ApplyUpgrade();

        // Báo cho LevelUpManager / ExperienceLevelController đóng bảng hoặc xử lý level kế
        if (LevelUpManager.instance != null)
        {
            LevelUpManager.instance.OnCardSelected();
        }
        else if (ExperienceLevelController.instance != null)
        {
            ExperienceLevelController.instance.FinishLevelUp();
        }
    }
}
