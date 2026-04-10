using UnityEngine;

public class DragAndDropSample : MonoBehaviour
{
    public Camera camera; // 드래그할 오브젝트를 볼 카메라
    public LayerMask ground;
    public LayerMask target;
    public LayerMask dropZone;
    private bool isDragging = false;
    private DragObject draggingObject;

    private void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, target))
            {
                Debug.Log("Drag Start");
                isDragging = true;
                draggingObject = hitInfo.collider.GetComponent<DragObject>();
                draggingObject.DragStart();
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, dropZone))
                {
                    draggingObject.DragEnd();
                }
                else
                {
                    draggingObject.Return();
                }

                isDragging = false;
                draggingObject = null;
            }
        }

        else if (isDragging)
        {
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, ground))
            {
                draggingObject.transform.position = hitInfo.point;
            }
        }
    }
}

