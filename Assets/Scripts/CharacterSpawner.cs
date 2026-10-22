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

            // Cập nhật Sprite hiển thị theo tướng đã chọn
            if (selectedData.characterSprite != null)
            {
                Transform spriteChild = spawnedPlayer.transform.Find("Sprite");
                if (spriteChild == null && spawnedPlayer.TryGetComponent<SpriteRenderer>(out _))
                {
                    spriteChild = spawnedPlayer.transform;
                }

                if (spriteChild != null && spriteChild.TryGetComponent<SpriteRenderer>(out var sr))
                {
                    sr.sprite = selectedData.characterSprite;
                }
            }

            // Khởi tạo hệ thống bước chân linh hoạt, khung hình chạy và chuẩn hóa kích thước to rõ
            PlayerController pc = spawnedPlayer.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.InitCharacter(selectedData);
            }
        }
        else
        {
            Debug.LogError($"Thiếu Character Prefab trong dữ liệu tướng tại index {selectedIndex}!");
        }
    }
}
