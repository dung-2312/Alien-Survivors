using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls floating combat damage text, with arcade bounce-arc physics,
/// vibrant color-grading for damage tiers, and spectacular critical hit popups.
/// </summary>
public class DamageNumber : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text damageText;

    [Header("Animation Settings")]
    public float lifeTime = 0.85f;
    public float floatSpeed = 1.1f;
    public float punchScaleMultiplier = 1.4f;
    public float punchDuration = 0.16f;
    [Range(0.1f, 0.9f)]
    public float fadeOutThreshold = 0.45f;

    [Header("Aggregation Settings")]
    public float aggregationWindow = 0.2f;

    private float lifeCounter;
    private float totalDamage;
    private Vector3 baseScale;
    private float punchTimer;
    private Vector3 targetScale;
    private GameObject currentTarget;
    private bool isAggregating = true;
    private float aggregationTimer;
    private bool isInitialized = false;
    private Vector2 velocity;
    public bool isCritical = false;

    public bool CanAggregate => isAggregating && gameObject.activeSelf;
    public GameObject Target => currentTarget;

    private void Awake()
    {
        InitIfNeeded();
    }

    private void InitIfNeeded()
    {
        if (isInitialized) return;

        if (damageText == null)
        {
            damageText = GetComponentInChildren<TMP_Text>();
        }

        if (damageText != null)
        {
            TMP_FontAsset sFont = MainMenu.GetSmoothFont() ?? (UIController.instance != null ? UIController.instance.retroFont : null);
            if (sFont != null)
            {
                damageText.font = sFont;
                if (sFont.material != null) damageText.fontSharedMaterial = sFont.material;
            }

            damageText.fontStyle = FontStyles.Bold;
            damageText.alignment = TextAlignmentOptions.Center;
            damageText.richText = true;
            damageText.raycastTarget = false;
        }

        baseScale = transform.localScale;
        if (baseScale == Vector3.zero)
        {
            baseScale = Vector3.one * 1.15f;
        }

        isInitialized = true;
    }

    void Update()
    {
        // 1. Arcade bounce-arc trajectory: pops up with horizontal spread then drifts up
        transform.position += (Vector3)velocity * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, 0f, Time.deltaTime * 2.2f);
        velocity.y = Mathf.MoveTowards(velocity.y, floatSpeed, Time.deltaTime * 4.5f);

        // 2. Elastic punch scale decay
        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(punchTimer / punchDuration);
            // Elastic ease out
            float ease = Mathf.Sin(progress * Mathf.PI * 0.5f);
            transform.localScale = Vector3.Lerp(targetScale, baseScale, ease);
        }
        else
        {
            transform.localScale = baseScale;
        }

        // 3. Aggregation window countdown
        if (isAggregating)
        {
            aggregationTimer -= Time.deltaTime;
            if (aggregationTimer <= 0f)
            {
                isAggregating = false;
            }
        }

        // 4. Smooth Alpha Fade-out across all rich-text tags
        if (lifeCounter > 0f)
        {
            lifeCounter -= Time.deltaTime;

            float fadeDuration = lifeTime * fadeOutThreshold;
            if (lifeCounter <= fadeDuration && damageText != null && fadeDuration > 0f)
            {
                float alpha = Mathf.Clamp01(lifeCounter / fadeDuration);
                damageText.alpha = alpha;
            }

            if (lifeCounter <= 0f)
            {
                ReturnToPool();
            }
        }
    }

    public void Setup(float damage, Vector3 position, bool isCrit, GameObject target = null)
    {
        InitIfNeeded();

        currentTarget = target;
        totalDamage = damage;
        isCritical = isCrit;
        lifeCounter = isCrit ? 1.15f : 0.85f;
        isAggregating = !isCrit;
        aggregationTimer = aggregationWindow;

        // Spread spawn point slightly
        transform.position = position;
        transform.localScale = baseScale;

        // Dynamic arc velocity
        float spreadX = Random.Range(-0.85f, 0.85f);
        float popY = isCrit ? Random.Range(2.8f, 3.4f) : Random.Range(1.9f, 2.4f);
        velocity = new Vector2(spreadX, popY);

        if (damageText != null)
        {
            damageText.alpha = 1f;

            if (isCrit)
            {
                int val = Mathf.RoundToInt(totalDamage);
                damageText.text = $"<color=#FF2244><b>CRIT!</b></color> <size=125%><color=#FFE033><b>{val}</b></color></size>";
                PunchScale(punchScaleMultiplier * 1.85f);
            }
            else
            {
                int val = Mathf.RoundToInt(totalDamage);
                if (val >= 100)
                {
                    damageText.text = $"<color=#FF4422><b>{val}</b></color>";
                }
                else if (val >= 40)
                {
                    damageText.text = $"<color=#FFAA22><b>{val}</b></color>";
                }
                else
                {
                    damageText.text = $"<color=#E0EEFF><b>{val}</b></color>";
                }

                PunchScale(punchScaleMultiplier);
            }
        }
    }

    public void Setup(float damage, Vector3 position, GameObject target = null)
    {
        Setup(damage, position, false, target);
    }

    public void Setup(int damageDisplay)
    {
        Setup(damageDisplay, transform.position, false, null);
    }

    public void AddDamage(float additionalDamage)
    {
        totalDamage += additionalDamage;

        float minRemainingLife = (lifeTime > 0f ? lifeTime : 0.85f) * 0.65f;
        if (lifeCounter < minRemainingLife)
        {
            lifeCounter = minRemainingLife;
        }

        aggregationTimer = aggregationWindow;

        if (damageText != null)
        {
            damageText.alpha = 1f;
            int val = Mathf.RoundToInt(totalDamage);
            if (isCritical)
            {
                damageText.text = $"<color=#FF2244><b>CRIT!</b></color> <size=125%><color=#FFE033><b>{val}</b></color></size>";
            }
            else if (val >= 100)
            {
                damageText.text = $"<color=#FF4422><b>{val}</b></color>";
            }
            else if (val >= 40)
            {
                damageText.text = $"<color=#FFAA22><b>{val}</b></color>";
            }
            else
            {
                damageText.text = $"<color=#E0EEFF><b>{val}</b></color>";
            }
        }

        // Bouncy punch boost when combo damage piles up
        float comboBoost = Mathf.Min(0.45f, (totalDamage / 100f) * 0.2f);
        PunchScale(punchScaleMultiplier + comboBoost);
    }

    private void PunchScale(float multiplier)
    {
        targetScale = baseScale * multiplier;
        transform.localScale = targetScale;
        punchTimer = punchDuration;
    }

    private void ReturnToPool()
    {
        currentTarget = null;
        if (DamageNumberController.instance != null)
        {
            DamageNumberController.instance.PlaceInPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}