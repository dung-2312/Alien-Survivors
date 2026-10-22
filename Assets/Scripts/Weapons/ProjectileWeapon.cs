using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [Header("ThÃ nh pháº§n vÅ© khÃ­")]
    public EnemyDamager damager;
    public Projectile projectile;

    [Header("ThÃ´ng sá»‘ táº§m Ä‘Ã¡nh & Layer má»¥c tiÃªu")]
    public float weaponRange = 5f;
    public LayerMask whatIsEnemy;

    [Header("Tá»‘i Æ°u hÃ³a hiá»‡u nÄƒng (Performance Optimization)")]
    [Tooltip("Khoáº£ng cÃ¡ch thá»i gian giá»¯a cÃ¡c láº§n quÃ©t má»¥c tiÃªu má»›i (giÃ¢y)")]
    [SerializeField] private float targetScanInterval = 0.15f;

    [Tooltip("KÃ­ch thÆ°á»›c buffer khá»Ÿi táº¡o trÆ°á»›c cho máº£ng Collider (trÃ¡nh GC Alloc)")]
    [SerializeField] private int initialBufferSize = 32;

    [Tooltip("Giá»›i háº¡n kÃ­ch thÆ°á»›c buffer tá»‘i Ä‘a khi tá»± Ä‘á»™ng má»Ÿ rá»™ng")]
    [SerializeField] private int maxBufferSize = 256;

    // 1. Buffer cáº¥p phÃ¡t trÆ°á»›c Ä‘á»ƒ triá»‡t tiÃªu hoÃ n toÃ n rÃ¡c bá»™ nhá»› (0 GC Alloc)
    private Collider2D[] enemyBuffer;
    private ContactFilter2D contactFilter;

    // 2. Cache má»¥c tiÃªu hiá»‡n táº¡i vÃ  bá»™ Ä‘áº¿m thá»i gian
    private Transform currentTarget;
    private float scanTimer;
    private float shotCounter;

    void Awake()
    {
        // Khá»Ÿi táº¡o trÆ°á»›c máº£ng Collider2D trong Awake Ä‘á»ƒ khÃ´ng cáº¥p phÃ¡t trong Update
        enemyBuffer = new Collider2D[initialBufferSize];

        // Khá»Ÿi táº¡o ContactFilter2D chuáº©n Unity 6
        contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(whatIsEnemy);
        contactFilter.useLayerMask = true;
    }

    void Start()
    {
        SetStats();
    }

    void Update()
    {
        if (statsUpdated)
        {
            statsUpdated = false;
            SetStats();
        }

        float currentRange = weaponRange * stats[weaponLevel].range;
        // TÃ­nh trÆ°á»›c bÃ¬nh phÆ°Æ¡ng táº§m Ä‘Ã¡nh Ä‘á»ƒ so sÃ¡nh sqrMagnitude
        float currentRangeSqr = currentRange * currentRange;

        // Giáº£m cÃ¡c biáº¿n Ä‘áº¿m thá»i gian
        scanTimer -= Time.deltaTime;
        shotCounter -= Time.deltaTime;

        // 4. KIá»‚M TRA Má»¤C TIÃŠU CÅ¨ (Target Caching):
        // Náº¿u má»¥c tiÃªu cÅ© váº«n há»£p lá»‡ (cÃ²n sá»‘ng, active, trong táº§m), ta tÃ¡i sá»­ dá»¥ng ngay mÃ  khÃ´ng cáº§n quÃ©t láº¡i.
        if (!IsTargetValid(currentTarget, currentRangeSqr))
        {
            currentTarget = null;

            // 3. NHá»ŠP QUÃ‰T Äá»ŠNH Ká»² (Tick Rate / Scan Interval):
            // Chá»‰ quÃ©t tÃ¬m má»¥c tiÃªu má»›i khi Ä‘Ã£ háº¿t thá»i gian scanTimer (thay vÃ¬ quÃ©t má»—i frame)
            if (scanTimer <= 0f)
            {
                scanTimer = targetScanInterval;
                currentTarget = FindClosestTarget(currentRange, currentRangeSqr);
            }
        }
        else if (scanTimer <= 0f)
        {
            // Má»¥c tiÃªu cÅ© váº«n há»£p lá»‡, chá»‰ cáº§n reset timer nhá»‹p quÃ©t
            scanTimer = targetScanInterval;
        }

        // Báº¯n Ä‘áº¡n khi Ä‘Ã£ há»“i chiÃªu xong
        if (shotCounter <= 0f)
        {
            shotCounter = stats[weaponLevel].timeBetweenAttacks;

            // Náº¿u táº¡i thá»i Ä‘iá»ƒm báº¯n chÆ°a cÃ³ má»¥c tiÃªu, thá»­ tÃ¬m ngay má»™t láº§n
            if (currentTarget == null)
            {
                currentTarget = FindClosestTarget(currentRange, currentRangeSqr);
            }

            if (currentTarget != null)
            {
                FireProjectiles(currentTarget);
            }
            else
            {
                FireForward();
            }
        }
    }

    /// <summary>
    /// Kiá»ƒm tra má»¥c tiÃªu cÅ© cÃ²n há»£p lá»‡ khÃ´ng:
    /// - KhÃ´ng null
    /// - GameObject Ä‘ang active trong hierarchy
    /// - QuÃ¡i cÃ²n mÃ¡u (chÆ°a cháº¿t)
    /// - Váº«n náº±m trong bÃ¡n kÃ­nh táº§m Ä‘Ã¡nh (dÃ¹ng sqrMagnitude)
    /// </summary>
    private bool IsTargetValid(Transform target, float maxRangeSqr)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        // Kiá»ƒm tra mÃ¡u quÃ¡i váº­t
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null && enemy.health <= 0) return false;

        // 2. So sÃ¡nh khoáº£ng cÃ¡ch báº±ng sqrMagnitude Ä‘á»ƒ trÃ¡nh phÃ©p cÄƒn báº­c hai (Mathf.Sqrt)
        float distSqr = (target.position - transform.position).sqrMagnitude;
        return distSqr <= maxRangeSqr;
    }

    /// <summary>
    /// TÃ¬m quÃ¡i váº­t gáº§n nháº¥t báº±ng Physics NonAlloc vÃ  sqrMagnitude
    /// </summary>
    private Transform FindClosestTarget(float range, float rangeSqr)
    {
        contactFilter.SetLayerMask(whatIsEnemy);

        // 1. QuÃ©t quÃ¡i vÃ o buffer Ä‘Ã£ cáº¥p phÃ¡t trÆ°á»›c (Unity 6 OverlapCircle khÃ´ng sinh GC Alloc)
        int hitCount = Physics2D.OverlapCircle(transform.position, range, contactFilter, enemyBuffer);

        // Xá»¬ LÃ KHI Sá» LÆ¯á»¢NG QUÃI VÆ¯á»¢T QUÃ KÃCH THÆ¯á»šC BUFFER:
        // Náº¿u hitCount cháº¡m ngÆ°á»¡ng tá»‘i Ä‘a cá»§a buffer hiá»‡n táº¡i, tá»©c lÃ  cÃ³ thá»ƒ cÃ²n quÃ¡i ngoÃ i rÃ¬a chÆ°a Ä‘á»c háº¿t.
        // Ta nhÃ¢n Ä‘Ã´i buffer (náº¿u chÆ°a vÆ°á»£t quÃ¡ maxBufferSize) vÃ  quÃ©t láº¡i Ä‘á»ƒ khÃ´ng bá» sÃ³t má»¥c tiÃªu.
        if (hitCount >= enemyBuffer.Length && enemyBuffer.Length < maxBufferSize)
        {
            int newSize = Mathf.Min(enemyBuffer.Length * 2, maxBufferSize);
            System.Array.Resize(ref enemyBuffer, newSize);
            hitCount = Physics2D.OverlapCircle(transform.position, range, contactFilter, enemyBuffer);
        }

        if (hitCount <= 0) return null;

        // 2. TÃ¬m quÃ¡i gáº§n nháº¥t trong sá»‘ cÃ¡c quÃ¡i quÃ©t Ä‘Æ°á»£c báº±ng sqrMagnitude
        Transform closest = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = enemyBuffer[i];
            if (col == null || !col.gameObject.activeInHierarchy) continue;

            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null && enemy.health <= 0) continue;

            // So sÃ¡nh khoáº£ng cÃ¡ch bÃ¬nh phÆ°Æ¡ng (sqrMagnitude)
            float distSqr = (col.transform.position - currentPos).sqrMagnitude;
            if (distSqr < minDistanceSqr && distSqr <= rangeSqr)
            {
                minDistanceSqr = distSqr;
                closest = col.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// Báº¯n Ä‘áº¡n vá» phÃ­a má»¥c tiÃªu Ä‘Ã£ Ä‘Æ°á»£c khÃ³a
    /// </summary>
    private void FireProjectiles(Transform target)
    {
        Vector3 fireOrigin = transform.position;
        if (PlayerController.instance != null)
        {
            float facing = PlayerController.instance.facingDirection;
            fireOrigin = PlayerController.instance.transform.position + Vector3.up * 0.25f + Vector3.right * (facing * 0.35f);
            PlayerController.instance.TriggerAttackSwing(facing);
        }

        Vector3 direction = target.position - fireOrigin;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        for (int i = 0; i < stats[weaponLevel].amount; i++)
        {
            float spread = (i - (stats[weaponLevel].amount - 1) / 2f) * 8f;
            Quaternion rot = Quaternion.AngleAxis(baseAngle + spread, Vector3.forward);
            Instantiate(projectile, fireOrigin, rot).gameObject.SetActive(true);
        }

        SFXManager.instance.PlaySFXPitched(6);
    }

    private void FireForward()
    {
        Vector3 fireOrigin = transform.position;
        float facing = 1f;
        if (PlayerController.instance != null)
        {
            facing = PlayerController.instance.facingDirection;
            fireOrigin = PlayerController.instance.transform.position + Vector3.up * 0.25f + Vector3.right * (facing * 0.35f);
            PlayerController.instance.TriggerAttackSwing(facing);
        }

        float baseAngle = (facing < 0f) ? 90f : -90f;

        for (int i = 0; i < stats[weaponLevel].amount; i++)
        {
            float spread = (i - (stats[weaponLevel].amount - 1) / 2f) * 8f;
            Quaternion rot = Quaternion.AngleAxis(baseAngle + spread, Vector3.forward);
            Instantiate(projectile, fireOrigin, rot).gameObject.SetActive(true);
        }

        SFXManager.instance.PlaySFXPitched(6);
    }

    void SetStats()
    {
        damager.damageAmount = stats[weaponLevel].damage;
        damager.lifeTime = stats[weaponLevel].duration;
        damager.transform.localScale = Vector3.one * stats[weaponLevel].range;
        shotCounter = 0f;
        projectile.moveSpeed = stats[weaponLevel].speed;
    }
}
