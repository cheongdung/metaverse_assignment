using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieAI : MonoBehaviour {
    public enum ZombieState { Idle, Chase, Attack, Dead }

    [Header("탐지 설정")]
    public float detectionRange = 10f;
    public float attackRange = 1.8f;
    public float fieldOfView = 120f;

    [Header("이동 설정")]
    public float walkSpeed = 1.5f;
    public float chaseSpeed = 3.5f;

    [Header("전투 설정")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;

    [Header("체력 설정")]
    public float maxHealth = 100f;

    [Header("총알 설정")]
    public string bulletTag = "Bullet";       // 총알 태그
    public float bulletDamage = 100f;          // 총알 1발 데미지

    [Header("레퍼런스")]
    public Transform player;

    private NavMeshAgent agent;
    private Animator animator;
    private ZombieState currentState = ZombieState.Idle;
    private float currentHealth;
    private float lastAttackTime = -999f;
    private float nextDestinationUpdate = 0f;

    private static readonly int HashSpeed = Animator.StringToHash("speed");
    private static readonly int HashAttack = Animator.StringToHash("isAttack");
    private static readonly int HashDead = Animator.StringToHash("isDead");
    private static readonly int HashGrounded = Animator.StringToHash("isGrounded");

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;

        if (player == null) {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        agent.speed = walkSpeed;
        agent.stoppingDistance = attackRange;
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

    // ───────────── 총알 충돌 감지 ─────────────

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag(bulletTag)) {
            TakeDamage(bulletDamage);
            Destroy(collision.gameObject); // 총알 제거
        }
    }

    // Trigger Collider 쓰는 경우도 대비
    void OnTriggerEnter(Collider other) {
        if (other.CompareTag(bulletTag)) {
            TakeDamage(bulletDamage);
            Destroy(other.gameObject);
        }
    }

    // ───────────── 상태별 업데이트 ─────────────

    void UpdateIdle(float dist) {
        agent.isStopped = true;
        ChangeState(ZombieState.Chase);
    }

    void UpdateChase(float dist) {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        if (Time.time >= nextDestinationUpdate) {
            agent.SetDestination(player.position);
            nextDestinationUpdate = Time.time + 0.1f;
        }

        // 이동 방향으로 부드럽게 회전
        if (agent.velocity.sqrMagnitude > 0.1f) {
            Vector3 dir = agent.velocity.normalized;
            dir.y = 0f;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }

        if (dist <= attackRange)
            ChangeState(ZombieState.Attack);
    }

    void UpdateAttack(float dist) {

        // 플레이어 방향으로 부드럽게 회전
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);

        if (Time.time - lastAttackTime >= attackCooldown) {
            lastAttackTime = Time.time;
            animator.SetTrigger(HashAttack);
            Debug.Log("공격");

            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(attackDamage);
        }

        if (dist > attackRange * 1.2f)
            ChangeState(ZombieState.Chase);

    }

    // ───────────── 피격 / 사망 ─────────────

    public void TakeDamage(float damage) {
        if (currentState == ZombieState.Dead) return;
        currentHealth -= damage;

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

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 5f);
    }

    // ───────────── 유틸 ─────────────

    void ChangeState(ZombieState newState) {
        currentState = newState;

        if (newState == ZombieState.Attack) {
            agent.isStopped = true;
            agent.ResetPath();          // ← 경로 초기화로 완전 정지
            agent.speed = chaseSpeed;
        }
        else if (newState == ZombieState.Idle || newState == ZombieState.Dead) {
            agent.speed = walkSpeed;
        }
        else if (newState == ZombieState.Chase) {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }
    }

    void UpdateAnimator() {
        float speed = agent.velocity.magnitude;
        animator.SetFloat(HashSpeed, speed);
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Vector3 leftFOV = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 rightFOV = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftFOV * detectionRange);
        Gizmos.DrawRay(transform.position, rightFOV * detectionRange);
    }
}