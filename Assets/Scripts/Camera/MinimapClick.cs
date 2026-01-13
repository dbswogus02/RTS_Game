using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapClick : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public Transform mainCameraTransform; // RTSCameraController가 있는 카메라
    public RectTransform minimapRect;
    public Vector2 mapSize = new Vector2(100, 100); // 실제 게임 맵 크기

    public void OnPointerDown(PointerEventData eventData) => MoveCamera(eventData);
    public void OnDrag(PointerEventData eventData) => MoveCamera(eventData);

    void MoveCamera(PointerEventData eventData)
    {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        // UI 좌표를 0~1 비율로 변환
        float px = Mathf.Clamp01((localCursor.x - minimapRect.rect.x) / minimapRect.rect.width);
        float py = Mathf.Clamp01((localCursor.y - minimapRect.rect.y) / minimapRect.rect.height);

        // 실제 월드 좌표로 변환
        Vector3 worldPos = new Vector3((px - 0.5f) * mapSize.x, mainCameraTransform.position.y, (py - 0.5f) * mapSize.y);
        mainCameraTransform.position = worldPos;
    }
}