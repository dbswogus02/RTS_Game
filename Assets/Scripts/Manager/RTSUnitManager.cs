using UnityEngine;
using System.Collections.Generic;

public class RTSUnitManager : MonoBehaviour
{
    public static RTSUnitManager Instance;
    public List<GameObject> selectedUnits = new List<GameObject>();

    void Awake() { Instance = this; }

    public void SelectUnit(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);

            // 강조 색상을 노란색에서 흰색(white)으로 변경
            Renderer rend = unit.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.white;
            }
        }
    }

    public void DeselectAll()
    {
        foreach (var unit in selectedUnits)
        {
            if (unit != null)
            {
                // 유닛의 이동 스크립트(또는 위에서 만든 스크립트)를 가져와 색상 복구 함수 호출
                PhysicsUnitMover mover = unit.GetComponent<PhysicsUnitMover>();
                if (mover != null)
                {
                    mover.ResetColor();
                }
            }
        }
        selectedUnits.Clear();
    }
}