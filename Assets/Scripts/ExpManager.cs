using System.Collections.Generic;
using UnityEngine;

public class ExpManager : MonoBehaviour
{
    public static ExpManager instance;

    [Header("Settings & Limits")]
    [Tooltip("Số lượng ngọc kinh nghiệm tối đa đồng thời trên toàn bản đồ")]
    public int maxActiveGems = 200;

    [Tooltip("Bán kính tìm ngọc lân cận để gộp trực tiếp khi quái chết gần nhau")]
    public float mergeProximityRadius = 2.5f;

    [Tooltip("Số lượng ngọc khởi tạo trước vào Pool")]
    public int initialPoolSize = 60;

    [Header("Prefab Reference")]
    public ExpPickup pickupPrefab;

    // Danh sách ngọc đang hiển thị trên map (Active)
    private readonly List<ExpPickup> activeGems = new List<ExpPickup>(256);

    // Object Pool ngọc nhàn rỗi (Inactive)
    private readonly List<ExpPickup> gemPool = new List<ExpPickup>(256);

    // Cache thông tin Player để tránh gọi singleton liên tục
    private Transform playerTransform;
    private PlayerController playerController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tự động tìm prefab từ ExperienceLevelController nếu chưa gán
        if (pickupPrefab == null && ExperienceLevelController.instance != null)
        {
            pickupPrefab = ExperienceLevelController.instance.pickup;
        }

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        if (pickupPrefab == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            ExpPickup gem = Instantiate(pickupPrefab, transform);
            gem.gameObject.SetActive(false);
            gemPool.Add(gem);
        }
    }

    private void CachePlayerReferences()
    {
        if (playerTransform == null && PlayerHealthController.instance != null)
        {
            playerTransform = PlayerHealthController.instance.transform;
        }

        if (playerController == null && PlayerController.instance != null)
        {
            playerController = PlayerController.instance;
        }
    }

    private void Update()
    {
        CachePlayerReferences();
        if (playerTransform == null || playerController == null) return;

        Vector3 playerPos = playerTransform.position;
        float pickupRangeSqr = playerController.pickupRange * playerController.pickupRange;

        // Vòng lặp tối ưu hóa hút ngọc (Magnet/Vacuum):
        // 1 vòng lặp tập trung duy nhất ở Manager thay vì 200 - 500 hàm Update() riêng lẻ!
        for (int i = 0; i < activeGems.Count; i++)
        {
            ExpPickup gem = activeGems[i];
            if (gem == null || gem.movingToPlayer) continue;

            // Kiểm tra khoảng cách bình phương (sqrMagnitude) không tốn căn bậc 2
            float distSqr = (gem.transform.position - playerPos).sqrMagnitude;
            if (distSqr <= pickupRangeSqr)
            {
                gem.StartMovingToPlayer(playerTransform, playerController.moveSpeed);
            }
        }
    }

    /// <summary>
    /// Sinh ngọc kinh nghiệm hoặc tự động gộp (Merge) vào ngọc có sẵn khi chạm ngưỡng giới hạn.
    /// </summary>
    public void SpawnExp(Vector3 position, int expValue)
    {
        CachePlayerReferences();

        // 1. Nếu số lượng ngọc chưa đạt giới hạn tối đa -> Sinh ngọc bình thường từ Pool
        if (activeGems.Count < maxActiveGems)
        {
            ExpPickup newGem = GetFromPool();
            if (newGem != null)
            {
                newGem.transform.position = position;
                newGem.transform.rotation = Quaternion.identity;
                newGem.gameObject.SetActive(true);
                newGem.ResetPickup(expValue);
                activeGems.Add(newGem);
                return;
            }
        }

        // 2. KHI ĐẠT GIỚI HẠN (EXP MERGING SYSTEM - CƠ CHẾ GOM NGỌC):
        // Bước A: Thử tìm ngọc gần điểm quái chết (trong bán kính mergeProximityRadius)
        ExpPickup targetGem = FindNearbyIdleGem(position, mergeProximityRadius);

        // Bước B: Nếu không có ngọc ở gần, tìm viên ngọc xa Player nhất (ngoài màn hình - Red Gem)
        if (targetGem == null)
        {
            targetGem = FindFurthestIdleGemFromPlayer();
        }

        // Bước C: Nếu tìm thấy, gộp toàn bộ điểm EXP vào viên ngọc đó
        if (targetGem != null)
        {
            targetGem.AddExp(expValue);
            return;
        }

        // Fallback khẩn cấp nếu mọi ngọc đều đang bay về phía Player: sinh tạm thêm 1 viên
        ExpPickup emergencyGem = GetFromPool();
        if (emergencyGem != null)
        {
            emergencyGem.transform.position = position;
            emergencyGem.transform.rotation = Quaternion.identity;
            emergencyGem.gameObject.SetActive(true);
            emergencyGem.ResetPickup(expValue);
            activeGems.Add(emergencyGem);
        }
    }

    /// <summary>
    /// Tìm viên ngọc nhàn rỗi gần vị trí spawn nhất.
    /// </summary>
    private ExpPickup FindNearbyIdleGem(Vector3 position, float radius)
    {
        float minSqr = radius * radius;
        ExpPickup closest = null;

        for (int i = 0; i < activeGems.Count; i++)
        {
            ExpPickup gem = activeGems[i];
            if (gem != null && !gem.movingToPlayer)
            {
                float sqrDist = (gem.transform.position - position).sqrMagnitude;
                if (sqrDist < minSqr)
                {
                    minSqr = sqrDist;
                    closest = gem;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// Tìm viên ngọc nhàn rỗi xa Player nhất (ngọc ngoài tầm nhìn/viewport - Cơ chế Red Gem của Vampire Survivors).
    /// </summary>
    private ExpPickup FindFurthestIdleGemFromPlayer()
    {
        if (playerTransform == null) return null;

        Vector3 playerPos = playerTransform.position;
        float maxSqr = -1f;
        ExpPickup furthest = null;

        for (int i = 0; i < activeGems.Count; i++)
        {
            ExpPickup gem = activeGems[i];
            if (gem != null && !gem.movingToPlayer)
            {
                float sqrDist = (gem.transform.position - playerPos).sqrMagnitude;
                if (sqrDist > maxSqr)
                {
                    maxSqr = sqrDist;
                    furthest = gem;
                }
            }
        }

        return furthest;
    }

    public ExpPickup GetFromPool()
    {
        ExpPickup output = null;

        if (gemPool.Count == 0)
        {
            if (pickupPrefab != null)
            {
                output = Instantiate(pickupPrefab, transform);
            }
        }
        else
        {
            int lastIndex = gemPool.Count - 1;
            output = gemPool[lastIndex];
            gemPool.RemoveAt(lastIndex); // Thao tác O(1)
        }

        return output;
    }

    public void ReturnToPool(ExpPickup gem)
    {
        if (gem == null) return;

        // Xóa khỏi danh sách active bằng cách swap với phần tử cuối O(1)
        int index = activeGems.IndexOf(gem);
        if (index >= 0)
        {
            int lastIndex = activeGems.Count - 1;
            activeGems[index] = activeGems[lastIndex];
            activeGems.RemoveAt(lastIndex);
        }

        gem.gameObject.SetActive(false);
        gemPool.Add(gem);
    }

    /// <summary>
    /// Kích hoạt hút toàn bộ ngọc trên map về phía người chơi (Hiệu ứng Nam châm / Vacuum toàn bản đồ).
    /// </summary>
    public void TriggerGlobalVacuum()
    {
        CachePlayerReferences();
        if (playerTransform == null || playerController == null) return;

        for (int i = 0; i < activeGems.Count; i++)
        {
            if (activeGems[i] != null && !activeGems[i].movingToPlayer)
            {
                activeGems[i].StartMovingToPlayer(playerTransform, playerController.moveSpeed * 1.5f);
            }
        }
    }

    /// <summary>
    /// Alias gọi nhanh hiệu ứng hút ngọc toàn map cho SpecialPickup / Magnet.
    /// </summary>
    public void AttractAllGems()
    {
        TriggerGlobalVacuum();
    }
}
