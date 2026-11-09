using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum BossType
{
    BroodQueen,      // Stage 1 (2:00)
    FrostfangFenrir, // Stage 2 (5:00)
    SkeletonWarlord, // Stage 3 (8:00)
    DeathPhantom,    // Stage 4 (11:30)
    GoldenDragon     // Stage 5 (15:00 Final Boss)
}

/// <summary>
/// Controls multi-tier progression bosses with distinct identities, attack patterns,
/// telegraphed moves, and royal gothic health bars.
/// </summary>
[RequireComponent(typeof(EnemyController))]
public class BossController : MonoBehaviour
{
    [Header("Boss Identity")]
    public BossType bossType = BossType.BroodQueen;
    public string bossName = "THE BROOD QUEEN";
    public string bossSubtitle = "✦ MATRIARCH OF THE TOXIC SWARM ✦";
    public Color barColor = new Color(0.62f, 0.3f, 0.95f);
    public int chestTier = 1;

    [Header("Movement")]
    public float stalkSpeed = 1.6f;
    public float stalkTime = 2.2f;

    [Header("Charge / Pounce")]
    public float chargeWindup = 0.85f;
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.55f;
    public float chargeRecovery = 0.55f;

    [Header("Projectiles")]
    public int novaBullets = 14;
    public float bulletSpeed = 4.8f;
    public float bulletDamage = 12f;
    public Sprite bulletSprite;
    public Color bulletColor = new Color(0.6f, 1f, 0.35f);

    [Header("Spiral (Enraged)")]
    public float spiralDuration = 2.8f;
    public float spiralFireRate = 0.08f;
    public int spiralArms = 3;
    public float spiralTurnSpeed = 200f;

    [Header("Minions")]
    public GameObject minionPrefab;
    public int minionsPerCall = 6;
    public float minionRingRadius = 2.6f;

    [Header("Enrage")]
    [Range(0f, 1f)] public float enrageAtHealth = 0.5f;
    public float enrageSpeedMultiplier = 1.35f;
    public Color enrageTint = new Color(1f, 0.45f, 0.45f);

    [Header("Rewards")]
    public int bonusCoins = 20;

    private enum Attack
    {
        None,
        Charge,
        Nova,
        MinionSummon,
        Spiral,
        TargetedBarrage,
        DangerZones,
        TeleportWarp,
        DragonBreath
    }

    // ---------------------------------------------------------------- FACTORY

    public static BossController ConfigureAsBroodQueen(GameObject go, GameObject minion)
    {
        return ConfigureBoss(go, BossType.BroodQueen, minion);
    }

