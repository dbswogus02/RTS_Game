using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // 어디서든 접근 가능하게 싱글톤 추가

    public Text resourceText;

    [Header("Unit Info UI")]
    public GameObject unitInfoPanel; // 정보창 패널
    public Text unitNameText;        // 유닛 이름 텍스트
    public Text unitHealthText;      // 유닛 체력 텍스트

    private PhysicsUnitMover targetUnit; // 현재 정보를 보여줄 대상 유닛

    void Awake() { Instance = this; }

    void Update()
    {
        // 1. 자원 UI 업데이트
        if (ResourceManager.Instance != null && resourceText != null)
        {
            resourceText.text = string.Format("Gold: {0} | Units: {1}/{2}",
                ResourceManager.Instance.gold,
                ResourceManager.Instance.currentUnitCount,
                ResourceManager.Instance.maxUnitCount);
        }

        // 2. 선택된 유닛 정보 실시간 업데이트
        UpdateUnitInfo();
    }

    // 정보를 표시할 유닛을 설정하는 함수
    public void DisplayUnitInfo(PhysicsUnitMover unit)
    {
        targetUnit = unit;
        if (unit != null)
        {
            unitInfoPanel.SetActive(true);
            unitNameText.text = unit.unitData.unitName;
        }
        else
        {
            unitInfoPanel.SetActive(false);
        }
    }

    void UpdateUnitInfo()
    {
        if (targetUnit != null && targetUnit.unitData != null)
        {
            // SO(UnitData)에 저장된 maxHealth를 사용
            float maxHP = targetUnit.unitData.maxHealth;
            unitHealthText.text = string.Format("HP: {0} / {1}", (int)targetUnit.currentHealth, (int)maxHP);
        }
    }
}