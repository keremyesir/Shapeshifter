using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Mermiyi silahýn baktýðý yöne (kendi sað eksenine) doðru fýrlatýr
        rb.linearVelocity = transform.right * speed;

        // Performans için mermiyi belli bir süre sonra sahneden siler
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ýleride düþman eklediðimizde burasý tetiklenecek
        if (collision.CompareTag("Enemy"))
        {
            // Vurulma efektleri buraya gelecek
            Destroy(gameObject);
        }
    }
}