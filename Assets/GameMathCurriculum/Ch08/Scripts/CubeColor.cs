using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class CubeColor : MonoBehaviour
{
    // Canvas에 있는 6개의 Image에 6개의 색이 있는 큐브를 적용하는 스크립트
    [SerializeField] public UnityEngine.UI.Image[] ImageColor; // 6개의 색이 적용된 Image 배열
    [SerializeField] private Renderer[] cubeRenderers; // 6개의 큐브의 Renderer 배열

    public Renderer[] CubeRenderers => cubeRenderers;

    private void Awake()
    {
        for (int i = 0; i < ImageColor.Length; i++)
        {
            Color cubeColor = cubeRenderers[i].material.color;

            ImageColor[i].color = cubeColor;

            Debug.Log($"[CubeColor] Cube {i}의 색상 {cubeColor}이 Image {i}에 적용되었습니다.");
        }
    }
}
