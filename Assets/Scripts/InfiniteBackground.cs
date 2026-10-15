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
    }

    private void OnEnable()
    {
        SetupBackgroundRenderer();
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
