using UnityEngine;
using UnityEngine.Events; // 파괴 시 실행될 이벤트(UnityEvent)를 위해 필요

public class Health : MonoBehaviour
{
    public float maxHealth = 500f;   // 최대 체력
    public float currentHealth;      // 현재 남은 체력
    public bool isBuilding = false;  // 유닛인지 건물인지 구분 (파괴 로그 출력용)

    [Header("Team Settings")]
    public bool isPlayerSide = true; // 아군(Player)이면 true, 적군(Enemy)이면 false

    // 파괴되었을 때 실행될 함수들을 에디터에서 연결할 수 있는 이벤트
    // 예: 폭발 이펙트 재생, 자원 보상 지급, 유닛 목록에서 삭제 등
    public UnityEvent OnDestroyed;

    void Start()
    {
        // 게임 시작 시 현재 체력을 최대 체력으로 초기화합니다.
        currentHealth = maxHealth;
    }

    // 외부(공격자)에서 데미지를 줄 때 호출하는 함수
    public void TakeDamage(float amount)
    {
        // 이미 죽은 상태라면 중복 처리를 방지하기 위해 리턴
        if (currentHealth <= 0) return;

        currentHealth -= amount; // 체력 차감

        // 체력이 0 이하가 되면 사망(파괴) 로직 실행
        if (currentHealth <= 0) Die();
    }

    // 체력이 바닥났을 때 호출되는 내부 함수
    void Die()
    {
        // 1. 등록된 모든 이벤트(폭발 효과 등)를 실행합니다.
        OnDestroyed?.Invoke();

        // 2. 건물일 경우 팀에 따라 다른 로그를 출력합니다.
        if (isBuilding)
        {
            Debug.Log(isPlayerSide ? "아군 건물 파괴!" : "적 건물 파괴!");
        }

        // 3. 실제 게임 오브젝트를 월드에서 제거합니다.
        Destroy(gameObject);
    }
}