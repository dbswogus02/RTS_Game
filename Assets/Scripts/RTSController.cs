using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RTSController : MonoBehaviour
{
    [Header("UI & Visuals")]
    public RectTransform selectionBox;
    public RectTransform attackMoveMarker;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask clickableLayer; // Building과 Unit 레이어가 모두 포함되어야 합니다.

    private Vector2 startPos;
    private bool isDragging = false;
    private bool isAttackMoveMode = false;

    void Start()
    {
        if (selectionBox != null) selectionBox.gameObject.SetActive(false);
        if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleHotkeys();

        if (isAttackMoveMode && attackMoveMarker != null)
        {
            UpdateMarkerPosition();
        }

        // UI 위를 클릭할 때는 게임 로직 중단 (단, 드래그 중에는 예외)
        if (EventSystem.current.IsPointerOverGameObject() && !isDragging) return;

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
            if (isAttackMoveMode) CancelAttackMode();
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            UpdateSelectionBox(Input.mousePosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (selectionBox != null) selectionBox.gameObject.SetActive(false);
            PerformSelection(); // 드디어 이 함수가 실행됩니다!
        }

        if (Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }
    }

    void PerformSelection()
    {
        Vector2 endPos = Input.mousePosition;

        // 1. 단일 클릭 판정 (이동 거리가 10픽셀 미만일 때)
        if (Vector2.Distance(startPos, endPos) < 10f)
        {
            Ray ray = Camera.main.ScreenPointToRay(endPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, clickableLayer))
            {
                Debug.Log($"[클릭 감지] 이름: {hit.collider.name}, 태그: {hit.collider.tag}");

                // 새로운 대상을 선택하기 위해 기존 선택 해제
                RTSUnitManager.Instance.DeselectAll();

                if (hit.collider.CompareTag("Building"))
                {
                    // 건물 클릭 시 생산 UI 표시
                    RTSSelectionManager.Instance.HandleBuildingClick();
                }
                else if (hit.collider.CompareTag("Unit"))
                {
                    // 유닛 클릭 시 정보창 표시
                    RTSUnitManager.Instance.SelectUnit(hit.collider.gameObject);
                    PhysicsUnitMover mover = hit.collider.GetComponentInParent<PhysicsUnitMover>();
                    if (mover != null && UIManager.Instance != null)
                        UIManager.Instance.DisplayUnitInfo(mover);
                }
            }
            else
            {
                // 빈 땅 클릭 시 모든 선택 해제 및 UI 닫기
                RTSUnitManager.Instance.DeselectAll();
                RTSSelectionManager.Instance.CloseAllUI();
            }
        }
        // 2. 드래그 선택 판정
        else
        {
            RTSUnitManager.Instance.DeselectAll();
            if (RTSSelectionManager.Instance != null) RTSSelectionManager.Instance.CloseAllUI();

            // 화면 좌표계의 사각형 생성
            Rect rect = new Rect(
                Mathf.Min(startPos.x, endPos.x),
                Mathf.Min(startPos.y, endPos.y),
                Mathf.Abs(startPos.x - endPos.x),
                Mathf.Abs(startPos.y - endPos.y)
            );

            // "Unit" 태그를 가진 모든 오브젝트 검사
            foreach (var unit in GameObject.FindGameObjectsWithTag("Unit"))
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(unit.transform.position);
                if (rect.Contains(screenPos))
                {
                    RTSUnitManager.Instance.SelectUnit(unit);
                }
            }

            // 드래그로 한 명만 잡혔다면 정보 UI 띄워주기
            if (RTSUnitManager.Instance.selectedUnits.Count == 1)
            {
                PhysicsUnitMover mover = RTSUnitManager.Instance.selectedUnits[0].GetComponentInParent<PhysicsUnitMover>();
                if (mover != null && UIManager.Instance != null)
                    UIManager.Instance.DisplayUnitInfo(mover);
            }
        }
    }

    // --- 이하 보조 함수들 (변화 없음) ---
    void UpdateSelectionBox(Vector2 curPos)
    {
        if (selectionBox == null) return;
        if (Vector2.Distance(startPos, curPos) < 2f) return;
        if (!selectionBox.gameObject.activeSelf) selectionBox.gameObject.SetActive(true);

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(curPos.x - startPos.x), Mathf.Abs(curPos.y - startPos.y));
        RectTransform pRect = selectionBox.parent as RectTransform;
        if (pRect != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(pRect, (startPos + curPos) / 2f, null, out localPoint);
            selectionBox.anchoredPosition = localPoint;
        }
    }

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

    void HandleHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.A) && RTSUnitManager.Instance.selectedUnits.Count > 0)
        {
            isAttackMoveMode = true;
            if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            CancelAttackMode();
            foreach (var unit in RTSUnitManager.Instance.selectedUnits)
            {
                var mover = unit.GetComponent<PhysicsUnitMover>();
                if (mover != null) { mover.currentState = UnitState.HoldGround; mover.SetTarget(unit.transform.position); }
            }
        }
    }

    // RTSController.cs의 HandleRightClick 함수 수정
    void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            // 선택된 유닛 리스트 가져오기
            var units = RTSUnitManager.Instance.selectedUnits;
            for (int i = 0; i < units.Count; i++)
            {
                var mover = units[i].GetComponent<PhysicsUnitMover>();
                if (mover == null) continue;

                // [겹침 방지 핵심] 유닛 번호(i)에 따라 나선형으로 목적지 분산
                float angle = i * 0.5f;
                float radius = 1.0f * Mathf.Sqrt(i); // 유닛이 많아질수록 원이 커짐
                Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Vector3 individualTarget = hit.point + offset;

                if (isAttackMoveMode)
                    mover.SetAttackMove(individualTarget);
                else
                    mover.SetTarget(individualTarget);
            }
            CancelAttackMode();
        }
    }

    void CancelAttackMode()
    {
        isAttackMoveMode = false;
        if (attackMoveMarker != null) attackMoveMarker.gameObject.SetActive(false);
    }
}