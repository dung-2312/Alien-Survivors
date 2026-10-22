using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseAttackWeapon : Weapon
{
    public EnemyDamager damager; // Giữ để tương thích dữ liệu stats từ prefab
    public LayerMask whatIsEnemy;
    private readonly Collider2D[] enemyBuffer = new Collider2D[32];
    private float attackCounter;

    private static Sprite crescentSlashSprite;

    void Start()
    {
        SetStats();
        // Ẩn vĩnh viễn damager tĩnh cũ từ prefab (loại bỏ hoàn toàn các thanh kiếm vụn vặt đâm ra dưới chân)
        if (damager != null)
        {
            damager.gameObject.SetActive(false);
            SpriteRenderer oldSr = damager.GetComponentInChildren<SpriteRenderer>();
            if (oldSr != null) oldSr.enabled = false;
        }
    }

    void Update()
    {
        if (statsUpdated)
        {
            statsUpdated = false;
            SetStats();
        }

        attackCounter -= Time.deltaTime;
        if (attackCounter <= 0f)
        {
            attackCounter = stats[weaponLevel].timeBetweenAttacks;
            ExecuteSwordSlashCombo();
        }
    }

    /// <summary>
    /// Thực hiện chuỗi vung kiếm chém ra Kiếm Khí Nguyệt Nha trực tiếp từ tay nhân vật
    /// </summary>
    private void ExecuteSwordSlashCombo()
    {
        // 1. Xác định vị trí xuất chiêu từ tay cầm kiếm của nhân vật (ngang ngực/tay, không bao giờ ở dưới chân)
        Vector3 playerCenter = PlayerController.instance != null
            ? PlayerController.instance.transform.position + Vector3.up * 0.25f
            : transform.position;

        float facingDir = PlayerController.instance != null ? PlayerController.instance.facingDirection : 1f;

        // 2. Tìm quái vật gần nhất để chém chuẩn xác theo hướng mục tiêu
        Transform closestEnemy = null;
        float minDistanceSqr = float.MaxValue;
        float searchRadius = stats[weaponLevel].range * 5.0f;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(whatIsEnemy.value != 0 ? whatIsEnemy : LayerMask.GetMask("Enemy", "Enemies", "Default"));
        filter.useLayerMask = true;

        int count = Physics2D.OverlapCircle(playerCenter, searchRadius, filter, enemyBuffer);
        for (int c = 0; c < count; c++)
        {
            if (enemyBuffer[c] != null && enemyBuffer[c].CompareTag("Enemy"))
            {
                float distSqr = (enemyBuffer[c].transform.position - playerCenter).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closestEnemy = enemyBuffer[c].transform;
                }
            }
        }

        // 3. Tính góc chém kiếm khí
        float baseAngle = 0f;
        if (closestEnemy != null)
        {
            Vector3 diff = closestEnemy.position - playerCenter;
            baseAngle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        }
        else
        {
            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (moveInput.sqrMagnitude > 0.01f)
            {
                baseAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            }
            else
            {
                baseAngle = facingDir < 0 ? 180f : 0f;
            }
        }

        // 4. Kích hoạt hiệu ứng nhân vật vung kiếm (Character Attack Lean)
        if (PlayerController.instance != null)
        {
            PlayerController.instance.TriggerAttackSwing(Mathf.Cos(baseAngle * Mathf.Deg2Rad));
        }

        // 5. Chuỗi nhát chém combo theo cấp độ vũ khí
        // Cấp 1: 1 nhát chém nguyệt nha quét rộng phía trước
        // Cấp 2-3: Combo chém 2 nhát liên hoàn (Chém Thuận + Chém Nghịch)
        // Cấp 4+: Combo chém chữ X bão kiếm quét sạch quân thù
        int comboStrikes = Mathf.Clamp((int)stats[weaponLevel].amount, 1, 3);
        StartCoroutine(PerformSlashRoutine(playerCenter, baseAngle, comboStrikes));
    }

    private IEnumerator PerformSlashRoutine(Vector3 playerCenter, float baseAngle, int comboStrikes)
    {
        for (int i = 0; i < comboStrikes; i++)
        {
            float angleOffset = (comboStrikes > 1) ? (i % 2 == 0 ? -14f : 14f) : 0f;
            float currentAngle = baseAngle + angleOffset;
            bool flipArc = (i % 2 == 1);

            SpawnCrescentSlash(playerCenter, currentAngle, flipArc);

            if (SFXManager.instance != null)
            {
                SFXManager.instance.PlaySFXPitched(9);
            }

            if (comboStrikes > 1)
            {
                yield return new WaitForSeconds(0.09f);
            }
        }
    }

    private void SpawnCrescentSlash(Vector3 origin, float angleDeg, bool flipY)
    {
        Vector3 dir = new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0f);
        Vector3 spawnPos = origin + dir * 0.55f; // Xuất phát trực tiếp từ lưỡi kiếm của nhân vật

        GameObject slashObj = new GameObject("CrescentSwordSlash");
        slashObj.transform.position = spawnPos;
        slashObj.transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);

        SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateCrescentSlashSprite();
        sr.flipY = flipY;
        sr.color = new Color(0.95f, 0.98f, 1f, 0.95f);
        sr.sortingLayerName = "Default";
        sr.sortingOrder = 10;

        float rangeScale = stats[weaponLevel].range;
        slashObj.transform.localScale = Vector3.one * (1.3f * rangeScale);

        // Collider bao phủ toàn bộ vùng quét kiếm khí
        BoxCollider2D col = slashObj.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.offset = new Vector2(0.35f, 0f);
        col.size = new Vector2(1.2f, 1.4f);

        // Gắn EnemyDamager để gây sát thương và đẩy lùi quái
        EnemyDamager ed = slashObj.AddComponent<EnemyDamager>();
        ed.damageAmount = stats[weaponLevel].damage;
        ed.lifeTime = 0.22f;
        ed.shouldKnockBack = true;
        ed.destroyOnImpact = false; // Chém xuyên qua tất cả quái vật trong đường kiếm!
        ed.growSpeed = 8f;

        // Vệt kiếm lướt tới trước và fade mờ dần
        SwordSlashMover mover = slashObj.AddComponent<SwordSlashMover>();
        mover.Init(dir * 5.5f, 0.22f, rangeScale);
    }

    void SetStats()
    {
        if (damager != null)
        {
            damager.damageAmount = stats[weaponLevel].damage;
            damager.lifeTime = stats[weaponLevel].duration;
            damager.transform.localScale = Vector3.one * stats[weaponLevel].range;
        }
        attackCounter = 0f;
    }

    // ==========================================
    // PROCEDURAL CRESCENT BLADE ARC SPRITE
    // ==========================================
    public static Sprite GetOrCreateCrescentSlashSprite()
    {
        if (crescentSlashSprite != null) return crescentSlashSprite;

        int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];
        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        Color bladeWhite = new Color(1f, 1f, 1f, 1f);
        Color auraCyan = new Color(0.3f, 0.88f, 1f, 0.95f);
        Color trailBlue = new Color(0.15f, 0.5f, 0.95f, 0.65f);

        float cx = 14f, cy = 24f; // Tâm cung kiếm nằm bên trái (gần tay cầm)
        float rOuter = 26f, rInner = 18f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist >= rInner && dist <= rOuter && dx > 0f)
                {
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (Mathf.Abs(angle) <= 65f)
                    {
                        float angleNorm = 1f - Mathf.Abs(angle) / 65f;
                        float thickDist = Mathf.Abs(dist - (rInner + rOuter) * 0.5f) / ((rOuter - rInner) * 0.5f);

                        if (thickDist <= 0.45f && angleNorm > 0.15f)
                        {
                            p[y * size + x] = bladeWhite; // Lưỡi kiếm trắng sáng sắc lẹm
                        }
                        else if (thickDist <= 0.85f)
                        {
                            p[y * size + x] = auraCyan; // Hào quang kiếm khí xanh băng ngọc
                        }
                        else
                        {
                            p[y * size + x] = trailBlue; // Vệt xé gió phía sau
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        crescentSlashSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.2f, 0.5f), 16f);
        return crescentSlashSprite;
    }
}

public class SwordSlashMover : MonoBehaviour
{
    private Vector3 velocity;
    private float lifeTime;
    private float timer = 0f;
    private SpriteRenderer sr;
    private Vector3 baseScale;

    public void Init(Vector3 vel, float dur, float rangeScale)
    {
        velocity = vel;
        lifeTime = dur;
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 6f);

        // Vệt kiếm xòe to dần về phía trước
        float progress = Mathf.Clamp01(timer / lifeTime);
        transform.localScale = Vector3.Lerp(baseScale * 0.8f, baseScale * 1.35f, progress);

        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, progress * progress);
            sr.color = c;
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
