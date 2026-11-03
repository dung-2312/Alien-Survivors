using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class InfiniteBackground : MonoBehaviour
{
    [Header("Background Texture & Sprite")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("Visual Tuning")]
    [Tooltip("Color tint to adjust background brightness and tone. Lower brightness helps characters and monsters stand out vividly.")]
    [SerializeField] private Color tintColor = Color.white;

    [Header("Tiling Settings")]
    [Tooltip("Size of one tile in world units. For 1024px texture at 64 PPU, this is 16.")]
    [SerializeField] private float tileSize = 16f;

    [Tooltip("Total dimensions of the tiled background mesh (e.g. 80x80 covers 5x5 tiles).")]
    [SerializeField] private Vector2 backgroundSize = new Vector2(80f, 80f);

    [Tooltip("Sorting layer name. Project has 'BG' layer at the bottom.")]
    [SerializeField] private string sortingLayerName = "BG";

    [Tooltip("Sorting order to ensure background stays behind all gameplay elements.")]
    [SerializeField] private int sortingOrder = -100;

    private Transform camTransform;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        SetupBackgroundRenderer();
    }

    private void Start()
    {
        SetupBackgroundRenderer();
        FindCamera();
        SetupAtmosphere();
    }

    private void OnEnable()
    {
        SetupBackgroundRenderer();
        SetupAtmosphere();
    }

    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = tintColor;
            spriteRenderer.size = backgroundSize;
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
            }
            spriteRenderer.sortingOrder = sortingOrder;

            if (backgroundSprite != null && spriteRenderer.sprite != backgroundSprite)
            {
                spriteRenderer.sprite = backgroundSprite;
            }
        }
    }

    private void SetupBackgroundRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (backgroundSprite == null)
        {
            // Prefer the dark readable alien floor for optimal gameplay contrast
            backgroundSprite = Resources.Load<Sprite>("VS_Grass_Seamless");
            if (backgroundSprite == null)
            {
                backgroundSprite = Resources.Load<Sprite>("Alien_Floor_Seamless");
            }

            if (backgroundSprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("Alien_Floor_Dark_Readable") ?? Resources.Load<Texture2D>("Alien_Floor_Seamless");
                if (tex != null)
                {
                    backgroundSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect);
                }
            }
        }

        if (spriteRenderer != null)
        {
            if (backgroundSprite != null && (spriteRenderer.sprite == null || spriteRenderer.sprite != backgroundSprite))
            {
                spriteRenderer.sprite = backgroundSprite;
            }

            spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            spriteRenderer.size = backgroundSize;
            spriteRenderer.color = tintColor;
            
            // Set sorting layer and order to guarantee rendering behind all entities
            if (!string.IsNullOrEmpty(sortingLayerName))
            {
                spriteRenderer.sortingLayerName = sortingLayerName;
            }
            spriteRenderer.sortingOrder = sortingOrder;
        }
    }

    private void LateUpdate()
    {
        if (camTransform == null)
        {
            FindCamera();
            if (camTransform == null) return;
        }

        // Snaps background position by exact tile increments so the texture remains seamlessly aligned
        float snapX = Mathf.Floor(camTransform.position.x / tileSize) * tileSize;
        float snapY = Mathf.Floor(camTransform.position.y / tileSize) * tileSize;

        transform.position = new Vector3(snapX, snapY, 0f);
    }

    private void FindCamera()
    {
        if (Camera.main != null)
        {
            camTransform = Camera.main.transform;
        }
        else if (PlayerController.instance != null)
        {
            camTransform = PlayerController.instance.transform;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitSceneLoadedListener()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
        {
            TrySpawnBackground(scene.name);
        };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureBackgroundExists()
    {
        TrySpawnBackground();
    }

    public static void TrySpawnBackground(string sceneName = "")
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        // Never spawn in Menu scenes
        if (sceneName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return;
        }

        if (FindAnyObjectByType<InfiniteBackground>() == null)
        {
            // Spawn in gameplay scene (Main scene or where LevelManager / Main Camera exists)
            if (sceneName.Equals("Main", System.StringComparison.OrdinalIgnoreCase) ||
                FindAnyObjectByType<LevelManager>() != null ||
                Camera.main != null)
            {
                GameObject bgObj = new GameObject("Alien_Infinite_Background");
                bgObj.AddComponent<InfiniteBackground>();
            }
        }
    }

    private GameObject vignetteObj;
    private static Sprite vignetteSprite;

    private void SetupAtmosphere()
    {
        if (!Application.isPlaying) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // 1. Subtle Gothic Camera Vignette
        if (vignetteObj == null)
        {
            Transform existing = cam.transform.Find("Atmospheric_Vignette");
            if (existing != null)
            {
                vignetteObj = existing.gameObject;
            }
            else
            {
                vignetteObj = new GameObject("Atmospheric_Vignette", typeof(SpriteRenderer));
                vignetteObj.transform.SetParent(cam.transform, false);
                vignetteObj.transform.localPosition = new Vector3(0f, 0f, 1f);

                SpriteRenderer vsr = vignetteObj.GetComponent<SpriteRenderer>();
                vsr.sprite = GetOrCreateVignetteSprite();
                vsr.sortingLayerName = "Default";
                vsr.sortingOrder = 999; // Phủ viền màn hình nhưng nằm dưới Canvas UI
                vsr.color = new Color(0.02f, 0.03f, 0.06f, 0.42f);
            }
        }

        if (vignetteObj != null && cam != null)
        {
            float height = cam.orthographicSize * 2f;
            float width = height * cam.aspect;
            vignetteObj.transform.localScale = new Vector3(width, height, 1f);
        }

        // 2. Floating Ethereal Fog Wisps & Ground Motes
        if (transform.Find("Ambient_Gothic_Motes") == null)
        {
            GameObject motesObj = new GameObject("Ambient_Gothic_Motes");
            motesObj.transform.SetParent(transform, false);
            motesObj.AddComponent<GothicMotesController>();
        }
    }

    public static Sprite GetOrCreateVignetteSprite()
    {
        if (vignetteSprite != null) return vignetteSprite;

        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];

        float center = (size - 1) * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Đường cong bóng tối mềm mại: trong veo ở giữa màn hình (để nhìn rõ nhân vật & quái), tối dần ở 4 góc cạnh
                float alpha = Mathf.InverseLerp(0.52f, 1.08f, dist);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                pixels[y * size + x] = new Color(0.02f, 0.02f, 0.05f, alpha);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        vignetteSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 128f);
        return vignetteSprite;
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void EditorInit()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (scene, mode) =>
        {
            if (!Application.isPlaying && scene.name.Equals("Main", System.StringComparison.OrdinalIgnoreCase))
            {
                TrySpawnBackground(scene.name);
            }
        };
    }
