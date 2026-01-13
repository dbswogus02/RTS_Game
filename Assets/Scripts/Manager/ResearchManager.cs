using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    public static ResearchManager Instance;

    [Header("Current Research Levels")]
    public float buildingHealthBonus = 0f;
    public float meleeAttackBonus = 0f;
    public float rangeAttackBonus = 0f;
    public float cavalryAttackBonus = 0f;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 공통 자원 체크 및 소모 함수
    private bool TrySpendGold(int cost)
    {
        if (ResourceManager.Instance != null && ResourceManager.Instance.gold >= cost)
        {
            ResourceManager.Instance.gold -= cost;
            return true;
        }

        Debug.Log("<color=yellow>자원이 부족합니다!</color>");
        return false;
    }

    // 1. 건물 체력 업그레이드
    public void UpgradeBuildingHealth(int cost)
    {
        if (TrySpendGold(cost))
        {
            float bonusAmount = 100f;
            buildingHealthBonus += bonusAmount;

            // 현재 맵에 존재하는 모든 아군 건물의 체력 즉시 증가
            Health[] allHealths = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health h in allHealths)
            {
                if (h.isBuilding && h.isPlayerSide)
                {
                    h.maxHealth += bonusAmount;
                    h.currentHealth += bonusAmount;
                }
            }
            Debug.Log($"<color=cyan>[연구 완료]</color> 건물 체력 연구 성공! (현재 보너스: +{buildingHealthBonus})");
        }
    }

    // 2. 근접 유닛 공격력 업그레이드
    public void UpgradeMeleeAttack(int cost)
    {
        if (TrySpendGold(cost))
        {
            meleeAttackBonus += 5f;
            Debug.Log($"<color=red>[연구 완료]</color> 근접 유닛 공격력 업그레이드 완료! (현재 보너스: +{meleeAttackBonus})");
        }
    }

    // 3. 원거리 유닛 공격력 업그레이드
    public void UpgradeRangeAttack(int cost)
    {
        if (TrySpendGold(cost))
        {
            rangeAttackBonus += 3f;
            Debug.Log($"<color=green>[연구 완료]</color> 원거리 유닛 공격력 업그레이드 완료! (현재 보너스: +{rangeAttackBonus})");
        }
    }

    // 4. 기마 유닛 공격력 업그레이드
    public void UpgradeCavalryAttack(int cost)
    {
        if (TrySpendGold(cost))
        {
            cavalryAttackBonus += 4f;
            Debug.Log($"<color=blue>[연구 완료]</color> 기마 유닛 공격력 업그레이드 완료! (현재 보너스: +{cavalryAttackBonus})");
        }
    }
}