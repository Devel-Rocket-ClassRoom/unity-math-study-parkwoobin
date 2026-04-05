// =============================================================================
// Assignment_BulletHell.cs
// -----------------------------------------------------------------------------
// 삼각함수를 이용한 다양한 탄막 패턴 생성 및 발사
// =============================================================================

using UnityEngine;
using TMPro;

public class Assignment_BulletHell : MonoBehaviour
{
    public enum PatternType
    {
        Circle,
        Spiral,
        Fan
    }
    [Header("=== 탄막 설정 ===")]
    [SerializeField] private GameObject bulletPrefab;
    [Tooltip("발사 탄막 개수 (8~36권장)")]
    [Range(8, 36)]
    [SerializeField] private int bulletCount = 16;
    [Tooltip("발사된 탄막 속도 (단위/초)")]
    [Range(1f, 20f)]
    [SerializeField] private float bulletSpeed = 10f;
    [Tooltip("다음 발사까지 대기시간 (초)")]
    [Range(0.1f, 2f)]
    [SerializeField] private float fireInterval = 0.5f;


    [Header("=== 패턴 선택 ===")]
    [SerializeField] private PatternType patternType = PatternType.Circle;

    [Header("=== 나선형 패턴 파라미터 ===")]
    [Tooltip("나선형 회전 속도 (라디안/초)")]
    [Range(0.5f, 5f)]
    [SerializeField] private float spiralTurnSpeed = 2f;

    [Header("=== 부채꼴 패턴 파라미터 ===")]
    [Tooltip("부채꼴 각도 범위 (도, 360까지)")]
    [Range(30f, 360f)]
    [SerializeField] private float fanAngle = 120f;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private TextMeshProUGUI debugUI;
    [SerializeField] private float fireTimer = 0f;

    private void Start()
    {
        fireTimer = 0f;
    }

    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            FireBulletPattern();
            fireTimer = fireInterval;
        }

        UpdateDebugUI();
    }

    private void FireBulletPattern()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[BulletHell] bulletPrefab이 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 direction = patternType switch
            {
                PatternType.Circle => CalculateCircleDirection(i, bulletCount),
                PatternType.Spiral => CalculateSpiralDirection(i, bulletCount),
                PatternType.Fan => CalculateFanDirection(i, bulletCount),
                _ => Vector3.forward
            };

            if ((patternType == PatternType.Spiral || patternType == PatternType.Fan) && direction == Vector3.zero)
                continue;

            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Destroy(bullet, 1f);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }
        }
    }

    private Vector3 CalculateCircleDirection(int index, int total)  // 원형 패턴의 방향 계산
    {
        float angleDegrees = index * 360f / total + 90f;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)).normalized;

        return direction;
    }

    private Vector3 CalculateSpiralDirection(int index, int total)  // 나선형 패턴의 방향 계산
    {
        if (total <= 0) return Vector3.forward;

        // 발사 타이밍 기준으로 현재 쏠 탄의 인덱스를 계산해 한 발씩만 발사한다.
        int currentShotIndex = Mathf.FloorToInt(Time.time / fireInterval) % total;
        if (index != currentShotIndex)
            return Vector3.zero;

        float angleDegrees = currentShotIndex * (360f / total) + Time.time * spiralTurnSpeed * Mathf.Rad2Deg;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)).normalized;

        return direction;

    }

    private Vector3 CalculateFanDirection(int index, int total) // 부채꼴 패턴의 방향 계산
    {
        if (total <= 0) return Vector3.forward;

        // 한 번 발사 시 total개 탄이 fanAngle 범위 안에서 부채꼴로 퍼지도록 계산한다.
        float t = (total == 1) ? 0.5f : (float)index / (total - 1);
        float angleDegrees = Mathf.Lerp(-fanAngle * 0.5f, fanAngle * 0.5f, t) + 90f;
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)).normalized;

        return direction;

    }

    private void UpdateDebugUI()
    {
        if (debugUI == null) return;

        debugUI.text = $"<b>[BulletHell]</b>\n" +
            $"패턴: <color=yellow>{patternType}</color>\n" +
            $"탄막개수: {bulletCount}\n" +
            $"발사속도: {bulletSpeed:F1} u/s\n" +
            $"다음 발사: {fireTimer:F2}s";

        if (patternType == PatternType.Spiral)
            debugUI.text += $"\n나선속도: {spiralTurnSpeed:F2} rad/s";
        else if (patternType == PatternType.Fan)
            debugUI.text += $"\n부채꼴각도: {fanAngle:F0}°";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.6f);
        float previewRadius = 3f;

        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 direction = patternType switch
            {
                PatternType.Circle => CalculateCircleDirection(i, bulletCount),
                PatternType.Spiral => CalculateSpiralDirection(i, bulletCount),
                PatternType.Fan => CalculateFanDirection(i, bulletCount),
                _ => Vector3.forward
            };

            Vector3 endPos = transform.position + direction * previewRadius;
            Gizmos.DrawLine(transform.position, endPos);
            Gizmos.DrawSphere(endPos, 0.15f);
        }
    }
#endif
}
