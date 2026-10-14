using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlienSurvivors.Upgrades;

// Save & Exit / Continue: snapshots the current run (player, weapons, upgrades, timer) and restores it later. GK

[Serializable]
public class RunSaveData
{
    public int version = 1;
    public int characterIndex;
    public float timer;
    public float posX, posY, posZ;
    public float health, maxHealth;
    public float moveSpeed, pickupRange;
    public int maxWeapons;
    public int level, experience, queuedLevels;
    public int runCoins; // Coins picked up during this run.
    public List<WeaponSave> weapons = new List<WeaponSave>();
    public List<CardSave> cards = new List<CardSave>();
}

[Serializable]
public class WeaponSave
{
    public string name;
    public int level;
    public int state; // 0 = owned but not equipped, 1 = equipped, 2 = fully levelled
    public float[] damages; // Per-level damage, because the damage passive modifies it at runtime.
}

[Serializable]
public class CardSave
{
    public string name;
    public int level; // Passive stat cards: how many times it was taken.
    public bool evolved; // Evolution cards: whether the evolution already happened.
}

public static class RunSave
{
    const string Key = "RunSaveJson";

    public static bool continueRequested; // Set by the Continue button, consumed when the Main scene loads.

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(Key);
    }

    public static RunSaveData Load()
    {
        if (!PlayerPrefs.HasKey(Key)) return null;
        try
        {
            return JsonUtility.FromJson<RunSaveData>(PlayerPrefs.GetString(Key));
        }
        catch (Exception e)
        {
            Debug.LogWarning("[RunSave] Corrupt save ignored: " + e.Message);
            return null;
        }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }

    public static string Describe(RunSaveData d) // e.g. "Lv 7  -  12:34"
    {
        int minutes = Mathf.FloorToInt(d.timer / 60f);
        int seconds = Mathf.FloorToInt(d.timer % 60f);
        return "Lv " + d.level + "  -  " + minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    static string Clean(string objectName)
    {
        return objectName.Replace("(Clone)", "").Trim();
    }

    // ---------------------------------------------------------------- save

    public static void Save()
    {
        PlayerController player = PlayerController.instance;
        if (player == null || PlayerHealthController.instance == null || PlayerHealthController.instance.currentHealth <= 0f) return;

        RunSaveData d = new RunSaveData();
        d.characterIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
        d.timer = LevelManager.instance != null ? LevelManager.instance.timer : 0f;

        Vector3 pos = player.transform.position;
        d.posX = pos.x; d.posY = pos.y; d.posZ = pos.z;

        d.health = PlayerHealthController.instance.currentHealth;
        d.maxHealth = PlayerHealthController.instance.maxHealth;
        d.moveSpeed = player.moveSpeed;
        d.pickupRange = player.pickupRange;
        d.maxWeapons = player.maxWeapons;

        if (ExperienceLevelController.instance != null)
        {
            d.level = ExperienceLevelController.instance.currentLevel;
            d.experience = ExperienceLevelController.instance.currentExperience;
            d.queuedLevels = ExperienceLevelController.instance.queuedLevels;
        }
        if (CoinController.instance != null) d.runCoins = CoinController.instance.coinsCollectedThisRun;

        AddWeapons(d, player.unassignedWeapons, 0);
        AddWeapons(d, player.assignedWeapons, 1);
        AddWeapons(d, player.fullyLevelledWeapons, 2);

        if (UpgradeManager.instance != null)
        {
            foreach (UpgradeData card in UpgradeManager.instance.upgradeDatabase)
            {
                if (card is UpgradeStatUpgradeData stat && stat.currentLevel > 0)
                    d.cards.Add(new CardSave { name = card.name, level = stat.currentLevel });
                else if (card is WeaponEvolutionUpgradeData evo && evo.isEvolved)
                    d.cards.Add(new CardSave { name = card.name, evolved = true });
            }
        }

        PlayerPrefs.SetString(Key, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    static void AddWeapons(RunSaveData d, List<Weapon> list, int state)
    {
        foreach (Weapon w in list)
        {
            if (w == null) continue;
            WeaponSave ws = new WeaponSave { name = Clean(w.name), level = w.weaponLevel, state = state };
            ws.damages = new float[w.stats.Count];
            for (int i = 0; i < w.stats.Count; i++) ws.damages[i] = w.stats[i].damage;
            d.weapons.Add(ws);
        }
    }

    // ---------------------------------------------------------------- restore

    // Run by UIController when the Main scene starts after pressing Continue. Waits for the scene's own Start() calls first.
    public static IEnumerator Restore(RunSaveData d)
    {
        for (int i = 0; i < 120 && (PlayerController.instance == null || PlayerHealthController.instance == null
            || ExperienceLevelController.instance == null || LevelManager.instance == null); i++)
            yield return null;
        yield return null; // One more frame so every Start() (random starting weapon, exp table) has run.
        yield return null;

        PlayerController player = PlayerController.instance;
        if (player == null) yield break;

        if (UpgradeManager.instance == null) new GameObject("UpgradeManager").AddComponent<UpgradeManager>();

        // Position
        player.transform.position = new Vector3(d.posX, d.posY, d.posZ);
        if (player.theRB != null)
        {
            player.theRB.position = new Vector2(d.posX, d.posY);
            player.theRB.linearVelocity = Vector2.zero;
        }

        // Stats
        player.moveSpeed = d.moveSpeed;
        player.pickupRange = d.pickupRange;
        player.maxWeapons = d.maxWeapons;

        PlayerHealthController health = PlayerHealthController.instance;
        health.maxHealth = d.maxHealth;
        health.currentHealth = d.health;
        health.healthSlider.maxValue = d.maxHealth;
        health.healthSlider.value = d.health;

        RestoreWeapons(player, d);
        RestoreCards(d);

        // Level, timer, coins
        ExperienceLevelController exp = ExperienceLevelController.instance;
        exp.currentLevel = d.level;
        exp.currentExperience = d.experience;
        UIController.instance.UpdateExperience(exp.currentExperience, exp.expLevels[exp.currentLevel], exp.currentLevel);

        LevelManager.instance.timer = d.timer;
        UIController.instance.UpdateTimer(d.timer);
        if (CoinController.instance != null) CoinController.instance.coinsCollectedThisRun = d.runCoins;

        if (d.queuedLevels > 0) // Was saved while a level-up was pending
        {
            exp.queuedLevels = d.queuedLevels;
            exp.ShowLevelUp();
        }
    }

    static void RestoreWeapons(PlayerController player, RunSaveData d)
    {
        List<Weapon> all = new List<Weapon>();
        all.AddRange(player.assignedWeapons);
        all.AddRange(player.unassignedWeapons);
        all.AddRange(player.fullyLevelledWeapons);
        player.assignedWeapons.Clear();
        player.unassignedWeapons.Clear();
        player.fullyLevelledWeapons.Clear();
        foreach (Weapon w in all) if (w != null) w.gameObject.SetActive(false); // Anything not in the save was evolved away.

        foreach (WeaponSave ws in d.weapons)
        {
            Weapon w = all.Find(x => x != null && Clean(x.name) == ws.name);
            if (w == null) w = SpawnEvolvedWeapon(player, ws.name);
            if (w == null)
            {
                Debug.LogWarning("[RunSave] Weapon not found, skipped: " + ws.name);
                continue;
            }

            w.weaponLevel = Mathf.Clamp(ws.level, 0, w.stats.Count - 1);
            if (ws.damages != null && ws.damages.Length == w.stats.Count)
                for (int i = 0; i < w.stats.Count; i++) w.stats[i].damage = ws.damages[i];
            w.statsUpdated = true;

            switch (ws.state)
            {
                case 1:
                    player.assignedWeapons.Add(w);
                    w.gameObject.SetActive(true);
                    break;
                case 2:
                    player.fullyLevelledWeapons.Add(w);
                    w.gameObject.SetActive(true);
                    break;
                default:
                    player.unassignedWeapons.Add(w);
                    break;
            }
        }
    }

    // Evolved weapons only exist after an evolution card created them, so rebuild them from the evolution card's prefab.
    static Weapon SpawnEvolvedWeapon(PlayerController player, string weaponName)
    {
        if (UpgradeManager.instance == null) return null;
        foreach (UpgradeData card in UpgradeManager.instance.upgradeDatabase)
        {
            if (card is WeaponEvolutionUpgradeData evo && evo.evolvedWeaponName == weaponName && evo.evolvedWeaponPrefab != null)
            {
                GameObject obj = UnityEngine.Object.Instantiate(evo.evolvedWeaponPrefab, player.transform);
                obj.name = evo.evolvedWeaponName;
                obj.SetActive(false);
                evo.isEvolved = true;
                return obj.GetComponent<Weapon>();
            }
        }
        return null;
    }

    static void RestoreCards(RunSaveData d)
    {
        if (UpgradeManager.instance == null) return;
        foreach (CardSave cs in d.cards)
        {
            UpgradeData card = UpgradeManager.instance.upgradeDatabase.Find(c => c != null && c.name == cs.name);
            if (card is UpgradeStatUpgradeData stat) stat.currentLevel = cs.level;
            else if (card is WeaponEvolutionUpgradeData evo) evo.isEvolved = cs.evolved;
        }
    }
}
