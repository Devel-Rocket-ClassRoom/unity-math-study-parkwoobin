// =============================================================================
// Assignment_UIPopup.cs
// -----------------------------------------------------------------------------
// EaseIn/EaseOut을 적용한 UI 팝업 등장/퇴장 애니메이션 시스템
// =============================================================================

using TMPro;
using UnityEngine;

public class Assignment_UIPopup : MonoBehaviour
{
    private enum PopupState
    {
        Hidden,
        Showing,
        Visible,
        Hiding
    }

    [Header("=== 애니메이션 설정 ===")]
    [SerializeField] private float animationDuration = 0.5f;
    [Tooltip("팝업 RectTransform (Canvas 하위 Panel)")]
    [SerializeField] private RectTransform popupPanel;
    [Tooltip("투명도를 조절할 CanvasGroup 컴포넌트")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("=== UI 출력 ===")]
    [Tooltip("현재 상태/시간을 표시할 TextMeshPro UI")]
    [SerializeField] private TextMeshProUGUI stateText;

    private PopupState currentState = PopupState.Hidden;
    private float elapsedTime = 0f;

    private void Start()
    {
        // 팝업 패널을 활성화하되 Scale=0, Alpha=0으로 숨긴 상태로 시작
        if (popupPanel != null)
        {
            popupPanel.gameObject.SetActive(true);
            popupPanel.localScale = Vector3.zero;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == PopupState.Hidden || currentState == PopupState.Hiding)
            {
                ShowPopup();
            }
            else if (currentState == PopupState.Visible || currentState == PopupState.Showing)
            {
                HidePopup();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = PopupState.Hidden;
            elapsedTime = 0f;
            popupPanel.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;
        }

        UpdateAnimation();

        UpdateUI();
    }

    private void ShowPopup()
    {
        if (currentState == PopupState.Showing || currentState == PopupState.Visible)
        {
            return;
        }

        currentState = PopupState.Showing;
        elapsedTime = 0f;
    }

    private void HidePopup()
    {
        if (currentState == PopupState.Hiding || currentState == PopupState.Hidden)
        {
            return;
        }

        currentState = PopupState.Hiding;
        elapsedTime = 0f;
    }

    private void UpdateAnimation()
    {
        if (currentState == PopupState.Hidden || currentState == PopupState.Visible)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / animationDuration);

        if (currentState == PopupState.Showing)
        {
            // 팝업이 등장하는 동안은 EaseOut을 적용하여 부드럽게 등장
            float easeT = EaseOut(t);   // EaseOut 적용
            popupPanel.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, easeT); // Scale은 EaseOut으로 부드럽게 증가
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, easeT);  // Alpha도 EaseOut으로 증가

            if (t >= 1f)
            {
                currentState = PopupState.Visible;
                popupPanel.localScale = Vector3.one;
                canvasGroup.alpha = 1f;
            }
        }
        else if (currentState == PopupState.Hiding)
        {
            // 팝업이 사라지는 동안은 EaseIn을 적용하여 부드럽게 퇴장
            float easeT = EaseIn(t);    // t를 EaseIn 함수로 변환하여 가속 효과 적용
            popupPanel.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, easeT); // Scale은 EaseIn으로 부드럽게 감소
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, easeT);  // Alpha도 EaseIn으로 감소

            if (t >= 1f)
            {
                currentState = PopupState.Hidden;
                popupPanel.localScale = Vector3.zero;
                canvasGroup.alpha = 0f;
            }
        }

    }

    private float EaseOut(float t)
    {
        return 1f - Mathf.Pow(1f - t, 2f);
    }

    private float EaseIn(float t)
    {
        return t * t;
    }

    private void UpdateUI()
    {
        if (stateText == null) return;

        string stateLabel = currentState switch
        {
            PopupState.Hidden => "<color=red>Hidden</color>",
            PopupState.Showing => "<color=yellow>Showing</color>",
            PopupState.Visible => "<color=green>Visible</color>",
            PopupState.Hiding => "<color=orange>Hiding</color>",
            _ => "Unknown"
        };

        float progress = animationDuration > 0f ? (elapsedTime / animationDuration) : 0f;
        progress = Mathf.Clamp01(progress);

        stateText.text = $"<b>[UI Popup Animation]</b>\n" +
                         $"상태: {stateLabel}\n" +
                         $"진행도: {progress:P0}\n" +
                         $"Scale: {popupPanel.localScale.x:F2}\n" +
                         $"Alpha: {canvasGroup.alpha:F2}\n" +
                         $"\n<color=cyan><b>[ Space ] 토글  [ Esc ] 강제종료</b></color>";
    }
}
