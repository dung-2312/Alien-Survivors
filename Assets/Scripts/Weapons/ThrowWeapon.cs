using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowWeapon : MonoBehaviour
{
    public float throwPower; // The power of the throw. GK
    public Rigidbody2D theRB; // The rigidbody of the weapon. GK
    public float rotateSpeed; // The speed of rotation. GK
    // Start is called before the first frame update
    void Start()
    {
        // Ném rìu theo hướng nhìn của nhân vật tạo đường bay vòng cung tự nhiên
        float facing = PlayerController.instance != null ? PlayerController.instance.facingDirection : 1f;
        float hPower = facing * Random.Range(throwPower * 0.45f, throwPower * 1.05f);
        theRB.linearVelocity = new Vector2(hPower, throwPower * Random.Range(0.85f, 1.25f));
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f,0f,transform.rotation.eulerAngles.z + (rotateSpeed * 360f * Time.deltaTime * Mathf.Sign(theRB.linearVelocity.x))); // Rotate the weapon. GK
    }
}
