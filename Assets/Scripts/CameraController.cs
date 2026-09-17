using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target;

    void LateUpdate()
    {
        // Nếu chưa tìm thấy target, liên tục tìm PlayerController vừa được sinh ra
        if (target == null)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
            return; // Chưa có target thì dừng frame này, không chạy dòng dưới để tránh lỗi
        }

        // Đã có target: bám theo vị trí Player
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}



