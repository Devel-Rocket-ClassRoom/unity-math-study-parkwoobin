using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System;
using Random = Unity.Mathematics.Random;

/// <summary>
/// SPACE 키로 여러 개의 bullet을 랜덤한 베지어 곡선 경로로 발사하는 시스템
/// </summary>
public class BezierRandomMover : MonoBehaviour
{
    [Header("=== Spline 설정 ===")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("=== 발사 설정 ===")]
    [SerializeField] public BezierBullet bulletPrefab;
    [SerializeField] private float Interval = 0.5f;
    [Range(1f, 20f)][SerializeField] private float duration = 6f;



    private Vector3 startPoint;
    private Vector3 endPoint;
    private float timer;
    private Random mathRandom;

    private void Awake()
    {
        mathRandom = new Random((uint)DateTime.Now.Ticks);

        // Knot[0], Knot[1] 위치를 World space로 가져오기
        Spline spline = splineContainer.Splines[0];
        Transform splineTransform = splineContainer.transform;

        // Local → World space 변환
        startPoint = splineTransform.TransformPoint((Vector3)spline[0].Position);
        endPoint = splineTransform.TransformPoint((Vector3)spline[1].Position);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            timer += Time.deltaTime;   // SPACE 키가 눌려있는 동안 timer 증가
            if (timer > Interval)
            {
                // Bullet 생성
                // GameObject bulletObj = Instantiate(bulletPrefab, startPoint, Quaternion.identity);  // startPoint에서 bulletPrefab 인스턴스화해서 생성
                BezierBullet bullet = Instantiate(bulletPrefab);


                // BezierBullet 컴포넌트 처리
                // BezierBullet bezierBullet = bullet.GetComponent<BezierBullet>(); // 생성된 bulletObj에서 BezierBullet 컴포넌트 가져오기

                // null 체크 - 컴포넌트가 없으면 추가
                // if (bezierBullet == null)
                // {
                //     bezierBullet = bullet.AddComponent<BezierBullet>();
                // }

                // 랜덤 제어점 생성
                Vector3 control1 = GenerateRandomControlPoint();
                Vector3 control2 = GenerateRandomControlPoint();

                // 랜덤 이동 시간으로 총알마다 속도가 다름 (30%~100% 범위)
                float moveDuration = mathRandom.NextFloat(duration * 0.3f, duration);

                // Bullet 초기화
                bullet.Initialize(startPoint, control1, control2, endPoint, moveDuration);
                timer = 0f;
            }
        }
    }

    private Vector3 GenerateRandomControlPoint()    // startPoint과 endPoint의 중간 지점을 기준으로 랜덤한 오프셋을 더하여 제어점 생성
    {
        Vector3 midPoint = (startPoint + endPoint) * 0.5f;  // midPoint를 기준으로 -3 ~ +3 범위의 랜덤한 오프셋 생성
        Vector3 offset = new Vector3(
            mathRandom.NextFloat(-3f, 3f),
            mathRandom.NextFloat(-3f, 3f),
            mathRandom.NextFloat(-3f, 3f)
        );
        return midPoint + offset;
    }

    private void OnDrawGizmosSelected() // Scene 뷰에서 Knot 위치를 시각적으로 표시
    {
        Spline spline = splineContainer.Splines[0]; // Spline의 Transform 가져오기

        Transform splineTransform = splineContainer.transform;

        // Knot 위치 표시
        Vector3 startWorldPos = splineTransform.TransformPoint(spline[0].Position);
        Vector3 endWorldPos = splineTransform.TransformPoint(spline[1].Position);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startWorldPos, 0.3f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(endWorldPos, 0.3f);
    }
}