#endif
}

public class GothicMotesController : MonoBehaviour
{
    private struct MoteData
    {
        public Transform transform;
        public SpriteRenderer sr;
        public Vector2 driftVelocity;
        public float baseAlpha;
        public float phase;
    }

    private readonly List<MoteData> motes = new List<MoteData>();
    private Transform camTarget;

    void Start()
    {
        if (Camera.main != null) camTarget = Camera.main.transform;
        else if (PlayerController.instance != null) camTarget = PlayerController.instance.transform;

        int count = 18;
        for (int i = 0; i < count; i++)
        {
            GameObject m = new GameObject("Mote_" + i, typeof(SpriteRenderer));
            m.transform.SetParent(transform, false);

            Vector2 offset = new Vector2(Random.Range(-15f, 15f), Random.Range(-10f, 10f));
            Vector3 initPos = (camTarget != null ? camTarget.position : Vector3.zero) + (Vector3)offset;
            initPos.z = 0f;
            m.transform.position = initPos;
            m.transform.localScale = Vector3.one * Random.Range(0.25f, 0.65f);

            SpriteRenderer msr = m.GetComponent<SpriteRenderer>();
            msr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
            msr.sortingLayerName = "Default";
            msr.sortingOrder = -85; // Nổi trên nền cỏ và vũng nước

            Color moteColor = (i % 3 == 0) ? new Color(0.35f, 0.85f, 1f, 0.45f) :
                              ((i % 3 == 1) ? new Color(0.75f, 0.45f, 1f, 0.40f) :
                              new Color(1f, 0.85f, 0.4f, 0.35f));
            msr.color = moteColor;

            MoteData md = new MoteData
            {
                transform = m.transform,
                sr = msr,
                driftVelocity = new Vector2(Random.Range(-0.8f, -0.2f), Random.Range(0.15f, 0.45f)),
                baseAlpha = moteColor.a,
                phase = Random.Range(0f, Mathf.PI * 2f)
            };
            motes.Add(md);
        }
    }

    void Update()
    {
        if (camTarget == null)
        {
            if (Camera.main != null) camTarget = Camera.main.transform;
            else if (PlayerController.instance != null) camTarget = PlayerController.instance.transform;
            if (camTarget == null) return;
        }

        Vector3 camPos = camTarget.position;
        float halfW = 16f;
        float halfH = 11f;

        for (int i = 0; i < motes.Count; i++)
        {
            var m = motes[i];
            m.transform.position += (Vector3)(m.driftVelocity * Time.deltaTime);

            // Nhấp nháy nhẹ nhàng
            float alphaMod = 0.7f + 0.3f * Mathf.Sin(Time.time * 2.5f + m.phase);
            Color c = m.sr.color;
            c.a = m.baseAlpha * alphaMod;
            m.sr.color = c;

            // Tái tuần hoàn vị trí khi trôi ra ngoài tầm nhìn camera
            Vector3 diff = m.transform.position - camPos;
            if (diff.x < -halfW) m.transform.position += new Vector3(halfW * 2f, 0f, 0f);
            if (diff.x > halfW) m.transform.position -= new Vector3(halfW * 2f, 0f, 0f);
            if (diff.y < -halfH) m.transform.position += new Vector3(0f, halfH * 2f, 0f);
            if (diff.y > halfH) m.transform.position -= new Vector3(0f, halfH * 2f, 0f);
        }
    }
}
