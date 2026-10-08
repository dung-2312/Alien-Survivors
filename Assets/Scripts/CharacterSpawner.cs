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

                    // Chuẩn hóa kích thước nhân vật trong màn chơi để hiển thị to rõ ràng, cân đối
                    float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                    if (spriteHeight > 0.05f)
                    {
                        float targetHeight = 1.8f; // Chiều cao chuẩn nhân vật trong game
                        float s = targetHeight / spriteHeight;
                        spriteChild.localScale = new Vector3(s, s, 1f);
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Thiếu Character Prefab trong dữ liệu tướng tại index {selectedIndex}!");
        }
    }
}