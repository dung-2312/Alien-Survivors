using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using AlienSurvivors.Upgrades;

public class LevelUpSellectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public TMP_Text upgradeDescText; // The upgrade description text. GK 
    public TMP_Text nameLevelText;    // The name level text. GK 
    public Image weaponIcon;          // The weapon icon. GK

    [Header("Sci-Fi Theme & Hover")]
    public float hoverScale = 1.05f;
    public Color normalBorderColor = new Color(0.12f, 0.22f, 0.35f, 0.8f);
    public Color hoverBorderColor = new Color(0f, 1f, 0.64f, 1f); // Neon Mint #00FFA3
    public Color cardBgColor = new Color(0.043f, 0.075f, 0.145f, 0.96f); // Dark Navy #0B1325

    private UpgradeData assignedUpgrade;
    private Weapon assignedWeapon;

    private RectTransform rectTransform;
    private Image buttonImage;
    private Outline outline;
    private Vector3 initialScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Vector2 originalAnchoredPosition;
    private bool isSetup = false;

    private void Awake()
    {
        SetupStyle();
    }

    private void Start()
    {
        SetupStyle();
    }

    private Coroutine introCoroutine;

    private void SetupStyle()
    {
        if (isSetup) return;

        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(320f, 480f);
            initialScale = rectTransform.localScale != Vector3.zero ? rectTransform.localScale : Vector3.one;
            targetScale = initialScale;
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        // Tạo viền phát sáng (Outline) cho nút nếu chưa có
        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        if (outline != null)
        {
            outline.effectColor = normalBorderColor;
            outline.effectDistance = new Vector2(3f, -3f);
        }

        // Đổi màu nền nút sang Dark Sci-Fi Navy
        if (buttonImage != null)
        {
            buttonImage.color = cardBgColor;
        }

        // Căn chỉnh vị trí Header (Tên kỹ năng)
        if (nameLevelText != null)
        {
            RectTransform nrt = nameLevelText.GetComponent<RectTransform>();
            if (nrt != null)
            {
                nrt.anchorMin = new Vector2(0.04f, 0.74f);
                nrt.anchorMax = new Vector2(0.96f, 0.96f);
                nrt.offsetMin = Vector2.zero;
                nrt.offsetMax = Vector2.zero;
            }
            nameLevelText.alignment = TextAlignmentOptions.Center;
            nameLevelText.color = Color.white;
            nameLevelText.enableAutoSizing = true;
            nameLevelText.fontSizeMin = 14;
            nameLevelText.fontSizeMax = 22;
        }

        // Căn chỉnh Icon trung tâm
        if (weaponIcon != null)
        {
            RectTransform irt = weaponIcon.GetComponent<RectTransform>();
            if (irt != null)
            {
                irt.anchorMin = new Vector2(0.5f, 0.5f);
                irt.anchorMax = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(0f, 30f);
                irt.sizeDelta = new Vector2(130f, 130f);
            }
        }

        // Căn chỉnh Body (Mô tả hiệu ứng)
        if (upgradeDescText != null)
        {
            RectTransform drt = upgradeDescText.GetComponent<RectTransform>();
            if (drt != null)
            {
                drt.anchorMin = new Vector2(0.06f, 0.05f);
                drt.anchorMax = new Vector2(0.94f, 0.40f);
                drt.offsetMin = Vector2.zero;
                drt.offsetMax = Vector2.zero;
            }
            upgradeDescText.alignment = TextAlignmentOptions.Center;
            upgradeDescText.color = new Color(0.85f, 0.92f, 1f, 0.95f);
            upgradeDescText.enableAutoSizing = true;
            upgradeDescText.fontSizeMin = 13;
            upgradeDescText.fontSizeMax = 18;
        }

        isSetup = true;
    }

    public void PlayIntroAnimation(float delay = 0f)
    {
        if (introCoroutine != null) StopCoroutine(introCoroutine);
        introCoroutine = StartCoroutine(AnimateIntro(delay));
    }

    private IEnumerator AnimateIntro(float delay)
    {
        if (rectTransform == null) yield break;

        Vector2 finalPos = originalAnchoredPosition;
        Vector2 startPos = finalPos + Vector2.down * 60f;
        rectTransform.anchoredPosition = startPos;

        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float timer = 0f;
        while (timer < delay)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        float duration = 0.28f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ease = 1f - Mathf.Pow(1f - t, 3); // Ease-Out Cubic
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, finalPos, ease);
            cg.alpha = Mathf.Lerp(0f, 1f, ease);
            yield return null;
        }

        rectTransform.anchoredPosition = finalPos;
        cg.alpha = 1f;
    }

    private void Update()
    {
        // Chạy animation phóng to mượt mà bằng Time.unscaledDeltaTime (bất chấp Time.timeScale = 0)
        if (rectTransform != null && rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, 15f * Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// Cập nhật hiển thị thẻ nâng cấp Data-Driven với Highlight số liệu Neon.
    /// </summary>
    public void UpdateButtonDisplay(UpgradeData upgrade)
    {
        SetupStyle();
        assignedUpgrade = upgrade;
        assignedWeapon = null;

        if (upgrade == null) return;

        // 1. Tên nâng cấp (Màu vàng ánh kim hoặc xanh neon)
        if (nameLevelText != null)
        {
            string prefix = upgrade.upgradeType switch
            {
                UpgradeType.UnlockWeapon => "<color=#FFD700>[NEW]</color> ",
                UpgradeType.UpgradeStat => "<color=#00FFA3>[PASSIVE]</color> ",
                UpgradeType.WeaponEvolution => "<color=#FF3B30>[EVO]</color> ",
                _ => ""
            };
            nameLevelText.text = $"{prefix}<b>{upgrade.GetDisplayName().ToUpper()}</b>";
        }

        // 2. Mô tả chỉ số tự động tô màu xanh Neon (#00FFA3) cho các con số
        if (upgradeDescText != null)
        {
            string rawDesc = upgrade.GetDescription();
            string highlighted = Regex.Replace(rawDesc, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#00FFA3><b>$1</b></color>");
            upgradeDescText.text = highlighted;
        }

        // 3. Icon hiển thị
        if (weaponIcon != null)
        {
            Sprite icon = upgrade.GetIcon();
            if (icon != null)
            {
                weaponIcon.sprite = icon;
                weaponIcon.gameObject.SetActive(true);
            }
            else
            {
                weaponIcon.gameObject.SetActive(false);
            }
        }

        // Khôi phục viền mặc định
        if (outline != null) outline.effectColor = normalBorderColor;
        targetScale = initialScale;
        if (rectTransform != null) rectTransform.localScale = initialScale;

        // Kích hoạt hiệu ứng trượt xuất hiện tuần tự
        float delay = Mathf.Clamp(transform.GetSiblingIndex(), 0, 5) * 0.08f;
        PlayIntroAnimation(delay);
    }

    /// <summary>
    /// Tương thích ngược với hệ thống vũ khí cũ.
    /// </summary>
    public void UpdateButtonDisplay(Weapon theWeapon)
    {
        SetupStyle();
        assignedUpgrade = null;
        assignedWeapon = theWeapon;

        if (theWeapon == null) return;

        if (theWeapon.gameObject.activeSelf == true)
        {
            string raw = theWeapon.stats[theWeapon.weaponLevel].upgradeText;
            if (upgradeDescText != null)
            {
                upgradeDescText.text = Regex.Replace(raw, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#00FFA3><b>$1</b></color>");
            }
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null) nameLevelText.text = $"<b>{theWeapon.name.ToUpper()}</b> - <color=#FFD700>LV.{theWeapon.weaponLevel + 1}</color>";
        }
        else
        {
            if (upgradeDescText != null) upgradeDescText.text = $"Unlocks and equips <color=#00FFA3>{theWeapon.name}</color>.";
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null) nameLevelText.text = $"<color=#FFD700>[NEW]</color> <b>{theWeapon.name.ToUpper()}</b>";
        }

        if (weaponIcon != null) weaponIcon.gameObject.SetActive(weaponIcon.sprite != null);

        // Kích hoạt hiệu ứng trượt xuất hiện tuần tự
        float delay = Mathf.Clamp(transform.GetSiblingIndex(), 0, 5) * 0.08f;
        PlayIntroAnimation(delay);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale * hoverScale;
        if (outline != null) outline.effectColor = hoverBorderColor;

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;
        if (outline != null) outline.effectColor = normalBorderColor;
    }

    public void SelectUpgrade()
    {
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(2);
        }

        // 1. Áp dụng nâng cấp Data-Driven
        if (assignedUpgrade != null)
        {
            assignedUpgrade.ApplyUpgrade();
            ExperienceLevelController.instance.FinishLevelUp();
            return;
        }

        // 2. Fallback sang logic vũ khí cũ
        if (assignedWeapon != null)
        {
            if (assignedWeapon.gameObject.activeSelf == true)
            {
                assignedWeapon.LevelUp();
            }
            else
            {
                PlayerController.instance.AddWeapon(assignedWeapon);
            }
            ExperienceLevelController.instance.FinishLevelUp();
        }
    }
}
