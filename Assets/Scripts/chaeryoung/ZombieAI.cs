using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ZombieAI : MonoBehaviour {
    public enum ZombieState { Idle, Chase, Attack, Dead }

    [Header("탐지 설정")]
    public float detectionRange = 10f;   // 플레이어 탐지 범위
    public float attackRange = 1.8f;     // 공격 범위
    public float fieldOfView = 120f;     // 시야각 (도)

    [Header("이동 설정")]
    public float walkSpeed = 1.5f;
    public float chaseSpeed = 3.5f;

    [Header("전투 설정")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;  // 공격 쿨타임 (초)

    [Header("체력 설정")]
    public float maxHealth = 100f;

    [Header("레퍼런스")]
    public Transform player;             // 인스펙터에서 직접 연결하거나 자동 탐지

    // 내부 변수
    private NavMeshAgent agent;
    private Animator animator;
    private ZombieState currentState = ZombieState.Idle;
    private float currentHealth;
    private float lastAttackTime = -999f;

    // 애니메이터 파라미터 해시 (성능 최적화)
    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashAttack = Animator.StringToHash("isAttack");
    private static readonly int HashDead = Animator.StringToHash("isDead");
    private static readonly int HashGrounded = Animator.StringToHash("isGrounded");

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // 플레이어 자동 탐지 (인스펙터에서 연결 안 했을 경우)
        if (player == null) {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        agent.speed = walkSpeed;
        animator.SetBool(HashGrounded, true);
    }

    void Update() {
        if (currentState == ZombieState.Dead) return;
        if (player == null) return;

        float distToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState) {
            case ZombieState.Idle: UpdateIdle(distToPlayer); break;
            case ZombieState.Chase: UpdateChase(distToPlayer); break;
            case ZombieState.Attack: UpdateAttack(distToPlayer); break;
        }

        UpdateAnimator();
    }

    // ───────────── 상태별 업데이트 ─────────────

    void UpdateIdle(float dist) {
        agent.isStopped = true;

        // 탐지 범위 + 시야각 안에 플레이어가 있으면 추적 시작
        if (dist < detectionRange && IsPlayerInFOV())
            ChangeState(ZombieState.Chase);
    }

    void UpdateChase(float dist) {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position); // 이 한 줄이 길찾기 전부!

        if (dist <= attackRange)
            ChangeState(ZombieState.Attack);
        else if (dist > detectionRange * 1.5f)
            ChangeState(ZombieState.Idle);      // 너무 멀어지면 포기
    }

    void UpdateAttack(float dist) {
        agent.isStopped = true;

        // 플레이어 방향으로 천천히 회전
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(dir), 10f * Time.deltaTime);

        // 쿨타임마다 공격
        if (Time.time - lastAttackTime >= attackCooldown) {
            lastAttackTime = Time.time;
            animator.SetTrigger(HashAttack);

            // 플레이어에게 데미지 (PlayerHealth 컴포넌트 있을 경우)
            //PlayerHealth ph = player.GetComponent<PlayerHealth>();
            //if (ph != null) ph.TakeDamage(attackDamage);
        }

        // 범위 벗어나면 다시 추적
        if (dist > attackRange * 1.2f)
            ChangeState(ZombieState.Chase);
    }

    // ───────────── 피격 / 사망 ─────────────

    public void TakeDamage(float damage) {
        if (currentState == ZombieState.Dead) return;

        currentHealth -= damage;

        // 피격 시 즉시 추적 시작
        if (currentState == ZombieState.Idle)
            ChangeState(ZombieState.Chase);

        if (currentHealth <= 0f)
            Die();
    }

    void Die() {
        ChangeState(ZombieState.Dead);
        agent.isStopped = true;
        agent.enabled = false;
        animator.SetTrigger(HashDead);

        // 충돌체 비활성화
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 5초 후 오브젝트 제거
        Destroy(gameObject, 5f);
    }

    // ───────────── 유틸 ─────────────

    void ChangeState(ZombieState newState) {
        currentState = newState;

        if (newState == ZombieState.Idle || newState == ZombieState.Dead)
            agent.speed = walkSpeed;
        else if (newState == ZombieState.Chase)
            agent.speed = chaseSpeed;
    }

    bool IsPlayerInFOV() {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > fieldOfView * 0.5f) return false;

        // 장애물 확인 (Raycast)
        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer,
                            out RaycastHit hit, detectionRange)) {
            if (hit.transform == player) return true;
        }
        return false;
    }

    void UpdateAnimator() {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(HashSpeed, speed);
    }

    // 씬에서 탐지 범위 시각화 (개발용)
    void OnDrawGizmosSelected() {
        // 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 시야각
        Gizmos.color = Color.cyan;
        Vector3 leftFOV = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightFOV = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftFOV * detectionRange);
        Gizmos.DrawRay(transform.position, rightFOV * detectionRange);
    }
}