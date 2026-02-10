using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트를 처리하기 위해 필요합니다.

// IPointerDownHandler(클릭), IDragHandler(드래그) 인터페이스를 상속받아 이벤트를 감지합니다.
public class MinimapClick : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public Transform mainCameraTransform; // 이동시킬 메인 카메라의 Transform
    public RectTransform minimapRect;      // 미니맵 UI의 RectTransform (크기 및 위치 정보)
    public Vector2 mapSize = new Vector2(100, 100); // 실제 게임 월드(맵)의 가로, 세로 크기

    // 미니맵을 클릭했을 때 호출되는 함수
    public void OnPointerDown(PointerEventData eventData) => MoveCamera(eventData);

    // 미니맵을 드래그하고 있을 때 호출되는 함수
    public void OnDrag(PointerEventData eventData) => MoveCamera(eventData);

    // 실제 카메라 위치를 계산하고 옮기는 핵심 로직
    void MoveCamera(PointerEventData eventData)
    {
        Vector2 localCursor;

        // 1. 스크린 마우스 좌표를 미니맵 UI 내의 로컬 좌표로 변환합니다.
        // RectTransformUtility.ScreenPointToLocalPointInRectangle은 UI 영역 안에서 마우스가 어디에 있는지 계산해줍니다.
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        // 2. UI 좌표를 0~1 사이의 비율(Normalized)로 변환합니다.
        // (현재 마우스 위치 - 미니맵 왼쪽 끝 위치) / 미니맵 전체 너비
        float px = Mathf.Clamp01((localCursor.x - minimapRect.rect.x) / minimapRect.rect.width);
        float py = Mathf.Clamp01((localCursor.y - minimapRect.rect.y) / minimapRect.rect.height);

        // 3. 0~1 비율을 실제 게임 월드 좌표로 변환합니다.
        // (px - 0.5f)를 하는 이유는 월드 좌표의 중심이 (0,0,0)인 경우를 가정했기 때문입니다.
        // px가 0이면 -0.5, 0.5면 0, 1이면 0.5가 되어 mapSize를 곱했을 때 중앙 정렬이 됩니다.
        Vector3 worldPos = new Vector3(
            (px - 0.5f) * mapSize.x,           // 월드 X 좌표
            mainCameraTransform.position.y,     // 높이(Y)는 현재 카메라 높이 유지
            (py - 0.5f) * mapSize.y            // 월드 Z 좌표
        );

        // 4. 계산된 월드 좌표로 카메라를 즉시 이동시킵니다.
        mainCameraTransform.position = worldPos;
    }
}