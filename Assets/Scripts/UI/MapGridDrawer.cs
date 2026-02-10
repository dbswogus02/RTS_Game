using UnityEngine;

public class MapGridDrawer : MonoBehaviour
{
    public Material lineMaterial; // 선을 그릴 때 사용할 재질 (색상이나 투명도 조절용)
    private float mapSize = 250f;  // 전체 맵의 가로/세로 크기
    private int gridX = 30;        // 가로 방향 칸 수 (세로선 개수 결정)
    private int gridZ = 30;        // 세로 방향 칸 수 (가로선 개수 결정)

    void Start()
    {
        // 게임이 시작되자마자 그리드를 그립니다.
        DrawGrid();
    }

    void DrawGrid()
    {
        // 맵의 중심이 (0,0,0)일 때, 시작 지점은 -절반 위치입니다.
        float halfSize = mapSize / 2f;

        // 1. 가로선 그리기 (Z축 방향을 일정 간격으로 나누어 X축으로 평행한 선을 그림)
        for (int i = 0; i <= gridZ; i++)
        {
            // i번째 선의 Z 좌표 계산
            float z = -halfSize + (mapSize / gridZ) * i;
            // 왼쪽 끝에서 오른쪽 끝까지 선 생성 (Y축 0.1f는 바닥과 겹쳐서 깜빡이는 현상 방지)
            CreateLine(new Vector3(-halfSize, 0.1f, z), new Vector3(halfSize, 0.1f, z));
        }

        // 2. 세로선 그리기 (X축 방향을 일정 간격으로 나누어 Z축으로 평행한 선을 그림)
        for (int i = 0; i <= gridX; i++)
        {
            // i번째 선의 X 좌표 계산
            float x = -halfSize + (mapSize / gridX) * i;
            // 아래 끝에서 위 끝까지 선 생성
            CreateLine(new Vector3(x, 0.1f, -halfSize), new Vector3(x, 0.1f, halfSize));
        }
    }

    // 3. 실제 LineRenderer를 가진 게임 오브젝트를 생성하는 함수
    void CreateLine(Vector3 start, Vector3 end)
    {
        // 선을 담당할 새 게임 오브젝트 생성
        GameObject lineObj = new GameObject("GridLine");
        lineObj.transform.SetParent(this.transform); // 관리하기 편하게 부모 설정

        // 라인을 그리는 컴포넌트 추가 및 설정
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();

        // 재질 설정 (없으면 기본 스프라이트 재질 사용)
        lr.material = lineMaterial ? lineMaterial : new Material(Shader.Find("Sprites/Default"));

        lr.startWidth = 0.2f; // 선 시작 두께
        lr.endWidth = 0.2f;   // 선 끝 두께
        lr.positionCount = 2; // 점의 개수 (시작점, 끝점 총 2개)

        lr.SetPosition(0, start); // 시작 좌표 설정
        lr.SetPosition(1, end);   // 끝 좌표 설정

        // 선의 색상을 반투명한 흰색으로 설정
        lr.startColor = new Color(1, 1, 1, 0.3f);
        lr.endColor = new Color(1, 1, 1, 0.3f);

        lr.useWorldSpace = true; // 월드 좌표계를 기준으로 선을 그림
    }
}