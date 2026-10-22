using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Transform sprite; // The sprite object. AK
    public float speed; // The speed at which the sprite rotates. AK
    public float minSize, maxSize; // The minimum and maximum size of the sprite. AK
    private float activeSize; // The size of the sprite. AK

    private float initialZRotation;

    void Start()
    {
        activeSize = maxSize; // Set the active size to the maximum size. AK
        speed = speed * Random.Range(.75f, 1.25f); // Randomize the speed. AK
        if (sprite != null) initialZRotation = sprite.localEulerAngles.z;
    }

    void Update()
    {
        if (sprite == null) return;
        sprite.localScale = Vector3.MoveTowards(sprite.localScale, Vector3.one * activeSize, speed * Time.deltaTime); // Change the size of the sprite. AK

        if(Mathf.Approximately(sprite.localScale.x, activeSize)) // If the sprite has reached the active size. AK
        {
            if(activeSize == maxSize) // If the active size is the maximum size. AK
            {
                activeSize = minSize; // Set the active size to the minimum size. AK
            }
            else // If the active size is the minimum size. AK
            {
                activeSize = maxSize; // Set the active size to the maximum size. AK
            }
        }

        // Nhịp lắc lư nhẹ nhàng tự nhiên tạo cảm giác sinh vật sống động
        float wobble = Mathf.Sin(Time.time * speed * 3.2f) * 3.0f;
        sprite.localRotation = Quaternion.Euler(0, 0, initialZRotation + wobble);
    }
}