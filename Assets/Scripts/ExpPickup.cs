using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPickup : MonoBehaviour
{
    [Header("Values")]
    public int expValue = 1;
    public float moveSpeed = 6f;
    public float acceleration = 15f;

    [Header("Visual Tiers (Cấp bậc ngọc)")]
    public Color tier1Color = new Color(0.35f, 0.85f, 1f, 1f);   // 1 - 5 exp (Cyan / Blue)
    public Color tier2Color = new Color(0.3f, 1f, 0.45f, 1f);    // 6 - 25 exp (Emerald Green)
    public Color tier3Color = new Color(1f, 0.25f, 0.25f, 1f);    // 26 - 99 exp (Ruby Red)
    public Color tier4Color = new Color(1f, 0.85f, 0.15f, 1f);    // 100+ exp (Solar Gold)

    public bool movingToPlayer { get; private set; }

    private float currentMoveSpeed;
    private float baseMoveSpeed;
    private Transform targetPlayerTransform;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private float punchTimer;
    private float targetScaleMult = 1f;

    private void Awake()
    {
        baseMoveSpeed = moveSpeed;
        baseScale = transform.localScale;
        if (baseScale == Vector3.zero) baseScale = Vector3.one;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Update()
    {
        // 1. Hiệu ứng phóng to đàn hồi khi nhận gộp kinh nghiệm (Zero-GC Punch Scale)
        if (punchTimer > 0f)
        {
            punchTimer -= Time.deltaTime;
            float punch = 1f + (punchTimer / 0.15f) * 0.35f;
            transform.localScale = baseScale * (targetScaleMult * punch);
            if (punchTimer <= 0f && !movingToPlayer)
            {
                enabled = false; // Tắt Update khi hết hiệu ứng nảy để giải phóng CPU
            }
        }

        // 2. Di chuyển nhanh dần đều về phía Player khi đã bị hút
        if (movingToPlayer && targetPlayerTransform != null)
        {
            currentMoveSpeed += acceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPlayerTransform.position, currentMoveSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Thiết lập lại ngọc khi lấy ra từ Object Pool.
    /// </summary>
    public void ResetPickup(int newExpValue)
    {
        expValue = newExpValue;
        movingToPlayer = false;
        currentMoveSpeed = baseMoveSpeed;
        targetPlayerTransform = null;
        punchTimer = 0f;

        UpdateVisuals();

        // QUAN TRỌNG: Tắt Update() khi đang nằm yên trên sàn đấu để triệt tiêu tải CPU cho 200+ ngọc!
        // ExpManager sẽ quản lý bán kính hút tập trung trong 1 vòng lặp duy nhất.
        enabled = false;
    }

    /// <summary>
    /// Gộp thêm điểm kinh nghiệm vào viên ngọc này và nâng cấp bậc hiển thị.
    /// </summary>
    public void AddExp(int amount)
    {
        expValue += amount;
        UpdateVisuals();

        // Kích hoạt hiệu ứng nảy to ngọc lên
        punchTimer = 0.15f;
        enabled = true; // Bật Update tạm thời để chạy hiệu ứng punch scale
    }

    /// <summary>
    /// Bắt đầu bay về phía người chơi.
    /// </summary>
    public void StartMovingToPlayer(Transform playerTransform, float playerSpeed)
    {
        if (movingToPlayer) return;

        movingToPlayer = true;
        targetPlayerTransform = playerTransform;
        currentMoveSpeed = baseMoveSpeed + playerSpeed;
        enabled = true; // Bật Update để ngọc bay mượt mà
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        Color targetColor;
        if (expValue <= 5)
        {
            targetColor = tier1Color;
            targetScaleMult = 1f;
        }
        else if (expValue <= 25)
        {
            targetColor = tier2Color;
            targetScaleMult = 1.25f;
        }
        else if (expValue <= 99)
        {
            targetColor = tier3Color;
            targetScaleMult = 1.5f;
        }
        else
        {
            targetColor = tier4Color;
            targetScaleMult = 1.85f;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }

        transform.localScale = baseScale * targetScaleMult;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (ExperienceLevelController.instance != null)
            {
                ExperienceLevelController.instance.GetExp(expValue);
            }

            if (ExpManager.instance != null)
            {
                ExpManager.instance.ReturnToPool(this);
            }
            else if (ExperienceLevelController.instance != null)
            {
                ExperienceLevelController.instance.ReturnToPool(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
