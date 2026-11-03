using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PuddleType
{
    Acid,       // Vũng axit xanh lục: Gây sát thương theo thời gian (4 DPS lên người chơi, 20 DPS lên quái vật)
    HolyWater,  // Vũng nước thánh ánh hoàng kim/lam ngọc: Hồi máu người chơi (+6 HP/s), thiêu đốt quái vật (25 DPS)
    MudTar,     // Vũng bùn hắc ín lầy lội: Giảm 40% tốc độ di chuyển của mọi thực thể bước vào
    BloodPool   // Vũng huyết tế: Cường hóa sát thương người chơi (+30% Might trong 4 giây)
}

public class PuddleProp : MonoBehaviour
{
    [Header("Puddle Configuration")]
    public PuddleType puddleType = PuddleType.Acid;
    public float radius = 1.35f;

    private SpriteRenderer sr;
    private CircleCollider2D col;
    private float effectTickTimer = 0f;
    private const float TICK_INTERVAL = 0.5f;

    // Track entities currently inside the puddle
    private readonly HashSet<EnemyController> enemiesInside = new HashSet<EnemyController>();
    private bool playerInside = false;

    // Static cached sprites
    private static readonly Dictionary<PuddleType, Sprite> cachedSprites = new Dictionary<PuddleType, Sprite>();

    public void InitPuddle(PuddleType type)
    {
        puddleType = type;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sprite = GetOrCreatePuddleSprite(type);
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -90; // Nằm trên nền cỏ, dưới mọi nhân vật & quái

        col = GetComponent<CircleCollider2D>();
        if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = radius;

        transform.localScale = Vector3.one * Random.Range(1.1f, 1.45f);

        // Thêm gợn sóng nhấp nhô nhẹ tạo cảm giác chất lỏng chuyển động
        StartCoroutine(PuddleWobbleRoutine());
    }

    private void Update()
    {
        effectTickTimer += Time.deltaTime;
        if (effectTickTimer >= TICK_INTERVAL)
        {
            effectTickTimer = 0f;
            ApplyPeriodicEffects();
        }
    }

    private void ApplyPeriodicEffects()
    {
        // 1. Áp dụng hiệu ứng lên người chơi
        if (playerInside && PlayerHealthController.instance != null)
        {
            switch (puddleType)
            {
                case PuddleType.Acid:
                    // Sát thương nhẹ nếu người chơi dẫm vào axit
                    PlayerHealthController.instance.TakeDamage(2f);
                    break;

                case PuddleType.HolyWater:
                    // Hồi máu thánh thiện
                    PlayerHealthController.instance.Heal(3f);
                    break;

                case PuddleType.BloodPool:
                    // Kích hoạt buff Cuồng Bạo (Blood Frenzy)
                    ApplyBloodFrenzyToPlayer();
                    break;
            }
        }

        // 2. Áp dụng hiệu ứng lên quái vật trong vũng
        if (enemiesInside.Count > 0)
        {
            List<EnemyController> toRemove = null;
            foreach (var enemy in enemiesInside)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.health <= 0f)
                {
                    if (toRemove == null) toRemove = new List<EnemyController>();
                    toRemove.Add(enemy);
                    continue;
                }

                switch (puddleType)
                {
                    case PuddleType.Acid:
                        // Axit ăn mòn quái vật cực mạnh
                        enemy.TakeDamage(10f, false, false);
                        break;

                    case PuddleType.HolyWater:
                        // Nước thánh thiêu đốt quái ác quỷ
                        enemy.TakeDamage(14f, false, true);
                        break;
                }
            }

