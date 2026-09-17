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
}