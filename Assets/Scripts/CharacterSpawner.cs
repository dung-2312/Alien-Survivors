using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Danh sách dữ liệu nhân vật")]
    [SerializeField] private CharacterData[] characters;

    [Header("Vị trí xuất hiện")]
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Chưa gán danh sách Characters vào CharacterSpawner!");
            return;
        }

        if (selectedIndex < 0 || selectedIndex >= characters.Length)
        {
            selectedIndex = 0;
        }

        CharacterData selectedData = characters[selectedIndex];

        // Dùng characterPrefab chữ thường
        if (selectedData != null && selectedData.characterPrefab != null)
        {
            Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
            Quaternion rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

            GameObject spawnedPlayer = Instantiate(selectedData.characterPrefab, pos, rot);
            spawnedPlayer.name = "Player";
        }
        else
        {
            Debug.LogError($"Thiếu Character Prefab trong dữ liệu tướng tại index {selectedIndex}!");
        }
    }
}