using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrineManager : MonoBehaviour
{
    public static ShrineManager instance;

    [Header("Spawn Settings")]
    public float timeBetweenShrines = 45f;
    public int maxActiveShrines = 2;
    private float spawnTimer = 25f; // Đàn tế đầu tiên xuất hiện sau 25s

    private List<AncientShrine> activeShrines = new List<AncientShrine>();

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        // Xóa các shrine đã bị hủy
        activeShrines.RemoveAll(s => s == null);

        if (activeShrines.Count < maxActiveShrines)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = timeBetweenShrines;
                SpawnRandomShrine();
            }
        }
    }

    /// <summary>
    /// Xuất hiện 1 đàn tế ngẫu nhiên quanh người chơi
    /// </summary>
    public void SpawnRandomShrine()
    {
        if (PlayerController.instance == null) return;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float distance = Random.Range(8f, 13f);
        Vector3 spawnPos = PlayerController.instance.transform.position + (Vector3)(randomDir * distance);

        GameObject shrineObj = new GameObject("AncientShrine", typeof(AncientShrine));
        shrineObj.transform.position = spawnPos;

        AncientShrine shrine = shrineObj.GetComponent<AncientShrine>();
        ShrineType randomType = (ShrineType)Random.Range(0, 3);
        shrine.InitShrine(randomType);

        activeShrines.Add(shrine);

        if (UIController.instance != null)
        {
            string shrineName = randomType == ShrineType.FrostChronos ? "CHRONOS FROST" : 
                               (randomType == ShrineType.MidasWealth ? "MIDAS GOLD" : "BLOOD MOON");
            UIController.instance.ShowWarning($"[ AN ANCIENT {shrineName} SHRINE HAS MANIFESTED! ]", 2.0f);
        }
    }
}
