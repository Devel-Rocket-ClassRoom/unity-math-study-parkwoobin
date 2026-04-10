using UnityEngine;

public class PlayerMove : MonoBehaviour
{


    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Rotation");


        // Capsule 회전
        transform.Rotate(0f, rotation * 180f * Time.deltaTime, 0f);

        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical);

        transform.Translate(moveDirection * Time.deltaTime * 5f);
    }
}
