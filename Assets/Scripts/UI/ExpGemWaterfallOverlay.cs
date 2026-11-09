using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hiệu ứng Thác Kim Cương Kinh Nghiệm (EXP Gem Waterfall) cuồn cuộn chảy phía sau khung Level Up chuẩn Vampire Survivors
/// </summary>
public class ExpGemWaterfallOverlay : MonoBehaviour
{
    private struct WaterfallGem
    {
        public RectTransform rt;
        public Image img;
        public float speedY;
        public float swaySpeed;
        public float swayWidth;
        public float swayOffset;
        public float rotSpeed;
        public float currentRot;
        public float baseX;
    }

    private readonly List<WaterfallGem> gems = new List<WaterfallGem>();
    private const int GEM_COUNT = 85;
    private static Sprite gemDiamondSprite;

    private static readonly Color[] gemColors = new[]
    {
        new Color(0.12f, 0.95f, 1.0f, 0.95f),  // Lam ngọc Cyan sáng
        new Color(1.0f, 0.18f, 0.32f, 0.95f),   // Huyết ngọc Crimson đỏ thắm
        new Color(0.15f, 1.0f, 0.50f, 0.95f),  // Lục bảo Emerald xanh
        new Color(0.35f, 0.65f, 1.0f, 0.95f),  // Lam sapphire sâu thẳm
        new Color(1.0f, 0.85f, 0.20f, 0.95f)   // Kim cương vàng kim
    };

    void Awake()
    {
        CreateGemSprite();
        SpawnWaterfallGems();
    }

    private void CreateGemSprite()
    {
        if (gemDiamondSprite != null) return;

        int size = 20;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        float c = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float manhattan = Mathf.Abs(x - c) + Mathf.Abs(y - c);
                if (manhattan > c + 0.8f)
                {
                    p[y * size + x] = Color.clear;
                }
                else if (manhattan >= c - 1.2f)
                {
                    p[y * size + x] = new Color(0.1f, 0.08f, 0.15f, 0.9f); // Viền đen hắc thiết
                }
                else
                {
                    // Lõi kim cương có điểm sáng góc trên trái
                    if (x <= c && y >= c)
                        p[y * size + x] = Color.white;
                    else
                        p[y * size + x] = new Color(0.85f, 0.85f, 0.85f, 1f);
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        gemDiamondSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    private void SpawnWaterfallGems()
    {
        for (int i = 0; i < GEM_COUNT; i++)
        {
            GameObject go = new GameObject("WaterfallGem_" + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            float startX = Random.Range(-960f, 960f);
            float startY = Random.Range(-580f, 620f);
            rt.anchoredPosition = new Vector2(startX, startY);

            float size = Random.Range(16f, 32f);
            rt.sizeDelta = new Vector2(size, size);

            Image img = go.GetComponent<Image>();
            img.sprite = gemDiamondSprite;
            img.color = gemColors[Random.Range(0, gemColors.Length)];
            img.raycastTarget = false;

            gems.Add(new WaterfallGem
            {
                rt = rt,
                img = img,
                speedY = Random.Range(550f, 950f), // Tốc độ thác đổ cực nhanh
                swaySpeed = Random.Range(3.5f, 7.5f),
                swayWidth = Random.Range(12f, 30f),
                swayOffset = Random.Range(0f, Mathf.PI * 2f),
                rotSpeed = Random.Range(-240f, 240f),
                currentRot = Random.Range(0f, 360f),
                baseX = startX
            });
        }
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime; // Chạy bất chấp thời gian game pause (Time.timeScale = 0)

        for (int i = 0; i < gems.Count; i++)
        {
            WaterfallGem gem = gems[i];
            if (gem.rt == null) continue;

            Vector2 pos = gem.rt.anchoredPosition;
            pos.y -= gem.speedY * dt;
            pos.x = gem.baseX + Mathf.Sin(Time.unscaledTime * gem.swaySpeed + gem.swayOffset) * gem.swayWidth;

            // Xoay kim cương lấp lánh khi rơi
            gem.currentRot += gem.rotSpeed * dt;
            gem.rt.localRotation = Quaternion.Euler(0, 0, gem.currentRot);

            // Tái sinh kim cương trên đỉnh khi rơi chạm đáy màn hình
            if (pos.y < -620f)
            {
                pos.y = 620f;
                gem.baseX = Random.Range(-960f, 960f);
                pos.x = gem.baseX;
                gem.speedY = Random.Range(550f, 950f);
                if (gem.img != null)
                {
                    gem.img.color = gemColors[Random.Range(0, gemColors.Length)];
                }
            }

            gem.rt.anchoredPosition = pos;
            gems[i] = gem;
        }
    }
}