    public static BossController ConfigureBoss(GameObject go, BossType type, GameObject minionPrefab)
    {
        BossController boss = go.GetComponent<BossController>();
        if (boss == null) boss = go.AddComponent<BossController>();

        boss.bossType = type;
        boss.minionPrefab = minionPrefab;

        EnemyController ec = go.GetComponent<EnemyController>();
        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>(true);
        EnemyAnimation anim = go.GetComponent<EnemyAnimation>();

        if (body != null) body.mass = 60f;
        if (anim != null)
        {
            anim.speed = 0.22f;
            anim.minSize = 0.94f;
            anim.maxSize = 1.06f;
        }

        switch (type)
        {
            case BossType.BroodQueen:
                go.name = "Boss - Brood Queen";
                go.transform.localScale = Vector3.one * 3.2f;
                boss.bossName = "THE BROOD QUEEN";
                boss.bossSubtitle = "✦ MATRIARCH OF THE TOXIC SWARM ✦";
                boss.barColor = new Color(0.65f, 0.25f, 0.95f);
                boss.bulletColor = new Color(0.55f, 1f, 0.35f);
                boss.chestTier = 1;
                boss.bonusCoins = 15;
                boss.stalkSpeed = 1.6f;
                boss.chargeSpeed = 12f;
                boss.novaBullets = 14;

                ec.health = 220f; // Easy-going first boss
                ec.damage = 14f;
                ec.moveSpeed = 1.6f;
                ec.hitWaitTime = 0.8f;
                ec.knockBackTime = 0f;
                ec.expToGive = 60;
                ec.coinValue = 10;
                ec.coinDropRate = 1f;

                if (sr != null)
                {
                    sr.color = new Color(0.85f, 0.65f, 1f);
                    sr.sortingOrder += 6;
                }
                break;

            case BossType.FrostfangFenrir:
                go.name = "Boss - Frostfang Fenrir";
                go.transform.localScale = Vector3.one * 3.4f;
                boss.bossName = "FROSTFANG FENRIR";
                boss.bossSubtitle = "✦ ANCIENT GLACIAL ALPHA ✦";
                boss.barColor = new Color(0.1f, 0.82f, 1f);
                boss.bulletColor = new Color(0.45f, 0.9f, 1f);
                boss.chestTier = 2;
                boss.bonusCoins = 25;
                boss.stalkSpeed = 2.2f;
                boss.chargeSpeed = 14.5f;
                boss.chargeWindup = 0.7f;
                boss.chargeRecovery = 0.45f;
                boss.novaBullets = 18;
                boss.spiralArms = 4;

                ec.health = 520f;
                ec.damage = 18f;
                ec.moveSpeed = 2.2f;
                ec.hitWaitTime = 0.7f;
                ec.knockBackTime = 0f;
                ec.expToGive = 110;
                ec.coinValue = 15;
                ec.coinDropRate = 1f;

                if (sr != null)
                {
                    sr.color = new Color(0.4f, 0.85f, 1f);
                    sr.sortingOrder += 6;
                }
                break;

            case BossType.SkeletonWarlord:
                go.name = "Boss - Warlord Malakor";
                go.transform.localScale = Vector3.one * 3.6f;
                boss.bossName = "WARLORD MALAKOR";
                boss.bossSubtitle = "✦ TYRANT OF THE CURSED CATACOMBS ✦";
                boss.barColor = new Color(0.92f, 0.2f, 0.25f);
                boss.bulletColor = new Color(1f, 0.35f, 0.4f);
                boss.chestTier = 2;
                boss.bonusCoins = 35;
                boss.stalkSpeed = 1.8f;
                boss.chargeSpeed = 11.5f;
                boss.novaBullets = 20;
                boss.spiralArms = 4;

                ec.health = 850f;
                ec.damage = 22f;
                ec.moveSpeed = 1.8f;
                ec.hitWaitTime = 0.75f;
                ec.knockBackTime = 0f;
                ec.expToGive = 160;
                ec.coinValue = 20;
                ec.coinDropRate = 1f;

                if (sr != null)
                {
                    sr.color = new Color(1f, 0.55f, 0.55f);
                    sr.sortingOrder += 6;
                }
                break;

            case BossType.DeathPhantom:
                go.name = "Boss - Thanatos";
                go.transform.localScale = Vector3.one * 3.8f;
                boss.bossName = "THANATOS";
                boss.bossSubtitle = "✦ GRIM HARVESTER OF THE VOID ✦";
                boss.barColor = new Color(0.72f, 0.15f, 0.95f);
                boss.bulletColor = new Color(0.85f, 0.45f, 1f);
                boss.chestTier = 3;
                boss.bonusCoins = 50;
                boss.stalkSpeed = 2.0f;
                boss.chargeSpeed = 13f;
                boss.novaBullets = 24;
                boss.spiralArms = 5;

                ec.health = 1250f;
                ec.damage = 26f;
                ec.moveSpeed = 2.0f;
                ec.hitWaitTime = 0.7f;
                ec.knockBackTime = 0f;
                ec.expToGive = 220;
                ec.coinValue = 30;
                ec.coinDropRate = 1f;

                if (sr != null)
                {
                    sr.color = new Color(0.7f, 0.45f, 1f, 0.92f);
                    sr.sortingOrder += 6;
                }
                break;

            case BossType.GoldenDragon:
                go.name = "Boss - Pyroclast The Dragon";
                go.transform.localScale = Vector3.one * 4.2f;
                boss.bossName = "PYROCLAST";
                boss.bossSubtitle = "✦ CATACLYSM SOVEREIGN OF THE GOLDEN SUN ✦";
                boss.barColor = new Color(1f, 0.75f, 0.1f);
                boss.bulletColor = new Color(1f, 0.45f, 0.1f);
                boss.chestTier = 3;
                boss.bonusCoins = 80;
                boss.stalkSpeed = 2.1f;
                boss.chargeSpeed = 15f;
                boss.chargeWindup = 1.0f;
                boss.chargeDuration = 0.65f;
                boss.novaBullets = 28;
                boss.spiralArms = 5;

                ec.health = 1950f;
                ec.damage = 32f;
                ec.moveSpeed = 2.1f;
                ec.hitWaitTime = 0.65f;
                ec.knockBackTime = 0f;
                ec.expToGive = 350;
                ec.coinValue = 50;
                ec.coinDropRate = 1f;

                if (sr != null)
                {
                    sr.color = new Color(1f, 0.88f, 0.35f);
                    sr.sortingOrder += 6;
                }
                break;
        }

        return boss;
    }

    // ---------------------------------------------------------------- STATE

    private EnemyController enemy;
    private Rigidbody2D rb;
    private Transform target;
    private SpriteRenderer body;
    private Transform bodyT;
    private Vector3 bodyRestPos;
    private Color baseColor;
    private LineRenderer telegraph;

    private bool enraged, enragePending;
    private float SpeedMul => enraged ? enrageSpeedMultiplier : 1f;

    private RectTransform healthBarRoot, healthFill, healthGhost;
    private Image healthFillImage;
    private TMP_Text hpValueText;
    private TMP_Text subtitleText;
    private float ghostPct = 1f;

    // Boss Visual Aura & Crest
    private GameObject bossAuraObj;
    private SpriteRenderer bossAuraSr;
    private Transform bossCrownT;
    private float baseAuraScale = 2.4f;
    private float auraRotationSpeed = 28f;

