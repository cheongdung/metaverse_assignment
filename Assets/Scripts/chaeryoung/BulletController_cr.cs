using UnityEngine;

/// <summary>
/// 총알 컨트롤러
/// - Shoot() 으로 발사
/// - 충돌 시 또는 y <= -30 이면 Destroy
/// </summary>
public class BulletController_cr : MonoBehaviour {
    [Header("총알 설정")]
    public float speed = 20f;
    public float damage = 10f;
    public float destroyBelow = -30f;   // 이 y값 이하면 제거

    private Vector3 _direction;
    private bool _fired = false;

    public float bulletTime = 0.8f;

    float time = 0;

    private void Update() {
        if (!_fired) return;

        // 방향으로 이동
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);

        // y값 -30 이하면 Destroy
        if (transform.position.y <= destroyBelow)
            Destroy(gameObject);

        // bulletTime초 후 Destroy
        time += Time.deltaTime;
        if (time > bulletTime) Destroy(gameObject);
    }

    /// <summary>
    /// 발사 방향 설정 후 총알 활성화
    /// </summary>
    public void Shoot(Vector3 direction) {
        _direction = direction.normalized;
        _fired = true;
    }

    // 트리거 콜라이더와 충돌 시 Destroy
    private void OnTriggerEnter(Collider other) {
        Destroy(gameObject);
    }
}