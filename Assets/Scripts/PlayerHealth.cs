using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can")]
    public int maxHealth = 5;
    private int currentHealth;

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;

    private int originalMaxHealth;
    private int healthBeforeColossus;
    private bool isColossusHealthActive = false;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void HasarAl(int hasar)
    {
        currentHealth -= hasar;
        Debug.Log("Can: " + currentHealth);

        // Hasar sesi
        if (AudioManager.Instance != null) AudioManager.Instance.PlayHit();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Oyuncu oldu!");

            // Olum animasyonunu tetikle
            PlayerAnimator pa = GetComponent<PlayerAnimator>();
            if (pa != null)
            {
                pa.TriggerDeath();
            }

            // Kontrolleri ve fizik etkilesimlerini kapat
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // GameManager uzerinden GameOver durumunu tetikle
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }

            Destroy(gameObject, 0.5f); // Animasyonun oynamasi icin kisa bekleme
        }
    }

    public void ActivateColossusHealth(int multiplier)
    {
        originalMaxHealth = maxHealth;
        healthBeforeColossus = currentHealth;

        maxHealth *= multiplier;
        currentHealth = maxHealth;
        isColossusHealthActive = true;

        Debug.Log("Colossus Can: " + currentHealth);
    }

    public void DeactivateColossusHealth()
    {
        maxHealth = originalMaxHealth;
        currentHealth = Mathf.Min(healthBeforeColossus, maxHealth);
        isColossusHealthActive = false;

        Debug.Log("Can eski haline dondu: " + currentHealth);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            PlayerController pc = GetComponent<PlayerController>();

            if (pc != null && pc.IsColossusActive)
            {
                if (enemy != null)
                {
                    enemy.HasarAl(pc.colossusContactDamage);
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();
                }
                return;
            }

            if (enemy != null)
            {
                HasarAl(enemy.damage);
            }
            else
            {
                HasarAl(1);
            }
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        PlayerController pc = GetComponent<PlayerController>();
        if (pc == null || !pc.IsColossusActive) return;

        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.HasarAl(pc.colossusContactDamage);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayMelee();
            }
        }
    }
}
