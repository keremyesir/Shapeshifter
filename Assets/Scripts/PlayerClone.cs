using UnityEngine;

public class PlayerClone : MonoBehaviour
{
    private Transform player;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;
    private Vector2 followOffset;

    [Header("Clone Settings")]
    public float followSpeed = 8f;
    public GameObject bulletPrefab;

    private float nextFireTime = 0f;

    // Silah senkronizasyonu icin gerekli degiskenler
    private GameObject cloneWeaponObject;
    private SpriteRenderer cloneWeaponSpriteRenderer;
    private Transform cloneWeaponTransform;

    public void Initialize(Transform playerTransform, PlayerController controller, GameObject bullet, Sprite playerSprite)
    {
        player = playerTransform;
        playerController = controller;
        bulletPrefab = bullet;

        // Sprite ayarlari
        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            spriteRenderer.sprite = playerSprite;
        }

        // Yari saydam gorunum
        Color c = spriteRenderer.color;
        c.a = 0.6f; // Biraz daha gorunur yapalim
        spriteRenderer.color = c;

        // Sorting layer - player ile ayni katmanda dursun
        SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR != null)
        {
            spriteRenderer.sortingLayerID = playerSR.sortingLayerID;
            spriteRenderer.sortingLayerName = playerSR.sortingLayerName;
            spriteRenderer.sortingOrder = playerSR.sortingOrder; // Player ile ayni katman sirasi
        }
        else
        {
            spriteRenderer.sortingOrder = 5;
        }

        // Oyuncunun yaninda dogma ofseti
        followOffset = new Vector2(Random.Range(0, 2) == 0 ? -1.2f : 1.2f, 0.3f);

        // Baslangic pozisyonu (Z ekseninin 0 oldugundan emin olalim)
        transform.position = new Vector3(player.position.x + followOffset.x, player.position.y + followOffset.y, 0f);

        // OLCER/SCALE ESITLEME: Klonun boyutunu oyuncuyla birebir ayni yapalim
        transform.localScale = player.localScale;

        // --- SILAH OLUSTURMA VE SENKRONIZASYONU ---
        cloneWeaponObject = new GameObject("CloneWeapon");
        cloneWeaponObject.transform.SetParent(transform);
        cloneWeaponObject.transform.localPosition = new Vector3(0.15f, -0.05f, 0f); // El hizasinda
        cloneWeaponTransform = cloneWeaponObject.transform;

        cloneWeaponSpriteRenderer = cloneWeaponObject.AddComponent<SpriteRenderer>();
        if (playerController != null && cloneWeaponSpriteRenderer != null)
        {
            cloneWeaponSpriteRenderer.sprite = playerController.GetCurrentWeaponSprite();
            
            // Yari saydam yapalim silahini da
            Color wc = cloneWeaponSpriteRenderer.color;
            wc.a = 0.7f;
            cloneWeaponSpriteRenderer.color = wc;

            if (playerSR != null)
            {
                cloneWeaponSpriteRenderer.sortingLayerID = playerSR.sortingLayerID;
                cloneWeaponSpriteRenderer.sortingLayerName = playerSR.sortingLayerName;
                cloneWeaponSpriteRenderer.sortingOrder = playerSR.sortingOrder + 1; // Karakterin hemen ustunde dursun
            }
        }
    }

    void Update()
    {
        if (player == null || playerController == null)
        {
            Destroy(gameObject);
            return;
        }

        // Oyuncuyu takip et
        Vector3 targetPos = player.position + (Vector3)followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        // Fareye gore yuz dondurme
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool lookLeft = mousePos.x < transform.position.x;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = lookLeft;
        }

        // Player'in sprite'ini senkronize et
        SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
        if (playerSR != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = playerSR.sprite;
        }

        // Silah Sprite'ini ve Rotasyonunu Senkronize Et
        if (cloneWeaponSpriteRenderer != null && playerController != null)
        {
            cloneWeaponSpriteRenderer.sprite = playerController.GetCurrentWeaponSprite();
        }

        if (cloneWeaponTransform != null)
        {
            Vector2 aimDir = (mousePos - cloneWeaponTransform.position).normalized;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            cloneWeaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Silah flip kontrolu
            if (lookLeft)
            {
                cloneWeaponTransform.localScale = new Vector3(1f, -1f, 1f);
                cloneWeaponTransform.localPosition = new Vector3(-0.15f, -0.05f, 0f); // Sol el hizasi
            }
            else
            {
                cloneWeaponTransform.localScale = new Vector3(1f, 1f, 1f);
                cloneWeaponTransform.localPosition = new Vector3(0.15f, -0.05f, 0f); // Sag el hizasi
            }
        }

        // Oyuncu ates ediyorsa klon da etsin
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime && bulletPrefab != null)
        {
            Vector2 dir = ((Vector2)(mousePos - transform.position)).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            // Klonun silahinin ucundan ates etmek icin
            Vector3 spawnPos = cloneWeaponTransform != null ? cloneWeaponTransform.position + cloneWeaponTransform.right * 0.4f : transform.position;

            // Oyuncunun aktif silah moduna gore ates et
            PlayerController.WeaponMode mode = playerController.GetCurrentWeaponMode();

            GameObject bullet1 = null;
            GameObject bullet2 = null;
            GameObject bullet3 = null;

            switch (mode)
            {
                case PlayerController.WeaponMode.Pistol:
                    bullet1 = Instantiate(bulletPrefab, spawnPos, rotation);
                    nextFireTime = Time.time + 0.2f;
                    break;

                case PlayerController.WeaponMode.Shotgun:
                    bullet1 = Instantiate(bulletPrefab, spawnPos, rotation);
                    bullet2 = Instantiate(bulletPrefab, spawnPos, rotation * Quaternion.Euler(0, 0, -15f));
                    bullet3 = Instantiate(bulletPrefab, spawnPos, rotation * Quaternion.Euler(0, 0, 15f));
                    nextFireTime = Time.time + 0.4f;
                    break;

                case PlayerController.WeaponMode.Raygun:
                    bullet1 = Instantiate(bulletPrefab, spawnPos, rotation);
                    nextFireTime = Time.time + 0.05f;
                    break;
            }

            // Eger oyuncu Colossus (Dev) modundaysa, klonun mermileri de buyusun ve guclensin
            if (playerController.IsColossusActive)
            {
                ApplyColossusBulletModifiers(bullet1);
                ApplyColossusBulletModifiers(bullet2);
                ApplyColossusBulletModifiers(bullet3);
            }
        }
    }

    void ApplyColossusBulletModifiers(GameObject bulletObj)
    {
        if (bulletObj == null) return;
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetScale(2.5f); // Klon mermileri de 2.5 kat buyuk
            bulletScript.SetDamage(3);   // Klon mermileri de 3 hasar vurur
        }
    }
}