            if (toRemove != null)
            {
                foreach (var e in toRemove) enemiesInside.Remove(e);
            }
        }
    }

    private void ApplyBloodFrenzyToPlayer()
    {
        if (PlayerController.instance != null)
        {
            PlayerController.instance.ApplyBloodFrenzy(4.0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            if (puddleType == PuddleType.MudTar && PlayerController.instance != null)
            {
                PlayerController.instance.theRB.linearDamping = 6f; // Tăng ma sát làm chậm
            }
            else if (puddleType == PuddleType.BloodPool)
            {
                ApplyBloodFrenzyToPlayer();
            }
        }
        else
        {
            EnemyController ec = collision.GetComponent<EnemyController>() ?? collision.GetComponentInParent<EnemyController>();
            if (ec != null && !enemiesInside.Contains(ec))
            {
                enemiesInside.Add(ec);
                if (puddleType == PuddleType.MudTar)
                {
                    ec.moveSpeed *= 0.6f; // Làm chậm quái vật 40% để thả diều chiến thuật
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            if (puddleType == PuddleType.MudTar && PlayerController.instance != null)
            {
                PlayerController.instance.theRB.linearDamping = 0f; // Khôi phục ma sát bình thường
            }
        }
        else
        {
            EnemyController ec = collision.GetComponent<EnemyController>() ?? collision.GetComponentInParent<EnemyController>();
            if (ec != null && enemiesInside.Contains(ec))
            {
                enemiesInside.Remove(ec);
                if (puddleType == PuddleType.MudTar)
                {
                    ec.moveSpeed /= 0.6f; // Khôi phục tốc độ di chuyển
                }
            }
        }
    }

    private IEnumerator PuddleWobbleRoutine()
    {
        Vector3 baseScale = transform.localScale;
        float offset = Random.Range(0f, Mathf.PI * 2f);

        while (true)
        {
            float wobble = Mathf.Sin(Time.time * 2.2f + offset) * 0.035f;
            transform.localScale = new Vector3(baseScale.x * (1f + wobble), baseScale.y * (1f - wobble), 1f);
            yield return null;
        }
    }

    // ==========================================
    // PROCEDURAL PIXEL SPRITE CREATION
    // ==========================================

    public static Sprite GetOrCreatePuddleSprite(PuddleType type)
    {
        if (cachedSprites.TryGetValue(type, out var s) && s != null) return s;

        int w = 64, h = 40;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] pixels = new Color[w * h];

        Color deepCol;
        Color shallowCol;
        Color highlightCol;
        Color mudRimCol;

        switch (type)
        {
            case PuddleType.Acid:
                deepCol = new Color(0.08f, 0.44f, 0.12f, 0.88f);
                shallowCol = new Color(0.24f, 0.72f, 0.18f, 0.65f);
                highlightCol = new Color(0.78f, 1.0f, 0.45f, 0.90f);
                mudRimCol = new Color(0.14f, 0.12f, 0.08f, 0.75f);
                break;
            case PuddleType.HolyWater:
                deepCol = new Color(0.10f, 0.36f, 0.75f, 0.85f);
                shallowCol = new Color(0.30f, 0.68f, 0.95f, 0.65f);
                highlightCol = new Color(0.92f, 0.96f, 1.0f, 0.92f);
                mudRimCol = new Color(0.12f, 0.14f, 0.15f, 0.75f);
                break;
            case PuddleType.MudTar:
                deepCol = new Color(0.07f, 0.05f, 0.04f, 0.94f);
                shallowCol = new Color(0.20f, 0.15f, 0.12f, 0.78f);
                highlightCol = new Color(0.42f, 0.35f, 0.28f, 0.75f);
                mudRimCol = new Color(0.12f, 0.09f, 0.07f, 0.82f);
                break;
            case PuddleType.BloodPool:
            default:
                deepCol = new Color(0.42f, 0.02f, 0.05f, 0.92f);
                shallowCol = new Color(0.75f, 0.06f, 0.12f, 0.70f);
                highlightCol = new Color(1.0f, 0.45f, 0.45f, 0.90f);
                mudRimCol = new Color(0.16f, 0.06f, 0.06f, 0.78f);
                break;
        }

        float cx = w * 0.5f;
        float cy = h * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - cx) / (cx * 0.88f);
                float dy = (y - cy) / (cy * 0.82f);
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Đường biên lượn sóng tự nhiên đa tần số
                float wobble = 1.0f + 0.14f * Mathf.Sin(angle * 3f + 1.2f)
                                    + 0.08f * Mathf.Cos(angle * 5f - 0.7f)
                                    + 0.05f * Mathf.Sin(angle * 7f + 2.1f);
                float normDist = dist / wobble;

                if (normDist > 1.20f)
                {
                    pixels[y * w + x] = Color.clear;
                }
                else if (normDist > 1.0f)
                {
                    // Viền đất bùn ẩm ướt chuyển sắc mềm mại vào nền cỏ
                    float rimT = (normDist - 1.0f) / 0.20f;
                    float alpha = Mathf.Lerp(mudRimCol.a, 0f, rimT);
                    float grit = ((x * 17 + y * 31) % 7 == 0) ? -0.04f : 0.03f;
                    pixels[y * w + x] = new Color(
                        Mathf.Clamp01(mudRimCol.r + grit),
                        Mathf.Clamp01(mudRimCol.g + grit),
                        Mathf.Clamp01(mudRimCol.b + grit),
                        alpha
                    );
                }
                else
                {
                    // Mặt nước bên trong: chuyển sắc theo độ sâu
                    float depth = 1.0f - normDist;
                    Color liquid = Color.Lerp(shallowCol, deepCol, Mathf.SmoothStep(0f, 1f, depth));

                    // Vệt phản quang ánh sáng mặt nước tự nhiên (Specular sheen arc)
                    float specArc = Mathf.Exp(-Mathf.Pow((normDist - 0.52f) / 0.15f, 2f)) * Mathf.Clamp01(dy * 1.6f + 0.35f);
                    float ripple = Mathf.Sin(normDist * 16f - 0.8f);
                    if (specArc > 0.40f && ripple > 0.35f)
                    {
                        float blend = Mathf.Clamp01((specArc - 0.40f) * 2.2f);
                        liquid = Color.Lerp(liquid, highlightCol, blend * 0.70f);
                    }

                    // Đốm sáng phản chiếu phụ
                    float glint = Mathf.Exp(-Mathf.Pow(dx - 0.25f, 2f) * 12f - Mathf.Pow(dy - 0.18f, 2f) * 24f);
                    if (glint > 0.5f)
                    {
                        liquid = Color.Lerp(liquid, highlightCol, (glint - 0.5f) * 1.5f);
                    }

                    pixels[y * w + x] = liquid;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        Sprite res = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 20f);
        cachedSprites[type] = res;
        return res;
    }
}
