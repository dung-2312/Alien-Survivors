using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum IndicatorTargetType
{
    Boss,
    MiniBoss,
    Shrine,
    Chest
}

/// <summary>
/// Quản lý mũi tên chỉ hướng mục tiêu ngoài màn hình (Off-Screen Quest & Boss Pointer).
/// Giúp người chơi định hướng chính xác vị trí của Boss, Mini-Boss, Đàn Tế Cổ và Hòm Kho Báu khi chúng xuất hiện ở xa.
/// </summary>
public class OffScreenIndicatorManager : MonoBehaviour
{
    public static OffScreenIndicatorManager instance;

    [Header("Indicator Settings")]
    public float screenMargin = 55f; // Khoảng cách viền màn hình (pixel)
    public float pulseSpeed = 4.5f;

    private Canvas targetCanvas;
    private RectTransform canvasRect;
    private Camera mainCam;

    private class TrackedIndicator
    {
        public GameObject rootObj;
        public RectTransform rootRect;
        public RectTransform arrowRect;
        public Image arrowImage;
        public TMP_Text distanceText;
        public Transform targetTransform;
        public IndicatorTargetType type;
    }

    private readonly List<TrackedIndicator> activeIndicators = new List<TrackedIndicator>();
    private readonly List<TrackedIndicator> indicatorPool = new List<TrackedIndicator>();

