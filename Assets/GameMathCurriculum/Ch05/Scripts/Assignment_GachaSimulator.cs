// =============================================================================
// Assignment_GachaSimulator.cs
// -----------------------------------------------------------------------------
// 목적: 천장(Pity) 메커니즘이 있는 뽑기(Gacha) 시스템 구현 (과제 스켈레톤)
// ★ 과제 설명:
//    기본 SSR(5성) 확률에 천장 메커니즘을 적용하는 확률 시스템.
// 사용법: 빈 GameObject에 부착, TMP_Text UI 연결, Inspector에서 파라미터 설정
// 씬: Ch05_Random
// 수업 단계: 심화② — 천장 메커니즘 (50분)
// =============================================================================

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Assignment_GachaSimulator : MonoBehaviour
{
    [Header("=== 뽑기 파라미터 ===")]
    [SerializeField][Range(0.001f, 0.1f)] private float baseRate = 0.006f;
    [Tooltip("이 횟수 이후부터 확률 상승 시작")]
    [SerializeField][Range(1, 70)] private int softPityStart = 61;
    [Tooltip("이 횟수에서 100% 보장")]
    [SerializeField][Range(1, 90)] private int hardPity = 90;

    [Header("=== UI 참조 ===")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text pullHistoryText;
    [SerializeField] private TMP_Text expectedValueText;

    [Header("=== 디버그 정보 (읽기 전용) ===")]
    [SerializeField] private int totalPulls;
    [SerializeField] private int totalSSRs;
    [SerializeField] private int currentPityCount;
    [SerializeField] private float currentEffectiveRate;
    [SerializeField] private List<bool> pullHistory = new List<bool>(); // 최근 뽑기 결과 기록 (true=SSR, false=일반)
    [SerializeField] private List<int> ssrPityList = new List<int>();   // SSR 획득 시의 피티 카운트 기록
    private const int MAX_HISTORY = 30;

    private void Start()
    {
        if (statusText == null || pullHistoryText == null || expectedValueText == null)
        {
            Debug.LogError("[Assignment_GachaSimulator] TMP_Text UI가 할당되지 않았습니다.");
            return;
        }

        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PerformSinglePull();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PerformTenPulls();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            PerformHardPityPulls();
        }
    }

    private void PerformSinglePull()
    {
        ExecutePull();
    }

    private void PerformTenPulls()
    {
        for (int i = 0; i < 10; i++)
        {
            ExecutePull();
        }
    }

    private void PerformHardPityPulls()
    {
        while (true)
        {
            bool isSSR = ExecutePull();
            if (isSSR)
            {
                break;  // SSR 획득 시 루프 종료
            }
        }
    }

    private bool ExecutePull()  // 실제 뽑기 로직 구현
    {
        // 1. 기본 SSR 확률 0.6%, 소프트 피티 시작 61회, 하드 피티(확정) 90회
        // 2. 소프트 피티 구간에서는 확률이 선형으로 증가하여 하드 피티 직전에 100%에 근접한다
        // 3. 하드 피티 도달 시 확률 100%로 SSR 확정
        // 4. SSR 획득 시 피티 카운터를 0으로 리셋한다
        // 5. 키 입력: Space(1회 뽑기), R(10연차), T(천장까지 뽑기)
        // 6. UI에 총 뽑기 수, SSR 획득 수, 현재 천장 진행도, 유효 확률, 뽑기 기록을 표시한다


        totalPulls++;   // 총 뽑기 수 증가

        currentEffectiveRate = Mathf.Clamp01(currentEffectiveRate); // 확률이 0~1 범위를 벗어나지 않도록 클램프

        int pityThisPull = currentPityCount + 1;    // 이번 뽑기에서의 피티 카운트 (현재 카운트 + 이번 뽑기)

        if (pityThisPull == hardPity)   // 3. 하드 피티 도달 시 확률 100%로 SSR 확정
        {
            currentEffectiveRate = 1f;  // 확률 100%로 설정
        }
        else if (pityThisPull >= softPityStart) // 2. 소프트 피티 구간에서는 확률이 선형으로 증가하여 하드 피티 직전에 100%에 근접한다
        {
            float t = (float)(pityThisPull - softPityStart) / (hardPity - softPityStart);   // softPityStart부터 hardPity까지의 진행도 (0~1)
            currentEffectiveRate = Mathf.Lerp(baseRate, 1f, t); // softPityStart에서 baseRate, hardPity에서 1이 되도록 선형 보간
        }
        else
        {
            currentEffectiveRate = baseRate;    // 1. 기본 SSR 확률 0.6%, 소프트 피티 시작 61회, 하드 피티(확정) 90회
        }



        bool isSSR = Random.value < currentEffectiveRate;   // 0.0 ~ 1.0 사이의 난수 생성 후 현재 유효 확률과 비교하여 SSR 획득 여부 결정
        if (isSSR)  // SSR 획득 시
        {
            totalSSRs++;
            pullHistory.Add(true);  // SSR 획득 기록
            ssrPityList.Add(pityThisPull);  // SSR 획득 시 피티 카운트 기록
            currentPityCount = 0;  // 현재 피티 카운트 리셋
            currentEffectiveRate = baseRate;  // 확률 리셋
        }
        else
        {
            pullHistory.Add(false); // 일반 아이템 획득 기록
            currentPityCount++; // 현재 피티 카운트 증가
        }


        // 최대 기록 수 유지
        if (pullHistory.Count > MAX_HISTORY)
        {
            pullHistory.RemoveAt(0);
        }
        UpdateUI();
        return isSSR;
    }

    private void UpdateUI()
    {
        if (statusText == null) return;

        statusText.text = $"[뽑기 통계]\n" +
            $"총 뽑기: {totalPulls}\n" +
            $"획득 SSR: <color=yellow>{totalSSRs}</color>\n" +
            $"현재 천장: {currentPityCount}/{hardPity}\n" +
            $"유효 확률: <color=cyan>{currentEffectiveRate * 100f:F2}%</color>";

        if (pullHistoryText == null) return;

        string historyMsg = "[최근 뽑기 기록]\n";
        for (int i = 0; i < pullHistory.Count; i++)
        {
            historyMsg += pullHistory[i] ? "<color=yellow>★</color> " : "○ ";
            if ((i + 1) % 10 == 0) historyMsg += "\n";
        }

        pullHistoryText.text = historyMsg;

        if (expectedValueText == null) return;

        string expectedMsg = "[SSR 통계]\n";
        if (ssrPityList.Count > 0)
        {
            float averagePity = 0f;
            for (int i = 0; i < ssrPityList.Count; i++)
            {
                averagePity += ssrPityList[i];
            }
            averagePity /= ssrPityList.Count;
            expectedMsg += $"평균 천장: {averagePity:F1}\n";
            expectedMsg += $"최근 3회: ";
            for (int i = 0; i < Mathf.Min(3, ssrPityList.Count); i++)
            {
                expectedMsg += $"{ssrPityList[i]} ";
            }
        }
        else
        {
            expectedMsg += "아직 SSR 없음\n";
        }

        expectedMsg += $"\n[예상값]\n";
        float avgRate = (baseRate + currentEffectiveRate) / 2f;
        if (avgRate > 0)
        {
            float expectedPulls = (hardPity - currentPityCount) / avgRate;
            expectedMsg += $"예상 뽑기: {expectedPulls:F0}회";
        }

        expectedValueText.text = expectedMsg;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enabled) return;

        Vector3 basePos = transform.position;
        float graphScale = 3f;
        float graphHeight = 2f;

        // 그래프 배경
        Gizmos.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawCube(basePos + Vector3.right * graphScale * 0.5f + Vector3.up * graphHeight * 0.5f,
                       new Vector3(graphScale, graphHeight, 0.1f));

        // 기본 확률 수평선 (피티 로직 구현 전까지 baseRate만 표시)
        Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.8f);
        float baseY = baseRate * graphHeight;
        Gizmos.DrawLine(basePos + Vector3.up * baseY,
                       basePos + Vector3.right * graphScale + Vector3.up * baseY);

        // 현재 pity 위치 표시
        Gizmos.color = Color.red;
        float currentX = (float)currentPityCount / hardPity * graphScale;
        Gizmos.DrawLine(basePos + Vector3.right * currentX,
                       basePos + Vector3.right * currentX + Vector3.up * graphHeight);
    }
#endif
}
