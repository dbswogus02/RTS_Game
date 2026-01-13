using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float edgeSize = 10f;

    [Header("Zoom")]
    public float zoomSpeed = 500f;
    public float minHeight = 5f;
    public float maxHeight = 40f;

    [Header("Map Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    void Update()
    {
        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 moveDir = Vector3.zero;

        // 1. 마우스 화면 끝 감지 (기존 로직)
        if (Input.mousePosition.x >= Screen.width - edgeSize) moveDir.x = 1;
        if (Input.mousePosition.x <= edgeSize) moveDir.x = -1;
        if (Input.mousePosition.y >= Screen.height - edgeSize) moveDir.z = 1;
        if (Input.mousePosition.y <= edgeSize) moveDir.z = -1;

        // 2. 화살표 방향키 입력 추가
        // 기존의 GetAxisRaw 대신 특정 키(Keycode)를 직접 검사합니다.
        float h = 0;
        float v = 0;

        // 화살표 키만 감지
        if (Input.GetKey(KeyCode.RightArrow)) h = 1;
        if (Input.GetKey(KeyCode.LeftArrow)) h = -1;
        if (Input.GetKey(KeyCode.UpArrow)) v = 1;
        if (Input.GetKey(KeyCode.DownArrow)) v = -1;

        // 이후 moveDir.x = h; moveDir.z = v; 로직은 동일

        // 마우스 이동값이 없을 때만 방향키 값을 적용하거나, 두 값을 합칩니다.
        if (h != 0) moveDir.x = h;
        if (v != 0) moveDir.z = v;

        Vector3 targetMove = new Vector3(moveDir.x, 0, moveDir.z).normalized;
        Vector3 newPosition = transform.position + targetMove * moveSpeed * Time.deltaTime;

        // 위치 제한 적용
        newPosition.x = Mathf.Clamp(newPosition.x, minBounds.x, maxBounds.x);
        newPosition.z = Mathf.Clamp(newPosition.z, minBounds.y, maxBounds.y);

        transform.position = newPosition;
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float newY = transform.position.y - (scroll * zoomSpeed * Time.deltaTime);
            newY = Mathf.Clamp(newY, minHeight, maxHeight);

            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}