    private static Sprite arrowSprite;
    private static Sprite crownIcon;
    private static Sprite skullIcon;
    private static Sprite shrineIcon;
    private static Sprite chestIcon;
    private static TMP_FontAsset retroFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0) return;

        if (instance == null)
        {
            GameObject go = new GameObject("OffScreenIndicatorManager", typeof(OffScreenIndicatorManager));
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        mainCam = Camera.main;
        FindOrCreateCanvas();
        LoadResources();
    }

    private void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        FindOrCreateCanvas();
    }

    private void FindOrCreateCanvas()
    {
        if (targetCanvas != null) return;

        // Ưu tiên Canvas của UIController
        if (UIController.instance != null)
        {
            targetCanvas = UIController.instance.GetComponentInParent<Canvas>();
        }

        if (targetCanvas == null)
        {
            targetCanvas = FindAnyObjectByType<Canvas>();
        }

        if (targetCanvas != null)
        {
            transform.SetParent(targetCanvas.transform, false);
            canvasRect = targetCanvas.GetComponent<RectTransform>();
        }
    }

    private void LateUpdate()
    {
        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        if (canvasRect == null)
        {
            FindOrCreateCanvas();
            if (canvasRect == null) return;
        }

        // 1. Quét tìm các mục tiêu cần theo dõi trong cảnh
        List<(Transform target, IndicatorTargetType type)> currentTargets = GatherTrackedTargets();

        // 2. Đồng bộ số lượng indicator với mục tiêu
        EnsureIndicatorsCount(currentTargets.Count);

        Vector3 playerPos = (PlayerController.instance != null) ? PlayerController.instance.transform.position : Vector3.zero;
        Vector2 halfScreen = canvasRect.sizeDelta * 0.5f;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.08f;

        // 3. Cập nhật vị trí, góc xoay và cự ly của từng Indicator
        for (int i = 0; i < currentTargets.Count; i++)
        {
            TrackedIndicator ind = activeIndicators[i];
            var targetData = currentTargets[i];
            ind.targetTransform = targetData.target;
            ind.type = targetData.type;

            if (ind.targetTransform == null || !ind.targetTransform.gameObject.activeInHierarchy)
            {
                ind.rootObj.SetActive(false);
                continue;
            }

            Vector3 targetWorldPos = ind.targetTransform.position;
            Vector3 viewportPos = mainCam.WorldToViewportPoint(targetWorldPos);

            // Kiểm tra xem mục tiêu có đang nằm trọn trong màn hình hay không
            bool isInsideScreen = viewportPos.z > 0f &&
                                  viewportPos.x >= 0.05f && viewportPos.x <= 0.95f &&
                                  viewportPos.y >= 0.06f && viewportPos.y <= 0.94f;

            if (isInsideScreen)
            {
                // Mục tiêu đã lọt vào tầm mắt người chơi -> Ẩn mũi tên để không che tầm nhìn
                ind.rootObj.SetActive(false);
                continue;
            }

            ind.rootObj.SetActive(true);

            // Xử lý mục tiêu ở phía sau Camera
            if (viewportPos.z < 0f)
            {
                viewportPos.x = 1f - viewportPos.x;
                viewportPos.y = 1f - viewportPos.y;
            }

            // Chuyển sang tọa độ UI Canvas tâm (0,0)
            Vector2 screenPos = new Vector2(
                (viewportPos.x - 0.5f) * canvasRect.sizeDelta.x,
                (viewportPos.y - 0.5f) * canvasRect.sizeDelta.y
            );

            // Giới hạn (clamp) trong đường biên màn hình
            float clampX = halfScreen.x - screenMargin;
            float clampY = halfScreen.y - screenMargin;
            Vector2 clampedPos = new Vector2(
                Mathf.Clamp(screenPos.x, -clampX, clampX),
                Mathf.Clamp(screenPos.y, -clampY, clampY)
            );

            ind.rootRect.anchoredPosition = clampedPos;
            ind.rootRect.localScale = Vector3.one * pulse;

            // Xoay mũi tên chỉ thẳng về phía mục tiêu
            Vector2 dirToTarget = (Vector2)targetWorldPos - (Vector2)playerPos;
            float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
            if (ind.arrowRect != null)
            {
                ind.arrowRect.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }

            // Cập nhật khoảng cách & nhãn hiển thị
            float distance = Vector2.Distance(playerPos, targetWorldPos);
            if (ind.distanceText != null)
            {
                ind.distanceText.text = $"{distance:F0}m";
            }

            ApplyTypeTheme(ind, targetData.type);
        }

        // Tắt các indicator dư thừa
        for (int i = currentTargets.Count; i < activeIndicators.Count; i++)
        {
            activeIndicators[i].rootObj.SetActive(false);
        }
    }

    private List<(Transform target, IndicatorTargetType type)> GatherTrackedTargets()
    {
        List<(Transform target, IndicatorTargetType type)> list = new List<(Transform, IndicatorTargetType)>();

        // A. Trùm chính (BossController)
        BossController[] bosses = FindObjectsByType<BossController>();
        foreach (var b in bosses)
        {
            if (b != null && b.gameObject.activeInHierarchy)
            {
                list.Add((b.transform, IndicatorTargetType.Boss));
            }
        }

        // B. Mini-Boss / Quái Tinh Anh (isMiniBoss)
        EnemyController[] enemies = FindObjectsByType<EnemyController>();
        foreach (var e in enemies)
        {
            if (e != null && e.isMiniBoss && e.gameObject.activeInHierarchy && e.health > 0)
            {
                // Tránh trùng với BossController
                if (e.GetComponent<BossController>() == null)
                {
                    list.Add((e.transform, IndicatorTargetType.MiniBoss));
                }
            }
        }

        // C. Đàn Tế Cổ (AncientShrine)
        AncientShrine[] shrines = FindObjectsByType<AncientShrine>();
        foreach (var s in shrines)
        {
            if (s != null && s.gameObject.activeInHierarchy)
            {
                list.Add((s.transform, IndicatorTargetType.Shrine));
            }
        }

        // D. Hòm Kho Báu (TreasureChest)
        TreasureChest[] chests = FindObjectsByType<TreasureChest>();
        foreach (var c in chests)
        {
            if (c != null && c.gameObject.activeInHierarchy)
            {
                list.Add((c.transform, IndicatorTargetType.Chest));
            }
        }

        return list;
    }

    private void EnsureIndicatorsCount(int count)
    {
        while (activeIndicators.Count < count)
        {
            TrackedIndicator ind = (indicatorPool.Count > 0) ? indicatorPool[indicatorPool.Count - 1] : CreateIndicatorUI();
            if (indicatorPool.Count > 0) indicatorPool.RemoveAt(indicatorPool.Count - 1);
            activeIndicators.Add(ind);
        }
    }

    private TrackedIndicator CreateIndicatorUI()
    {
        TrackedIndicator ind = new TrackedIndicator();

        GameObject root = new GameObject("OffScreenIndicator", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        ind.rootObj = root;
        ind.rootRect = root.GetComponent<RectTransform>();
        ind.rootRect.sizeDelta = new Vector2(100f, 65f);

        // Mũi tên định hướng (Arrow)
        GameObject arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        arrowGO.transform.SetParent(root.transform, false);
        ind.arrowRect = arrowGO.GetComponent<RectTransform>();
        ind.arrowRect.sizeDelta = new Vector2(28f, 28f);
        ind.arrowRect.anchoredPosition = new Vector2(0f, 22f);
        ind.arrowImage = arrowGO.GetComponent<Image>();
        ind.arrowImage.sprite = GetOrCreateArrowSprite();

        // Khung hiển thị Nhãn & Cự ly (Pill Capsule)
        GameObject pillGO = new GameObject("Pill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        pillGO.transform.SetParent(root.transform, false);
        RectTransform pillRect = pillGO.GetComponent<RectTransform>();
        pillRect.sizeDelta = new Vector2(85f, 32f);
        pillRect.anchoredPosition = new Vector2(0f, -12f);
        Image pillImg = pillGO.GetComponent<Image>();
        pillImg.sprite = GetOrCreateCapsuleSprite();
        pillImg.type = Image.Type.Sliced;
        pillImg.color = new Color(0.08f, 0.08f, 0.12f, 0.90f);

        // Chữ Tên & Cự ly (Label / Distance)
        GameObject txtGO = new GameObject("DistanceText", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(pillGO.transform, false);
        RectTransform trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        ind.distanceText = txtGO.GetComponent<TextMeshProUGUI>();
        if (retroFont != null) ind.distanceText.font = retroFont;
        ind.distanceText.fontSize = 15f;
        ind.distanceText.fontStyle = FontStyles.Bold;
        ind.distanceText.alignment = TextAlignmentOptions.Center;
        ind.distanceText.color = Color.white;

        return ind;
    }

    private void ApplyTypeTheme(TrackedIndicator ind, IndicatorTargetType type)
    {
        Color themeColor;
        switch (type)
        {
            case IndicatorTargetType.Boss:
                themeColor = new Color(1.0f, 0.20f, 0.25f); // Đỏ rực hoàng gia
                break;
            case IndicatorTargetType.MiniBoss:
                themeColor = new Color(1.0f, 0.55f, 0.15f); // Cam lửa
                break;
            case IndicatorTargetType.Shrine:
                themeColor = new Color(0.20f, 0.85f, 1.0f); // Xanh băng tuyết
                break;
            case IndicatorTargetType.Chest:
                themeColor = new Color(1.0f, 0.88f, 0.25f); // Hoàng kim lấp lánh
                break;
            default:
                themeColor = Color.white;
                break;
        }

        if (ind.arrowImage != null) ind.arrowImage.color = themeColor;
        if (ind.distanceText != null) ind.distanceText.color = themeColor;
    }

    private static Sprite capsuleSprite;
    private static Sprite GetOrCreateCapsuleSprite()
    {
        if (capsuleSprite != null) return capsuleSprite;

        const int w = 24, h = 24;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool isBorder = x == 0 || x == w - 1 || y == 0 || y == h - 1;
                px[y * w + x] = isBorder ? new Color(0.85f, 0.75f, 0.35f, 0.95f) : new Color(0.08f, 0.08f, 0.12f, 0.88f);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        capsuleSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(2, 2, 2, 2));
        return capsuleSprite;
    }
    private static Sprite GetOrCreateArrowSprite()
    {
        if (arrowSprite != null) return arrowSprite;

        const int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];

        Color transparent = Color.clear;
        Color solidWhite = Color.white;

        // Vẽ hình mũi tên tam giác sắc nhọn pixel art hướng lên trên (0, 1)
        for (int y = 0; y < h; y++)
        {
            int span = (int)((1f - (float)y / h) * (w * 0.45f));
            int mid = w / 2;
            for (int x = 0; x < w; x++)
            {
                if (x >= mid - span && x <= mid + span && y >= 6)
                {
                    px[y * w + x] = solidWhite;
                }
                else
                {
                    px[y * w + x] = transparent;
                }
            }
        }

        tex.SetPixels(px);
        tex.Apply();
        arrowSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        return arrowSprite;
    }

    private void LoadResources()
    {
        if (retroFont == null)
        {
            retroFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }
    }
}
