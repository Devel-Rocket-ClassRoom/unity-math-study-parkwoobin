using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    public Transform mover;
    public float duration = 5f;
    private SplineContainer splineContainer;
    private float t;

    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
    }
    void Update()
    {
        t += Time.deltaTime / duration;
        t = Mathf.Repeat(t, 1f);

        if (!splineContainer.Evaluate(splineContainer.Spline, t, out float3 position, out float3 tangent, out float3 up))
        {
            return;
        }

        mover.position = position;
        if (math.length(tangent) > 0.001f)
        {
            mover.rotation = Quaternion.LookRotation(tangent, up);
        }
    }
}
