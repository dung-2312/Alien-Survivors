using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : Weapon
{
    [Header("Thành phần vũ khí")]
    public EnemyDamager damager;
    public Projectile projectile;

    [Header("Thông số tầm đánh & Layer mục tiêu")]
    public float weaponRange = 5f;
    public LayerMask whatIsEnemy;

    [Header("Tối ưu hóa hiệu năng (Performance Optimization)")]
    [Tooltip("Khoảng cách thời gian giữa các lần quét mục tiêu mới (giây)")]
    [SerializeField] private float targetScanInterval = 0.15f;

    [Tooltip("Kích thước buffer khởi tạo trước cho mảng Collider (tránh GC Alloc)")]
    [SerializeField] private int initialBufferSize = 32;

    [Tooltip("Giới hạn kích thước buffer tối đa khi tự động mở rộng")]
    [SerializeField] private int maxBufferSize = 256;

    // 1. Buffer cấp phát trước để triệt tiêu hoàn toàn rác bộ nhớ (0 GC Alloc)
    private Collider2D[] enemyBuffer;
    private ContactFilter2D contactFilter;

    // 2. Cache mục tiêu hiện tại và bộ đếm thời gian
    private Transform currentTarget;
    private float scanTimer;
    private float shotCounter;

    void Awake()
    {
        // Khởi tạo trước mảng Collider2D trong Awake để không cấp phát trong Update
        enemyBuffer = new Collider2D[initialBufferSize];

        // Khởi tạo ContactFilter2D chuẩn Unity 6
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
        // Tính trước bình phương tầm đánh để so sánh sqrMagnitude
        float currentRangeSqr = currentRange * currentRange;

        // Giảm các biến đếm thời gian
        scanTimer -= Time.deltaTime;
        shotCounter -= Time.deltaTime;

        // 4. KIỂM TRA MỤC TIÊU CŨ (Target Caching):
        // Nếu mục tiêu cũ vẫn hợp lệ (còn sống, active, trong tầm), ta tái sử dụng ngay mà không cần quét lại.
        if (!IsTargetValid(currentTarget, currentRangeSqr))
        {
            currentTarget = null;

            // 3. NHỊP QUÉT ĐỊNH KỲ (Tick Rate / Scan Interval):
            // Chỉ quét tìm mục tiêu mới khi đã hết thời gian scanTimer (thay vì quét mỗi frame)
            if (scanTimer <= 0f)
            {
                scanTimer = targetScanInterval;
                currentTarget = FindClosestTarget(currentRange, currentRangeSqr);
            }
        }
        else if (scanTimer <= 0f)
        {
            // Mục tiêu cũ vẫn hợp lệ, chỉ cần reset timer nhịp quét
            scanTimer = targetScanInterval;
        }

        // Bắn đạn khi đã hồi chiêu xong
        if (shotCounter <= 0f)
        {
            shotCounter = stats[weaponLevel].timeBetweenAttacks;

            // Nếu tại thời điểm bắn chưa có mục tiêu, thử tìm ngay một lần
            if (currentTarget == null)
            {
                currentTarget = FindClosestTarget(currentRange, currentRangeSqr);
            }

            if (currentTarget != null)
            {
                FireProjectiles(currentTarget);
            }
        }
    }

    /// <summary>
    /// Kiểm tra mục tiêu cũ còn hợp lệ không:
    /// - Không null
    /// - GameObject đang active trong hierarchy
    /// - Quái còn máu (chưa chết)
    /// - Vẫn nằm trong bán kính tầm đánh (dùng sqrMagnitude)
    /// </summary>
    private bool IsTargetValid(Transform target, float maxRangeSqr)
    {
        if (target == null) return false;
        if (!target.gameObject.activeInHierarchy) return false;

        // Kiểm tra máu quái vật
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null && enemy.health <= 0) return false;

        // 2. So sánh khoảng cách bằng sqrMagnitude để tránh phép căn bậc hai (Mathf.Sqrt)
        float distSqr = (target.position - transform.position).sqrMagnitude;
        return distSqr <= maxRangeSqr;
    }

    /// <summary>
    /// Tìm quái vật gần nhất bằng Physics NonAlloc và sqrMagnitude
    /// </summary>
    private Transform FindClosestTarget(float range, float rangeSqr)
    {
        contactFilter.SetLayerMask(whatIsEnemy);

        // 1. Quét quái vào buffer đã cấp phát trước (Unity 6 OverlapCircle không sinh GC Alloc)
        int hitCount = Physics2D.OverlapCircle(transform.position, range, contactFilter, enemyBuffer);

        // XỬ LÝ KHI SỐ LƯỢNG QUÁI VƯỢT QUÁ KÍCH THƯỚC BUFFER:
        // Nếu hitCount chạm ngưỡng tối đa của buffer hiện tại, tức là có thể còn quái ngoài rìa chưa đọc hết.
        // Ta nhân đôi buffer (nếu chưa vượt quá maxBufferSize) và quét lại để không bỏ sót mục tiêu.
        if (hitCount >= enemyBuffer.Length && enemyBuffer.Length < maxBufferSize)
        {
            int newSize = Mathf.Min(enemyBuffer.Length * 2, maxBufferSize);
            System.Array.Resize(ref enemyBuffer, newSize);
            hitCount = Physics2D.OverlapCircle(transform.position, range, contactFilter, enemyBuffer);
        }

        if (hitCount <= 0) return null;

        // 2. Tìm quái gần nhất trong số các quái quét được bằng sqrMagnitude
        Transform closest = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = enemyBuffer[i];
            if (col == null || !col.gameObject.activeInHierarchy) continue;

            EnemyController enemy = col.GetComponent<EnemyController>();
            if (enemy != null && enemy.health <= 0) continue;

            // So sánh khoảng cách bình phương (sqrMagnitude)
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
    /// Bắn đạn về phía mục tiêu đã được khóa
    /// </summary>
    private void FireProjectiles(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        for (int i = 0; i < stats[weaponLevel].amount; i++)
        {
            // Xòe góc nhẹ nếu có nhiều tia đạn
            float spread = (i - (stats[weaponLevel].amount - 1) / 2f) * 8f;
            Quaternion rot = Quaternion.AngleAxis(baseAngle + spread, Vector3.forward);
            Instantiate(projectile, transform.position, rot).gameObject.SetActive(true);
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
