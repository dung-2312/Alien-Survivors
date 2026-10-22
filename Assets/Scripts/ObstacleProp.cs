using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Gravestone,     // Bia má»™ cá»•
    AncientObelisk, // Cá»™t Ä‘Ã¡ cá»• Ä‘áº¡i kiÃªn cá»‘
    Boulder,        // Táº£ng Ä‘Ã¡ rÃªu phong
    AncientUrn      // BÃ¬nh gá»‘m / HÃ²m kho bÃ¡u chá»©a váº­t pháº©m
}

public class ObstacleProp : MonoBehaviour
{
    [Header("Obstacle Settings")]
    public ObstacleType obstacleType = ObstacleType.Gravestone;
    public bool isBreakable = true;
    public float health = 30f;
    public float maxHealth = 30f;

    [Header("Drops")]
    [Range(0f, 1f)] public float coinDropChance = 0.85f;
    [Range(0f, 1f)] public float healMeatChance = 0.35f;
    public int minCoins = 2;
    public int maxCoins = 6;

    private SpriteRenderer sr;
    private Collider2D solidCollider;
    private Color originalColor = Color.white;
    private bool isDestroyed = false;
    private Coroutine flashCo;

    public void InitProp(ObstacleType type, Sprite sprite, bool breakable = true)
    {
        obstacleType = type;
        isBreakable = breakable;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        if (sprite != null)
        {
            sr.sprite = sprite;
        }

        sr.sortingLayerName = "Default";
        sr.sortingOrder = 2;

        switch (type)
        {
            case ObstacleType.AncientObelisk:
                maxHealth = 9999f;
                isBreakable = false;
                transform.localScale = Vector3.one * 1.3f;
                break;

            case ObstacleType.Boulder:
                maxHealth = 9999f;
                isBreakable = false;
                transform.localScale = Vector3.one * 1.25f;
                break;

            case ObstacleType.Gravestone:
                maxHealth = 40f;
                isBreakable = true;
                transform.localScale = Vector3.one * 1.15f;
                break;

            case ObstacleType.AncientUrn:
                maxHealth = 15f;
                isBreakable = true;
                transform.localScale = Vector3.one * 1.05f;
                break;
        }

        health = maxHealth;
        originalColor = sr.color;

        SetupCollider();
        CreateGroundShadow();
    }

