using System.Collections.Generic;
using UnityEngine;

// A bullet fired by a boss. Built at runtime so the boss needs no extra prefabs; hurts the player on touch.
public class BossProjectile : MonoBehaviour
{
    private static readonly List<BossProjectile> live = new List<BossProjectile>(); // Every bullet in flight, so a dying boss can clear them.
    private static Sprite orbSprite; // Generated once and shared by every bullet without a custom sprite.

    private float damage;
    private float lifeTime;

    public static BossProjectile Spawn(Vector3 position, Vector2 velocity, float damage, Sprite sprite, Color color,
        int sortingLayerID, int sortingOrder, float scale = 0.5f, float lifeTime = 6f)
    {
        GameObject go = new GameObject("Boss Spore");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite != null ? sprite : GetOrbSprite();
        sr.color = color;
        sr.sortingLayerID = sortingLayerID;
        sr.sortingOrder = sortingOrder;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.35f; // A little smaller than the glow so near misses feel fair.

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Needed for trigger callbacks; moved by velocity, unaffected by physics.
        rb.gravityScale = 0f;
        rb.linearVelocity = velocity;

        BossProjectile p = go.AddComponent<BossProjectile>();
        p.damage = damage;
        p.lifeTime = lifeTime;
        return p;
    }

    public static void ClearAll()
    {
        for (int i = live.Count - 1; i >= 0; i--)
        {
            if (live[i] != null) Destroy(live[i].gameObject);
        }
        live.Clear();
    }

    void OnEnable() { live.Add(this); }
    void OnDisable() { live.Remove(this); }

    void Update()
    {
        transform.Rotate(0f, 0f, 360f * Time.deltaTime); // Slow spin so the orb reads as alive.

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerHealthController.instance != null
            && PlayerHealthController.instance.gameObject.activeSelf)
        {
            PlayerHealthController.instance.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private static Sprite GetOrbSprite() // A soft round glow with a bright core, tinted per bullet.
    {
        if (orbSprite != null) return orbSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r; // 0 at centre, 1 at edge.
                float alpha = Mathf.Clamp01(1f - d);
                alpha = alpha * alpha * 1.6f;
                float core = d < 0.35f ? 1f : Mathf.Clamp01(1f - (d - 0.35f) * 2.5f); // Whiten the middle.
                tex.SetPixel(x, y, new Color(Mathf.Lerp(0.75f, 1f, core), Mathf.Lerp(0.75f, 1f, core), Mathf.Lerp(0.75f, 1f, core), Mathf.Clamp01(alpha)));
            }
        }
        tex.Apply();
        orbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size); // 1 world unit across.
        return orbSprite;
    }
}
