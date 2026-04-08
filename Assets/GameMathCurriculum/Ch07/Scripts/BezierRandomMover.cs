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
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float shootInterval = 0.5f;
    [Range(1f, 20f)][SerializeField] private float duration = 6f;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private float shootTimer;
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
            shootTimer += Time.deltaTime;
            if (shootTimer > shootInterval)
            {
                ShootBullet();
                shootTimer = 0f;
            }
        }
    }

    private void ShootBullet()
    {
        // Bullet 생성
        GameObject bulletObj = Instantiate(bulletPrefab, startPoint, Quaternion.identity);
        bulletObj.name = "BezierBullet";

        // BezierBullet 컴포넌트 처리
        BezierBullet bezierBullet = bulletObj.GetComponent<BezierBullet>();
        if (bezierBullet == null)
            bezierBullet = bulletObj.AddComponent<BezierBullet>();

        // 랜덤 제어점 생성
        Vector3 control1 = GenerateRandomControlPoint();
        Vector3 control2 = GenerateRandomControlPoint();

        // 랜덤 이동 시간
        float moveDuration = mathRandom.NextFloat(duration * 0.3f, duration);

        // Bullet 초기화
        bezierBullet.Initialize(startPoint, control1, control2, endPoint, moveDuration);
    }

    private Vector3 GenerateRandomControlPoint()    // startPoint과 endPoint의 중간 지점을 기준으로 랜덤한 오프셋을 더하여 제어점 생성
    {
        Vector3 midPoint = (startPoint + endPoint) * 0.5f;
        Vector3 offset = new Vector3(
            mathRandom.NextFloat(-3f, 3f),
            mathRandom.NextFloat(-3f, 3f),
            mathRandom.NextFloat(-3f, 3f)
        );
        return midPoint + offset;
    }
    private void OnDrawGizmosSelected() // Scene 뷰에서 Knot 위치를 시각적으로 표시
    {
        Spline spline = splineContainer.Splines[0];

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


/// <summary>
/// 베지어 곡선을 따라 이동하는 bullet
/// </summary>
public class BezierBullet : MonoBehaviour
{
    private Vector3 p0, p1, p2, p3;
    private float duration;
    private float t;

    public void Initialize(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float moveDuration)
    {
        p0 = start;
        p1 = control1;
        p2 = control2;
        p3 = end;
        duration = moveDuration;
        t = 0f;
    }

    private void Update()
    {
        t += Time.deltaTime / duration;

        if (t > 1f)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = EvaluateCubicBezier(t);
    }

    private Vector3 EvaluateCubicBezier(float t)
    {
        Vector3 a = Vector3.Lerp(p0, p1, t);
        Vector3 b = Vector3.Lerp(p1, p2, t);
        Vector3 c = Vector3.Lerp(p2, p3, t);

        Vector3 d = Vector3.Lerp(a, b, t);
        Vector3 e = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(d, e, t);
    }
}