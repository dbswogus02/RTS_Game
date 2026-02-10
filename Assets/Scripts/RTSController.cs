using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RTSController : MonoBehaviour
{
    [Header("UI & Visuals")]
    public RectTransform selectionBox;     // 드래그 시 화면에 그려질 사각형 이미지 (UI)
    public RectTransform attackMoveMarker; // 어택 땅 모드일 때 커서를 따라다니는 시각적 표시기

    [Header("Layers")]
    public LayerMask groundLayer;     // 이동 지점을 잡기 위한 바닥 레이어
    public LayerMask clickableLayer;  // 건물(Building)과 유닛(Unit)이 포함된 선택 가능 레이어

    private Vector2 startPos;          // 드래그를 시작한 마우스의 화면 좌표
    private bool isDragging = false;   // 현재 마우스를 누르고 드래그 중인지 확인
    private bool isAttackMoveMode = false; // 'A' 단축키를 눌러 공격 이동 대기 상태인지 확인

    void Start()
    {
        // 시작 시 드래그 박스와 어택 마커를 비활성화합니다.
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
        if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleHotkeys(); // 단축키 입력(A: 공격, H: 정지 등) 감지

        // 어택 땅 모드라면 마커가 현재 마우스 위치를 계속 따라다니게 합니다.
        if (isAttackMoveMode && attackMoveMarker != null)
        {
            UpdateMarkerPosition();
        }

        // 마우스가 UI(버튼 등) 위에 있다면 게임 월드 클릭 로직을 실행하지 않습니다. (단, 드래그 중에는 허용)
        if (EventSystem.current.IsPointerOverGameObject() && !isDragging) return;

        // --- 마우스 왼쪽 클릭 (선택 로직) ---
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition; // 클릭 시작 지점 저장
            isDragging = true;
            if (isAttackMoveMode) CancelAttackMode(); // 클릭 시 공격 모드 해제
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            // 마우스를 누른 채 움직이면 드래그 박스 UI를 갱신합니다.
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (selectionBox != null) selectionBox.gameObject.SetActive(false); // 드래그 박스 숨기기
            PerformSelection(); // 드래그 영역 또는 클릭 지점에 있는 대상을 실제로 선택
        }

        // --- 마우스 오른쪽 클릭 (명령 로직) ---
        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    // 유닛/건물을 선택하는 핵심 함수
    void PerformSelection()
    {
        Vector2 endPos = Input.mousePosition;

        // 1. 단순 클릭 판정: 드래그 거리가 10픽셀 미만이면 '클릭'으로 간주
        if (Vector2.Distance(startPos, endPos) < 10f)
        {
            Ray ray = Camera.main.ScreenPointToRay(endPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
            {
                Debug.Log($"[클릭 감지] 이름: {hit.collider.name}, 태그: {hit.collider.tag}");

                // 새로운 대상을 잡기 위해 이전 선택 모두 해제
                RTSUnitManager.Instance.DeselectAll();

                if (hit.collider.CompareTag("Building"))
                {
                    // 건물을 클릭한 경우: 생산 UI 등을 띄움
                    RTSSelectionManager.Instance.HandleBuildingClick();
                }
                else if (hit.collider.CompareTag("Unit"))
                {
                    // 유닛을 클릭한 경우: 유닛 선택 및 정보창 업데이트
                    RTSUnitManager.Instance.SelectUnit(hit.collider.gameObject);
                    PhysicsUnitMover mover = hit.collider.GetComponentInParent<PhysicsUnitMover>();
                    if (mover != null && UIManager.Instance != null)
                        UIManager.Instance.DisplayUnitInfo(mover);
                }
            }
            else
            {
                // 빈 땅을 클릭하면 모든 선택을 해제하고 UI를 닫습니다.
                RTSUnitManager.Instance.DeselectAll();
                RTSSelectionManager.Instance.CloseAllUI();
            }
        }
        // 2. 드래그 판정: 영역 안에 들어온 모든 유닛을 선택
        else
        {
            RTSUnitManager.Instance.DeselectAll();
            if (RTSSelectionManager.Instance != null) RTSSelectionManager.Instance.CloseAllUI();

            // 마우스 좌표를 이용해 2D Rect(사각형 영역) 생성
            Rect rect = new Rect(
                Mathf.Min(startPos.x, endPos.x),
                Mathf.Min(startPos.y, endPos.y),
                Mathf.Abs(startPos.x - endPos.x),
                Mathf.Abs(startPos.y - endPos.y)
            );

            // "Unit" 태그를 가진 모든 오브젝트를 찾아 영역 안에 있는지 검사
            foreach (var unit in GameObject.FindGameObjectsWithTag("Unit"))
            {
                // 월드 좌표를 화면 좌표로 변환하여 사각형 안에 있는지 확인
                Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                if (rect.Contains(screenPos))
                {
                    RTSUnitManager.Instance.SelectUnit(unit);
                }
            }

            // 드래그로 단 한 명의 유닛만 선택되었다면 상세 정보를 UI에 표시
            if (RTSUnitManager.Instance.selectedUnits.Count == 1)
            {
                PhysicsUnitMover mover = RTSUnitManager.Instance.selectedUnits[0].GetComponentInParent<PhysicsUnitMover>();
                if (mover != null && UIManager.Instance != null)
                    UIManager.Instance.DisplayUnitInfo(mover);
            }
        }
    }

    // 마우스 드래그 시 UI 사각형의 크기와 위치를 갱신하는 함수
    void UpdateSelectionBox(Vector2 curPos)
    {
        if (selectionBox == null) return;
        if (Vector2.Distance(startPos, curPos) < 2f) return; // 미세한 움직임은 무시
        if (!selectionBox.gameObject.activeSelf) selectionBox.gameObject.SetActive(true);

        // 사각형의 크기 설정
        selectionBox.sizeDelta = new Vector2(Mathf.Abs(curPos.x - startPos.x), Mathf.Abs(curPos.y - startPos.y));

        // 사각형의 중심 위치 계산 및 캔버스 좌표계로 변환
        RectTransform pRect = selectionBox.parent as RectTransform;
        if (pRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pRect, (startPos + curPos) / 2f, null, out localPoint);
            selectionBox.anchoredPosition = localPoint;
        }
    }

    // 어택 마커(UI)를 마우스 위치로 부드럽게 이동시키는 함수
    void UpdateMarkerPosition()
    {
        RectTransform pRect = attackMoveMarker.parent as RectTransform;
        if (pRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pRect, Input.mousePosition, null, out localPoint);
            attackMoveMarker.anchoredPosition = localPoint;
        }
    }

    // 키보드 단축키 처리 함수
    void HandleHotkeys()
    {
        // A키: 공격 이동(Attack Move) 모드 진입
        if (Input.GetKeyDown(KeyCode.A) && RTSUnitManager.Instance.selectedUnits.Count > 0)
        {
            isAttackMoveMode = true;
            if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(true);
        }
        // H키: 현재 위치 사수(Hold Ground) 명령
        if (Input.GetKeyDown(KeyCode.H))
        {
            CancelAttackMode();
            foreach (var unit in RTSUnitManager.Instance.selectedUnits)
            {
                var mover = unit.GetComponent<PhysicsUnitMover>();
                if (mover != null)
                {
                    mover.currentState = UnitState.HoldGround;
                    mover.SetTarget(unit.transform.position);
                }
            }
        }
    }

    // 마우스 오른쪽 클릭 처리 (유닛 이동 명령)
    void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            var units = RTSUnitManager.Instance.selectedUnits;
            for (int i = 0; i < units.Count; i++)
            {
                var mover = units[i].GetComponent<PhysicsUnitMover>();
                if (mover == null) continue;

                // [중요: 나선형 분산 로직] 
                // 모든 유닛이 한 지점으로 모이면 서로 겹쳐서 버벅거리므로, 
                // 유닛 번호(i)에 따라 조금씩 떨어진 목적지를 할당합니다.
                float angle = i * 0.5f;
                float radius = 1.0f * Mathf.Sqrt(i); // 유닛이 많아질수록 원의 반지름이 커짐
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Vector3 individualTarget = hit.point + offset;

                // 모드에 따라 공격 이동 혹은 일반 이동 명령 전달
                if (isAttackMoveMode)
                    mover.SetAttackMove(individualTarget);
                else
                    mover.SetTarget(individualTarget);
            }
            CancelAttackMode(); // 명령 후 모드 해제
        }
    }

    // 공격 이동 모드 취소 함수
    void CancelAttackMode()
    {
        isAttackMoveMode = false;
        if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(false);
    }
}