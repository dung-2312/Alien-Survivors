using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    public Transform sprite; // The sprite object. AK
    public float speed = 1.5f; // The speed at which the sprite animates. AK
    public float minSize = 0.92f, maxSize = 1.08f; // The minimum and maximum size of the sprite. AK
    private float activeSize; // The size of the sprite. AK

    private float initialZRotation;
    private Vector3 initialLocalPos;
    private float animPhaseOffset;

    void Awake()
    {
        if (sprite == null)
        {
            Transform found = transform.Find("Sprite");
            if (found != null) sprite = found;
            else
            {
                SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sprite = sr.transform;
            }
        }
    }

    void Start()
    {
        activeSize = maxSize;
        if (speed < 1.2f) speed = 1.6f;
        speed = speed * Random.Range(0.85f, 1.25f);
        animPhaseOffset = Random.value * 10f;

        if (sprite != null)
        {
            initialZRotation = sprite.localEulerAngles.z;
            initialLocalPos = sprite.localPosition;
        }
    }

    void Update()
    {
        if (sprite == null) return;

        // 1. Nhịp thở co giãn sinh động (Breathing squash and stretch)
        sprite.localScale = Vector3.MoveTowards(sprite.localScale, Vector3.one * activeSize, speed * 2.2f * Time.deltaTime);

        if (Mathf.Approximately(sprite.localScale.x, activeSize))
        {
            activeSize = (activeSize == maxSize) ? minSize : maxSize;
        }

        // 2. Nhịp vẫy cánh / nhún nhảy tự nhiên chống tình trạng đơ cứng (Organic Flap & Step Tilt)
        float flapFrequency = speed * 8.5f;
        float wobble = Mathf.Sin(Time.time * flapFrequency + animPhaseOffset) * 7.5f;
        sprite.localRotation = Quaternion.Euler(0, 0, initialZRotation + wobble);

        // 3. Nhấp nhô trục Y tạo cảm giác bước chân hoặc bay lơ lửng
        float bob = Mathf.Sin(Time.time * flapFrequency * 0.7f + animPhaseOffset) * 0.06f;
        sprite.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + bob, initialLocalPos.z);
    }
}