using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    private Transform target;

    [Header("Screen Shake Settings")]
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.2f;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Kích hoạt rung màn hình trong khoảng thời gian duration với biên độ magnitude.
    /// </summary>
    public void Shake(float duration = 0.2f, float magnitude = 0.25f)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    void LateUpdate()
    {
        // Nếu chưa tìm thấy target, liên tục tìm PlayerController vừa được sinh ra
        if (target == null)
        {
            if (PlayerController.instance != null)
            {
                target = PlayerController.instance.transform;
            }
            return; // Chưa có target thì dừng frame này, không chạy dòng dưới để tránh lỗi
        }

        // Đã có target: bám theo vị trí Player
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Hiệu ứng rung màn hình mượt mà
        if (shakeDuration > 0f)
        {
            Vector2 randomOffset = Random.insideUnitCircle * shakeMagnitude;
            targetPos.x += randomOffset.x;
            targetPos.y += randomOffset.y;
            shakeDuration -= Time.deltaTime;
        }

        transform.position = targetPos;
    }
}
