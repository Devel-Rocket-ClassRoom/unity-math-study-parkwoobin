using UnityEngine;

public class MoveControll : MonoBehaviour
{
    private const float ARRIVAL_THRESHOLD = 0.05f;  // 목적지에 도착했다고 간주하는 거리 임계값

    private Vector3 destination;    // 이동할 목적지
    private bool isMoving;  // 이동 중인지 여부
    private Vector3 startPosition;  // 이동 시작 위치

    [SerializeField] private float moveSpeed = 300f;

    private void Awake()
    {
        startPosition = transform.position;
    }

    public void MoveTo(Vector3 position)    // 외부에서 호출하여 이동을 시작하는 메서드
    {
        destination = position;
        isMoving = true;
    }

    public void Stop()  // 이동을 중지하는 메서드
    {
        isMoving = false;
    }

    public Vector3 GetStartPosition() => startPosition; // 시작 위치를 반환하는 메서드

    private void Update()
    {
        if (!isMoving) return;  // 이동 중이 아니면 업데이트하지 않음


        // 현재 위치에서 목적지까지 일정 속도로 이동
        transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * moveSpeed);

        if (Vector3.Distance(transform.position, destination) < ARRIVAL_THRESHOLD)  // 목적지에 거의 도착했으면
        {
            transform.position = destination;
            isMoving = false;
        }
    }
}