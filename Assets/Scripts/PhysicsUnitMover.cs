using UnityEngine;
using UnityEngine.AI;

public enum UnitState { Idle, Move, Attack, HoldGround, AttackMove }

public class PhysicsUnitMover : MonoBehaviour
{
    [Header("Unit Stats")]
    public float moveSpeed = 5f;
    public float attackRange = 3f;
    public float detectionRange = 20f;
    public float attackRate = 1.0f;
    private float attackTimer = 0f;

    public UnitState currentState = UnitState.Idle;
    public UnitData unitData;
    public float currentHealth;

    [Header("Passive: Berserker Dash")]
    public bool isMeleeUnit = false;
    public float dashSpeedMultiplier = 2.0f;
    public float dashDuration = 0.5f;
    public float dashCooldown = 3.0f;

    [Header("Team Settings")]
    public bool isPlayerUnit = true;

    private float currentDashTimer = 0f;
    private float currentCooldownTimer = 0f;
    private bool isDashing = false;

    private Vector3 finalDestination;
    private Transform attackTarget;
    private Rigidbody rb;
    private NavMeshAgent agent;
    private Color originalColor;
    private Renderer myRenderer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        myRenderer = GetComponentInChildren<Renderer>();
        if (myRenderer != null) originalColor = myRenderer.material.color;

        if (agent != null)
        {
            agent.updatePosition = false;
            agent.updateRotation = true;
            agent.acceleration = 100f;
            agent.angularSpeed = 720f;

            // [겹침 방지 보강]
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(30, 70); // 우선순위를 랜덤하게 주어 서로 잘 비키게 함
            agent.stoppingDistance = 0.5f;
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // 물리 안정성 강화
    }

    void Start()
    {
        if (unitData != null)
        {
            currentHealth = unitData.maxHealth;
            moveSpeed = unitData.moveSpeed;
            attackRange = unitData.attackRange;
        }
        finalDestination = transform.position;
        currentCooldownTimer = dashCooldown;
    }

    void FixedUpdate()
    {
        if (attackTimer > 0) attackTimer -= Time.fixedDeltaTime;
        HandleBerserkerDash();

        if (currentState != UnitState.Move) SearchForEnemy();

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

        if (agent != null && agent.isOnNavMesh)
        {
            agent.nextPosition = transform.position;
        }
    }

    void MoveTo(Vector3 pos)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (agent.destination != pos) agent.SetDestination(pos);
        agent.isStopped = false;

        float finalSpeed = isDashing ? moveSpeed * dashSpeedMultiplier : moveSpeed;
        Vector3 worldVelocity = agent.desiredVelocity.normalized * finalSpeed;

        float yVel = rb.linearVelocity.y;
        if (yVel > 0) yVel *= 0.5f;

        rb.linearVelocity = new Vector3(worldVelocity.x, yVel, worldVelocity.z);

        // [도착 판정 개선]
        // 남은 거리가 stoppingDistance보다 작으면 확실히 멈추게 함
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            StopMoving();
            if (currentState == UnitState.Move) currentState = UnitState.Idle;
        }
    }

    public void SetTarget(Vector3 target)
    {
        attackTarget = null;
        finalDestination = target;
        currentState = UnitState.Move;
    }

    public void SetAttackMove(Vector3 target)
    {
        attackTarget = null;
        finalDestination = target;
        currentState = UnitState.AttackMove;
    }

    public void ResetColor()
    {
        if (myRenderer != null) myRenderer.material.color = originalColor;
    }

    void HandleAttack()
    {
        if (attackTarget == null || !attackTarget.gameObject.activeInHierarchy)
        {
            attackTarget = null;
            currentState = (Vector3.Distance(transform.position, finalDestination) > 1.2f) ? UnitState.AttackMove : UnitState.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, attackTarget.position);

        if (dist <= attackRange)
        {
            StopMoving();
            RotateTowards(attackTarget.position);
            if (attackTimer <= 0)
            {
                PerformAttack();
                attackTimer = 1f / attackRate;
            }
        }
        else
        {
            MoveTo(attackTarget.position);
        }
    }

    bool SearchForEnemy()
    {
        if (attackTarget != null && attackTarget.gameObject.activeInHierarchy) return true;

        Collider[] cols = Physics.OverlapSphere(transform.position, detectionRange);
        float closestDist = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var col in cols)
        {
            Health targetHealth = col.GetComponent<Health>();
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

    private void HandleBerserkerDash()
    {
        if (!isMeleeUnit) return;
        if (isDashing)
        {
            currentDashTimer -= Time.fixedDeltaTime;
            if (currentDashTimer <= 0) isDashing = false;
        }
        if (currentCooldownTimer > 0) currentCooldownTimer -= Time.fixedDeltaTime;
        else if (currentState == UnitState.Move || currentState == UnitState.AttackMove || attackTarget != null)
            TriggerDash();
    }

    private void TriggerDash()
    {
        isDashing = true;
        currentDashTimer = dashDuration;
        currentCooldownTimer = dashCooldown;
    }

    void PerformAttack()
    {
        if (attackTarget == null) return;

        float totalDamage = unitData.attackDamage;
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
            Debug.Log($"<color=cyan>[공격 완료]</color> {unitData.unitName} -> {attackTarget.name}에게 <color=red>{totalDamage}</color> 피해를 입힘!");
        }
    }
}