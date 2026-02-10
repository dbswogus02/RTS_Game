using UnityEngine;
using System.Collections; // Coroutine(코루틴) 사용을 위해 필요합니다.

public class ResourceManager : MonoBehaviour
{
    // 1. 싱글톤 패턴: 어디서나 ResourceManager.Instance로 자원 정보에 접근 가능
    public static ResourceManager Instance;

    [Header("Gold Settings")]
    public int gold = 200;           // 현재 보유 중인 골드 (시작 자원)
    public int goldPerSecond = 2;    // 1초마다 자동으로 들어오는 골드 양

    [Header("Unit Capacity")]
    public int currentUnitCount = 0; // 현재 생성된 유닛의 총 인구수
    public int maxUnitCount = 300;   // 맵에서 허용되는 최대 인구수 제한

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 게임 시작 시 자원을 지속적으로 추가하는 코루틴 실행
        StartCoroutine(AddGoldOverTime());
    }

    // 2. 자원 자동 생성 로직 (코루틴)
    IEnumerator AddGoldOverTime()
    {
        while (true) // 게임이 실행되는 동안 무한 반복
        {
            yield return new WaitForSeconds(1f); // 1초 대기
            gold += goldPerSecond;               // 설정된 양만큼 골드 증가
        }
    }

    // 3. 자원 소모 처리 함수
    // cost: 소모할 골드, capacity: 해당 유닛이 차지하는 인구수
    public void UseResources(int cost, int capacity = 1)
    {
        gold -= cost;                        // 골드 차감
        currentUnitCount += capacity;        // 사용 중인 인구수 증가
    }

    // 4. 생산 가능 여부 확인 (조건 검사)
    // 유닛을 뽑기 전에 돈이 충분한지, 인구수 제한에 걸리지 않는지 체크합니다.
    public bool CanProduceUnit(int cost, int capacity = 1)
    {
        // 조건 1: 현재 인구수 + 새로 뽑을 유닛의 인구수가 최대치를 넘으면 생산 불가
        if (currentUnitCount + capacity > maxUnitCount) return false;

        // 조건 2: 보유한 골드가 유닛 가격보다 적으면 생산 불가
        if (gold < cost) return false;

        // 두 조건을 모두 통과하면 생산 가능(true)
        return true;
    }

    // 5. 유닛 사망 시 처리
    // 유닛이 파괴되었을 때 호출하여 인구수를 다시 확보합니다.
    public void OnUnitDestroyed(int capacity)
    {
        // Mathf.Max를 사용하여 인구수가 0 아래로 내려가는 버그를 방지합니다.
        currentUnitCount = Mathf.Max(0, currentUnitCount - capacity);
    }
}