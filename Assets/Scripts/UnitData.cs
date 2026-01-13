using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject unitPrefab; // 생성될 유닛 프리팹
    public float moveSpeed;       // 이동 속도 (기마 유닛은 높게 설정)
    public float attackRange;     // 사거리 (원거리는 길게 설정)
    public int unitCost; // 소환 비용 (예: 근접 50, 원거리 80)
    public int unitCapacity = 1; // 이 유닛이 차지하는 인구수 (기본 1)
    public float maxHealth = 50f;
    public float attackDamage = 10f;
    public float buildingDamageMultiplier = 1.5f; // 건물 공격 시 추가 데미지 배율
}