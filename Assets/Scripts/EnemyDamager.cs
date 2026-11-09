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
        // Äáº£m báº£o Collider gáº¯n trÃªn vÅ© khÃ­ luÃ´n hoáº¡t Ä‘á»™ng dÆ°á»›i dáº¡ng Trigger Ä‘á»ƒ kÃ­ch hoáº¡t va cháº¡m vá»›i quÃ¡i
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
                    // Kiá»ƒm tra quÃ¡i cÃ²n tá»“n táº¡i, Ä‘ang active vÃ  cÃ²n mÃ¡u (trÃ¡nh gÃ¢y sÃ¡t thÆ°Æ¡ng lÃªn quÃ¡i trong object pool)
                    if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.health > 0)
                    {
                        DealDamageToEnemy(enemy, enemy.transform.position);
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
                DealDamageToEnemy(enemy, collision.bounds.center);
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
        else
        {
            // Kiểm tra va chạm với vật cản có thể phá hủy (hòm gỗ, bình gốm, bia mộ)
            ObstacleProp obs = collision.GetComponent<ObstacleProp>() ?? collision.GetComponentInParent<ObstacleProp>();
            if (obs != null)
            {
                obs.TakeDamage(damageAmount);
                if (destroyOnImpact)
                {
                    Destroy(gameObject);
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

    private void DealDamageToEnemy(EnemyController enemy, Vector3 hitPos)
    {
        float critChance = 0.12f;
        float critMultiplier = 2.0f;
        if (PlayerController.instance != null)
        {
            critChance = PlayerController.instance.GetCriticalStrikeChance();
            critMultiplier = PlayerController.instance.GetCriticalStrikeMultiplier();
        }

        bool isCrit = Random.value < critChance;
        float finalDmg = isCrit ? damageAmount * critMultiplier : damageAmount;

        // Apply Meta Might damage bonus
        finalDmg *= MetaProgression.GetDamageMultiplier();

        if (PlayerController.instance != null && PlayerController.instance.HasBloodFrenzy)
        {
            finalDmg *= 1.30f;
        }

        enemy.TakeDamage(finalDmg, shouldKnockBack, isCrit);

        if (isCrit)
        {
            VFXImpactManager.PlayShockwaveRing(hitPos, new Color(1f, 0.85f, 0.2f), 2.2f, 0.25f);
        }

        // Hiệu ứng tia lửa va chạm (Hit Sparks)
        SpawnHitSparks(hitPos, isCrit);
    }

    private static void SpawnHitSparks(Vector3 pos, bool isCrit)
    {
        GameObject sparksObj = new GameObject("HitSparks");
        sparksObj.transform.position = pos;

        int sparkCount = isCrit ? 6 : 3;
        Color sparkColor = isCrit ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(1f, 0.95f, 0.85f, 0.9f);

        for (int i = 0; i < sparkCount; i++)
        {
            GameObject spark = new GameObject("Spark", typeof(SpriteRenderer));
            spark.transform.SetParent(sparksObj.transform, false);
            spark.transform.localScale = Vector3.one * (isCrit ? 0.22f : 0.14f);

            SpriteRenderer sr = spark.GetComponent<SpriteRenderer>();
            sr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
            sr.color = sparkColor;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 25;

            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float speed = Random.Range(3.5f, 7.0f);

            sparksObj.AddComponent<HitSparkMover>().Init(spark, dir * speed);
        }

        Destroy(sparksObj, 0.16f);
    }
}

public class HitSparkMover : MonoBehaviour
{
    private GameObject sparkGO;
    private Vector2 velocity;

    public void Init(GameObject go, Vector2 vel)
    {
        sparkGO = go;
        velocity = vel;
    }

    void Update()
    {
        if (sparkGO != null)
        {
            sparkGO.transform.position += (Vector3)(velocity * Time.deltaTime);
            sparkGO.transform.localScale = Vector3.MoveTowards(sparkGO.transform.localScale, Vector3.zero, 1.4f * Time.deltaTime);
        }
    }
}
