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
    }

    public void ApplySciFiLevelUpTheme()
    {
        if (levelUpPanel != null)
        {
            Image panelImg = levelUpPanel.GetComponent<Image>();
            if (panelImg != null)
            {
                panelImg.color = new Color(0.031f, 0.051f, 0.102f, 0.94f); // #080D1A Dark Sci-Fi background
            }

            // Định dạng tiêu đề và nút Skip chuẩn Sci-Fi
            TMP_Text[] allTexts = levelUpPanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in allTexts)
            {
                if (t.name.Contains("Title") || t.text.Contains("Select An Upgrade") || t.text.Contains("SELECT AN UPGRADE"))
                {
                    t.text = "<color=#00FFA3>—</color>  <b>SELECT AN UPGRADE</b>  <color=#00FFA3>—</color>";
                    t.fontSize = 38;
                    t.alignment = TextAlignmentOptions.Center;
                    t.color = Color.white;
                }
                else if (t.text.Contains("Skip Level Up") || t.text.Contains("SKIP"))
                {
                    t.text = "<color=#8EA5C8><b>SKIP >></b></color>";
                }
            }
        }

        // Ẩn 4 nút mua shop ở đáy bảng level-up để tập trung trọn vẹn vào 3 thẻ nâng cấp
        if (moveSpeedUpgradeDisplay != null) moveSpeedUpgradeDisplay.gameObject.SetActive(false);
        if (healthUpgradeDisplay != null) healthUpgradeDisplay.gameObject.SetActive(false);
        if (pickupRangeUpgradeDisplay != null) pickupRangeUpgradeDisplay.gameObject.SetActive(false);
        if (maxWeaponsUpgradeDisplay != null) maxWeaponsUpgradeDisplay.gameObject.SetActive(false);

        // Định vị 3 thẻ ngang nhau cân đối, kích thước chuẩn 320x480px
        if (levelUpButtons != null && levelUpButtons.Length >= 3)
        {
            float[] xOffsets = new float[] { -380f, 0f, 380f };
            for (int i = 0; i < 3; i++)
            {
                if (levelUpButtons[i] != null)
                {
                    RectTransform rt = levelUpButtons[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.sizeDelta = new Vector2(320f, 480f);
                        rt.anchoredPosition = new Vector2(xOffsets[i], -15f);
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // If the escape key is pressed. GK
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