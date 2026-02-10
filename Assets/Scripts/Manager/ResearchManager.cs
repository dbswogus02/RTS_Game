using UnityEngine;

public class ResearchManager : MonoBehaviour
{
    // 1. 싱글톤 패턴: 어디서나 ResearchManager.Instance로 접근하여 연구 수치를 확인할 수 있게 함
    public static ResearchManager Instance;

    [Header("Current Research Levels")]
    // 현재 각 분야별로 적용된 보너스 수치들 (다른 유닛/건물 스크립트에서 이 값을 참조함)
    public float buildingHealthBonus = 0f;    // 건물 체력 보너스
    public float meleeAttackBonus = 0f;       // 근접 공격력 보너스
    public float rangeAttackBonus = 0f;       // 원거리 공격력 보너스
    public float cavalryAttackBonus = 0f;     // 기마 공격력 보너스

    void Awake()
    {
        // 인스턴스가 비어있다면 현재 스크립트를 할당 (싱글톤 초기화)
        if (Instance == null) Instance = this;
    }

    // --- 공통 내부 함수 ---

    // 자원이 충분한지 확인하고 소모하는 함수 (코드 중복을 줄이기 위해 사용)
    private bool TrySpendGold(int cost)
    {
        // ResourceManager의 인스턴스가 존재하고, 현재 골드가 비용(cost)보다 많으면 실행
        if (ResourceManager.Instance != null && ResourceManager.Instance.gold >= cost)
        {
            ResourceManager.Instance.gold -= cost; // 골드 차감
            return true; // 성공 반환
        }

        // 자원이 부족하면 경고 로그를 띄우고 실패 반환
        Debug.Log("<color=yellow>자원이 부족합니다!</color>");
        return false;
    }

    // --- 연구 실행 함수들 (UI 버튼 등에 연결) ---

    // 1. 건물 체력 업그레이드
    public void UpgradeBuildingHealth(int cost)
    {
        if (TrySpendGold(cost)) // 골드 소모에 성공하면
        {
            float bonusAmount = 100f;
            buildingHealthBonus += bonusAmount; // 보너스 수치 누적

            // [핵심] 현재 맵에 존재하는 모든 Health 컴포넌트를 찾아 실시간으로 반영
            Health[] allHealths = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
            foreach (Health h in allHealths)
            {
                // '건물'이면서 '플레이어 소속'인 경우에만 적용
                if (h.isBuilding && h.isPlayerSide)
                {
                    h.maxHealth += bonusAmount;    // 최대 체력 증가
                    h.currentHealth += bonusAmount; // 현재 체력도 보너스만큼 회복
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
            meleeAttackBonus += 5f; // 보너스 수치 5 증가
            Debug.Log($"<color=red>[연구 완료]</color> 근접 유닛 공격력 업그레이드 완료! (현재 보너스: +{meleeAttackBonus})");
        }
    }

    // 3. 원거리 유닛 공격력 업그레이드
    public void UpgradeRangeAttack(int cost)
    {
        if (TrySpendGold(cost))
        {
            rangeAttackBonus += 3f; // 보너스 수치 3 증가
            Debug.Log($"<color=green>[연구 완료]</color> 원거리 유닛 공격력 업그레이드 완료! (현재 보너스: +{rangeAttackBonus})");
        }
    }

    // 4. 기마 유닛 공격력 업그레이드
    public void UpgradeCavalryAttack(int cost)
    {
        if (TrySpendGold(cost))
        {
            cavalryAttackBonus += 4f; // 보너스 수치 4 증가
            Debug.Log($"<color=blue>[연구 완료]</color> 기마 유닛 공격력 업그레이드 완료! (현재 보너스: +{cavalryAttackBonus})");
        }
    }
}