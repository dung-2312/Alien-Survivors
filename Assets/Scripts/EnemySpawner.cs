using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance; // Singleton so pooled enemies can return themselves. AK

    public GameObject enemyToSpawn; // The enemy prefab. AK
    public float timeToSpawn; // The rate at which the enemies spawn. AK
    private float spawnCounter; // The counter for the spawn time. AK
    public Transform minSpawn, maxSpawn; // The minimum and maximum spawn positions. AK
    private Transform target; // The target that the enemy will follow. AK
    private float despawnDistance; // The distance at which the enemy will despawn. AK
    private float despawnDistanceSqr; // The squared despawn distance, used to avoid sqrt in the per-frame check. AK
    private List<GameObject> spawnedEnemies = new List<GameObject>(); // The list of spawned enemies. AK
    public int checkPerFrame; // The number of enemies to check per frame. AK
    private int enemyToCheck; // The enemy to check. AK
    public List<WaveInfo> waves; // The list of waves. AK
    private int currentWave; // The current wave. AK
    private float waveCounter; // The counter for the wave. AK

    [Header("Boss")]
    public GameObject bossPrefab; // Needs an EnemyController and a BossController. Empty = build the Brood Queen from the first wave's enemy.
    public float bossSpawnTime = 30f; // Run time (seconds) at which the boss arrives. Low for testing; 180 for the real run.
    public bool spawnBoss = true; // Untick for no boss at all.
    public bool pauseWavesDuringBoss = true; // The boss summons its own minions, so regular waves stop while it lives.
    [HideInInspector]
    public bool bossDefeated; // Saved with the run so Continue doesn't bring a beaten boss back.
    private BossController activeBoss;

    [Header("Multi-Tier Boss Progression")]
    public List<BossEncounter> bossEncounters = new List<BossEncounter>();
    private int currentEncounterIndex = -1;
    private float endlessSpawnTimer = 1080f; // 18 minutes (post 15m dragon)
    private float endlessInterval = 180f; // Every 3 minutes
    private int endlessCycleCount = 0;

    [Header("Mini-Boss Events")]
    [Tooltip("Khoảng thời gian định kỳ triệu hồi Mini-Boss (giây), ví dụ 120s = 2 phút")]
    public float miniBossInterval = 120f;
    private float miniBossCounter;
    [Tooltip("Prefab vật phẩm đặc biệt rớt ra khi diệt Boss (Nam châm hoặc Hồi máu)")]
    public GameObject specialDropPrefab;

    [Header("Dynamic Swarm & Ambush Events")]
    public bool enableDynamicEvents = true;
    public float hordeInterval = 130f; // Mỗi ~2 phút có 1 đợt bầy quái ùa vào
    private float hordeCounter = 65f; // Đợt đầu tiên lúc 1:05
    public float circleAmbushInterval = 190f; // Mỗi ~3 phút có 1 đợt vòng vây bao bọc
    private float circleAmbushCounter = 135f; // Đợt đầu tiên lúc 2:15

    private List<GameObject> allEnemyPrefabs = new List<GameObject>();
    private readonly Dictionary<GameObject, List<GameObject>> enemyPools = new Dictionary<GameObject, List<GameObject>>(); // A pool of inactive enemies per prefab, so waves don't Instantiate/Destroy constantly. AK

    void Awake()
    {
        instance = this; // Set the instance to this. AK
    }

    void Start()
    {
        //spawnCounter = timeToSpawn; // Set the spawn counter to the spawn time. AK

        if (PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform; // Find the player and set it as the target. AK
        }

        despawnDistance = Vector3.Distance(transform.position, maxSpawn.position) + 4f; // Set the despawn distance to the distance between the spawner and the maximum spawn position plus 4. AK
        despawnDistanceSqr = despawnDistance * despawnDistance; // Cache the squared despawn distance so the per-frame check can avoid a sqrt. AK

        allEnemyPrefabs.Clear();
        if (waves != null)
        {
            foreach (var w in waves)
            {
                if (w != null && w.enemyToSpawn != null && !allEnemyPrefabs.Contains(w.enemyToSpawn))
                {
                    allEnemyPrefabs.Add(w.enemyToSpawn);
                }
            }
        }
        if (allEnemyPrefabs.Count == 0 && enemyToSpawn != null)
        {
            allEnemyPrefabs.Add(enemyToSpawn);
        }

        miniBossInterval = 65f;
        miniBossCounter = 40f;

        InitBossEncounters();

        currentWave = -1; // Set the current wave to -1. AK
        GoToNextWave(); // Go to the next wave. AK
    }

    void Update()
    {
        if (EnemyController.globalFreezeTimer > 0f)
        {
            EnemyController.globalFreezeTimer -= Time.deltaTime;
        }

        if (target == null && PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform;
        }

        if(PlayerHealthController.instance != null && PlayerHealthController.instance.gameObject.activeSelf) // If the player is active. AK
        {
            // 1. Kiểm tra Boss chính theo phân hóa từng cấp độ / từng màn
            if (spawnBoss && activeBoss == null && LevelManager.instance != null)
            {
                CheckBossProgression();
            }

            // 2. Định kỳ triệu hồi Mini-Boss
            miniBossCounter -= Time.deltaTime;
            if (miniBossCounter <= 0f)
            {
                miniBossCounter = miniBossInterval;
                SpawnMiniBossEvent();
            }

            // 3. Sự kiện Đột biến Trong Trận (Horde Rush & Circle Ambush)
            if (enableDynamicEvents)
            {
                hordeCounter -= Time.deltaTime;
                if (hordeCounter <= 0f)
                {
                    hordeCounter = hordeInterval;
                    StartCoroutine(TriggerHordeRushCo());
                }

                circleAmbushCounter -= Time.deltaTime;
                if (circleAmbushCounter <= 0f)
                {
                    circleAmbushCounter = circleAmbushInterval;
                    TriggerCircleAmbush();
                }
            }

            // 4. Wave thông thường
            bool wavesPaused = pauseWavesDuringBoss && activeBoss != null;
            if(!wavesPaused && currentWave < waves.Count) // If the current wave is less than the number of waves. AK
            {
                waveCounter -= Time.deltaTime; // Decrease the wave counter. AK
                if(waveCounter <= 0) // If the wave counter is less than or equal to 0. AK
                {
                    GoToNextWave(); // Go to the next wave. AK
                }
                spawnCounter -= Time.deltaTime; // Decrease the spawn counter. AK
                if(spawnCounter <= 0) // If the spawn counter is less than or equal to 0. AK
                {
                    spawnCounter = waves[currentWave].timeBetweenSpawns; // Reset the spawn counter. AK

                    GameObject newEnemy = SpawnEnemyWithVariant(waves[currentWave].enemyToSpawn, SelectSpawnPoint()); // Spawn quái (kèm cơ hội sinh biến thể Charger / Exploder)

                    spawnedEnemies.Add(newEnemy); // Add the new enemy to the list of spawned enemies. AK
                }
            }
        }

        if (target != null)
        {
            transform.position = target.position; // Move the spawner to the player's position. AK
        }

        int checkTarget = enemyToCheck + checkPerFrame; // Set the check target to the enemy to check plus the check per frame. AK

        while(enemyToCheck < checkTarget) // While the enemy to check is less than the check target. AK
        {
            if(enemyToCheck < spawnedEnemies.Count) // If the enemy to check is less than the number of spawned enemies. AK
            {
                if(spawnedEnemies[enemyToCheck] != null) // If the enemy to check is not null. AK
                {
                    if((transform.position - spawnedEnemies[enemyToCheck].transform.position).sqrMagnitude > despawnDistanceSqr) // If the distance between the spawner and the enemy is greater than the despawn distance. AK
                    {
                        spawnedEnemies[enemyToCheck].GetComponent<EnemyController>().DespawnToPool(); // Return the enemy to the pool instead of destroying it. AK
                        spawnedEnemies.RemoveAt(enemyToCheck); // Remove the enemy from the list of spawned enemies. AK
                        checkTarget--; // Decrease the check target. AK
                    }
                    else // If the distance between the spawner and the enemy is less than or equal to the despawn distance. AK
                    {
                        enemyToCheck++; // Increase the enemy to check. AK
                    }
                }
                else // If the enemy to check is null. AK
                {
                    spawnedEnemies.RemoveAt(enemyToCheck); // Remove the enemy from the list of spawned enemies. AK
                    checkTarget--; // Decrease the check target. AK
                }
            }
            else // If the enemy to check is greater than or equal to the number of spawned enemies. AK
            {
                enemyToCheck = 0; // Set the enemy to check to 0. AK
                checkTarget = 0; // Set the check target to 0. AK
            }
        }
    }

    private void InitBossEncounters()
    {
        if (bossEncounters == null || bossEncounters.Count == 0)
        {
            bossEncounters = new List<BossEncounter>();
            // If bossSpawnTime has a custom low value (e.g. 30s for testing), respect it for Tier 1
            float t1 = (bossSpawnTime > 0f && bossSpawnTime < 60f) ? bossSpawnTime : 60f;

            bossEncounters.Add(new BossEncounter(BossType.BroodQueen, "STAGE I: THE TOXIC MEADOW", "THE BROOD QUEEN", t1, 0, 0));
            bossEncounters.Add(new BossEncounter(BossType.FrostfangFenrir, "STAGE II: THE FROSTBITE TUNDRA", "FROSTFANG FENRIR", 150f, 1, 1));
            bossEncounters.Add(new BossEncounter(BossType.SkeletonWarlord, "STAGE III: THE CURSED CATACOMBS", "WARLORD MALAKOR", 255f, 3, 3));
            bossEncounters.Add(new BossEncounter(BossType.DeathPhantom, "STAGE IV: THE VOID NETHERWORLD", "THANATOS", 375f, 4, 4));
            bossEncounters.Add(new BossEncounter(BossType.GoldenDragon, "STAGE V: THE INFERNAL CATACLYSM", "PYROCLAST THE DRAGON", 510f, 5, 5));
        }

        if (bossDefeated && bossEncounters != null)
        {
            foreach (var enc in bossEncounters)
            {
                enc.spawned = true;
                enc.defeated = true;
            }
        }
    }

    private void CheckBossProgression()
    {
        float timer = LevelManager.instance.timer;

        for (int i = 0; i < bossEncounters.Count; i++)
        {
            BossEncounter enc = bossEncounters[i];
            if (!enc.spawned && !enc.defeated && timer >= enc.spawnTime)
            {
                enc.spawned = true;
                currentEncounterIndex = i;
                SpawnBossEncounter(enc);
                return;
            }
        }

        // Post-15:00 Endless Mode: Ascended Chaos Overlords every 3 minutes
        if (bossEncounters.Count > 0 && bossEncounters[bossEncounters.Count - 1].defeated && timer >= endlessSpawnTimer)
        {
            endlessSpawnTimer = timer + endlessInterval;
            SpawnEndlessOverlord();
        }
    }

    private void SpawnBossEncounter(BossEncounter enc)
    {
        if (bossPrefab != null && enc.bossType == BossType.BroodQueen)
        {
            activeBoss = Instantiate(bossPrefab, SelectSpawnPoint(), Quaternion.identity).GetComponent<BossController>();
            return;
        }

        GameObject prefabToUse = null;
        GameObject minionPrefab = null;

        if (waves != null && waves.Count > 0)
        {
            int waveIdx = Mathf.Clamp(enc.enemyWavePrefabIndex, 0, waves.Count - 1);
            prefabToUse = waves[waveIdx].enemyToSpawn;

            int minionIdx = Mathf.Clamp(enc.minionPrefabIndex, 0, waves.Count - 1);
            minionPrefab = waves[minionIdx].enemyToSpawn;
        }

        if (prefabToUse == null) prefabToUse = enemyToSpawn;
        if (minionPrefab == null) minionPrefab = prefabToUse;

        if (prefabToUse != null)
        {
            Vector3 spawnPoint = SelectSpawnPoint();
            GameObject holder = new GameObject("Boss Holder");
            holder.SetActive(false);
            GameObject bossObj = Instantiate(prefabToUse, spawnPoint, Quaternion.identity, holder.transform);
            activeBoss = BossController.ConfigureBoss(bossObj, enc.bossType, minionPrefab);
            bossObj.transform.SetParent(null, true);
            Destroy(holder);

            if (CameraController.instance != null) CameraController.instance.Shake(0.45f, 0.4f);
            if (UIController.instance != null)
            {
                UIController.instance.ShowWarning($"⚠ {enc.stageTitle} ⚠\n{enc.bossTitle} HAS AWAKENED!", 4.0f);
            }
        }
    }

    private void SpawnEndlessOverlord()
    {
        endlessCycleCount++;
        BossType randomType = (BossType)Random.Range(0, 5);
        int waveIdx = 0;
        switch (randomType)
        {
            case BossType.BroodQueen: waveIdx = 0; break;
            case BossType.FrostfangFenrir: waveIdx = 1; break;
            case BossType.SkeletonWarlord: waveIdx = 3; break;
            case BossType.DeathPhantom: waveIdx = 4; break;
            case BossType.GoldenDragon: waveIdx = 5; break;
        }

        GameObject prefab = (waves != null && waves.Count > waveIdx) ? waves[waveIdx].enemyToSpawn : enemyToSpawn;
        if (prefab == null) return;

        Vector3 spawnPoint = SelectSpawnPoint();
        GameObject holder = new GameObject("Overlord Holder");
        holder.SetActive(false);
        GameObject bossObj = Instantiate(prefab, spawnPoint, Quaternion.identity, holder.transform);
        activeBoss = BossController.ConfigureBoss(bossObj, randomType, prefab);
        bossObj.transform.SetParent(null, true);
        Destroy(holder);

        EnemyController ec = bossObj.GetComponent<EnemyController>();
        if (ec != null)
        {
            float hpMult = 1f + (endlessCycleCount * 0.35f);
            ec.health *= hpMult;
            ec.damage *= (1f + endlessCycleCount * 0.2f);
        }

        if (activeBoss != null)
        {
            activeBoss.bossName = "ASCENDED " + activeBoss.bossName;
            activeBoss.bossSubtitle = $"✦ CHAOS OVERLORD (TIER {endlessCycleCount}) ✦";
            activeBoss.chestTier = 3;
            activeBoss.bonusCoins += 40 * endlessCycleCount;
        }

        if (CameraController.instance != null) CameraController.instance.Shake(0.5f, 0.5f);
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning($"☠ DANGER: ASCENDED OVERLORD AWAKENS (CYCLE {endlessCycleCount})! ☠", 4.0f);
        }
    }

    private void SpawnBoss() // Fallback method
    {
        if (bossEncounters != null && bossEncounters.Count > 0)
        {
            SpawnBossEncounter(bossEncounters[0]);
        }
    }

    public void OnBossDefeated(BossController boss = null)
    {
        if (currentEncounterIndex >= 0 && currentEncounterIndex < bossEncounters.Count)
        {
            bossEncounters[currentEncounterIndex].defeated = true;
            if (currentEncounterIndex == bossEncounters.Count - 1)
            {
                bossDefeated = true;
                if (LevelManager.instance != null) LevelManager.instance.TriggerVictory();
            }
            else
            {
                // Fast-track the next progression boss encounter to arrive in 45 seconds!
                int nextIdx = currentEncounterIndex + 1;
                if (nextIdx < bossEncounters.Count && LevelManager.instance != null)
                {
                    float nextArrival = LevelManager.instance.timer + 45f;
                    if (bossEncounters[nextIdx].spawnTime > nextArrival)
                    {
                        bossEncounters[nextIdx].spawnTime = nextArrival;
                    }
                }
            }
        }
        else
        {
            bossDefeated = true;
        }

        activeBoss = null;
    }

    /// <summary>
    /// Triệu hồi trực tiếp Boss theo loại (cho Demo/Báo cáo đồ án)
    /// </summary>
    public BossController SpawnBossDirectly(BossType type)
    {
        if (target == null && PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform;
        }

        if (activeBoss != null)
        {
            Destroy(activeBoss.gameObject);
            activeBoss = null;
        }

        int waveIdx = 0;
        string stageTitle = "STAGE I: THE TOXIC MEADOW";
        string bossTitle = "THE BROOD QUEEN";

        switch (type)
        {
            case BossType.BroodQueen:
                waveIdx = 0;
                stageTitle = "STAGE I: THE TOXIC MEADOW";
                bossTitle = "THE BROOD QUEEN";
                break;
            case BossType.FrostfangFenrir:
                waveIdx = 1;
                stageTitle = "STAGE II: THE FROSTBITE TUNDRA";
                bossTitle = "FROSTFANG FENRIR";
                break;
            case BossType.SkeletonWarlord:
                waveIdx = 3;
                stageTitle = "STAGE III: THE CURSED CATACOMBS";
                bossTitle = "WARLORD MALAKOR";
                break;
            case BossType.DeathPhantom:
                waveIdx = 4;
                stageTitle = "STAGE IV: THE VOID NETHERWORLD";
                bossTitle = "THANATOS THE DEATH PHANTOM";
                break;
            case BossType.GoldenDragon:
                waveIdx = 5;
                stageTitle = "STAGE V: THE INFERNAL CATACLYSM";
                bossTitle = "PYROCLAST THE GOLDEN DRAGON";
                break;
        }

        GameObject prefab = (waves != null && waves.Count > waveIdx) ? waves[waveIdx].enemyToSpawn : enemyToSpawn;
        if (prefab == null) prefab = enemyToSpawn;
        if (prefab == null) return null;

        Vector3 spawnPoint = SelectSpawnPoint();
        GameObject holder = new GameObject("Boss Holder");
        holder.SetActive(false);
        GameObject bossObj = Instantiate(prefab, spawnPoint, Quaternion.identity, holder.transform);
        activeBoss = BossController.ConfigureBoss(bossObj, type, prefab);
        bossObj.transform.SetParent(null, true);
        Destroy(holder);

        if (CameraController.instance != null) CameraController.instance.Shake(0.5f, 0.45f);
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning($"⚠ {stageTitle} ⚠\n{bossTitle} HAS AWAKENED!", 3.5f);
        }

        return activeBoss;
    }

    /// <summary>
    /// Tiêu diệt toàn bộ quái vật thường đang có trên màn hình (Demo Nuke)
    /// </summary>
    public void KillAllEnemies(bool dropLoot = true)
    {
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>();
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy && enemy != activeBoss)
            {
                if (dropLoot)
                {
                    enemy.TakeDamage(99999f, true);
                }
                else
                {
                    enemy.gameObject.SetActive(false);
                }
            }
        }
    }

    public void TriggerHordeRush()
    {
        StartCoroutine(TriggerHordeRushCo());
    }
    public GameObject SpawnMinion(GameObject prefab, Vector3 position) // Lets a boss summon regular, pooled enemies.
    {
        GameObject minion = SpawnEnemy(prefab, position);
        spawnedEnemies.Add(minion);
        return minion;
    }

    /// <summary>
    /// Triệu hồi sự kiện Mini-Boss kèm theo hiệu ứng cảnh báo và rung màn hình.
    /// </summary>
    public void SpawnMiniBossEvent()
    {
        if (target == null && PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform;
        }

        GameObject prefabToUse = (waves != null && waves.Count > 0 && currentWave >= 0 && currentWave < waves.Count)
            ? waves[currentWave].enemyToSpawn
            : enemyToSpawn;

        if (prefabToUse == null) return;

        Vector3 spawnPoint = SelectSpawnPoint();
        GameObject bossObj = SpawnEnemy(prefabToUse, spawnPoint);
        spawnedEnemies.Add(bossObj);

        EnemyController ec = bossObj.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.PromoteToMiniBoss(5f, 1.7f, specialDropPrefab);
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.35f, 0.35f);
        }

        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("WARNING: ELITE ALIEN DETECTED!", 3f);
        }
    }

    /// <summary>
    /// Sự kiện Đột Biến: Bầy quái vật ùa vào dồn dập (Horde Swarm Rush).
    /// </summary>
    private IEnumerator TriggerHordeRushCo()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("⚠️ WARNING: HORDE SWARM INCOMING! ⚠️", 3.5f);
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.35f, 0.35f);
        }

        GameObject prefab = (waves != null && waves.Count > 0 && currentWave >= 0 && currentWave < waves.Count)
            ? waves[currentWave].enemyToSpawn
            : enemyToSpawn;

        if (prefab == null) yield break;

        // Sinh 35 quái vật liên tục trong 10 giây
        int totalEnemies = 35;
        for (int i = 0; i < totalEnemies; i++)
        {
            Vector3 spawnPos = SelectSpawnPoint();
            GameObject enemyObj = SpawnEnemy(prefab, spawnPos);
            spawnedEnemies.Add(enemyObj);

            EnemyController ec = enemyObj.GetComponent<EnemyController>();
            if (ec != null && Random.value < 0.25f)
            {
                ec.PromoteToSpecial(EnemySpecialType.Charger);
            }

            yield return new WaitForSeconds(0.28f);
        }
    }

    /// <summary>
    /// Sự kiện Đột Biến: Vòng vây tử thần khép kín (Circle Ambush).
    /// </summary>
    public void TriggerCircleAmbush()
    {
        if (target == null && PlayerHealthController.instance != null)
        {
            target = PlayerHealthController.instance.transform;
        }
        if (target == null) return;

        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("💀 DANGER: AMBUSH SURROUNDED! 💀", 3.5f);
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.45f, 0.4f);
        }

        GameObject prefab = (waves != null && waves.Count > 0 && currentWave >= 0 && currentWave < waves.Count)
            ? waves[currentWave].enemyToSpawn
            : enemyToSpawn;

        if (prefab == null) return;

        // Sinh 28 quái xếp thành vòng tròn khép kín bao vây người chơi ở bán kính 9m
        int circleCount = 28;
        float radius = 9f;
        for (int i = 0; i < circleCount; i++)
        {
            float angle = (i * 360f / circleCount) * Mathf.Deg2Rad;
            Vector3 spawnPos = target.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;

            GameObject enemyObj = SpawnEnemy(prefab, spawnPos);
            spawnedEnemies.Add(enemyObj);

            EnemyController ec = enemyObj.GetComponent<EnemyController>();
            if (ec != null)
            {
                // Rải một vài Exploder để người chơi có thể bắn nổ mở đường máu thoát thân!
                if (i % 5 == 0)
                {
                    ec.PromoteToSpecial(EnemySpecialType.Exploder);
                }
            }
        }
    }

    public GameObject SpawnEnemyWithVariant(GameObject prefab, Vector3 position)
    {
        GameObject enemyObj = SpawnEnemy(prefab, position);
        EnemyController ec = enemyObj.GetComponent<EnemyController>();
        if (ec != null && currentWave >= 1) // Từ wave 1 trở đi bắt đầu xuất hiện các biến thể phong phú
        {
            float roll = Random.value;
            if (roll < 0.09f)
            {
                ec.PromoteToSpecial(EnemySpecialType.Charger);
            }
            else if (roll < 0.17f)
            {
                ec.PromoteToSpecial(EnemySpecialType.Exploder);
            }
            else if (roll < 0.25f)
            {
                ec.PromoteToSpecial(EnemySpecialType.Spitter);
            }
            else if (roll < 0.32f)
            {
                ec.PromoteToSpecial(EnemySpecialType.GhostPhaser);
            }
            else if (roll < 0.38f && currentWave >= 2)
            {
                ec.PromoteToSpecial(EnemySpecialType.Necromancer);
            }
            else if (roll < 0.44f && currentWave >= 2)
            {
                ec.PromoteToSpecial(EnemySpecialType.VampiricShield);
            }
        }
        return enemyObj;
    }

    public GameObject SpawnEnemy(GameObject prefab, Vector3 position) // Get a pooled enemy of this type, or instantiate a new one if the pool is empty. AK
    {
        List<GameObject> pool = GetPoolFor(prefab); // The pool bucket for this enemy prefab. AK
        GameObject enemyInstance; // The enemy instance to spawn. AK
        EnemyController controller; // The enemy's controller component. AK

        if(pool.Count > 0) // Reuse a pooled enemy if one is available. AK
        {
            enemyInstance = pool[pool.Count - 1]; // Take the last enemy from the pool. AK
            pool.RemoveAt(pool.Count - 1); // Remove it from the pool. AK
            enemyInstance.transform.SetPositionAndRotation(position, Quaternion.identity); // Move it to the spawn position. AK
            enemyInstance.SetActive(true); // Reactivate it. AK
            controller = enemyInstance.GetComponent<EnemyController>(); // Get its controller. AK
        }
        else // No pooled enemy available, so instantiate a new one. AK
        {
            enemyInstance = Instantiate(prefab, position, Quaternion.identity); // Instantiate the enemy at the position. AK
            controller = enemyInstance.GetComponent<EnemyController>(); // Get its controller. AK
            controller.sourcePrefab = prefab; // Remember which prefab (and therefore which pool) it belongs to. AK
        }

        controller.ResetForSpawn(); // Reset its runtime state (health, move speed, timers) for this spawn. AK
        return enemyInstance; // Return the enemy. AK
    }

    private List<GameObject> GetPoolFor(GameObject prefab) // Get (or create) the pool bucket for a given enemy prefab. AK
    {
        if(!enemyPools.TryGetValue(prefab, out List<GameObject> pool)) // If there isn't a pool for this prefab yet. AK
        {
            pool = new List<GameObject>(); // Create one. AK
            enemyPools[prefab] = pool; // Store it. AK
        }
        return pool; // Return the pool bucket. AK
    }

    public void ReturnEnemyToPool(GameObject prefab, GameObject enemyInstance) // Deactivate an enemy and add it back to its pool bucket. AK
    {
        enemyInstance.SetActive(false); // Deactivate it. AK
        GetPoolFor(prefab).Add(enemyInstance); // Add it back to the pool for reuse. AK
    }

    public void RemoveFromTracking(GameObject enemyInstance) // Stop tracking an enemy that died before the despawn check reached it. AK
    {
        spawnedEnemies.Remove(enemyInstance); // Remove it from the list of spawned enemies. AK
    }

    public Vector3 SelectSpawnPoint()
    {
        Vector3 spawnPoint = Vector3.zero; // The spawn point. AK

        bool spawnVerticalEdge = Random.Range(0f, 1f) > .5f; // Randomly select if the spawn point is on the vertical edge. AK

        if(spawnVerticalEdge) // If the spawn point is on the vertical edge. AK
        {
            spawnPoint.y = Random.Range(minSpawn.position.y, maxSpawn.position.y); // Set the y position to a random value between the minimum and maximum spawn positions. AK

            if(Random.Range(0f, 1f) > .5f) // If the random value is greater than .5. AK
            {
                spawnPoint.x = maxSpawn.position.x; // Set the x position to the maximum spawn position. AK
            }
            else // If the random value is less than or equal to .5. AK
            {
                spawnPoint.x = minSpawn.position.x; // Set the x position to the minimum spawn position. AK
            }
        }
        else
        {
            spawnPoint.x = Random.Range(minSpawn.position.x, maxSpawn.position.x); // Set the y position to a random value between the minimum and maximum spawn positions. AK

            if(Random.Range(0f, 1f) > .5f) // If the random value is greater than .5. AK
            {
                spawnPoint.y = maxSpawn.position.y; // Set the x position to the maximum spawn position. AK
            }
            else // If the random value is less than or equal to .5. AK
            {
                spawnPoint.y = minSpawn.position.y; // Set the x position to the minimum spawn position. AK
            }
        }
        return spawnPoint; // Return the spawn point. AK
    }

    public void GoToNextWave() // Go to the next wave. AK
    {
        currentWave++; // Increase the current wave. AK

        if(currentWave >= waves.Count) // If the current wave is less than the number of waves. AK
        {
            currentWave = waves.Count - 1; // Set the current wave to the last wave. AK
        }

        waveCounter = waves[currentWave].waveLength; // Set the wave counter to the wave length. AK
        spawnCounter = waves[currentWave].timeBetweenSpawns; // Set the spawn counter to the time between spawns. AK

        if (waves[currentWave].spawnMiniBossAtStart)
        {
            SpawnMiniBossEvent();
        }
    }
}

[System.Serializable] // Make the class serializable. AK
public class WaveInfo // The wave information. AK
{
    public GameObject enemyToSpawn; // The enemy to spawn. AK
    public float waveLength = 10f; // The length of the wave. AK
    public float timeBetweenSpawns = 1f; // The time between spawns. AK
    public bool spawnMiniBossAtStart = false; // Triệu hồi Mini-Boss khi bắt đầu wave này
}

[System.Serializable]
public class BossEncounter
{
    public BossType bossType;
    public string stageTitle;
    public string bossTitle;
    public float spawnTime;
    public int enemyWavePrefabIndex;
    public int minionPrefabIndex;
    [HideInInspector] public bool spawned;
    [HideInInspector] public bool defeated;

    public BossEncounter() { }

    public BossEncounter(BossType type, string stage, string boss, float time, int waveIdx, int minionIdx)
    {
        bossType = type;
        stageTitle = stage;
        bossTitle = boss;
        spawnTime = time;
        enemyWavePrefabIndex = waveIdx;
        minionPrefabIndex = minionIdx;
        spawned = false;
        defeated = false;
    }
}
