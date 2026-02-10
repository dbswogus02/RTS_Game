using UnityEngine;

// 연구의 종류를 구분하기 위한 열거형(Enum) 정의
// 건물 체력, 근접 공격력, 원거리 공격력, 기마 공격력 중 하나를 선택할 수 있습니다.
public enum ResearchType { BuildingHealth, MeleeAttack, RangeAttack, CavalryAttack }

// 프로젝트 창에서 [우클릭 -> Create -> RTS -> Research Data] 메뉴를 통해 연구 에셋을 생성할 수 있게 합니다.
[CreateAssetMenu(fileName = "NewResearch", menuName = "RTS/Research Data")]
public class ResearchData : ScriptableObject
{
    [Header("연구 기본 정보")]
    public string researchName;    // 연구의 이름 (예: "강철 검 연마", "강화 성벽")
    public ResearchType type;      // 연구의 종류 (위에서 정의한 enum 값 중 하나)

    [Header("비용 및 효과")]
    public int goldCost;           // 연구를 시작하는 데 필요한 골드 비용
    public float upgradeValue;     // 연구 완료 시 실제 게임 수치에 더해질 값 (예: 공격력 +5)

    [Header("상태")]
    public bool isCompleted = false; // 연구가 이미 완료되었는지 여부 (중복 연구 방지용)
}