using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Top Resources")]
    public Text resourceText;

    [Header("Unit Info UI")]
    public GameObject unitInfoPanel; // 하단 정보창 전체 (UnitInfoPanel 오브젝트)
    public Text unitNameText;
    public Text unitHealthText;

    [Header("Skill Info UI")]
    public GameObject skillPanel;      // [중요] SkillText와 SkillSlider만 포함된 부모 오브젝트여야 함
    public Text skillNameText;
    public Slider skillCooldownSlider;

    private PhysicsUnitMover targetUnit;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        // 1. 자원 정보 업데이트
        UpdateResourceUI();

        // 2. 선택된 유닛 실시간 정보 업데이트
        UpdateUnitInfo();
    }

    private void UpdateResourceUI()
    {
        if (ResourceManager.Instance != null && resourceText != null)
        {
            resourceText.text = string.Format("Gold: {0} | Units: {1}/{2}",
                ResourceManager.Instance.gold,
                ResourceManager.Instance.currentUnitCount,
                ResourceManager.Instance.maxUnitCount);
        }
    }

    // 유닛을 클릭했을 때 호출되는 함수
    public void DisplayUnitInfo(PhysicsUnitMover unit)
    {
        targetUnit = unit;

        if (unit != null && unit.unitData != null)
        {
            if (unitInfoPanel != null) unitInfoPanel.SetActive(true);
            if (unitNameText != null) unitNameText.text = unit.unitData.unitName;

            // 추가: Melee 유닛인 경우 스킬 텍스트를 즉시 설정
            if (unit.isMeleeUnit)
            {
                if (skillPanel != null) skillPanel.SetActive(true);
                if (skillNameText != null) skillNameText.text = "Berserker Dash";
            }
        }
        else
        {
            if (unitInfoPanel != null) unitInfoPanel.SetActive(false);
        }
    }

    // 실시간 데이터(체력, 쿨타임) 갱신 로직
    void UpdateUnitInfo()
    {
        // 타겟 유닛이 없으면 실행하지 않음
        if (targetUnit == null || targetUnit.unitData == null) return;

        // 1. 공통 정보 업데이트 (이름, 체력)
        // 이 로직은 if(isMeleeUnit) 밖에 있으므로 모든 유닛에게 적용됩니다.
        if (unitHealthText != null)
        {
            float maxHP = targetUnit.unitData.maxHealth;
            unitHealthText.text = string.Format("HP: {0} / {1}", (int)targetUnit.currentHealth, (int)maxHP);
        }

        // 2. 유닛 타입별 스킬 UI 처리
        if (targetUnit.isMeleeUnit)
        {
            // 근접 유닛: 스킬 패널 표시
            if (skillPanel != null && !skillPanel.activeSelf)
                skillPanel.SetActive(true);

            if (skillNameText != null)
                skillNameText.text = "Berserker Dash";

            if (skillCooldownSlider != null)
            {
                float ratio = 1f - (targetUnit.DashCooldownTimer / targetUnit.dashCooldown);
                skillCooldownSlider.value = Mathf.Clamp01(ratio);
            }
        }
        else
        {
            // 캐벌리, 레인저 등: 스킬 패널만 숨김 (이름/체력은 유지됨)
            if (skillPanel != null && skillPanel.activeSelf)
                skillPanel.SetActive(false);
        }
    }
}