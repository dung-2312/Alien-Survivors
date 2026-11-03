using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using AlienSurvivors.Upgrades;

public class ExperienceLevelController : MonoBehaviour
{
    public static ExperienceLevelController instance; // Create a singleton instance of the ExperienceLevelController. GK
    public void Awake()
    {
        instance = this; // Set the instance to this object. GK
    }

    public int currentExperience; // The current experience of the player. GK 

    public ExpPickup pickup; // The experience pickup. GK
    public List<int> expLevels; // The list of experience levels. GK
    public int currentLevel = 1, levelCount = 100; // The current level of the player. GK
    public List<Weapon> weaponsToUpgrade; // The list of weapons to upgrade. GK

    private readonly List<ExpPickup> pickupPool = new List<ExpPickup>(); // The pool of experience pickups, so kills don't Instantiate/Destroy constantly. GK

    [HideInInspector]
    public int queuedLevels;

    // Start is called before the first frame update
    void Start()
    {
        // Build authentic Vampire Survivors progressive EXP curve
        // Early levels are accessible; higher levels progressively take much more EXP to fill
        expLevels = new List<int>();
        expLevels.Add(0); // Level 0 dummy index
        for (int lvl = 1; lvl <= levelCount; lvl++)
        {
            int req;
            if (lvl <= 1) req = 8;
            else if (lvl <= 5) req = 8 + (lvl - 1) * 12; // Lvl 2: 20, Lvl 3: 32, Lvl 4: 44, Lvl 5: 56
            else if (lvl <= 10) req = 56 + (lvl - 5) * 24; // Lvl 6: 80, Lvl 7: 104, Lvl 8: 128, Lvl 9: 152, Lvl 10: 176
            else if (lvl <= 20) req = 176 + (lvl - 10) * 48; // Lvl 15: 416, Lvl 20: 656
            else if (lvl <= 40) req = 656 + (lvl - 20) * 95; // Lvl 30: 1606, Lvl 40: 2556
            else req = 2556 + (lvl - 40) * 180 + (int)(Mathf.Pow(lvl - 40, 1.85f) * 25f);
            expLevels.Add(req);
        }

        // Tự động khởi tạo ExpManager nếu chưa có trong Scene
        if (ExpManager.instance == null)
        {
            GameObject expMgrObj = new GameObject("ExpManager");
            ExpManager expMgr = expMgrObj.AddComponent<ExpManager>();
            expMgr.pickupPrefab = pickup;
        }

        if (UIController.instance != null && expLevels.Count > currentLevel)
        {
            UIController.instance.UpdateExperience(currentExperience, expLevels[currentLevel], currentLevel);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetExp(int amountToGet) // Function to get experience. GK
    {
        currentExperience += amountToGet; // Add the amount to the current experience. GK
        while(currentLevel < expLevels.Count && currentExperience >= expLevels[currentLevel])
        {
            currentExperience -= expLevels[currentLevel];
            currentLevel++;
            queuedLevels++;
            if(currentLevel >= expLevels.Count)
            {
                currentLevel = expLevels.Count - 1;
                break;
            }
        }
        if(queuedLevels > 0 && !UIController.instance.levelUpPanel.activeSelf)
        {
            ShowLevelUp();
        }
        UIController.instance.UpdateExperience(currentExperience, expLevels[currentLevel], currentLevel); // Update the experience. GK
        SFXManager.instance.PlaySFXPitched(2); // Play the sound effect. GK
    }

    public void SpawnExp(Vector3 position, int expValue) // Function to spawn experience into the world. GK
    {
        if (ComboManager.instance != null && ComboManager.instance.ExpBonusMultiplier > 1f)
        {
            expValue = Mathf.RoundToInt(expValue * ComboManager.instance.ExpBonusMultiplier);
        }
        // 1. Sử dụng ExpManager với cơ chế gom ngọc tự động (EXP Merging) & giới hạn số lượng
        if (ExpManager.instance != null)
        {
            ExpManager.instance.SpawnExp(position, expValue);
            return;
        }

        // 2. Fallback sang logic cũ nếu không có ExpManager
        ExpPickup newPickup;

        if (pickupPool.Count > 0)
        {
            newPickup = pickupPool[pickupPool.Count - 1];
            pickupPool.RemoveAt(pickupPool.Count - 1);
            newPickup.transform.SetPositionAndRotation(position, Quaternion.identity);
        }
        else
        {
            newPickup = Instantiate(pickup, position, Quaternion.identity);
        }

        newPickup.gameObject.SetActive(true);
        newPickup.ResetPickup(expValue);
    }

    public void ReturnToPool(ExpPickup pickupToReturn) // Function to return a picked-up experience orb to the pool instead of destroying it. GK
    {
        if (ExpManager.instance != null)
        {
            ExpManager.instance.ReturnToPool(pickupToReturn);
            return;
        }

        pickupToReturn.gameObject.SetActive(false);
        pickupPool.Add(pickupToReturn);
    }

    public void ShowLevelUp() // Function to show level up panel for the player. GK
    {
        // 1. Ưu tiên sử dụng LevelUpManager chuyên nghiệp với animation trượt mượt mà
        if (LevelUpManager.instance != null)
        {
            LevelUpManager.instance.ShowLevelUp();
            return;
        }

        if (UIController.instance != null)
        {
            UIController.instance.ApplySciFiLevelUpTheme();
            UIController.instance.levelUpPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Set the time scale to 0. GK

        // Khởi tạo UpgradeManager tự động nếu chưa có trong scene
        if (UpgradeManager.instance == null)
        {
            GameObject managerObj = new GameObject("UpgradeManager");
            managerObj.AddComponent<UpgradeManager>();
        }

        int buttonCount = (UIController.instance != null && UIController.instance.levelUpButtons != null) ? UIController.instance.levelUpButtons.Length : 3;

        // Sử dụng kiến trúc Data-Driven với UpgradeManager
        if (UpgradeManager.instance != null)
        {
            List<UpgradeData> chosenUpgrades = UpgradeManager.instance.GetRandomUpgrades(buttonCount);

            if (chosenUpgrades != null && chosenUpgrades.Count > 0)
            {
                for (int i = 0; i < buttonCount; i++)
                {
                    if (i < chosenUpgrades.Count)
                    {
                        UIController.instance.levelUpButtons[i].gameObject.SetActive(true);
                        UIController.instance.levelUpButtons[i].UpdateButtonDisplay(chosenUpgrades[i]);
                    }
                    else
                    {
                        UIController.instance.levelUpButtons[i].gameObject.SetActive(false);
                    }
                }
                return;
            }
        }

        // FAILSAFE: Nếu danh sách thẻ Data-Driven chưa sẵn sàng, dùng hệ thống vũ khí cơ bản để 3 thẻ luôn hiển thị
        weaponsToUpgrade.Clear();
        List<Weapon> availableWeapons = new List<Weapon>();
        if (PlayerController.instance != null)
        {
            availableWeapons.AddRange(PlayerController.instance.assignedWeapons);
            if (PlayerController.instance.assignedWeapons.Count + PlayerController.instance.fullyLevelledWeapons.Count < PlayerController.instance.maxWeapons)
            {
                availableWeapons.AddRange(PlayerController.instance.unassignedWeapons);
            }
        }

        for (int i = 0; i < buttonCount; i++)
        {
            if (availableWeapons.Count > 0)
            {
                int sel = Random.Range(0, availableWeapons.Count);
                weaponsToUpgrade.Add(availableWeapons[sel]);
                availableWeapons.RemoveAt(sel);
            }
        }

        for (int i = 0; i < buttonCount; i++)
        {
            if (i < weaponsToUpgrade.Count)
            {
                UIController.instance.levelUpButtons[i].gameObject.SetActive(true);
                UIController.instance.levelUpButtons[i].UpdateButtonDisplay(weaponsToUpgrade[i]);
            }
            else
            {
                UIController.instance.levelUpButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void FinishLevelUp()
    {
        queuedLevels--;
        if (queuedLevels > 0)
        {
            ShowLevelUp();
        }
        else
        {
            queuedLevels = 0;
            if (LevelUpManager.instance != null)
            {
                LevelUpManager.instance.ClosePanel();
            }
            else
            {
                UIController.instance.levelUpPanel.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}
