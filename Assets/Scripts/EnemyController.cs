using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemySpecialType
{
    Normal,
    Charger,
    Exploder,
    Spitter,
    Necromancer,
    VampiricShield,
    GhostPhaser
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

    // Spitter Variant
    private float spitterCooldownTimer = 0f;
    private float spitterTelegraphTimer = 0f;
    private bool isSpitterTelegraphing = false;

    // Necromancer Variant
    private float necromancerSummonTimer = 0f;
    private GameObject summonRuneObj;

    // VampiricShield Variant
    private bool vampiricShieldActive = false;
    private float shieldHealth = 30f;
    private GameObject shieldOrbsContainer;

    // GhostPhaser Variant
    private float ghostPhaseTimer = 0f;
    private float ghostPhaseDurationTimer = 0f;
    private bool isPhasing = false;

    [HideInInspector]
    public GameObject sourcePrefab; // The prefab this instance was spawned from, so it can be returned to the right pool. AK
    private float baseMoveSpeed; // The move speed as configured on the prefab, restored each time this is reused from the pool. AK
    private float baseHealth; // The health as configured on the prefab, restored each time this is reused from the pool. AK
    private int baseExpToGive;
    private int baseCoinValue;
    private float baseCoinDropRate;
    private float baseDamage;
    private bool isReturnedToPool; // Guards against returning the same enemy to the pool twice in one frame. AK

    [HideInInspector]
    public bool movementOverridden; // Set by a BossController, which drives the rigidbody itself.
    [HideInInspector]
    public bool invulnerable; // Ignores all damage while true (e.g. a boss mid phase-change).
    public event System.Action<EnemyController> Died; // Raised once when health reaches 0, before the enemy is removed.
    public float MaxHealth => baseHealth; // The health this enemy spawned with.

    private float animPhaseOffset;

    void Awake()
    {
        if (theRB == null) theRB = GetComponent<Rigidbody2D>();
        if (theRB != null)
        {
            theRB.gravityScale = 0f;
            theRB.freezeRotation = true;
            theRB.sleepMode = RigidbodySleepMode2D.NeverSleep;
            theRB.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        float configuredSpeed = Mathf.Abs(moveSpeed);
        // TÄƒng tá»‘c Ä‘á»™ cÆ¡ báº£n Ä‘á»ƒ quÃ¡i khÃ´ng bá»‹ rá» rÃ , Ä‘Æ¡ cá»©ng; chuáº©n Vampire Survivors dá»“n dáº­p
        baseMoveSpeed = Mathf.Max(configuredSpeed * 2.4f, 2.85f) * MapSelectionManager.GetEnemySpeedMultiplier();
        moveSpeed = baseMoveSpeed;

        baseHealth = health * MapSelectionManager.GetEnemyHealthMultiplier();
        health = baseHealth;
        baseDamage = damage;
        baseExpToGive = expToGive;
        baseCoinValue = coinValue;
        baseCoinDropRate = coinDropRate;
        baseScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animPhaseOffset = Random.value * 10f;
        CreateGroundShadow();
    }

    private Vector2 CalculateFlockingDirection(Vector2 targetPos, float swayFactor = 0.32f)
    {
        Vector2 toTarget = targetPos - (Vector2)transform.position;
        float dist = toTarget.magnitude;
        Vector2 dir = dist > 0.05f ? toTarget / dist : Vector2.up;
        float flank = Mathf.Sin(Time.time * 4.2f + animPhaseOffset) * swayFactor;
        Vector2 normalTangent = new Vector2(-dir.y, dir.x);
        return (dir + normalTangent * flank).normalized;
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
        damage = baseDamage;

        if (theRB == null) theRB = GetComponent<Rigidbody2D>();
        if (theRB != null)
        {
            theRB.gravityScale = 0f;
            theRB.freezeRotation = true;
            theRB.sleepMode = RigidbodySleepMode2D.NeverSleep;
            theRB.interpolation = RigidbodyInterpolation2D.Interpolate;
            theRB.linearVelocity = Vector2.zero;
        }

        // Promax Difficulty & Thrill Scaling:
        // As survival time progresses, enemies gain +7% health per minute and +3% move speed per minute
        if (LevelManager.instance != null)
        {
            float elapsedMin = LevelManager.instance.timer / 60f;
            float hpMult = 1f + Mathf.Min(1.6f, elapsedMin * 0.07f);
            float spdMult = 1f + Mathf.Min(0.35f, elapsedMin * 0.03f);
            health = baseHealth * hpMult;
            moveSpeed = baseMoveSpeed * spdMult;
        }

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
        isSpitterTelegraphing = false;
        vampiricShieldActive = false;
        isPhasing = false;
        DestroyShieldOrbs();
        DestroySummoningRune();
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = false;
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
        moveSpeed = baseMoveSpeed * 1.25f; // Mini-bosses are faster and more menacing
        damage = baseDamage * 1.2f;
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
    /// Thăng cấp quái vật thường thành biến thể đặc biệt (Charger, Exploder, Spitter, Necromancer, VampiricShield, GhostPhaser).
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
                spriteRenderer.color = new Color(1f, 0.65f, 0.2f, 1f); // Cam lửa
            }
        }
        else if (type == EnemySpecialType.Exploder)
        {
            health = baseHealth * 0.85f;
            moveSpeed = baseMoveSpeed * 1.15f;
            transform.localScale = baseScale * 1.15f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.35f, 1f, 0.45f, 1f); // Xanh axit phóng xạ
            }
        }
        else if (type == EnemySpecialType.Spitter)
        {
            spitterCooldownTimer = Random.Range(1.8f, 3.2f);
            transform.localScale = baseScale * 1.15f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.20f, 0.95f, 0.35f, 1f); // Xanh lục dạ quang
            }
        }
        else if (type == EnemySpecialType.Necromancer)
        {
            health = baseHealth * 2.2f;
            moveSpeed = baseMoveSpeed * 0.75f;
            necromancerSummonTimer = Random.Range(3.5f, 5.5f);
            expToGive = baseExpToGive * 3;
            coinValue = baseCoinValue * 2;
            transform.localScale = baseScale * 1.35f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.75f, 0.25f, 0.95f, 1f); // Tím ma pháp
            }
            AttachSummoningRune();
        }
        else if (type == EnemySpecialType.VampiricShield)
        {
            health = baseHealth * 1.6f;
            vampiricShieldActive = true;
            shieldHealth = 35f;
            transform.localScale = baseScale * 1.25f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.95f, 0.20f, 0.25f, 1f); // Đỏ huyết
            }
            AttachBloodShieldOrbs();
        }
        else if (type == EnemySpecialType.GhostPhaser)
        {
            ghostPhaseTimer = Random.Range(3.5f, 5.5f);
            isPhasing = false;
            transform.localScale = baseScale * 1.15f;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.45f, 0.88f, 1f, 0.95f); // Lam bạc hư vô
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
            theRB.linearVelocity = CalculateFlockingDirection(target.position) * moveSpeed;
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
        // 3. Quái Bắn Axit (Spitter Alien)
        if (specialType == EnemySpecialType.Spitter)
        {
            float distSqr = (target.position - transform.position).sqrMagnitude;
            Vector2 dirToTarget = (target.position - transform.position).normalized;

            if (isSpitterTelegraphing)
            {
                theRB.linearVelocity = Vector2.zero;
                spitterTelegraphTimer -= Time.deltaTime;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(new Color(0.20f, 0.95f, 0.35f), Color.white, Mathf.PingPong(Time.time * 16f, 1f));
                }

                if (spitterTelegraphTimer <= 0f)
                {
                    isSpitterTelegraphing = false;
                    spitterCooldownTimer = Random.Range(2.8f, 4.0f);
                    if (spriteRenderer != null) spriteRenderer.color = new Color(0.20f, 0.95f, 0.35f);

                    BossProjectile.Spawn(transform.position, dirToTarget * 7.5f, damage * 0.9f, null,
                        new Color(0.3f, 1f, 0.25f, 0.95f), spriteRenderer != null ? spriteRenderer.sortingLayerID : 0, 10, 0.45f, 5f);

                    if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
                }
                return;
            }

            spitterCooldownTimer -= Time.deltaTime;
            if (spitterCooldownTimer <= 0f && distSqr <= 49f) // Cự ly trong 7m
            {
                isSpitterTelegraphing = true;
                spitterTelegraphTimer = 0.45f;
                return;
            }

            // Kiting AI: duy trì khoảng cách 4.5m - 6.5m
            if (distSqr < 16f)
            {
                theRB.linearVelocity = -dirToTarget * (baseMoveSpeed * 0.8f);
            }
            else if (distSqr > 36f)
            {
                theRB.linearVelocity = dirToTarget * moveSpeed;
            }
            else
            {
                Vector2 perpendicular = new Vector2(-dirToTarget.y, dirToTarget.x);
                theRB.linearVelocity = perpendicular * (baseMoveSpeed * 0.5f);
            }
            return;
        }

        // 4. Pháp sư Chiêu Hồn (Necromancer Alien)
        if (specialType == EnemySpecialType.Necromancer)
        {
            theRB.linearVelocity = CalculateFlockingDirection(target.position) * moveSpeed;
            necromancerSummonTimer -= Time.deltaTime;
            if (necromancerSummonTimer <= 0f)
            {
                necromancerSummonTimer = Random.Range(5.5f, 7.5f);
                StartCoroutine(NecromancerSummonRoutine());
            }
            return;
        }

        // 5. Quái Huyết Thuẫn (VampiricShield Alien)
        if (specialType == EnemySpecialType.VampiricShield)
        {
            theRB.linearVelocity = CalculateFlockingDirection(target.position) * moveSpeed;
            if (shieldOrbsContainer != null)
            {
                shieldOrbsContainer.transform.Rotate(0f, 0f, 220f * Time.deltaTime);
            }
            return;
        }

        // 6. Bóng Ma Hư Vô (GhostPhaser Alien)
        if (specialType == EnemySpecialType.GhostPhaser)
        {
            if (isPhasing)
            {
                theRB.linearVelocity = (target.position - transform.position).normalized * (baseMoveSpeed * 1.75f);
                ghostPhaseDurationTimer -= Time.deltaTime;
                if (ghostPhaseDurationTimer <= 0f)
                {
                    isPhasing = false;
                    ghostPhaseTimer = Random.Range(3.5f, 5.5f);
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.color = new Color(0.45f, 0.88f, 1f, 0.95f);
                    }
                    Collider2D pCol = GetComponent<Collider2D>();
                    if (pCol != null) pCol.isTrigger = false;
                }
                return;
            }

            theRB.linearVelocity = CalculateFlockingDirection(target.position) * moveSpeed;
            ghostPhaseTimer -= Time.deltaTime;
            if (ghostPhaseTimer <= 0f)
            {
                isPhasing = true;
                ghostPhaseDurationTimer = 2.2f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = new Color(0.45f, 0.88f, 1f, 0.28f); // Xuyên thấu
                }
                Collider2D pCol = GetComponent<Collider2D>();
                if (pCol != null) pCol.isTrigger = true;
            }
            return;
        }

        // 7. Quái vật thường di chuyển bám theo mục tiêu
        theRB.linearVelocity = CalculateFlockingDirection(target.position) * moveSpeed;
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

            if (specialType == EnemySpecialType.VampiricShield)
            {
                health = Mathf.Min(baseHealth * 1.6f, health + 15f); // Hồi máu hút huyết
            }

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

            if (specialType == EnemySpecialType.VampiricShield)
            {
                health = Mathf.Min(baseHealth * 1.6f, health + 15f); // Hồi máu hút huyết
            }

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

        if (vampiricShieldActive && specialType == EnemySpecialType.VampiricShield)
        {
            damageToTake *= 0.35f; // Khiên máu hấp thụ 65% sát thương
            shieldHealth -= damageToTake;
            if (shieldHealth <= 0f)
            {
                vampiricShieldActive = false;
                DestroyShieldOrbs();
            }
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
                                  (specialType == EnemySpecialType.Exploder ? new Color(0.35f, 1f, 0.45f) :
                                  (specialType == EnemySpecialType.Spitter ? new Color(0.20f, 0.95f, 0.35f) :
                                  (specialType == EnemySpecialType.Necromancer ? new Color(0.75f, 0.25f, 0.95f) :
                                  (specialType == EnemySpecialType.VampiricShield ? new Color(0.95f, 0.20f, 0.25f) :
                                  (specialType == EnemySpecialType.GhostPhaser ? new Color(0.45f, 0.88f, 1f, isPhasing ? 0.28f : 0.95f) : Color.white))))));
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

    private IEnumerator NecromancerSummonRoutine()
    {
        theRB.linearVelocity = Vector2.zero;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(0.4f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(0.75f, 0.25f, 0.95f);
        }

        if (EnemySpawner.instance != null && sourcePrefab != null)
        {
            for (int i = 0; i < 2; i++)
            {
                Vector3 spawnOffset = (Vector3)(Random.insideUnitCircle.normalized * 1.4f);
                GameObject minion = EnemySpawner.instance.SpawnEnemy(sourcePrefab, transform.position + spawnOffset);
                if (minion != null)
                {
                    minion.transform.localScale = baseScale * 0.7f;
                    EnemyController mec = minion.GetComponent<EnemyController>();
                    if (mec != null)
                    {
                        mec.health = baseHealth * 0.4f;
                        mec.moveSpeed = baseMoveSpeed * 1.35f;
                    }
                }
            }
        }
    }

    private void AttachBloodShieldOrbs()
    {
        DestroyShieldOrbs();
        shieldOrbsContainer = new GameObject("BloodShieldOrbs");
        shieldOrbsContainer.transform.SetParent(transform, false);

        for (int i = 0; i < 2; i++)
        {
            GameObject orb = new GameObject("Orb_" + i, typeof(SpriteRenderer));
            orb.transform.SetParent(shieldOrbsContainer.transform, false);
            float angle = i * 180f * Mathf.Deg2Rad;
            orb.transform.localPosition = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 0.95f;
            orb.transform.localScale = Vector3.one * 0.45f;

            SpriteRenderer osr = orb.GetComponent<SpriteRenderer>();
            osr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
            osr.color = new Color(1f, 0.15f, 0.2f, 0.95f);
            osr.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
            osr.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder : 10) + 2;
        }
    }

    private void DestroyShieldOrbs()
    {
        if (shieldOrbsContainer != null)
        {
            Destroy(shieldOrbsContainer);
            shieldOrbsContainer = null;
        }
    }

    private void AttachSummoningRune()
    {
        DestroySummoningRune();
        summonRuneObj = new GameObject("SummonRune", typeof(SpriteRenderer));
        summonRuneObj.transform.SetParent(transform, false);
        summonRuneObj.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        summonRuneObj.transform.localScale = new Vector3(1.3f, 0.5f, 1f);

        SpriteRenderer rsr = summonRuneObj.GetComponent<SpriteRenderer>();
        rsr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
        rsr.color = new Color(0.75f, 0.2f, 1f, 0.55f);
        rsr.sortingLayerName = spriteRenderer != null ? spriteRenderer.sortingLayerName : "Default";
        rsr.sortingOrder = (spriteRenderer != null ? spriteRenderer.sortingOrder : 2) - 1;
    }

    private void DestroySummoningRune()
    {
        if (summonRuneObj != null)
        {
            Destroy(summonRuneObj);
            summonRuneObj = null;
        }
    }
}
