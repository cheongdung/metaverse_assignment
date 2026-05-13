using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// VR용 플레이어 이동 스크립트
/// - WASD / 키보드로 이동 (PC 테스트용 겸용)
/// - 이동 방향 기준: VR 헤드셋이 바라보는 방향
/// - Ctrl: 걷기 / 기본: 뛰기
/// - 마우스 회전 없음 (헤드셋이 카메라 직접 제어)
/// 
/// 씬 구조:
/// Player (이 스크립트 + CharacterController)
/// └── XR Origin
///     └── Camera Offset
///         └── Main Camera (VR 헤드셋 연동)
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement_cr : MonoBehaviour {
    [Header("이동 속도")]
    public float runSpeed = 4f;
    public float walkSpeed = 2f;

    [Header("중력 & 점프")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("VR 참조")]
    [Tooltip("XR Origin 하위의 Main Camera Transform")]
    public Transform vrCamera;   // Inspector에서 XR Origin > Camera Offset > Main Camera 할당

    [Header("이동 방향 기준")]
    [Tooltip("true = 헤드셋 바라보는 방향 기준 / false = 플레이어 몸통 기준")]
    public bool moveRelativeToHead = true;

    // ── 내부 ──────────────────────────────────────────────────────
    private CharacterController _cc;
    private Vector3 _velocity;
    private bool _isWalking;

    private void Awake() {
        _cc = GetComponent<CharacterController>();

        // vrCamera 자동 탐색 (Inspector 미할당 시)
        if (vrCamera == null && Camera.main != null)
            vrCamera = Camera.main.transform;
    }

    private void Start() {
        // VR 모드에서는 커서 숨김 불필요하지만, PC 테스트 시 편의상 유지
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update() {
        HandleMovement();
        HandleGravityAndJump();
    }

    // ── 이동 ──────────────────────────────────────────────────────
    private void HandleMovement() {
        _isWalking = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 마우스 X로 플레이어 좌우 회전
        float mouseX = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = right * h + forward * v;
        if (moveDir.magnitude > 1f) moveDir.Normalize();

        float speed = _isWalking ? walkSpeed : runSpeed;
        _cc.Move(moveDir * speed * Time.deltaTime);
    }

    // ── 중력 & 점프 ────────────────────────────────────────────────
    private void HandleGravityAndJump() {
        if (_cc.isGrounded) {
            if (_velocity.y < 0f) _velocity.y = -2f;

            if (Input.GetButtonDown("Jump"))
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}