using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance; // Create a singleton instance of the UIController. GK

    public void Awake()
    {
        instance = this; // Set the instance to this object. GK
    }

    public Slider explvlSlider; // The experience level slider. GK
    public TMP_Text explvlText; // The experience level text. GK

    public LevelUpSellectionButton[] levelUpButtons; // The level up buttons. GK

    public GameObject levelUpPanel; // The level up panel. GK
    public TMP_Text coinText; // The coin text. D
    public GameObject levelEndScreen; // The level end script. GK
    public TMP_Text endTimeText; // The end time text. GK

    public PlayerStatUpgradeDisplay moveSpeedUpgradeDisplay,
        healthUpgradeDisplay,
        pickupRangeUpgradeDisplay,
        maxWeaponsUpgradeDisplay; // The player stat upgrade display. GK

    public TMP_Text timeText; // The time text. GK
    public string mainMenuName; // The main menu scene. GK

    public GameObject pauseScreen; // The pause screen. GK

    // Start is called before the first frame update
    void Start()
    {
        ApplySciFiLevelUpTheme();

        if (RunSave.continueRequested) // Came from the Main Menu's Continue button
        {
            RunSave.continueRequested = false;
            RunSaveData saved = RunSave.Load();
            if (saved != null) StartCoroutine(RunSave.Restore(saved));
        }
    }

    // Vampire-Survivors style level-up: a framed window with the title on top, three wide rows inside it
    // and a red Skip button beside it. Built at runtime from the existing panel objects.
    public void ApplySciFiLevelUpTheme()
    {
        if (levelUpPanel != null)
        {
            RectTransform panelRt = levelUpPanel.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; // Backdrop covers the whole screen.
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            Image panelImg = levelUpPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.sprite = null;
                panelImg.color = new Color(0.02f, 0.03f, 0.07f, 0.7f); // Dim the game behind the window
            }

            // Gold-framed window behind everything
            Transform windowT = levelUpPanel.transform.Find("Level Up Window");
            if (windowT == null)
            {
                GameObject w = new GameObject("Level Up Window", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                w.transform.SetParent(levelUpPanel.transform, false);
                windowT = w.transform;
            }
            windowT.SetAsFirstSibling();
            RectTransform windowRt = (RectTransform)windowT;
            windowRt.anchorMin = windowRt.anchorMax = windowRt.pivot = new Vector2(0.5f, 0.5f);
            windowRt.anchoredPosition = Vector2.zero;
            windowRt.sizeDelta = new Vector2(900f, 780f);
            Image windowImg = windowT.GetComponent<Image>();
            windowImg.sprite = Resources.Load<Sprite>("UI/UI_WindowFrame_Gold");
            windowImg.type = Image.Type.Sliced;
            windowImg.color = Color.white;

            TMP_Text[] allTexts = levelUpPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in allTexts)
            {
                if (t.name == "Upgrade Text")
                {
                    t.text = "Level Up!";
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.fontSize = 72;
                    t.characterSpacing = 4f;
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.alignment = TextAlignmentOptions.Center;
                    t.color = new Color(1f, 0.88f, 0.25f);
                    t.rectTransform.anchorMin = t.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    t.rectTransform.sizeDelta = new Vector2(800f, 100f);
                    t.rectTransform.anchoredPosition = new Vector2(0f, 320f);
                }
                else if (t.transform.parent != null && t.transform.parent.name == "Skip Level Up")
                {
                    t.text = "SKIP";
                    t.fontStyle = FontStyles.Bold;
                    t.enableAutoSizing = false;
                    t.fontSize = 48;
                    t.color = Color.white;
                }
            }

            // Red Skip button on the right of the window
            Transform skip = levelUpPanel.transform.Find("Skip Level Up");
            if (skip != null)
            {
                RectTransform skipRt = (RectTransform)skip;
                skipRt.anchorMin = skipRt.anchorMax = skipRt.pivot = new Vector2(0.5f, 0.5f);
                skipRt.sizeDelta = new Vector2(280f, 92f);
                skipRt.anchoredPosition = new Vector2(640f, 140f);
                Image skipImg = skip.GetComponent<Image>();
                Sprite redButton = Resources.Load<Sprite>("UI/UI_Button_Back");
                if (skipImg != null && redButton != null)
                {
                    skipImg.sprite = redButton;
                    skipImg.type = Image.Type.Sliced;
                    skipImg.color = Color.white;
                }
            }
        }

        // The old stat shop buttons are not part of this screen
        if (moveSpeedUpgradeDisplay != null) moveSpeedUpgradeDisplay.gameObject.SetActive(false);
        if (healthUpgradeDisplay != null) healthUpgradeDisplay.gameObject.SetActive(false);
        if (pickupRangeUpgradeDisplay != null) pickupRangeUpgradeDisplay.gameObject.SetActive(false);
        if (maxWeaponsUpgradeDisplay != null) maxWeaponsUpgradeDisplay.gameObject.SetActive(false);

        // Three wide rows stacked inside the window
        if (levelUpButtons != null)
        {
            for (int i = 0; i < levelUpButtons.Length && i < 3; i++)
            {
                if (levelUpButtons[i] != null)
                    levelUpButtons[i].SetLayout(new Vector2(820f, 178f), new Vector2(0f, 135f - i * 192f));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !levelEndScreen.activeSelf) // If the escape key is pressed and the player isn't dead. GK
        {
            PauseUnpause(); // Pause and unpause the game. GK
        }
    }

    public void UpdateExperience(int currentExp, int levelExp, int currentLvl) // Function to update the experience. GK
    {
        explvlSlider.maxValue = levelExp; // Set the max value of the slider to the level experience. GK
        explvlSlider.value = currentExp; // Set the value of the slider to the current experience. GK
        explvlText.text = "Level: " + currentLvl; // Set the text to the current level and experience. GK
    }

    public void SkipLevelUp() // Function to skip the level. GK
    {
        ExperienceLevelController.instance.FinishLevelUp();
    }

    public void UpdateCoins() // Function to update the coins. D
    {
        coinText.text = "Coins: " + CoinController.instance.currentCoins; // Set the coin text to the current coins. D
    }

    public void PurchaseMoveSpeed() // Function to purchase the move speed. GK
    {
        PlayerStatController.instance.PurchaseMoveSpeed();
        SkipLevelUp();
    }

    public void PurchaseHealth() // Function to purchase the health. GK
    {
        PlayerStatController.instance.PurchaseHealth();
        SkipLevelUp();
    }

    public void PurchasePickupRange() // Function to purchase the pickup range. GK
    {
        PlayerStatController.instance.PurchasePickupRange();
        SkipLevelUp();
    }

    public void PurchaseMaxWeapons() // Function to purchase the max weapons. GK
    {
        PlayerStatController.instance.PurchaseMaxWeapons();
        SkipLevelUp();
    }

    public void UpdateTimer(float time) // Function to update the timer. GK
    {
        float minutes = Mathf.FloorToInt(time / 60); // Set the minutes to the time divided by 60. GK
        float seconds = Mathf.FloorToInt(time % 60); // Set the seconds to the time modulo 60. GK
        timeText.text =
            "Time: " + minutes + ":" + seconds.ToString("00"); // Set the time text to the minutes and seconds. GK
    }

    public void GoToMainMenu() // Function to go to the main menu. GK
    {
        SceneManager.LoadScene(mainMenuName); // Load the main menu. GK
        Time.timeScale = 1f; // Set the time scale to 1. GK 
    }

    public void SaveAndExit() // Save the run exactly as it is right now, then go back to the main menu.
    {
        RunSave.Save();
        GoToMainMenu();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f; // Set the time scale to 1. GK 
    }

    public void QuitGame() // Function to quit the game. GK
    {
        Application.Quit(); // Quit the application. GK
    }

    public void PauseUnpause() // Function to pause and unpause the game. GK
    {
        if (pauseScreen.activeSelf == false) // If the pause screen is active. GK
        {
            pauseScreen.SetActive(true); // Set the pause screen to the opposite of itself. GK
            Time.timeScale = 0f; // Set the time scale to 0. GK
        }
        else
        {
            pauseScreen.SetActive(false); // Set the pause screen to the opposite of itself. GK
            if (levelUpPanel.activeSelf == false) // If the level up panel is not active. GK
            {
                Time.timeScale = 1f; // Set the time scale to 1. GK 

            }
        }
    }
}