using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public string firstLevelName; // The first level name. GK

    void Start()
    {
        ShowMainMenuFirst();
        SetupTitle();
        SetupMenuButtons();
    }

    // The scene is saved with the Character Selection panel switched on, so every load (including coming back from
    // Save & Exit) opened on it. Start on the menu itself; Start Game / NEW GAME opens character selection.
    void ShowMainMenuFirst()
    {
        Transform panel = transform.Find("CharacterSelectionPanel");
        if (panel != null) panel.gameObject.SetActive(false);

        foreach (string buttonName in new[] { "Start Game", "Quit Game" })
        {
            Transform t = transform.Find(buttonName);
            if (t != null) t.gameObject.SetActive(true); // Character select hid them when it enabled
        }
    }

    // The old logo image has "VAMPIRE SURVIVORS" baked into it, so it is replaced by a text title in the game's name.
    void SetupTitle()
    {
        Transform logo = transform.Find("VS_Image");
        if (logo != null) logo.gameObject.SetActive(false);

        Transform title = transform.Find("Title"); // Existing (hidden) title text that already uses the pixel title font
        if (title == null) return;

        Transform shadow = transform.Find("Title Shadow");
        if (shadow == null)
        {
            shadow = Instantiate(title.gameObject, title.parent).transform;
            shadow.name = "Title Shadow";
        }
        ConfigureTitle(shadow, "<color=#14081F>ALIEN\nSURVIVORS</color>", new Vector2(6f, -6f));
        ConfigureTitle(title, "<color=#7CFF5A>ALIEN</color>\n<color=#FFE040>SURVIVORS</color>", new Vector2(0f, 0f));
        shadow.SetSiblingIndex(title.GetSiblingIndex()); // Shadow draws behind the title
    }

    static void ConfigureTitle(Transform t, string text, Vector2 position)
    {
        t.gameObject.SetActive(true);
        RectTransform rt = (RectTransform)t;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(1300f, 400f);
        rt.anchoredPosition = new Vector2(0f, 220f);

        TMP_Text tmp = t.GetComponent<TMP_Text>();
        tmp.enableAutoSizing = false;
        tmp.fontSize = 150f;
        tmp.lineSpacing = -20f;
        tmp.characterSpacing = 6f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        tmp.color = Color.white;
        tmp.text = text;

        // Centre on the glyphs that are actually drawn (the font's side spacing otherwise pulls the text left),
        // then apply the offset (used to push the shadow copy down and right).
        tmp.ForceMeshUpdate();
        TMP_TextInfo info = tmp.textInfo;
        float left = float.MaxValue, right = float.MinValue;
        for (int i = 0; i < info.characterCount; i++)
        {
            if (!info.characterInfo[i].isVisible) continue;
            left = Mathf.Min(left, info.characterInfo[i].bottomLeft.x);
            right = Mathf.Max(right, info.characterInfo[i].topRight.x);
        }
        float centerX = left <= right ? (left + right) * 0.5f : 0f;
        rt.anchoredPosition = new Vector2(-centerX + position.x, 220f + position.y);
    }

    public void StartGame() // Start the game. GK
    {
        SceneManager.LoadScene(firstLevelName); // Load the first level. GK
    }

    public void ContinueGame() // Load the run saved by "Save & Exit". GK
    {
        RunSaveData saved = RunSave.Load();
        if (saved == null) return;

        PlayerPrefs.SetInt("SelectedCharacterIndex", saved.characterIndex); // So the same hero is spawned
        PlayerPrefs.Save();
        RunSave.continueRequested = true;
        SceneManager.LoadScene(string.IsNullOrEmpty(firstLevelName) ? "Main" : firstLevelName);
    }

    public void QuitGame() // Quit the game. GK
    {
        Application.Quit(); // Quit the game. GK
        Debug.Log("I'm Quitting");
    }

    // Gold-framed retro buttons, with a CONTINUE button on top when there is a saved run.
    void SetupMenuButtons()
    {
        Transform start = transform.Find("Start Game");
        Transform quit = transform.Find("Quit Game");
        if (start == null || quit == null) return;

        RunSaveData saved = RunSave.Load();
        bool canContinue = saved != null;

        Transform cont = transform.Find("Continue Button");
        if (canContinue && cont == null)
        {
            cont = Instantiate(start.gameObject, start.parent).transform;
            cont.name = "Continue Button"; // Contains "Button", so Character Select hides it together with the other menu buttons.
            cont.SetSiblingIndex(start.GetSiblingIndex() + 1);
            Button b = cont.GetComponent<Button>();
            b.onClick = new Button.ButtonClickedEvent(); // Drops the copied Start Game wiring
            b.onClick.AddListener(ContinueGame);
        }
        if (cont != null) cont.gameObject.SetActive(canContinue);

        Sprite green = Resources.Load<Sprite>("UI/UI_Button_Confirm");
        Sprite navy = Resources.Load<Sprite>("UI/UI_SubPanel_Gold");
        Sprite red = Resources.Load<Sprite>("UI/UI_Button_Back");

        float[] y = canContinue ? new[] { -110f, -235f, -360f } : new[] { 0f, -150f, -275f };
        if (canContinue) StyleButton(cont, green, "CONTINUE\n<size=55%>" + RunSave.Describe(saved) + "</size>", y[0]);
        StyleButton(start, navy, canContinue ? "NEW GAME" : "START GAME", y[1]);
        StyleButton(quit, red, "QUIT", y[2]);
    }

    static void StyleButton(Transform t, Sprite sprite, string label, float y)
    {
        RectTransform rt = (RectTransform)t;
        rt.sizeDelta = new Vector2(480f, 110f);
        rt.anchoredPosition = new Vector2(0f, y);

        Image img = t.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_Text text = t.GetComponentInChildren<TMP_Text>(true);
        if (text == null) return;
        TMP_FontAsset smooth = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (smooth != null)
        {
            text.font = smooth;
            text.fontSharedMaterial = smooth.material;
        }
        text.enableAutoSizing = false;
        text.fontSize = 52f;
        text.fontStyle = FontStyles.Bold;
        text.characterSpacing = 3f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.text = label;
    }
}
