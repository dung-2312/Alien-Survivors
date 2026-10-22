using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponThrower : Weapon
{
    public EnemyDamager damager; // The enemy damager. GK
    private float throwCounter; // The counter for the throw. GK
    
    // Start is called before the first frame update
    void Start()
    {
        SetStats();
    }

    // Update is called once per frame
    void Update()
    {
        if(statsUpdated == true) // If the stats are updated. GK
        {
            statsUpdated = false; // Set the stats updated to false. GK
            SetStats(); // Set the stats. GK
        }
        throwCounter -= Time.deltaTime; // Decrease the throw counter. GK
        if (throwCounter <= 0)
        {
            throwCounter = stats[weaponLevel].timeBetweenAttacks; // Reset the throw counter. GK

            // Xuất chiêu trực tiếp từ tay nhân vật (ngang ngực/tay, không bao giờ ở dưới chân)
            Vector3 spawnPos = PlayerController.instance != null
                ? PlayerController.instance.transform.position + new Vector3(PlayerController.instance.facingDirection * 0.35f, 0.3f, 0f)
                : transform.position;

            for (int i = 0; i < stats[weaponLevel].amount; i++)
            {
                GameObject axe = Instantiate(damager, spawnPos, damager.transform.rotation).gameObject;
                axe.SetActive(true);
            }

            SFXManager.instance.PlaySFXPitched(4); // Play the sound effect. GK
        }
    }
    void SetStats() // Function to set the stats of the weapon. GK
    {
        damager.damageAmount = stats[weaponLevel].damage; // Set the damage amount of the damager to the damage of the weapon. GK
        damager.lifeTime = stats[weaponLevel].duration; // Set the life time of the damager to the duration of the weapon. GK
        damager.transform.localScale = Vector3.one * stats[weaponLevel].range; // Set the scale of the damager to the range of the weapon. GK
        throwCounter = 0f; // Reset the throw counter. GK
    }
}