    private void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        originalColor = sr != null ? sr.color : Color.white;
        SetupCollider();
    }

    private void SetupCollider()
    {
        solidCollider = GetComponent<Collider2D>();
        if (solidCollider == null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            // Vá»‹ trÃ­ va cháº¡m náº±m á»Ÿ pháº§n chÃ¢n cá»§a váº­t cáº£n Ä‘á»ƒ nhÃ¢n váº­t vÃ  quÃ¡i cÃ³ cáº£m giÃ¡c chiá»u sÃ¢u 2.5D
            box.offset = new Vector2(0f, 0.25f);
            box.size = new Vector2(0.85f, 0.5f);
            solidCollider = box;
        }

        // ThÃªm 1 Trigger collider phá»¥ Ä‘á»ƒ nháº­n sÃ¡t thÆ°Æ¡ng tá»« vÅ© khÃ­ ngÆ°á»i chÆ¡i
        if (isBreakable)
        {
            CircleCollider2D triggerCol = gameObject.AddComponent<CircleCollider2D>();
            triggerCol.isTrigger = true;
            triggerCol.radius = 0.75f;
        }
    }

    private void CreateGroundShadow()
    {
        if (transform.Find("Shadow") != null) return;

        GameObject shadowObj = new GameObject("Shadow", typeof(SpriteRenderer));
        shadowObj.transform.SetParent(transform, false);
        shadowObj.transform.localPosition = Vector3.zero;
        shadowObj.transform.localScale = new Vector3(1.15f, 0.45f, 1f);

        SpriteRenderer ssr = shadowObj.GetComponent<SpriteRenderer>();
        ssr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
        ssr.color = new Color(0f, 0f, 0f, 0.45f);
        ssr.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        ssr.sortingOrder = (sr != null ? sr.sortingOrder : 2) - 1;
    }

    public void TakeDamage(float damageAmount)
    {
        if (!isBreakable || isDestroyed) return;

        health -= damageAmount;

        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(HitFlashRoutine());

        // Ã‚m thanh chÃ©m trÃºng Ä‘Ã¡/gá»—
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        if (health <= 0f)
        {
            BreakAndLoot();
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (sr == null) yield break;

        sr.color = Color.white * 2.2f;
        transform.localPosition += (Vector3)(Random.insideUnitCircle * 0.06f);

        yield return new WaitForSeconds(0.08f);

        if (sr != null) sr.color = originalColor;
    }

    private void BreakAndLoot()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // 1. RÆ¡i tiá»n vÃ ng
        if (CoinController.instance != null && Random.value <= coinDropChance)
        {
            int coinsToDrop = Random.Range(minCoins, maxCoins + 1);
            CoinController.instance.DropCoin(transform.position, coinsToDrop);
        }

        // 2. Rơi vật phẩm chiến thuật đặc sắc
        float lootRoll = Random.value;
        if (lootRoll < 0.28f)
        {
            SpecialPickup.SpawnPickup(transform.position, SpecialPickupType.HealthMeat);
        }
        else if (lootRoll < 0.40f)
        {
            SpecialPickup.SpawnPickup(transform.position, SpecialPickupType.GoldPouch);
        }
        else if (lootRoll < 0.48f)
        {
            SpecialPickup.SpawnPickup(transform.position, SpecialPickupType.Magnet);
        }
        else if (lootRoll < 0.54f)
        {
            SpecialPickup.SpawnPickup(transform.position, SpecialPickupType.TimeFreeze);
        }
        else if (lootRoll < 0.59f)
        {
            SpecialPickup.SpawnPickup(transform.position, SpecialPickupType.HolyBomb);
        }

        // 3. Hiá»‡u á»©ng vá»¡ tan thÃ nh máº£nh vá»¥n
        SpawnDebrisParticles();

        // 4. Rung mÃ n hÃ¬nh nháº¹ táº¡o lá»±c
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.12f, 0.15f);
        }

        Destroy(gameObject);
    }

    private void SpawnHealthPickup()
    {
        // Táº¡o váº­t pháº©m há»“i mÃ¡u ngay táº¡i vá»‹ trÃ­ vá»¡
        GameObject meatGO = new GameObject("HealthPickup_Meat", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpecialPickup));
        meatGO.transform.position = transform.position;

        SpriteRenderer msr = meatGO.GetComponent<SpriteRenderer>();
        msr.sprite = EnvironmentDecorManager.GetOrCreateMeatSprite();
        msr.color = new Color(1f, 0.4f, 0.4f, 1f);
        msr.sortingLayerName = "Default";
        msr.sortingOrder = 3;
        meatGO.transform.localScale = Vector3.one * 1.35f;

        CircleCollider2D mcol = meatGO.GetComponent<CircleCollider2D>();
        mcol.isTrigger = true;
        mcol.radius = 0.5f;

        SpecialPickup sp = meatGO.GetComponent<SpecialPickup>();
        sp.pickupType = SpecialPickupType.HealthMeat;
        sp.healAmount = 25f;
    }

    private void SpawnDebrisParticles()
    {
        int count = 6;
        for (int i = 0; i < count; i++)
        {
            GameObject fragment = new GameObject("Debris", typeof(SpriteRenderer));
            fragment.transform.position = transform.position + (Vector3)(Random.insideUnitCircle * 0.2f);
            fragment.transform.localScale = Vector3.one * Random.Range(0.2f, 0.45f);

            SpriteRenderer fsr = fragment.GetComponent<SpriteRenderer>();
            fsr.sprite = sr != null ? sr.sprite : null;
            fsr.color = Color.Lerp(originalColor, new Color(0.85f, 0.75f, 0.6f), Random.value);
            fsr.sortingOrder = 4;

            Vector2 flyDir = Random.insideUnitCircle.normalized * Random.Range(2.5f, 5.5f);
            fragment.AddComponent<DebrisFragment>().Init(flyDir, 0.45f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;
        EnemyDamager damager = collision.GetComponent<EnemyDamager>() ?? collision.GetComponentInParent<EnemyDamager>();
        if (damager != null)
        {
            TakeDamage(damager.damageAmount);
            if (damager.destroyOnImpact)
            {
                Destroy(damager.gameObject);
            }
        }
    }
}

public class DebrisFragment : MonoBehaviour
{
    private Vector2 velocity;
    private float lifeTime;
    private float timer = 0f;
    private SpriteRenderer sr;

    public void Init(Vector2 vel, float dur)
    {
        velocity = vel;
        lifeTime = dur;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 5f);
        transform.Rotate(0, 0, 360f * Time.deltaTime);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - (timer / lifeTime);
            sr.color = c;
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
