using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialPickupType
{
    HealthMeat,    // Hồi 35 HP
    Magnet,        // Hút toàn bộ EXP/Coin trên bản đồ
    HolyBomb,      // Bom hủy diệt quét sạch toàn bộ quái vật trên màn hình
    TimeFreeze,    // Đồng hồ cát đóng băng thời gian quái vật 5s
    GoldPouch      // Túi vàng rực rỡ +50 Coins ngay lập tức
}

public class SpecialPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public SpecialPickupType pickupType = SpecialPickupType.HealthMeat;
    public float healAmount = 35f;
    public float magnetAttractSpeed = 8f;

    private bool isMovingToPlayer = false;
    private Transform playerTransform;
    private PlayerController playerController;
    private Vector3 initialScale;

    // Static cached procedural sprites
    private static Sprite meatSprite;
    private static Sprite magnetSprite;
    private static Sprite bombSprite;
    private static Sprite freezeSprite;
    private static Sprite goldPouchSprite;

    private void Awake()
    {
        initialScale = transform.localScale;
        if (initialScale == Vector3.zero) initialScale = Vector3.one;
    }

    private void Start()
    {
        if (PlayerController.instance != null)
        {
            playerController = PlayerController.instance;
            playerTransform = PlayerController.instance.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            if (PlayerController.instance != null)
            {
                playerController = PlayerController.instance;
                playerTransform = PlayerController.instance.transform;
            }
            return;
        }

        // Nhẹ nhàng bay về phía player khi ở trong bán kính nhặt đồ
        if (!isMovingToPlayer && playerController != null)
        {
            float pickupRangeSqr = playerController.pickupRange * playerController.pickupRange;
            if ((transform.position - playerTransform.position).sqrMagnitude <= pickupRangeSqr)
            {
                isMovingToPlayer = true;
            }
        }

        if (isMovingToPlayer)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                (magnetAttractSpeed + (playerController != null ? playerController.moveSpeed : 0f)) * Time.deltaTime
            );
        }
        else
        {
            // Hiệu ứng bồng bềnh nhẹ (Bobbing) & xoay lấp lánh khi nằm trên sàn
            float bobOffset = Mathf.Sin(Time.time * 4f) * 0.08f;
            transform.localScale = initialScale * (1f + bobOffset * 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        VFXImpactManager.PlayPickupSparkles(transform.position, new Color(1f, 0.9f, 0.3f), 10);
        VFXImpactManager.PlayShockwaveRing(transform.position, new Color(0.85f, 0.95f, 1f), 2.5f, 0.3f);

        switch (pickupType)
        {
            case SpecialPickupType.HealthMeat:
                if (PlayerHealthController.instance != null)
                {
                    PlayerHealthController.instance.Heal(healAmount);
                }
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(2);
                if (DamageNumberController.instance != null && playerTransform != null)
                {
                    DamageNumberController.instance.SpawnDamage(healAmount, playerTransform.position + Vector3.up * 0.6f, false);
                }
                break;

            case SpecialPickupType.Magnet:
                if (ExpManager.instance != null)
                {
                    ExpManager.instance.AttractAllGems();
                }
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(1);
                if (UIController.instance != null) UIController.instance.ShowWarning("[ VACUUM MAGNET ACTIVATED! ]", 1.8f);
                break;

            case SpecialPickupType.HolyBomb:
                ExecuteHolyNuke();
                break;

            case SpecialPickupType.TimeFreeze:
                EnemyController.FreezeAllEnemies(5.0f);
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(8);
                break;

            case SpecialPickupType.GoldPouch:
                if (CoinController.instance != null)
                {
                    CoinController.instance.AddCoins(50);
                }
                if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(2);
                if (UIController.instance != null) UIController.instance.ShowWarning("[ GOLDEN POUCH: +50 COINS! ]", 1.8f);
                break;
        }

        Destroy(gameObject);
    }

    private void ExecuteHolyNuke()
    {
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        if (CameraController.instance != null) CameraController.instance.Shake(0.45f, 0.6f);

        // Quét tiêu diệt tất cả quái vật thường trong màn hình
        Vector3 center = (playerTransform != null) ? playerTransform.position : transform.position;
        Collider2D[] cols = Physics2D.OverlapCircleAll(center, 18f);
        int killed = 0;
        foreach (var c in cols)
        {
            if (c != null && c.TryGetComponent<EnemyController>(out var enemy))
            {
                if (!enemy.isMiniBoss && enemy.health > 0f)
                {
                    enemy.TakeDamage(9999f);
                    killed++;
                }
            }
        }

        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning($"[ HOLY NUKE! {killed} ENEMIES PURGED! ]", 2.2f);
        }
    }

    /// <summary>
    /// Factory tạo và thả một vật phẩm đặc biệt tại vị trí bất kỳ
    /// </summary>
    public static GameObject SpawnPickup(Vector3 position, SpecialPickupType type)
    {
        GameObject go = new GameObject("Pickup_" + type, typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(SpecialPickup));
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 1.3f;

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForType(type);
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 5;

        CircleCollider2D col = go.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        SpecialPickup pickup = go.GetComponent<SpecialPickup>();
        pickup.pickupType = type;

        return go;
    }

    public static Sprite GetSpriteForType(SpecialPickupType type)
    {
        switch (type)
        {
            case SpecialPickupType.HealthMeat: return GetOrCreateMeatSprite();
            case SpecialPickupType.Magnet: return GetOrCreateMagnetSprite();
            case SpecialPickupType.HolyBomb: return GetOrCreateBombSprite();
            case SpecialPickupType.TimeFreeze: return GetOrCreateFreezeSprite();
            case SpecialPickupType.GoldPouch: return GetOrCreateGoldPouchSprite();
            default: return GetOrCreateMeatSprite();
        }
    }

    // PROCEDURAL PIXEL ART SPRITES
    public static Sprite GetOrCreateMeatSprite()
    {
        if (meatSprite != null) return meatSprite;
        return meatSprite = EnvironmentDecorManager.GetOrCreateMeatSprite();
    }

    public static Sprite GetOrCreateMagnetSprite()
    {
        if (magnetSprite != null) return magnetSprite;
        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color red = new Color(0.9f, 0.2f, 0.2f);
        Color blue = new Color(0.2f, 0.45f, 0.95f);
        Color silver = new Color(0.85f, 0.88f, 0.92f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 12f) / 9f;
                float dy = (y - 12f) / 9f;
                float dOuter = dx * dx + dy * dy;
                float dInner = (dx / 0.55f) * (dx / 0.55f) + (dy / 0.55f) * (dy / 0.55f);

                if (dOuter <= 1f && dInner >= 1f && y >= 5)
                {
                    if (y <= 9) p[y * w + x] = silver;
                    else if (x < 12) p[y * w + x] = red;
                    else p[y * w + x] = blue;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(p);
        tex.Apply();
        return magnetSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite GetOrCreateBombSprite()
    {
        if (bombSprite != null) return bombSprite;
        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color bombBlack = new Color(0.18f, 0.18f, 0.22f);
        Color bombShine = new Color(0.55f, 0.58f, 0.65f);
        Color goldCap = new Color(1f, 0.85f, 0.2f);
        Color spark = new Color(1f, 0.45f, 0.1f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 11f) / 7.5f;
                float dy = (y - 9f) / 7.5f;
                if (dx * dx + dy * dy <= 1f)
                {
                    if (dx < -0.3f && dy > 0.2f) p[y * w + x] = bombShine;
                    else p[y * w + x] = bombBlack;
                }
                else if (x >= 9 && x <= 13 && y >= 17 && y <= 19)
                {
                    p[y * w + x] = goldCap;
                }
                else if ((x == 14 && y == 20) || (x == 15 && y == 21))
                {
                    p[y * w + x] = spark;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(p);
        tex.Apply();
        return bombSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite GetOrCreateFreezeSprite()
    {
        if (freezeSprite != null) return freezeSprite;
        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color gold = new Color(0.95f, 0.8f, 0.25f);
        Color frostCyan = new Color(0.35f, 0.9f, 1f, 0.95f);
        Color glass = new Color(0.7f, 0.9f, 1f, 0.45f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if ((y == 3 || y == 20) && x >= 5 && x <= 18)
                {
                    p[y * w + x] = gold;
                }
                else if (y >= 4 && y <= 19)
                {
                    int span = Mathf.RoundToInt(Mathf.Abs(y - 11.5f) / 8f * 6f) + 1;
                    if (Mathf.Abs(x - 11.5f) <= span)
                    {
                        if (y <= 7 || (y >= 14 && y <= 16)) p[y * w + x] = frostCyan;
                        else p[y * w + x] = glass;
                    }
                    else
                    {
                        p[y * w + x] = Color.clear;
                    }
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(p);
        tex.Apply();
        return freezeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    public static Sprite GetOrCreateGoldPouchSprite()
    {
        if (goldPouchSprite != null) return goldPouchSprite;
        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color pouchBrown = new Color(0.75f, 0.55f, 0.28f);
        Color pouchDark = new Color(0.55f, 0.38f, 0.18f);
        Color redRibbon = new Color(0.9f, 0.2f, 0.25f);
        Color goldCoin = new Color(1f, 0.88f, 0.2f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 12f) / 7.5f;
                float dy = (y - 9f) / 6.5f;
                if (dx * dx + dy * dy <= 1f)
                {
                    p[y * w + x] = (dy < -0.3f || dx > 0.3f) ? pouchDark : pouchBrown;
                }
                else if (y == 16 && x >= 8 && x <= 16)
                {
                    p[y * w + x] = redRibbon;
                }
                else if (y >= 17 && y <= 20 && x >= 9 && x <= 15)
                {
                    p[y * w + x] = goldCoin;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }
        tex.SetPixels(p);
        tex.Apply();
        return goldPouchSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }
}
