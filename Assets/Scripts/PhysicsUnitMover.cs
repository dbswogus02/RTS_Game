using UnityEngine;
using UnityEngine.AI;

// 유닛의 현재 행동 상태를 관리하는 유한 상태 머신(FSM) 열거형
public enum UnitState { Idle, Move, Attack, HoldGround, AttackMove }

public class PhysicsUnitMover : MonoBehaviour
{
    [Header("Unit Stats")]
    public float moveSpeed = 5f;      // 이동 속도
    public float attackRange = 3f;    // 사거리
    public float detectionRange = 20f; // 적 감지 범위
    public float attackRate = 1.0f;   // 초당 공격 횟수
    private float attackTimer = 0f;    // 공격 쿨타임용 타이머

    public UnitState currentState = UnitState.Idle; // 현재 상태
    public UnitData unitData;         // 유닛 스펙이 담긴 ScriptableObject
    public float currentHealth;       // 현재 체력

    [Header("Passive: Berserker Dash")]
    public bool isMeleeUnit = false;      // 근접 유닛인지 판별
    public float dashSpeedMultiplier = 2.0f; // 대쉬 시 속도 증가폭
    public float dashDuration = 0.5f;     // 대쉬 지속 시간
    public float dashCooldown = 3.0f;     // 대쉬 쿨타임 (전체)
    public ParticleSystem dashEffect;     // 대쉬 중 파티클 효과

    [Header("Team Settings")]
    public bool isPlayerUnit = true;      // 플레이어 진영 여부

    private float currentDashTimer = 0f;
    private float currentCooldownTimer = 0f; // 현재 남은 쿨타임 시간
    private bool isDashing = false;

    private Vector3 finalDestination;     // 이동 목표 지점
    private Transform attackTarget;       // 공격 목표 대상
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Color originalColor;
    private Renderer myRenderer;

    // --- UIManager에서 빨간 줄이 떴던 원인이자 해결책 ---
    // 외부에서 currentCooldownTimer를 읽을 수 있게 해주는 창구입니다.
    public float DashCooldownTimer => currentCooldownTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        myRenderer = GetComponentInChildren<Renderer>();
        if (myRenderer != null) originalColor = myRenderer.material.color;

        if (agent != null)
        {
            // [중요] NavMesh가 위치를 직접 수정하지 않고 물리(Rigidbody)가 담당하게 설정
            agent.updatePosition = false;
            agent.updateRotation = true;
            agent.acceleration = 100f;
            agent.angularSpeed = 720f;

            // 유닛끼리 겹치지 않게 우선순위를 랜덤으로 부여 (병목 현상 방지)
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(30, 70);
            agent.stoppingDistance = 0.5f;
        }

        // 유닛이 물리 충돌 시 넘어지지 않도록 회전 축 고정
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Start()
    {
        // 데이터 에셋으로부터 초기 수치 설정
        if (unitData != null)
        {
            currentHealth = unitData.maxHealth;
            moveSpeed = unitData.moveSpeed;
            attackRange = unitData.attackRange;
        }
        finalDestination = transform.position;
        currentCooldownTimer = dashCooldown;
    }

    void FixedUpdate() // 물리 기반 이동을 위해 FixedUpdate 사용
    {
        if (attackTimer > 0) attackTimer -= Time.fixedDeltaTime;

        HandleBerserkerDash(); // 대쉬 로직 체크

        // 강제 이동 중이 아니라면 주변 적을 항상 감시
        if (currentState != UnitState.Move) SearchForEnemy();

        // 상태별 행동 지침
        switch (currentState)
        {
            case UnitState.Idle:
            case UnitState.HoldGround:
                StopMoving();
                break;
            case UnitState.Move:
            case UnitState.AttackMove:
                MoveTo(finalDestination);
                break;
            case UnitState.Attack:
                HandleAttack();
                break;
        }

        // NavMeshAgent의 논리적 위치를 실제 오브젝트 위치에 동기화
        if (agent != null && agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }
    }

