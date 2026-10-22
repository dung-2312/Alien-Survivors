using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialPickupType
{
    HealthMeat,
    Magnet
}

public class SpecialPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public SpecialPickupType pickupType = SpecialPickupType.HealthMeat;
    public float healAmount = 30f;
    public float magnetAttractSpeed = 8f;

    private bool isMovingToPlayer = false;
    private Transform playerTransform;
    private PlayerController playerController;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
        if (initialScale == Vector3.zero) initialScale = Vector3.one;
    }

    private void Start()
    {
        if (PlayerController.instance != null)
        {
            playerController = PlayerController.instance;
            playerTransform = PlayerController.instance.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            if (PlayerController.instance != null)
            {
                playerController = PlayerController.instance;
                playerTransform = PlayerController.instance.transform;
            }
            return;
        }

        // Nhẹ nhàng bay về phía player khi ở trong bán kính nhặt đồ
        if (!isMovingToPlayer && playerController != null)
        {
            float pickupRangeSqr = playerController.pickupRange * playerController.pickupRange;
            if ((transform.position - playerTransform.position).sqrMagnitude <= pickupRangeSqr)
            {
                isMovingToPlayer = true;
            }
        }

        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                (magnetAttractSpeed + (playerController != null ? playerController.moveSpeed : 0f)) * Time.deltaTime
            );
        }
        else
        {
            // Hiệu ứng bồng bềnh nhẹ (Bobbing) khi nằm trên sàn
            float bobOffset = Mathf.Sin(Time.time * 4f) * 0.08f;
            transform.localScale = initialScale * (1f + bobOffset * 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        switch (pickupType)
        {
            case SpecialPickupType.HealthMeat:
                if (PlayerHealthController.instance != null)
                {
                    PlayerHealthController.instance.Heal(healAmount);
                }
                break;

            case SpecialPickupType.Magnet:
                if (ExpManager.instance != null)
                {
                    ExpManager.instance.AttractAllGems();
                }
                break;
        }

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFX(2);
        }

        Destroy(gameObject);
    }
}
