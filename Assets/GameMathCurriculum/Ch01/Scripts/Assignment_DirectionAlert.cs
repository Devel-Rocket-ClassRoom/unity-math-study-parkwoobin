// =============================================================================
// Assignment_DirectionAlert.cs
// -----------------------------------------------------------------------------
// 외적으로 적의 접근 방향을 감지하고 알려주는 시스템
// =============================================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;


public class Assignment_DirectionAlert : MonoBehaviour
{

    public enum Direction
    {
        None,
        Front,
        Back,
        Left,
        Right
    }

    [System.Serializable]
    public struct EnemyInfo
    {
        public Transform transform;
        public Direction direction;
        public float distance;
    }

    [Header("=== 감지 설정 ===")]
    [Tooltip("적 감지 반경")]
    [Range(1f, 30f)]
    [SerializeField] private float alertRange = 15f;

    [Header("=== 전 후방 설정 ===")]
    [Tooltip("전 감지 변경")]
    [Range(0.05f, 5f)]
    [SerializeField] private float sideThreshold = 0.3f;

    [Header("=== UI 연결 ===")]
    [Tooltip("정보 표시용 TMP_Text (Canvas 하위에 배치)")]
    [SerializeField] private TMP_Text uiInfoText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private List<EnemyInfo> nearbyEnemies = new List<EnemyInfo>();

    private GameObject[] allEnemies;

    private void Start()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
    }

    private void Update()
    {
        nearbyEnemies.Clear();

        foreach (GameObject enemyObj in allEnemies)
        {
            if (enemyObj == null) continue;

            Transform enemy = enemyObj.transform;
            float distance = Vector3.Distance(transform.position, enemy.position);

            if (distance > alertRange) continue;

            Direction dir = GetDirection(enemy);

            nearbyEnemies.Add(new EnemyInfo
            {
                transform = enemy,
                direction = dir,
                distance = distance
            });
        }

        UpdateUI();
    }

    private Direction GetDirection(Transform enemy) // 적이 플레이어의 어느 방향에 있는지 판별
    {
        Vector3 toEnemy = enemy.position - transform.position;  // 플레이어에서 적으로 향하는 벡터
        toEnemy.y = 0f; // 수직 방향 무시 (XZ 평면에서만 판단)

        if (toEnemy == Vector3.zero)
        {
            return Direction.None; // 같은 위치에 있으면 방향 없음
        }

        Vector3 cross = Vector3.Cross(transform.forward, toEnemy.normalized);  // 외적
        float dot = Vector3.Dot(transform.forward, toEnemy.normalized);    // 내적

        if (MathF.Abs(cross.y) > sideThreshold)
        {
            return cross.y > 0f ? Direction.Left : Direction.Right; // 외적의 Y값으로 좌우 판단
        }

        return dot > 0f ? Direction.Front : Direction.Back; // 내적의 부호로 전후 판단
    }

    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 origin = transform.position;

        VectorGizmoHelper.DrawCircleXZ(origin, alertRange, Color.yellow);

        VectorGizmoHelper.DrawArrow(origin, origin + transform.forward * 3f, Color.cyan, 0.3f);

        foreach (EnemyInfo info in nearbyEnemies)
        {
            if (info.transform == null) continue;

            Color dirColor = GetDirectionColor(info.direction);
            Gizmos.color = dirColor;
            Gizmos.DrawLine(origin, info.transform.position);
            Gizmos.DrawWireSphere(info.transform.position, 0.5f);

#if UNITY_EDITOR
            VectorGizmoHelper.DrawLabel(info.transform.position + Vector3.up * 1.5f,
                GetDirectionText(info.direction), dirColor);
#endif
        }
    }

    private void UpdateUI()
    {
        if (uiInfoText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[심화 과제] 방향 알림 시스템 — 범위 내 적: {nearbyEnemies.Count}");

        foreach (EnemyInfo info in nearbyEnemies)
        {
            sb.AppendLine(GetAlertText(info));
        }

        uiInfoText.text = sb.ToString();
    }

    private Color GetDirectionColor(Direction dir)
    {
        switch (dir)
        {
            case Direction.Front: return Color.green;
            case Direction.Back: return Color.red;
            case Direction.Left: return new Color(1f, 0.5f, 0f);
            case Direction.Right: return Color.cyan;
            default: return Color.gray;
        }
    }

    private string GetDirectionText(Direction dir)
    {
        switch (dir)
        {
            case Direction.Front: return "전방";
            case Direction.Back: return "후방";
            case Direction.Left: return "좌측";
            case Direction.Right: return "우측";
            default: return "???";
        }
    }

    private string GetAlertText(EnemyInfo info)
    {
        string dirText = GetDirectionText(info.direction);
        string colorTag;
        switch (info.direction)
        {
            case Direction.Front: colorTag = "green"; break;
            case Direction.Back: colorTag = "red"; break;
            case Direction.Left: colorTag = "orange"; break;
            case Direction.Right: colorTag = "cyan"; break;
            default: colorTag = "gray"; break;
        }
        return $"  <color={colorTag}>▶ {dirText}에서 적 접근! (거리: {info.distance:F1})</color>";
    }
}
