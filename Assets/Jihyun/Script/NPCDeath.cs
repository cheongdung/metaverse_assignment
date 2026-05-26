using UnityEngine;

public class NPCDeath : MonoBehaviour
{
    public string bulletTag = "Bullet";

    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(bulletTag))
        {
            Die(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(bulletTag))
        {
            Die(collision.gameObject);
        }
    }

    private void Die(GameObject bullet)
    {
        if (isDead) return;

        isDead = true;

        Debug.Log(gameObject.name + " 사망");

        if (bullet != null)
        {
            Destroy(bullet);
        }

        Destroy(gameObject);
    }
}
