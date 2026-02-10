using UnityEngine;

// [CreateAssetMenu] 어노테이션은 유니티 에디터의 프로젝트 창에서 
// 마우스 우클릭 -> Create -> RTS -> Unit Data 메뉴를 통해 이 파일을 만들 수 있게 해줍니다.
[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("기본 정보")]
    public string unitName;         // 유닛의 이름 (예: 보병, 궁수, 기사)
    public GameObject unitPrefab;   // 실제 게임 월드에 생성될 유닛의 3D 모델(프리팹)

    [Header("이동 및 전투")]
    public float moveSpeed;         // 이동 속도 (기마 유닛은 이 값을 높게 설정하여 차별화)
    public float attackRange;       // 사거리 (근접 유닛은 짧게, 원거리 유닛은 길게 설정)
    public float attackDamage = 10f; // 유닛의 기본 공격력

    [Header("경제 및 인구")]
    public int unitCost;            // 유닛 소환에 필요한 골드 비용
    public int unitCapacity = 1;    // 이 유닛이 차지하는 인구수 (강력한 유닛일수록 높게 설정)

    [Header("체력 및 상성")]
    public float maxHealth = 50f;   // 유닛의 최대 체력
    public float buildingDamageMultiplier = 1.5f; // 건물 공격 시 적용되는 데미지 배율 (공성 유닛 특화용)
}