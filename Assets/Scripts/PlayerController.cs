using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance; // Create a singleton instance of the PlayerController. GK
    public Rigidbody2D theRB;
    [Header("Sprite & Facing")]
    public Transform spriteTransform; // Transform of the sprite to flip
    private float originalScaleX = 1f;
    private float originalScaleY = 1f;
    [HideInInspector]
    public float facingDirection = 1f; // 1 = right, -1 = left
    private Vector2 lastMoveDirection = Vector2.down; // Stores last facing direction for Idle Blend Tree

    public void Awake()
    {
        instance = this; // Set the instance to this object. GK
        if (theRB == null)
        {
            theRB = GetComponent<Rigidbody2D>();
        }

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
            }
        }

        if (spriteTransform != null)
        {
            originalScaleX = Mathf.Abs(spriteTransform.localScale.x);
            originalScaleY = spriteTransform.localScale.y;
        }
    }
    public float moveSpeed; // The speed at which the player moves. AK

    public Animator anim; // The animator component. AK
    public float pickupRange = 1.5f; // The range of the attack. AK
    //public Weapon activeWeapon; // The active weapon. GK
    public List<Weapon> unassignedWeapons, assignedWeapons; // The list of unassigned and assigned weapons. GK
    public int maxWeapons = 3; // The maximum number of weapons. GK
    [HideInInspector]
    public List<Weapon> fullyLevelledWeapons = new List<Weapon>(); // The list of fully levelled weapons. GK

    void Start()
    {
        if(assignedWeapons.Count == 0) // If the assigned weapons count is 0. GK
        {        
            AddWeapon(Random.Range(0, unassignedWeapons.Count)); // Add a random weapon. GK

        }
        moveSpeed = PlayerStatController.instance.moveSpeed[0].value; // Set the move speed to the player stat controller move speed. GK
        pickupRange = PlayerStatController.instance.pickupRange[0].value; // Set the pickup range to the player stat controller pickup range. GK
        maxWeapons = Mathf.RoundToInt(PlayerStatController.instance.maxWeapons[0].value); // Set the max weapons to the player stat controller max weapons. GK
    }

    void Update()
    {
        Vector3 moveInput = new Vector3(0f, 0f, 0f); // Create a new vector to store the input. AK
        moveInput.x = Input.GetAxisRaw("Horizontal"); // Get the horizontal input. AK
        moveInput.y = Input.GetAxisRaw("Vertical"); // Get the vertical input. AK

        moveInput.Normalize(); // Normalize the vector so that the player moves at the same speed in all directions. AK

        // Facing direction flip
        if (moveInput.x > 0.05f)
        {
            facingDirection = 1f;
        }
        else if (moveInput.x < -0.05f)
        {
            facingDirection = -1f;
        }

        // Smooth physics-based movement (eliminates stiffness/hard snapping)
        Vector2 targetVelocity = new Vector2(moveInput.x, moveInput.y) * moveSpeed;
        if (theRB != null)
        {
            theRB.linearVelocity = Vector2.MoveTowards(theRB.linearVelocity, targetVelocity, moveSpeed * 15f * Time.deltaTime);
        }
        else
        {
            transform.position += (Vector3)targetVelocity * Time.deltaTime;
        }

        bool isMoving = moveInput != Vector3.zero;

        // Dynamic visual animation & sprite flip
        if (spriteTransform != null)
        {
            if (isMoving)
            {
                // Dynamic walk bounce
                float walkBounce = 1f + Mathf.Sin(Time.time * 12f) * 0.05f;
                spriteTransform.localScale = new Vector3(facingDirection * originalScaleX, originalScaleY * walkBounce, spriteTransform.localScale.z);
            }
            else
            {
                // Idle breathing effect so the character is never stiff/frozen
                float idleBreath = 1f + Mathf.Sin(Time.time * 3f) * 0.025f;
                spriteTransform.localScale = new Vector3(facingDirection * originalScaleX, originalScaleY * idleBreath, spriteTransform.localScale.z);
            }
        }

        // Update last move direction when moving
        if (isMoving)
        {
            lastMoveDirection = new Vector2(moveInput.x, moveInput.y);
        }

        // Animator integration (Blend Tree 2D Simple Directional & Legacy support)
        if (anim != null)
        {
            anim.SetFloat("MoveX", moveInput.x);
            anim.SetFloat("MoveY", moveInput.y);
            anim.SetFloat("LastMoveX", lastMoveDirection.x);
            anim.SetFloat("LastMoveY", lastMoveDirection.y);
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
            anim.SetBool("isMoving", isMoving);

            if (isMoving)
            {
                anim.speed = Mathf.Clamp(moveSpeed / 4f, 0.9f, 2.2f); // Scales walk animation speed with move speed
            }
            else
            {
                anim.speed = 1f;
            }
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
}