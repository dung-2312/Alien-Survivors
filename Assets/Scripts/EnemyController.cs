using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySpecialType
{
    Normal,
    Charger,   // Dá»«ng láº¡i tÃ­ch lá»±c 0.6s -> HÃºc tháº³ng cá»±c nhanh x3.2 tá»‘c Ä‘á»™
    Exploder   // Äáº¿n gáº§n hoáº·c cháº¿t -> Phá»“ng to phÃ¡t ná»• AOE sÃ¡t thÆ°Æ¡ng cáº£ Player láº«n Enemies
}

public class EnemyController : MonoBehaviour
{
    public static float globalFreezeTimer = 0f;

    public static void FreezeAllEnemies(float duration)
    {
        globalFreezeTimer = duration;
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(8);
        }
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("[ TIME FROZEN - 5 SECONDS! ]", 2.0f);
        }
    }

    public Rigidbody2D theRB; // The rigidbody component. AK
    public float moveSpeed; // The speed at which the enemy moves. AK
    private Transform target; // The target that the enemy will follow. AK
    public float damage; // The damage that the enemy deals. AK
    public float hitWaitTime = 1f; // The time to wait before the enemy can hit the player again. AK
    private float hitCounter; // The counter for the hit wait time. AK
    public float health = 5f; // The health of the enemy. AK
    public float knockBackTime = .5f; // The time to knock back the enemy. AK
    private float knockBackCounter; // The counter for the knock back time. AK
    public int expToGive = 1; // The experience to give when the enemy is destroyed. GK
    public int coinValue = 1; // The value of the coin. D
    public float coinDropRate = 0.5f; // The rate at which the coin drops. D

    [Header("Mini-Boss / Elite Monster")]
    public bool isMiniBoss = false;
    public GameObject specialDropPrefab; // Meat or Magnet prefab
    private Vector3 baseScale;
    private SpriteRenderer spriteRenderer;

    [Header("Special Alien Variants")]
    public EnemySpecialType specialType = EnemySpecialType.Normal;
    private float chargeCooldownTimer = 0f;
    private bool isCharging = false;
    private bool isTelegraphing = false;
    private Vector2 chargeDirection;
    private float chargeDurationTimer = 0f;
    private float telegraphTimer = 0f;

    private bool isExploding = false;
    private float explosionCountdownTimer = 0f;
    private bool hasDetonated = false;

    [HideInInspector]
    public GameObject sourcePrefab; // The prefab this instance was spawned from, so it can be returned to the right pool. AK
    private float baseMoveSpeed; // The move speed as configured on the prefab, restored each time this is reused from the pool. AK
    private float baseHealth; // The health as configured on the prefab, restored each time this is reused from the pool. AK
    private int baseExpToGive;
    private int baseCoinValue;
    private float baseCoinDropRate;
    private bool isReturnedToPool; // Guards against returning the same enemy to the pool twice in one frame. AK

    [HideInInspector]
    public bool movementOverridden; // Set by a BossController, which drives the rigidbody itself.
    [HideInInspector]
    public bool invulnerable; // Ignores all damage while true (e.g. a boss mid phase-change).
    public event System.Action<EnemyController> Died; // Raised once when health reaches 0, before the enemy is removed.
    public float MaxHealth => baseHealth; // The health this enemy spawned with.

    void Awake()
    {
        baseMoveSpeed = Mathf.Abs(moveSpeed); // Remember the prefab's configured speed before knockback flips its sign. AK
        baseHealth = health; // Remember the prefab's configured health. AK
        baseExpToGive = expToGive;
        baseCoinValue = coinValue;
        baseCoinDropRate = coinDropRate;
        baseScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        CreateGroundShadow();
    }

    void Start()
    {
        //target = FindObjectOfType<PlayerController>().transform; // Find the player and set it as the target. AK
        if (PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform; // Find the player and set it as the target. AK
        }
        CreateGroundShadow();
    }

    public void ResetForSpawn() // Reset this pooled enemy so it looks and behaves like a freshly spawned one. AK
    {
        health = baseHealth; // Restore full health. AK
        moveSpeed = baseMoveSpeed; // Restore the base move speed (knockback and death can leave it modified). AK
        expToGive = baseExpToGive;
        coinValue = baseCoinValue;
        coinDropRate = baseCoinDropRate;
        hitCounter = 0f; // Clear the player-hit cooldown. AK
        knockBackCounter = 0f; // Clear any pending knockback. AK
        isReturnedToPool = false; // It's active again, so it can be returned to the pool once more later. AK
        isMiniBoss = false;
        specialType = EnemySpecialType.Normal;
        isCharging = false;
        isTelegraphing = false;
        isExploding = false;
        hasDetonated = false;
        chargeCooldownTimer = Random.Range(3f, 5.5f);
        transform.localScale = baseScale;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void PromoteToMiniBoss(float healthMultiplier = 5f, float scaleMultiplier = 1.6f, GameObject dropPrefab = null)
    {
        isMiniBoss = true;
        health = baseHealth * healthMultiplier;
        expToGive = baseExpToGive * 5;
        coinValue = baseCoinValue * 5;
        coinDropRate = 1f;
        transform.localScale = baseScale * scaleMultiplier;
        if (dropPrefab != null)
        {
            specialDropPrefab = dropPrefab;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.45f, 0.45f, 1f); // Reddish Elite Tint
        }
    }

    /// <summary>
    /// ThÄƒng cáº¥p quÃ¡i váº­t thÆ°á»ng thÃ nh biáº¿n thá»ƒ Ä‘áº·c biá»‡t (Charger hoáº·c Exploder).
    /// </summary>
    public void PromoteToSpecial(EnemySpecialType type)
    {
        specialType = type;
        if (type == EnemySpecialType.Charger)
        {
            chargeCooldownTimer = Random.Range(2f, 4f);
            transform.localScale = baseScale * 1.25f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.65f, 0.2f, 1f); // Cam lá»­a
            }
        }
        else if (type == EnemySpecialType.Exploder)
        {
            health = baseHealth * 0.85f;
            moveSpeed = baseMoveSpeed * 1.15f;
            transform.localScale = baseScale * 1.15f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.35f, 1f, 0.45f, 1f); // Xanh axit phÃ³ng xáº¡
            }
        }
    }

    void Update()
    {
        if (globalFreezeTimer > 0f)
        {
            theRB.linearVelocity = Vector2.zero;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.42f, 0.88f, 1f, 1f);
            }
            return;
        }

        if (target == null && PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform;
        }

        if (PlayerController.instance != null && PlayerController.instance.gameObject.activeSelf == true) // If the player is active. GK
        {
            if (knockBackCounter > 0) // If the knock back counter is greater than 0. AK
            {
                knockBackCounter -= Time.deltaTime; // Decrease the knock back counter. AK

                if (moveSpeed > 0) // If the move speed is greater than 0. AK
                {
                    moveSpeed = -moveSpeed * 2f; // Set the move speed to the negative move speed times 2. AK
                }

                if (knockBackCounter <= 0) // If the knock back counter is less than or equal to 0. AK
                {
                    moveSpeed = Mathf.Abs(moveSpeed * .5f); // Set the move speed to the negative move speed times 0.5. AK
                }
            }

            if (!movementOverridden)
            {
                HandleMovementAndSpecialAI();
            }

            if (hitCounter > 0) // If the hit counter is greater than 0. AK
            {
                hitCounter -= Time.deltaTime; // Decrease the hit counter. AK
            }
        }
        else if (!movementOverridden)
        {
            theRB.linearVelocity = Vector2.zero; // Set the velocity to 0. GK
        }
    }

    private void HandleMovementAndSpecialAI()
    {
        if (target == null) return;

        // Cập nhật hướng nhìn quay mặt mượt mà về phía người chơi
        if (spriteRenderer != null)
        {
            float dx = target.position.x - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f)
            {
                spriteRenderer.flipX = dx < 0f;
            }
        }

        // 1. Quái Húc Lao (Charger Alien)
        if (specialType == EnemySpecialType.Charger)
        {
            if (isTelegraphing)
            {
                theRB.linearVelocity = Vector2.zero;
                telegraphTimer -= Time.deltaTime;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(new Color(1f, 0.65f, 0.2f), Color.red, Mathf.PingPong(Time.time * 12f, 1f));
                }

                if (telegraphTimer <= 0f)
                {
                    isTelegraphing = false;
                    isCharging = true;
                    chargeDurationTimer = 0.75f;
                    chargeDirection = (target.position - transform.position).normalized;
                }
                return;
            }

            if (isCharging)
            {
                theRB.linearVelocity = chargeDirection * (baseMoveSpeed * 3.2f);
                chargeDurationTimer -= Time.deltaTime;

                if (chargeDurationTimer <= 0f)
                {
                    isCharging = false;
                    chargeCooldownTimer = Random.Range(3.5f, 5.5f);
                    if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.65f, 0.2f);
                }
                return;
            }

            // Di chuyá»ƒn bÃ¬nh thÆ°á»ng vÃ  Ä‘áº¿m cooldown Ä‘á»ƒ lao
            theRB.linearVelocity = (target.position - transform.position).normalized * moveSpeed;
            chargeCooldownTimer -= Time.deltaTime;

            if (chargeCooldownTimer <= 0f && (target.position - transform.position).sqrMagnitude < 64f) // Trong táº§m 8m
            {
                isTelegraphing = true;
                telegraphTimer = 0.65f;
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(1);
            }
            return;
        }

        // 2. QuÃ¡i PhÃ¡t Ná»• (Exploder Alien)
        if (specialType == EnemySpecialType.Exploder)
        {
            float distSqr = (target.position - transform.position).sqrMagnitude;

            if (!isExploding && distSqr <= 2.8f) // CÃ¡ch player ~1.6m -> KÃ­ch hoáº¡t ngÃ²i ná»•
            {
                isExploding = true;
                explosionCountdownTimer = 0.65f;
            }

            if (isExploding)
            {
                theRB.linearVelocity = (target.position - transform.position).normalized * (baseMoveSpeed * 0.4f);
                float pulse = Mathf.PingPong(Time.time * 15f, 1f);
                transform.localScale = baseScale * (1.15f + pulse * 0.35f);

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(new Color(0.35f, 1f, 0.45f), Color.red, pulse);
                }

                explosionCountdownTimer -= Time.deltaTime;
                if (explosionCountdownTimer <= 0f)
                {
                    DetonateExplosion();
                }
                return;
            }
        }

        // 3. QuÃ¡i váº­t thÆ°á»ng di chuyá»ƒn bÃ¡m theo má»¥c tiÃªu
        theRB.linearVelocity = (target.position - transform.position).normalized * moveSpeed;
    }

    /// <summary>
    /// PhÃ¡t ná»• lan AOE: sÃ¡t thÆ°Æ¡ng Player VÃ€ tiÃªu diá»‡t cÃ¡c quÃ¡i váº­t lÃ¢n cáº­n.
    /// </summary>
    public void DetonateExplosion()
    {
        if (hasDetonated || isReturnedToPool) return;
        hasDetonated = true;

        float explosionRadius = 3.5f;

        // SÃ¡t thÆ°Æ¡ng Player náº¿u á»Ÿ trong táº§m ná»•
        if (PlayerHealthController.instance != null && target != null)
        {
            if ((target.position - transform.position).sqrMagnitude <= explosionRadius * explosionRadius)
            {
                PlayerHealthController.instance.TakeDamage(damage * 1.8f);
            }
        }

        // SÃ¡t thÆ°Æ¡ng toÃ n bá»™ káº» Ä‘á»‹ch xung quanh trong bÃ¡n kÃ­nh ná»•
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            if (col != null && col.gameObject != gameObject && col.TryGetComponent<EnemyController>(out var other))
            {
                other.TakeDamage(damage * 3.5f, true); // QuÃ¡i ná»• dÃ¢y chuyá»n cá»±c Ä‘Ã£!
            }
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.28f, 0.35f);
        }

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        ExperienceLevelController.instance.SpawnExp(transform.position, expToGive * 2);
        if (UIController.instance != null)
        {
            UIController.instance.RegisterKill();
        }

        ReturnToPool();
    }

    private void OnCollisionEnter2D(Collision2D collision) // When the enemy collides with something. AK
    {
        if (collision.gameObject.CompareTag("Player") && hitCounter <= 0f) // If the enemy collides with the player. AK
        {
            float dmg = damage;
            if (isCharging) dmg *= 1.5f;
            PlayerHealthController.instance.TakeDamage(dmg); // Take damage. AK

            hitCounter = hitWaitTime; // Set the hit counter to the hit wait time. AK
        }
    }

    private void OnCollisionStay2D(Collision2D collision) // When the enemy stays in contact with the player.
    {
        if (collision.gameObject.CompareTag("Player") && hitCounter <= 0f)
        {
            float dmg = damage;
            if (isCharging) dmg *= 1.5f;
            PlayerHealthController.instance.TakeDamage(dmg);

            hitCounter = hitWaitTime;
        }
    }

    public void TakeDamage(float damageToTake, bool shouldKnockBack, bool isCrit)
    {
        // Tránh nhận sát thương khi quái đã chết hoặc đã được thu hồi vào Object Pool
        if (isReturnedToPool || !gameObject.activeInHierarchy || health <= 0f || invulnerable)
        {
            return;
        }

        if (globalFreezeTimer > 0f)
        {
            damageToTake *= 1.35f; // Quái bị đóng băng nhận thêm 35% sát thương vỡ vụn
        }

        health -= damageToTake;

        if (shouldKnockBack)
        {
            knockBackCounter = knockBackTime;
        }

        // Hiệu ứng chớp trắng phản hồi lực đánh (Hit-Flash) cực đã mắt
        if (gameObject.activeInHierarchy && spriteRenderer != null && health > 0f)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(HitFlashCo());
        }

        if (health <= 0)
        {
            health = 0f;
            Died?.Invoke(this);

            if (specialType == EnemySpecialType.Exploder && !hasDetonated)
            {
                DetonateExplosion();
                return;
            }

            ReturnToPool();
            ExperienceLevelController.instance.SpawnExp(transform.position, expToGive);
            if (Random.value <= coinDropRate)
            {
                CoinController.instance.DropCoin(transform.position, coinValue);
            }

            if (isMiniBoss)
            {
                if (CameraController.instance != null)
                {
                    CameraController.instance.Shake(0.35f, 0.4f);
                }

                if (specialDropPrefab != null)
                {
                    Instantiate(specialDropPrefab, transform.position, Quaternion.identity);
                }
                else
                {
                    // Rơi vật phẩm đặc biệt ngẫu nhiên khi diệt Mini-Boss!
                    SpecialPickup.SpawnPickup(transform.position + Vector3.right * 0.4f, (SpecialPickupType)Random.Range(0, 5));
                }

                TreasureChest.SpawnChest(transform.position, 1);
            }

            if (UIController.instance != null)
            {
                UIController.instance.RegisterKill();
            }

            if (ComboManager.instance != null)
            {
                ComboManager.instance.RegisterKill(transform.position);
            }

            if (PlayerController.instance != null)
            {
                PlayerController.instance.OnEnemyKilled(transform.position);
            }

            SFXManager.instance.PlaySFXPitched(0);
        }
        else
        {
            if (isCrit)
            {
                SFXManager.instance.PlaySFXPitched(0);
                if (CameraController.instance != null) CameraController.instance.Shake(0.08f, 0.12f);
            }
            else
            {
                SFXManager.instance.PlaySFXPitched(1);
            }
        }

        if (DamageNumberController.instance != null)
        {
            DamageNumberController.instance.SpawnDamage(damageToTake, transform.position, isCrit, gameObject);
        }
    }

    public void TakeDamage(float damageToTake, bool shouldKnockBack)
    {
        TakeDamage(damageToTake, shouldKnockBack, false);
    }

    public void TakeDamage(float damageToTake)
    {
        TakeDamage(damageToTake, false, false);
    }

    private void ReturnToPool() // Called when the enemy dies. AK
    {
        if (isReturnedToPool) // Already returned (e.g. hit by two sources of damage in the same frame). AK
        {
            return;
        }
        isReturnedToPool = true; // Mark it as returned so this can't happen twice. AK

        SpawnDeathBurst();

        if (sourcePrefab == null) // Not spawned through the pool (e.g. a boss), so just destroy it.
        {
            Destroy(gameObject);
            return;
        }
        EnemySpawner.instance.RemoveFromTracking(gameObject); // Stop the spawner's despawn check from tracking it. AK
        EnemySpawner.instance.ReturnEnemyToPool(sourcePrefab, gameObject); // Deactivate it and add it back to its pool. AK
    }

    private Coroutine flashCoroutine;
    private GameObject shadowObj;

    private void CreateGroundShadow()
    {
        if (shadowObj != null || transform.Find("EnemyShadow") != null) return;
        shadowObj = new GameObject("EnemyShadow", typeof(SpriteRenderer));
        shadowObj.transform.SetParent(transform, false);
        shadowObj.transform.localPosition = new Vector3(0f, -0.42f, 0f);
        shadowObj.transform.localScale = new Vector3(0.85f, 0.32f, 1f);

        SpriteRenderer ssr = shadowObj.GetComponent<SpriteRenderer>();
        ssr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
        ssr.color = new Color(0f, 0f, 0f, 0.42f);
        ssr.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
        ssr.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder : 1) - 1;
    }

    private IEnumerator HitFlashCo()
    {
        if (spriteRenderer == null) yield break;
        Color prevCol = spriteRenderer.color;
        spriteRenderer.color = Color.white * 2.2f;

        yield return new WaitForSeconds(0.08f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = isMiniBoss ? new Color(1f, 0.45f, 0.45f, 1f) : 
                                  (specialType == EnemySpecialType.Charger ? new Color(1f, 0.65f, 0.2f) :
                                  (specialType == EnemySpecialType.Exploder ? new Color(0.35f, 1f, 0.45f) : Color.white));
        }
    }

    private void SpawnDeathBurst()
    {
        // Hiệu ứng tia sáng linh hồn ma mị tan biến khi quái bị tiêu diệt
        GameObject puff = new GameObject("SoulBurst", typeof(SpriteRenderer));
        puff.transform.position = transform.position;
        SpriteRenderer psr = puff.GetComponent<SpriteRenderer>();
        psr.sprite = EnvironmentDecorManager.GetOrCreateFlowerSprite(2);
        psr.color = new Color(0.85f, 0.95f, 1f, 0.85f);
        psr.sortingOrder = 6;
        puff.transform.localScale = Vector3.one * 0.9f;
        puff.AddComponent<DebrisFragment>().Init(Vector2.up * 1.6f, 0.35f);
    }

    public void DespawnToPool() // Called by the spawner when the enemy wanders too far from the player. AK
    {
        if (isReturnedToPool) // Already returned. AK
        {
            return;
        }
        isReturnedToPool = true; // Mark it as returned so this can't happen twice. AK
        if (sourcePrefab == null)
        {
            Destroy(gameObject);
            return;
        }
        EnemySpawner.instance.ReturnEnemyToPool(sourcePrefab, gameObject); // Deactivate it and add it back to its pool. AK
    }
}