    public static void HideAllBossUI()
    {
        BossController[] all = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include);
        foreach (var b in all)
        {
            if (b != null && b.healthBarRoot != null)
            {
                b.healthBarRoot.gameObject.SetActive(false);
            }
        }
    }

    void Awake()
    {
        enemy = GetComponent<EnemyController>();
        enemy.movementOverridden = true;
        enemy.Died += OnDied;
        rb = enemy.theRB != null ? enemy.theRB : GetComponent<Rigidbody2D>();

        body = GetComponentInChildren<SpriteRenderer>();
        if (body != null)
        {
            bodyT = body.transform;
            bodyRestPos = bodyT.localPosition;
            baseColor = body.color;
        }

        BuildTelegraph();
    }

    void Start()
    {
        if (PlayerHealthController.instance != null) target = PlayerHealthController.instance.transform;
        BuildBossAura();
        BuildBossCrown();
        BuildHealthBar();
        ProceduralSFX.PlayBossRoar();
        StartCoroutine(Brain());
    }

    void Update()
    {
        if (!enraged && !enragePending && enemy.health <= enemy.MaxHealth * enrageAtHealth)
        {
            enragePending = true;
        }
        UpdateBossVisualAura();
        UpdateHealthBar();
    }

    void OnDestroy()
    {
        if (healthBarRoot != null) Destroy(healthBarRoot.gameObject);
        if (bossAuraObj != null) Destroy(bossAuraObj);
        if (bossCrownT != null) Destroy(bossCrownT.gameObject);
    }

    private void BuildBossAura()
    {
        bossAuraObj = new GameObject("Boss Ground Aura");
        bossAuraObj.transform.SetParent(transform, false);
        bossAuraObj.transform.localPosition = new Vector3(0f, -0.4f, 0f);

        bossAuraSr = bossAuraObj.AddComponent<SpriteRenderer>();
        bossAuraSr.sprite = GetRunicAuraSprite();
        bossAuraSr.sortingOrder = 1; // Under the boss body, above ground tiles

        baseAuraScale = Mathf.Max(transform.localScale.x * 0.95f, 2.2f);
        bossAuraObj.transform.localScale = Vector3.one * baseAuraScale;

        // Elemental Aura Color per Boss
        Color auraCol = barColor;
        auraCol.a = 0.72f;
        bossAuraSr.color = auraCol;
    }

    private void BuildBossCrown()
    {
        GameObject crownGO = new GameObject("Boss Crown Crest");
        crownGO.transform.SetParent(transform, false);
        crownGO.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        bossCrownT = crownGO.transform;

        SpriteRenderer crownSr = crownGO.AddComponent<SpriteRenderer>();
        crownSr.sprite = GetBossCrownSprite();
        crownSr.sortingOrder = (body != null ? body.sortingOrder : 10) + 1;
        crownSr.color = new Color(1f, 0.92f, 0.45f, 0.95f);
        crownGO.transform.localScale = Vector3.one * 0.85f;
    }

    private void UpdateBossVisualAura()
    {
        if (bossAuraObj != null)
        {
            float rotSpeed = enraged ? auraRotationSpeed * 2.8f : auraRotationSpeed;
            bossAuraObj.transform.Rotate(0f, 0f, -rotSpeed * Time.deltaTime);

            float pulse = (enraged ? 1.25f : 1.05f) + Mathf.Sin(Time.time * (enraged ? 7f : 3f)) * 0.08f;
            bossAuraObj.transform.localScale = Vector3.one * (baseAuraScale * pulse);

            if (bossAuraSr != null)
            {
                Color targetCol = enraged ? Color.Lerp(barColor, new Color(1f, 0.2f, 0.2f), 0.75f) : barColor;
                targetCol.a = enraged ? 0.9f : 0.72f;
                bossAuraSr.color = targetCol;
            }
        }

        if (bossCrownT != null)
        {
            float bob = Mathf.Sin(Time.time * 4f) * 0.08f;
            bossCrownT.localPosition = new Vector3(0f, 1.25f + bob, 0f);
            float scaleBob = 0.85f + Mathf.Sin(Time.time * 6f) * 0.05f;
            bossCrownT.localScale = Vector3.one * scaleBob;
        }
    }

    private static Sprite runicAuraSprite;
    private static Sprite GetRunicAuraSprite()
    {
        if (runicAuraSprite != null) return runicAuraSprite;
        const int s = 128;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[s * s];
        Vector2 center = new Vector2(s * 0.5f, s * 0.5f);
        float maxR = s * 0.48f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                float normD = d / maxR;
                if (normD > 1f)
                {
                    px[y * s + x] = Color.clear;
                    continue;
                }

                float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;

                // 1. Outer runic notched teeth ring (normD 0.88 - 0.98)
                float outerRing = (normD >= 0.88f && normD <= 0.98f) ? 1f : 0f;
                float notches = Mathf.Sin(angle * 12f * Mathf.Deg2Rad) > 0f ? 1f : 0.25f;

                // 2. Mid glyph band (normD 0.62 - 0.72)
                float midRing = (normD >= 0.62f && normD <= 0.72f) ? 0.9f : 0f;
                float midNotches = Mathf.Sin(angle * 8f * Mathf.Deg2Rad) > 0.3f ? 1f : 0.3f;

                // 3. Inner mystic star rays (8 rays)
                float rays = Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 4f * Mathf.Deg2Rad)), 6f) * (1f - normD);

                // 4. Center radiant core
                float core = Mathf.Clamp01(1f - (normD / 0.45f)) * 0.65f;

                float alpha = Mathf.Clamp01((outerRing * notches) + (midRing * midNotches) + rays * 0.8f + core);
                px[y * s + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        runicAuraSprite = Sprite.Create(tex, new Rect(0, 0, s, s), new Vector2(0.5f, 0.5f), 100f);
        return runicAuraSprite;
    }

    private static Sprite bossCrownSprite;
    private static Sprite GetBossCrownSprite()
    {
        if (bossCrownSprite != null) return bossCrownSprite;
        const int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];

        Color gold = new Color(1f, 0.85f, 0.25f, 1f);
        Color gem = new Color(1f, 0.25f, 0.35f, 1f);
        Color white = Color.white;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                px[y * w + x] = Color.clear;
            }
        }

        // Mini 16x16 Crown pattern in center
        int ox = 8, oy = 8;
        for (int x = 0; x < 16; x++)
        {
            // Base gold bar
            px[(oy + 2) * w + (ox + x)] = gold;
            px[(oy + 3) * w + (ox + x)] = gold;
        }
        // Crown spires: left, center, right
        int[] spires = { 2, 8, 14 };
        int[] heights = { 10, 14, 10 };
        for (int i = 0; i < 3; i++)
        {
            int sx = spires[i];
            int sh = heights[i];
            for (int y = 3; y < sh; y++)
            {
                px[(oy + y) * w + (ox + sx)] = gold;
                if (sx > 0) px[(oy + y) * w + (ox + sx - 1)] = gold;
            }
            // Tip jewels
            px[(oy + sh) * w + (ox + sx)] = (i == 1) ? white : gem;
        }
        // Center gem on base
        px[(oy + 3) * w + (ox + 8)] = gem;

        tex.SetPixels(px);
        tex.Apply();
        bossCrownSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        return bossCrownSprite;
    }

    private bool PlayerAlive => PlayerHealthController.instance != null && PlayerHealthController.instance.gameObject.activeSelf;
    private Vector2 DirToPlayer => target != null ? ((Vector2)(target.position - transform.position)).normalized : Vector2.down;

    // ---------------------------------------------------------------- BRAIN

    private IEnumerator Brain()
    {
        yield return Roar(1.3f, false);

        Attack last = Attack.None;
        while (true)
        {
            while (!PlayerAlive)
            {
                rb.linearVelocity = Vector2.zero;
                yield return null;
            }

            yield return Stalk(stalkTime / SpeedMul);

            if (enragePending)
            {
                enragePending = false;
                yield return Enrage();
            }

            Attack next = PickAttack(last);
            last = next;

            switch (next)
            {
                case Attack.Charge:
                    yield return Charge();
                    break;
                case Attack.Nova:
                    yield return Nova();
                    break;
                case Attack.MinionSummon:
                    yield return MinionSummon();
                    break;
                case Attack.Spiral:
                    yield return Spiral();
                    break;
                case Attack.TargetedBarrage:
                    yield return TargetedBarrage();
                    break;
                case Attack.DangerZones:
                    yield return DangerZones();
                    break;
                case Attack.TeleportWarp:
                    yield return TeleportWarp();
                    break;
                case Attack.DragonBreath:
                    yield return DragonBreath();
                    break;
            }
        }
    }

    private Attack PickAttack(Attack last)
    {
        Attack[] pool;
        switch (bossType)
        {
            case BossType.BroodQueen:
                pool = enraged
                    ? new[] { Attack.Charge, Attack.Nova, Attack.MinionSummon, Attack.Spiral, Attack.Charge }
                    : new[] { Attack.Charge, Attack.Nova, Attack.MinionSummon, Attack.Nova };
                break;

            case BossType.FrostfangFenrir:
                pool = enraged
                    ? new[] { Attack.Charge, Attack.Nova, Attack.MinionSummon, Attack.Spiral, Attack.Charge }
                    : new[] { Attack.Charge, Attack.Nova, Attack.MinionSummon, Attack.Charge };
                break;

            case BossType.SkeletonWarlord:
                pool = enraged
                    ? new[] { Attack.TargetedBarrage, Attack.DangerZones, Attack.MinionSummon, Attack.Spiral, Attack.Nova }
                    : new[] { Attack.TargetedBarrage, Attack.DangerZones, Attack.MinionSummon, Attack.Nova };
                break;

            case BossType.DeathPhantom:
                pool = enraged
                    ? new[] { Attack.TeleportWarp, Attack.Nova, Attack.Spiral, Attack.MinionSummon, Attack.TeleportWarp }
                    : new[] { Attack.TeleportWarp, Attack.Nova, Attack.MinionSummon, Attack.Nova };
                break;

            case BossType.GoldenDragon:
                pool = enraged
                    ? new[] { Attack.DragonBreath, Attack.DangerZones, Attack.Charge, Attack.Spiral, Attack.Nova }
                    : new[] { Attack.DragonBreath, Attack.DangerZones, Attack.Charge, Attack.MinionSummon };
                break;

            default:
                pool = new[] { Attack.Charge, Attack.Nova };
                break;
        }

        for (int tries = 0; tries < 12; tries++)
        {
            Attack pick = pool[Random.Range(0, pool.Length)];
            if (pick == last) continue;
            if (pick == Attack.MinionSummon && (minionPrefab == null || EnemySpawner.instance == null)) continue;
            return pick;
        }
        return pool[0];
    }

    // ---------------------------------------------------------------- ATTACKS

    private IEnumerator Stalk(float duration)
    {
        for (float t = 0f; t < duration && PlayerAlive; t += Time.deltaTime)
        {
            rb.linearVelocity = DirToPlayer * stalkSpeed * SpeedMul;
            yield return null;
        }
    }

    private IEnumerator Charge()
    {
        int charges = enraged ? 2 : 1;
        if (bossType == BossType.FrostfangFenrir && enraged) charges = 3;

        for (int c = 0; c < charges; c++)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 dir = DirToPlayer;
            float lane = chargeSpeed * SpeedMul * chargeDuration;
            float windup = c == 0 ? chargeWindup : chargeWindup * 0.6f;

            telegraph.enabled = true;
            for (float t = 0f; t < windup; t += Time.deltaTime)
            {
                if (t < windup * 0.7f) dir = DirToPlayer;
                telegraph.SetPosition(0, transform.position);
                telegraph.SetPosition(1, (Vector2)transform.position + dir * lane);

                float blink = Mathf.PingPong(t * 9f, 1f);
                Color telegraphColor = bossType == BossType.FrostfangFenrir ? new Color(0.2f, 0.8f, 1f) : new Color(1f, 0.2f, 0.1f);
                telegraph.startColor = telegraph.endColor = new Color(telegraphColor.r, telegraphColor.g, telegraphColor.b, Mathf.Lerp(0.2f, 0.7f, blink));

                Flash(telegraphColor, blink * 0.7f);
                Shake(0.06f);
                yield return null;
            }
            telegraph.enabled = false;
            ResetBody();

            for (float t = 0f; t < chargeDuration; t += Time.deltaTime)
            {
                rb.linearVelocity = dir * chargeSpeed * SpeedMul;
                yield return null;
            }

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(chargeRecovery / (c == charges - 1 ? 1f : 2f));
        }
    }

    private IEnumerator Nova()
    {
        rb.linearVelocity = Vector2.zero;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            Flash(bulletColor, Mathf.PingPong(t * 6f, 1f));
            yield return null;
        }
        ResetBody();

        float offset = Random.Range(0f, 360f);
        FireRing(novaBullets, offset);
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(1);

        if (enraged)
        {
            yield return new WaitForSeconds(0.35f);
            FireRing(novaBullets, offset + 180f / novaBullets);
        }
        yield return new WaitForSeconds(0.4f);
    }

    private IEnumerator Spiral()
    {
        rb.linearVelocity = Vector2.zero;
        float angle = Random.Range(0f, 360f);
        float fireCounter = 0f;
        for (float t = 0f; t < spiralDuration && PlayerAlive; t += Time.deltaTime)
        {
            angle += spiralTurnSpeed * Time.deltaTime;
            fireCounter -= Time.deltaTime;
            if (fireCounter <= 0f)
            {
                fireCounter = spiralFireRate;
                FireRing(spiralArms, angle);
            }
            Shake(0.03f);
            yield return null;
        }
        ResetBody();
        yield return new WaitForSeconds(0.45f);
    }

    private IEnumerator MinionSummon()
    {
        rb.linearVelocity = Vector2.zero;
        for (float t = 0f; t < 0.65f; t += Time.deltaTime)
        {
            Flash(barColor, Mathf.PingPong(t * 6f, 1f));
            Shake(0.04f);
            yield return null;
        }
        ResetBody();

        int count = enraged ? minionsPerCall + 2 : minionsPerCall;
        float offset = Random.Range(0f, 360f);
        for (int i = 0; i < count; i++)
        {
            float a = (offset + i * 360f / count) * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(a), Mathf.Sin(a)) * minionRingRadius;
            EnemySpawner.instance.SpawnMinion(minionPrefab, pos);
        }

        if (CameraController.instance != null) CameraController.instance.Shake(0.2f, 0.2f);
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator TargetedBarrage()
    {
        rb.linearVelocity = Vector2.zero;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            Flash(new Color(1f, 0.2f, 0.2f), Mathf.PingPong(t * 8f, 1f));
            yield return null;
        }
        ResetBody();

        int spears = enraged ? 9 : 7;
        Vector2 baseDir = DirToPlayer;
        float fanAngle = 45f;
        int layer = body != null ? body.sortingLayerID : 0;
        int order = body != null ? body.sortingOrder + 1 : 10;

        for (int i = 0; i < spears; i++)
        {
            float spread = Mathf.Lerp(-fanAngle * 0.5f, fanAngle * 0.5f, (float)i / (spears - 1));
            Vector2 shotDir = Quaternion.Euler(0f, 0f, spread) * baseDir;
            BossProjectile.Spawn(transform.position, shotDir * (bulletSpeed * 1.35f * SpeedMul), bulletDamage * 1.15f, bulletSprite,
                bulletColor, layer, order, 0.6f);
        }

        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        yield return new WaitForSeconds(0.6f);
    }

    private IEnumerator DangerZones()
    {
        rb.linearVelocity = Vector2.zero;
        for (float t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            Flash(new Color(1f, 0.5f, 0.1f), Mathf.PingPong(t * 7f, 1f));
            yield return null;
        }
        ResetBody();

        int zones = enraged ? 5 : 3;
        Vector3 playerPos = target != null ? target.position : transform.position;

        for (int i = 0; i < zones; i++)
        {
            Vector3 spawnPos = playerPos + (Vector3)(Random.insideUnitCircle * (1.5f + i * 1.2f));
            Color dangerCol = bossType == BossType.GoldenDragon ? new Color(1f, 0.35f, 0.05f) : new Color(0.9f, 0.15f, 0.2f);
            Color burstCol = bossType == BossType.GoldenDragon ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.4f, 0.5f);
            BossDangerZone.Create(spawnPos, 2.4f, 1.3f, bulletDamage * 1.5f, dangerCol, burstCol);
        }

        yield return new WaitForSeconds(0.55f);
    }

    private IEnumerator TeleportWarp()
    {
        rb.linearVelocity = Vector2.zero;
        // Fade out
        for (float t = 0f; t < 0.35f; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(1f, 0.1f, t / 0.35f);
            if (body != null) body.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            Shake(0.08f);
            yield return null;
        }

        // Teleport near player
        if (target != null)
        {
            Vector2 randomSide = Random.insideUnitCircle.normalized * 4.5f;
            transform.position = target.position + (Vector3)randomSide;
        }

        // Fade in & unleash burst
        for (float t = 0f; t < 0.25f; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0.1f, 1f, t / 0.25f);
            if (body != null) body.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }
        ResetBody();

        FireRing(16, Random.Range(0f, 360f));
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(1);
        if (CameraController.instance != null) CameraController.instance.Shake(0.2f, 0.2f);

        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator DragonBreath()
    {
        rb.linearVelocity = Vector2.zero;
        int flameBursts = enraged ? 18 : 12;
        float sweepArc = 65f;
        int layer = body != null ? body.sortingLayerID : 0;
        int order = body != null ? body.sortingOrder + 1 : 10;

        for (int i = 0; i < flameBursts && PlayerAlive; i++)
        {
            Vector2 dir = DirToPlayer;
            float sweepAngle = Mathf.Sin((float)i / flameBursts * Mathf.PI * 2f) * sweepArc;
            Vector2 fireDir = Quaternion.Euler(0f, 0f, sweepAngle) * dir;

            BossProjectile.Spawn(transform.position, fireDir * (bulletSpeed * 1.25f * SpeedMul), bulletDamage,
                bulletSprite, new Color(1f, 0.45f, 0.1f), layer, order, 0.55f, 4.5f);

            Flash(new Color(1f, 0.7f, 0.1f), 0.5f);
            Shake(0.04f);
            yield return new WaitForSeconds(0.08f);
        }
        ResetBody();
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Enrage()
    {
        enraged = true;
        yield return Roar(1.4f, true);
        baseColor = enrageTint;
        ResetBody();

        if (healthFillImage != null) healthFillImage.color = new Color(1f, 0.15f, 0.15f);
        if (subtitleText != null) subtitleText.text = "<color=#FF2222>✦  ⚠  ENRAGED PHASE  ⚠  ✦</color>";

        FireRing(novaBullets + 8, Random.Range(0f, 360f));
        if (CameraController.instance != null) CameraController.instance.Shake(0.4f, 0.45f);
    }

    private IEnumerator Roar(float duration, bool angry)
    {
        rb.linearVelocity = Vector2.zero;
        enemy.invulnerable = true;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Shake(angry ? 0.14f : 0.08f);
            Flash(angry ? new Color(1f, 0.2f, 0.2f) : Color.white, Mathf.PingPong(t * 5f, 1f) * 0.7f);
            yield return null;
        }
        enemy.invulnerable = false;
        ResetBody();
    }

    private void FireRing(int count, float startAngle)
    {
        int layer = body != null ? body.sortingLayerID : 0;
        int order = body != null ? body.sortingOrder + 1 : 10;
        for (int i = 0; i < count; i++)
        {
            float a = (startAngle + i * 360f / count) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
            BossProjectile.Spawn(transform.position, dir * bulletSpeed * SpeedMul, bulletDamage, bulletSprite,
                enraged ? new Color(1f, 0.45f, 0.35f) : bulletColor, layer, order);
        }
    }

    // ---------------------------------------------------------------- DEATH

    private void OnDied(EnemyController e)
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        if (telegraph != null) telegraph.enabled = false;
        BossProjectile.ClearAll();

        if (CoinController.instance != null)
        {
            for (int i = 0; i < bonusCoins; i++)
            {
                CoinController.instance.DropCoin(transform.position + (Vector3)(Random.insideUnitCircle * 2.5f), enemy.coinValue);
            }
        }

        if (healthBarRoot != null) Destroy(healthBarRoot.gameObject);
        if (bossAuraObj != null) Destroy(bossAuraObj);
        if (bossCrownT != null) Destroy(bossCrownT.gameObject);

        // Spawn scaled Royal Treasure Chest
        TreasureChest.SpawnChest(transform.position, chestTier);

        if (bossType == BossType.GoldenDragon)
        {
            ShowBanner("✦ VICTORY! FINAL BOSS PYROCLAST SLAIN! ✦", new Color(1f, 0.9f, 0.3f));
            if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(8);
            if (LevelManager.instance != null) LevelManager.instance.TriggerVictory();
        }
        else
        {
            ShowBanner("✦ " + bossName + " DEFEATED ✦", new Color(1f, 0.88f, 0.35f));
        }

        if (EnemySpawner.instance != null)
        {
            EnemySpawner.instance.OnBossDefeated();
        }
    }

    // ---------------------------------------------------------------- VISUALS

    private void Flash(Color color, float amount)
    {
        if (body != null) body.color = Color.Lerp(baseColor, color, amount);
    }

    private void Shake(float strength)
    {
        if (bodyT != null) bodyT.localPosition = bodyRestPos + (Vector3)(Random.insideUnitCircle * strength);
    }

    private void ResetBody()
    {
        if (body == null) return;
        body.color = baseColor;
        bodyT.localPosition = bodyRestPos;
    }

    private void BuildTelegraph()
    {
        GameObject go = new GameObject("Charge Telegraph");
        go.transform.SetParent(transform, false);
        telegraph = go.AddComponent<LineRenderer>();
        telegraph.useWorldSpace = true;
        telegraph.positionCount = 2;
        telegraph.startWidth = 1.6f;
        telegraph.endWidth = 1.2f;
        telegraph.numCapVertices = 4;
        telegraph.material = new Material(Shader.Find("Sprites/Default"));
        if (body != null)
        {
            telegraph.sortingLayerID = body.sortingLayerID;
            telegraph.sortingOrder = body.sortingOrder - 1;
        }
        telegraph.enabled = false;
    }

    // ---------------------------------------------------------------- UI (ROYAL GOTHIC HEALTH BAR)

    private Canvas FindHudCanvas()
    {
        if (UIController.instance == null) return null;
        if (UIController.instance.timeText != null) return UIController.instance.timeText.canvas;
        return UIController.instance.GetComponentInParent<Canvas>();
    }

    private TMP_Text MakeText(Transform parent, string name, string text, float size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        if (UIController.instance != null && UIController.instance.retroFont != null) tmp.font = UIController.instance.retroFont;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    private RectTransform MakeBar(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return (RectTransform)go.transform;
    }

    private void BuildHealthBar()
    {
        Canvas canvas = FindHudCanvas();
        if (canvas == null) return;

        GameObject root = new GameObject("Boss Health Bar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        healthBarRoot = (RectTransform)root.transform;
        healthBarRoot.anchorMin = healthBarRoot.anchorMax = new Vector2(0.5f, 1f);
        healthBarRoot.pivot = new Vector2(0.5f, 1f);
        healthBarRoot.anchoredPosition = new Vector2(0f, -88f);
        healthBarRoot.sizeDelta = new Vector2(720f, 68f);

        // Boss Name Title
        RectTransform nameRt = MakeText(root.transform, "Boss Name", "❖ " + bossName + " ❖", 32, new Color(1f, 0.88f, 0.35f)).rectTransform;
        nameRt.anchorMin = new Vector2(0f, 1f);
        nameRt.anchorMax = new Vector2(1f, 1f);
        nameRt.pivot = new Vector2(0.5f, 1f);
        nameRt.anchoredPosition = new Vector2(0f, 0f);
        nameRt.sizeDelta = new Vector2(0f, 32f);

        // Boss Subtitle
        subtitleText = MakeText(root.transform, "Boss Subtitle", bossSubtitle, 19, new Color(0.9f, 0.82f, 0.65f, 0.9f));
        subtitleText.fontStyle = FontStyles.Normal;
        RectTransform subRt = subtitleText.rectTransform;
        subRt.anchorMin = new Vector2(0f, 1f);
        subRt.anchorMax = new Vector2(1f, 1f);
        subRt.pivot = new Vector2(0.5f, 1f);
        subRt.anchoredPosition = new Vector2(0f, -30f);
        subRt.sizeDelta = new Vector2(0f, 20f);

        // Gilded Gold Outer Frame
        RectTransform goldFrame = MakeBar(root.transform, "GoldFrame", new Color(0.85f, 0.65f, 0.15f, 0.95f));
        goldFrame.anchorMin = new Vector2(0f, 0f);
        goldFrame.anchorMax = new Vector2(1f, 0f);
        goldFrame.pivot = new Vector2(0.5f, 0f);
        goldFrame.anchoredPosition = Vector2.zero;
        goldFrame.sizeDelta = new Vector2(0f, 24f);

        // Dark Velvet Inner Well
        RectTransform innerWell = MakeBar(goldFrame, "InnerWell", new Color(0.06f, 0.04f, 0.08f, 0.95f));
        innerWell.anchorMin = Vector2.zero;
        innerWell.anchorMax = Vector2.one;
        innerWell.offsetMin = new Vector2(2f, 2f);
        innerWell.offsetMax = new Vector2(-2f, -2f);

        // Health Ghost (smooth drain behind damage)
        healthGhost = MakeBar(innerWell, "Ghost", new Color(1f, 1f, 1f, 0.8f));
        healthFill = MakeBar(innerWell, "Fill", barColor);
        healthFillImage = healthFill.GetComponent<Image>();

        foreach (RectTransform bar in new[] { healthGhost, healthFill })
        {
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = Vector2.one;
            bar.offsetMin = Vector2.zero;
            bar.offsetMax = Vector2.zero;
        }

        // Exact HP text overlay
        hpValueText = MakeText(goldFrame, "HP Value", "", 16, Color.white);
        hpValueText.rectTransform.anchorMin = Vector2.zero;
        hpValueText.rectTransform.anchorMax = Vector2.one;
        hpValueText.rectTransform.offsetMin = Vector2.zero;
        hpValueText.rectTransform.offsetMax = Vector2.zero;

        ShowBanner("⚠ APPROACHING: " + bossName + " ⚠", new Color(1f, 0.35f, 0.25f));
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null || enemy == null) return;
        float pct = enemy.MaxHealth > 0f ? Mathf.Clamp01(enemy.health / enemy.MaxHealth) : 0f;
        ghostPct = Mathf.MoveTowards(ghostPct, pct, Time.deltaTime * 0.35f);
        if (ghostPct < pct) ghostPct = pct;

        healthFill.anchorMax = new Vector2(pct, 1f);
        healthGhost.anchorMax = new Vector2(ghostPct, 1f);

        if (hpValueText != null)
        {
            hpValueText.text = $"{(int)Mathf.Max(0, enemy.health)} / {(int)enemy.MaxHealth} ({pct * 100f:0}%)";
        }
    }

    private void ShowBanner(string text, Color color)
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning(text, 3.5f);
        }
    }
}

