using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager instance;

    [Header("Combo State")]
    public int currentCombo = 0;
    public float comboTimer = 0f;
    public float maxComboDuration = 3.6f;
    public int highestCombo = 0;

    [Header("Combo Tier Multipliers")]
    private float moveSpeedBonus = 0f;
    private float cooldownReduction = 0f;
    private float expBonusMultiplier = 1f;

    public float MoveSpeedMultiplier => 1f + moveSpeedBonus;
    public float CooldownMultiplier => Mathf.Clamp(1f - cooldownReduction, 0.65f, 1f);
    public float ExpBonusMultiplier => expBonusMultiplier;
    public float ComboTimerProgress => Mathf.Clamp01(comboTimer / maxComboDuration);

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                EndCombo();
            }
        }
    }

    /// <summary>
    /// Đăng ký mỗi khi người chơi tiêu diệt 1 quái vật
    /// </summary>
    public void RegisterKill(Vector3 killPos)
    {
        currentCombo++;
        comboTimer = maxComboDuration;

        if (currentCombo > highestCombo)
        {
            highestCombo = currentCombo;
        }

        // Tích thêm nộ cho chiêu cuối
        if (PlayerUltimateController.instance != null)
        {
            float bonusRage = currentCombo >= 50 ? 3.0f : 2.0f;
            PlayerUltimateController.instance.AddRage(bonusRage);
        }

        // Cập nhật các mốc Cuồng Nộ (Bloodrush Tiers)
        UpdateComboTiers();
    }

    private void UpdateComboTiers()
    {
        string tierTitle = "";
        Color bannerColor = Color.white;

        if (currentCombo == 10)
        {
            tierTitle = "[ COMBO x10: SLAYER! ]";
            bannerColor = new Color(0.9f, 0.6f, 0.2f);
            moveSpeedBonus = 0.08f;
            cooldownReduction = 0.05f;
            expBonusMultiplier = 1.1f;
        }
        else if (currentCombo == 25)
        {
            tierTitle = "[ COMBO x25: BLOODLUST! ]";
            bannerColor = new Color(1f, 0.35f, 0.25f);
            moveSpeedBonus = 0.15f;
            cooldownReduction = 0.12f;
            expBonusMultiplier = 1.25f;
            if (CameraController.instance != null) CameraController.instance.Shake(0.12f, 0.2f);
        }
        else if (currentCombo == 50)
        {
            tierTitle = "[ COMBO x50: UNSTOPPABLE! ]";
            bannerColor = new Color(1f, 0.85f, 0.1f);
            moveSpeedBonus = 0.22f;
            cooldownReduction = 0.20f;
            expBonusMultiplier = 1.5f;
            if (CameraController.instance != null) CameraController.instance.Shake(0.2f, 0.3f);
        }
        else if (currentCombo == 100)
        {
            tierTitle = "[ COMBO x100: GODLIKE RUSH! ]";
            bannerColor = new Color(1f, 0.15f, 0.4f);
            moveSpeedBonus = 0.30f;
            cooldownReduction = 0.30f;
            expBonusMultiplier = 2.0f;
            if (CameraController.instance != null) CameraController.instance.Shake(0.3f, 0.4f);
        }
        else if (currentCombo == 200)
        {
            tierTitle = "[ COMBO x200: APOCALYPSE! ]";
            bannerColor = new Color(0.8f, 0.2f, 1f);
            moveSpeedBonus = 0.40f;
            cooldownReduction = 0.35f;
            expBonusMultiplier = 2.5f;
            if (CameraController.instance != null) CameraController.instance.Shake(0.4f, 0.5f);
        }

        if (!string.IsNullOrEmpty(tierTitle) && UIController.instance != null)
        {
            UIController.instance.ShowWarning(tierTitle, 1.4f);
        }
    }

    private void EndCombo()
    {
        if (currentCombo >= 15)
        {
            // Thưởng thêm vàng khi hoàn tất chuỗi combo ấn tượng
            int bonusGold = Mathf.RoundToInt(currentCombo * 1.5f);
            if (CoinController.instance != null && PlayerController.instance != null)
            {
                CoinController.instance.AddCoins(bonusGold);
            }

            if (UIController.instance != null)
            {
                UIController.instance.ShowWarning($"[ COMBO ENDED: +{bonusGold} GOLD BONUS! ]", 1.5f);
            }
        }

        currentCombo = 0;
        comboTimer = 0f;
        moveSpeedBonus = 0f;
        cooldownReduction = 0f;
        expBonusMultiplier = 1f;
    }
}
