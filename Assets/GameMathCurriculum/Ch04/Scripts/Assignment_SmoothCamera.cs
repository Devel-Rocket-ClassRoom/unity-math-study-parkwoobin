// =============================================================================
// Assignment_SmoothCamera.cs
// -----------------------------------------------------------------------------
// SmoothDamp로 부드러운 3인칭 카메라 팔로우와 줌 인/아웃을 구현하는 시스템
// =============================================================================

using UnityEngine;
using TMPro;
public class Assignment_SmoothCamera : MonoBehaviour
{

    [Header("=== 추적 대상 ===")]
    [Tooltip("카메라가 따라갈 플레이어(타겟)")]
    [SerializeField] private Transform target;

    [Tooltip("타겟으로부터 카메라의 오프셋(상대 위치)")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("=== SmoothDamp 설정 ===")]
    [Tooltip("위치 보간 부드러움 정도 (초, 작을수록 빠름)")]
    [Range(0.01f, 1f)]
    [SerializeField] private float positionSmoothTime = 0.3f;

    [Tooltip("줌 거리 보간 부드러움 정도")]
    [Range(0.01f, 1f)]
    [SerializeField] private float zoomSmoothTime = 0.2f;

    [Tooltip("회전 보간 속도 (높을수록 빠르게 회전)")]
    [Range(1f, 20f)]
    [SerializeField] private float rotationSmoothSpeed = 5f;

    [Header("=== 줌 설정 ===")]
    [Tooltip("최소 줌 거리")]
    [Range(2f, 10f)]
    [SerializeField] private float minZoomDistance = 3f;

    [Tooltip("최대 줌 거리")]
    [Range(10f, 30f)]
    [SerializeField] private float maxZoomDistance = 15f;

    [Tooltip("마우스 휠 줌 속도")]
    [Range(1f, 10f)]
    [SerializeField] private float zoomSpeed = 3f;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [Tooltip("현재 줌 거리")]
    [SerializeField] private float currentZoomDistance = 10f;

    [Tooltip("SmoothDamp 내부 위치 속도 (읽기 전용)")]
    [SerializeField] private Vector3 currentSmoothVelocity = Vector3.zero;

    [Tooltip("SmoothDamp 내부 줌 속도 (읽기 전용)")]
    [SerializeField] private float zoomVelocity = 0f;

    private Vector3 positionVelocity = Vector3.zero;
    private float targetZoomDistance;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("[Assignment_SmoothCamera] Target이 지정되지 않았습니다!");
            return;
        }

        targetZoomDistance = currentZoomDistance;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // 마우스 휠 입력으로 줌 거리 조절하고 min/max 범위로 클램프
        if (scrollInput != 0f)
        {
            targetZoomDistance -= scrollInput * zoomSpeed;
            targetZoomDistance = Mathf.Clamp(targetZoomDistance, minZoomDistance, maxZoomDistance);
        }

        // SmoothDamp로 줌 거리 보간
        currentZoomDistance = Mathf.SmoothDamp(currentZoomDistance, targetZoomDistance, ref zoomVelocity, zoomSmoothTime);

        // // 카메라 목표 위치 계산
        Vector3 desiredPosition = target.position + offset.normalized * currentZoomDistance + Vector3.up * offset.y;
        // // normalized해서 방향만 유지하고, 거리 조절은 줌 거리로 하는 방식
        // // Vector3.up * offset.y로 높이 오프셋을 별도로 적용해서, 줌 거리에 상관없이 일정한 높이를 유지하도록 함

        // // SmoothDamp로 카메라 위치 보간
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref positionVelocity, positionSmoothTime);

        // // 카메라 회전을 목표 방향에 Slerp로 보간
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        //rotationSmoothSpeed를 곱해서 프레임 레이트에 독립적인 회전 속도를 구현

        currentSmoothVelocity = positionVelocity;   // 디버그용으로 현재 위치 속도 저장


        UpdateUI();
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (target == null) return;

        Vector3 camPos = transform.position;
        Vector3 desiredPos = target.position + offset.normalized * currentZoomDistance + Vector3.up * offset.y;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(camPos, target.position);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(desiredPos, 0.5f);

        Gizmos.color = Color.yellow;
        DrawCircleXZ(target.position + Vector3.up * offset.y, currentZoomDistance, 24);
    }

    private void DrawCircleXZ(Vector3 center, float radius, int segments = 32)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        uiInfoText.text =
            $"[심화 과제] SmoothDamp 카메라\n" +
            $"줌 거리: {currentZoomDistance:F2} m\n" +
            $"위치 속도: {positionVelocity.magnitude:F3}\n" +
            $"마우스 휠: 줌 인/아웃\n" +
            $"회전 속도: {rotationSmoothSpeed:F1}\n" +
            $"SmoothTime: {positionSmoothTime:F2}s";
    }
}