public class BossBannerFade : MonoBehaviour
{
    public float duration = 3.5f;
    private float t;
    private TMP_Text text;

    void Awake() { text = GetComponent<TMP_Text>(); }

    void Update()
    {
        t += Time.deltaTime;
        if (text != null) text.alpha = t < duration * 0.6f ? 1f : Mathf.InverseLerp(duration, duration * 0.6f, t);
        transform.localPosition += Vector3.up * 14f * Time.deltaTime;
        if (t >= duration) Destroy(gameObject);
    }
}

public class BossDangerZone : MonoBehaviour
{
    private float delay = 1.2f;
    private float radius = 2.5f;
    private float damage = 18f;
    private Color dangerColor = new Color(1f, 0.2f, 0.2f);
    private Color burstColor = new Color(1f, 0.85f, 0.2f);
    private float timer = 0f;
    private SpriteRenderer sr;
    private SpriteRenderer indicator;
    private bool detonated = false;

    public static BossDangerZone Create(Vector3 pos, float radius, float delay, float damage, Color dangerColor, Color burstColor)
    {
        GameObject go = new GameObject("BossDangerZone");
        go.transform.position = pos;
        BossDangerZone dz = go.AddComponent<BossDangerZone>();
        dz.delay = delay;
        dz.radius = radius;
        dz.damage = damage;
        dz.dangerColor = dangerColor;
        dz.burstColor = burstColor;
        return dz;
    }

