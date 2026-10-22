using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentDecorManager : MonoBehaviour
{
    public static EnvironmentDecorManager instance;

    [Header("Chunk Grid Settings")]
    public float chunkSize = 20f;
    public int viewRadiusInChunks = 2; // Sinh cÃ¡c chunk trong bÃ¡n kÃ­nh quanh Player

    [Header("Props per Chunk")]
    public int minObstaclesPerChunk = 2;
    public int maxObstaclesPerChunk = 4;
    public int minFlowersPerChunk = 4;
    public int maxFlowersPerChunk = 8;

    private Transform playerTransform;
    private readonly HashSet<Vector2Int> generatedChunks = new HashSet<Vector2Int>();
    private readonly Dictionary<Vector2Int, List<GameObject>> chunkObjects = new Dictionary<Vector2Int, List<GameObject>>();

    // Static cached sprites
    private static Sprite softShadowSprite;
    private static Sprite meatSprite;
    private static Sprite gravestoneSprite;
    private static Sprite obeliskSprite;
    private static Sprite boulderSprite;
    private static Sprite urnSprite;
    private static readonly Dictionary<int, Sprite> flowerSprites = new Dictionary<int, Sprite>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (PlayerController.instance != null)
        {
            playerTransform = PlayerController.instance.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            if (PlayerController.instance != null)
            {
                playerTransform = PlayerController.instance.transform;
            }
            return;
        }

        Vector2Int currentChunk = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / chunkSize),
            Mathf.FloorToInt(playerTransform.position.y / chunkSize)
        );

        // 1. Sinh cÃ¡c chunk má»›i trong táº§m nhÃ¬n cá»§a Player
        for (int x = -viewRadiusInChunks; x <= viewRadiusInChunks; x++)
        {
            for (int y = -viewRadiusInChunks; y <= viewRadiusInChunks; y++)
            {
                Vector2Int coord = new Vector2Int(currentChunk.x + x, currentChunk.y + y);
                if (!generatedChunks.Contains(coord))
                {
                    GenerateChunk(coord);
                }
            }
        }

        // 2. Dá»n dáº¹p cÃ¡c chunk quÃ¡ xa Ä‘á»ƒ tá»‘i Æ°u bá»™ nhá»› vÃ  hiá»‡u nÄƒng
        List<Vector2Int> chunksToClean = null;
        foreach (var kvp in chunkObjects)
        {
            Vector2Int coord = kvp.Key;
            if (Mathf.Abs(coord.x - currentChunk.x) > viewRadiusInChunks + 1 ||
                Mathf.Abs(coord.y - currentChunk.y) > viewRadiusInChunks + 1)
            {
                if (chunksToClean == null) chunksToClean = new List<Vector2Int>();
                chunksToClean.Add(coord);
            }
        }

        if (chunksToClean != null)
        {
            foreach (var coord in chunksToClean)
            {
                if (chunkObjects.TryGetValue(coord, out var list))
                {
                    foreach (var go in list)
                    {
                        if (go != null) Destroy(go);
                    }
                }
                chunkObjects.Remove(coord);
                generatedChunks.Remove(coord);
            }
        }
    }

    private void GenerateChunk(Vector2Int coord)
    {
        generatedChunks.Add(coord);
        List<GameObject> objectsInThisChunk = new List<GameObject>();

        // DÃ¹ng Random.State vá»›i seed xÃ¡c Ä‘á»‹nh Ä‘á»ƒ cÃ¡c váº­t cáº£n phÃ¢n bá»‘ cá»‘ Ä‘á»‹nh vÃ  tá»± nhiÃªn
        int seed = coord.x * 73856093 ^ coord.y * 19349663;
        Random.InitState(seed);

        Vector2 chunkOrigin = new Vector2(coord.x * chunkSize, coord.y * chunkSize);

        // A. Sinh cÃ¡c cá»¥m hoa dáº¡i sáº·c sá»¡ trang trÃ­ ná»n cá» (Background Decals)
        int flowerCount = Random.Range(minFlowersPerChunk, maxFlowersPerChunk + 1);
        for (int i = 0; i < flowerCount; i++)
        {
            Vector2 pos = chunkOrigin + new Vector2(Random.Range(1f, chunkSize - 1f), Random.Range(1f, chunkSize - 1f));
            int flowerType = Random.Range(0, 4); // 0: VÃ ng dáº¡i, 1: Äá» má»¹ nhÃ¢n tháº£o, 2: TÃ­m ma quÃ¡i, 3: Cá» 4 lÃ¡ ngá»c bÃ­ch

            GameObject flower = new GameObject("Flora_Wildflower", typeof(SpriteRenderer));
            flower.transform.position = pos;
            flower.transform.SetParent(transform, true);

            SpriteRenderer fsr = flower.GetComponent<SpriteRenderer>();
            fsr.sprite = GetOrCreateFlowerSprite(flowerType);
            fsr.sortingLayerName = "Default";
            fsr.sortingOrder = -95; // Náº±m sÃ¡t ngay trÃªn tháº£m cá», dÆ°á»›i táº¥t cáº£ nhÃ¢n váº­t & quÃ¡i
            flower.transform.localScale = Vector3.one * Random.Range(1.1f, 1.5f);

            objectsInThisChunk.Add(flower);
        }

        // KhÃ´ng sinh váº­t cáº£n ngay trung tÃ¢m xuáº¥t phÃ¡t (0, 0) Ä‘á»ƒ ngÆ°á»i chÆ¡i cÃ³ khoáº£ng trá»‘ng ban Ä‘áº§u
        if (coord == Vector2Int.zero)
        {
            chunkObjects[coord] = objectsInThisChunk;
            return;
        }

        // B. Sinh cÃ¡c váº­t cáº£n chiáº¿n thuáº­t (Tactical Obstacles)
        int obstacleCount = Random.Range(minObstaclesPerChunk, maxObstaclesPerChunk + 1);
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2 pos = chunkOrigin + new Vector2(Random.Range(2f, chunkSize - 2f), Random.Range(2f, chunkSize - 2f));
            ObstacleType type = (ObstacleType)Random.Range(0, 4);

            GameObject obs = new GameObject("Obstacle_" + type, typeof(ObstacleProp));
            obs.transform.position = pos;
            obs.transform.SetParent(transform, true);

            Sprite propSprite = null;
            bool breakable = true;

            switch (type)
            {
                case ObstacleType.Gravestone:
                    propSprite = GetOrCreateGravestoneSprite();
                    breakable = true;
                    break;
                case ObstacleType.AncientObelisk:
                    propSprite = GetOrCreateObeliskSprite();
                    breakable = false;
                    break;
                case ObstacleType.Boulder:
                    propSprite = GetOrCreateBoulderSprite();
                    breakable = false;
                    break;
                case ObstacleType.AncientUrn:
                    propSprite = GetOrCreateUrnSprite();
                    breakable = true;
                    break;
            }

            ObstacleProp propComp = obs.GetComponent<ObstacleProp>();
            propComp.InitProp(type, propSprite, breakable);

            objectsInThisChunk.Add(obs);
        }

        chunkObjects[coord] = objectsInThisChunk;
    }

    // ==========================================
    // PROCEDURAL PIXEL ART ASSET FACTORY
    // ==========================================

    public static Sprite GetOrCreateSoftShadowSprite()
    {
        if (softShadowSprite != null) return softShadowSprite;

        int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // HÃ¬nh elip má»m náº±m ngang
                float dx = (x - center) / center;
                float dy = ((y - center) / (center * 0.45f));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(1f - dist);
                alpha = Mathf.SmoothStep(0f, 1f, alpha) * 0.75f;
                pixels[y * size + x] = new Color(0.02f, 0.02f, 0.04f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        softShadowSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        return softShadowSprite;
    }

    public static Sprite GetOrCreateMeatSprite()
    {
        if (meatSprite != null) return meatSprite;

        int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color boneWhite = new Color(0.95f, 0.95f, 0.92f);
        Color meatRed = new Color(0.85f, 0.22f, 0.25f);
        Color roastBrown = new Color(0.6f, 0.15f, 0.12f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 14f) / 8f;
                float dy = (y - 12f) / 6f;
                if (dx * dx + dy * dy <= 1f)
                {
                    p[y * w + x] = (dx > 0.3f || dy < -0.3f) ? roastBrown : meatRed;
                }
                else if (x >= 4 && x <= 8 && y >= 10 && y <= 14)
                {
                    p[y * w + x] = boneWhite;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        meatSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        return meatSprite;
    }

    public static Sprite GetOrCreateFlowerSprite(int type)
    {
        if (flowerSprites.TryGetValue(type, out var s)) return s;

        int size = 20;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color petalColor = type == 0 ? new Color(1f, 0.88f, 0.15f) : // HoÃ ng hoa cÃºc
                           (type == 1 ? new Color(0.95f, 0.22f, 0.32f) : // Huyáº¿t hoa
                           (type == 2 ? new Color(0.72f, 0.35f, 1f) :    // Tá»­ Ä‘áº±ng hoa
                           new Color(0.2f, 0.85f, 0.45f)));              // Diá»‡p lá»¥c 4 lÃ¡

        Color centerColor = new Color(1f, 0.95f, 0.65f);
        Color stemGreen = new Color(0.12f, 0.45f, 0.18f);

        for (int i = 0; i < p.Length; i++) p[i] = Color.clear;

        // Váº½ 4 cÃ¡nh hoa & cuá»‘ng lÃ¡ nhá»
        int cx = 10, cy = 10;
        p[cy * size + cx] = centerColor;
        p[(cy + 1) * size + cx] = petalColor;
        p[(cy - 1) * size + cx] = petalColor;
        p[cy * size + (cx + 1)] = petalColor;
        p[cy * size + (cx - 1)] = petalColor;

        p[(cy - 2) * size + cx] = stemGreen;
        p[(cy - 3) * size + (cx - 1)] = stemGreen;
        p[(cy - 3) * size + (cx + 1)] = stemGreen;

        tex.SetPixels(p);
        tex.Apply();
        Sprite res = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        flowerSprites[type] = res;
        return res;
    }

    public static Sprite GetOrCreateGravestoneSprite()
    {
        if (gravestoneSprite != null) return gravestoneSprite;

        int w = 24, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color stoneDark = new Color(0.28f, 0.30f, 0.34f);
        Color stoneMid = new Color(0.42f, 0.45f, 0.50f);
        Color stoneLight = new Color(0.60f, 0.63f, 0.68f);
        Color mossGreen = new Color(0.25f, 0.45f, 0.22f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (x >= 4 && x <= 19 && y >= 3 && y <= 24)
                {
                    // VÃ²m bÃ¡n nguyá»‡t trÃªn Ä‘á»‰nh bia má»™
                    if (y >= 20 && (x < 6 || x > 17))
                    {
                        p[y * w + x] = Color.clear;
                        continue;
                    }

                    // Äiá»ƒm rÃªu phong á»Ÿ chÃ¢n má»™
                    if (y <= 7 && (x + y) % 3 == 0)
                    {
                        p[y * w + x] = mossGreen;
                    }
                    else if (x == 4 || y == 24)
                    {
                        p[y * w + x] = stoneLight;
                    }
                    else if (x == 19 || y == 3)
                    {
                        p[y * w + x] = stoneDark;
                    }
                    else
                    {
                        // Chá»¯ tháº­p thÃ¡nh kháº¯c má» trÃªn bia má»™
                        if ((x >= 10 && x <= 13 && y >= 11 && y <= 19) ||
                            (x >= 7 && x <= 16 && y >= 15 && y <= 17))
                        {
                            p[y * w + x] = stoneDark;
                        }
                        else
                        {
                            p[y * w + x] = stoneMid;
                        }
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
        gravestoneSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 16f);
        return gravestoneSprite;
    }

    public static Sprite GetOrCreateObeliskSprite()
    {
        if (obeliskSprite != null) return obeliskSprite;

        int w = 24, h = 48;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color stoneLight = new Color(0.65f, 0.68f, 0.72f);
        Color stoneMid = new Color(0.48f, 0.50f, 0.56f);
        Color stoneDark = new Color(0.30f, 0.32f, 0.38f);
        Color runeCyan = new Color(0.2f, 0.85f, 1f, 0.9f); // KÃ½ tá»± rune phÃ¡t sÃ¡ng

        for (int y = 0; y < h; y++)
        {
            // Cá»™t thuÃ´n nhá»n dáº§n vá» Ä‘á»‰nh
            int minX = Mathf.RoundToInt(Mathf.Lerp(6, 9, (float)y / h));
            int maxX = Mathf.RoundToInt(Mathf.Lerp(17, 14, (float)y / h));

            for (int x = 0; x < w; x++)
            {
                if (x >= minX && x <= maxX && y >= 2 && y <= 45)
                {
                    if ((y % 8 == 0 || y % 8 == 1) && x == 11)
                    {
                        p[y * w + x] = runeCyan; // Vá»‡t rune ma má»‹
                    }
                    else if (x == minX)
                    {
                        p[y * w + x] = stoneLight;
                    }
                    else if (x == maxX)
                    {
                        p[y * w + x] = stoneDark;
                    }
                    else
                    {
                        p[y * w + x] = stoneMid;
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
        obeliskSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.05f), 16f);
        return obeliskSprite;
    }

    public static Sprite GetOrCreateBoulderSprite()
    {
        if (boulderSprite != null) return boulderSprite;

        int w = 32, h = 26;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color rockLight = new Color(0.55f, 0.58f, 0.60f);
        Color rockMid = new Color(0.38f, 0.40f, 0.44f);
        Color rockDark = new Color(0.22f, 0.24f, 0.28f);
        Color moss = new Color(0.25f, 0.52f, 0.24f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - 16f) / 13f;
                float dy = (y - 12f) / 9f;
                if (dx * dx + dy * dy <= 1f)
                {
                    if (y >= 16 && (x + y) % 2 == 0) p[y * w + x] = moss;
                    else if (dy > 0.4f || dx < -0.4f) p[y * w + x] = rockLight;
                    else if (dy < -0.4f || dx > 0.4f) p[y * w + x] = rockDark;
                    else p[y * w + x] = rockMid;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        boulderSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.12f), 16f);
        return boulderSprite;
    }

    public static Sprite GetOrCreateUrnSprite()
    {
        if (urnSprite != null) return urnSprite;

        int w = 20, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color clayLight = new Color(0.85f, 0.55f, 0.35f);
        Color clayMid = new Color(0.70f, 0.40f, 0.22f);
        Color clayDark = new Color(0.48f, 0.25f, 0.14f);
        Color goldBand = new Color(1f, 0.85f, 0.2f);

        for (int y = 0; y < h; y++)
        {
            int margin = (y < 4 || y > 18) ? 4 : (y > 7 && y < 14 ? 1 : 2);
            for (int x = 0; x < w; x++)
            {
                if (x >= margin && x < w - margin && y >= 2 && y <= 21)
                {
                    if (y == 11 || y == 12) p[y * w + x] = goldBand; // VÃ²ng Ä‘ai hoÃ ng kim
                    else if (x == margin) p[y * w + x] = clayLight;
                    else if (x == w - margin - 1) p[y * w + x] = clayDark;
                    else p[y * w + x] = clayMid;
                }
                else
                {
                    p[y * w + x] = Color.clear;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        urnSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 16f);
        return urnSprite;
    }
}
