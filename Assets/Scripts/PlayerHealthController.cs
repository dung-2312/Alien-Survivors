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
    [Tooltip("Khoáº£ng thá»i gian báº¥t tá»­ (giÃ¢y) sau khi dÃ­nh Ä‘Ã²n Ä‘á»ƒ trÃ¡nh bá»‹ quÃ¡i cáº¯n liÃªn tá»¥c")]
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

            // Dark background with ornate slot border for retro contrast
            Transform bgT = healthSlider.transform.Find("Background");
            if (bgT != null)
            {
                Image bgImg = bgT.GetComponent<Image>();
                if (bgImg != null)
                {
                    Sprite slotSprite = Resources.Load<Sprite>("UI/VS_HUD_Slot");
                    if (slotSprite != null)
                    {
                        bgImg.sprite = slotSprite;
                        bgImg.type = Image.Type.Sliced;
                    }
                    bgImg.color = new Color(0.12f, 0.10f, 0.14f, 0.95f);
                }
            }

            // Clean vibrant ruby red fill
            if (healthSlider.fillRect != null)
            {
                Image fillImg = healthSlider.fillRect.GetComponent<Image>();
                if (fillImg != null)
                {
                    fillImg.color = new Color(0.96f, 0.18f, 0.22f, 1f);
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
        // 1. Kiá»ƒm tra khung báº¥t tá»­ (I-Frames)
        if (invincibilityCounter > 0f)
        {
            return; // Äang trong thá»i gian báº¥t tá»­ -> Bá» qua sÃ¡t thÆ°Æ¡ng
        }

                // Tác động qua nội tại nhân vật (Giảm sát thương Hero Knight, kích hoạt phản đòn...)
        if (PlayerController.instance != null)
        {
            PlayerController.instance.OnTakeDamage(ref damageToTake);
        }

        currentHealth -= damageToTake; // Decrease the current health by the damage. AK
        invincibilityCounter = invincibilityDuration;

        // 2. KÃ­ch hoáº¡t hiá»‡u á»©ng Rung mÃ n hÃ¬nh (Screen Shake)
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.18f, 0.22f);
        }

        // 3. Hiá»‡u á»©ng nháº¥p nhÃ¡y nhÃ¢n váº­t (I-Frames Flash)
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
    /// KÃ­ch hoáº¡t khung báº¥t tá»­ cÆ°á»¡ng bá»©c (vÃ­ dá»¥ khi lÆ°á»›t nÃ© Ä‘Ã²n Dash).
    /// </summary>
    public void TriggerInvincibility(float duration)
    {
        invincibilityCounter = Mathf.Max(invincibilityCounter, duration);
        if (gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashIFrameCo());
        }
    }

    /// <summary>
    /// Há»“i mÃ¡u cho Player vÃ  cáº­p nháº­t thanh hiá»ƒn thá»‹.
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
