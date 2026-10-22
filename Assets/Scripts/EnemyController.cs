using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
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
    }

    void Start()
    {
        //target = FindObjectOfType<PlayerController>().transform; // Find the player and set it as the target. AK
        target = PlayerHealthController.instance.transform; // Find the player and set it as the target. AK
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

    void Update()
    {
        if (PlayerController.instance.gameObject.activeSelf == true) // If the player is not active. GK
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
                    moveSpeed = Mathf.Abs(moveSpeed *
                                          .5f); // Set the move speed to the negative move speed times 0.5. AK
                }
            }

            if (!movementOverridden)
            {
                theRB.linearVelocity =
                    (target.position - transform.position).normalized * moveSpeed; // Move the enemy towards the target. AK
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

    private void OnCollisionEnter2D(Collision2D collision) // When the enemy collides with something. AK
    {
        if (collision.gameObject.CompareTag("Player") && hitCounter <= 0f) // If the enemy collides with the player. AK
        {
            PlayerHealthController.instance.TakeDamage(damage); // Take damage. AK

            hitCounter = hitWaitTime; // Set the hit counter to the hit wait time. AK
        }
    }

    private void OnCollisionStay2D(Collision2D collision) // When the enemy stays in contact with the player.
    {
        if (collision.gameObject.CompareTag("Player") && hitCounter <= 0f)
        {
            PlayerHealthController.instance.TakeDamage(damage);

            hitCounter = hitWaitTime;
        }
    }

    public void TakeDamage(float damageToTake) // Function to take damage. AK
    {
        // Tránh nhận sát thương khi quái đã chết hoặc đã được thu hồi vào Object Pool
        if (isReturnedToPool || !gameObject.activeInHierarchy || health <= 0f || invulnerable)
        {
            return;
        }

        health -= damageToTake; // Decrease the health by the damage. AK

        if (health <= 0) // If the health is less than or equal to 0. AK
        {
            health = 0f;
            Died?.Invoke(this);
            ReturnToPool(); // Return the enemy to the pool instead of destroying it. AK
            ExperienceLevelController.instance.SpawnExp(transform.position,
                expToGive); // Set the experience when the enemy is destroyed. GK
            if (Random.value <= coinDropRate) // If the random value is less than or equal to the coin drop rate. D
            {
                CoinController.instance.DropCoin(transform.position, coinValue); // Drop the coin. D
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
            }

            if (UIController.instance != null)
            {
                UIController.instance.RegisterKill();
            }

            SFXManager.instance.PlaySFXPitched(0); // Play the sound effect. D
        }
        else
        {
            SFXManager.instance.PlaySFXPitched(1); // Play the sound effect. D
        }

        if (DamageNumberController.instance != null)
        {
            DamageNumberController.instance.SpawnDamage(damageToTake, transform.position, gameObject); // Spawn the damage number. AK
        }
    }

    public void TakeDamage(float damageToTake, bool shouldKnockBack) // Function to take damage. AK
    {
        TakeDamage(damageToTake); // Take damage. AK

        if (shouldKnockBack == true) // If the enemy should be knocked back. AK
        {
            knockBackCounter = knockBackTime; // Set the knock back counter to the knock back time. AK
        }
    }

    private void ReturnToPool() // Called when the enemy dies. AK
    {
        if (isReturnedToPool) // Already returned (e.g. hit by two sources of damage in the same frame). AK
        {
            return;
        }
        isReturnedToPool = true; // Mark it as returned so this can't happen twice. AK
        if (sourcePrefab == null) // Not spawned through the pool (e.g. a boss), so just destroy it.
        {
            Destroy(gameObject);
            return;
        }
        EnemySpawner.instance.RemoveFromTracking(gameObject); // Stop the spawner's despawn check from tracking it. AK
        EnemySpawner.instance.ReturnEnemyToPool(sourcePrefab, gameObject); // Deactivate it and add it back to its pool. AK
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
