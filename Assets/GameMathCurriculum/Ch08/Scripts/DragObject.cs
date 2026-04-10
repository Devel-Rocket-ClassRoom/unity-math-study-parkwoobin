using UnityEngine;

public class DragObject : MonoBehaviour
{
    public bool isReturning; // 드래그가 끝난 후 원래 위치로 돌아가는지 여부
    public float timeReturn = 2f;
    public Vector3 originalPosition; // 드래그 시작 시의 원래 위치
    public Vector3 startPosition;
    private Terrain terrain;
    private float timer;

    private void Start()
    {
        terrain = Terrain.activeTerrain; // 현재 활성화된 Terrain을 가져옴
    }
    private void ResetDrag()
    {
        isReturning = false;
        timer = 0f;
        originalPosition = Vector3.zero;
        startPosition = Vector3.zero;
    }
    public void DragStart()
    {
        isReturning = false;
        timer = 0f;
        startPosition = Vector3.zero;
        originalPosition = transform.position;
    }
    public void Return()
    {
        timer = 0f;
        isReturning = true;
        startPosition = transform.position; // 드래그가 끝난 시점의 위치를 저장
    }

    public void DragEnd()
    {
        ResetDrag();
    }

    private void Update()
    {
        if (isReturning)
        {
            timer += Time.deltaTime / timeReturn; // 보간에 사용할 시간 비율 계산
            Vector3 newPos = Vector3.Lerp(startPosition, originalPosition, timer); // 선형 보간으로 위치 계산
            newPos.y = terrain.SampleHeight(newPos); // Terrain의 높이에 맞게 y값 조정
            transform.position = newPos; // 오브젝트 위치 업데이트

            if (timer > 1f)
            {
                isReturning = false; // 반환 완료
                transform.position = originalPosition; // 위치를 원래 위치로 정확히 설정
                timer = 0f; // 타이머 초기화
            }
        }
    }
}
