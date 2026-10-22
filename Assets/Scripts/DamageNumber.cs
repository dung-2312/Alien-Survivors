using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("Components")]
    public TMP_Text damageText;

    [Header("Animation Settings")]
    public float lifeTime = 0.8f;
    public float floatSpeed = 1.2f;
    public float punchScaleMultiplier = 1.35f;
    public float punchDuration = 0.15f;
    [Range(0.1f, 0.9f)]
    public float fadeOutThreshold = 0.4f; // Fades out during the last portion of lifetime

    [Header("Aggregation Settings")]
    public float aggregationWindow = 0.2f; // Time window during which new hits will stack

    private float lifeCounter;
    private float totalDamage;
    private Vector3 baseScale;
    private Color baseColor = Color.white;
    private float punchTimer;
    private Vector3 targetScale;
    private GameObject currentTarget;
    private bool isAggregating = true;
    private float aggregationTimer;
    private bool isInitialized = false;

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
            baseColor = damageText.color;
        }

        baseScale = transform.localScale;
        if (baseScale == Vector3.zero)
        {
            baseScale = Vector3.one;
        }

        isInitialized = true;
    }

    void Update()
    {
        // 1. Bay lên trên
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

        // 2. Hiệu ứng phóng to & đàn hồi (Punch scale decay - Zero-GC)
        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(punchTimer / punchDuration);
            transform.localScale = Vector3.Lerp(targetScale, baseScale, progress);
        }
        else
        {
            transform.localScale = baseScale;
        }

        // 3. Đếm ngược cửa sổ gộp sát thương (Damage Aggregation window)
        if (isAggregating)
        {
            aggregationTimer -= Time.deltaTime;
            if (aggregationTimer <= 0f)
            {
                isAggregating = false; // Đóng cửa sổ gộp để đòn đánh tiếp theo tạo số mới nếu giãn cách
            }
        }

        // 4. Đếm ngược thời gian sống & Mờ dần (Fade out - Zero-GC)
        if (lifeCounter > 0f)
        {
            lifeCounter -= Time.deltaTime;

            float fadeDuration = lifeTime * fadeOutThreshold;
            if (lifeCounter <= fadeDuration && damageText != null && fadeDuration > 0f)
            {
                float alpha = Mathf.Clamp01(lifeCounter / fadeDuration);
                Color c = baseColor;
                c.a = alpha;
                damageText.color = c;
            }

            if (lifeCounter <= 0f)
            {
                ReturnToPool();
            }
        }
    }

    public bool isCritical = false;

    public void Setup(float damage, Vector3 position, bool isCrit, GameObject target = null)
    {
        InitIfNeeded();

        currentTarget = target;
        totalDamage = damage;
        isCritical = isCrit;
        lifeCounter = lifeTime > 0f ? lifeTime : (isCrit ? 1.0f : 0.8f);
        isAggregating = !isCrit; // Crits tách biệt rõ ràng để người chơi nhìn rõ cú đánh chí mạng
        aggregationTimer = aggregationWindow;

        transform.position = position;
        transform.localScale = baseScale;

        if (damageText != null)
        {
            if (isCrit)
            {
                damageText.color = new Color(1f, 0.84f, 0.12f, 1f); // Màu vàng kim hoàng kim rực rỡ
                damageText.text = $"CRIT! {Mathf.RoundToInt(totalDamage)}";
            }
            else
            {
                damageText.color = baseColor;
                damageText.text = Mathf.RoundToInt(totalDamage).ToString();
            }
        }

        PunchScale(isCrit ? punchScaleMultiplier * 1.6f : punchScaleMultiplier);
    }

    public void Setup(float damage, Vector3 position, GameObject target = null)
    {
        Setup(damage, position, false, target);
    }

    // Tương thích ngược với các lệnh gọi cũ: Setup(int)
    public void Setup(int damageDisplay)
    {
        Setup(damageDisplay, transform.position, null);
    }

    public void AddDamage(float additionalDamage)
    {
        totalDamage += additionalDamage;

        // Reset hoặc kéo dài thời gian sống để người chơi kịp đọc số tổng combo
        float minRemainingLife = (lifeTime > 0f ? lifeTime : 0.8f) * 0.6f;
        if (lifeCounter < minRemainingLife)
        {
            lifeCounter = minRemainingLife;
        }

        // Gia hạn cửa sổ gộp thêm một khoảng ngắn
        aggregationTimer = aggregationWindow;

        if (damageText != null)
        {
            damageText.color = baseColor; // Khôi phục độ sáng đầy đủ
            damageText.text = Mathf.RoundToInt(totalDamage).ToString();
        }

        // Hiệu ứng giật nảy to hơn một chút khi số tích lũy lớn dần
        float comboBoost = Mathf.Min(0.35f, (totalDamage / 100f) * 0.15f);
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