// =============================================================================
// Assignment_EnemyDetector.cs
// -----------------------------------------------------------------------------
// 시야각과 거리 기반으로 적을 탐지하는 시스템
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Assignment_EnemyDetector : MonoBehaviour
{

    [Header("=== 감지 설정 ===")]
    [Tooltip("시야각 (전체 각도)")]
    [Range(10f, 360f)]
    [SerializeField] private float detectionFOV = 120f;

    [Tooltip("감지 거리")]
    [Range(1f, 30f)]
    [SerializeField] private float detectionRange = 10f;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text (Canvas 하위에 배치)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [Tooltip("현재 탐지된 적의 수")]
    [SerializeField] private int detectedCount = 0;

    private List<Transform> detectedEnemies = new List<Transform>();

    private GameObject[] allEnemies;

    private void Start()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"[EnemyDetector] 씬에서 {allEnemies.Length}개의 적을 발견했습니다.");
    }

    private void Update()
    {
        detectedEnemies.Clear();

        foreach (GameObject enemyObj in allEnemies)
        {
            if (enemyObj == null) continue;

            Transform enemy = enemyObj.transform;

            if (IsDetected(enemy))
            {
                detectedEnemies.Add(enemy);
            }

            Renderer renderer = enemyObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = detectedEnemies.Contains(enemy) ? Color.green : Color.red;
            }
        }

        detectedCount = detectedEnemies.Count;
        UpdateUI();
    }

    private bool IsDetected(Transform enemy)    // 적이 시야각과 거리 조건을 모두 만족하는지 확인
    {
        Vector3 toEnemy = enemy.position - transform.position;  // 플레이어와 적의 거리 차이
        float distance = toEnemy.magnitude; // magnitude로 벡터 길이 반환
        if (distance > detectionRange) return false; // detectionRange는 거리 감지 값으로 이 값보다 멀어지면 감지 실패

        Vector3 toEnemyDir = toEnemy.normalized; // 방향 벡터 (길이 1로 정규화)
        float angle = Vector3.Angle(transform.forward, toEnemyDir); // transform.forward는 플레이어의 전방 방향 벡터
        if (angle > detectionFOV * 0.5f) return false; // detectionFOV는 시야각으로 이 값의 절반보다 크면 감지 실패

        return true; // 거리와 시야각 조건 모두 만족
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 origin = transform.position;

        VectorGizmoHelper.DrawFOV(origin, transform.forward,
            detectionFOV * 0.5f, detectionRange, new Color(1f, 1f, 0f, 0.3f));

        VectorGizmoHelper.DrawCircleXZ(origin, detectionRange, new Color(1f, 1f, 1f, 0.2f));

        foreach (Transform enemy in detectedEnemies)
        {
            if (enemy != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, enemy.position);
            }
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        uiInfoText.text =
            $"[과제] 적 감지 시스템\n" +
            $"탐지된 적: {detectedCount}마리\n" +
            $"FOV: {detectionFOV}° / 거리: {detectionRange}";
    }
}
