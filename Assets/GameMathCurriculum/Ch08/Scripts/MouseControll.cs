using UnityEngine;

public class MouseControll : MonoBehaviour
{
    private Camera main;
    private bool isDragging;
    private GameObject selected;
    private Vector3 originPos;
    private Vector3 pointOffset;

    public Terrain terrain;
    [SerializeField] private GameObject[] endPoints;
    [SerializeField] private float detectionRange = 115f;   // 도착 지점으로 간주하는 최대 거리

    private void Start()
    {
        main = Camera.main;
    }

    private void Update()
    {
        Ray ray = main.ScreenPointToRay(Input.mousePosition);   // 마우스 위치에서 레이 생성

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // 선택
        if (hit.collider.CompareTag("Object") && Input.GetMouseButtonDown(0))
        {
            selected = hit.collider.gameObject;
            isDragging = true;

            MoveControll ctrl = selected.GetComponent<MoveControll>();  // MoveControll 컴포넌트가 있는지 확인
            originPos = ctrl != null ? ctrl.GetStartPosition() : selected.transform.position;   // 이동 시작 위치 저장

            Vector3 size = selected.GetComponent<Renderer>().bounds.size;
            // 클릭한 지점과 오브젝트 중심 사이의 오프셋 계산
            pointOffset = selected.transform.position - new Vector3(
                hit.point.x,
                terrain.SampleHeight(hit.point) + size.y / 2f,
                hit.point.z
            );
        }

        // 드래그
        if (isDragging && selected != null)
        {
            Vector3 size = selected.GetComponent<Renderer>().bounds.size;   // 오브젝트의 크기 가져오기
            // 마우스 위치에 오프셋을 더해서 오브젝트의 중심이 마우스 위치에 오도록 설정
            selected.transform.position = new Vector3(
                hit.point.x,
                terrain.SampleHeight(hit.point) + size.y / 2f,
                hit.point.z
            ) + pointOffset;
        }

        // 해제
        if (isDragging && selected != null && Input.GetMouseButtonUp(0))
        {
            Vector3 size = selected.GetComponent<Renderer>().bounds.size;   // 오브젝트의 크기 가져오기
            Vector3 bottomPos = selected.transform.position - new Vector3(0f, size.y / 2f, 0f);   // 오브젝트의 바닥 위치 계산

            Transform targetEnd = null; // 도착 지점으로 간주할 타겟 초기화
            foreach (GameObject end in endPoints)
            {
                if (end == null) continue;
                // 타겟과 오브젝트의 바닥 위치 사이의 거리가 detectionRange보다 작으면 도착 지점으로 간주
                if (Vector3.Distance(bottomPos, end.transform.position) < detectionRange)
                {
                    targetEnd = end.transform;
                    break;
                }
            }

            MoveControll ctrl = selected.GetComponent<MoveControll>();  // MoveControll 컴포넌트가 있는지 확인

            if (targetEnd != null)
            {
                Vector3 endSize = targetEnd.GetComponent<Renderer>().bounds.size;   // endPoint의 크기 가져오기
                Vector3 targetSize = selected.GetComponent<Renderer>().bounds.size;   // 선택된 오브젝트의 크기 가져오기

                // 타겟의 위치에 오브젝트를 배치 (타겟의 중심이 아닌 바닥이 타겟의 중심에 오도록 조정)
                selected.transform.position = new Vector3(
                    targetEnd.position.x,
                    targetEnd.position.y + endSize.y / 2f + targetSize.y / 2f,  // 타겟의 높이 절반 + 오브젝트의 높이 절반 만큼 올려서 배치
                    targetEnd.position.z
                );
                ctrl.Stop();
            }
            else
            {
                ctrl.MoveTo(originPos);
            }

            selected = null;
            isDragging = false;
        }
    }
}