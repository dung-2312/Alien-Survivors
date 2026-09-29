using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamager : MonoBehaviour
{
    public float damageAmount; // The amount of damage the enemy deals. AK
    public float lifeTime, growSpeed = 5f; // The lifetime of the enemy and the speed at which the enemy grows. AK
    private Vector3 targetSize; // The target size of the enemy. AK
    public bool shouldKnockBack; // Should the enemy knock back the player. AK
    public bool destroyParent; // Should the parent be destroyed. AK
    public bool damageOverTime; // Should the damage be dealt over time. GK
    public float timeBetweenDamage; // The time between damage. GK
    private float damageCounter; // The counter for the damage. GK
    private List<EnemyController> enemiesInRange = new List<EnemyController>(); // The list of enemies in range. GK
    public bool destroyOnImpact; // Should the enemy be destroyed on impact. GK
    private void Awake()
    {
        // Đảm bảo Collider gắn trên vũ khí luôn hoạt động dưới dạng Trigger để kích hoạt va chạm với quái
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void Start()
    {
        //Destroy(gameObject, lifeTime); // Destroy the enemy after the lifetime. AK

        targetSize = transform.localScale; // Set the target size to the current scale. AK
        transform.localScale = Vector3.zero; // Set the scale to zero. AK
    }

    void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, targetSize, growSpeed * Time.deltaTime); // Grow the enemy towards the target size. AK

        lifeTime -= Time.deltaTime; // Decrease the lifetime. AK

        if(lifeTime <= 0) // If the lifetime is less than or equal to 0. AK
        {
            targetSize = Vector3.zero; // Set the target size to zero. AK

            if(transform.localScale.x == 0f) // If the scale is zero. AK
            {
                Destroy(gameObject); // Destroy the enemy. AK

                if(destroyParent) // If the parent should be destroyed. AK
                {
                    Destroy(transform.parent.gameObject); // Destroy the parent. AK
                }
            }
        }

        if (damageOverTime == true) // If the damage is over time. GK
        {
            damageCounter -= Time.deltaTime; // Decrease the damage counter. GK
            if(damageCounter <= 0) // If the damage counter is less than or equal to 0. GK
            {
                damageCounter = timeBetweenDamage; // Reset the damage counter. GK
                for(int i = 0; i < enemiesInRange.Count; i++) // For each enemy in range. GK
                {
                    EnemyController enemy = enemiesInRange[i];
                    // Kiểm tra quái còn tồn tại, đang active và còn máu (tránh gây sát thương lên quái trong object pool)
                    if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.health > 0)
                    {
                        enemy.TakeDamage(damageAmount, shouldKnockBack); // Take damage. GK
                    }
                    else // If the enemy is null, inactive, or already dead
                    {
                        enemiesInRange.RemoveAt(i); // Remove the enemy from the list. GK
                        i--; // Decrease the counter. GK
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollisionEnter(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollisionEnter(collision.collider);
    }

    private void HandleCollisionEnter(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>() ?? collision.GetComponentInParent<EnemyController>();
            if (enemy == null) return;

            if (damageOverTime == false)
            {
                enemy.TakeDamage(damageAmount, shouldKnockBack);
                if (destroyOnImpact)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                if (!enemiesInRange.Contains(enemy))
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        HandleCollisionExit(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        HandleCollisionExit(collision.collider);
    }

    private void HandleCollisionExit(Collider2D collision)
    {
        if (damageOverTime && collision != null && collision.CompareTag("Enemy"))
        {
            EnemyController enemy = collision.GetComponent<EnemyController>() ?? collision.GetComponentInParent<EnemyController>();
            if (enemy != null)
            {
                enemiesInRange.Remove(enemy);
            }
        }
    }
}