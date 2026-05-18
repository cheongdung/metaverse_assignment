using UnityEngine;

/// <summary>
/// 총알 제너레이터
/// - Player 위치에서 총알 생성
/// - 카메라(헤드셋) 바라보는 방향으로 발사
/// </summary>
public class BulletGenerator_cr : MonoBehaviour {
    [Header("참조")]
    public GameObject bulletPrefab;     // BulletController가 붙은 프리팹
    public Transform playerTransform;  // Player 오브젝트
    public Transform firePoint;        // 총구 위치 (없으면 Player 위치 사용)

    [Header("발사 설정")]
    public KeyCode fireKey = KeyCode.Mouse0;  // 마우스 좌클릭
    public float fireRate = 0.2f;            // 발사 간격 (초)
    public float bulletOffsetY = 1.5f;            // Player 중심 기준 총구 높이 오프셋

    private float _nextFireTime = 0f;
    private Camera _cam;

    private void Awake() {
        // Player 자동 탐색
        if (playerTransform == null) {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        _cam = Camera.main;
    }

    private void Update() {
        if (Input.GetKey(fireKey) && Time.time >= _nextFireTime) {
            _nextFireTime = Time.time + fireRate;
            SpawnBullet();
        }
    }

    private void SpawnBullet() {
        if (bulletPrefab == null || playerTransform == null) {
            Debug.LogWarning("BulletGenerator: bulletPrefab 또는 playerTransform이 없습니다.");
            return;
        }

        // 생성 위치: firePoint 지정 시 해당 위치, 없으면 Player 위치 + Y 오프셋
        Vector3 spawnPos = (firePoint != null)
            ? firePoint.position
            : playerTransform.position + Vector3.up * bulletOffsetY;

        // 발사 방향: 카메라 forward (VR이면 헤드셋 방향)
        Vector3 direction = (_cam != null)
            ? _cam.transform.forward
            : playerTransform.forward;

        // 총알 생성 & 발사
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(direction));

        BulletController_cr bc = bullet.GetComponent<BulletController_cr>();
        if (bc != null)
            bc.Shoot(direction);
        else
            Debug.LogWarning("BulletGenerator: bulletPrefab에 BulletController가 없습니다.");
    }

    // 에디터에서 총구 위치 시각화
    private void OnDrawGizmosSelected() {
        if (playerTransform == null) return;

        Gizmos.color = Color.yellow;
        Vector3 gizmoPos = (firePoint != null)
            ? firePoint.position
            : playerTransform.position + Vector3.up * bulletOffsetY;

        Gizmos.DrawWireSphere(gizmoPos, 0.1f);
        Gizmos.DrawRay(gizmoPos, playerTransform.forward * 2f);
    }
}