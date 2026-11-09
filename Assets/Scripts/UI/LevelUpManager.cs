using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AlienSurvivors.Upgrades;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager instance;

    [Header("UI Panels & Canvas")]
    public GameObject levelUpPanel;
    public CanvasGroup backgroundCanvasGroup; // Panel mờ tối màu (#080D1A)

    [Header("Cards Reference")]
    public Transform cardsContainer; // Parent chứa Horizontal Layout Group
    public UpgradeCardUI[] upgradeCards = new UpgradeCardUI[3]; // 3 thẻ nâng cấp

    [Header("Card Spawn Animation (Unscaled Time)")]
    public float cardSlideDistance = 80f;     // Khoảng cách trượt từ dưới lên (pixel)
    public float cardAnimDuration = 0.28f;    // Thời gian trượt của mỗi thẻ
    public float cardStaggerDelay = 0.08f;    // Độ trễ xuất hiện giữa các thẻ (thẻ 1 -> thẻ 2 -> thẻ 3)

    private Vector2[] cardOriginalPositions;
    private Coroutine introCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tự động tìm thẻ trong panel nếu chưa gán thủ công trong Inspector
        if (upgradeCards == null || upgradeCards.Length == 0 || upgradeCards[0] == null)
        {
            if (cardsContainer != null)
            {
                upgradeCards = cardsContainer.GetComponentsInChildren<UpgradeCardUI>(true);
            }
            else if (levelUpPanel != null)
            {
                upgradeCards = levelUpPanel.GetComponentsInChildren<UpgradeCardUI>(true);
            }
        }

        // Lưu lại vị trí chuẩn ban đầu của các thẻ
        if (upgradeCards != null && upgradeCards.Length > 0)
        {
            cardOriginalPositions = new Vector2[upgradeCards.Length];
            for (int i = 0; i < upgradeCards.Length; i++)
            {
                if (upgradeCards[i] != null && upgradeCards[i].rectTransform != null)
                {
                    cardOriginalPositions[i] = upgradeCards[i].rectTransform.anchoredPosition;
                }
            }
        }
    }

    /// <summary>
    /// Mở bảng Level Up, lọc ngẫu nhiên 3 thẻ hợp lệ và chạy animation xuất hiện mượt mà.
    /// </summary>
    public void ShowLevelUp()
    {
        // 1. Tạm dừng thời gian game
        Time.timeScale = 0f;

        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(true);
        }

        // 2. Lọc 3 thẻ nâng cấp hợp lệ (không trùng, chưa max cấp, đủ slot vũ khí)
        int cardCount = (upgradeCards != null && upgradeCards.Length > 0) ? upgradeCards.Length : 3;
        List<UpgradeData> selectedUpgrades = new List<UpgradeData>();

        if (UpgradeManager.instance != null)
        {
            selectedUpgrades = UpgradeManager.instance.GetRandomUpgrades(cardCount);
        }

        // 3. Nạp dữ liệu vào từng thẻ
        for (int i = 0; i < cardCount; i++)
        {
            if (i < upgradeCards.Length && upgradeCards[i] != null)
            {
                if (i < selectedUpgrades.Count)
                {
                    upgradeCards[i].gameObject.SetActive(true);
                    upgradeCards[i].SetCardData(selectedUpgrades[i]);
                }
                else
                {
                    upgradeCards[i].gameObject.SetActive(false);
                }
            }
        }

        // 4. Phát âm thanh mở bảng
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(2);
        }
        ProceduralSFX.PlayLevelUp();

        // 5. Chạy hiệu ứng trượt xuất hiện (Staggered Slide-up + Fade-in)
        if (introCoroutine != null) StopCoroutine(introCoroutine);
        introCoroutine = StartCoroutine(AnimateCardsIntro());
    }

    /// <summary>
    /// Coroutine animation trượt từ dưới lên sử dụng Time.unscaledDeltaTime (bất chấp Time.timeScale = 0).
    /// </summary>
    private IEnumerator AnimateCardsIntro()
    {
        // 1. Fade-in nền mờ tối (#080D1A)
        if (backgroundCanvasGroup != null)
        {
            backgroundCanvasGroup.alpha = 0f;
            float bgElapsed = 0f;
            while (bgElapsed < 0.15f)
            {
                bgElapsed += Time.unscaledDeltaTime;
                backgroundCanvasGroup.alpha = Mathf.Lerp(0f, 0.88f, bgElapsed / 0.15f);
                yield return null;
            }
            backgroundCanvasGroup.alpha = 0.88f;
        }

        // 2. Ẩn và hạ vị trí các thẻ xuống dưới
        for (int i = 0; i < upgradeCards.Length; i++)
        {
            UpgradeCardUI card = upgradeCards[i];
            if (card == null || !card.gameObject.activeSelf) continue;

            if (card.canvasGroup != null) card.canvasGroup.alpha = 0f;
            if (card.rectTransform != null && cardOriginalPositions != null && i < cardOriginalPositions.Length)
            {
                card.rectTransform.anchoredPosition = cardOriginalPositions[i] + Vector2.down * cardSlideDistance;
            }
        }

        // 3. Lần lượt trượt từng thẻ lên kèm hiệu ứng mờ dần (Staggered Animation)
        for (int i = 0; i < upgradeCards.Length; i++)
        {
            UpgradeCardUI card = upgradeCards[i];
            if (card == null || !card.gameObject.activeSelf) continue;

            Vector2 targetPos = (cardOriginalPositions != null && i < cardOriginalPositions.Length)
                ? cardOriginalPositions[i]
                : card.rectTransform.anchoredPosition;

            StartCoroutine(AnimateSingleCard(card, targetPos));

            // Đợi một khoảng ngắn giữa các thẻ để tạo nhịp điệu tự nhiên
            float staggerWait = 0f;
            while (staggerWait < cardStaggerDelay)
            {
                staggerWait += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    private IEnumerator AnimateSingleCard(UpgradeCardUI card, Vector2 targetPos)
    {
        Vector2 startPos = targetPos + Vector2.down * cardSlideDistance;
        float elapsed = 0f;

        while (elapsed < cardAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / cardAnimDuration);

            // Đường cong chuyển động mượt mà (Ease-Out Cubic)
            float ease = 1f - Mathf.Pow(1f - t, 3);

            if (card.rectTransform != null)
            {
                card.rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
            }

            if (card.canvasGroup != null)
            {
                card.canvasGroup.alpha = Mathf.Lerp(0f, 1f, ease);
            }

            yield return null;
        }

        if (card.rectTransform != null) card.rectTransform.anchoredPosition = targetPos;
        if (card.canvasGroup != null) card.canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Được gọi khi người chơi bấm chọn 1 thẻ nâng cấp.
    /// </summary>
    public void OnCardSelected()
    {
        if (ExperienceLevelController.instance != null)
        {
            ExperienceLevelController.instance.FinishLevelUp();
        }
        else
        {
            ClosePanel();
        }
    }

    /// <summary>
    /// Đóng bảng nâng cấp và tiếp tục chạy game.
    /// </summary>
    public void ClosePanel()
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}
