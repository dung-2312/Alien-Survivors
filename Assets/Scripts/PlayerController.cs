using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; // Create a singleton instance of the PlayerController. GK
    public Rigidbody2D theRB;

    [Header("Sprite & Facing")]
    public Transform spriteTransform; // Transform of the sprite to flip
    private float originalScaleZ = 1f;
    private Vector3 originalBasePos = Vector3.zero;

    [Header("Character World Size")]
    [Tooltip("Target visual height in world units to ensure all characters are clear, prominent and well-proportioned")]
    public float targetCharacterHeight = 2.4f;
    private float characterScaleMultiplier = 1.0f;
    private float referenceSpritePixelHeight = 32f;

    [HideInInspector]
    public float facingDirection = 1f; // 1 = right, -1 = left
    private Vector2 lastMoveDirection = Vector2.down; // Stores last facing direction for Idle Blend Tree

    [Header("Dash Skill (Active Skill)")]
    public bool canDash = true;
    public float dashSpeed = 16f; // Rapid burst speed
    public float dashDuration = 0.22f; // Short snappy dash
    public float dashCooldown = 3.5f; // Seconds
    private float dashCooldownTimer = 0f;
    private float dashDurationTimer = 0f;
    [HideInInspector]
    public bool isDashing = false;
    private Vector2 dashDirection;
    private float ghostTrailTimer = 0f;

    [Header("Dynamic Leg & Walking Animation")]
    private SpriteRenderer playerSR;
    private Sprite[] currentWalkFrames;
    private Sprite[] currentIdleFrames;
    private float currentWalkFrameRate = 10f;
    private float currentIdleFrameRate = 6f;
    private Sprite defaultCharacterSprite;
    private CharacterData currentCharacterData;
    private float animTimer = 0f;
    private float stepCycleTime = 0f;
    private float prevStepSin = 0f;
    private bool hasCustomWalkFrames = false;

    [Header("Character Passives & Combat Traits")]
    private float divineRegenTimer = 3.5f;
    private float divineAegisCooldownTimer = 0f;
    private float caltropDropTimer = 1.8f;
    private int antonioKillStreak = 0;

    public float DashCooldownProgress => Mathf.Clamp01(dashCooldownTimer / dashCooldown);
    public bool IsDashReady => dashCooldownTimer <= 0f && !isDashing;

    public void Awake()
    {
        instance = this; // Set the instance to this object. GK
        if (theRB == null)
        {
            theRB = GetComponent<Rigidbody2D>();
        }

        if (GetComponent<PlayerUltimateController>() == null)
        {
            gameObject.AddComponent<PlayerUltimateController>();
        }

        SetupSpriteReferences();
    }

    private void SetupSpriteReferences()
    {
        if (spriteTransform == null)
        {
            Transform foundSprite = transform.Find("Sprite");
            if (foundSprite != null)
            {
                spriteTransform = foundSprite;
            }
            else
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    spriteTransform = sr.transform;
                }
                else
                {
                    spriteTransform = transform;
                }
            }
        }

        if (spriteTransform != null)
        {
            playerSR = spriteTransform.GetComponent<SpriteRenderer>();
            if (playerSR == null) playerSR = GetComponentInChildren<SpriteRenderer>();

            if (playerSR != null && defaultCharacterSprite == null)
            {
                defaultCharacterSprite = playerSR.sprite;
            }

            if (defaultCharacterSprite != null)
            {
                referenceSpritePixelHeight = Mathf.Max(10f, defaultCharacterSprite.rect.height);
            }

            originalScaleZ = spriteTransform.localScale.z != 0f ? spriteTransform.localScale.z : 1f;
            originalBasePos = spriteTransform.localPosition;
        }

        // Táº¯t Animator máº·c Ä‘á»‹nh vÃ¬ Animator nÃ y chá»‰ cÃ³ 1 clip xoay cá»‘ Ä‘á»‹nh Ä‘Ã¨ lÃªn cÃ¡c chuyá»ƒn Ä‘á»™ng chÃ¢n
        if (anim != null)
        {
            anim.enabled = false;
        }
    }

    /// <summary>
    /// TÃ­nh toÃ¡n tá»‰ lá»‡ phÃ³ng Ä‘áº¡i Scale Ä‘á»ƒ nhÃ¢n váº­t luÃ´n hiá»ƒn thá»‹ to rÃµ rÃ ng, cÃ¢n Ä‘á»‘i hoÃ n háº£o trong game
    /// </summary>
    public float GetCurrentBaseScale()
    {
        if (playerSR == null || playerSR.sprite == null)
        {
            return targetCharacterHeight * characterScaleMultiplier;
        }

        float ppu = playerSR.sprite.pixelsPerUnit;
        if (ppu <= 0.01f) ppu = 100f;

        float refH = referenceSpritePixelHeight > 5f ? referenceSpritePixelHeight : 32f;
        float unitPerPixel = (targetCharacterHeight * characterScaleMultiplier) / refH;

        return unitPerPixel * ppu;
    }

    /// <summary>
    /// Khá»Ÿi táº¡o dá»¯ liá»‡u tÆ°á»›ng vÃ  cÃ¡c khung hÃ¬nh bÆ°á»›c chÃ¢n tá»« CharacterData
    /// </summary>
    public void InitCharacter(CharacterData data)
    {
        SetupSpriteReferences();
        if (data == null) return;

        currentCharacterData = data;
        characterScaleMultiplier = (data.customScaleMultiplier > 0.1f) ? data.customScaleMultiplier : 1.0f;

        if (data.characterSprite != null)
        {
            defaultCharacterSprite = data.characterSprite;
            referenceSpritePixelHeight = Mathf.Max(10f, data.characterSprite.rect.height);
            if (playerSR != null)
            {
                playerSR.sprite = data.characterSprite;
            }
        }
        else if (playerSR != null && playerSR.sprite != null)
        {
            referenceSpritePixelHeight = Mathf.Max(10f, playerSR.sprite.rect.height);
        }

        if (data.walkFrames != null && data.walkFrames.Length > 0)
        {
            currentWalkFrames = data.walkFrames;
            hasCustomWalkFrames = true;
            currentWalkFrameRate = data.walkFrameRate > 0f ? data.walkFrameRate : 10f;
        }
        else
        {
            currentWalkFrames = null;
            hasCustomWalkFrames = false;
        }

        if (data.idleFrames != null && data.idleFrames.Length > 0)
        {
            currentIdleFrames = data.idleFrames;
            currentIdleFrameRate = data.idleFrameRate > 0f ? data.idleFrameRate : 6f;
            if (playerSR != null && currentIdleFrames.Length > 0 && currentIdleFrames[0] != null)
            {
                playerSR.sprite = currentIdleFrames[0];
            }
        }
        else
        {
            currentIdleFrames = null;
        }

        // Ãp dá»¥ng scale chuáº©n hÃ³a ngay khi xuáº¥t hiá»‡n
        if (spriteTransform != null)
        {
            float initScale = GetCurrentBaseScale();
            spriteTransform.localScale = new Vector3(facingDirection * initScale, initScale, originalScaleZ);
            originalBasePos = spriteTransform.localPosition;
        }

        if (anim != null)
        {
            anim.enabled = false;
        }
    }

    public float moveSpeed; // The speed at which the player moves. AK
    public Animator anim; // The animator component. AK
    public float pickupRange = 1.5f; // The range of the attack. AK
    public List<Weapon> unassignedWeapons, assignedWeapons; // The list of unassigned and assigned weapons. GK
    public int maxWeapons = 3; // The maximum number of weapons. GK
    [HideInInspector]
    public List<Weapon> fullyLevelledWeapons = new List<Weapon>(); // The list of fully levelled weapons. GK

    void Start()
    {
        // Trang bá»‹ vÅ© khÃ­ Ä‘á»™c quyá»n cá»‘ Ä‘á»‹nh theo Ä‘áº·c trÆ°ng tá»«ng nhÃ¢n váº­t (khÃ´ng random)
        if (assignedWeapons.Count == 0)
        {
            AssignSignatureWeapon();
        }
        moveSpeed = PlayerStatController.instance.moveSpeed[0].value; // Set the move speed to the player stat controller move speed. GK
        pickupRange = PlayerStatController.instance.pickupRange[0].value; // Set the pickup range to the player stat controller pickup range. GK
        maxWeapons = Mathf.RoundToInt(PlayerStatController.instance.maxWeapons[0].value); // Set the max weapons to the player stat controller max weapons. GK
    }

    /// <summary>
    /// KÃ­ch hoáº¡t vÅ© khÃ­ Ä‘á»™c quyá»n cá»‘ Ä‘á»‹nh cá»§a tá»«ng nhÃ¢n váº­t
    /// </summary>
    private void AssignSignatureWeapon()
    {
        string targetWeapon = "";
        if (currentCharacterData != null && !string.IsNullOrEmpty(currentCharacterData.signatureWeaponName))
        {
            targetWeapon = currentCharacterData.signatureWeaponName.Trim();
        }
        else if (currentCharacterData != null)
        {
            string cName = currentCharacterData.characterName.ToLowerInvariant();
            if (cName.Contains("knight")) targetWeapon = "Giant Sword";
            else if (cName.Contains("wizard")) targetWeapon = "Magic Fireball";
            else if (cName.Contains("antonio")) targetWeapon = "Throwing Axes";
            else if (cName.Contains("bandit")) targetWeapon = "Dagger Throw";
            else if (cName.Contains("adventurer")) targetWeapon = "Bright Zone";
            else if (cName.Contains("archer") || cName.Contains("forest")) targetWeapon = "Forest Bow";
        }
        else
        {
            string objName = gameObject.name.ToLowerInvariant();
            if (objName.Contains("forest")) targetWeapon = "Forest Bow";
            else if (objName.Contains("wizard")) targetWeapon = "Magic Fireball";
            else targetWeapon = "Throwing Axes";
        }

        // Trường hợp là Forest Bow: Khởi tạo cung tên xuyên phá đặc trưng cho Cung thủ
        if (targetWeapon.Equals("Forest Bow", System.StringComparison.OrdinalIgnoreCase))
        {
            Weapon bow = SetupForestBowWeapon();
            if (bow != null)
            {
                AddWeapon(bow);
                return;
            }
        }

        // Trường hợp là Magic Fireball: Khởi tạo cầu lửa ma thuật phóng từ quyền trượng cho Evil Wizard
        if (targetWeapon.Equals("Magic Fireball", System.StringComparison.OrdinalIgnoreCase))
        {
            Weapon fireball = SetupEvilWizardFireballWeapon();
            if (fireball != null)
            {
                AddWeapon(fireball);
                return;
            }
        }

        // TrÆ°á»ng há»£p lÃ  Forest Bow: Khá»Ÿi táº¡o cung tÃªn xuyÃªn phÃ¡ Ä‘áº·c trÆ°ng cho Cung thá»§
        if (targetWeapon.Equals("Forest Bow", System.StringComparison.OrdinalIgnoreCase))
        {
            Weapon bow = SetupForestBowWeapon();
            if (bow != null)
            {
                AddWeapon(bow);
                return;
            }
        }

        // TÃ¬m vÅ© khÃ­ tÆ°Æ¡ng á»©ng trong danh sÃ¡ch unassignedWeapons
        Weapon matchedWeapon = null;
        for (int i = 0; i < unassignedWeapons.Count; i++)
        {
            if (unassignedWeapons[i] != null && 
                unassignedWeapons[i].name.Equals(targetWeapon, System.StringComparison.OrdinalIgnoreCase))
            {
                matchedWeapon = unassignedWeapons[i];
                break;
            }
        }

        if (matchedWeapon != null)
        {
            AddWeapon(matchedWeapon);
        }
        else if (unassignedWeapons.Count > 0)
        {
            AddWeapon(0);
        }
    }

    /// <summary>
    /// Táº¡o vÅ© khÃ­ Tháº§n Tiá»…n Rá»«ng Xanh (Forest Bow) Ä‘á»™c quyá»n cho Cung Thá»§ Red Archer
    /// </summary>
    private Weapon SetupForestBowWeapon()
    {
        Transform weaponsHolder = transform.Find("Weapons");
        if (weaponsHolder != null)
        {
            Transform existing = weaponsHolder.Find("Forest Bow");
            if (existing != null)
            {
                ProjectileWeapon pw = existing.GetComponent<ProjectileWeapon>();
                if (pw != null) return pw;
            }
        }

        // DÃ¹ng ProjectileWeapon lÃ m máº«u clone
        ProjectileWeapon daggerProto = GetComponentInChildren<ProjectileWeapon>(true);
        if (daggerProto == null) return null;

        Transform parentTr = daggerProto.transform.parent != null ? daggerProto.transform.parent : transform;
        GameObject bowObj = Instantiate(daggerProto.gameObject, parentTr);
        bowObj.name = "Forest Bow";
        bowObj.SetActive(false);

        ProjectileWeapon bowWeapon = bowObj.GetComponent<ProjectileWeapon>();
        if (bowWeapon != null)
        {
            bowWeapon.weaponRange = 8.5f; // Táº§m báº¯n xa chuáº©n xáº¡ thá»§
            if (currentCharacterData != null && currentCharacterData.weaponSprite != null)
            {
                bowWeapon.icon = currentCharacterData.weaponSprite;
            }

            // Tinh chá»‰nh mÅ©i tÃªn xuyÃªn phÃ¡ (Piercing Spirit Arrow)
            Transform projTr = bowObj.transform.Find("Dagger Projectile");
            if (projTr != null)
            {
                projTr.name = "Forest Arrow";
                EnemyDamager damager = projTr.GetComponent<EnemyDamager>();
                if (damager != null)
                {
                    damager.damageAmount = 26f;
                    damager.destroyOnImpact = false; // MÅ©i tÃªn bay xuyÃªn qua hÃ ng loáº¡t quÃ¡i!
                    damager.lifeTime = 2.2f;
                }

                Projectile proj = projTr.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.moveSpeed = 13.5f; // Tá»‘c Ä‘á»™ bay nhanh
                }

                SpriteRenderer arrowSr = projTr.GetComponent<SpriteRenderer>();
                if (arrowSr != null)
                {
                    arrowSr.color = new Color(0.25f, 1f, 0.45f, 1f); // Ãnh sÃ¡ng xanh ngá»c bÃ­ch
                }
            }
        }

        return bowWeapon;
    }

    /// <summary>
    /// Tao vu khi Cau Lua Ma Thuat (Magic Fireball) phong tu quyen truong dac trung cho Evil Wizard
    /// </summary>
    private Weapon SetupEvilWizardFireballWeapon()
    {
        Transform weaponsHolder = transform.Find("Weapons");
        if (weaponsHolder != null)
        {
            Transform existing = weaponsHolder.Find("Magic Fireball");
            if (existing != null)
            {
                ProjectileWeapon pw = existing.GetComponent<ProjectileWeapon>();
                if (pw != null) return pw;
            }
        }

        // Dung ProjectileWeapon (Dagger Throw) lam mau clone
        ProjectileWeapon daggerProto = GetComponentInChildren<ProjectileWeapon>(true);
        if (daggerProto == null) return null;

        Transform parentTr = daggerProto.transform.parent != null ? daggerProto.transform.parent : transform;
        GameObject fireballObj = Instantiate(daggerProto.gameObject, parentTr);
        fireballObj.name = "Magic Fireball";
        fireballObj.SetActive(false);

        ProjectileWeapon fbWeapon = fireballObj.GetComponent<ProjectileWeapon>();
        if (fbWeapon != null)
        {
            fbWeapon.weaponRange = 8f; // Tam ban xa cua phap su
            if (currentCharacterData != null && currentCharacterData.weaponSprite != null)
            {
                fbWeapon.icon = currentCharacterData.weaponSprite;
            }

            // Tinh chinh cau lua ma thuat
            Transform projTr = fireballObj.transform.Find("Dagger Projectile");
            if (projTr != null)
            {
                projTr.name = "Staff Fireball";
                EnemyDamager damager = projTr.GetComponent<EnemyDamager>();
                if (damager != null)
                {
                    damager.damageAmount = 30f;
                    damager.destroyOnImpact = true;
                    damager.lifeTime = 2.0f;
                    damager.shouldKnockBack = true;
                }

                Projectile proj = projTr.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.moveSpeed = 10.5f;
                }

                SpriteRenderer fbSr = projTr.GetComponent<SpriteRenderer>();
                if (fbSr != null)
                {
                    if (currentCharacterData != null && currentCharacterData.weaponSprite != null)
                    {
                        fbSr.sprite = currentCharacterData.weaponSprite;
                    }
                    fbSr.color = new Color(1f, 0.45f, 0.15f, 1f); // Mau lua ma dao ruc sang
                    projTr.localScale = Vector3.one * 1.6f;
                }
            }
        }

        return fbWeapon;
    }

    void Update()
    {
        // 1. Cáº­p nháº­t Cooldown cá»§a Dash
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        Vector3 moveInput = new Vector3(0f, 0f, 0f); // Create a new vector to store the input. AK
        moveInput.x = Input.GetAxisRaw("Horizontal"); // Get the horizontal input. AK
        moveInput.y = Input.GetAxisRaw("Vertical"); // Get the vertical input. AK

        moveInput.Normalize(); // Normalize the vector so that the player moves at the same speed in all directions. AK

        // 2. KÃ­ch hoáº¡t Dash khi nháº¥n Space hoáº·c Shift
        if (canDash && IsDashReady && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftShift)))
        {
            StartDash(moveInput);
        }

        // 3. Xá»­ lÃ½ tráº¡ng thÃ¡i Ä‘ang Dash
        if (isDashing)
        {
            dashDurationTimer -= Time.deltaTime;
            ghostTrailTimer -= Time.deltaTime;
            if (ghostTrailTimer <= 0f)
            {
                ghostTrailTimer = 0.05f;
                SpawnGhostAfterimage();
            }

            if (theRB != null)
            {
                theRB.linearVelocity = dashDirection * dashSpeed;
            }
            else
            {
                transform.position += (Vector3)(dashDirection * dashSpeed * Time.deltaTime);
            }

            if (dashDurationTimer <= 0f)
            {
                EndDash();
            }

            return; // Khi Ä‘ang lÆ°á»›t thÃ¬ bá» qua di chuyá»ƒn thÃ´ng thÆ°á»ng
        }

        // Facing direction flip
        if (moveInput.x > 0.05f)
        {
            facingDirection = 1f;
        }
        else if (moveInput.x < -0.05f)
        {
            facingDirection = -1f;
        }

        // Smooth physics-based movement káº¿t há»£p tá»‘c Ä‘á»™ tá»« chuá»—i combo Cuá»“ng Ná»™ (Combo Bloodrush)
        float comboSpeedBonus = (ComboManager.instance != null) ? ComboManager.instance.MoveSpeedMultiplier : 1.0f;
        float finalMoveSpeed = moveSpeed * comboSpeedBonus;
        Vector2 targetVelocity = new Vector2(moveInput.x, moveInput.y) * finalMoveSpeed;
        if (theRB != null)
        {
            theRB.linearVelocity = Vector2.MoveTowards(theRB.linearVelocity, targetVelocity, finalMoveSpeed * 15f * Time.deltaTime);
        }
        else
        {
            transform.position += (Vector3)targetVelocity * Time.deltaTime;
        }

        bool isMoving = moveInput != Vector3.zero;

        // 4. Há»‡ thá»‘ng Hoáº¡t áº¢nh BÆ°á»›c ChÃ¢n & Nhá»‹p Cháº¡y Linh Hoáº¡t (Walking Legs Cycle)
                UpdateCharacterWalkingAnimation(isMoving, moveInput);
        UpdateCharacterPassives();

        // Update last move direction when moving
        if (isMoving)
        {
            lastMoveDirection = new Vector2(moveInput.x, moveInput.y);
        }
    }

    /// <summary>
    /// Xá»­ lÃ½ chuyá»ƒn Ä‘á»™ng nhÃ¢n váº­t: Ä‘á»• ngÆ°á»i vá» trÆ°á»›c lÆ°á»›t giÃ³, nhÃºn náº£y bÆ°á»›c chÃ¢n tá»± nhiÃªn vÃ  giá»¯ pixel sáº¯c nÃ©t
    /// </summary>
    private void UpdateCharacterWalkingAnimation(bool isMoving, Vector3 moveInput)
    {
        if (spriteTransform == null) return;

        float baseScale = GetCurrentBaseScale();

        if (isMoving)
        {
            // Tá»‘c Ä‘á»™ nhá»‹p bÆ°á»›c chÃ¢n tá»‰ lá»‡ vá»›i tá»‘c Ä‘á»™ di chuyá»ƒn
            float actualSpeed = theRB != null ? theRB.linearVelocity.magnitude : moveSpeed;
            float stepSpeedFactor = Mathf.Clamp(actualSpeed / Mathf.Max(0.5f, moveSpeed), 0.7f, 1.5f);

            // A. Chu ká»³ bÆ°á»›c chÃ¢n nhá»‹p nhÃ ng
            stepCycleTime += Time.deltaTime * 12f * stepSpeedFactor;

            // NhÃºn náº£y trá»ng tÃ¢m bÆ°á»›c chÃ¢n nháº¹ nhÃ ng, tá»± nhiÃªn (Micro-hop 0.045f)
            float stepHop = Mathf.Abs(Mathf.Sin(stepCycleTime)) * 0.045f;

            // GÃ³c nghiÃªng Ä‘á»• ngÆ°á»i vá» phÃ­a trÆ°á»›c theo hÆ°á»›ng cháº¡y (Dynamic Run Lean):
            // GiÃºp nhÃ¢n váº­t trÃ´ng thanh thoÃ¡t, dÅ©ng mÃ£nh lÆ°á»›t giÃ³ thay vÃ¬ láº¯c lÆ° nhÆ° con láº¯c
            float targetLean = -4.5f * facingDirection;
            if (Mathf.Abs(moveInput.x) < 0.2f && Mathf.Abs(moveInput.y) > 0.2f)
            {
                targetLean = 0f; // Cháº¡y tháº³ng Ä‘á»©ng lÃªn/xuá»‘ng thÃ¬ giá»¯ thÄƒng báº±ng
            }

            // Vá»›i nhÃ¢n váº­t khÃ´ng cÃ³ animation frame (nhÆ° Antonio), thÃªm nhá»‹p chuyá»ƒn bÆ°á»›c chÃ¢n cá»±c nháº¹ (1.5Â°)
            float walkWiggle = hasCustomWalkFrames ? 0f : Mathf.Sin(stepCycleTime) * 1.5f;

            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetLean + walkWiggle);
            spriteTransform.localRotation = Quaternion.Lerp(spriteTransform.localRotation, targetRot, 14f * Time.deltaTime);
            spriteTransform.localPosition = new Vector3(originalBasePos.x, originalBasePos.y + stepHop, originalBasePos.z);

            // B. Khung hÃ¬nh bÆ°á»›c chÃ¢n (Frame-by-Frame Walk Sprites)
            if (hasCustomWalkFrames && currentWalkFrames != null && currentWalkFrames.Length > 0)
            {
                animTimer += Time.deltaTime * currentWalkFrameRate * stepSpeedFactor;
                int frameIdx = Mathf.FloorToInt(animTimer) % currentWalkFrames.Length;
                if (playerSR != null && frameIdx < currentWalkFrames.Length)
                {
                    playerSR.sprite = currentWalkFrames[frameIdx];
                    baseScale = GetCurrentBaseScale();
                }
            }

            // Giá»¯ nguyÃªn tá»· lá»‡ Pixel Art vuÃ´ng váº¯n sáº¯c nÃ©t 100% (KHÃ”NG co mÃ©o legSquash lÃ m vá»¡ pixel)
            spriteTransform.localScale = new Vector3(facingDirection * baseScale, baseScale, originalScaleZ);

            // C. Bá»¥i gÃ³t chÃ¢n má»m máº¡i khi tiáº¿p Ä‘áº¥t
            float currentSin = Mathf.Sin(stepCycleTime);
            if ((prevStepSin < 0f && currentSin >= 0f) || (prevStepSin > 0f && currentSin <= 0f))
            {
                SpawnFootstepDust(currentSin >= 0f ? 1f : -1f);
            }
            prevStepSin = currentSin;
        }
        else
        {
            // Reset gÃ³c nghiÃªng vá» tháº³ng Ä‘á»©ng khi dá»«ng láº¡i má»™t cÃ¡ch Ãªm Ã¡i
            spriteTransform.localRotation = Quaternion.Lerp(spriteTransform.localRotation, Quaternion.identity, 12f * Time.deltaTime);

            // Khung hÃ¬nh Idle náº¿u cÃ³
            if (currentIdleFrames != null && currentIdleFrames.Length > 0)
            {
                animTimer += Time.deltaTime * currentIdleFrameRate;
                int frameIdx = Mathf.FloorToInt(animTimer) % currentIdleFrames.Length;
                if (playerSR != null && frameIdx < currentIdleFrames.Length)
                {
                    playerSR.sprite = currentIdleFrames[frameIdx];
                    baseScale = GetCurrentBaseScale();
                }
            }
            else if (playerSR != null && defaultCharacterSprite != null && playerSR.sprite != defaultCharacterSprite)
            {
                playerSR.sprite = defaultCharacterSprite;
                baseScale = GetCurrentBaseScale();
            }

            // Nhá»‹p thá»Ÿ bá»“ng bá»nh thanh thoÃ¡t theo phÆ°Æ¡ng Y (Gentle Breathing Float)
            float idleBob = Mathf.Sin(Time.time * 3f) * 0.02f;
            spriteTransform.localPosition = new Vector3(originalBasePos.x, originalBasePos.y + idleBob, originalBasePos.z);

            // Giá»¯ nguyÃªn tá»· lá»‡ chuáº©n hÃ³a tuyá»‡t Ä‘á»‘i, khÃ´ng co dÃ£n mÃ©o pixel
            spriteTransform.localScale = new Vector3(facingDirection * baseScale, baseScale, originalScaleZ);
        }
    }

    /// <summary>
    /// Táº¡o hiá»‡u á»©ng bá»¥i chÃ¢n nhá» li ti khi bÆ°á»›c chÃ¢n tiáº¿p xÃºc máº·t Ä‘áº¥t
    /// </summary>
    private void SpawnFootstepDust(float sideOffset)
    {
        if (playerSR == null) return;

        GameObject dust = new GameObject("StepDust", typeof(SpriteRenderer));
        dust.transform.position = transform.position + new Vector3(sideOffset * 0.18f * facingDirection, -0.70f, 0f);
        dust.transform.localScale = Vector3.one * 0.12f;

        SpriteRenderer dsr = dust.GetComponent<SpriteRenderer>();
        dsr.sprite = playerSR.sprite; // MÆ°á»£n texture hoáº·c váº½ háº¡t bá»¥i
        dsr.sortingLayerID = playerSR.sortingLayerID;
        dsr.sortingOrder = playerSR.sortingOrder - 2;
        dsr.color = new Color(0.85f, 0.85f, 0.90f, 0.35f);

        StartCoroutine(AnimateStepDust(dust, dsr));
    }

    private IEnumerator AnimateStepDust(GameObject dust, SpriteRenderer dsr)
    {
        float lifetime = 0.22f;
        float elapsed = 0f;
        Vector3 startScale = dust.transform.localScale;
        Vector3 targetScale = startScale * 1.8f;
        Color col = dsr.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            if (dust != null)
            {
                dust.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                dust.transform.position += Vector3.up * (Time.deltaTime * 0.15f);
            }
            if (dsr != null)
            {
                dsr.color = new Color(col.r, col.g, col.b, Mathf.Lerp(col.a, 0f, t));
            }
            yield return null;
        }

        if (dust != null) Destroy(dust);
    }

    private void StartDash(Vector3 currentInput)
    {
        isDashing = true;
        dashDurationTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        ghostTrailTimer = 0f;

        if (currentInput.sqrMagnitude > 0.05f)
        {
            dashDirection = new Vector2(currentInput.x, currentInput.y).normalized;
        }
        else if (lastMoveDirection.sqrMagnitude > 0.05f)
        {
            dashDirection = lastMoveDirection.normalized;
        }
        else
        {
            dashDirection = new Vector2(facingDirection, 0f);
        }

        // Báº¥t tá»­ trong thá»i gian lÆ°á»›t
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.TriggerInvincibility(dashDuration + 0.1f);
        }

        // Rung nháº¹ mÃ n hÃ¬nh táº¡o cáº£m giÃ¡c lá»±c
        if (CameraController.instance != null)
        {
            CameraController.instance.Shake(0.12f, 0.16f);
        }

        // Ã‚m thanh vÃºt
        if (SFXManager.instance != null)
        {
            SFXManager.instance.PlaySFXPitched(1);
        }

        SpawnGhostAfterimage();
    }

    private void EndDash()
    {
        isDashing = false;
        if (theRB != null)
        {
            theRB.linearVelocity = Vector2.zero;
        }
    }

    private void SpawnGhostAfterimage()
    {
        if (spriteTransform == null) return;
        SpriteRenderer sr = spriteTransform.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        GameObject ghost = new GameObject("GhostTrail", typeof(SpriteRenderer));
        ghost.transform.position = spriteTransform.position;
        ghost.transform.rotation = spriteTransform.rotation;
        ghost.transform.localScale = spriteTransform.lossyScale;

        SpriteRenderer ghostSr = ghost.GetComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder - 1;
        ghostSr.color = new Color(0.35f, 0.85f, 1f, 0.65f); // Cyan trail bÃ³ng má» Ä‘áº¹p máº¯t

        StartCoroutine(FadeAndDestroyGhost(ghost, ghostSr));
    }

    private IEnumerator FadeAndDestroyGhost(GameObject ghost, SpriteRenderer ghostSr)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Color initialColor = ghostSr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, elapsed / duration);
            if (ghostSr != null)
            {
                ghostSr.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }
            yield return null;
        }

        if (ghost != null)
        {
            Destroy(ghost);
        }
    }

    public void AddWeapon(int weaponNumber) // Function to add a weapon. GK
    {
        if (weaponNumber < unassignedWeapons.Count) // If the weapon number is less than the unassigned weapons count. GK  
        {
            assignedWeapons.Add(unassignedWeapons[weaponNumber]); // Add the weapon to the assigned weapons. GK
            unassignedWeapons[weaponNumber].gameObject.SetActive(true); // Set the weapon to active. GK
            unassignedWeapons.RemoveAt(weaponNumber); // Remove the weapon from the unassigned weapons. GK
            if (UIController.instance != null)
            {
                UIController.instance.UpdateWeaponSlots();
            }
        }
    }

    public void AddWeapon(Weapon weaponToAdd) // Function to add the weapon. GK
    {
        weaponToAdd.gameObject.SetActive(true); // Set the weapon to active. GK
        assignedWeapons.Add(weaponToAdd); // Add the weapon to the assigned weapons. GK
        unassignedWeapons.Remove(weaponToAdd); // Remove the weapon from the unassigned weapons. GK
        if (UIController.instance != null)
        {
            UIController.instance.UpdateWeaponSlots();
        }
    }
    private Coroutine attackSwingCo;

    /// <summary>
    /// Kich hoat cu dong chem / vung vu khi vat ly cua nhan vat
    /// </summary>
    public void TriggerAttackSwing(float swingDirX)
    {
        if (spriteTransform == null) return;
        if (attackSwingCo != null)
        {
            StopCoroutine(attackSwingCo);
        }
        attackSwingCo = StartCoroutine(AttackSwingRoutine(swingDirX));
    }

    private IEnumerator AttackSwingRoutine(float swingDirX)
    {
        float sign = Mathf.Sign(swingDirX);
        if (Mathf.Abs(swingDirX) > 0.05f)
        {
            facingDirection = sign;
        }

        float swingDuration = 0.16f;
        float elapsed = 0f;
        float baseScale = GetCurrentBaseScale();

        Vector3 punchOffset = new Vector3(sign * 0.14f, 0.03f, 0f);
        Quaternion swingRot = Quaternion.Euler(0f, 0f, -14f * sign);

        while (elapsed < swingDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swingDuration;

            float forwardFactor = (t < 0.35f) ? (t / 0.35f) : (1f - (t - 0.35f) / 0.65f);
            forwardFactor = Mathf.SmoothStep(0f, 1f, forwardFactor);

            if (spriteTransform != null)
            {
                spriteTransform.localPosition = originalBasePos + punchOffset * forwardFactor;
                spriteTransform.localRotation = Quaternion.Slerp(Quaternion.identity, swingRot, forwardFactor);
                spriteTransform.localScale = new Vector3(facingDirection * baseScale, baseScale, originalScaleZ);
            }
            yield return null;
        }

        if (spriteTransform != null)
        {
            spriteTransform.localPosition = originalBasePos;
            spriteTransform.localRotation = Quaternion.identity;
            spriteTransform.localScale = new Vector3(facingDirection * baseScale, baseScale, originalScaleZ);
        }
        attackSwingCo = null;
    }
    public float GetCriticalStrikeChance()
    {
        string cName = (currentCharacterData != null) ? currentCharacterData.characterName.ToLowerInvariant() : "";
        if (cName.Contains("bandit")) return 0.35f;
        if (cName.Contains("antonio"))
        {
            float hpPct = (PlayerHealthController.instance != null && PlayerHealthController.instance.maxHealth > 0)
                ? (PlayerHealthController.instance.currentHealth / PlayerHealthController.instance.maxHealth)
                : 1f;
            return 0.15f + (1f - hpPct) * 0.25f;
        }
        if (cName.Contains("archer") || cName.Contains("forest")) return 0.22f;
        return 0.12f;
    }

    public float GetCriticalStrikeMultiplier()
    {
        string cName = (currentCharacterData != null) ? currentCharacterData.characterName.ToLowerInvariant() : "";
        if (cName.Contains("bandit")) return 2.4f;
        return 2.0f;
    }

    public void OnTakeDamage(ref float damage)
    {
        string cName = (currentCharacterData != null) ? currentCharacterData.characterName.ToLowerInvariant() : "";

        if (cName.Contains("knight"))
        {
            damage *= 0.8f;
            TriggerHolyRetributionShockwave();
        }

        if (cName.Contains("adventurer"))
        {
            if (PlayerHealthController.instance != null)
            {
                float nextHealth = PlayerHealthController.instance.currentHealth - damage;
                if (nextHealth <= PlayerHealthController.instance.maxHealth * 0.35f && divineAegisCooldownTimer <= 0f)
                {
                    TriggerDivineAegis();
                }
            }
        }
    }

    public void OnEnemyKilled(Vector3 deathPos)
    {
        if (PlayerUltimateController.instance != null)
        {
            PlayerUltimateController.instance.AddRage(3.5f);
        }

        string cName = (currentCharacterData != null) ? currentCharacterData.characterName.ToLowerInvariant() : "";

        if (cName.Contains("wizard"))
        {
            if (Random.value <= 0.22f)
            {
                TriggerArcaneFireBurst(deathPos);
            }
        }

        if (cName.Contains("antonio"))
        {
            antonioKillStreak++;
            if (antonioKillStreak >= 10)
            {
                antonioKillStreak = 0;
                TriggerTitanCleaverWhirlwind();
            }
        }
    }

    private void UpdateCharacterPassives()
    {
        if (divineAegisCooldownTimer > 0f) divineAegisCooldownTimer -= Time.deltaTime;

        string cName = (currentCharacterData != null) ? currentCharacterData.characterName.ToLowerInvariant() : "";

        if (cName.Contains("adventurer"))
        {
            divineRegenTimer -= Time.deltaTime;
            if (divineRegenTimer <= 0f)
            {
                divineRegenTimer = 3.5f;
                if (PlayerHealthController.instance != null)
                {
                    PlayerHealthController.instance.Heal(2.5f);
                }
            }
        }

        if (cName.Contains("bandit") && theRB != null && theRB.linearVelocity.sqrMagnitude > 1f)
        {
            caltropDropTimer -= Time.deltaTime;
            if (caltropDropTimer <= 0f)
            {
                caltropDropTimer = 1.8f;
                DropShadowCaltrop();
            }
        }
    }

    private void TriggerHolyRetributionShockwave()
    {
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        if (CameraController.instance != null) CameraController.instance.Shake(0.18f, 0.22f);

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 4.5f);
        foreach (var col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(35f, true, true);
            }
        }
    }

    private void TriggerDivineAegis()
    {
        divineAegisCooldownTimer = 45f;
        if (PlayerHealthController.instance != null)
        {
            PlayerHealthController.instance.Heal(35f);
            PlayerHealthController.instance.TriggerInvincibility(3.0f);
        }
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(2);
        if (UIController.instance != null) UIController.instance.ShowWarning("[ DIVINE AEGIS TRIGGERED! +35 HP ]", 2.2f);
    }

    private void TriggerArcaneFireBurst(Vector3 pos)
    {
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        Collider2D[] enemies = Physics2D.OverlapCircleAll(pos, 3.2f);
        foreach (var col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(45f, true, true);
            }
        }
    }

    private void TriggerTitanCleaverWhirlwind()
    {
        if (SFXManager.instance != null) SFXManager.instance.PlaySFXPitched(0);
        if (CameraController.instance != null) CameraController.instance.Shake(0.22f, 0.28f);
        if (UIController.instance != null) UIController.instance.ShowWarning("[ TITAN CLEAVER UNLEASHED! ]", 1.8f);

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 6.0f);
        foreach (var col in enemies)
        {
            if (col.CompareTag("Enemy") && col.TryGetComponent<EnemyController>(out var enemy))
            {
                enemy.TakeDamage(80f, true, true);
            }
        }
    }

    private void DropShadowCaltrop()
    {
        GameObject caltrop = new GameObject("ShadowCaltrop", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(ShadowCaltrop));
        caltrop.transform.position = transform.position;
        caltrop.transform.localScale = Vector3.one * 0.7f;

        SpriteRenderer sr = caltrop.GetComponent<SpriteRenderer>();
        sr.sprite = EnvironmentDecorManager.GetOrCreateSoftShadowSprite();
        sr.color = new Color(0.2f, 0.95f, 0.35f, 0.85f);
        sr.sortingOrder = 2;

        CircleCollider2D col = caltrop.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.45f;
    }
}

public class ShadowCaltrop : MonoBehaviour
{
    private float lifeTime = 7f;

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy") && col.TryGetComponent<EnemyController>(out var enemy))
        {
            enemy.TakeDamage(28f, true, true);
            enemy.moveSpeed *= 0.6f;
            Destroy(gameObject);
        }
    }
}