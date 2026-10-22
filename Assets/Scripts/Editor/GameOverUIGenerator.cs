#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AlienSurvivors.EditorTools
{
    // Rebuilds the Game Over (Level End Panel) and Pause screens in the gold-framed retro pixel style
    // used by Character Select. Existing buttons are reused so their onClick wiring to UIController is kept.
    // Safe to run again: objects are found by name and restyled instead of duplicated.
    // Applies the generator automatically whenever a scene with an unstyled Game Over screen is open,
    // so the new UI shows up without anyone having to find the menu item. The scene is marked dirty; save to keep it.
    [InitializeOnLoad]
    static class GameOverUIAutoApply
    {
        static GameOverUIAutoApply()
        {
            EditorSceneManager.sceneOpened += (scene, mode) => Queue();
            Queue();
        }

        static void Queue()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
                UIController ui = Object.FindAnyObjectByType<UIController>();
                if (ui == null || ui.levelEndScreen == null) return;
                bool built = ui.levelEndScreen.GetComponent<GameOverScreen>() != null;
                TMP_FontAsset body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
                bool cardFontStale = ui.levelUpButtons != null && ui.levelUpButtons.Length > 0 && ui.levelUpButtons[0] != null
                    && ui.levelUpButtons[0].nameLevelText != null && body != null && ui.levelUpButtons[0].nameLevelText.font != body;
                bool saveButtonMissing = ui.pauseScreen != null && ui.pauseScreen.transform.Find("Window/Save And Exit") == null;
                if (built && !cardFontStale && !saveButtonMissing) return;
                GameOverUIGenerator.Generate(true);
            };
        }
    }

    public static class GameOverUIGenerator
    {
        static readonly Color Gold = new Color(1f, 0.88f, 0.25f);
        static readonly Color GoldShadow = new Color(0.36f, 0.22f, 0.02f);
        static readonly Color BloodRed = new Color(0.95f, 0.22f, 0.18f);
        static readonly Color BloodShadow = new Color(0.28f, 0.02f, 0.04f);
        static readonly Color Muted = new Color(0.62f, 0.67f, 0.8f);
        static readonly Color Line = new Color(1f, 0.88f, 0.25f, 0.55f);

        static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
        static readonly Vector2 TopCenter = new Vector2(0.5f, 1f);
        static readonly Vector2 BottomCenter = new Vector2(0.5f, 0f);

        static TMP_FontAsset font, bodyFont; // font = pixel (big headings only), bodyFont = smooth (everything small)
        static Sprite windowSprite, subPanelSprite, confirmSprite, backSprite, badgeSprite, coinSprite;

        [MenuItem("Tools/Survivors/Generate Game Over & Pause UI")]
        public static void Generate() { Generate(false); }

        public static void Generate(bool silent)
        {
            UIController ui = Object.FindAnyObjectByType<UIController>();
            if (ui == null)
            {
                EditorUtility.DisplayDialog("Error", "UIController not found in current scene! Open the Main scene first.", "OK");
                return;
            }

            LoadAssets();

            if (ui.levelEndScreen != null) BuildGameOver(ui.levelEndScreen);
            if (ui.pauseScreen != null) BuildPause(ui.pauseScreen);
            if (ui.levelUpPanel != null) BuildLevelUp(ui.levelUpPanel);

            EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            Selection.activeGameObject = ui.levelEndScreen;

            Debug.Log("[GameOverUIGenerator] Game Over, Pause & Level Up screens restyled in retro gold style. Save the scene (Ctrl+S) to keep it.");
            if (silent) return;
            EditorUtility.DisplayDialog("Game Over & Pause UI Generated",
                "Both screens now use the gold window frame, pixel font and retro buttons.\n\n" +
                "• Game Over: GAME OVER title, time survived, level / coins / best time, NEW BEST badge\n" +
                "• Pause: framed menu with Resume / Restart / Main Menu / Quit\n\n" +
                "Tip: enable 'Level End Panel' temporarily to preview it.\n" +
                "Press Ctrl + S to save the scene.", "Great!");
        }

        // -------------------------------------------------------------
        // Game Over
        // -------------------------------------------------------------
        static void BuildGameOver(GameObject panel)
        {
            Undo.RegisterFullObjectHierarchyUndo(panel, "Generate Game Over UI");
            Transform root = panel.transform;

            Stretch(panel);
            Img(panel, null, new Color(0.06f, 0.01f, 0.03f, 0.88f)).raycastTarget = true; // Blocks clicks to the HUD underneath
            CanvasGroup group = EnsureComponent<CanvasGroup>(panel);

            // Window
            GameObject window = Ensure(root, root, "Window", typeof(Image));
            Place(window, Center, Center, new Vector2(0f, -45f), new Vector2(760f, 620f));
            Img(window, windowSprite, Color.white);

            // Title with a hard pixel drop shadow; the container is what gets animated
            GameObject title = Ensure(root, root, "Title", null);
            Place(title, Center, Center, new Vector2(0f, 365f), new Vector2(1200f, 160f));
            ShadowedText(root, title, "GAME OVER", 132f, BloodRed, BloodShadow, 7f);
            title.transform.SetAsLastSibling();

            Transform win = window.transform;

            GameObject subtitle = Ensure(root, win, "Subtitle", typeof(TextMeshProUGUI));
            Place(subtitle, TopCenter, TopCenter, new Vector2(0f, -30f), new Vector2(700f, 40f));
            Text(subtitle, "✦ THE DARKNESS HAS CLAIMED YOUR SOUL ✦", 30f, Gold, true);

            GameObject divider = Ensure(root, win, "Divider", typeof(Image));
            Place(divider, TopCenter, TopCenter, new Vector2(0f, -86f), new Vector2(640f, 4f));
            Img(divider, null, Line);

            GameObject survived = Ensure(root, win, "Survived Text", typeof(TextMeshProUGUI));
            Place(survived, TopCenter, TopCenter, new Vector2(0f, -104f), new Vector2(700f, 40f));
            Text(survived, "TIME SURVIVED", 38f, Gold, true);

            GameObject time = Ensure(root, win, "Time Text", typeof(TextMeshProUGUI));
            Place(time, TopCenter, TopCenter, new Vector2(0f, -144f), new Vector2(700f, 116f));
            TMP_Text timeText = Text(time, "00:00", 116f, Color.white);

            GameObject badge = Ensure(root, win, "New Best Badge", typeof(Image));
            Place(badge, TopCenter, Center, new Vector2(0f, -286f), new Vector2(230f, 44f));
            Img(badge, badgeSprite, Color.white);
            GameObject badgeLabel = Ensure(root, badge.transform, "Badge Text", typeof(TextMeshProUGUI));
            Stretch(badgeLabel);
            Text(badgeLabel, "NEW BEST!", 32f, Color.white, true);
            badge.SetActive(false);

            // Stats panel: LEVEL | COINS | BEST TIME
            GameObject stats = Ensure(root, win, "Stats Panel", typeof(Image));
            Place(stats, TopCenter, TopCenter, new Vector2(0f, -322f), new Vector2(680f, 150f));
            Img(stats, subPanelSprite, Color.white);
            Transform st = stats.transform;

            TMP_Text levelText = StatColumn(root, st, "Stat Level", -226f, "LEVEL");
            TMP_Text coinsText = StatColumn(root, st, "Stat Coins", 0f, "COINS");
            TMP_Text bestText = StatColumn(root, st, "Stat Best", 226f, "BEST TIME");

            // Coin icon sits left of the COINS label, so nudge the label right to keep the pair centred
            Transform coinsColumn = coinsText.transform.parent;
            GameObject coinIcon = Ensure(coinsColumn, coinsColumn, "Coin Icon", typeof(Image));
            Place(coinIcon, Center, Center, new Vector2(-50f, 34f), new Vector2(26f, 26f));
            Img(coinIcon, coinSprite, Color.white, false);
            Place(coinsColumn.Find("Label").gameObject, Center, Center, new Vector2(14f, 34f), new Vector2(200f, 34f));

            Separator(root, st, "Separator Left", -113f);
            Separator(root, st, "Separator Right", 113f);

            // Buttons (reused so their UIController onClick wiring survives)
            GameObject restart = Reuse(root, win, "Restart");
            if (restart != null)
            {
                Place(restart, BottomCenter, BottomCenter, new Vector2(-175f, 36f), new Vector2(320f, 76f));
                StyleButton(restart, confirmSprite, "TRY AGAIN", 36f);
            }
            GameObject menu = Reuse(root, win, "Main Menu");
            if (menu != null)
            {
                Place(menu, BottomCenter, BottomCenter, new Vector2(175f, 36f), new Vector2(320f, 76f));
                StyleButton(menu, backSprite, "MAIN MENU", 36f);
            }

            // Wire the runtime controller
            GameOverScreen screen = EnsureComponent<GameOverScreen>(panel);
            Undo.RecordObject(screen, "Wire Game Over Screen");
            screen.canvasGroup = group;
            screen.window = window.GetComponent<RectTransform>();
            screen.title = title.GetComponent<RectTransform>();
            screen.timeText = timeText;
            screen.levelText = levelText;
            screen.coinsText = coinsText;
            screen.bestText = bestText;
            screen.newBestBadge = badge;
            EditorUtility.SetDirty(screen);

            panel.SetActive(false);
        }

        // -------------------------------------------------------------
        // Pause
        // -------------------------------------------------------------
        static void BuildPause(GameObject panel)
        {
            Undo.RegisterFullObjectHierarchyUndo(panel, "Generate Pause UI");
            Transform root = panel.transform;

            Stretch(panel);
            Img(panel, null, new Color(0.02f, 0.03f, 0.07f, 0.78f)).raycastTarget = true; // Blocks clicks to the HUD underneath
            CanvasGroup group = EnsureComponent<CanvasGroup>(panel);

            GameObject window = Ensure(root, root, "Window", typeof(Image));
            Place(window, Center, Center, Vector2.zero, new Vector2(520f, 650f));
            Img(window, windowSprite, Color.white);
            Transform win = window.transform;

            GameObject title = Ensure(root, win, "Title", null);
            Place(title, TopCenter, TopCenter, new Vector2(0f, -22f), new Vector2(480f, 96f));
            ShadowedText(root, title, "— PAUSED —", 88f, Gold, GoldShadow, 0f); // No shadow: it left a stray glyph at this size

            GameObject divider = Ensure(root, win, "Divider", typeof(Image));
            Place(divider, TopCenter, TopCenter, new Vector2(0f, -124f), new Vector2(420f, 4f));
            Img(divider, null, Line);

            EnsureSaveAndExitButton(root);

            string[] names = { "Resume", "Save And Exit", "Restart", "Main Menu", "Quit Game" };
            string[] labels = { "RESUME", "SAVE & EXIT", "RESTART", "MAIN MENU", "QUIT" };
            Sprite[] sprites = { confirmSprite, subPanelSprite, subPanelSprite, subPanelSprite, backSprite };
            for (int i = 0; i < names.Length; i++)
            {
                GameObject button = Reuse(root, win, names[i]);
                if (button == null) continue;
                Place(button, TopCenter, Center, new Vector2(0f, -184f - i * 88f), new Vector2(380f, 72f));
                StyleButton(button, sprites[i], labels[i], 34f);
            }

            UIPopIn pop = EnsureComponent<UIPopIn>(panel);
            Undo.RecordObject(pop, "Wire Pause Pop In");
            pop.target = window.GetComponent<RectTransform>();
            pop.canvasGroup = group;
            EditorUtility.SetDirty(pop);

            panel.SetActive(false);
        }

        // Clones the Main Menu button and points it at UIController.SaveAndExit (saved into the scene as a persistent listener).
        static void EnsureSaveAndExitButton(Transform root)
        {
            if (FindDeep(root, "Save And Exit") != null) return;
            Transform source = FindDeep(root, "Main Menu");
            UIController ui = Object.FindAnyObjectByType<UIController>();
            if (source == null || ui == null) return;

            GameObject clone = Object.Instantiate(source.gameObject, source.parent);
            clone.name = "Save And Exit";
            Undo.RegisterCreatedObjectUndo(clone, "Create Save And Exit");
            Button button = clone.GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent(); // Drop the copied GoToMainMenu wiring
            UnityEventTools.AddPersistentListener(button.onClick, ui.SaveAndExit);
        }

        // -------------------------------------------------------------
        // Level Up (cards are styled at runtime by LevelUpSellectionButton; this sets fonts and the Skip button)
        // -------------------------------------------------------------
        static void BuildLevelUp(GameObject panel)
        {
            Undo.RegisterFullObjectHierarchyUndo(panel, "Generate Level Up UI");

            // Title keeps the pixel font; card names/descriptions use the smooth font so they stay readable when small.
            foreach (TMP_Text t in panel.GetComponentsInChildren<TMP_Text>(true))
            {
                TMP_FontAsset f = t.name == "Upgrade Text" || t.transform.parent.name == "Skip Level Up" ? font : bodyFont;
                if (f == null) continue;
                Undo.RecordObject(t, "Level Up Font");
                t.font = f;
                t.fontSharedMaterial = f.material;
            }

            Transform skip = FindDeep(panel.transform, "Skip Level Up");
            if (skip != null)
            {
                // Position, sprite and label are set at runtime by UIController.ApplySciFiLevelUpTheme.
                EnsureComponent<UIButtonHover>(skip.gameObject);
            }
        }

        // -------------------------------------------------------------
        // Building blocks
        // -------------------------------------------------------------
        static TMP_Text StatColumn(Transform root, Transform parent, string name, float x, string label)
        {
            GameObject column = Ensure(root, parent, name, null);
            Place(column, Center, Center, new Vector2(x, 0f), new Vector2(220f, 130f));

            GameObject labelObj = Ensure(column.transform, column.transform, "Label", typeof(TextMeshProUGUI));
            Place(labelObj, Center, Center, new Vector2(0f, 34f), new Vector2(220f, 34f));
            Text(labelObj, label, 30f, Gold, true);

            GameObject valueObj = Ensure(column.transform, column.transform, "Value", typeof(TextMeshProUGUI));
            Place(valueObj, Center, Center, new Vector2(0f, -18f), new Vector2(220f, 64f));
            return Text(valueObj, "0", 58f, Color.white);
        }

        static void Separator(Transform root, Transform parent, string name, float x)
        {
            GameObject sep = Ensure(root, parent, name, typeof(Image));
            Place(sep, Center, Center, new Vector2(x, 0f), new Vector2(3f, 100f));
            Img(sep, null, new Color(Gold.r, Gold.g, Gold.b, 0.35f));
        }

        // A container holding a dark offset copy of the text behind the real one, for a hard pixel drop shadow.
        static void ShadowedText(Transform root, GameObject container, string text, float size, Color color, Color shadowColor, float offset)
        {
            Transform c = container.transform;

            GameObject shadow = Ensure(root, c, "Title Shadow", typeof(TextMeshProUGUI));
            shadow.SetActive(offset > 0f);
            Stretch(shadow);
            shadow.GetComponent<RectTransform>().anchoredPosition = new Vector2(offset, -offset);
            Text(shadow, text, size, shadowColor).characterSpacing = 6f;
            shadow.transform.SetAsFirstSibling();

            GameObject main = Ensure(root, c, "Title Text", typeof(TextMeshProUGUI));
            Stretch(main);
            Text(main, text, size, color).characterSpacing = 6f;
            main.transform.SetAsLastSibling();
        }

        static void StyleButton(GameObject go, Sprite sprite, string label, float fontSize)
        {
            Img(go, sprite, Color.white).raycastTarget = true;

            Button button = go.GetComponent<Button>();
            Undo.RecordObject(button, "Style Button");
            button.transition = Selectable.Transition.ColorTint;
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.9f, 0.9f, 0.9f);
            colors.highlightedColor = Color.white;
            colors.selectedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.8f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.06f;
            button.colors = colors;

            TMP_Text text = go.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                Stretch(text.gameObject);
                Text(text.gameObject, label, fontSize, Color.white).characterSpacing = 3f;
            }

            EnsureComponent<UIButtonHover>(go);
        }

        static TMP_Text Text(GameObject go, string value, float size, Color color, bool body = false)
        {
            TMP_Text t = go.GetComponent<TMP_Text>();
            Undo.RecordObject(t, "Style Text");
            TMP_FontAsset f = body && bodyFont != null ? bodyFont : font;
            if (f != null)
            {
                t.font = f;
                t.fontSharedMaterial = f.material;
            }
            t.text = value;
            t.enableAutoSizing = false;
            t.fontSize = size;
            t.fontStyle = FontStyles.Bold;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Overflow;
            t.characterSpacing = 0f;
            t.raycastTarget = false;
            return t;
        }

        static Image Img(GameObject go, Sprite sprite, Color color, bool sliced = true)
        {
            Image img = go.GetComponent<Image>();
            Undo.RecordObject(img, "Style Image");
            img.sprite = sprite;
            img.type = sprite != null && sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.preserveAspect = sprite != null && !sliced;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static void Place(GameObject go, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            Undo.RecordObject(rt, "Place UI");
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;
        }

        static void Stretch(GameObject go)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            Undo.RecordObject(rt, "Stretch UI");
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = Center;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        // Finds a child anywhere under root by name (so re-runs pick up moved objects) or creates it, then parents it.
        static GameObject Ensure(Transform root, Transform parent, string name, System.Type graphic)
        {
            Transform found = FindDeep(root, name);
            GameObject go;
            if (found != null)
            {
                go = found.gameObject;
            }
            else
            {
                go = graphic != null
                    ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), graphic)
                    : new GameObject(name, typeof(RectTransform));
                go.layer = parent.gameObject.layer;
                Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            }

            if (go.transform.parent != parent) Undo.SetTransformParent(go.transform, parent, "Parent " + name);
            go.SetActive(true);
            return go;
        }

        static GameObject Reuse(Transform root, Transform parent, string name)
        {
            Transform found = FindDeep(root, name);
            if (found == null)
            {
                Debug.LogWarning($"[GameOverUIGenerator] Button '{name}' not found under '{root.name}', skipped.");
                return null;
            }
            if (found.parent != parent) Undo.SetTransformParent(found, parent, "Parent " + name);
            found.gameObject.SetActive(true);
            return found.gameObject;
        }

        static Transform FindDeep(Transform root, string name)
        {
            foreach (Transform child in root)
            {
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(go);
        }

        static void LoadAssets()
        {
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Udemy Vampire Survival Assets/Fonts/Kenney Fonts/Kenney Pixel SDF - Outline.asset");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Udemy Vampire Survival Assets/Fonts/Kenney Fonts/Kenney Pixel SDF.asset");
            if (font == null) Debug.LogWarning("[GameOverUIGenerator] Kenney Pixel font not found, keeping existing fonts.");

            bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

            windowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_WindowFrame_Gold.png");
            subPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_SubPanel_Gold.png");
            confirmSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Button_Confirm.png");
            backSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Button_Back.png");
            badgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Badge_Unlocked.png");
            coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/UI/UI_Coin_Icon.png");
        }
    }
}
#endif
