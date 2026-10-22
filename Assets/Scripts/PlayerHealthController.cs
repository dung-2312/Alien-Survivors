using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    public static PlayerHealthController instance; // Create a singleton of the PlayerHealthController. AK

    private void Awake()
    {
        instance = this; // Set the instance to this script. AK
    }

    public float currentHealth, maxHealth; // Current and max health of the player. AK

    public Slider healthSlider; // The health slider. AK
    public GameObject deathEffect; // The death effect. GK

    [Header("I-Frames & Feedback")]
    [Tooltip("Khoảng thời gian bất tử (giây) sau khi dính đòn để tránh bị quái cắn liên tục")]
    public float invincibilityDuration = 0.4f;
    private float invincibilityCounter = 0f;
    private SpriteRenderer[] playerSprites;
    private Coroutine flashCoroutine;

    void Start()
    {
        maxHealth = PlayerStatController.instance.health[0].value; // Set the max health to the health value. GK
        currentHealth = maxHealth; // Set the current health to the max health at the start of the game. AK

        playerSprites = GetComponentsInChildren<SpriteRenderer>();

        SetupHealthBarStyle();
        UpdateHealthUI();
    }

    public void SetupHealthBarStyle()
    {
        if (healthSlider != null)
        {
            RectTransform rt = healthSlider.GetComponent<RectTransform>();
            if (rt != null)
            {
                // Position below character feet matching Vampire Survivors (Image 2)
                rt.anchoredPosition = new Vector2(0f, -48f);
                rt.sizeDelta = new Vector2(85f, 6.5f);
            }

            // Dark background for retro contrast
            Transform bgT = healthSlider.transform.Find("Background");
            if (bgT != null)
            {
                Image bgImg = bgT.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
                }
            }

            // Clean vibrant red fill
            if (healthSlider.fillRect != null)
            {
                Image fillImg = healthSlider.fillRect.GetComponent<Image>();
                if (fillImg != null)
                {
                    fillImg.color = new Color(0.88f, 0.15f, 0.15f, 1f);
                }
            }
        }
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    void Update()
    {
        if (invincibilityCounter > 0f)
        {
            invincibilityCounter -= Time.deltaTime;
        }
    }

    public void TakeDamage(float damageToTake) // Function to take damage. AK
    {
        // 1. Kiểm tra khung bất tử (I-Frames)
        if (invincibilityCounter > 0f)
        {
            return; // Đang trong thời gian bất tử -> Bỏ qua sát thương
        }

        currentHealth -= damageToTake; // Decrease the current health by the damage. AK
        invincibilityCounter = invincibilityDuration;

        // 2. Kích hoạt hiệu ứng Rung màn hình (Screen Shake)
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.18f, 0.22f);
        }

        // 3. Hiệu ứng nhấp nháy nhân vật (I-Frames Flash)
        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashIFrameCo());
        }

        if(currentHealth <= 0) // If the current health is less than or equal to 0. AK
        {
            gameObject.SetActive(false); // Deactivate the player. AK
            LevelManager.instance.EndLevel(); // End the level. GK
            Instantiate(deathEffect, transform.position, transform.rotation); // Instantiate the death effect. GK
            
            SFXManager.instance.PlaySFX(3); // Play the sound effect. GK
        }

        UpdateHealthUI();
    }

    /// <summary>
    /// Hồi máu cho Player và cập nhật thanh hiển thị.
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    private IEnumerator FlashIFrameCo()
    {
        if (playerSprites == null || playerSprites.Length == 0)
        {
            playerSprites = GetComponentsInChildren<SpriteRenderer>();
        }

        float flickerInterval = 0.08f;
        while (invincibilityCounter > 0f)
        {
            SetSpriteAlpha(0.35f);
            yield return new WaitForSeconds(flickerInterval);
            SetSpriteAlpha(1.0f);
            yield return new WaitForSeconds(flickerInterval);
        }

        SetSpriteAlpha(1.0f);
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (playerSprites == null) return;
        for (int i = 0; i < playerSprites.Length; i++)
        {
            if (playerSprites[i] != null)
            {
                Color c = playerSprites[i].color;
                c.a = alpha;
                playerSprites[i].color = c;
            }
        }
    }

    private void OnDisable()
    {
        SetSpriteAlpha(1.0f);
    }
}
