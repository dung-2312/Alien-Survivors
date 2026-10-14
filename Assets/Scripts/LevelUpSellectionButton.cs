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
    public float hoverScale = 1.03f;
    public Color highlightColor = new Color(1f, 0.88f, 0.25f); // Gold, matches the Character Select / Game Over frames.
    public Color nameColor = new Color(1f, 0.95f, 0.85f);
    public Color descColor = Color.white;

    private Sprite cardNormalSprite, cardSelectedSprite; // Gold-framed retro card sprites from Resources/UI.

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
            rectTransform.sizeDelta = new Vector2(800f, 170f); // Wide row card: icon left, name + description right.
            initialScale = rectTransform.localScale != Vector3.zero ? rectTransform.localScale : Vector3.one;
            targetScale = initialScale;
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        // The old flat Outline look is replaced by the gold pixel card frame.
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        cardNormalSprite = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        cardSelectedSprite = Resources.Load<Sprite>("UI/UI_Card_Selected");
        if (buttonImage != null && cardNormalSprite != null)
        {
            buttonImage.sprite = cardNormalSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = Color.white;
        }

        // Căn chỉnh vị trí Header (Tên kỹ năng)
        if (nameLevelText != null)
        {
            RectTransform nrt = nameLevelText.GetComponent<RectTransform>();
            if (nrt != null)
            {
                nrt.anchorMin = new Vector2(0.19f, 0.56f);
                nrt.anchorMax = new Vector2(0.97f, 0.95f);
                nrt.offsetMin = Vector2.zero;
                nrt.offsetMax = Vector2.zero;
            }
            nameLevelText.alignment = TextAlignmentOptions.MidlineLeft;
            nameLevelText.color = nameColor;
            nameLevelText.enableAutoSizing = true;
            nameLevelText.fontSizeMin = 24;
            nameLevelText.fontSizeMax = 42;
            nameLevelText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // Căn chỉnh Icon trung tâm
        if (weaponIcon != null)
        {
            RectTransform irt = weaponIcon.GetComponent<RectTransform>();
            if (irt != null)
            {
                irt.anchorMin = new Vector2(0f, 0.5f);
                irt.anchorMax = new Vector2(0f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(84f, 0f);
                irt.sizeDelta = new Vector2(112f, 112f);
            }
            weaponIcon.preserveAspect = true;
            weaponIcon.raycastTarget = false;
        }

        // Căn chỉnh Body (Mô tả hiệu ứng)
        if (upgradeDescText != null)
        {
            RectTransform drt = upgradeDescText.GetComponent<RectTransform>();
            if (drt != null)
            {
                drt.anchorMin = new Vector2(0.19f, 0.06f);
                drt.anchorMax = new Vector2(0.97f, 0.57f);
                drt.offsetMin = Vector2.zero;
                drt.offsetMax = Vector2.zero;
            }
            upgradeDescText.alignment = TextAlignmentOptions.TopLeft;
            upgradeDescText.color = descColor;
            upgradeDescText.enableAutoSizing = true;
            upgradeDescText.fontSizeMin = 20;
            upgradeDescText.fontSizeMax = 32;
            upgradeDescText.textWrappingMode = TextWrappingModes.Normal;
        }

        // The pixel (Kenney) font turns to blocks at this size, so card text uses the smooth Liberation font.
        TMP_FontAsset smooth = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (smooth != null)
        {
            if (nameLevelText != null) { nameLevelText.font = smooth; nameLevelText.fontSharedMaterial = smooth.material; }
            if (upgradeDescText != null) { upgradeDescText.font = smooth; upgradeDescText.fontSharedMaterial = smooth.material; }
        }

        isSetup = true;
    }

    public void SetLayout(Vector2 size, Vector2 position) // Called by UIController to place this row in the window.
    {
        SetupStyle();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;
        originalAnchoredPosition = position;
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
            string suffix = upgrade.upgradeType switch
            {
                UpgradeType.UnlockWeapon => "  <color=#FFE040><size=65%>New!</size></color>",
                UpgradeType.UpgradeStat => "  <color=#7CFF9B><size=65%>Passive</size></color>",
                UpgradeType.WeaponEvolution => "  <color=#FF5A4A><size=65%>Evolve!</size></color>",
                _ => ""
            };
            nameLevelText.text = $"{upgrade.GetDisplayName()}{suffix}";
        }

        // 2. Mô tả chỉ số tự động tô màu xanh Neon (#00FFA3) cho các con số
        if (upgradeDescText != null)
        {
            string rawDesc = upgrade.GetDescription();
            string highlighted = Regex.Replace(rawDesc, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#FFE040><b>$1</b></color>");
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

        SetFrame(false);
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
                upgradeDescText.text = Regex.Replace(raw, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#FFE040><b>$1</b></color>");
            }
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null) nameLevelText.text = $"{theWeapon.name}  <color=#FFE040><size=65%>Lv {theWeapon.weaponLevel + 1}</size></color>";
        }
        else
        {
            if (upgradeDescText != null) upgradeDescText.text = $"Unlocks and equips <color=#FFE040>{theWeapon.name}</color>.";
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null) nameLevelText.text = $"{theWeapon.name}  <color=#FFE040><size=65%>New!</size></color>";
        }

        if (weaponIcon != null) weaponIcon.gameObject.SetActive(weaponIcon.sprite != null);

        // Kích hoạt hiệu ứng trượt xuất hiện tuần tự
        float delay = Mathf.Clamp(transform.GetSiblingIndex(), 0, 5) * 0.08f;
        PlayIntroAnimation(delay);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale * hoverScale;
        SetFrame(true);

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;
        SetFrame(false);
    }

    private void SetFrame(bool selected) // Swap the card frame between normal and the brighter selected one.
    {
        if (buttonImage == null) return;
        Sprite frame = selected ? cardSelectedSprite : cardNormalSprite;
        if (frame != null) buttonImage.sprite = frame;
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
