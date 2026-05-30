using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Can")]
    public int maxHealth = 5;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void HasarAl(int hasar)
    {
        currentHealth -= hasar;
        Debug.Log("Can: " + currentHealth); // test için

        if (currentHealth <= 0)
        {
            Debug.Log("Oyuncu öldü!");
            Destroy(gameObject); // þimdilik yok olsun
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HasarAl(1);
        }
    }
}