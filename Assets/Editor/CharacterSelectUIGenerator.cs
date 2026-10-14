#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace AlienSurvivors.EditorTools
{
    public static class CharacterSelectUIGenerator
    {
        [MenuItem("Tools/Generate Character Select UI")]
        public static void GenerateUI()
        {
            // -------------------------------------------------------------
            // 1. Canvas & Canvas Scaler Setup (Landscape 16:9 - 1920x1080)
            // -------------------------------------------------------------
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();
            GameObject canvasObj;

            if (canvas == null)
            {
                canvasObj = new GameObject("CharacterSelectCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            }
            else
            {
                canvasObj = canvas.gameObject;
                Undo.RecordObject(canvas, "Configure Canvas");
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObj.AddComponent<CanvasScaler>();
            }
            Undo.RecordObject(scaler, "Configure Canvas Scaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Ensure EventSystem exists
            EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
            }

            // -------------------------------------------------------------
            // 2. Crisp Retro Pixel Font Configuration (TextMeshPro)
            // -------------------------------------------------------------
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Udemy Vampire Survival Assets/Fonts/Kenney Fonts/Kenney Pixel SDF - Outline.asset");
            if (fontAsset == null)
            {
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Udemy Vampire Survival Assets/Fonts/Kenney Fonts/Kenney Pixel SDF.asset");
            }
            if (fontAsset == null)
            {
                string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset Pixel");
                if (fontGuids.Length > 0)
                {
                    fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
                }
            }

            if (fontAsset != null)
            {
                // Force Point filter on font atlas texture
                if (fontAsset.atlasTexture != null)
                {
                    fontAsset.atlasTexture.filterMode = FilterMode.Point;
                    string atlasPath = AssetDatabase.GetAssetPath(fontAsset.atlasTexture);
                    if (!string.IsNullOrEmpty(atlasPath))
                    {
                        TextureImporter importer = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
                        if (importer != null && importer.filterMode != FilterMode.Point)
                        {
                            importer.filterMode = FilterMode.Point;
                            importer.SaveAndReimport();
                        }
                    }
                }

                // Force Point filter and subtle black outline on font material
                if (fontAsset.material != null)
                {
                    if (fontAsset.material.mainTexture != null)
                    {
                        fontAsset.material.mainTexture.filterMode = FilterMode.Point;
                    }

                    fontAsset.material.EnableKeyword("OUTLINE_ON");
                    if (fontAsset.material.HasProperty(ShaderUtilities.ID_OutlineColor))
                    {
                        fontAsset.material.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0f, 0f, 0f, 0.95f));
                    }
                    if (fontAsset.material.HasProperty(ShaderUtilities.ID_OutlineWidth))
                    {
                        fontAsset.material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.15f);
                    }

                    EditorUtility.SetDirty(fontAsset.material);
                }
                EditorUtility.SetDirty(fontAsset);
            }

            // -------------------------------------------------------------
            // 3. Load Vampire Survivors Style Sprites
            // -------------------------------------------------------------
            Sprite windowFrameSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_WindowFrame_Gold.png");
            Sprite cardNormalSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Card_Normal.png");
            Sprite cardSelectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Card_Selected.png");
            Sprite confirmButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Button_Confirm.png");
            Sprite backButtonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Button_Back.png");
            Sprite subPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_SubPanel_Gold.png");
            Sprite coinBoxSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Coin_Box.png");
            Sprite coinIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Coin_Icon.png");
            Sprite coinSackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Icon_CoinSack.png");
            Sprite padlockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Icon_Padlock.png");
            Sprite badgeUnlockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Badge_Unlocked.png");

            // -------------------------------------------------------------
            // 4. Character Reordering Logic, Names & Data Loading
            // -------------------------------------------------------------
            // Slot 1: Name "Rowan" (Default Hero 1 - Unlocked) -> Char_LightBandit
            // Slot 2: Name "Kaelen" (Default Hero 2 - Unlocked) -> Char_FemaleAdventurer
            // Slot 3: Name "Valerius" (Locked - 100 Coins) -> Char_HeroKnight
            // Slot 4: Name "Seraphina" (Locked - 250 Coins) -> Char_EvilWizard
            // Slot 5: Name "Gorgon" (Locked - 500 Coins) -> Char_Antonio
            // Slot 6: Name "Morvath" (Locked - 800 Coins) -> Char_ForestRed
            string[] characterPaths = new string[]
            {
                "Assets/DataCharacters/Char_LightBandit.asset",
                "Assets/DataCharacters/Char_FemaleAdventurer.asset",
                "Assets/DataCharacters/Char_HeroKnight.asset",
                "Assets/DataCharacters/Char_EvilWizard.asset",
                "Assets/DataCharacters/Char_Antonio.asset",
                "Assets/DataCharacters/Char_ForestRed.asset"
            };

            string[] characterDisplayNames = new string[]
            {
                "Rowan",
                "Kaelen",
                "Valerius",
                "Seraphina",
                "Gorgon",
                "Morvath"
            };

            int[] unlockCosts = new int[] { 0, 0, 100, 250, 500, 800 };

            CharacterData[] characters = new CharacterData[characterPaths.Length];
            for (int i = 0; i < characterPaths.Length; i++)
            {
                characters[i] = AssetDatabase.LoadAssetAtPath<CharacterData>(characterPaths[i]);
                if (characters[i] == null)
                {
                    Debug.LogWarning($"[CharacterSelectUIGenerator] Missing CharacterData at path: {characterPaths[i]}");
                }
                else
                {
                    // Enforce Point filtering on character and weapon sprites for crisp 1:1 pixel rendering
                    EnforcePointFilter(characters[i].characterSprite);
                    EnforcePointFilter(characters[i].weaponSprite);
                }
            }

            // -------------------------------------------------------------
            // 5. Main Character Selection Panel (Dark Backdrop)
            // -------------------------------------------------------------
            Transform existingPanel = canvasObj.transform.Find("CharacterSelectionPanel");
            GameObject panelObj;
            if (existingPanel != null)
            {
                panelObj = existingPanel.gameObject;
                Undo.RecordObject(panelObj, "Update CharacterSelectionPanel");
            }
            else
            {
                panelObj = new GameObject("CharacterSelectionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                panelObj.transform.SetParent(canvasObj.transform, false);
                Undo.RegisterCreatedObjectUndo(panelObj, "Create CharacterSelectionPanel");
            }

            panelObj.SetActive(true);

            RectTransform panelRt = panelObj.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Image panelBg = panelObj.GetComponent<Image>();
            if (panelBg == null) panelBg = panelObj.AddComponent<Image>();
            panelBg.color = new Color(0.04f, 0.04f, 0.07f, 0.92f);

            // -------------------------------------------------------------
            // 6. Main Container (Dark Blue Modal Window - 880x960 Landscape)
            // -------------------------------------------------------------
            // Occupies ~46% screen width and ~89% screen height, leaving ample space on both sides
            Transform existingWb = panelObj.transform.Find("WindowBox");
            GameObject wbObj = existingWb != null ? existingWb.gameObject : new GameObject("WindowBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wbObj.transform.SetParent(panelObj.transform, false);
            RectTransform wbRt = wbObj.GetComponent<RectTransform>();
            wbRt.anchorMin = new Vector2(0.5f, 0.5f);
            wbRt.anchorMax = new Vector2(0.5f, 0.5f);
            wbRt.pivot = new Vector2(0.5f, 0.5f);
            wbRt.sizeDelta = new Vector2(880f, 960f);
            wbRt.anchoredPosition = new Vector2(0f, -40f);

            Image wbImg = wbObj.GetComponent<Image>();
            wbImg.sprite = windowFrameSprite;
            wbImg.type = Image.Type.Sliced;
            wbImg.color = Color.white;

            // Header Title "Character Selection" in bold white pixel typography
            Transform existingTitle = wbObj.transform.Find("Title");
            GameObject titleObj = existingTitle != null ? existingTitle.gameObject : new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(wbObj.transform, false);
            RectTransform titleRt = titleObj.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 1f);
            titleRt.anchorMax = new Vector2(0.5f, 1f);
            titleRt.pivot = new Vector2(0.5f, 1f);
            titleRt.anchoredPosition = new Vector2(0f, -18f);
            titleRt.sizeDelta = new Vector2(800f, 48f);

            TMP_Text titleTMP = titleObj.GetComponent<TMP_Text>();
            titleTMP.text = "Character Selection";
            titleTMP.fontSize = 36f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = Color.white;
            if (fontAsset != null) titleTMP.font = fontAsset;

            // -------------------------------------------------------------
            // 7. Top Bar (Positioned Directly Above Main Window)
            // -------------------------------------------------------------
            // Gold-bordered Coin Display with green money bag icon and large white pixel digits
            Transform existingCoinBox = panelObj.transform.Find("TopCoinBox");
            GameObject coinBoxObj = existingCoinBox != null ? existingCoinBox.gameObject : new GameObject("TopCoinBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            coinBoxObj.transform.SetParent(panelObj.transform, false);
            RectTransform cbRt = coinBoxObj.GetComponent<RectTransform>();
            cbRt.anchorMin = new Vector2(0.5f, 0.5f);
            cbRt.anchorMax = new Vector2(0.5f, 0.5f);
            cbRt.pivot = new Vector2(0.5f, 0.5f);
            cbRt.anchoredPosition = new Vector2(-40f, 478f);
            cbRt.sizeDelta = new Vector2(240f, 58f);

            Image cbImg = coinBoxObj.GetComponent<Image>();
            cbImg.sprite = coinBoxSprite;
            cbImg.type = Image.Type.Sliced;
            cbImg.color = Color.white;

            // Green Money Bag Icon
            Transform existingSack = coinBoxObj.transform.Find("CoinSackIcon");
            GameObject sackObj = existingSack != null ? existingSack.gameObject : new GameObject("CoinSackIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            sackObj.transform.SetParent(coinBoxObj.transform, false);
            RectTransform sackRt = sackObj.GetComponent<RectTransform>();
            sackRt.anchorMin = new Vector2(0f, 0.5f);
            sackRt.anchorMax = new Vector2(0f, 0.5f);
            sackRt.pivot = new Vector2(0f, 0.5f);
            sackRt.anchoredPosition = new Vector2(10f, 0f);
            sackRt.sizeDelta = new Vector2(42f, 42f);
            Image sackImg = sackObj.GetComponent<Image>();
            sackImg.sprite = coinSackSprite ?? coinIconSprite;
            sackImg.preserveAspect = true;
            sackImg.color = Color.white;

            // High-contrast large white pixel digits
            Transform existingCoinText = coinBoxObj.transform.Find("CoinText");
            GameObject coinTextObj = existingCoinText != null ? existingCoinText.gameObject : new GameObject("CoinText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            coinTextObj.transform.SetParent(coinBoxObj.transform, false);
            RectTransform ctRt = coinTextObj.GetComponent<RectTransform>();
            ctRt.anchorMin = new Vector2(0f, 0f);
            ctRt.anchorMax = new Vector2(1f, 1f);
            ctRt.pivot = new Vector2(1f, 0.5f);
            ctRt.anchoredPosition = new Vector2(-15f, 0f);
            ctRt.sizeDelta = new Vector2(-60f, 0f);
            TMP_Text coinTxt = coinTextObj.GetComponent<TMP_Text>();
            int savedCoins = PlayerPrefs.GetInt("SavedCoins", 0);
            coinTxt.text = savedCoins.ToString();
            coinTxt.fontSize = 24f;
            coinTxt.fontStyle = FontStyles.Bold;
            coinTxt.alignment = TextAlignmentOptions.MidlineRight;
            coinTxt.color = Color.white;
            if (fontAsset != null) coinTxt.font = fontAsset;

            // Rounded-corner red button with a gold rim and bold white pixel text reading "BACK"
            Transform existingBack = panelObj.transform.Find("BackButton");
            GameObject backObj = existingBack != null ? existingBack.gameObject : new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            backObj.transform.SetParent(panelObj.transform, false);
            RectTransform backRt = backObj.GetComponent<RectTransform>();
            backRt.anchorMin = new Vector2(0.5f, 0.5f);
            backRt.anchorMax = new Vector2(0.5f, 0.5f);
            backRt.pivot = new Vector2(0.5f, 0.5f);
            backRt.anchoredPosition = new Vector2(260f, 478f);
            backRt.sizeDelta = new Vector2(150f, 56f);

            Image backImg = backObj.GetComponent<Image>();
            backImg.sprite = backButtonSprite;
            backImg.type = Image.Type.Sliced;
            backImg.color = Color.white;
            Button backButton = backObj.GetComponent<Button>();

            Transform existingBackText = backObj.transform.Find("Text");
            GameObject backTextObj = existingBackText != null ? existingBackText.gameObject : new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            backTextObj.transform.SetParent(backObj.transform, false);
            RectTransform btRt = backTextObj.GetComponent<RectTransform>();
            btRt.anchorMin = Vector2.zero;
            btRt.anchorMax = Vector2.one;
            btRt.sizeDelta = Vector2.zero;
            btRt.anchoredPosition = Vector2.zero;
            TMP_Text backTxt = backTextObj.GetComponent<TMP_Text>();
            backTxt.text = "BACK";
            backTxt.fontSize = 24f;
            backTxt.fontStyle = FontStyles.Bold;
            backTxt.alignment = TextAlignmentOptions.Center;
            backTxt.color = Color.white;
            if (fontAsset != null) backTxt.font = fontAsset;

            // -------------------------------------------------------------
            // 8. Character Grid Container (2 Rows x 3 Columns)
            // -------------------------------------------------------------
            Transform existingGrid = wbObj.transform.Find("CharacterGrid");
            GameObject gridObj = existingGrid != null ? existingGrid.gameObject : new GameObject("CharacterGrid", typeof(RectTransform));
            gridObj.transform.SetParent(wbObj.transform, false);
            RectTransform gridRt = gridObj.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0.5f, 1f);
            gridRt.anchorMax = new Vector2(0.5f, 1f);
            gridRt.pivot = new Vector2(0.5f, 1f);
            gridRt.anchoredPosition = new Vector2(0f, -70f);
            gridRt.sizeDelta = new Vector2(820f, 600f);

            GridLayoutGroup glg = gridObj.GetComponent<GridLayoutGroup>();
            if (glg == null) glg = gridObj.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(250f, 280f);
            glg.spacing = new Vector2(18f, 16f);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 3;
            glg.childAlignment = TextAnchor.UpperCenter;

            // -------------------------------------------------------------
            // 9. Generate 6 Character Slot Cards
            // -------------------------------------------------------------
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterData charData = characters[i];
                bool isUnlocked = (i < 2);
                int cost = unlockCosts[i];
                string characterName = characterDisplayNames[i];

                string slotName = $"Slot_{i}";
                Transform existingSlot = gridObj.transform.Find(slotName);
                GameObject slot = existingSlot != null ? existingSlot.gameObject : new GameObject(slotName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                slot.transform.SetParent(gridObj.transform, false);
                slot.SetActive(true);

                // 1. Card Background & Golden Selection Frame Brackets
                Image slotCardImg = slot.GetComponent<Image>();
                slotCardImg.type = Image.Type.Sliced;
                slotCardImg.sprite = (i == 0) ? cardSelectedSprite : cardNormalSprite;
                slotCardImg.color = Color.white;

                // 2. Name Label at Top-Left of Card (Rowan, Kaelen, Valerius...)
                Transform labelT = slot.transform.Find("NameLabel");
                GameObject labelObj = labelT != null ? labelT.gameObject : new GameObject("NameLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(slot.transform, false);
                RectTransform labelRt = labelObj.GetComponent<RectTransform>();
                labelRt.anchorMin = new Vector2(0f, 1f);
                labelRt.anchorMax = new Vector2(0f, 1f);
                labelRt.pivot = new Vector2(0f, 1f);
                labelRt.anchoredPosition = new Vector2(12f, -8f);
                labelRt.sizeDelta = new Vector2(185f, 38f);

                TMP_Text labelTxt = labelObj.GetComponent<TMP_Text>();
                labelTxt.text = characterName;
                labelTxt.fontSize = 30f;
                labelTxt.fontStyle = FontStyles.Bold;
                labelTxt.alignment = TextAlignmentOptions.TopLeft;
                labelTxt.textWrappingMode = TextWrappingModes.NoWrap;
                labelTxt.color = Color.white;
                if (fontAsset != null) labelTxt.font = fontAsset;

                // 3. Golden Padlock Icon (top-right corner for locked slots)
                Transform lockT = slot.transform.Find("PadlockIcon");
                GameObject lockObj = lockT != null ? lockT.gameObject : new GameObject("PadlockIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                lockObj.transform.SetParent(slot.transform, false);
                RectTransform lockRt = lockObj.GetComponent<RectTransform>();
                lockRt.anchorMin = new Vector2(1f, 1f);
                lockRt.anchorMax = new Vector2(1f, 1f);
                lockRt.pivot = new Vector2(1f, 1f);
                lockRt.anchoredPosition = new Vector2(-10f, -8f);
                lockRt.sizeDelta = new Vector2(26f, 26f);

                Image lockImg = lockObj.GetComponent<Image>();
                lockImg.sprite = padlockSprite;
                lockImg.preserveAspect = true;
                lockImg.color = Color.white;
                lockObj.SetActive(!isUnlocked);

                // 4. Character Sprite (fully visible colored if unlocked, dark silhouette if locked)
                Transform avatarT = slot.transform.Find("Avatar");
                GameObject avatarObj = avatarT != null ? avatarT.gameObject : new GameObject("Avatar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                avatarObj.transform.SetParent(slot.transform, false);
                RectTransform avRt = avatarObj.GetComponent<RectTransform>();
                avRt.anchorMin = new Vector2(0.5f, 0.5f);
                avRt.anchorMax = new Vector2(0.5f, 0.5f);
                avRt.pivot = new Vector2(0.5f, 0.5f);
                avRt.anchoredPosition = new Vector2(-22f, 8f);
                avRt.sizeDelta = new Vector2(96f, 96f);

                Image avImg = avatarObj.GetComponent<Image>();
                if (charData != null) avImg.sprite = charData.characterSprite;
                avImg.preserveAspect = true;
                avImg.color = isUnlocked ? Color.white : new Color(0.10f, 0.12f, 0.16f, 1f);

                // 5. Dedicated Framed Weapon Box & Weapon Sprite (colored vs silhouette)
                Transform wpBoxT = slot.transform.Find("WeaponBox");
                GameObject wpBoxObj = wpBoxT != null ? wpBoxT.gameObject : new GameObject("WeaponBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                wpBoxObj.transform.SetParent(slot.transform, false);
                RectTransform wpBoxRt = wpBoxObj.GetComponent<RectTransform>();
                wpBoxRt.anchorMin = new Vector2(0.5f, 0.5f);
                wpBoxRt.anchorMax = new Vector2(0.5f, 0.5f);
                wpBoxRt.pivot = new Vector2(0f, 0.5f);
                wpBoxRt.anchoredPosition = new Vector2(42f, 8f);
                wpBoxRt.sizeDelta = new Vector2(42f, 42f);

                Image wpBoxImg = wpBoxObj.GetComponent<Image>();
                wpBoxImg.sprite = subPanelSprite;
                wpBoxImg.type = Image.Type.Sliced;
                wpBoxImg.color = Color.white;

                Transform wpIconT = wpBoxObj.transform.Find("WeaponIcon");
                GameObject wpIconObj = wpIconT != null ? wpIconT.gameObject : new GameObject("WeaponIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                wpIconObj.transform.SetParent(wpBoxObj.transform, false);
                RectTransform wpIconRt = wpIconObj.GetComponent<RectTransform>();
                wpIconRt.anchorMin = Vector2.zero;
                wpIconRt.anchorMax = Vector2.one;
                wpIconRt.sizeDelta = new Vector2(-6f, -6f);
                wpIconRt.anchoredPosition = Vector2.zero;

                Image wpIconImg = wpIconObj.GetComponent<Image>();
                if (charData != null) wpIconImg.sprite = charData.weaponSprite;
                wpIconImg.preserveAspect = true;
                wpIconImg.color = isUnlocked ? Color.white : new Color(0.10f, 0.12f, 0.16f, 0.9f);

                // 6. Bottom Status: Vibrant Green "UNLOCKED" Badge OR Price Tag with Coin Icon
                Transform statusT = slot.transform.Find("BottomStatus");
                GameObject statusObj = statusT != null ? statusT.gameObject : new GameObject("BottomStatus", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                statusObj.transform.SetParent(slot.transform, false);
                RectTransform statusRt = statusObj.GetComponent<RectTransform>();
                statusRt.anchorMin = new Vector2(0.5f, 0f);
                statusRt.anchorMax = new Vector2(0.5f, 0f);
                statusRt.pivot = new Vector2(0.5f, 0f);
                statusRt.anchoredPosition = new Vector2(0f, 12f);
                statusRt.sizeDelta = new Vector2(180f, 32f);

                Image statusImg = statusObj.GetComponent<Image>();
                statusImg.type = Image.Type.Sliced;

                Transform stTextT = statusObj.transform.Find("StatusText");
                GameObject stTextObj = stTextT != null ? stTextT.gameObject : new GameObject("StatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                stTextObj.transform.SetParent(statusObj.transform, false);
                RectTransform stTextRt = stTextObj.GetComponent<RectTransform>();

                Transform stCoinT = statusObj.transform.Find("StatusCoin");
                GameObject stCoinObj = stCoinT != null ? stCoinT.gameObject : new GameObject("StatusCoin", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                stCoinObj.transform.SetParent(statusObj.transform, false);
                RectTransform stCoinRt = stCoinObj.GetComponent<RectTransform>();
                stCoinRt.anchorMin = new Vector2(1f, 0.5f);
                stCoinRt.anchorMax = new Vector2(1f, 0.5f);
                stCoinRt.pivot = new Vector2(1f, 0.5f);
                stCoinRt.anchoredPosition = new Vector2(-12f, 0f);
                stCoinRt.sizeDelta = new Vector2(22f, 22f);
                Image stCoinImg = stCoinObj.GetComponent<Image>();
                stCoinImg.sprite = coinIconSprite;
                stCoinImg.preserveAspect = true;

                TMP_Text stText = stTextObj.GetComponent<TMP_Text>();
                if (fontAsset != null) stText.font = fontAsset;

                if (isUnlocked)
                {
                    statusImg.sprite = badgeUnlockedSprite;
                    statusImg.color = Color.white;
                    stCoinObj.SetActive(false);

                    stTextRt.anchorMin = Vector2.zero;
                    stTextRt.anchorMax = Vector2.one;
                    stTextRt.sizeDelta = Vector2.zero;
                    stTextRt.anchoredPosition = Vector2.zero;

                    stText.text = "UNLOCKED";
                    stText.fontSize = 18f;
                    stText.fontStyle = FontStyles.Bold;
                    stText.alignment = TextAlignmentOptions.Center;
                    stText.color = Color.white;
                }
                else
                {
                    statusImg.sprite = coinBoxSprite ?? subPanelSprite;
                    statusImg.color = Color.white;
                    stCoinObj.SetActive(true);

                    stTextRt.anchorMin = new Vector2(0f, 0f);
                    stTextRt.anchorMax = new Vector2(1f, 1f);
                    stTextRt.sizeDelta = new Vector2(-46f, 0f);
                    stTextRt.anchoredPosition = new Vector2(-10f, 0f);

                    stText.text = $"{cost:N0}";
                    stText.fontSize = 20f;
                    stText.fontStyle = FontStyles.Bold;
                    stText.alignment = TextAlignmentOptions.Center;
                    stText.color = new Color(1f, 0.88f, 0.25f);
                }
            }

            // Hide any unused slots beyond 6
            for (int i = characters.Length; i < gridObj.transform.childCount; i++)
            {
                gridObj.transform.GetChild(i).gameObject.SetActive(false);
            }

            // -------------------------------------------------------------
            // 10. Bottom Character Detail Panel
            // -------------------------------------------------------------
            Transform existingBottom = wbObj.transform.Find("BottomDetailPanel");
            GameObject bottomObj = existingBottom != null ? existingBottom.gameObject : new GameObject("BottomDetailPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bottomObj.transform.SetParent(wbObj.transform, false);
            RectTransform bottomRt = bottomObj.GetComponent<RectTransform>();
            bottomRt.anchorMin = new Vector2(0.5f, 0f);
            bottomRt.anchorMax = new Vector2(0.5f, 0f);
            bottomRt.pivot = new Vector2(0.5f, 0f);
            bottomRt.anchoredPosition = new Vector2(0f, 18f);
            bottomRt.sizeDelta = new Vector2(830f, 220f);

            Image bottomImg = bottomObj.GetComponent<Image>();
            bottomImg.sprite = subPanelSprite;
            bottomImg.type = Image.Type.Sliced;
            bottomImg.color = Color.white;

            // Character Name Text
            Transform existingName = bottomObj.transform.Find("NameText");
            GameObject nameObj = existingName != null ? existingName.gameObject : new GameObject("NameText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            nameObj.transform.SetParent(bottomObj.transform, false);
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(0f, 1f);
            nameRt.pivot = new Vector2(0f, 1f);
            nameRt.anchoredPosition = new Vector2(24f, -12f);
            nameRt.sizeDelta = new Vector2(500f, 44f);
            TMP_Text nameText = nameObj.GetComponent<TMP_Text>();
            nameText.text = characterDisplayNames[0]; // "Rowan"
            nameText.fontSize = 32f;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = new Color(1f, 0.95f, 0.85f);
            nameText.alignment = TextAlignmentOptions.Left;
            if (fontAsset != null) nameText.font = fontAsset;

            // Selected Character Sprite Preview
            Transform existingAvPrev = bottomObj.transform.Find("AvatarPreview");
            GameObject avPrevObj = existingAvPrev != null ? existingAvPrev.gameObject : new GameObject("AvatarPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            avPrevObj.transform.SetParent(bottomObj.transform, false);
            RectTransform avPrevRt = avPrevObj.GetComponent<RectTransform>();
            avPrevRt.anchorMin = new Vector2(0f, 0.5f);
            avPrevRt.anchorMax = new Vector2(0f, 0.5f);
            avPrevRt.pivot = new Vector2(0.5f, 0.5f);
            avPrevRt.anchoredPosition = new Vector2(65f, -14f);
            avPrevRt.sizeDelta = new Vector2(90f, 90f);
            Image charPreview = avPrevObj.GetComponent<Image>();
            if (characters[0] != null) charPreview.sprite = characters[0].characterSprite;
            charPreview.preserveAspect = true;
            charPreview.color = Color.white;

            // Dedicated Framed Weapon Preview Box
            Transform obsoleteDirectWp = bottomObj.transform.Find("WeaponPreview");
            if (obsoleteDirectWp != null)
            {
                Object.DestroyImmediate(obsoleteDirectWp.gameObject);
            }

            Transform existingWpPrevBox = bottomObj.transform.Find("WeaponPreviewBox");
            GameObject wpPrevBoxObj = existingWpPrevBox != null ? existingWpPrevBox.gameObject : new GameObject("WeaponPreviewBox", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wpPrevBoxObj.transform.SetParent(bottomObj.transform, false);
            RectTransform wpPrevBoxRt = wpPrevBoxObj.GetComponent<RectTransform>();
            wpPrevBoxRt.anchorMin = new Vector2(0f, 0.5f);
            wpPrevBoxRt.anchorMax = new Vector2(0f, 0.5f);
            wpPrevBoxRt.pivot = new Vector2(0.5f, 0.5f);
            wpPrevBoxRt.anchoredPosition = new Vector2(145f, -14f);
            wpPrevBoxRt.sizeDelta = new Vector2(46f, 46f);
            Image wpPrevBoxImg = wpPrevBoxObj.GetComponent<Image>();
            wpPrevBoxImg.sprite = subPanelSprite;
            wpPrevBoxImg.type = Image.Type.Sliced;
            wpPrevBoxImg.color = Color.white;

            Transform existingWpPrev = wpPrevBoxObj.transform.Find("WeaponPreview");
            GameObject wpPrevObj = existingWpPrev != null ? existingWpPrev.gameObject : new GameObject("WeaponPreview", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            wpPrevObj.transform.SetParent(wpPrevBoxObj.transform, false);
            RectTransform wpPrevRt = wpPrevObj.GetComponent<RectTransform>();
            wpPrevRt.anchorMin = Vector2.zero;
            wpPrevRt.anchorMax = Vector2.one;
            wpPrevRt.sizeDelta = new Vector2(-8f, -8f);
            wpPrevRt.anchoredPosition = Vector2.zero;
            Image weaponPreview = wpPrevObj.GetComponent<Image>();
            if (characters[0] != null) weaponPreview.sprite = characters[0].weaponSprite;
            weaponPreview.preserveAspect = true;
            weaponPreview.color = Color.white;

            // Passive Skills & Lore Description
            Transform existingDesc = bottomObj.transform.Find("DescText");
            GameObject descObj = existingDesc != null ? existingDesc.gameObject : new GameObject("DescText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            descObj.transform.SetParent(bottomObj.transform, false);
            RectTransform descRt = descObj.GetComponent<RectTransform>();
            descRt.anchorMin = new Vector2(0f, 0.5f);
            descRt.anchorMax = new Vector2(1f, 0.5f);
            descRt.pivot = new Vector2(0f, 0.5f);
            descRt.anchoredPosition = new Vector2(205f, -14f);
            descRt.sizeDelta = new Vector2(-385f, 130f);
            TMP_Text descText = descObj.GetComponent<TMP_Text>();
            descText.text = characters[0] != null ? characters[0].passiveDescription : "Shadow Assassin: +20% move speed and +15% critical chance. Rapid throwing daggers.";
            descText.fontSize = 24f;
            descText.color = Color.white;
            descText.textWrappingMode = TextWrappingModes.Normal;
            descText.alignment = TextAlignmentOptions.MidlineLeft;
            if (fontAsset != null) descText.font = fontAsset;

            // Confirm Button (large green retro button with double borders)
            Transform existingConfirm = bottomObj.transform.Find("ConfirmButton");
            GameObject confirmObj = existingConfirm != null ? existingConfirm.gameObject : new GameObject("ConfirmButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            confirmObj.transform.SetParent(bottomObj.transform, false);
            RectTransform confirmRt = confirmObj.GetComponent<RectTransform>();
            confirmRt.anchorMin = new Vector2(1f, 0.5f);
            confirmRt.anchorMax = new Vector2(1f, 0.5f);
            confirmRt.pivot = new Vector2(1f, 0.5f);
            confirmRt.anchoredPosition = new Vector2(-25f, -14f);
            confirmRt.sizeDelta = new Vector2(145f, 56f);

            Image confirmImg = confirmObj.GetComponent<Image>();
            confirmImg.sprite = confirmButtonSprite;
            confirmImg.type = Image.Type.Sliced;
            confirmImg.color = Color.white;
            Button confirmButton = confirmObj.GetComponent<Button>();

            Transform existingConfirmText = confirmObj.transform.Find("Text");
            GameObject confirmTextObj = existingConfirmText != null ? existingConfirmText.gameObject : new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            confirmTextObj.transform.SetParent(confirmObj.transform, false);
            RectTransform cbtRt = confirmTextObj.GetComponent<RectTransform>();
            cbtRt.anchorMin = Vector2.zero;
            cbtRt.anchorMax = Vector2.one;
            cbtRt.sizeDelta = Vector2.zero;
            cbtRt.anchoredPosition = Vector2.zero;
            TMP_Text confirmTxt = confirmTextObj.GetComponent<TMP_Text>();
            confirmTxt.text = "Confirm";
            confirmTxt.fontSize = 24f;
            confirmTxt.fontStyle = FontStyles.Bold;
            confirmTxt.alignment = TextAlignmentOptions.Center;
            confirmTxt.color = Color.white;
            if (fontAsset != null) confirmTxt.font = fontAsset;

            // -------------------------------------------------------------
            // 11. Attach & Wire Up CharacterSelectManager Component
            // -------------------------------------------------------------
            CharacterSelectManager manager = panelObj.GetComponent<CharacterSelectManager>();
            if (manager == null) manager = panelObj.AddComponent<CharacterSelectManager>();

            SerializedObject so = new SerializedObject(manager);

            // Assign characters array in exact order
            SerializedProperty charProp = so.FindProperty("characters");
            charProp.arraySize = characters.Length;
            for (int i = 0; i < characters.Length; i++)
            {
                charProp.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
            }

            // Assign UI references
            so.FindProperty("nameText").objectReferenceValue = nameText;
            so.FindProperty("descriptionText").objectReferenceValue = descText;
            so.FindProperty("characterPreview").objectReferenceValue = charPreview;
            so.FindProperty("weaponPreview").objectReferenceValue = weaponPreview;
            so.FindProperty("confirmButton").objectReferenceValue = confirmButton;
            so.FindProperty("backButton").objectReferenceValue = backButton;
            so.FindProperty("windowBox").objectReferenceValue = wbRt;
            so.FindProperty("gridTransform").objectReferenceValue = gridRt;
            so.FindProperty("bottomDetailPanel").objectReferenceValue = bottomRt;
            so.FindProperty("titleText").objectReferenceValue = titleTMP;

            // Assign theme sprites
            so.FindProperty("windowFrameSprite").objectReferenceValue = windowFrameSprite;
            so.FindProperty("cardNormalSprite").objectReferenceValue = cardNormalSprite;
            so.FindProperty("cardSelectedSprite").objectReferenceValue = cardSelectedSprite;
            so.FindProperty("confirmButtonSprite").objectReferenceValue = confirmButtonSprite;
            so.FindProperty("backButtonSprite").objectReferenceValue = backButtonSprite;
            so.FindProperty("subPanelSprite").objectReferenceValue = subPanelSprite;
            so.FindProperty("coinBoxSprite").objectReferenceValue = coinBoxSprite;
            so.FindProperty("coinIconSprite").objectReferenceValue = coinIconSprite;
            so.FindProperty("coinSackSprite").objectReferenceValue = coinSackSprite;
            so.FindProperty("padlockSprite").objectReferenceValue = padlockSprite;
            so.FindProperty("badgeUnlockedSprite").objectReferenceValue = badgeUnlockedSprite;
            so.FindProperty("fontAsset").objectReferenceValue = fontAsset;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(panelObj);

            // Mark Scene Dirty for Save
            EditorSceneManager.MarkSceneDirty(panelObj.scene);
            Selection.activeGameObject = panelObj;

            Debug.Log("[CharacterSelectUIGenerator] Successfully updated Character Selection UI to Landscape 16:9!");
            EditorUtility.DisplayDialog(
                "Character Select UI Generated",
                "Character Selection UI has been successfully updated to Landscape 16:9 (1920x1080)!\n\n" +
                "• Window Size: 880x960 (~46% width, ~89% height)\n" +
                "• Grid: 2 Rows x 3 Columns (Cells: 250x280)\n" +
                "• Character Slots:\n" +
                "  1. Rowan (Unlocked, Default Hero 1)\n" +
                "  2. Kaelen (Unlocked, Default Hero 2)\n" +
                "  3. Valerius (Locked, 100 Coins)\n" +
                "  4. Seraphina (Locked, 250 Coins)\n" +
                "  5. Gorgon (Locked, 500 Coins)\n" +
                "  6. Morvath (Locked, 800 Coins)\n\n" +
                "Press Ctrl + S to save the scene.",
                "Great!"
            );
        }

        private static void EnforcePointFilter(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;
            sprite.texture.filterMode = FilterMode.Point;
            string path = AssetDatabase.GetAssetPath(sprite.texture);
            if (!string.IsNullOrEmpty(path))
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && importer.filterMode != FilterMode.Point)
                {
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
#endif
