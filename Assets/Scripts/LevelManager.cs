using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance; // Singleton. GK
    public void Awake() // GK
    {
        instance = this; // GK
    }
    private bool gameActive; // The game is active. GK

    public float timer;
    public float waitToShowEndScreen = 1f;
    public bool IsGameActive => gameActive;

    // Start is called before the first frame update
    void Start()
    {
        gameActive = true; // The game is active. GK
    }

    // Update is called once per frame
    void Update()
    {
        if (gameActive) // If the game is active. GK
        {
            timer += Time.deltaTime; // Add the time. GK
            UIController.instance.UpdateTimer(timer); // Update the timer. GK
        }
    }

    public void EndLevel() // End the level. GK
    {
        if (!gameActive) return;
        gameActive = false; // The game is not active. GK
        RunSave.Clear(); // A finished run can't be continued.
        StartCoroutine(EndLevelCo()); // Start the coroutine. GK
    }

    /// <summary>
    /// Kích hoạt chuỗi sự kiện Chiến Thắng (Victory Flow) khi hoàn thành mục tiêu hoặc diệt Boss cuối.
    /// </summary>
    public void TriggerVictory(float waitTime = 3.5f)
    {
        if (!gameActive) return;
        StartCoroutine(VictoryCo(waitTime));
    }

    private IEnumerator VictoryCo(float waitTime)
    {
        // 1. Rung lắc màn hình & thông báo chiến thắng rực rỡ
        if (CameraController.instance != null) CameraController.instance.Shake(0.5f, 0.7f);
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("✦ VICTORY! FINAL BOSS PYROCLAST VANQUISHED! ✦", 4.0f);
            ProceduralSFX.PlayVictory();
            StartCoroutine(UIController.instance.BossSlowMoEffect(0.35f, 2.5f));
        }

        // 2. Tự động thu hút toàn bộ ngọc kinh nghiệm và trao thưởng vàng thắng trận
        if (ExpManager.instance != null) ExpManager.instance.AttractAllGems();
        if (CoinController.instance != null)
        {
            CoinController.instance.AddCoins(500); // Thưởng 500 vàng chiến thắng
        }

        // 3. Đợi một chút để người chơi tận hưởng khoảnh khắc chiến thắng và nhặt đồ
        yield return new WaitForSecondsRealtime(waitTime);

        gameActive = false;
        RunSave.Clear();

        float bestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f);
        bool isNewBest = timer > bestTime;
        if (isNewBest)
        {
            bestTime = timer;
            PlayerPrefs.SetFloat("BestSurvivalTime", bestTime);
        }
        PlayerPrefs.Save();

        GameOverScreen gameOverScreen = UIController.instance != null && UIController.instance.levelEndScreen != null
            ? UIController.instance.levelEndScreen.GetComponent<GameOverScreen>()
            : null;

        if (gameOverScreen != null)
        {
            gameOverScreen.Show(timer, bestTime, isNewBest,
                ExperienceLevelController.instance != null ? ExperienceLevelController.instance.currentLevel : 1,
                CoinController.instance != null ? CoinController.instance.coinsCollectedThisRun : 0);
        }
        else if (UIController.instance != null && UIController.instance.levelEndScreen != null)
        {
            UIController.instance.levelEndScreen.SetActive(true);
        }
    }

    public void ResumeAfterEndless()
    {
        gameActive = true;
    }
    
    IEnumerator EndLevelCo() // Coroutine to end the level. GK
    {
        yield return new WaitForSeconds(waitToShowEndScreen); // Wait for 1 second. GK

        float bestTime = PlayerPrefs.GetFloat("BestSurvivalTime", 0f);
        bool isNewBest = timer > bestTime;
        if (isNewBest)
        {
            bestTime = timer;
            PlayerPrefs.SetFloat("BestSurvivalTime", bestTime);
        }
        PlayerPrefs.Save();

        GameOverScreen gameOverScreen = UIController.instance.levelEndScreen.GetComponent<GameOverScreen>();
        if (gameOverScreen != null) // Retro Game Over screen built by the UI generator. GK
        {
            gameOverScreen.Show(timer, bestTime, isNewBest,
                ExperienceLevelController.instance.currentLevel, CoinController.instance.coinsCollectedThisRun);
            yield break;
        }

        float minutes = Mathf.FloorToInt(timer / 60); // The minutes. GK
        float seconds = Mathf.FloorToInt(timer % 60); // The seconds. GK
        float bestMinutes = Mathf.FloorToInt(bestTime / 60);
        float bestSeconds = Mathf.FloorToInt(bestTime % 60);

        UIController.instance.endTimeText.text = minutes.ToString() + " mins " + seconds.ToString("00") + " secs" +
            "\n<size=70%>Best: " + bestMinutes.ToString() + " mins " + bestSeconds.ToString("00") + " secs</size>"; // Set the end time text. GK
        UIController.instance.levelEndScreen.SetActive(true); // Set the end panel to active. GK
    }
}
