using UnityEngine;

public class OffScreenIndicator : MonoBehaviour
{
    public Transform target; // 추적할 대상

    private Vector3 offset = new Vector3(0f, 5f, -8f);

    private float rotationSpeed = 180f; // 회전 속도 (도/초)

    private float positionSmoothTime = 0.03f;    // 카메라 위치 보간 부드러움 정도 (초, 작을수록 빠름)
    private Vector3 positionVelocity = Vector3.zero;    // 카메라 위치 보간 속도

    private float rotationSmoothSpeed = 180f;   // 카메라 회전 보간 속도

    private void Awake()
    {

    }

    private void LateUpdate()
    {
        float rotation = Input.GetAxis("Rotation"); //Q/E 키로 회전 입력 받기
        if (rotation != 0f)
        {
            Quaternion rotate = Quaternion.Euler(0f, rotation * rotationSpeed * Time.deltaTime, 0f);
            offset = rotate * offset;
        }

        Vector3 targetPosition = target.position + offset; // 타겟 위치에 오프셋을 더해서 원하는 카메라 위치 계산

        // // SmoothDamp로 카메라 위치 보간
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, positionSmoothTime);

        // // 카메라 회전을 목표 방향에 Slerp로 보간
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);

    }
}
