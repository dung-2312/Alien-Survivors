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
            rectTransform.sizeDelta = new Vector2(840f, 172f); // Wide row card: icon left, name + description right.
            initialScale = rectTransform.localScale != Vector3.zero ? rectTransform.localScale : Vector3.one;
            targetScale = initialScale;
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        // The old flat Outline look is replaced by the gold pixel card frame.
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        cardNormalSprite = Resources.Load<Sprite>("UI/VS_Card_Stone_Slot") ?? Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        cardSelectedSprite = Resources.Load<Sprite>("UI/VS_Card_Selected_Brackets") ?? Resources.Load<Sprite>("UI/UI_Card_Selected");
        if (buttonImage != null && cardNormalSprite != null)
        {
            buttonImage.sprite = cardNormalSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = Color.white;
        }

        // Khung viền đá trang trí cho Icon kỹ năng
        Transform frameT = transform.Find("IconSlotFrame");
        GameObject frameGO = frameT != null ? frameT.gameObject : new GameObject("IconSlotFrame", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (frameT == null)
        {
            frameGO.transform.SetParent(transform, false);
            if (weaponIcon != null) frameGO.transform.SetSiblingIndex(weaponIcon.transform.GetSiblingIndex());
        }
        RectTransform frt = frameGO.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0.5f);
        frt.anchorMax = new Vector2(0f, 0.5f);
        frt.pivot = new Vector2(0.5f, 0.5f);
        frt.anchoredPosition = new Vector2(84f, 0f);
        frt.sizeDelta = new Vector2(116f, 116f);
        Image frtImg = frameGO.GetComponent<Image>();
        frtImg.sprite = Resources.Load<Sprite>("UI/VS_HUD_Slot") ?? cardNormalSprite;
        frtImg.type = Image.Type.Sliced;
        frtImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);
        frtImg.raycastTarget = false;

        // Căn chỉnh Icon trung tâm bên trong khung viền
        if (weaponIcon != null)
        {
            RectTransform irt = weaponIcon.GetComponent<RectTransform>();
            if (irt != null)
            {
                irt.anchorMin = new Vector2(0f, 0.5f);
                irt.anchorMax = new Vector2(0f, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(84f, 0f);
                irt.sizeDelta = new Vector2(96f, 96f);
            }
            weaponIcon.preserveAspect = true;
            weaponIcon.raycastTarget = false;
        }

        // Căn chỉnh vị trí Header (Tên kỹ năng & Level badge & Level Pips)
        if (nameLevelText != null)
        {
            RectTransform nrt = nameLevelText.GetComponent<RectTransform>();
            if (nrt != null)
            {
                nrt.anchorMin = new Vector2(0.19f, 0.52f);
                nrt.anchorMax = new Vector2(0.97f, 0.96f);
                nrt.offsetMin = Vector2.zero;
                nrt.offsetMax = Vector2.zero;
            }
            nameLevelText.alignment = TextAlignmentOptions.MidlineLeft;
            nameLevelText.color = nameColor;
            nameLevelText.enableAutoSizing = true;
            nameLevelText.fontSizeMin = 22;
            nameLevelText.fontSizeMax = 38;
            nameLevelText.textWrappingMode = TextWrappingModes.Normal;
            nameLevelText.richText = true;
        }

        // Căn chỉnh Body (Mô tả hiệu ứng)
        if (upgradeDescText != null)
        {
            RectTransform drt = upgradeDescText.GetComponent<RectTransform>();
            if (drt != null)
            {
                drt.anchorMin = new Vector2(0.19f, 0.05f);
                drt.anchorMax = new Vector2(0.97f, 0.52f);
                drt.offsetMin = Vector2.zero;
                drt.offsetMax = Vector2.zero;
            }
            upgradeDescText.alignment = TextAlignmentOptions.TopLeft;
            upgradeDescText.color = descColor;
            upgradeDescText.enableAutoSizing = true;
            upgradeDescText.fontSizeMin = 18;
            upgradeDescText.fontSizeMax = 28;
            upgradeDescText.textWrappingMode = TextWrappingModes.Normal;
            upgradeDescText.richText = true;
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

    public static string BuildLevelPips(int currentLevel, int maxLevel = 8)
    {
        if (maxLevel <= 1) return "";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 1; i <= maxLevel; i++)
        {
            if (i <= currentLevel)
                sb.Append("<color=#FFD700><b>◆</b></color> ");
            else
                sb.Append("<color=#404B5E><b>◇</b></color> ");
        }
        return sb.ToString().TrimEnd();
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
    /// Cập nhật hiển thị thẻ nâng cấp Data-Driven với Highlight số liệu Neon và thanh Level Pips phong cách Vampire Survivors.
    /// </summary>
    public void UpdateButtonDisplay(UpgradeData upgrade)
    {
        SetupStyle();
        assignedUpgrade = upgrade;
        assignedWeapon = null;

        if (upgrade == null) return;

        // 1. Tên nâng cấp + Badge + Level Pips chuẩn Vampire Survivors
        if (nameLevelText != null)
        {
            string badge = "";
            string pips = "";
            string displayName = upgrade.upgradeName.ToUpper();

            if (upgrade is UpgradeStatUpgradeData statUp)
            {
                badge = $"  <color=#FFE040><b>[ LV {statUp.currentLevel + 1} ]</b></color>";
                pips = $"  <size=65%>{BuildLevelPips(statUp.currentLevel + 1, statUp.maxLevel)}</size>";
            }
            else if (upgrade is WeaponEvolutionUpgradeData)
            {
                badge = "  <color=#FF3344><b>[ FORBIDDEN EVOLUTION ]</b></color>";
                pips = "  <size=65%><color=#FF3344>★ ★ ★ ★ ★</color></size>";
            }
            else if (upgrade.upgradeType == UpgradeType.UnlockWeapon)
            {
                badge = "  <color=#00FFAA><b>[ NEW! ]</b></color>";
                pips = $"  <size=65%>{BuildLevelPips(1, 8)}</size>";
            }
            else
            {
                badge = "  <color=#FFE040><b>[ UPGRADE ]</b></color>";
            }

            nameLevelText.text = $"<color=#FFE040><b>{displayName}</b></color>{badge}{pips}";
        }

        // 2. Mô tả chỉ số tự động tô màu vàng hoàng kim cho các con số
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
    /// Tương thích ngược với hệ thống vũ khí cũ: hiển thị tên vàng, cấp độ và thanh Pips Vampire Survivors.
    /// </summary>
    public void UpdateButtonDisplay(Weapon theWeapon)
    {
        SetupStyle();
        assignedUpgrade = null;
        assignedWeapon = theWeapon;

        if (theWeapon == null) return;

        int maxLvl = (theWeapon.stats != null && theWeapon.stats.Count > 0) ? theWeapon.stats.Count : 8;

        if (theWeapon.gameObject.activeSelf == true)
        {
            string raw = (theWeapon.stats != null && theWeapon.weaponLevel < theWeapon.stats.Count) ? theWeapon.stats[theWeapon.weaponLevel].upgradeText : "Improves combat stats.";
            if (upgradeDescText != null)
            {
                upgradeDescText.text = Regex.Replace(raw, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#FFE040><b>$1</b></color>");
            }
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null)
            {
                int nextLvl = theWeapon.weaponLevel + 1;
                string pips = BuildLevelPips(nextLvl, maxLvl);
                nameLevelText.text = $"<color=#FFE040><b>{theWeapon.name.ToUpper()}</b></color>  <color=#FFE040><b>[ LV {nextLvl} ]</b></color>  <size=65%>{pips}</size>";
            }
        }
        else
        {
            if (upgradeDescText != null) upgradeDescText.text = $"Unlocks and equips <color=#FFE040>{theWeapon.name}</color>.";
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null)
            {
                string pips = BuildLevelPips(1, maxLvl);
                nameLevelText.text = $"<color=#FFE040><b>{theWeapon.name.ToUpper()}</b></color>  <color=#00FFAA><b>[ NEW! ]</b></color>  <size=65%>{pips}</size>";
            }
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
