using UnityEngine;
using UnityEngine.UI;

public class MapCoordinateManager : MonoBehaviour
{
    public Text mousePosText;
    public Text gridZoneText;
    public LayerMask groundLayer;

    [Header("Mouse Follow UI")]
    public RectTransform followUIPanel; // 마우스를 따라다닐 UI 부모 패널
    public Vector2 uiOffset = new Vector2(20, 20); // 마우스와 UI 사이의 간격

    private float mapSize = 250f;
    private int gridX = 26;
    private int gridZ = 10;

    public GameObject selectionHighlight;

    void Update()
    {
        UpdateMouseCoordinates();
        UpdateFollowUIPosition(); // 마우스 추적 UI 위치 업데이트 추가
    }

    void UpdateFollowUIPosition()
    {
        if (followUIPanel != null)
        {
            // 마우스의 현재 스크린 좌표에 오프셋을 더해 UI 위치 설정
            followUIPanel.position = Input.mousePosition + (Vector3)uiOffset;
        }
    }

    void UpdateMouseCoordinates()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 point = hit.point;

            // 구역 인덱스 계산 및 하이라이트 로직 (기존 유지)
            float cellWidth = mapSize / gridX;
            float cellHeight = mapSize / gridZ;
            float relativeX = point.x + (mapSize / 2f);
            float relativeZ = point.z + (mapSize / 2f);

            int indexX = Mathf.FloorToInt(relativeX / cellWidth);
            int indexZ = Mathf.FloorToInt(relativeZ / cellHeight);

            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(true);
                float centerX = -(mapSize / 2f) + (indexX * cellWidth) + (cellWidth / 2f);
                float centerZ = -(mapSize / 2f) + (indexZ * cellHeight) + (cellHeight / 2f);
                selectionHighlight.transform.position = new Vector3(centerX, 0.15f, centerZ);
                selectionHighlight.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);
            }

            // 구역(Grid) 계산 결과 업데이트
            string zone = CalculateGridZone(point);
            if (gridZoneText != null)
                gridZoneText.text = zone; // 이제 마우스를 따라다니는 텍스트에 표시됨

            if (mousePosText != null)
                mousePosText.text = string.Format("X: {0:F1}, Z: {1:F1}", point.x, point.z);

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"<color=green>[Map System]</color> 클릭한 구역: {zone}");
            }
        }
        else
        {
            if (selectionHighlight != null) selectionHighlight.SetActive(false);
            // 마우스가 바닥을 벗어나면 UI 숨기기 (선택 사항)
            // if (followUIPanel != null) followUIPanel.gameObject.SetActive(false);
        }
    }

    string CalculateGridZone(Vector3 hitPoint)
    {
        float relativeX = hitPoint.x + (mapSize / 2f);
        float relativeZ = hitPoint.z + (mapSize / 2f);

        int indexX = Mathf.Clamp(Mathf.FloorToInt((relativeX / mapSize) * gridX), 0, gridX - 1);
        int indexZ = Mathf.Clamp(Mathf.FloorToInt((relativeZ / mapSize) * gridZ), 0, gridZ - 1);

        char colAlpha = (char)(65 + indexX);
        int rowNum = indexZ + 1;

        return $"{colAlpha}-{rowNum}";
    }
}