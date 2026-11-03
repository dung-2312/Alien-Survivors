using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Survivors/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    [TextArea(2, 4)]
    public string passiveDescription;
    public Sprite characterSprite;
    public Sprite weaponSprite;
    public GameObject characterPrefab; // Prefab nhân vật sẽ xuất hiện trong game

    [Header("Mở khóa tướng")]
    public int unlockCost = 0; // 0 = mặc định đã mở khóa

    [Header("Hoạt Ảnh Bước Chân & Di Chuyển")]
    public Sprite[] walkFrames; // Các khung hình bước đi/chạy của nhân vật
    public Sprite[] idleFrames; // Khung hình đứng thở nhịp nhàng
    public float walkFrameRate = 10f; // Tốc độ chạy khung hình bước chân
    public float idleFrameRate = 5f;

    [Header("Kích thước hiển thị trong game")]
    [Tooltip("Hệ số nhân kích thước nhân vật (1.0 = mặc định chuẩn hóa 2.4 units)")]
    public float customScaleMultiplier = 1.0f;

    [Header("Vũ khí khởi đầu đặc trưng")]
    [Tooltip("Tên vũ khí khởi đầu độc quyền của nhân vật này (ví dụ: Giant Sword, Orbitting Fireball, Throwing Axes, Dagger Throw, Bright Zone, Forest Bow)")]
    public string signatureWeaponName;

    [Header("Hướng mặt Sprite mặc định")]
    [Tooltip("Nếu sprite gốc vẽ quay sang trái (ví dụ Bandit), tích chọn để đảo chiều di chuyển cho đúng hướng")]
    public bool defaultSpriteFacesLeft = false;
}
