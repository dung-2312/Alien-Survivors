using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShrineType
{
    FrostChronos,  // Đóng băng toàn bộ quái vật trên màn hình
    MidasWealth,   // Phun trào mưa vàng và nhân đôi vàng
    BloodMoonBerserk // Cuồng nộ tăng 100% sát thương và triệu hồi quái để càn quét
}

public class AncientShrine : MonoBehaviour
{
    [Header("Cấu hình Đàn Tế")]
    public ShrineType shrineType;
    public float triggerRadius = 1.35f;
    private bool isActivated = false;

    private SpriteRenderer sr;
    private Transform runeTransform;
    private Color shrineColor;

    public void InitShrine(ShrineType type)
    {
        shrineType = type;
        SetupVisuals();
    }

    void Awake()
    {
        SetupVisuals();
    }

    private void SetupVisuals()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        // Mượn sprite hòm hoặc viên ngọc làm biểu tượng đàn tế
        if (sr.sprite == null && CoinController.instance != null && CoinController.instance.coin != null)
        {
            sr.sprite = CoinController.instance.coin.GetComponent<SpriteRenderer>()?.sprite;
        }

        switch (shrineType)
        {
            case ShrineType.FrostChronos:
                shrineColor = new Color(0.1f, 0.9f, 1f, 1f); // Xanh băng tuyết
                break;
            case ShrineType.MidasWealth:
                shrineColor = new Color(1f, 0.85f, 0.1f, 1f); // Hoàng kim
                break;
            case ShrineType.BloodMoonBerserk:
                shrineColor = new Color(1f, 0.2f, 0.35f, 1f); // Đỏ huyết nguyệt
                break;
        }

        sr.color = shrineColor;
        sr.sortingOrder = 5;
        transform.localScale = Vector3.one * 1.8f;

        // Tạo vòng sáng hào quang xung quanh
        if (transform.Find("Aura") == null)
        {
            GameObject aura = new GameObject("Aura", typeof(SpriteRenderer));
            aura.transform.SetParent(transform);
            aura.transform.localPosition = Vector3.zero;
            aura.transform.localScale = Vector3.one * 1.6f;
            SpriteRenderer asr = aura.GetComponent<SpriteRenderer>();
            asr.sprite = sr.sprite;
            asr.color = new Color(shrineColor.r, shrineColor.g, shrineColor.b, 0.35f);
            asr.sortingOrder = 4;
            runeTransform = aura.transform;
        }
    }

    void Update()
    {
        if (isActivated) return;

        // Hiệu ứng phát sáng và bồng bềnh rune
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.15f;
        transform.localScale = Vector3.one * (1.8f * pulse);

        if (runeTransform != null)
        {
            runeTransform.Rotate(0, 0, 45f * Time.deltaTime);
        }

        // Kiểm tra Player bước vào đàn tế
        if (PlayerController.instance != null)
        {
            float dist = Vector2.Distance(transform.position, PlayerController.instance.transform.position);
            if (dist <= triggerRadius)
            {
                ActivateShrine();
            }
        }
    }

    private void ActivateShrine()
    {
        isActivated = true;

        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        if (CameraController.instance != null) CameraController.instance.Shake(0.25f, 0.35f);

        switch (shrineType)
        {
            case ShrineType.FrostChronos:
                ActivateFrostChronos();
                break;

            case ShrineType.MidasWealth:
                ActivateMidasWealth();
                break;

            case ShrineType.BloodMoonBerserk:
                ActivateBloodMoonBerserk();
                break;
        }

        StartCoroutine(DestroyShrineEffect());
    }

    private void ActivateFrostChronos()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("[ CHRONOS SHRINE: ALL ENEMIES FROZEN IN ICE! ]", 2.2f);
        }

        // Đóng băng toàn bộ quái vật hiện có trên sân
        EnemyController[] allEnemies = FindObjectsByType<EnemyController>();
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.StartCoroutine(FreezeEnemyRoutine(enemy, 6.5f));
            }
        }
    }

    private IEnumerator FreezeEnemyRoutine(EnemyController enemy, float duration)
    {
        if (enemy == null) yield break;
        float originalSpeed = enemy.moveSpeed;
        enemy.moveSpeed = 0f;
        if (enemy.theRB != null) enemy.theRB.linearVelocity = Vector2.zero;

        SpriteRenderer esr = enemy.GetComponentInChildren<SpriteRenderer>();
        Color originalColor = esr != null ? esr.color : Color.white;
        if (esr != null) esr.color = new Color(0.2f, 0.8f, 1f, 1f);

        yield return new WaitForSeconds(duration);

        if (enemy != null)
        {
            enemy.moveSpeed = originalSpeed;
            if (esr != null) esr.color = originalColor;
        }
    }

    private void ActivateMidasWealth()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("[ MIDAS SHRINE: GOLDEN FOUNTAIN & 2X COINS! ]", 2.2f);
        }

        // Phun trào mưa vàng xung quanh đàn tế
        if (CoinController.instance != null)
        {
            int coinCount = 28;
            for (int i = 0; i < coinCount; i++)
            {
                Vector2 spread = Random.insideUnitCircle * Random.Range(1f, 4f);
                CoinController.instance.DropCoin(transform.position + (Vector3)spread, 5);
            }
        }
    }

    private void ActivateBloodMoonBerserk()
    {
        if (UIController.instance != null)
        {
            UIController.instance.ShowWarning("[ BLOOD MOON BERSERK: +100% DAMAGE ACTIVATED! ]", 2.2f);
        }

        // Buff cuồng chiến cho player trong 12 giây
        if (PlayerController.instance != null)
        {
            PlayerController.instance.StartCoroutine(BerserkBuffRoutine(12f));
        }

        // Triệu hồi 1 đàn quái nhỏ để người chơi thỏa sức càn quét
        if (EnemySpawner.instance != null)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 spawnOffset = Random.insideUnitCircle.normalized * Random.Range(7f, 11f);
                // Tạo quái nếu EnemySpawner có phương thức
            }
        }
    }

    private IEnumerator BerserkBuffRoutine(float duration)
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null) yield break;

        float origSpeed = pc.moveSpeed;
        pc.moveSpeed *= 1.35f;

        // Buff sát thương cho các vũ khí đã trang bị
        foreach (var wp in pc.assignedWeapons)
        {
            if (wp != null && wp.stats != null && wp.weaponLevel < wp.stats.Count)
            {
                wp.stats[wp.weaponLevel].damage *= 2f;
            }
        }

        yield return new WaitForSeconds(duration);

        if (pc != null)
        {
            pc.moveSpeed = origSpeed;
            foreach (var wp in pc.assignedWeapons)
            {
                if (wp != null && wp.stats != null && wp.weaponLevel < wp.stats.Count)
                {
                    wp.stats[wp.weaponLevel].damage /= 2f;
                }
            }
        }
    }

    private IEnumerator DestroyShrineEffect()
    {
        float t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, startScale * 2.5f, t / 0.4f);
            if (sr != null) sr.color = new Color(shrineColor.r, shrineColor.g, shrineColor.b, 1f - (t / 0.4f));
            yield return null;
        }
        Destroy(gameObject);
    }
}
