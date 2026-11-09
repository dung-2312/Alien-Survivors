using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý hiệu ứng hạt & va chạm bùng nổ (Procedural VFX & Impact Debris).
/// Tạo ra các vụ nổ mảnh vụn đá, bụi sao lấp lánh khi nhặt đồ, và vòng sóng xung kích khi chí mạng.
/// Hoạt động 100% độc lập, tự sinh texture procedural không phụ thuộc file ngoài.
/// </summary>
public class VFXImpactManager : MonoBehaviour
{
    public static VFXImpactManager instance;

    private static Sprite starSparkleSprite;
    private static Sprite circleRingSprite;
    private static Sprite shardSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0) return;

        if (instance == null)
        {
            GameObject go = new GameObject("VFXImpactManager", typeof(VFXImpactManager));
            DontDestroyOnLoad(go);
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // PUBLIC VFX TRIGGERS
    // =========================================================================

    /// <summary>
    /// Hiệu ứng bụi sao lấp lánh khi nhặt Ngọc Kinh Nghiệm hoặc Vàng
    /// </summary>
    public static void PlayPickupSparkles(Vector3 pos, Color color, int count = 7)
    {
        EnsureInstance();
        instance.StartCoroutine(instance.SparkleBurstRoutine(pos, color, count));
    }

    /// <summary>
    /// Hiệu ứng mảnh vỡ nổ tung khi đánh vỡ Bia Mộ, Bình Gốm, hoặc Quan Tài Đá
    /// </summary>
    public static void PlayDebrisExplosion(Vector3 pos, Color color, int count = 10)
    {
        EnsureInstance();
        instance.StartCoroutine(instance.DebrisBurstRoutine(pos, color, count));
    }

    /// <summary>
    /// Vòng sóng xung kích mở rộng khi ra đòn Bạo Kích (Critical Hit) hoặc kích hoạt Đàn Tế (Shrine)
    /// </summary>
    public static void PlayShockwaveRing(Vector3 pos, Color color, float maxRadius = 3.0f, float duration = 0.38f)
    {
        EnsureInstance();
        instance.StartCoroutine(instance.ShockwaveRingRoutine(pos, color, maxRadius, duration));
    }

    private static void EnsureInstance()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("VFXImpactManager", typeof(VFXImpactManager));
            instance = go.GetComponent<VFXImpactManager>();
            DontDestroyOnLoad(go);
        }
    }

    // =========================================================================
    // COROUTINES ANIMATIONS
    // =========================================================================

    private IEnumerator SparkleBurstRoutine(Vector3 center, Color color, int count)
    {
        Sprite sparkle = GetOrCreateStarSparkleSprite();

        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("SparkleParticle", typeof(SpriteRenderer));
            p.transform.position = center;
            p.transform.localScale = Vector3.one * Random.Range(0.20f, 0.40f);

            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            sr.sprite = sparkle;
            sr.color = color;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 20;

            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float speed = Random.Range(2.5f, 6.0f);

            StartCoroutine(AnimateSparkle(p, sr, dir * speed));
        }

        yield return null;
    }

    private IEnumerator AnimateSparkle(GameObject p, SpriteRenderer sr, Vector2 velocity)
    {
        float lifetime = Random.Range(0.35f, 0.55f);
        float elapsed = 0f;
        Color initialColor = sr.color;
        Vector3 initialScale = p.transform.localScale;

        while (elapsed < lifetime)
        {
            if (p == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            p.transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity = Vector2.MoveTowards(velocity, Vector2.zero, Time.deltaTime * 8f);

            // Spin & scale down
            p.transform.Rotate(0f, 0f, 180f * Time.deltaTime);
            p.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

            if (sr != null)
            {
                sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0f, t));
            }

            yield return null;
        }

        if (p != null) Destroy(p);
    }

    private IEnumerator DebrisBurstRoutine(Vector3 center, Color color, int count)
    {
        Sprite shard = GetOrCreateShardSprite();

        for (int i = 0; i < count; i++)
        {
            GameObject p = new GameObject("DebrisShard", typeof(SpriteRenderer));
            p.transform.position = center + (Vector3)(Random.insideUnitCircle * 0.15f);
            p.transform.localScale = Vector3.one * Random.Range(0.25f, 0.55f);

            SpriteRenderer sr = p.GetComponent<SpriteRenderer>();
            sr.sprite = shard;
            // Varied earthy tints
            sr.color = Color.Lerp(color, new Color(0.3f, 0.25f, 0.2f), Random.Range(0f, 0.6f));
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 18;

            float angle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            float speed = Random.Range(3.0f, 7.5f);

            StartCoroutine(AnimateDebris(p, sr, dir * speed));
        }

        yield return null;
    }

    private IEnumerator AnimateDebris(GameObject p, SpriteRenderer sr, Vector2 velocity)
    {
        float lifetime = Random.Range(0.40f, 0.65f);
        float elapsed = 0f;
        float rotSpeed = Random.Range(-360f, 360f);

        while (elapsed < lifetime)
        {
            if (p == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // Simple pseudo-gravity downward
            velocity.y -= 9.8f * Time.deltaTime;
            p.transform.position += (Vector3)(velocity * Time.deltaTime);
            p.transform.Rotate(0f, 0f, rotSpeed * Time.deltaTime);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                sr.color = c;
            }

            yield return null;
        }

        if (p != null) Destroy(p);
    }

    private IEnumerator ShockwaveRingRoutine(Vector3 pos, Color color, float maxRadius, float duration)
    {
        GameObject ring = new GameObject("ShockwaveRing", typeof(SpriteRenderer));
        ring.transform.position = pos;
        ring.transform.localScale = Vector3.zero;

        SpriteRenderer sr = ring.GetComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateCircleRingSprite();
        sr.color = color;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 15;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (ring == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float currentRadius = Mathf.Lerp(0.1f, maxRadius, Mathf.Sin(t * Mathf.PI * 0.5f));
            ring.transform.localScale = Vector3.one * currentRadius;

            if (sr != null)
            {
                sr.color = new Color(color.r, color.g, color.b, Mathf.Lerp(color.a, 0f, t));
            }

            yield return null;
        }

        if (ring != null) Destroy(ring);
    }

    // =========================================================================
    // PROCEDURAL TEXTURE GENERATORS
    // =========================================================================

    private static Sprite GetOrCreateStarSparkleSprite()
    {
        if (starSparkleSprite != null) return starSparkleSprite;

        const int size = 24;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[size * size];

        Color solid = Color.white;
        Color clear = Color.clear;
        int mid = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - mid);
                int dy = Mathf.Abs(y - mid);

                // 4-pointed diamond star
                if (dx + dy <= 7 || (dx <= 1 && dy <= 10) || (dy <= 1 && dx <= 10))
                {
                    px[y * size + x] = solid;
                }
                else
                {
                    px[y * size + x] = clear;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        starSparkleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 50f);
        return starSparkleSprite;
    }

    private static Sprite GetOrCreateCircleRingSprite()
    {
        if (circleRingSprite != null) return circleRingSprite;

        const int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[size * size];

        float center = (size - 1) * 0.5f;
        float outerR = center;
        float innerR = center - 4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist <= outerR && dist >= innerR)
                {
                    float alpha = 1f - Mathf.Abs(dist - (innerR + outerR) * 0.5f) / 2f;
                    px[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                }
                else
                {
                    px[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        circleRingSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
        return circleRingSprite;
    }

    private static Sprite GetOrCreateShardSprite()
    {
        if (shardSprite != null) return shardSprite;

        const int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Jagged irregular shard shape
                if (x + y >= 4 && x + y <= 22 && x >= 2 && y <= 13)
                {
                    px[y * size + x] = Color.white;
                }
                else
                {
                    px[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        shardSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 50f);
        return shardSprite;
    }
}
