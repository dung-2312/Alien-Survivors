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

    private Image healthFillImage;
    private RectTransform ghostBarRt;
    private float ghostHealth;
    private static Sprite gothicCapsuleFrame;

    void Start()
    {
        maxHealth = PlayerStatController.instance.health[0].value; // Set the max health to the health value. GK
        currentHealth = maxHealth; // Set the current health to the max health at the start of the game. AK
        ghostHealth = currentHealth;

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
                // Promax Gothic Health Capsule below character feet
                rt.anchoredPosition = new Vector2(0f, -48f);
                rt.sizeDelta = new Vector2(104f, 11f);
            }

            // Dark gothic obsidian background with gold filigree border
            Transform bgT = healthSlider.transform.Find("Background");
            if (bgT != null)
            {
                Image bgImg = bgT.GetComponent<Image>();
                if (bgImg != null)
                {
                    bgImg.sprite = GetGothicCapsuleFrame();
                    bgImg.type = Image.Type.Sliced;
                    bgImg.color = Color.white;
                }
            }

            // Ghost damage bar (behind active fill)
            if (healthSlider.fillRect != null)
            {
                Transform parent = healthSlider.fillRect.parent;
                Transform existingGhost = parent.Find("GhostBar");
                GameObject ghostGO = existingGhost != null ? existingGhost.gameObject : new GameObject("GhostBar", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                if (existingGhost == null) ghostGO.transform.SetParent(parent, false);

                ghostBarRt = ghostGO.GetComponent<RectTransform>();
                ghostBarRt.anchorMin = new Vector2(0f, 0f);
                ghostBarRt.anchorMax = new Vector2(1f, 1f);
                ghostBarRt.pivot = new Vector2(0f, 0.5f);
                ghostBarRt.offsetMin = Vector2.zero;
                ghostBarRt.offsetMax = Vector2.zero;

                Image ghostImg = ghostGO.GetComponent<Image>();
                ghostImg.color = new Color(1f, 0.94f, 0.65f, 0.85f);
                ghostGO.transform.SetSiblingIndex(healthSlider.fillRect.GetSiblingIndex());

                // Foreground active fill
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
                if (healthFillImage != null)
                {
                    healthFillImage.color = new Color(0.20f, 0.90f, 0.40f, 1f);
                }
            }

            // Left gold jewel pip
            Transform pipT = healthSlider.transform.Find("LeftJewelPip");
            if (pipT == null)
            {
                GameObject pipGO = new GameObject("LeftJewelPip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                pipGO.transform.SetParent(healthSlider.transform, false);
                RectTransform pipRt = pipGO.GetComponent<RectTransform>();
                pipRt.anchorMin = new Vector2(0f, 0.5f);
                pipRt.anchorMax = new Vector2(0f, 0.5f);
                pipRt.pivot = new Vector2(0.5f, 0.5f);
                pipRt.anchoredPosition = new Vector2(-4f, 0f);
                pipRt.sizeDelta = new Vector2(7f, 7f);

                Image pipImg = pipGO.GetComponent<Image>();
                pipImg.color = new Color(1f, 0.88f, 0.35f, 1f);
            }
        }
    }

    public void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            // Dynamic color grading by remaining health percentage
            float pct = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 1f;
            if (healthFillImage != null)
            {
                if (pct > 0.5f)
                {
                    // Full vigor: Radiant Emerald Green
                    healthFillImage.color = new Color(0.20f, 0.90f, 0.40f, 1f);
                }
                else if (pct > 0.25f)
                {
                    // Wounded: Warm Amber Gold
                    healthFillImage.color = new Color(1f, 0.75f, 0.15f, 1f);
                }
                else
                {
                    // Critical: Blood Crimson
                    healthFillImage.color = new Color(0.98f, 0.16f, 0.22f, 1f);
                }
            }
        }
    }

    void Update()
    {
        if (invincibilityCounter > 0f)
        {
            invincibilityCounter -= Time.deltaTime;
        }

        UpdateHealthGhostAndPulse();
    }

    private void UpdateHealthGhostAndPulse()
    {
        // Smooth catch-up on ghost damage bar
        if (ghostBarRt != null && maxHealth > 0f)
        {
            ghostHealth = Mathf.MoveTowards(ghostHealth, currentHealth, Time.deltaTime * (maxHealth * 0.4f));
            if (ghostHealth < currentHealth) ghostHealth = currentHealth;
            float ghostPct = Mathf.Clamp01(ghostHealth / maxHealth);
            ghostBarRt.anchorMax = new Vector2(ghostPct, 1f);
        }

        // Low HP heartbeat warning pulse
        if (healthSlider != null)
        {
            float hpPct = maxHealth > 0f ? (currentHealth / maxHealth) : 1f;
            if (hpPct <= 0.25f && currentHealth > 0f)
            {
                float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 8.5f);
                healthSlider.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else
            {
                healthSlider.transform.localScale = Vector3.one;
            }
        }
    }

    private static Sprite GetGothicCapsuleFrame()
    {
        if (gothicCapsuleFrame != null) return gothicCapsuleFrame;
        const int w = 32, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];

        Color darkIron = new Color(0.08f, 0.08f, 0.11f, 1f);
        Color goldHigh = new Color(1f, 0.88f, 0.42f, 1f);
        Color goldShadow = new Color(0.55f, 0.40f, 0.12f, 1f);
        Color wellDark = new Color(0.06f, 0.05f, 0.09f, 0.98f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Outer dark iron border
                if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                {
                    px[y * w + x] = darkIron;
                }
                // Gold beveled hairline (1px)
                else if (x == 1 || x == w - 2 || y == 1 || y == h - 2)
                {
                    px[y * w + x] = (y == h - 2 || x == 1) ? goldHigh : goldShadow;
                }
                // Inner dark velvet well
                else
                {
                    px[y * w + x] = wellDark;
                }
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        gothicCapsuleFrame = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(3, 3, 3, 3));
        return gothicCapsuleFrame;
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
