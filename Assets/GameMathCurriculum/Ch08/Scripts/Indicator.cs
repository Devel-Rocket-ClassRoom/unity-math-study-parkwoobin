using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Indicator : MonoBehaviour
{
    // CubeColor 스크립트를 참조하여 화면 밖의 Cube를 감지하는 스크립트
    [SerializeField] private CubeColor cubeColor; // CubeColor 스크립트 참조

    private RectTransform canvasRectTransform;
    private Camera mainCamera;
    private Canvas canvas;

    // 2. 씬에 복수의 타겟 오브젝트를 배치하고 Inspector에서 연결한다
    // 3. 매 프레임 각 타겟의 월드 좌표를 `WorldToScreenPoint`로 스크린 좌표로 변환한다
    // 4. 타겟이 화면 밖에 있으면 화면 가장자리에 인디케이터(UI Image)를 표시한다
    // 5. 타겟이 카메라 뒤에 있는 경우도 처리한다
    // 6. 인디케이터가 타겟 방향을 가리키도록 회전시킨다
    // 7. 타겟이 화면 안에 있으면 인디케이터를 숨긴다


    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponentInParent<Canvas>();
        canvasRectTransform = canvas.GetComponent<RectTransform>();
    }

    private void Update()
    {
        // 큐브의 위치를 화면좌표로 변환

        for (int i = 0; i < cubeColor.CubeRenderers.Length; i++)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(cubeColor.CubeRenderers[i].transform.position);
            // 큐브가 화면 안에 표시되고 있는지 확인
            if (screenPos.x > 0 && screenPos.x < Screen.width && screenPos.y > 0 && screenPos.y < Screen.height)
            {
                // 화면 안에 있으면 인디케이터 숨김
                GetComponent<CubeColor>().ImageColor[i].enabled = false;
            }
            else
            {
                UnityEngine.UI.Image indicatorImage = GetComponent<CubeColor>().ImageColor[i];
                indicatorImage.enabled = true;

                if (screenPos.z < 0)
                {
                    // 카메라 뒤의 오브젝트는 화면 중심 기준 반대쪽으로 표시
                    screenPos.x = Screen.width - screenPos.x;
                    screenPos.y = Screen.height - screenPos.y;
                }

                // 화면 가장자리에 인디케이터 위치 설정
                // 큐브가 오른쪽에 있으면 x값은 Screen.width로 고정하고 y값만 screenPos.y로 설정
                if (screenPos.x > Screen.width)
                {
                    screenPos.x = Screen.width;
                }
                else if (screenPos.x < 0)
                {
                    screenPos.x = 0;
                }

                if (screenPos.y < 0)
                {
                    screenPos.y = 0;
                }
                else if (screenPos.y > Screen.height)
                {
                    screenPos.y = Screen.height;
                }



                Debug.Log($"{indicatorImage.name}  x: {screenPos.x}, y: {screenPos.y}, z: {screenPos.z}");

                indicatorImage.rectTransform.position = new Vector3(screenPos.x, screenPos.y, 0);
            }
        }
    }
}