using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUltimateController : MonoBehaviour
{
    public static PlayerUltimateController instance;

    [Header("Rage / Energy Stats")]
    public float currentRage = 0f;
    public float maxRage = 100f;
    public float ragePerKill = 3.5f;
    public float passiveRageRegen = 1.5f;

    [Header("Ultimate State")]
    public bool isCasting = false;
    private PlayerController player;

    public float RagePercent => Mathf.Clamp01(currentRage / maxRage);
    public bool IsUltimateReady => currentRage >= maxRage && !isCasting;

    public string GetUltimateName()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        switch (selectedIndex)
        {
            case 0: return "AXE TEMPEST";
            case 1: return "EXCALIBUR STORM";
            case 2: return "SHADOW DANCE";
            case 3: return "METEOR CALAMITY";
            case 4: return "DIVINE SANCTUARY";
            case 5: return "SPIRIT BARRAGE";
            default: return "EXCALIBUR STORM";
        }
    }

    void Awake()
    {
        instance = this;
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        // Tích nộ bị động chậm theo thời gian
        if (currentRage < maxRage && !isCasting)
        {
            currentRage = Mathf.Min(maxRage, currentRage + passiveRageRegen * Time.deltaTime);
        }

        // Kích hoạt chiêu cuối khi nhấn E hoặc Chuột Phải
        if (IsUltimateReady && (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1)))
        {
            ActivateUltimate();
        }
    }

    /// <summary>
    /// Thêm điểm nộ khi hạ gục quái vật hoặc đánh trúng boss
    /// </summary>
    public void AddRage(float amount)
    {
        if (isCasting) return;
        currentRage = Mathf.Clamp(currentRage + amount, 0f, maxRage);
    }

    /// <summary>
    /// Kích hoạt Chiêu Cuối theo đặc trưng từng nhân vật
    /// </summary>
    public void ActivateUltimate()
    {
        if (!IsUltimateReady) return;

        currentRage = 0f;
        isCasting = true;

        // Bất tử trong thời gian vận chiêu
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.TriggerInvincibility(1.6f);
        }

        // Rung chấn màn hình
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.35f, 0.45f);
        }

        // Âm thanh uy lực
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        // Xác định chiêu cuối theo tên tướng
        string charName = "Antonio";
        if (player != null)
        {
            string objName = player.gameObject.name.ToLowerInvariant();
            if (objName.Contains("knight")) charName = "Hero Knight";
            else if (objName.Contains("wizard")) charName = "Evil Wizard";
            else if (objName.Contains("adventurer")) charName = "Adventurer";
            else if (objName.Contains("bandit")) charName = "Light Bandit";
            else if (objName.Contains("forest") || objName.Contains("archer")) charName = "Red Archer";
            else charName = "Antonio";
        }

        // Lấy tên từ CharacterData nếu có
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        switch (selectedIndex)
        {
            case 0: charName = "Antonio"; break;
            case 1: charName = "Hero Knight"; break;
            case 2: charName = "Light Bandit"; break;
            case 3: charName = "Evil Wizard"; break;
            case 4: charName = "Adventurer"; break;
            case 5: charName = "Red Archer"; break;
        }

        StartCoroutine(ExecuteHeroUltimate(charName));
    }

    private IEnumerator ExecuteHeroUltimate(string charName)
    {
        string ultTitle = "";

        switch (charName)
        {
            case "Hero Knight":
                ultTitle = "[ EXCALIBUR BLADE STORM ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateHeroKnight());
                break;

            case "Evil Wizard":
                ultTitle = "[ METEOR CALAMITY ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateEvilWizard());
                break;

            case "Antonio":
                ultTitle = "[ AXE TEMPEST ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateAntonio());
                break;

            case "Light Bandit":
                ultTitle = "[ SHADOW BLADE DANCE ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateLightBandit());
                break;

            case "Adventurer":
                ultTitle = "[ DIVINE SANCTUARY ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateAdventurer());
                break;

            case "Red Archer":
                ultTitle = "[ SPIRIT ARROW BARRAGE ]";
                if (UIController.instance != null) UIController.instance.ShowWarning(ultTitle, 1.8f);
                yield return StartCoroutine(UltimateRedArcher());
                break;

            default:
                yield return StartCoroutine(UltimateHeroKnight());
                break;
        }

        isCasting = false;
    }

    // ==========================================
    // 1. HERO KNIGHT: EXCALIBUR BLADE STORM
    // ==========================================
    private IEnumerator UltimateHeroKnight()
    {
        int waveCount = 3;
        for (int w = 0; w < waveCount; w++)
        {
            int bladeCount = 10;
            float angleStep = 360f / bladeCount;
            for (int i = 0; i < bladeCount; i++)
            {
                float angle = (i * angleStep) + (w * 18f);
                Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
                SpawnEnergyBlade(transform.position, dir, 120f, new Color(1f, 0.85f, 0.2f, 1f));
            }
            if (CameraController.instance != null) CameraController.instance.Shake(0.2f, 0.3f);
            yield return new WaitForSeconds(0.25f);
        }
    }

    private void SpawnEnergyBlade(Vector3 origin, Vector3 direction, float damage, Color color)
    {
        GameObject blade = new GameObject("EnergyBlade", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager));
        blade.transform.position = origin;
        blade.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        blade.transform.localScale = new Vector3(2.2f, 0.9f, 1f);

        SpriteRenderer sr = blade.GetComponent<SpriteRenderer>();
        if (player != null && player.unassignedWeapons != null && player.unassignedWeapons.Count > 0)
        {
            sr.sprite = player.unassignedWeapons[0].icon;
        }
        sr.color = color;
        sr.sortingOrder = 15;

        CircleCollider2D col = blade.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.8f;

        EnemyDamager ed = blade.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.85f;

        Rigidbody2D rb = blade.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = direction * 12f;

        Destroy(blade, 0.85f);
    }

    // ==========================================
    // 2. EVIL WIZARD: METEOR CALAMITY
    // ==========================================
    private IEnumerator UltimateEvilWizard()
    {
        int meteorCount = 14;
        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * Random.Range(2f, 9f);
            Vector3 impactPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            StartCoroutine(SpawnMeteorStrike(impactPos));
            yield return new WaitForSeconds(0.12f);
        }
    }

    private IEnumerator SpawnMeteorStrike(Vector3 impactPos)
    {
        // Vệt cảnh báo đỏ
        GameObject marker = new GameObject("MeteorMarker", typeof(SpriteRenderer));
        marker.transform.position = impactPos;
        marker.transform.localScale = Vector3.one * 2.5f;
        SpriteRenderer msr = marker.GetComponent<SpriteRenderer>();
        msr.color = new Color(1f, 0.2f, 0.1f, 0.35f);
        msr.sortingOrder = 5;

        yield return new WaitForSeconds(0.35f);
        Destroy(marker);

        // Vụ nổ thiên thạch hủy diệt
        GameObject meteorExplosion = new GameObject("MeteorBlast", typeof(CircleCollider2D), typeof(EnemyDamager));
        meteorExplosion.transform.position = impactPos;
        CircleCollider2D col = meteorExplosion.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 2.4f;

        EnemyDamager ed = meteorExplosion.GetComponent<EnemyDamager>();
        ed.damageAmount = 140f;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.5f;

        if (CameraController.instance != null) CameraController.instance.Shake(0.18f, 0.25f);
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);

        // Hiệu ứng hạt bụi lửa lan rộng
        SpawnNovaRing(impactPos, 2.5f, new Color(1f, 0.45f, 0.1f, 0.9f));

        Destroy(meteorExplosion, 0.5f);
    }

    // ==========================================
    // 3. ANTONIO: AXE TEMPEST
    // ==========================================
    private IEnumerator UltimateAntonio()
    {
        int axeCount = 18;
        float angleStep = 360f / axeCount;
        for (int i = 0; i < axeCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.up;

            GameObject axe = new GameObject("TempestAxe", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(EnemyDamager), typeof(Rigidbody2D));
            axe.transform.position = transform.position;
            axe.transform.localScale = Vector3.one * 1.6f;

            SpriteRenderer sr = axe.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.7f, 0.3f, 1f);
            sr.sortingOrder = 15;

            CircleCollider2D col = axe.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.6f;

            EnemyDamager ed = axe.GetComponent<EnemyDamager>();
            ed.damageAmount = 95f;
            ed.destroyOnImpact = false;
            ed.lifeTime = 1.6f;

            Rigidbody2D rb = axe.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = dir * 9f;

            StartCoroutine(SpinAndDestroy(axe, 720f, 1.6f));
        }

        if (CameraController.instance != null) CameraController.instance.Shake(0.3f, 0.4f);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator SpinAndDestroy(GameObject obj, float rotSpeed, float lifetime)
    {
        float t = 0f;
        while (t < lifetime && obj != null)
        {
            t += Time.deltaTime;
            obj.transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    // ==========================================
    // 4. LIGHT BANDIT: SHADOW BLADE DANCE
    // ==========================================
    private IEnumerator UltimateLightBandit()
    {
        int strikeCount = 16;
        for (int i = 0; i < strikeCount; i++)
        {
            // Tìm quái gần nhất trong bán kính 10m
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 10f);
            EnemyController targetEnemy = null;
            float nearestDist = float.MaxValue;
            foreach (var h in hits)
            {
                if (h != null && h.TryGetComponent<EnemyController>(out var ec))
                {
                    float d = Vector2.Distance(transform.position, ec.transform.position);
                    if (d < nearestDist && ec.gameObject.activeInHierarchy)
                    {
                        nearestDist = d;
                        targetEnemy = ec;
                    }
                }
            }

            if (targetEnemy != null)
            {
                // Dash ảo ảnh tới quái và chém chí mạng
                Vector3 strikePos = targetEnemy.transform.position;
                SpawnShadowSlash(strikePos, 160f);
                if (CameraController.instance != null) CameraController.instance.Shake(0.08f, 0.15f);
            }
            yield return new WaitForSeconds(0.07f);
        }
    }

    private void SpawnShadowSlash(Vector3 pos, float damage)
    {
        GameObject slash = new GameObject("ShadowSlash", typeof(CircleCollider2D), typeof(EnemyDamager));
        slash.transform.position = pos;
        CircleCollider2D col = slash.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 1.8f;

        EnemyDamager ed = slash.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.15f;

        SpawnNovaRing(pos, 1.6f, new Color(0.2f, 0.85f, 1f, 0.85f));
        Destroy(slash, 0.15f);
    }

    // ==========================================
    // 5. ADVENTURER: DIVINE SANCTUARY
    // ==========================================
    private IEnumerator UltimateAdventurer()
    {
        // 1. Hồi 60 máu cho người chơi
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.Heal(60f);
        }

        // 2. Vòng thánh quang quét sạch quái trong bán kính 9m
        GameObject blast = new GameObject("HolySanctuary", typeof(CircleCollider2D), typeof(EnemyDamager));
        blast.transform.position = transform.position;
        CircleCollider2D col = blast.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 9f;

        EnemyDamager ed = blast.GetComponent<EnemyDamager>();
        ed.damageAmount = 150f;
        ed.destroyOnImpact = false;
        ed.lifeTime = 0.6f;

        // 3. Hút toàn bộ Gem và Vàng trên toàn bản đồ về người!
        ExperienceLevelController.instance?.SpawnExp(transform.position, 15);
        CoinController.instance?.DropCoin(transform.position, 20);

        // Hút các pickup hiện có trên sân
        ExpPickup[] gems = FindObjectsByType<ExpPickup>();
        foreach (var g in gems)
        {
            if (g != null) g.transform.position = Vector3.MoveTowards(g.transform.position, transform.position, 15f);
        }

        SpawnNovaRing(transform.position, 9f, new Color(1f, 0.95f, 0.5f, 0.85f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.35f, 0.45f);

        yield return new WaitForSeconds(0.6f);
        Destroy(blast);
    }

    // ==========================================
    // 6. RED ARCHER: SPIRIT ARROW BARRAGE
    // ==========================================
    private IEnumerator UltimateRedArcher()
    {
        int waveCount = 5;
        float baseAngle = player != null && player.facingDirection < 0 ? 180f : 0f;

        for (int w = 0; w < waveCount; w++)
        {
            int arrowCount = 10;
            for (int i = 0; i < arrowCount; i++)
            {
                float spread = Random.Range(-35f, 35f);
                Vector3 dir = Quaternion.Euler(0, 0, baseAngle + spread) * Vector3.right;
                SpawnSpiritArrow(transform.position, dir, 85f);
            }
            if (CameraController.instance != null) CameraController.instance.Shake(0.15f, 0.2f);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SpawnSpiritArrow(Vector3 origin, Vector3 dir, float damage)
    {
        GameObject arrow = new GameObject("SpiritArrow", typeof(CircleCollider2D), typeof(EnemyDamager), typeof(Rigidbody2D));
        arrow.transform.position = origin;
        arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        arrow.transform.localScale = new Vector3(1.8f, 0.4f, 1f);

        CircleCollider2D col = arrow.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        EnemyDamager ed = arrow.GetComponent<EnemyDamager>();
        ed.damageAmount = damage;
        ed.destroyOnImpact = false; // Xuyên táo
        ed.lifeTime = 1.4f;

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = dir * 18f;

        Destroy(arrow, 1.4f);
    }

    private void SpawnNovaRing(Vector3 center, float radius, Color col)
    {
        GameObject ring = new GameObject("NovaRing", typeof(SpriteRenderer));
        ring.transform.position = center;
        ring.transform.localScale = Vector3.one * 0.1f;
        SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
        sr.color = col;
        sr.sortingOrder = 12;
        StartCoroutine(AnimateRing(ring, radius, 0.4f));
    }

    private IEnumerator AnimateRing(GameObject ring, float targetScale, float dur)
    {
        float t = 0f;
        SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
        Color startCol = sr.color;

        while (t < dur && ring != null)
        {
            t += Time.deltaTime;
            float progress = t / dur;
            ring.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, targetScale * 2f, progress);
            sr.color = new Color(startCol.r, startCol.g, startCol.b, Mathf.Lerp(startCol.a, 0f, progress));
            yield return null;
        }

        if (ring != null) Destroy(ring);
    }
}
