using UnityEngine;
using System.Collections.Generic; // List 자료구조를 사용하기 위해 필요합니다.

public class RTSUnitManager : MonoBehaviour
{
    // 1. 싱글톤 패턴: 다른 스크립트에서 RTSUnitManager.Instance로 즉시 접근할 수 있게 합니다.
    public static RTSUnitManager Instance;

    // 현재 선택된 유닛들을 담아두는 리스트입니다.
    public List<GameObject> selectedUnits = new List<GameObject>();

    void Awake()
    {
        // 게임이 시작될 때 자기 자신을 Instance에 할당합니다.
        Instance = this;
    }

    // 유닛을 선택 리스트에 추가하고 하이라이트 효과를 주는 함수
    public void SelectUnit(GameObject unit)
    {
        // 리스트에 이미 들어있는 유닛이 아닐 때만 실행합니다.
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);

            // 유닛의 Renderer 컴포넌트를 가져와서 시각적 피드백을 줍니다.
            Renderer rend = unit.GetComponent<Renderer>();
            if (rend != null)
            {
                // 강조 색상을 흰색(White)으로 변경하여 선택되었음을 표시합니다.
                rend.material.color = Color.white;
            }
        }
    }

    // 선택된 모든 유닛의 선택을 해제하고 원래 상태로 되돌리는 함수
    public void DeselectAll()
    {
        // 리스트에 담긴 모든 유닛을 하나씩 꺼내어 처리합니다.
        foreach (var unit in selectedUnits)
        {
            if (unit != null)
            {
                // 유닛에 붙어 있는 'PhysicsUnitMover' 스크립트를 참조합니다.
                // 이 스크립트 안에 원래 색상으로 되돌리는 ResetColor() 함수가 있어야 합니다.
                PhysicsUnitMover mover = unit.GetComponent<PhysicsUnitMover>();
                if (mover != null)
                {
                    mover.ResetColor(); // 원래 색상으로 복구
                }
            }
        }

        // 처리가 끝난 후 리스트를 비웁니다.
        selectedUnits.Clear();
    }
}