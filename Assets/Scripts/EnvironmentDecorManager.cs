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
    private static Sprite brazierSprite;
    private static Sprite sarcophagusSprite;
    private static Sprite fenceSprite;
    private static Sprite stoneSlabSprite;
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

        // B. Sinh các phiến đá lát cổ & vết tích di tích (Ancient Stone Slabs & Cobblestones)
        int slabCount = Random.Range(3, 6);
        for (int i = 0; i < slabCount; i++)
        {
            Vector2 pos = chunkOrigin + new Vector2(Random.Range(1f, chunkSize - 1f), Random.Range(1f, chunkSize - 1f));
            GameObject slab = new GameObject("Ground_StoneSlab", typeof(SpriteRenderer));
            slab.transform.position = pos;
            slab.transform.SetParent(transform, true);

            SpriteRenderer ssr = slab.GetComponent<SpriteRenderer>();
            ssr.sprite = GetOrCreateStoneSlabSprite();
            ssr.sortingLayerName = "Default";
            ssr.sortingOrder = -96; // Nằm sát ngay trên cỏ
            slab.transform.localScale = Vector3.one * Random.Range(1.2f, 1.8f);
            slab.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0, 4) * 90f);

            objectsInThisChunk.Add(slab);
        }

        // Không sinh vật cản và vũng nước nguy hiểm ngay sát điểm xuất phát (0, 0)
        if (coord == Vector2Int.zero)
        {
            chunkObjects[coord] = objectsInThisChunk;
            return;
        }

        // C. Sinh các vũng nước chiến thuật (Tactical Puddles: Acid, HolyWater, MudTar, BloodPool)
        int puddleCount = Random.Range(1, 3);
        for (int i = 0; i < puddleCount; i++)
        {
            Vector2 pos = chunkOrigin + new Vector2(Random.Range(3f, chunkSize - 3f), Random.Range(3f, chunkSize - 3f));
            PuddleType pType = (PuddleType)Random.Range(0, 4);

            GameObject puddle = new GameObject("Puddle_" + pType, typeof(PuddleProp));
            puddle.transform.position = pos;
            puddle.transform.SetParent(transform, true);

            PuddleProp pProp = puddle.GetComponent<PuddleProp>();
            pProp.InitPuddle(pType);

            objectsInThisChunk.Add(puddle);
        }

        // D. Sinh các vật cản chiến thuật phong phú (Tactical Obstacles)
        int obstacleCount = Random.Range(minObstaclesPerChunk, maxObstaclesPerChunk + 2);
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2 pos = chunkOrigin + new Vector2(Random.Range(2f, chunkSize - 2f), Random.Range(2f, chunkSize - 2f));
            ObstacleType type = (ObstacleType)Random.Range(0, 7);

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
                case ObstacleType.GothicBrazier:
                    propSprite = GetOrCreateBrazierSprite();
                    breakable = true;
                    break;
                case ObstacleType.AncientSarcophagus:
                    propSprite = GetOrCreateSarcophagusSprite();
                    breakable = true;
                    break;
                case ObstacleType.CryptFence:
                    propSprite = GetOrCreateFenceSprite();
                    breakable = false;
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

        int w = 32, h = 44;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.12f, 0.13f, 0.16f);
        Color stoneDark = new Color(0.22f, 0.24f, 0.28f);
        Color stoneMid = new Color(0.42f, 0.45f, 0.50f);
        Color stoneLight = new Color(0.66f, 0.70f, 0.76f);
        Color stoneGlint = new Color(0.85f, 0.88f, 0.94f);
        Color mossDark = new Color(0.16f, 0.35f, 0.14f);
        Color mossLight = new Color(0.32f, 0.62f, 0.24f);
        Color engraveDark = new Color(0.14f, 0.16f, 0.20f);
        Color engraveLight = new Color(0.60f, 0.64f, 0.70f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                bool inPedestal1 = (y >= 2 && y <= 5 && x >= 3 && x <= 28);
                bool inPedestal2 = (y >= 6 && y <= 8 && x >= 5 && x <= 26);

                bool inArch = false;
                if (y >= 9 && y <= 41)
                {
                    int halfW;
                    if (y <= 28)
                    {
                        halfW = 9;
                    }
                    else
                    {
                        float t = (float)(y - 28) / 13f;
                        halfW = Mathf.RoundToInt(Mathf.Lerp(9f, 0.8f, Mathf.Pow(t, 1.35f)));
                    }
                    inArch = (x >= 15 - halfW && x <= 16 + halfW);
                }

                if (inPedestal1 || inPedestal2 || inArch)
                {
                    // Rêu phong ở các góc chân đế
                    bool isMossCorner = (y <= 10 && ((x <= 8 && y <= 8) || (x >= 23 && y <= 7) || ((x + y) % 5 == 0 && y <= 6)));
                    if (isMossCorner)
                    {
                        p[y * w + x] = ((x + y) % 2 == 0) ? mossLight : mossDark;
                        continue;
                    }

                    // Viền cạnh
                    bool isLeftEdge = (inPedestal1 && x == 3) || (inPedestal2 && x == 5) || (inArch && (x == 6 || (y > 28 && x == 15 - Mathf.RoundToInt(Mathf.Lerp(9f, 0.8f, Mathf.Pow((y - 28) / 13f, 1.35f))))));
                    bool isRightEdge = (inPedestal1 && x == 28) || (inPedestal2 && x == 26) || (inArch && (x == 25 || (y > 28 && x == 16 + Mathf.RoundToInt(Mathf.Lerp(9f, 0.8f, Mathf.Pow((y - 28) / 13f, 1.35f))))));
                    bool isTopEdge = (y == 41 || (inPedestal1 && y == 5 && (x < 5 || x > 26)) || (inPedestal2 && y == 8 && (x < 7 || x > 24)));
                    bool isBottomEdge = (y == 2);

                    if (isBottomEdge || isRightEdge)
                    {
                        p[y * w + x] = outline;
                    }
                    else if (isTopEdge || isLeftEdge)
                    {
                        p[y * w + x] = (y >= 38 || (inPedestal1 && y == 5) || (inPedestal2 && y == 8)) ? stoneGlint : stoneLight;
                    }
                    else
                    {
                        // Đường nứt đá tự nhiên
                        bool isCrack = (x == 21 && y >= 26 && y <= 30) || (x == 20 && y >= 22 && y <= 25) || (x == 19 && y >= 18 && y <= 21);
                        if (isCrack)
                        {
                            p[y * w + x] = engraveDark;
                            continue;
                        }
                        if (x == 22 && y >= 26 && y <= 30)
                        {
                            p[y * w + x] = stoneLight;
                            continue;
                        }

                        // Chữ thập Gothic khắc chìm 3D
                        bool isCrossVert = (x >= 15 && x <= 16 && y >= 14 && y <= 32) || (y == 33 && x >= 14 && x <= 17) || (y == 13 && x >= 14 && x <= 17);
                        bool isCrossHoriz = (x >= 10 && x <= 21 && y >= 23 && y <= 25) || (x == 9 && y >= 22 && y <= 26) || (x == 22 && y >= 22 && y <= 26);

                        if (isCrossVert || isCrossHoriz)
                        {
                            if (x == 15 || x == 10 || y == 25 || y == 32)
                            {
                                p[y * w + x] = engraveDark;
                            }
                            else if (x == 16 || x == 21 || y == 23 || y == 13)
                            {
                                p[y * w + x] = engraveLight;
                            }
                            else
                            {
                                p[y * w + x] = stoneDark;
                            }
                        }
                        else
                        {
                            float grain = ((x * 13 + y * 29) % 7 == 0) ? -0.04f : (((x * 19 + y * 7) % 11 == 0) ? 0.04f : 0f);
                            p[y * w + x] = new Color(
                                Mathf.Clamp01(stoneMid.r + grain),
                                Mathf.Clamp01(stoneMid.g + grain),
                                Mathf.Clamp01(stoneMid.b + grain)
                            );
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        gravestoneSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 18f);
        return gravestoneSprite;
    }

    public static Sprite GetOrCreateObeliskSprite()
    {
        if (obeliskSprite != null) return obeliskSprite;

        int w = 28, h = 56;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.12f, 0.14f, 0.17f);
        Color stoneDark = new Color(0.25f, 0.28f, 0.34f);
        Color stoneMid = new Color(0.44f, 0.48f, 0.55f);
        Color stoneLight = new Color(0.68f, 0.74f, 0.82f);
        Color runeCyan = new Color(0.15f, 0.95f, 1.0f, 0.95f);
        Color runeCore = new Color(0.85f, 1.0f, 1.0f, 1.0f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                bool inPlinth = (y >= 2 && y <= 7 && x >= 4 && x <= 23);
                int halfW = Mathf.RoundToInt(Mathf.Lerp(6.5f, 2.5f, (float)(y - 8) / 40f));
                bool inShaft = (y >= 8 && y <= 47 && x >= 14 - halfW && x <= 14 + halfW);
                int peakHalfW = Mathf.RoundToInt(Mathf.Lerp(2.5f, 0.5f, (float)(y - 48) / 7f));
                bool inPeak = (y >= 48 && y <= 54 && x >= 14 - peakHalfW && x <= 14 + peakHalfW);

                if (inPlinth || inShaft || inPeak)
                {
                    bool isRight = (inPlinth && x == 23) || (inShaft && x == 14 + halfW) || (inPeak && x == 14 + peakHalfW);
                    bool isLeft = (inPlinth && x == 4) || (inShaft && x == 14 - halfW) || (inPeak && x == 14 - peakHalfW);

                    if (isRight || y == 2)
                    {
                        p[y * w + x] = outline;
                    }
                    else if (isLeft || y == 54 || (inPlinth && y == 7))
                    {
                        p[y * w + x] = stoneLight;
                    }
                    else
                    {
                        bool isRune = (x == 14 && y >= 12 && y <= 44 && (y % 6 != 0));
                        bool isRuneGlow = ((x == 13 || x == 15) && y >= 12 && y <= 44 && (y % 6 == 3));

                        if (isRune)
                        {
                            p[y * w + x] = runeCore;
                        }
                        else if (isRuneGlow)
                        {
                            p[y * w + x] = runeCyan;
                        }
                        else if (x < 14)
                        {
                            p[y * w + x] = stoneLight;
                        }
                        else
                        {
                            p[y * w + x] = stoneMid;
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        obeliskSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.05f), 18f);
        return obeliskSprite;
    }

    public static Sprite GetOrCreateBoulderSprite()
    {
        if (boulderSprite != null) return boulderSprite;

        int w = 36, h = 30;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.14f, 0.15f, 0.17f);
        Color rockDark = new Color(0.24f, 0.26f, 0.30f);
        Color rockMid = new Color(0.42f, 0.45f, 0.50f);
        Color rockLight = new Color(0.65f, 0.69f, 0.75f);
        Color rockGlint = new Color(0.82f, 0.86f, 0.92f);
        Color mossDark = new Color(0.18f, 0.38f, 0.16f);
        Color mossLight = new Color(0.35f, 0.65f, 0.26f);

        float cx = w * 0.5f;
        float cy = h * 0.44f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                float dx = (x - cx) / (cx * 0.86f);
                float dy = (y - cy) / (cy * 0.80f);
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float rockContour = 1.0f + 0.12f * Mathf.Sin(angle * 4f) + 0.08f * Mathf.Cos(angle * 6f);
                if (dist <= rockContour)
                {
                    if (dist >= rockContour - 0.10f)
                    {
                        p[y * w + x] = (dy > 0.3f && dx < 0.2f) ? rockGlint : outline;
                        continue;
                    }

                    if (dy > 0.25f && dx >= -0.6f && dx <= 0.6f)
                    {
                        p[y * w + x] = ((x + y) % 3 == 0) ? mossLight : mossDark;
                        continue;
                    }

                    if (dx <= 0f && dy >= -0.2f)
                    {
                        float g = ((x * 13 + y * 19) % 7 == 0) ? 0.03f : -0.02f;
                        p[y * w + x] = new Color(rockLight.r + g, rockLight.g + g, rockLight.b + g);
                    }
                    else if (dx >= 0.25f || dy <= -0.3f)
                    {
                        p[y * w + x] = rockDark;
                    }
                    else
                    {
                        float g = ((x * 17 + y * 23) % 9 == 0) ? -0.04f : 0.03f;
                        p[y * w + x] = new Color(rockMid.r + g, rockMid.g + g, rockMid.b + g);
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        boulderSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.12f), 18f);
        return boulderSprite;
    }

    public static Sprite GetOrCreateUrnSprite()
    {
        if (urnSprite != null) return urnSprite;

        int w = 24, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.20f, 0.10f, 0.05f);
        Color clayDark = new Color(0.48f, 0.24f, 0.12f);
        Color clayMid = new Color(0.72f, 0.40f, 0.22f);
        Color clayLight = new Color(0.88f, 0.58f, 0.36f);
        Color clayHighlight = new Color(0.98f, 0.78f, 0.58f);
        Color goldTrim = new Color(1.0f, 0.85f, 0.28f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                int halfW;
                if (y <= 4) halfW = 4;
                else if (y <= 8) halfW = 3;
                else if (y <= 18) halfW = 7;
                else if (y <= 24) halfW = 4;
                else if (y <= 27) halfW = 5;
                else halfW = 0;

                bool inBody = (x >= 12 - halfW && x <= 12 + halfW && y >= 2 && y <= 27);
                bool isHandle = (y >= 14 && y <= 22 && ((x >= 3 && x <= 5) || (x >= 19 && x <= 21)));

                if (inBody || isHandle)
                {
                    bool isEdge = (inBody && (x == 12 - halfW || x == 12 + halfW || y == 2 || y == 27)) ||
                                  (isHandle && (x == 3 || x == 21));

                    if (isEdge)
                    {
                        p[y * w + x] = outline;
                    }
                    else if (y == 16 || y == 17 || y == 24)
                    {
                        p[y * w + x] = goldTrim;
                    }
                    else if (x <= 12 - halfW + 2)
                    {
                        p[y * w + x] = clayHighlight;
                    }
                    else if (x <= 12)
                    {
                        p[y * w + x] = clayLight;
                    }
                    else if (x >= 12 + halfW - 2)
                    {
                        p[y * w + x] = clayDark;
                    }
                    else
                    {
                        p[y * w + x] = clayMid;
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        urnSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 18f);
        return urnSprite;
    }

    public static Sprite GetOrCreateBrazierSprite()
    {
        if (brazierSprite != null) return brazierSprite;

        int w = 28, h = 42;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color ironBlack = new Color(0.14f, 0.14f, 0.16f);
        Color ironMid = new Color(0.28f, 0.28f, 0.32f);
        Color bronzeRim = new Color(0.78f, 0.55f, 0.22f);
        Color bronzeBody = new Color(0.52f, 0.35f, 0.14f);
        Color coalGlow = new Color(0.92f, 0.22f, 0.05f);
        Color fireOrange = new Color(1.0f, 0.52f, 0.10f);
        Color fireYellow = new Color(1.0f, 0.90f, 0.25f);
        Color fireWhite = new Color(1.0f, 1.0f, 0.85f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                bool isLegLeft = (y >= 2 && y <= 15 && ((y <= 6 && x >= 3 && x <= 6) || (y > 6 && x >= 7 && x <= 10)));
                bool isLegRight = (y >= 2 && y <= 15 && ((y <= 6 && x >= 21 && x <= 24) || (y > 6 && x >= 17 && x <= 20)));
                bool isLegCenter = (y >= 2 && y <= 15 && x >= 12 && x <= 15);
                bool isRingCollar = (y >= 13 && y <= 16 && x >= 8 && x <= 19);

                if (isLegLeft || isLegRight || isLegCenter)
                {
                    p[y * w + x] = (x == 3 || x == 7 || x == 12 || x == 17 || x == 21) ? ironMid : ironBlack;
                    continue;
                }
                if (isRingCollar)
                {
                    p[y * w + x] = (y == 16) ? bronzeRim : bronzeBody;
                    continue;
                }

                if (y >= 17 && y <= 24 && x >= 4 && x <= 23)
                {
                    if (y == 24 || x == 4 || x == 23)
                    {
                        p[y * w + x] = bronzeRim;
                    }
                    else if (y == 17)
                    {
                        p[y * w + x] = ironBlack;
                    }
                    else
                    {
                        p[y * w + x] = bronzeBody;
                    }
                    continue;
                }

                if (y >= 23 && y <= 26 && x >= 7 && x <= 20)
                {
                    p[y * w + x] = ((x + y) % 3 == 0) ? fireOrange : coalGlow;
                    continue;
                }

                if (y >= 27 && y <= 40)
                {
                    int flameHalfW = Mathf.RoundToInt(Mathf.Lerp(6.5f, 0.5f, (float)(y - 27) / 13f));
                    if (x >= 14 - flameHalfW && x <= 14 + flameHalfW)
                    {
                        if (y >= 35)
                        {
                            p[y * w + x] = (x == 14) ? fireYellow : fireOrange;
                        }
                        else if (y >= 29 && x >= 12 && x <= 16)
                        {
                            p[y * w + x] = (x == 14 && y <= 32) ? fireWhite : fireYellow;
                        }
                        else
                        {
                            p[y * w + x] = fireOrange;
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        brazierSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 18f);
        return brazierSprite;
    }

    public static Sprite GetOrCreateSarcophagusSprite()
    {
        if (sarcophagusSprite != null) return sarcophagusSprite;

        int w = 32, h = 50;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.12f, 0.13f, 0.16f);
        Color stoneDark = new Color(0.24f, 0.26f, 0.30f);
        Color stoneMid = new Color(0.44f, 0.47f, 0.52f);
        Color stoneLight = new Color(0.68f, 0.72f, 0.78f);
        Color stoneGlint = new Color(0.86f, 0.90f, 0.95f);
        Color goldRelief = new Color(0.95f, 0.80f, 0.28f);
        Color goldShadow = new Color(0.55f, 0.40f, 0.10f);
        Color moss = new Color(0.22f, 0.48f, 0.20f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                bool inBase = (y >= 2 && y <= 13 && x >= 4 && x <= 27);
                bool inCornice = (y >= 14 && y <= 16 && x >= 3 && x <= 28);
                int taper = (y >= 44) ? 2 : (y >= 38 ? 1 : 0);
                bool inLid = (y >= 17 && y <= 47 && x >= 5 + taper && x <= 26 - taper);

                if (inBase || inCornice || inLid)
                {
                    if (y <= 7 && ((x <= 7) || (x >= 24) || ((x + y) % 6 == 0)))
                    {
                        p[y * w + x] = moss;
                        continue;
                    }

                    bool isLeft = (inBase && x == 4) || (inCornice && x == 3) || (inLid && x == 5 + taper);
                    bool isRight = (inBase && x == 27) || (inCornice && x == 28) || (inLid && x == 26 - taper);
                    bool isTop = (y == 47) || (inCornice && y == 16 && (x < 5 || x > 26));
                    bool isBottom = (y == 2) || (inCornice && y == 14);

                    if (isRight || isBottom)
                    {
                        p[y * w + x] = outline;
                    }
                    else if (isLeft || isTop)
                    {
                        p[y * w + x] = (y == 47 || y == 16) ? stoneGlint : stoneLight;
                    }
                    else
                    {
                        if (inBase)
                        {
                            if (y == 8 || ((y <= 8 && x == 15) || (y > 8 && (x == 10 || x == 21))))
                            {
                                p[y * w + x] = stoneDark;
                            }
                            else
                            {
                                p[y * w + x] = stoneMid;
                            }
                            continue;
                        }

                        bool isGoldCrossVert = (x >= 15 && x <= 16 && y >= 22 && y <= 42);
                        bool isGoldCrossHoriz = (x >= 10 && x <= 21 && y >= 32 && y <= 34);
                        bool isGoldCorner = (y == 44 && (x == 8 || x == 23)) || (y == 20 && (x == 7 || x == 24));

                        if (isGoldCrossVert || isGoldCrossHoriz || isGoldCorner)
                        {
                            p[y * w + x] = (x == 15 || y == 34) ? goldRelief : goldShadow;
                        }
                        else
                        {
                            float grain = ((x * 11 + y * 23) % 7 == 0) ? -0.04f : 0.03f;
                            p[y * w + x] = new Color(
                                Mathf.Clamp01(stoneMid.r + grain),
                                Mathf.Clamp01(stoneMid.g + grain),
                                Mathf.Clamp01(stoneMid.b + grain)
                            );
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        sarcophagusSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 18f);
        return sarcophagusSprite;
    }

    public static Sprite GetOrCreateFenceSprite()
    {
        if (fenceSprite != null) return fenceSprite;

        int w = 32, h = 30;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[w * h];

        Color outline = new Color(0.12f, 0.13f, 0.16f);
        Color ironMid = new Color(0.26f, 0.28f, 0.32f);
        Color ironHighlight = new Color(0.55f, 0.58f, 0.65f);
        Color stoneBase = new Color(0.38f, 0.40f, 0.45f);

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                p[y * w + x] = Color.clear;

                if (y >= 2 && y <= 6 && x >= 2 && x <= 29)
                {
                    if (y == 2 || x == 29) p[y * w + x] = outline;
                    else if (y == 6 || x == 2) p[y * w + x] = ironHighlight;
                    else p[y * w + x] = stoneBase;
                }
                else if ((y == 11 || y == 12 || y == 20 || y == 21) && x >= 3 && x <= 28)
                {
                    p[y * w + x] = (y == 12 || y == 21) ? ironHighlight : ironMid;
                }
                else if ((x == 6 || x == 11 || x == 16 || x == 21 || x == 26) && y >= 6 && y <= 25)
                {
                    p[y * w + x] = ironMid;
                }
                else if (y >= 26 && y <= 28 && (x == 6 || x == 11 || x == 16 || x == 21 || x == 26))
                {
                    p[y * w + x] = ironHighlight;
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        fenceSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.08f), 18f);
        return fenceSprite;
    }

    public static Sprite GetOrCreateStoneSlabSprite()
    {
        if (stoneSlabSprite != null) return stoneSlabSprite;

        int size = 36;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] p = new Color[size * size];

        Color stoneLight = new Color(0.60f, 0.63f, 0.68f, 0.88f);
        Color stoneMid = new Color(0.40f, 0.43f, 0.48f, 0.85f);
        Color stoneDark = new Color(0.22f, 0.24f, 0.27f, 0.90f);
        Color seamDark = new Color(0.12f, 0.10f, 0.08f, 0.75f);
        Color grassSprout = new Color(0.24f, 0.55f, 0.22f, 0.85f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                p[y * size + x] = Color.clear;

                if (x >= 2 && x <= 33 && y >= 2 && y <= 33)
                {
                    bool isMortarSeam = (x == 17 || (x == 18 && y >= 14)) || 
                                       (y == 16 && x <= 18) || 
                                       (y == 19 && x >= 17) ||
                                       (x + y == 10 && x <= 6) ||
                                       (x - y == 23 && y <= 7);

                    if (isMortarSeam)
                    {
                        if ((x * 7 + y * 13) % 3 == 0)
                        {
                            p[y * size + x] = grassSprout;
                        }
                        else
                        {
                            p[y * size + x] = seamDark;
                        }
                    }
                    else
                    {
                        bool isSlabTopLeft = (x == 2 || y == 33 || (x == 19 && y > 19) || (y == 17 && x < 17));
                        bool isSlabBottomRight = (x == 33 || y == 2 || (x == 16 && y < 16) || (y == 20 && x > 17));

                        if (isSlabTopLeft)
                        {
                            p[y * size + x] = stoneLight;
                        }
                        else if (isSlabBottomRight)
                        {
                            p[y * size + x] = stoneDark;
                        }
                        else
                        {
                            float grain = ((x * 13 + y * 17) % 7 == 0) ? -0.04f : 0.03f;
                            p[y * size + x] = new Color(
                                Mathf.Clamp01(stoneMid.r + grain),
                                Mathf.Clamp01(stoneMid.g + grain),
                                Mathf.Clamp01(stoneMid.b + grain),
                                stoneMid.a
                            );
                        }
                    }
                }
            }
        }

        tex.SetPixels(p);
        tex.Apply();
        stoneSlabSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 18f);
        return stoneSlabSprite;
    }
}
