using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Text resourceText;

    [Header("Unit Info UI")]
    public GameObject unitInfoPanel;
    public Text unitNameText;
    public Text unitHealthText;

    [Header("Skill Info UI")]
    public GameObject skillPanel;      // 스킬창 (부모)
    public Text skillNameText;         // "Berserker Dash"
    public Slider skillCooldownSlider; // 쿨타임 바

    private PhysicsUnitMover targetUnit;

    void Awake() { Instance = this; }

    void Update()
    {
        if (ResourceManager.Instance != null && resourceText != null)
        {
            resourceText.text = string.Format("Gold: {0} | Units: {1}/{2}",
                ResourceManager.Instance.gold,
                ResourceManager.Instance.currentUnitCount,
                ResourceManager.Instance.maxUnitCount);
        }

        UpdateUnitInfo();
    }

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
            float maxHP = targetUnit.unitData.maxHealth;
            unitHealthText.text = string.Format("HP: {0} / {1}", (int)targetUnit.currentHealth, (int)maxHP);

            // 패시브 스킬 UI 처리
            if (targetUnit.isMeleeUnit)
            {
                if (skillPanel != null) skillPanel.SetActive(true);
                if (skillNameText != null) skillNameText.text = "Berserker Dash";

                if (skillCooldownSlider != null)
                {
                    // 쿨타임이 찰수록 슬라이더가 오른쪽으로 차오름
                    float ratio = 1f - (targetUnit.DashCooldownTimer / targetUnit.dashCooldown);
                    skillCooldownSlider.value = Mathf.Clamp01(ratio);
                }
            }
            else
            {
                if (skillPanel != null) skillPanel.SetActive(false);
            }
        }
    }
} // 클래스 끝