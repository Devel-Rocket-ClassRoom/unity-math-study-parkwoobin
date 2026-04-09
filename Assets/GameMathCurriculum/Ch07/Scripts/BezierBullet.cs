using UnityEngine;
/// <summary>
/// 베지어 곡선을 따라 이동하는 bullet
/// </summary>
public class BezierBullet : MonoBehaviour
{
    private Vector3 p0, p1, p2, p3;
    private float duration;
    private float t;

    // 베지어 4점 설정 및 이동 시간 초기화
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
        // 0 -> 1로 t 증가시키면서 베지어 곡선을 따라 이동
        t += Time.deltaTime / duration;

        if (t > 1f) // t가 1을 초과하면 이동 완료로 간주하고 오브젝트 제거
        {
            Destroy(gameObject);
            return;
        }

        transform.position = EvaluateCubicBezier(t);    // 위치 업데이트
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