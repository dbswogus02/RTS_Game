using UnityEngine;
using System.Collections;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int gold = 200;           // 시작 골드
    public int goldPerSecond = 2;    // 초당 획득 자원
    public int currentUnitCount = 0;
    public int maxUnitCount = 300;

    void Awake() { Instance = this; }

    void Start()
    {
        StartCoroutine(AddGoldOverTime());
    }

    IEnumerator AddGoldOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            gold += goldPerSecond;
        }
    }

    // 자원 소모
    public void UseResources(int cost, int capacity = 1)
    {
        gold -= cost;
        currentUnitCount += capacity;
    }

    // 생산 가능 체크 (비용과 인구수 둘 다 확인)
    // CanProduceUnit 이름도 UnitSpawner와 맞추기 위해 수정 (선택사항)
    public bool CanProduceUnit(int cost, int capacity = 1)
    {
        if (currentUnitCount + capacity > maxUnitCount) return false;
        if (gold < cost) return false;
        return true;
    }

    // 유닛 사망 시 호출
    public void OnUnitDestroyed(int capacity)
    {
        currentUnitCount = Mathf.Max(0, currentUnitCount - capacity);
    }
}