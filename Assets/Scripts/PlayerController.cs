using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [Header("Movement & Recoil Settings")]
    [SerializeField] private float recoilForce = 8f;

    [Header("Weapon & Shooting Settings")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float fireRate = 0.15f;

    [Header("Bullet & Spawn References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    private Rigidbody2D rb;
    private float nextFireTime = 0f;

    // Mobil Kaydırma (Swipe/Joystick) Değişkenleri
    private Vector2 touchStartPos;
    private Vector2 currentShootDirection = Vector2.right; // Varsayılan ateş yönü

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // 1. Parmağı ekrana İLK koyduğu an (Merkez Noktası Belirleme)
        if (Input.GetMouseButtonDown(0))
        {
            // Eğer dokunma TouchZone (UI) üzerindeyse merkez noktasını kaydet
            if (EventSystem.current.IsPointerOverGameObject())
            {
                touchStartPos = Input.mousePosition;
            }
        }

        // 2. Parmak ekranda BASILI TUTULDUĞU sürece (Yön Hesaplama ve Ateş)
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 touchCurrentPos = Input.mousePosition;

                // Parmağın ilk dokunulan yerden ne kadar ve ne yöne kaydırıldığını buluyoruz
                Vector2 offset = touchCurrentPos - touchStartPos;

                // Eğer oyuncu parmağını yeterince kaydırdıysa yeni yönü hesapla (Ölü noktayı engellemek için)
                if (offset.magnitude > 10f)
                {
                    currentShootDirection = offset.normalized;
                }

                // Silahı parmağın kaydırıldığı yöne döndür (Her yöne dönebilir!)
                RotateWeapon(currentShootDirection);

                // Ateş etme döngüsü
                if (Time.time >= nextFireTime)
                {
                    HandleShootAndRecoil(currentShootDirection);
                    nextFireTime = Time.time + fireRate;
                }
            }
        }
    }

    private void RotateWeapon(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShootAndRecoil(Vector2 shootDirection)
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, weaponTransform.rotation);
        }

        // Geri tepme, hesaplanan kaydırma yönünün tam tersidir
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(-shootDirection * recoilForce, ForceMode2D.Impulse);
    }
}