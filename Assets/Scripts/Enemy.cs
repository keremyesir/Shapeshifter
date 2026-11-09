using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Can")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Hareket")]
    public float moveSpeed = 2f;

    [Header("Combat")]
    public int damage = 1;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead || player == null) return;

        Vector2 yon = (player.position - transform.position).normalized;
        rb.linearVelocity = yon * moveSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
    }

    public void HasarAl(int hasar)
    {
        if (isDead) return;

        currentHealth -= hasar;

        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            // Undead Survivor paketindeki dusman animatorlerinde "Dead" parametresi bool (Boolean) tipindedir.
            animator.SetBool("Dead", true);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEnemyDead();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
    }
}