using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hệ thống Bộ Sưu Tập (Collection) chuẩn phong cách Vampire Survivors
/// Hiển thị lưới vũ khí, vật phẩm bị động, giọt rơi và báu vật đã khám phá
/// </summary>
public class CollectionManager : MonoBehaviour
{
    public static CollectionManager instance;

    [System.Serializable]
    public class CollectionItem
    {
        public string id;
        public string displayName;
        public string description;
        public string iconResourcePath;
        public bool isDefaultUnlocked;
    }

    private static readonly List<CollectionItem> catalog = new List<CollectionItem>
    {
        new CollectionItem { id = "whip", displayName = "Giant Sword", description = "Attacks horizontally, passes through enemies and inflicts heavy critical slashes.", iconResourcePath = "UI/VS_Icon_Sword", isDefaultUnlocked = true },
        new CollectionItem { id = "wand", displayName = "Magic Wand", description = "Fires rapid magical energy bolts at the nearest enemy with stardust trail.", iconResourcePath = "UI/VS_Icon_Wand", isDefaultUnlocked = true },
        new CollectionItem { id = "knife", displayName = "Throwing Daggers", description = "Fires rapidly in the direction the character is facing.", iconResourcePath = "UI/VS_Icon_Dagger", isDefaultUnlocked = true },
        new CollectionItem { id = "axe", displayName = "Throwing Axes", description = "Flies upward in high arcs, dealing catastrophic damage to swarms above.", iconResourcePath = "UI/VS_Icon_Axe", isDefaultUnlocked = true },
        new CollectionItem { id = "bible", displayName = "King Bible", description = "Orbits sacred scriptures around the character, shredding approaching monsters.", iconResourcePath = "UI/VS_Icon_Bible", isDefaultUnlocked = true },
        new CollectionItem { id = "dove", displayName = "Peachone", description = "Bombards circular flight paths with holy bombardment beams.", iconResourcePath = "UI/VS_Icon_Dove", isDefaultUnlocked = false },
        new CollectionItem { id = "water", displayName = "Santa Water", description = "Generates holy fiery pools that scorch enemies over time.", iconResourcePath = "UI/VS_Icon_Water", isDefaultUnlocked = false },
        new CollectionItem { id = "laurel", displayName = "Laurel", description = "Shields character from damage when active. Grants temporary invincibility.", iconResourcePath = "UI/VS_Icon_Laurel", isDefaultUnlocked = true },
        new CollectionItem { id = "gauntlet", displayName = "Flaming Gauntlet", description = "Increases all weapon damage by +15% per rank.", iconResourcePath = "UI/VS_Icon_Spinach", isDefaultUnlocked = true },
        new CollectionItem { id = "armor", displayName = "Gothic Armor", description = "Reduces incoming damage by 1. Retaliates with holy thorns.", iconResourcePath = "UI/VS_Icon_Armor", isDefaultUnlocked = true },
        new CollectionItem { id = "heart", displayName = "Hollow Heart", description = "Increases maximum health by +20% and regenerates vitality.", iconResourcePath = "UI/VS_Icon_Heart", isDefaultUnlocked = true },
        new CollectionItem { id = "tome", displayName = "Empty Tome", description = "Reduces weapon attack cooldown by -8% per rank.", iconResourcePath = "UI/VS_Icon_Tome", isDefaultUnlocked = true },
        new CollectionItem { id = "candelabrador", displayName = "Candelabrador", description = "Augments area of all attack effects by +10%.", iconResourcePath = "UI/VS_Icon_Candle", isDefaultUnlocked = true },
        new CollectionItem { id = "bracer", displayName = "Bracer", description = "Increases projectile flying speed by +10%.", iconResourcePath = "UI/VS_Icon_Bracer", isDefaultUnlocked = true },
        new CollectionItem { id = "spellbinder", displayName = "Spellbinder", description = "Increases the duration of weapon effects by +10%.", iconResourcePath = "UI/VS_Icon_Spellbinder", isDefaultUnlocked = false },
        new CollectionItem { id = "duplicator", displayName = "Duplicator", description = "Weapons fire +1 additional projectile per attack.", iconResourcePath = "UI/VS_Icon_Duplicator", isDefaultUnlocked = false },
        new CollectionItem { id = "wings", displayName = "Wings", description = "Increases character movement speed by +10%.", iconResourcePath = "UI/VS_Icon_Wings", isDefaultUnlocked = true },
        new CollectionItem { id = "crown", displayName = "Crown", description = "Character gains +8% more experience from all sources.", iconResourcePath = "UI/VS_Icon_Crown", isDefaultUnlocked = true },
        new CollectionItem { id = "magnet", displayName = "Attractorb", description = "Increases item and gem pickup radius significantly.", iconResourcePath = "UI/VS_Icon_Magnet", isDefaultUnlocked = true },
        new CollectionItem { id = "clover", displayName = "Clover", description = "Improves overall luck and critical strike chance.", iconResourcePath = "UI/VS_Icon_Clover", isDefaultUnlocked = true },
        new CollectionItem { id = "gem_blue", displayName = "Blue EXP Gem", description = "Provides minor experience points upon collection.", iconResourcePath = "UI/VS_Gem_Blue", isDefaultUnlocked = true },
        new CollectionItem { id = "gem_green", displayName = "Green EXP Gem", description = "Provides moderate experience points upon collection.", iconResourcePath = "UI/VS_Gem_Green", isDefaultUnlocked = true },
        new CollectionItem { id = "gem_red", displayName = "Red EXP Gem", description = "Concentrated experience crystal dropped by elite foes.", iconResourcePath = "UI/VS_Gem_Red", isDefaultUnlocked = true },
        new CollectionItem { id = "coin", displayName = "Gold Coin", description = "Currency retrieved from fallen enemies to purchase permanent power-ups.", iconResourcePath = "UI/VS_Icon_Coin", isDefaultUnlocked = true },
        new CollectionItem { id = "coin_bag", displayName = "Coin Bag", description = "A hefty pouch containing abundant gold coins.", iconResourcePath = "UI/VS_Icon_CoinBag", isDefaultUnlocked = true },
        new CollectionItem { id = "chicken", displayName = "Floor Chicken", description = "Savory roast chicken. Instantly restores 30 Health.", iconResourcePath = "UI/VS_Icon_Chicken", isDefaultUnlocked = true },
        new CollectionItem { id = "chest", displayName = "Treasure Chest", description = "Dropped by elite bosses. Bestows multiple upgrades and gold fountain.", iconResourcePath = "UI/VS_Icon_Chest", isDefaultUnlocked = true },
        new CollectionItem { id = "rosary", displayName = "Rosary Cross", description = "Holy relic. Obliterates all common monsters currently visible on screen.", iconResourcePath = "UI/VS_Icon_Rosary", isDefaultUnlocked = false },
        new CollectionItem { id = "vacuum", displayName = "Vacuum Orb", description = "Draws every single scattered experience gem across the map to the player.", iconResourcePath = "UI/VS_Icon_Vacuum", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_death", displayName = "Death Spiral", description = "Evolution of Throwing Axes. Casts spinning scythes of doom outward.", iconResourcePath = "UI/VS_Icon_EvolDeath", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_wand", displayName = "Holy Wand", description = "Evolution of Magic Wand. Unleashes continuous beam with zero cooldown.", iconResourcePath = "UI/VS_Icon_EvolWand", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_knife", displayName = "Thousand Edge", description = "Evolution of Knife. Fires infinite stream of relentless piercing blades.", iconResourcePath = "UI/VS_Icon_EvolKnife", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_vespers", displayName = "Unholy Vespers", description = "Evolution of King Bible. Creates permanent unbreakable orbit of holy wards.", iconResourcePath = "UI/VS_Icon_EvolBible", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_hellfire", displayName = "Hellfire", description = "Evolution of Fireball. Conjures colossal piercing meteors that scorch all.", iconResourcePath = "UI/VS_Icon_EvolHellfire", isDefaultUnlocked = false },
        new CollectionItem { id = "evol_soul", displayName = "Soul Eater", description = "Evolution of Garlic/Zone. Steals life force from swarms and heals the player.", iconResourcePath = "UI/VS_Icon_EvolSoul", isDefaultUnlocked = false }
    };

    public static bool IsUnlocked(string id)
    {
        CollectionItem item = catalog.Find(x => x.id == id);
        if (item == null) return false;
        int defaultVal = item.isDefaultUnlocked ? 1 : 0;
        return PlayerPrefs.GetInt("Collection_Item_" + id, defaultVal) == 1;
    }

    public static void UnlockItem(string id)
    {
        PlayerPrefs.SetInt("Collection_Item_" + id, 1);
        PlayerPrefs.Save();
    }

    public static int GetUnlockedCount()
    {
        int count = 0;
        for (int i = 0; i < catalog.Count; i++)
        {
            if (IsUnlocked(catalog[i].id)) count++;
        }
        return count;
    }

    // ====================================================================================
    // UI BUILDER: BẢNG COLLECTION CHUẨN VAMPIRE SURVIVORS (ẢNH 4)
    // ====================================================================================
    private GameObject panelRoot;
    private Image previewIconImg;
    private TextMeshProUGUI previewNameTxt;
    private TextMeshProUGUI previewDescTxt;
    private GameObject selectBracket;

    public static void OpenPanel(Transform canvasParent, Action onBack)
    {
        GameObject go = new GameObject("CollectionScreenPanel", typeof(RectTransform));
        go.transform.SetParent(canvasParent, false);
        CollectionManager cm = go.AddComponent<CollectionManager>();
        cm.BuildUI(onBack);
    }

    private void BuildUI(Action onBack)
    {
        panelRoot = gameObject;
        RectTransform rootRt = GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // 1. Nền tối ma mị mờ ảo
        GameObject bg = new GameObject("Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.transform.SetParent(transform, false);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero;
        bgrt.offsetMax = Vector2.zero;
        Image bgImg = bg.GetComponent<Image>();
        bgImg.color = new Color(0.04f, 0.02f, 0.05f, 0.92f);

        // 2. Thanh hiển thị Tiền Vàng ở đỉnh giữa (Coin Capsule)
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (CoinController.instance != null) currentCoins = CoinController.instance.currentCoins;

        GameObject coinCap = new GameObject("TopCoinCapsule", typeof(RectTransform), typeof(Image));
        coinCap.transform.SetParent(transform, false);
        RectTransform ccrt = coinCap.GetComponent<RectTransform>();
        ccrt.anchorMin = new Vector2(0.5f, 1f);
        ccrt.anchorMax = new Vector2(0.5f, 1f);
        ccrt.pivot = new Vector2(0.5f, 1f);
        ccrt.anchoredPosition = new Vector2(0f, -18f);
        ccrt.sizeDelta = new Vector2(220f, 48f);
        Image ccImg = coinCap.GetComponent<Image>();
        ccImg.sprite = UIController.GetGothicBanishButtonSprite();
        ccImg.type = Image.Type.Sliced;
        ccImg.color = new Color(0.18f, 0.14f, 0.10f, 0.95f);

        GameObject coinTxtObj = new GameObject("CoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
        coinTxtObj.transform.SetParent(coinCap.transform, false);
        RectTransform ctrt = coinTxtObj.GetComponent<RectTransform>();
        ctrt.anchorMin = Vector2.zero;
        ctrt.anchorMax = Vector2.one;
        ctrt.offsetMin = new Vector2(10f, 0f);
        ctrt.offsetMax = new Vector2(-15f, 0f);
        TextMeshProUGUI ctTmp = coinTxtObj.GetComponent<TextMeshProUGUI>();
        ctTmp.text = $"💰 <color=#FFD700><b>{currentCoins}</b></color>";
        ctTmp.fontSize = 26f;
        ctTmp.alignment = TextAlignmentOptions.Center;

        // 3. Nút BACK màu đỏ hoàng gia ở góc trên bên phải
        GameObject backBtnObj = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
        backBtnObj.transform.SetParent(transform, false);
        RectTransform bbrt = backBtnObj.GetComponent<RectTransform>();
        bbrt.anchorMin = new Vector2(0.5f, 1f);
        bbrt.anchorMax = new Vector2(0.5f, 1f);
        bbrt.pivot = new Vector2(0.5f, 1f);
        bbrt.anchoredPosition = new Vector2(210f, -18f);
        bbrt.sizeDelta = new Vector2(140f, 48f);
        Image bbImg = backBtnObj.GetComponent<Image>();
        bbImg.sprite = UIController.GetGothicBanishButtonSprite();
        bbImg.type = Image.Type.Sliced;
        bbImg.color = Color.white;
        Button bBtn = backBtnObj.GetComponent<Button>();
        bBtn.onClick.AddListener(() =>
        {
            if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
            Destroy(gameObject);
            onBack?.Invoke();
        });

        GameObject bbTxtObj = new GameObject("BackText", typeof(RectTransform), typeof(TextMeshProUGUI));
        bbTxtObj.transform.SetParent(backBtnObj.transform, false);
        RectTransform bbtRt = bbTxtObj.GetComponent<RectTransform>();
        bbtRt.anchorMin = Vector2.zero;
        bbtRt.anchorMax = Vector2.one;
        bbtRt.offsetMin = Vector2.zero;
        bbtRt.offsetMax = Vector2.zero;
        TextMeshProUGUI bbTmp = bbTxtObj.GetComponent<TextMeshProUGUI>();
        bbTmp.text = "<b>BACK</b>";
        bbTmp.fontSize = 24f;
        bbTmp.alignment = TextAlignmentOptions.Center;
        bbTmp.color = new Color(1f, 0.94f, 0.88f);

        // 4. Khung Cửa Sổ Gothic Hoàng Gia "Collection" (Window)
        GameObject win = new GameObject("CollectionWindow", typeof(RectTransform), typeof(Image));
        win.transform.SetParent(transform, false);
        RectTransform winRt = win.GetComponent<RectTransform>();
        winRt.anchorMin = new Vector2(0.5f, 0.5f);
        winRt.anchorMax = new Vector2(0.5f, 0.5f);
        winRt.pivot = new Vector2(0.5f, 0.5f);
        winRt.anchoredPosition = new Vector2(0f, -15f);
        winRt.sizeDelta = new Vector2(860f, 620f);
        Image winImg = win.GetComponent<Image>();
        winImg.sprite = UIController.GetMysticAscensionWindowSprite();
        winImg.type = Image.Type.Sliced;
        winImg.color = Color.white;

        // Tiêu đề: Collection
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(win.transform, false);
        RectTransform trt = titleObj.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.5f, 1f);
        trt.anchorMax = new Vector2(0.5f, 1f);
        trt.pivot = new Vector2(0.5f, 1f);
        trt.anchoredPosition = new Vector2(0f, -22f);
        trt.sizeDelta = new Vector2(500f, 45f);
        TextMeshProUGUI tTmp = titleObj.GetComponent<TextMeshProUGUI>();
        tTmp.text = "✦  C O L L E C T I O N  ✦";
        tTmp.fontSize = 34f;
        tTmp.alignment = TextAlignmentOptions.Center;
        tTmp.color = new Color(1f, 0.88f, 0.32f);
        tTmp.characterSpacing = 3f;

        // 5. Khung Lưới Items (Grid 7 Cột x 5 Hàng = 35 Ô)
        GameObject gridObj = new GameObject("ItemsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObj.transform.SetParent(win.transform, false);
        RectTransform grt = gridObj.GetComponent<RectTransform>();
        grt.anchorMin = new Vector2(0.5f, 0.5f);
        grt.anchorMax = new Vector2(0.5f, 0.5f);
        grt.pivot = new Vector2(0.5f, 0.5f);
        grt.anchoredPosition = new Vector2(0f, 65f);
        grt.sizeDelta = new Vector2(740f, 340f);

        GridLayoutGroup glg = gridObj.GetComponent<GridLayoutGroup>();
        glg.cellSize = new Vector2(85f, 58f);
        glg.spacing = new Vector2(18f, 10f);
        glg.childAlignment = TextAnchor.MiddleCenter;

        // 6. Khung Preview Chi Tiết ở Đáy Cửa Sổ (Bottom Detail Box)
        GameObject detailBox = new GameObject("DetailBox", typeof(RectTransform), typeof(Image));
        detailBox.transform.SetParent(win.transform, false);
        RectTransform dbrt = detailBox.GetComponent<RectTransform>();
        dbrt.anchorMin = new Vector2(0.5f, 0f);
        dbrt.anchorMax = new Vector2(0.5f, 0f);
        dbrt.pivot = new Vector2(0.5f, 0f);
        dbrt.anchoredPosition = new Vector2(0f, 30f);
        dbrt.sizeDelta = new Vector2(760f, 105f);
        Image dbImg = detailBox.GetComponent<Image>();
        dbImg.sprite = LevelUpSellectionButton.GetGothicCardSprite(false);
        dbImg.type = Image.Type.Sliced;
        dbImg.color = Color.white;

        // Icon preview lớn bên trái
        GameObject prevSlot = new GameObject("PreviewSlot", typeof(RectTransform), typeof(Image));
        prevSlot.transform.SetParent(detailBox.transform, false);
        RectTransform psrt = prevSlot.GetComponent<RectTransform>();
        psrt.anchorMin = new Vector2(0f, 0.5f);
        psrt.anchorMax = new Vector2(0f, 0.5f);
        psrt.pivot = new Vector2(0.5f, 0.5f);
        psrt.anchoredPosition = new Vector2(62f, 0f);
        psrt.sizeDelta = new Vector2(74f, 74f);
        Image psImg = prevSlot.GetComponent<Image>();
        psImg.sprite = LevelUpSellectionButton.GetGothicIconSlotSprite();
        psImg.type = Image.Type.Sliced;

        GameObject prevIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        prevIcon.transform.SetParent(prevSlot.transform, false);
        RectTransform pirt = prevIcon.GetComponent<RectTransform>();
        pirt.anchorMin = Vector2.zero;
        pirt.anchorMax = Vector2.one;
        pirt.offsetMin = new Vector2(8f, 8f);
        pirt.offsetMax = new Vector2(-8f, -8f);
        previewIconImg = prevIcon.GetComponent<Image>();
        previewIconImg.preserveAspect = true;

        // Tiêu đề vật phẩm
        GameObject nameObj = new GameObject("ItemName", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(detailBox.transform, false);
        RectTransform nrt = nameObj.GetComponent<RectTransform>();
        nrt.anchorMin = new Vector2(0.18f, 0.52f);
        nrt.anchorMax = new Vector2(0.97f, 0.95f);
        nrt.offsetMin = Vector2.zero;
        nrt.offsetMax = Vector2.zero;
        previewNameTxt = nameObj.GetComponent<TextMeshProUGUI>();
        previewNameTxt.fontSize = 24f;
        previewNameTxt.color = new Color(1f, 0.88f, 0.25f);
        previewNameTxt.fontStyle = FontStyles.Bold;

        // Mô tả vật phẩm
        GameObject descObj = new GameObject("ItemDesc", typeof(RectTransform), typeof(TextMeshProUGUI));
        descObj.transform.SetParent(detailBox.transform, false);
        RectTransform drt = descObj.GetComponent<RectTransform>();
        drt.anchorMin = new Vector2(0.18f, 0.08f);
        drt.anchorMax = new Vector2(0.97f, 0.55f);
        drt.offsetMin = Vector2.zero;
        drt.offsetMax = Vector2.zero;
        previewDescTxt = descObj.GetComponent<TextMeshProUGUI>();
        previewDescTxt.fontSize = 17f;
        previewDescTxt.color = new Color(0.92f, 0.92f, 0.95f);
        previewDescTxt.textWrappingMode = TextWrappingModes.Normal;

        // 7. Khởi tạo 35 slots vật phẩm trong lưới
        for (int i = 0; i < catalog.Count && i < 35; i++)
        {
            CollectionItem item = catalog[i];
            bool unlocked = IsUnlocked(item.id);

            GameObject slot = new GameObject("Slot_" + i, typeof(RectTransform), typeof(Image), typeof(Button));
            slot.transform.SetParent(gridObj.transform, false);
            Image sImg = slot.GetComponent<Image>();
            sImg.sprite = LevelUpSellectionButton.GetGothicIconSlotSprite();
            sImg.type = Image.Type.Sliced;
            sImg.color = unlocked ? Color.white : new Color(0.45f, 0.45f, 0.55f, 0.85f);

            // Icon hoặc dấu ?
            GameObject iconChild = new GameObject("Content", typeof(RectTransform));
            iconChild.transform.SetParent(slot.transform, false);
            RectTransform icrt = iconChild.GetComponent<RectTransform>();
            icrt.anchorMin = Vector2.zero;
            icrt.anchorMax = Vector2.one;
            icrt.offsetMin = new Vector2(6f, 6f);
            icrt.offsetMax = new Vector2(-6f, -6f);

            if (unlocked)
            {
                Image iconImg = iconChild.AddComponent<Image>();
                iconImg.preserveAspect = true;
                iconImg.raycastTarget = false;
                Sprite s = Resources.Load<Sprite>(item.iconResourcePath);
                if (s != null) iconImg.sprite = s;
                else iconImg.sprite = LevelUpSellectionButton.GetPipDiamondSprite(true);
            }
            else
            {
                TextMeshProUGUI qTmp = iconChild.AddComponent<TextMeshProUGUI>();
                qTmp.text = "?";
                qTmp.fontSize = 28f;
                qTmp.alignment = TextAlignmentOptions.Center;
                qTmp.color = new Color(0.65f, 0.65f, 0.75f, 0.7f);
                qTmp.fontStyle = FontStyles.Bold;
                qTmp.raycastTarget = false;
            }

            // Gán sự kiện click vào ô
            Button btn = slot.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
                SelectItem(item, unlocked, slot.transform);
            });

            // Mặc định chọn ô đầu tiên
            if (i == 0)
            {
                SelectItem(item, unlocked, slot.transform);
            }
        }
    }

    private void SelectItem(CollectionItem item, bool unlocked, Transform slotT)
    {
        if (selectBracket == null)
        {
            selectBracket = new GameObject("SelectBracket", typeof(RectTransform), typeof(Image));
            RectTransform sbrt = selectBracket.GetComponent<RectTransform>();
            sbrt.sizeDelta = new Vector2(96f, 68f);
            Image sbImg = selectBracket.GetComponent<Image>();
            sbImg.sprite = LevelUpSellectionButton.GetGothicCardSprite(true);
            sbImg.type = Image.Type.Sliced;
            sbImg.color = new Color(1f, 0.9f, 0.3f, 0.9f);
            sbImg.raycastTarget = false;
        }

        selectBracket.transform.SetParent(slotT, false);
        selectBracket.transform.localPosition = Vector3.zero;
        selectBracket.SetActive(true);

        if (unlocked)
        {
            previewNameTxt.text = $"<color=#FFE040><b>{item.displayName}</b></color>";
            previewDescTxt.text = item.description;
            Sprite s = Resources.Load<Sprite>(item.iconResourcePath);
            previewIconImg.sprite = s != null ? s : LevelUpSellectionButton.GetPipDiamondSprite(true);
            previewIconImg.color = Color.white;
        }
        else
        {
            previewNameTxt.text = "<color=#8895A5><b>???</b></color>";
            previewDescTxt.text = "<i>Not yet found. Discover in battles or unlock through achievements.</i>";
            previewIconImg.sprite = null;
            previewIconImg.color = Color.clear;
        }
    }
}
