using System.Collections;
using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    [Header("Chest Configuration")]
    public int chestLevel = 1; // 1 = Mini-Boss (1-3 items), 2 = Major Boss (3-5 items)
    private bool isCollected = false;
    private Vector3 initialScale;
    private SpriteRenderer sr;

    private void Awake()
    {
        initialScale = transform.localScale;
        if (initialScale == Vector3.zero) initialScale = Vector3.one * 1.5f;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
        }

        if (sr.sprite == null)
        {
            // Tận dụng sprite hòm vàng / túi kho báu pixel art từ Resources
            sr.sprite = Resources.Load<Sprite>("UI/UI_Icon_CoinSack") ?? Resources.Load<Sprite>("UI/UI_Coin_Box");
        }

        sr.sortingOrder = 5;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.isTrigger = true;
        col.radius = 0.8f;
    }

    private void Update()
    {
        if (isCollected) return;

        // Hiệu ứng bồng bềnh và phát sáng nhẹ nhàng
        float floatOffset = Mathf.Sin(Time.time * 4f) * 0.08f;
        transform.localScale = initialScale * (1f + floatOffset * 0.35f);

        if (sr != null)
        {
            // Nhấp nháy ánh vàng lung linh
            float glow = Mathf.PingPong(Time.time * 2f, 0.4f);
            sr.color = new Color(1f, 0.85f + glow * 0.35f, 0.3f + glow * 0.5f, 1f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected) return;

        if (collision.CompareTag("Player"))
        {
            isCollected = true;
            CollectChest();
        }
    }

    private void CollectChest()
    {
        if (TreasureChestUI.instance != null)
        {
            TreasureChestUI.instance.OpenChest(chestLevel);
        }

        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(0);
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// Hàm tiện ích spawn hòm kho báu tại tọa độ mong muốn.
    /// </summary>
    public static GameObject SpawnChest(Vector3 position, int level = 1)
    {
        GameObject chestObj = new GameObject("TreasureChest_" + (level > 1 ? "Boss" : "MiniBoss"));
        chestObj.transform.position = position;
        TreasureChest chest = chestObj.AddComponent<TreasureChest>();
        chest.chestLevel = level;
        return chestObj;
    }
}
