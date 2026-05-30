using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Can")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Hareket")]
    public float moveSpeed = 2f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        // Player'a doðru yönü hesapla
        Vector2 yon = (player.position - transform.position).normalized;
        rb.linearVelocity = yon * moveSpeed;
    }

    public void HasarAl(int hasar)
    {
        currentHealth -= hasar;
        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player'ýn mermi tag'i "Bullet" ise hasar al
        if (other.CompareTag("Bullet"))
        {
            HasarAl(1);
            Destroy(other.gameObject);
        }
    }
}