// =============================================================================
// TurretAim.cs
// -----------------------------------------------------------------------------
// atan2로 터렛이 타겟을 추적 조준
// =============================================================================

using UnityEngine;
using TMPro;


public class TurretAim : MonoBehaviour
{
    [Header("=== 타겟 설정 ===")]
    [SerializeField] private Transform target;
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private bool lockYAxis = true;

    [Header("=== 감지 범위 ===")]
    [SerializeField] private float detectionRange = 20f;
    [SerializeField] private bool drawDetectionCircle = true;

    [Header("=== UI 텍스트 ===")]
    [SerializeField] private TextMeshProUGUI uiText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private Vector3 directionToTarget;
    [SerializeField] private float distanceToTarget;
    [SerializeField] private float targetAngleDegrees;
    [SerializeField] private float targetAngleRadians;
    [SerializeField] private bool targetInRange;

    private void Update()
    {
        if (target == null)
        {
            UpdateUI();
            return;
        }

        directionToTarget = target.position - transform.position;   // 타겟으로 향하는 벡터 계산
        distanceToTarget = directionToTarget.magnitude; // 타겟과의 거리 계산
        targetInRange = distanceToTarget <= detectionRange; // 타겟이 감지 범위 내에 있는지 여부



        if (!targetInRange)
        {
            UpdateUI();
            return;
        }

        directionToTarget.Normalize(); // 방향 벡터 정규화
        targetAngleRadians = Mathf.Atan2(directionToTarget.z, directionToTarget.x); // atan2로 타겟 각도 계산
        targetAngleDegrees = targetAngleRadians * Mathf.Rad2Deg; // 라디안을 각도로 변환


        RotateTowardTarget();
        UpdateUI();
    }

    private void RotateTowardTarget()
    {
        Quaternion targetQuaternion = Quaternion.Euler(0f, 90f - targetAngleDegrees, 0f);   // 타겟 방향으로 회전할 쿼터니언 계산 (Y축 기준)
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetQuaternion, rotationSpeed * Time.deltaTime); // 현재 회전에서 타겟 회전으로 부드럽게 회전
    }

    private void UpdateUI()
    {
        if (uiText == null) return;

        if (target == null)
        {
            uiText.text = $"<b>[터렛 조준]</b>\n<color=red>타겟: 없음</color>";
            return;
        }

        string rangeStatus = targetInRange
            ? $"<color=green>범위 내</color>"
            : $"<color=red>범위 외</color>";

        uiText.text = $"<b>[터렛 조준]</b>\n" +
                     $"거리: {distanceToTarget:F2}u ({rangeStatus})\n" +
                     $"각도(°): {targetAngleDegrees:F1}°\n" +
                     $"각도(rad): {targetAngleRadians:F3}\n" +
                     $"방향: ({directionToTarget.x:F2}, {directionToTarget.y:F2}, {directionToTarget.z:F2})\n" +
                     $"감지 범위: {detectionRange}u";
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        if (!Application.isPlaying) return;
        if (target == null) return;

        if (drawDetectionCircle)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
            VectorGizmoHelper.DrawCircleXZ(transform.position, detectionRange, Color.yellow, 32);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(target.position, 0.15f);

        if (!targetInRange) return;

        VectorGizmoHelper.DrawArrow(
            transform.position,
            target.position,
            Color.cyan
        );

        float currentYaw = transform.eulerAngles.y;
        float angleDiff = Mathf.DeltaAngle(currentYaw, 90f - targetAngleDegrees);

        VectorGizmoHelper.DrawFOV(
            transform.position,
            transform.forward,
            Mathf.Abs(angleDiff) * 0.5f,
            2f,
            new Color(0f, 1f, 1f, 0.5f),
            16
        );

        VectorGizmoHelper.DrawLabel(
            Vector3.Lerp(transform.position, target.position, 0.5f) + Vector3.up * 0.3f,
            $"d={distanceToTarget:F1}u\nθ={targetAngleDegrees:F0}°",
            Color.white
        );
    }
}
