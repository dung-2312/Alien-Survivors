using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Brood Queen: a giant alien matriarch that stalks the player between telegraphed attacks.
//   Charge      - stops, shows a red warning lane, then rams down it (twice in a row when enraged).
//   Spore Nova  - a ring of slow bullets (a second, offset ring when enraged).
//   Brood Call  - summons a ring of minions around herself.
//   Spiral      - enraged only: plants herself and sprays a rotating bullet spiral.
// At enrageAtHealth she roars (briefly invulnerable), turns red and gets faster.
// Sits on top of an EnemyController, which still handles health, contact damage and weapon hits.
[RequireComponent(typeof(EnemyController))]
public class BossController : MonoBehaviour
{
    public string bossName = "THE BROOD QUEEN";

    [Header("Movement")]
    public float stalkSpeed = 1.6f; // Walk speed toward the player between attacks.
    public float stalkTime = 2.2f; // How long she walks before the next attack.

    [Header("Charge")]
    public float chargeWindup = 0.9f; // Warning time before the dash. Aim locks for the last 30%.
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.55f;
    public float chargeRecovery = 0.6f; // Stands still afterwards: the player's window to punish.

    [Header("Spore Nova")]
    public int novaBullets = 14;
    public float bulletSpeed = 4.5f;
    public float bulletDamage = 10f;
    public Sprite bulletSprite; // Optional; a glowing orb is generated when empty.
    public Color bulletColor = new Color(0.6f, 1f, 0.35f);

    [Header("Spiral (enraged only)")]
    public float spiralDuration = 2.6f;
    public float spiralFireRate = 0.08f; // Seconds between volleys.
    public int spiralArms = 3;
    public float spiralTurnSpeed = 200f; // Degrees per second.

    [Header("Brood Call")]
    public GameObject minionPrefab; // A regular enemy prefab. Leave empty to skip this attack.
    public int minionsPerCall = 6;
    public float minionRingRadius = 2.5f;

    [Header("Enrage")]
    [Range(0f, 1f)] public float enrageAtHealth = 0.5f;
    public float enrageSpeedMultiplier = 1.35f;
    public Color enrageTint = new Color(1f, 0.55f, 0.55f);

    [Header("Rewards")]
    public int bonusCoins = 15; // Extra coins burst out on death, on top of the EnemyController drop.

    private enum Attack { None, Charge, Nova, BroodCall, Spiral }