    // NavMesh의 경로 정보를 Rigidbody의 속도로 변환하여 물리적 이동 구현
    void MoveTo(Vector3 pos)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.destination != pos) agent.SetDestination(pos);
        agent.isStopped = false;

        // 대쉬 중이라면 속도 증가
        float finalSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
        Vector3 worldVelocity = agent.desiredVelocity.normalized * finalSpeed;

        float yVel = rb.linearVelocity.y;
        if (yVel > 0) yVel *= 0.5f; // 점프 방지 로직

        rb.linearVelocity = new Vector3(worldVelocity.x, yVel, worldVelocity.z);

        // 목적지 도착 판정
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StopMoving();
            if (currentState == UnitState.Move) currentState = UnitState.Idle;
        }
    }

    // 마우스 우클릭 등 일반 이동 명령 시 호출
    public void SetTarget(Vector3 target)
    {
        attackTarget = null;
        finalDestination = target;
        currentState = UnitState.Move;
    }

    // 'A' 키 등 공격 이동 명령 시 호출
    public void SetAttackMove(Vector3 target)
    {
        attackTarget = null;
        finalDestination = target;
        currentState = UnitState.AttackMove;
    }

    // 공격 상태일 때 적과의 거리에 따른 행동
    void HandleAttack()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            attackTarget = null;
            // 타겟이 사라지면 목적지가 남았을 경우 다시 이동, 없으면 대기
            currentState = (Vector3.Distance(transform.position, finalDestination) > 1.2f) ? UnitState.AttackMove : UnitState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, attackTarget.position);

        if (dist <= attackRange) // 사거리 내
        {
            StopMoving();
            RotateTowards(attackTarget.position); // 적을 조준
            if (attackTimer <= 0)
            {
                PerformAttack(); // 실제 타격
                attackTimer = 1f / attackRate;
            }
        }
        else // 사거리 밖이면 추격
        {
            MoveTo(attackTarget.position);
        }
    }

    // 주변의 적 진영 유닛을 찾는 센서 함수
    bool SearchForEnemy()
    {
        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy) return true;

        Collider[] cols = Physics.OverlapSphere(transform.position, detectionRange);
        float closestDist = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var col in cols)
        {
            Health targetHealth = col.GetComponent<Health>();
            // 팀이 다르고 살아있는 유닛만 타겟팅
            if (targetHealth != null && targetHealth.isPlayerSide != this.isPlayerUnit)
            {
                float dist = Vector3.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTarget = col.transform;
                }
            }
        }

        if (closestTarget != null)
        {
            attackTarget = closestTarget;
            if (currentState != UnitState.Move) currentState = UnitState.Attack;
            return true;
        }
        return false;
    }

    void StopMoving()
    {
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 15f);
    }

    // 근접 유닛 패시브: 공격 대상이 생기면 빠르게 접근
    private void HandleBerserkerDash()
    {
        if (!isMeleeUnit) return;

        if (isDashing)
        {
            currentDashTimer -= Time.fixedDeltaTime;
            if (currentDashTimer <= 0)
            {
                isDashing = false;
                if (dashEffect != null) dashEffect.Stop();
            }
        }

        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.fixedDeltaTime;
        }
        else if (attackTarget != null && attackTarget.gameObject.activeInHierarchy)
        {
            TriggerDash();
        }
    }

    private void TriggerDash()
    {
        isDashing = true;
        currentDashTimer = dashDuration;
        currentCooldownTimer = dashCooldown;
        if (dashEffect != null) dashEffect.Play();
    }

    // 실제로 데미지를 입히고 보너스 데미지를 적용하는 함수
    void PerformAttack()
    {
        if (attackTarget == null) return;

        float totalDamage = unitData.attackDamage;
        // 업그레이드 매니저가 있다면 보너스 합산
        if (ResearchManager.Instance != null)
        {
            if (unitData.unitName.Contains("Melee")) totalDamage += ResearchManager.Instance.meleeAttackBonus;
            else if (unitData.unitName.Contains("Range")) totalDamage += ResearchManager.Instance.rangeAttackBonus;
            else if (unitData.unitName.Contains("Cavalry")) totalDamage += ResearchManager.Instance.cavalryAttackBonus;
        }

        Health targetHealth = attackTarget.GetComponent<Health>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(totalDamage);
            Debug.Log($"<color=cyan>[공격]</color> {unitData.unitName} -> {attackTarget.name} : {totalDamage} 피해");
        }
    }

    // 1. 유닛의 색상을 원래대로 되돌리는 함수
    // RTSUnitManager.DeselectAll()에서 호출할 때 발생하던 오류를 해결합니다.
    public void ResetColor()
    {
        if (myRenderer != null)
        {
            myRenderer.material.color = originalColor;
        }
    }

    // 2. 유닛의 하이라이트 색상을 변경하는 함수 (선택 사항이지만 유용함)
    public void SetHighlightColor(Color color)
    {
        if (myRenderer != null)
        {
            myRenderer.material.color = color;
        }
    }
}