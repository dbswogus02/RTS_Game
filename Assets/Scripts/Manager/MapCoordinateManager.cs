using UnityEngine;
using UnityEngine.UI;

public class MapCoordinateManager : MonoBehaviour
{
    public Text mousePosText;      // 마우스의 월드 좌표(X, Z)를 표시할 텍스트
    public Text gridZoneText;      // 계산된 그리드 구역(예: A-1)을 표시할 텍스트
    public LayerMask groundLayer;  // 마우스 레이캐스트가 부딪힐 바닥 레이어

    [Header("Mouse Follow UI")]
    public RectTransform followUIPanel;      // 마우스를 따라다닐 UI 부모 패널
    public Vector2 uiOffset = new Vector2(20, 20); // 마우스와 UI 사이의 거리 조절(오프셋)

    private float mapSize = 250f; // 전체 맵의 크기 (정사각형 기준)
    private int gridX = 26;       // 가로 칸 수 (A~Z까지 26칸)
    private int gridZ = 10;       // 세로 칸 수 (1~10까지 10칸)

    public GameObject selectionHighlight; // 현재 마우스가 위치한 칸을 보여주는 하이라이트 오브젝트

    void Update()
    {
        UpdateMouseCoordinates(); // 마우스의 월드 위치 및 구역 계산
        UpdateFollowUIPosition(); // 마우스 포인터를 따라다니는 UI 위치 갱신
    }

    // 1. UI가 마우스 위치를 실시간으로 추적하게 하는 함수
    void UpdateFollowUIPosition()
    {
        if (followUIPanel != null)
        {
            // Input.mousePosition은 스크린 좌표계이므로, 여기에 오프셋을 더해 UI의 위치로 설정합니다.
            followUIPanel.position = Input.mousePosition + (Vector3)uiOffset;
        }
    }

    // 2. 마우스 월드 좌표 계산 및 그리드 로직
    void UpdateMouseCoordinates()
    {
        // 카메라에서 마우스 위치를 향해 레이(Ray)를 쏩니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 레이가 바닥 레이어(groundLayer)에 부딪혔을 때만 실행
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Vector3 point = hit.point; // 레이가 부딪힌 지점의 좌표

            // --- 그리드 하이라이트 위치 계산 ---
            float cellWidth = mapSize / gridX;  // 한 칸의 너비
            float cellHeight = mapSize / gridZ; // 한 칸의 높이

            // 맵의 중심이 (0,0)일 경우를 대비해 좌표를 0부터 시작하도록 보정
            float relativeX = point.x + (mapSize / 2f);
            float relativeZ = point.z + (mapSize / 2f);

            // 현재 마우스가 몇 번째 칸(Index)에 있는지 계산
            int indexX = Mathf.FloorToInt(relativeX / cellWidth);
            int indexZ = Mathf.FloorToInt(relativeZ / cellHeight);

            // 하이라이트 오브젝트가 있다면 위치와 크기 조절
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(true);
                // 해당 칸의 정중앙 좌표 계산
                float centerX = -(mapSize / 2f) + (indexX * cellWidth) + (cellWidth / 2f);
                float centerZ = -(mapSize / 2f) + (indexZ * cellHeight) + (cellHeight / 2f);

                selectionHighlight.transform.position = new Vector3(centerX, 0.15f, centerZ);
                // 하이라이트 크기를 그리드 한 칸 크기에 맞춤
                selectionHighlight.transform.localScale = new Vector3(cellWidth, cellHeight, 1f);
            }

            // --- UI 텍스트 업데이트 ---
            string zone = CalculateGridZone(point); // "A-1" 형태의 문자열 생성
            if (gridZoneText != null) gridZoneText.text = zone;

            if (mousePosText != null)
                mousePosText.text = string.Format("X: {0:F1}, Z: {1:F1}", point.x, point.z);

            // 클릭 시 현재 구역 로그 출력
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log($"<color=green>[Map System]</color> 클릭한 구역: {zone}");
            }
        }
        else
        {
            // 마우스가 바닥을 벗어나면 하이라이트를 끕니다.
            if (selectionHighlight != null) selectionHighlight.SetActive(false);
        }
    }

    // 3. 좌표를 "A-1"과 같은 구역 이름으로 변환하는 함수
    string CalculateGridZone(Vector3 hitPoint)
    {
        float relativeX = hitPoint.x + (mapSize / 2f);
        float relativeZ = hitPoint.z + (mapSize / 2f);

        // 0 ~ (gridX-1) 사이의 인덱스로 변환
        int indexX = Mathf.Clamp(Mathf.FloorToInt((relativeX / mapSize) * gridX), 0, gridX - 1);
        int indexZ = Mathf.Clamp(Mathf.FloorToInt((relativeZ / mapSize) * gridZ), 0, gridZ - 1);

        // 아스키 코드를 이용해 indexX를 알파벳으로 변환 (65는 'A')
        char colAlpha = (char)(65 + indexX);
        int rowNum = indexZ + 1; // 0부터 시작하므로 1을 더해줌

        return $"{colAlpha}-{rowNum}";
    }
}