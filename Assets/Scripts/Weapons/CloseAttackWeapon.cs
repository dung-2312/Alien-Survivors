using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseAttackWeapon : Weapon
{
    public EnemyDamager damager; // The enemy damager. GK
    public LayerMask whatIsEnemy; // The layer mask for enemies.
    private readonly Collider2D[] enemyBuffer = new Collider2D[32];
    private float attackCounter, direction; // The counter for the attack and the direction. GK
    
    // Start is called before the first frame update
    void Start()
    {
        SetStats();// Set the stats. GK
    }

    // Update is called once per frame
    void Update()
    {
        if (statsUpdated == true) // If the stats are updated. GK
        {
            statsUpdated = false; // Set the stats updated to false. GK
            SetStats(); // Set the stats. GK
        }
        attackCounter -= Time.deltaTime; // Decrease the attack counter. GK
        if (attackCounter <= 0) // If the attack counter is less than or equal to 0. GK
        {
            attackCounter = stats[weaponLevel].timeBetweenAttacks; // Reset the attack counter. GK
            
            // Auto aim at the closest enemy within range
            Transform closestEnemy = null;
            float minDistanceSqr = float.MaxValue;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(whatIsEnemy.value != 0 ? whatIsEnemy : LayerMask.GetMask("Enemy", "Enemies", "Default"));
            filter.useLayerMask = true;

            int count = Physics2D.OverlapCircle(transform.position, stats[weaponLevel].range * 5f, filter, enemyBuffer);
            for (int c = 0; c < count; c++)
            {
                if (enemyBuffer[c] != null && enemyBuffer[c].CompareTag("Enemy"))
                {
                    float distSqr = (enemyBuffer[c].transform.position - transform.position).sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        closestEnemy = enemyBuffer[c].transform;
                    }
                }
            }

            // Determine base attack angle: exact angle towards closest enemy, or player movement/facing
            float baseAngle = 0f;
            if (closestEnemy != null)
            {
                Vector3 diff = closestEnemy.position - transform.position;
                baseAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
            }
            else
            {
                Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (moveInput.sqrMagnitude > 0.01f)
                {
                    baseAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                }
                else
                {
                    float face = PlayerController.instance != null ? PlayerController.instance.facingDirection : 1f;
                    baseAngle = face < 0 ? 180f : 0f;
                }
            }

            // Tấn công xung quanh nhân vật (360 độ):
            // Cấp 1 (amount = 1): chém 4 hướng (chữ thập 90°) bao quanh nhân vật
            // Khi nâng cấp: chém 6, 8, 10, 12 hướng tạo thành vòng bão kiếm quét sạch mọi phía
            int strikeCount = Mathf.Max(4, (int)stats[weaponLevel].amount * 2);
            float angleStep = 360f / strikeCount;

            for (int i = 0; i < strikeCount; i++)
            {
                float currentAngle = baseAngle + (angleStep * i);
                Quaternion rot = Quaternion.Euler(0f, 0f, currentAngle);
                Instantiate(damager, damager.transform.position, rot, transform).gameObject.SetActive(true);
            }
            SFXManager.instance.PlaySFXPitched(9); // Play the sound effect. GK

        }
    }
    void SetStats() // Function to set the stats of the weapon. GK
    {
        damager.damageAmount = stats[weaponLevel].damage; // Set the damage amount of the damager to the damage of the weapon. GK
        damager.lifeTime = stats[weaponLevel].duration; // Set the life time of the damager to the duration of the weapon. GK
        damager.transform.localScale = Vector3.one * stats[weaponLevel].range; // Set the scale of the damager to the range of the weapon. GK
        attackCounter = 0f; // Reset the attack counter. GK
        
    }
}
