using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Weapon & Shooting Settings")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float fireRate = 0.15f;

    [Header("Bullet & Spawn References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private Rigidbody2D rb;
    private float nextFireTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Hareket();
        SilahDondur();
        AtesEt();
    }

    void Hareket()
    {
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float y = Input.GetAxisRaw("Vertical");   // W/S

        Vector2 yon = new Vector2(x, y).normalized;
        rb.linearVelocity = yon * moveSpeed;
    }

    void SilahDondur()
    {
        // Mouse'un dünya pozisyonunu al
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 yon = (mousePos - transform.position).normalized;

        float angle = Mathf.Atan2(yon.y, yon.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void AtesEt()
    {
        // Sol tık basılı tutunca ateş et
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (bulletPrefab != null && firePoint != null)
                Instantiate(bulletPrefab, firePoint.position, weaponTransform.rotation);

            nextFireTime = Time.time + fireRate;
        }
    }
}