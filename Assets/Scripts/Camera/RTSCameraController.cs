using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;   // 카메라의 이동 속도
    public float edgeSize = 10f;    // 마우스가 화면 끝의 몇 픽셀 안에 들어와야 이동할지 결정하는 감지 범위

    [Header("Zoom")]
    public float zoomSpeed = 500f;  // 줌인/아웃 속도
    public float minHeight = 5f;    // 카메라의 최소 높이 (가장 가까운 거리)
    public float maxHeight = 40f;   // 카메라의 최대 높이 (가장 먼 거리)

    [Header("Map Bounds")]
    public Vector2 minBounds;       // 맵의 왼쪽 아래 끝(X, Z) 제한 좌표
    public Vector2 maxBounds;       // 맵의 오른쪽 위 끝(X, Z) 제한 좌표

    void Update()
    {
        // 매 프레임마다 이동과 줌 로직을 실행합니다.
        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 moveDir = Vector3.zero; // 이번 프레임에 이동할 방향 초기화

        // 1. 마우스 화면 끝 감지 (Screen Edge Scrolling)
        // 화면 오른쪽 끝에 마우스가 있으면 오른쪽(X=1)으로 이동
        if (Input.mousePosition.x >= Screen.width - edgeSize) moveDir.x = 1;
        // 화면 왼쪽 끝에 마우스가 있으면 왼쪽(X=-1)으로 이동
        if (Input.mousePosition.x <= edgeSize) moveDir.x = -1;
        // 화면 위쪽 끝에 마우스가 있으면 위(Z=1)로 이동
        if (Input.mousePosition.y >= Screen.height - edgeSize) moveDir.z = 1;
        // 화면 아래쪽 끝에 마우스가 있으면 아래(Z=-1)로 이동
        if (Input.mousePosition.y <= edgeSize) moveDir.z = -1;

        // 2. 화살표 방향키 입력 추가
        float h = 0;
        float v = 0;

        // 화살표 키(Right, Left, Up, Down)를 눌렀는지 확인하여 이동 값을 할당
        if (Input.GetKey(KeyCode.RightArrow)) h = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) h = -1;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1;

        // 3. 입력값 통합
        // 마우스로 이동하고 있지 않을 때 화살표 입력이 있다면 그 값을 이동 방향에 적용합니다.
        if (h != 0) moveDir.x = h;
        if (v != 0) moveDir.z = v;

        // 4. 이동 처리
        // 방향을 정규화(normalized)하여 대각선 이동 시 빨라지는 것을 방지하고, 속도와 프레임 시간을 곱합니다.
        Vector3 targetMove = new Vector3(moveDir.x, 0, moveDir.z).normalized;
        Vector3 newPosition = transform.position + targetMove * moveSpeed * Time.deltaTime;

        // 5. 맵 경계 제한 (Clamping)
        // 카메라가 설정된 minBounds와 maxBounds 영역 밖으로 나가지 못하게 고정합니다.
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.z = Mathf.Clamp(newPosition.z, minBounds.y, maxBounds.y);

        // 최종 위치 적용
        transform.position = newPosition;
    }

    void Zoom()
    {
        // 마우스 휠 입력을 받습니다 (위로 올리면 +, 아래로 내리면 -)
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            // 현재 높이(Y)에서 휠 스크롤 값을 뺍니다 (휠을 올리면 Y값이 작아져서 지면에 가까워짐)
            float newY = transform.position.y - (scroll * zoomSpeed * Time.deltaTime);

            // 카메라 높이가 최소/최대 범위를 넘지 않도록 제한합니다.
            newY = Mathf.Clamp(newY, minHeight, maxHeight);

            // 새로운 높이 값만 적용하여 카메라 위치를 갱신합니다.
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}