    void Start()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0.35f);
        sr.sortingOrder = 3;
        transform.localScale = Vector3.one * (radius * 2f);

        GameObject inner = new GameObject("Indicator");
        inner.transform.SetParent(transform, false);
        indicator = inner.AddComponent<SpriteRenderer>();
        indicator.sprite = GetCircleSprite();
        indicator.color = new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0.65f);
        indicator.sortingOrder = 4;
        inner.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (detonated) return;

        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / delay);

        if (indicator != null)
        {
            indicator.transform.localScale = Vector3.one * progress;
            float blink = Mathf.PingPong(timer * 10f, 1f);
            indicator.color = Color.Lerp(dangerColor, Color.white, blink * 0.45f);
        }

        if (timer >= delay)
        {
            detonated = true;
            Detonate();
        }
    }

    private void Detonate()
    {
        if (PlayerHealthController.instance != null && PlayerHealthController.instance.gameObject.activeSelf)
        {
            float dist = Vector2.Distance(transform.position, PlayerHealthController.instance.transform.position);
            if (dist <= radius)
            {
                PlayerHealthController.instance.TakeDamage(damage);
            }
        }

        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.25f, 0.28f);
        }

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        StartCoroutine(ExplodeVisual());
    }

    private IEnumerator ExplodeVisual()
    {
        if (indicator != null) Destroy(indicator.gameObject);
        sr.color = burstColor;
        transform.localScale = Vector3.one * (radius * 2.4f);

        for (float t = 0f; t < 0.28f; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(1f, 0f, t / 0.28f);
            sr.color = new Color(burstColor.r, burstColor.g, burstColor.b, a);
            yield return null;
        }

        Destroy(gameObject);
    }

    private static Sprite circleSprite;
    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null) return circleSprite;
        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
                float alpha = d <= 1f ? (d > 0.82f ? 1f : 0.35f) : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return circleSprite;
    }
}
