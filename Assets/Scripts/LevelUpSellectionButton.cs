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

    [Header("Gothic Theme & Hover")]
    public float hoverScale = 1.03f;
    public Color highlightColor = new Color(1f, 0.88f, 0.25f);
    public Color nameColor = new Color(1f, 0.95f, 0.85f);
    public Color descColor = Color.white;

    private static Sprite cardNormalProceduralSprite;
    private static Sprite cardSelectedProceduralSprite;
    private static Sprite iconFrameProceduralSprite;
    private static Sprite pipFilledSprite;
    private static Sprite pipEmptySprite;

    private Sprite cardNormalSprite;
    private Sprite cardSelectedSprite;

    private UpgradeData assignedUpgrade;
    private Weapon assignedWeapon;

    private RectTransform rectTransform;
    private Image buttonImage;
    private Outline outline;
    private Vector3 initialScale = Vector3.one;
    private Vector3 targetScale = Vector3.one;
    private Vector2 originalAnchoredPosition;
    private bool isSetup = false;

    private RectTransform pipContainer;
    private readonly List<Image> pipImages = new List<Image>();
    private Coroutine introCoroutine;

    private RectTransform leftPointer;
    private RectTransform rightPointer;
    private bool isHovered = false;
    private static Sprite leftArrowSprite;
    private static Sprite rightArrowSprite;

    private void Awake()
    {
        SetupStyle();
    }

    private void Start()
    {
        SetupStyle();
    }

    private void SetupStyle()
    {
        if (isSetup) return;

        rectTransform = GetComponent<RectTransform>();
        buttonImage = GetComponent<Image>();

        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(840f, 172f);
            initialScale = rectTransform.localScale != Vector3.zero ? rectTransform.localScale : Vector3.one;
            targetScale = initialScale;
            originalAnchoredPosition = rectTransform.anchoredPosition;
        }

        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        // Khung tháº» phong cÃ¡ch Gothic HoÃ ng gia Ma má»‹
        cardNormalSprite = GetGothicCardSprite(false);
        cardSelectedSprite = GetGothicCardSprite(true);

        if (buttonImage != null)
        {
            buttonImage.sprite = cardNormalSprite;
            buttonImage.type = Image.Type.Sliced;
            buttonImage.color = Color.white;
        }

        // Khung viá» n há»™p Ä‘á»±ng Icon kÄ© nÄƒng (Relic Frame)
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
        frtImg.sprite = GetGothicIconSlotSprite();
        frtImg.type = Image.Type.Sliced;
        frtImg.color = Color.white;
        frtImg.raycastTarget = false;

        // CÄƒn chá»‰nh Icon trung tÃ¢m bÃªn trong khung viá» n
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

        // CÄƒn chá»‰nh vá»‹ trÃ­ Header (TÃªn kÄ© nÄƒng & Badge)
        if (nameLevelText != null)
        {
            RectTransform nrt = nameLevelText.GetComponent<RectTransform>();
            if (nrt != null)
            {
                nrt.anchorMin = new Vector2(0.19f, 0.54f);
                nrt.anchorMax = new Vector2(0.70f, 0.94f);
                nrt.offsetMin = Vector2.zero;
                nrt.offsetMax = Vector2.zero;
            }
            nameLevelText.alignment = TextAlignmentOptions.MidlineLeft;
            nameLevelText.color = nameColor;
            nameLevelText.enableAutoSizing = true;
            nameLevelText.fontSizeMin = 22;
            nameLevelText.fontSizeMax = 38;
            nameLevelText.textWrappingMode = TextWrappingModes.NoWrap;
            nameLevelText.richText = true;
        }

        // CÄƒn chá»‰nh Body (MÃ´ táº£ hiá»‡u á»©ng)
        if (upgradeDescText != null)
        {
            RectTransform drt = upgradeDescText.GetComponent<RectTransform>();
            if (drt != null)
            {
                drt.anchorMin = new Vector2(0.19f, 0.08f);
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

        // Khởi tạo container hiển thị Level Pips (Kim cương ngọc ma thuật)
        SetupPipContainer();
        SetupSelectionPointers();

        TMP_FontAsset smooth = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (smooth != null)
        {
            if (nameLevelText != null) { nameLevelText.font = smooth; nameLevelText.fontSharedMaterial = smooth.material; }
            if (upgradeDescText != null) { upgradeDescText.font = smooth; upgradeDescText.fontSharedMaterial = smooth.material; }
        }

        isSetup = true;
    }

    private void SetupSelectionPointers()
    {
        // 1. Mũi tên chỉ trái (Left Pointer hướng vào thẻ >)
        Transform lpT = transform.Find("SelectionLeftPointer");
        GameObject lpGO = lpT != null ? lpT.gameObject : new GameObject("SelectionLeftPointer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (lpT == null) lpGO.transform.SetParent(transform, false);
        leftPointer = lpGO.GetComponent<RectTransform>();
        leftPointer.anchorMin = new Vector2(0f, 0.5f);
        leftPointer.anchorMax = new Vector2(0f, 0.5f);
        leftPointer.pivot = new Vector2(0.5f, 0.5f);
        leftPointer.sizeDelta = new Vector2(36f, 36f);
        leftPointer.anchoredPosition = new Vector2(-28f, 0f);
        Image lpImg = lpGO.GetComponent<Image>();
        lpImg.sprite = GetArrowPointerSprite(true);
        lpImg.color = Color.white;
        lpImg.raycastTarget = false;
        leftPointer.gameObject.SetActive(false);

        // 2. Mũi tên chỉ phải (Right Pointer hướng vào thẻ <)
        Transform rpT = transform.Find("SelectionRightPointer");
        GameObject rpGO = rpT != null ? rpT.gameObject : new GameObject("SelectionRightPointer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (rpT == null) rpGO.transform.SetParent(transform, false);
        rightPointer = rpGO.GetComponent<RectTransform>();
        rightPointer.anchorMin = new Vector2(1f, 0.5f);
        rightPointer.anchorMax = new Vector2(1f, 0.5f);
        rightPointer.pivot = new Vector2(0.5f, 0.5f);
        rightPointer.sizeDelta = new Vector2(36f, 36f);
        rightPointer.anchoredPosition = new Vector2(28f, 0f);
        Image rpImg = rpGO.GetComponent<Image>();
        rpImg.sprite = GetArrowPointerSprite(false);
        rpImg.color = Color.white;
        rpImg.raycastTarget = false;
        rightPointer.gameObject.SetActive(false);
    }

    private static Sprite GetArrowPointerSprite(bool pointsRight)
    {
        if (pointsRight && rightArrowSprite != null) return rightArrowSprite;
        if (!pointsRight && leftArrowSprite != null) return leftArrowSprite;

        int size = 28;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color goldHighlight = new Color(1f, 0.95f, 0.65f, 1f);
        Color goldMid = new Color(0.92f, 0.72f, 0.20f, 1f);
        Color goldShadow = new Color(0.40f, 0.25f, 0.08f, 1f);
        Color rubyGem = new Color(0.95f, 0.15f, 0.25f, 1f);

        float midY = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int checkX = pointsRight ? x : (size - 1 - x);
                float distFromCenterY = Mathf.Abs(y - midY);
                float arrowEdge = checkX * 0.75f;

                if (distFromCenterY <= arrowEdge && distFromCenterY >= arrowEdge - 5.5f)
                {
                    if (checkX >= size - 6 && distFromCenterY <= 2.5f)
                    {
                        p[y * size + x] = rubyGem; // Mũi ngọc ruby ở đầu mũi tên
                    }
                    else if (y >= midY)
                    {
                        p[y * size + x] = goldHighlight;
                    }
                    else
                    {
                        p[y * size + x] = goldShadow;
                    }
                }
                else if (distFromCenterY < arrowEdge - 5.5f && distFromCenterY >= arrowEdge - 8f)
                {
                    p[y * size + x] = goldMid;
                }
                else
                {
                    p[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        if (pointsRight) rightArrowSprite = s;
        else leftArrowSprite = s;
        return s;
    }

    private void SetupPipContainer()
    {
        Transform pt = transform.Find("LevelPipContainer");
        GameObject pgo = pt != null ? pt.gameObject : new GameObject("LevelPipContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        if (pt == null)
        {
            pgo.transform.SetParent(transform, false);
        }

        pipContainer = pgo.GetComponent<RectTransform>();
        pipContainer.anchorMin = new Vector2(1f, 0.54f);
        pipContainer.anchorMax = new Vector2(1f, 0.94f);
        pipContainer.pivot = new Vector2(1f, 0.5f);
        pipContainer.anchoredPosition = new Vector2(-28f, 0f);
        pipContainer.sizeDelta = new Vector2(230f, 28f);

        HorizontalLayoutGroup hlg = pgo.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.spacing = 6f;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        pipImages.Clear();
        for (int i = 0; i < pipContainer.childCount; i++)
        {
            Image img = pipContainer.GetChild(i).GetComponent<Image>();
            if (img != null) pipImages.Add(img);
        }
    }

    private void UpdatePipVisuals(int currentLevel, int maxLevel)
    {
        if (pipContainer == null) SetupPipContainer();
        if (pipContainer == null) return;

        if (maxLevel <= 1)
        {
            pipContainer.gameObject.SetActive(false);
            return;
        }

        pipContainer.gameObject.SetActive(true);
        maxLevel = Mathf.Clamp(maxLevel, 2, 10);

        while (pipImages.Count < maxLevel)
        {
            GameObject pipGO = new GameObject($"Pip_{pipImages.Count}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            pipGO.transform.SetParent(pipContainer, false);
            RectTransform prt = pipGO.GetComponent<RectTransform>();
            prt.sizeDelta = new Vector2(20f, 20f);
            Image pImg = pipGO.GetComponent<Image>();
            pImg.preserveAspect = true;
            pImg.raycastTarget = false;
            pipImages.Add(pImg);
        }

        for (int i = 0; i < pipImages.Count; i++)
        {
            if (i < maxLevel)
            {
                pipImages[i].gameObject.SetActive(true);
                bool filled = (i < currentLevel);
                pipImages[i].sprite = GetPipDiamondSprite(filled);
                pipImages[i].color = filled ? Color.white : new Color(0.7f, 0.7f, 0.85f, 0.9f);
            }
            else
            {
                pipImages[i].gameObject.SetActive(false);
            }
        }
    }

    public static string BuildLevelPips(int currentLevel, int maxLevel = 8)
    {
        return ""; // Thay tháº¿ hoÃ n toÃ n báº±ng container Image pips sáº¯c nÃ©t
    }

    public void SetLayout(Vector2 size, Vector2 position)
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
            float ease = 1f - Mathf.Pow(1f - t, 3);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, finalPos, ease);
            cg.alpha = Mathf.Lerp(0f, 1f, ease);
            yield return null;
        }

        rectTransform.anchoredPosition = finalPos;
        cg.alpha = 1f;
    }

    private void Update()
    {
        if (rectTransform != null && rectTransform.localScale != targetScale)
        {
            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, 15f * Time.unscaledDeltaTime);
        }

        // Hiệu ứng xoay lắc lư và nhấp nhô của 2 mũi tên chọn thẻ
        if (isHovered)
        {
            float rot = Mathf.Sin(Time.unscaledTime * 6f) * 20f;
            float bob = Mathf.Sin(Time.unscaledTime * 8f) * 6f;
            if (leftPointer != null)
            {
                leftPointer.localRotation = Quaternion.Euler(0, 0, rot);
                leftPointer.anchoredPosition = new Vector2(-28f + bob, 0f);
            }
            if (rightPointer != null)
            {
                rightPointer.localRotation = Quaternion.Euler(0, 0, -rot);
                rightPointer.anchoredPosition = new Vector2(28f - bob, 0f);
            }
        }
    }

    public void UpdateButtonDisplay(UpgradeData upgrade)
    {
        SetupStyle();
        assignedUpgrade = upgrade;
        assignedWeapon = null;

        if (upgrade == null) return;

        int curLvl = 1;
        int maxLvl = 1;
        string badge = "";
        string displayName = upgrade.upgradeName.ToUpper();

        if (upgrade is UpgradeStatUpgradeData statUp)
        {
            curLvl = statUp.currentLevel + 1;
            maxLvl = statUp.maxLevel;
            badge = $"  <color=#FFE040><b>[ LV {curLvl} ]</b></color>";
        }
        else if (upgrade is WeaponEvolutionUpgradeData)
        {
            curLvl = 5;
            maxLvl = 5;
            badge = "  <color=#FF3344><b>[ FORBIDDEN EVOLUTION ]</b></color>";
        }
        else if (upgrade.upgradeType == UpgradeType.UnlockWeapon)
        {
            curLvl = 1;
            maxLvl = 8;
            badge = "  <color=#00FFAA><b>[ NEW! ]</b></color>";
        }
        else
        {
            badge = "  <color=#FFE040><b>[ UPGRADE ]</b></color>";
        }

        if (nameLevelText != null)
        {
            nameLevelText.text = $"<color=#FFE040><b>{displayName}</b></color>{badge}";
        }

        UpdatePipVisuals(curLvl, maxLvl);

        if (upgradeDescText != null)
        {
            string rawDesc = upgrade.GetDescription();
            string highlighted = Regex.Replace(rawDesc, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#FFE040><b>$1</b></color>");
            upgradeDescText.text = highlighted;
        }

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

        float delay = Mathf.Clamp(transform.GetSiblingIndex(), 0, 5) * 0.08f;
        PlayIntroAnimation(delay);
    }

    public void UpdateButtonDisplay(Weapon theWeapon)
    {
        SetupStyle();
        assignedUpgrade = null;
        assignedWeapon = theWeapon;

        if (theWeapon == null) return;

        int maxLvl = (theWeapon.stats != null && theWeapon.stats.Count > 0) ? theWeapon.stats.Count : 8;

        if (theWeapon.gameObject.activeSelf == true)
        {
            int nextLvl = theWeapon.weaponLevel + 1;
            string raw = (theWeapon.stats != null && theWeapon.weaponLevel < theWeapon.stats.Count) ? theWeapon.stats[theWeapon.weaponLevel].upgradeText : "Improves combat stats.";
            if (upgradeDescText != null)
            {
                upgradeDescText.text = Regex.Replace(raw, @"(\+?\-?\d+(\.\d+)?%?|\b[xX]\d+(\.\d+)?)", "<color=#FFE040><b>$1</b></color>");
            }
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null)
            {
                nameLevelText.text = $"<color=#FFE040><b>{theWeapon.name.ToUpper()}</b></color>  <color=#FFE040><b>[ LV {nextLvl} ]</b></color>";
            }
            UpdatePipVisuals(nextLvl, maxLvl);
        }
        else
        {
            if (upgradeDescText != null) upgradeDescText.text = $"Unlocks and equips <color=#FFE040>{theWeapon.name}</color>.";
            if (weaponIcon != null) weaponIcon.sprite = theWeapon.icon;
            if (nameLevelText != null)
            {
                nameLevelText.text = $"<color=#FFE040><b>{theWeapon.name.ToUpper()}</b></color>  <color=#00FFAA><b>[ NEW! ]</b></color>";
            }
            UpdatePipVisuals(1, maxLvl);
        }

        if (weaponIcon != null) weaponIcon.gameObject.SetActive(weaponIcon.sprite != null);

        SetFrame(false);
        targetScale = initialScale;
        if (rectTransform != null) rectTransform.localScale = initialScale;

        float delay = Mathf.Clamp(transform.GetSiblingIndex(), 0, 5) * 0.08f;
        PlayIntroAnimation(delay);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = initialScale * hoverScale;
        SetFrame(true);
        isHovered = true;
        if (leftPointer != null) leftPointer.gameObject.SetActive(true);
        if (rightPointer != null) rightPointer.gameObject.SetActive(true);

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = initialScale;
        SetFrame(false);
        isHovered = false;
        if (leftPointer != null) leftPointer.gameObject.SetActive(false);
        if (rightPointer != null) rightPointer.gameObject.SetActive(false);
    }

    private void SetFrame(bool selected)
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

        if (assignedUpgrade != null)
        {
            assignedUpgrade.ApplyUpgrade();
            ExperienceLevelController.instance.FinishLevelUp();
            return;
        }

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

    // ====================================================================================
    // PROCEDURAL HIGH-RESOLUTION GOTHIC ASSET GENERATION
    // ====================================================================================
    public static Sprite GetGothicCardSprite(bool selected)
    {
        if (selected && cardSelectedProceduralSprite != null) return cardSelectedProceduralSprite;
        if (!selected && cardNormalProceduralSprite != null) return cardNormalProceduralSprite;

        int w = 192;
        int h = 96;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color darkIron = new Color(0.04f, 0.03f, 0.06f, 1f);
        Color borderHighlight = selected ? new Color(1f, 0.95f, 0.65f, 1f) : new Color(0.88f, 0.72f, 0.28f, 1f);
        Color borderMid = selected ? new Color(1f, 0.76f, 0.18f, 1f) : new Color(0.68f, 0.52f, 0.18f, 1f);
        Color borderShadow = selected ? new Color(0.60f, 0.35f, 0.08f, 1f) : new Color(0.24f, 0.16f, 0.05f, 1f);

        Color bgDark = selected ? new Color(0.14f, 0.09f, 0.22f, 0.98f) : new Color(0.06f, 0.05f, 0.09f, 0.98f);
        Color bgLight = selected ? new Color(0.24f, 0.14f, 0.36f, 0.98f) : new Color(0.11f, 0.09f, 0.16f, 0.98f);
        Color innerAuraLine = selected ? new Color(1f, 0.85f, 0.40f, 0.8f) : new Color(0.35f, 0.22f, 0.52f, 0.6f);
        Color gemGleam = new Color(1f, 0.95f, 0.85f, 1f);
        Color gemCore = selected ? new Color(1f, 0.50f, 0.10f, 1f) : new Color(0.85f, 0.15f, 0.25f, 1f);

        for (int y = 0; y < h; y++)
        {
            float tY = (float)y / (h - 1);
            Color currentBg = Color.Lerp(bgDark, bgLight, tY);

            for (int x = 0; x < w; x++)
            {
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    p[y * w + x] = darkIron;
                }
                else if (x <= 4 || x >= w - 5 || y <= 4 || y >= h - 5)
                {
                    bool isCornerGem = (x >= 2 && x <= 5 && y >= 2 && y <= 5) ||
                                       (x >= w - 6 && x <= w - 3 && y >= 2 && y <= 5) ||
                                       (x >= 2 && x <= 5 && y >= h - 6 && y <= h - 3) ||
                                       (x >= w - 6 && x <= w - 3 && y >= h - 6 && y <= h - 3);

                    if (isCornerGem)
                    {
                        bool isCenter = (x == 3 || x == 4 || x == w - 5 || x == w - 4) &&
                                        (y == 3 || y == 4 || y == h - 5 || y == h - 4);
                        p[y * w + x] = isCenter ? gemGleam : gemCore;
                    }
                    else
                    {
                        bool isTopOrLeft = (x <= 2 || y >= h - 3);
                        p[y * w + x] = isTopOrLeft ? borderHighlight : borderShadow;
                    }
                }
                else if (x == 7 || x == w - 8 || y == 7 || y == h - 8)
                {
                    p[y * w + x] = innerAuraLine;
                }
                else
                {
                    p[y * w + x] = currentBg;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();

        Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(24, 24, 24, 24));
        if (selected) cardSelectedProceduralSprite = s;
        else cardNormalProceduralSprite = s;
        return s;
    }

    public static Sprite GetGothicIconSlotSprite()
    {
        if (iconFrameProceduralSprite != null) return iconFrameProceduralSprite;

        int size = 80;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color darkIron = new Color(0.04f, 0.03f, 0.06f, 1f);
        Color borderGold = new Color(0.85f, 0.68f, 0.22f, 1f);
        Color shadowBronze = new Color(0.28f, 0.18f, 0.06f, 1f);
        Color highlightGold = new Color(1f, 0.94f, 0.65f, 1f);
        Color innerDark = new Color(0.04f, 0.03f, 0.07f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    p[y * size + x] = darkIron;
                }
                else if (x <= 4 || x >= size - 5 || y <= 4 || y >= size - 5)
                {
                    bool isTopOrLeft = (x <= 2 || y >= size - 3);
                    p[y * size + x] = isTopOrLeft ? highlightGold : shadowBronze;
                }
                else if (x == 6 || x == size - 7 || y == 6 || y == size - 7)
                {
                    p[y * size + x] = borderGold * 0.75f;
                }
                else
                {
                    p[y * size + x] = innerDark;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();

        iconFrameProceduralSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(16, 16, 16, 16));
        return iconFrameProceduralSprite;
    }

    public static Sprite GetPipDiamondSprite(bool filled)
    {
        if (filled && pipFilledSprite != null) return pipFilledSprite;
        if (!filled && pipEmptySprite != null) return pipEmptySprite;

        int size = 24;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color transparent = new Color(0, 0, 0, 0);

        Color rimGold = new Color(0.78f, 0.58f, 0.12f, 1f);
        Color brightGold = new Color(1f, 0.85f, 0.28f, 1f);
        Color highlightWhite = new Color(1f, 1f, 0.95f, 1f);
        Color deepGold = new Color(0.90f, 0.55f, 0.08f, 1f);

        Color emptyRim = new Color(0.42f, 0.38f, 0.48f, 0.95f);
        Color emptyShade = new Color(0.24f, 0.20f, 0.28f, 0.9f);
        Color emptyVoid = new Color(0.08f, 0.07f, 0.11f, 0.95f);

        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float manhattan = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                if (manhattan > center + 0.8f)
                {
                    p[y * size + x] = transparent;
                }
                else if (manhattan >= center - 1.2f)
                {
                    p[y * size + x] = filled ? rimGold : emptyRim;
                }
                else
                {
                    if (filled)
                    {
                        if (x <= center && y >= center)
                        {
                            p[y * size + x] = (x == (int)center && y == (int)center + 2) ? highlightWhite : brightGold;
                        }
                        else
                        {
                            p[y * size + x] = deepGold;
                        }
                    }
                    else
                    {
                        p[y * size + x] = (manhattan >= center - 2.8f) ? emptyShade : emptyVoid;
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();

        Sprite s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        if (filled) pipFilledSprite = s;
        else pipEmptySprite = s;
        return s;
    }
}
