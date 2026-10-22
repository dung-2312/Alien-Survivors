using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageNumberController : MonoBehaviour
{
    public static DamageNumberController instance;

    [Header("References")]
    public DamageNumber numberToSpawn;
    public Transform numberCanvas;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 40;

    [Header("Display Settings")]
    [Tooltip("Khoảng cách jitter ngẫu nhiên để các số sát thương liên tiếp không bị đè khít lên nhau")]
    [SerializeField] private float spawnJitterRadius = 0.25f;

    [Tooltip("Gộp sát thương theo mục tiêu (Mặc định tắt để người chơi thấy rõ từng phát đánh)")]
    [SerializeField] private bool aggregateDamage = false;

    private readonly List<DamageNumber> numberPool = new List<DamageNumber>();
    
    // Lưu vết DamageNumber đang hoạt động theo từng Enemy để gộp sát thương (chỉ dùng khi bật aggregateDamage)
    private readonly Dictionary<GameObject, DamageNumber> activeTargetNumbers = new Dictionary<GameObject, DamageNumber>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        if (numberToSpawn == null || numberCanvas == null) return;

        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageNumber newObj = Instantiate(numberToSpawn, numberCanvas);
            newObj.gameObject.SetActive(false);
            numberPool.Add(newObj);
        }
    }

    public void SpawnDamage(float damageAmount, Vector3 location, bool isCrit, GameObject target = null)
    {
        if (damageAmount <= 0f) return;

        // 1. Kiểm tra Damage Aggregation (chỉ kích hoạt nếu aggregateDamage = true và KHÔNG phải đòn Crit)
        if (!isCrit && aggregateDamage && target != null && target.activeInHierarchy && activeTargetNumbers.TryGetValue(target, out DamageNumber activeNum))
        {
            if (activeNum != null && activeNum.CanAggregate)
            {
                activeNum.AddDamage(damageAmount);
                return;
            }
            else
            {
                activeTargetNumbers.Remove(target);
            }
        }

        // 2. Lấy số hiển thị từ Object Pool
        DamageNumber newDamage = GetFromPool();
        if (newDamage == null) return;

        // Thêm độ lệch jitter ngẫu nhiên nhẹ để mỗi hit đều có số bay lên tách biệt, rõ ràng
        Vector3 jitter = new Vector3(
            Random.Range(-spawnJitterRadius, spawnJitterRadius),
            Random.Range(-spawnJitterRadius * 0.5f, spawnJitterRadius),
            0f
        );

        newDamage.gameObject.SetActive(true);
        newDamage.Setup(damageAmount, location + jitter, isCrit, target);

        // 3. Đăng ký vào từ điển nếu bật gộp sát thương
        if (!isCrit && aggregateDamage && target != null)
        {
            activeTargetNumbers[target] = newDamage;
        }
    }

    // Overload nhận target GameObject
    public void SpawnDamage(float damageAmount, Vector3 location, GameObject target = null)
    {
        SpawnDamage(damageAmount, location, false, target);
    }

    // Hàm gốc tương thích với code cũ
    public void SpawnDamage(float damageAmount, Vector3 location)
    {
        SpawnDamage(damageAmount, location, null);
    }

    public DamageNumber GetFromPool()
    {
        DamageNumber numberToOutput = null;

        if (numberPool.Count == 0)
        {
            if (numberToSpawn != null && numberCanvas != null)
            {
                numberToOutput = Instantiate(numberToSpawn, numberCanvas);
            }
        }
        else
        {
            int lastIndex = numberPool.Count - 1;
            numberToOutput = numberPool[lastIndex];
            numberPool.RemoveAt(lastIndex); // Thao tác O(1) không dời mảng
        }

        return numberToOutput;
    }

    public void PlaceInPool(DamageNumber numberToPlace)
    {
        if (numberToPlace == null) return;

        // Dọn dẹp liên kết khỏi danh sách gộp sát thương khi chữ mờ hết hoặc biến mất
        if (numberToPlace.Target != null)
        {
            if (activeTargetNumbers.TryGetValue(numberToPlace.Target, out DamageNumber current) && current == numberToPlace)
            {
                activeTargetNumbers.Remove(numberToPlace.Target);
            }
        }

        numberToPlace.gameObject.SetActive(false);
        numberPool.Add(numberToPlace);
    }
}