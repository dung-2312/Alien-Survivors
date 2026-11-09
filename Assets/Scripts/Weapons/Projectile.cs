using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;

    [Header("Visual Effects (Vampire Survivors Magic Beam)")]
    public bool enableMagicTrail = true;
    public Color trailColorHead = new Color(0.9f, 0.98f, 1.0f, 0.95f);
    public Color trailColorBody = new Color(0.0f, 0.82f, 1.0f, 0.85f);
    public Color trailColorTail = new Color(0.05f, 0.35f, 0.95f, 0.0f);

    private TrailRenderer trailRenderer;
    private float sparkleTimer = 0f;
    private static Sprite sparkleStarSprite;
    private static Material trailMaterial;

    void Awake()
    {
        // Phóng to kích thước đạn để nổi bật và uy lực chuẩn Vampire Survivors
        if (transform.localScale.magnitude < 1.4f)
        {
            transform.localScale = Vector3.one * 1.55f;
        }

        SetupMagicTrail();
    }

    void Start()
    {
        // Đổi màu Sprite đầu đạn rực sáng ánh năng lượng xanh lam ma thuật
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0.7f, 0.95f, 1.0f, 1f);
            sr.sortingOrder = 16;
        }
    }

    private void SetupMagicTrail()
    {
        if (!enableMagicTrail) return;

        trailRenderer = GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
        }

        if (trailMaterial == null)
        {
            Shader s = Shader.Find("Sprites/Default");
            if (s != null) trailMaterial = new Material(s);
        }

        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }

        trailRenderer.time = 0.22f;
        trailRenderer.minVertexDistance = 0.06f;
        trailRenderer.sortingOrder = 14;

        // Vuốt thon dần từ đầu đạn về đuôi (Tapered Width Curve)
        AnimationCurve widthCurve = new AnimationCurve();
        widthCurve.AddKey(0f, 0.42f);
        widthCurve.AddKey(0.4f, 0.28f);
        widthCurve.AddKey(1f, 0.0f);
        trailRenderer.widthCurve = widthCurve;

        // Dải màu gradient từ trắng sáng -> xanh ngọc lam -> xanh sapphire tan dần
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(trailColorBody, 0.4f),
                new GradientColorKey(trailColorTail, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.95f, 0.0f),
                new GradientAlphaKey(0.80f, 0.45f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        trailRenderer.colorGradient = grad;
    }

    void Update()
    {
        // Di chuyển đạn
        transform.position += transform.up * moveSpeed * Time.deltaTime;

        // Tạo bầy hạt bụi sao lấp lánh (Sparkling Stardust Motes) rơi lại phía sau như Vampire Survivors
        if (enableMagicTrail)
        {
            sparkleTimer -= Time.deltaTime;
            if (sparkleTimer <= 0f)
            {
                sparkleTimer = 0.038f;
                Vector3 offset = -transform.up * 0.18f + (Vector3)(Random.insideUnitCircle * 0.14f);
                SpawnSparkleMote(transform.position + offset);
            }
        }
    }

    private void SpawnSparkleMote(Vector3 pos)
    {
        GameObject star = new GameObject("StardustMote", typeof(SpriteRenderer));
        star.transform.position = pos;
        star.transform.localScale = Vector3.one * Random.Range(0.22f, 0.42f);
        star.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

        SpriteRenderer sr = star.GetComponent<SpriteRenderer>();
        sr.sprite = GetSparkleStarSprite();
        sr.color = Color.Lerp(new Color(0.4f, 0.95f, 1.0f, 0.95f), Color.white, Random.value);
        sr.sortingOrder = 15;

        // Mote tự tan biến mượt mà trong 0.25 giây
        StartCoroutine(FadeOutAndDestroy(star, 0.24f));
    }

    private IEnumerator FadeOutAndDestroy(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 initialScale = obj.transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (obj != null)
            {
                obj.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
                if (sr != null)
                {
                    Color c = sr.color;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    sr.color = c;
                }
            }
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private static Sprite GetSparkleStarSprite()
    {
        if (sparkleStarSprite != null) return sparkleStarSprite;

        int size = 16;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        float c = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - c);
                float dy = Mathf.Abs(y - c);
                // Ngôi sao 4 cánh nhọn (Diamond 4-Point Star)
                float starDist = Mathf.Min(dx * 2.5f + dy * 0.5f, dx * 0.5f + dy * 2.5f);
                if (starDist <= c)
                {
                    float alpha = Mathf.Clamp01(1f - (starDist / c));
                    p[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    p[y * size + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        sparkleStarSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return sparkleStarSprite;
    }
}