    // Turns a regular enemy (normally the Green Bee) into the Brood Queen. Used by the editor prefab tool, and at runtime
    // by the EnemySpawner when no boss prefab is assigned. Call it before the object's Awake runs (e.g. on an inactive object).
    public static BossController ConfigureAsBroodQueen(GameObject go, GameObject minion)
    {
        go.name = "Boss - Brood Queen";
        go.transform.localScale = Vector3.one * 3.5f; // Scales the collider with it.

        EnemyController ec = go.GetComponent<EnemyController>();
        ec.health = 750f;
        ec.damage = 20f;
        ec.moveSpeed = 1.6f;
        ec.hitWaitTime = 0.8f;
        ec.knockBackTime = 0f; // Bosses don't get pushed around.
        ec.expToGive = 60;
        ec.coinValue = 5;
        ec.coinDropRate = 1f;

        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        if (body != null) body.mass = 50f; // Minions bump off her instead of shoving her.

        SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
        {
            sr.color = new Color(0.85f, 0.65f, 1f); // Royal violet, distinct from the regular bees.
            sr.sortingOrder += 5; // Drawn above her brood.
        }

        EnemyAnimation anim = go.GetComponent<EnemyAnimation>();
        if (anim != null)
        {
            anim.speed = 0.2f; // Slow, heavy breathing instead of the bees' jitter.
            anim.minSize = 0.95f;
            anim.maxSize = 1.05f;
        }

        BossController boss = go.GetComponent<BossController>();
        if (boss == null) boss = go.AddComponent<BossController>();
        boss.minionPrefab = minion;
        return boss;
    }

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
    private float ghostPct = 1f;

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
        target = PlayerHealthController.instance.transform;
        BuildHealthBar();
        StartCoroutine(Brain());
    }

    void Update()
    {
        if (!enraged && !enragePending && enemy.health <= enemy.MaxHealth * enrageAtHealth)
        {
            enragePending = true; // Handled by the brain between attacks so an attack is never cut in half.
        }
        UpdateHealthBar();
    }

    void OnDestroy()
    {
        if (healthBarRoot != null) Destroy(healthBarRoot.gameObject);
    }

    private bool PlayerAlive => PlayerHealthController.instance != null && PlayerHealthController.instance.gameObject.activeSelf;

    private Vector2 DirToPlayer => ((Vector2)(target.position - transform.position)).normalized;

    // ---------------------------------------------------------------- brain

    private IEnumerator Brain()
    {
        yield return Roar(1.2f, false); // Entrance.

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
                case Attack.Charge: yield return Charge(); break;
                case Attack.Nova: yield return Nova(); break;
                case Attack.BroodCall: yield return BroodCall(); break;
                case Attack.Spiral: yield return Spiral(); break;
            }
        }
    }

    private Attack PickAttack(Attack last) // Random, never the same attack twice in a row.
    {
        Attack[] pool = enraged
            ? new[] { Attack.Charge, Attack.Nova, Attack.BroodCall, Attack.Spiral, Attack.Charge }
            : new[] { Attack.Charge, Attack.Nova, Attack.BroodCall, Attack.Nova };

        for (int tries = 0; tries < 10; tries++)
        {
            Attack pick = pool[Random.Range(0, pool.Length)];
            if (pick == last) continue;
            if (pick == Attack.BroodCall && (minionPrefab == null || EnemySpawner.instance == null)) continue;
            return pick;
        }
        return last == Attack.Nova ? Attack.Charge : Attack.Nova;
    }

    // ---------------------------------------------------------------- attacks

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
        for (int c = 0; c < charges; c++)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 dir = DirToPlayer;
            float lane = chargeSpeed * SpeedMul * chargeDuration;
            float windup = c == 0 ? chargeWindup : chargeWindup * 0.6f; // The follow-up charge comes faster.

            telegraph.enabled = true;
            for (float t = 0f; t < windup; t += Time.deltaTime)
            {
                if (t < windup * 0.7f) dir = DirToPlayer; // Track, then lock so the player can sidestep.
                telegraph.SetPosition(0, transform.position);
                telegraph.SetPosition(1, (Vector2)transform.position + dir * lane);
                float blink = Mathf.PingPong(t * 8f, 1f);
                telegraph.startColor = telegraph.endColor = new Color(1f, 0.15f, 0.1f, Mathf.Lerp(0.15f, 0.5f, blink));
                Flash(new Color(1f, 0.3f, 0.3f), blink);
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
        SFXManager.instance.PlaySFXPitched(1);

        if (enraged)
        {
            yield return new WaitForSeconds(0.35f);
            FireRing(novaBullets, offset + 180f / novaBullets); // Fills the gaps of the first ring.
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
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator BroodCall()
    {
        rb.linearVelocity = Vector2.zero;
        for (float t = 0f; t < 0.7f; t += Time.deltaTime)
        {
            Flash(new Color(0.75f, 0.45f, 1f), Mathf.PingPong(t * 5f, 1f));
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
        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator Enrage()
    {
        enraged = true;
        yield return Roar(1.3f, true);
        baseColor = enrageTint;
        ResetBody();
        if (healthFillImage != null) healthFillImage.color = new Color(0.95f, 0.2f, 0.2f);
        FireRing(novaBullets + 6, Random.Range(0f, 360f)); // Shockwave.
    }

    private IEnumerator Roar(float duration, bool angry) // Invulnerable pause with a shake: entrance and phase change.
    {
        rb.linearVelocity = Vector2.zero;
        enemy.invulnerable = true;
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            Shake(angry ? 0.12f : 0.07f);
            Flash(angry ? new Color(1f, 0.2f, 0.2f) : Color.white, Mathf.PingPong(t * 4f, 1f) * 0.6f);
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

    // ---------------------------------------------------------------- death

    private void OnDied(EnemyController e)
    {
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        telegraph.enabled = false;
        BossProjectile.ClearAll();

        if (CoinController.instance != null)
        {
            for (int i = 0; i < bonusCoins; i++)
            {
                CoinController.instance.DropCoin(transform.position + (Vector3)(Random.insideUnitCircle * 2f), enemy.coinValue);
            }
        }

        if (healthBarRoot != null) Destroy(healthBarRoot.gameObject);
        ShowBanner("BOSS DEFEATED", new Color(1f, 0.85f, 0.3f));
        if (EnemySpawner.instance != null) EnemySpawner.instance.OnBossDefeated();
    }

    // ---------------------------------------------------------------- visuals

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

    private void BuildTelegraph() // The red lane shown before a charge.
    {
        GameObject go = new GameObject("Charge Telegraph");
        go.transform.SetParent(transform, false);
        telegraph = go.AddComponent<LineRenderer>();
        telegraph.useWorldSpace = true;
        telegraph.positionCount = 2;
        telegraph.startWidth = 1.4f;
        telegraph.endWidth = 1.0f;
        telegraph.numCapVertices = 4;
        telegraph.material = new Material(Shader.Find("Sprites/Default"));
        if (body != null)
        {
            telegraph.sortingLayerID = body.sortingLayerID;
            telegraph.sortingOrder = body.sortingOrder - 1; // Under the boss.
        }
        telegraph.enabled = false;
    }

    // ---------------------------------------------------------------- UI

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
        go.GetComponent<Image>().color = color;
        go.GetComponent<Image>().raycastTarget = false;
        return (RectTransform)go.transform;
    }

    private void BuildHealthBar() // Top-centre bar under the timer: name, dark frame, white "recent damage" ghost, coloured fill.
    {
        Canvas canvas = FindHudCanvas();
        if (canvas == null) return;

        GameObject root = new GameObject("Boss Health Bar", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        healthBarRoot = (RectTransform)root.transform;
        healthBarRoot.anchorMin = healthBarRoot.anchorMax = new Vector2(0.5f, 1f);
        healthBarRoot.pivot = new Vector2(0.5f, 1f);
        healthBarRoot.anchoredPosition = new Vector2(0f, -86f);
        healthBarRoot.sizeDelta = new Vector2(640f, 56f);

        RectTransform nameRt = MakeText(root.transform, "Boss Name", bossName, 30, new Color(1f, 0.85f, 0.3f)).rectTransform;
        nameRt.anchorMin = new Vector2(0f, 1f);
        nameRt.anchorMax = new Vector2(1f, 1f);
        nameRt.pivot = new Vector2(0.5f, 1f);
        nameRt.anchoredPosition = Vector2.zero;
        nameRt.sizeDelta = new Vector2(0f, 30f);

        RectTransform frame = MakeBar(root.transform, "Frame", new Color(0.05f, 0.04f, 0.08f, 0.9f));
        frame.anchorMin = new Vector2(0f, 0f);
        frame.anchorMax = new Vector2(1f, 0f);
        frame.pivot = new Vector2(0.5f, 0f);
        frame.anchoredPosition = Vector2.zero;
        frame.sizeDelta = new Vector2(0f, 20f);

        healthGhost = MakeBar(frame, "Ghost", new Color(1f, 1f, 1f, 0.75f));
        healthFill = MakeBar(frame, "Fill", new Color(0.62f, 0.3f, 0.95f));
        healthFillImage = healthFill.GetComponent<Image>();
        foreach (RectTransform bar in new[] { healthGhost, healthFill })
        {
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = Vector2.one;
            bar.offsetMin = new Vector2(3f, 3f);
            bar.offsetMax = new Vector2(-3f, -3f);
        }

        ShowBanner("WARNING: " + bossName + " APPROACHES", new Color(1f, 0.3f, 0.25f));
    }

    private void UpdateHealthBar()
    {
        if (healthFill == null) return;
        float pct = enemy.MaxHealth > 0f ? Mathf.Clamp01(enemy.health / enemy.MaxHealth) : 0f;
        ghostPct = Mathf.MoveTowards(ghostPct, pct, Time.deltaTime * 0.35f); // The white chunk drains behind the real bar.
        if (ghostPct < pct) ghostPct = pct;
        healthFill.anchorMax = new Vector2(pct, 1f);
        healthGhost.anchorMax = new Vector2(ghostPct, 1f);
    }

    private void ShowBanner(string text, Color color) // Centre-screen message that fades out on its own.
    {
        Canvas canvas = FindHudCanvas();
        if (canvas == null) return;
        TMP_Text banner = MakeText(canvas.transform, "Boss Banner", text, 56, color);
        banner.rectTransform.anchorMin = banner.rectTransform.anchorMax = new Vector2(0.5f, 0.68f);
        banner.rectTransform.sizeDelta = new Vector2(1400f, 80f);
        banner.gameObject.AddComponent<BossBannerFade>();
    }
}

// Fades and lifts a banner, then removes it. Lives on the banner so it outlives the boss.
public class BossBannerFade : MonoBehaviour
{
    public float duration = 3f;
    private float t;
    private TMP_Text text;

    void Awake() { text = GetComponent<TMP_Text>(); }

    void Update()
    {
        t += Time.deltaTime;
        if (text != null) text.alpha = t < duration * 0.6f ? 1f : Mathf.InverseLerp(duration, duration * 0.6f, t);
        transform.localPosition += Vector3.up * 12f * Time.deltaTime;
        if (t >= duration) Destroy(gameObject);
    }
}
