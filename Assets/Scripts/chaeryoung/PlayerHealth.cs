using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour {
    [Header("체력 설정")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI (선택)")]
    public Slider healthSlider;   // 인스펙터에서 연결 (없어도 됨)

    void Start() {
        currentHealth = maxHealth;

        if (healthSlider != null) {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float damage) {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"플레이어 체력: {currentHealth}/{maxHealth}");

        if (healthSlider != null)
            healthSlider.value = currentHealth;

        if (currentHealth <= 0f)
            Die();
    }

    void Die() {
        Debug.Log("플레이어 사망");
        // 게임오버 처리 (씬 재로드 등)
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}