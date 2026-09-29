using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Danh sách dữ liệu nhân vật")]
    [SerializeField] private CharacterData[] characters;

    [Header("UI Chi tiết bên dưới")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image characterPreview;
    [SerializeField] private Image weaponPreview;
    [SerializeField] private Button confirmButton;

    [Header("Nút quay lại")]
    [SerializeField] private Button backButton;
    [SerializeField] private GameObject mainMenuElements;

    private int currentSelectedIndex = 0;

    private void Awake()
    {
        // Gán sự kiện click một lần duy nhất lúc khởi tạo
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnEnable()
    {
        // Mỗi khi bảng được bật lên, luôn làm mới về tướng mặc định hoặc tướng hiện tại
        if (characters != null && characters.Length > 0)
        {
            SelectCharacter(currentSelectedIndex);
        }
    }

    public void SelectCharacter(int index)
    {
        if (characters == null || index < 0 || index >= characters.Length) return;
        currentSelectedIndex = index;

        CharacterData data = characters[currentSelectedIndex];
        if (data == null) return;

        // Cập nhật thông tin chữ
        if (nameText != null) nameText.text = data.characterName;
        if (descriptionText != null) descriptionText.text = data.passiveDescription;

        // Cập nhật ảnh đại diện nhân vật và chỉnh độ rõ nét tối đa
        if (characterPreview != null)
        {
            characterPreview.sprite = data.characterSprite;
            characterPreview.color = Color.white;
            characterPreview.preserveAspect = true;
        }

        // Cập nhật ảnh vũ khí
        if (weaponPreview != null)
        {
            weaponPreview.sprite = data.weaponSprite;
            weaponPreview.color = Color.white;
            weaponPreview.preserveAspect = true;
        }
    }

    private void OnConfirmClicked()
    {
        // Lưu index tướng đã chọn sang PlayerPrefs
        PlayerPrefs.SetInt("SelectedCharacterIndex", currentSelectedIndex);
        PlayerPrefs.Save();

        // Chuyển sang màn chơi chính
        SceneManager.LoadScene("Main");
    }

    private void OnBackClicked()
    {
        gameObject.SetActive(false);
        if (mainMenuElements != null)
        {
            mainMenuElements.SetActive(true);
        }
    }
}