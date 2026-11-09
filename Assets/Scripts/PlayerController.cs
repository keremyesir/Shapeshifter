using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Silah modlarini belirleyen Enum yapisi
    public enum WeaponMode { Pistol, Shotgun, Raygun }
    private WeaponMode currentWeaponMode = WeaponMode.Pistol;

    // Vucut yeteneklerini belirleyen Enum
    private enum BodyAbilityType { None, Clone, Colossus }
    private BodyAbilityType activeBodyAbility = BodyAbilityType.None;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Weapon & Shooting Settings")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private float pistolFireRate = 0.2f;
    [SerializeField] private float shotgunFireRate = 0.4f;
    [SerializeField] private float raygunFireRate = 0.05f;

    [Header("Weapon Sprites (Gorseller)")]
    [SerializeField] private Sprite pistolSprite;
    [SerializeField] private Sprite shotgunSprite;
    [SerializeField] private Sprite raygunSprite;

    [Header("Bullet & Spawn References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Weapon Ability Durations")]
    [SerializeField] private float shotgunDuration = 7f;
    [SerializeField] private float raygunDuration = 5f;

    [Header("Weapon Cooldowns")]
    [SerializeField] private float shotgunCooldown = 20f;
    [SerializeField] private float raygunCooldown = 20f;

    [Header("Clone Ability Settings (Key 3)")]
    [SerializeField] private float cloneDuration = 10f;
    [SerializeField] private float cloneCooldown = 20f;

    [Header("Colossus Ability Settings (Key 4)")]
    [SerializeField] private float colossusDuration = 8f;
    [SerializeField] private float colossusCooldown = 20f;
    [SerializeField] private float colossusScaleMultiplier = 3f;
    [SerializeField] private int colossusHealthMultiplier = 10;
    public int colossusContactDamage = 5;

    private Rigidbody2D rb;
    private SpriteRenderer characterSpriteRenderer;
    private float nextFireTime = 0f;

    // Silah yetenek zamanlayicilari
    private float weaponAbilityTimer = 0f;
    private bool isWeaponAbilityActive = false;
    private float shotgunCooldownTimer = 0f;
    private float raygunCooldownTimer = 0f;

    // Vucut yetenek zamanlayicilari
    private float bodyAbilityTimer = 0f;
    private float cloneCooldownTimer = 0f;
    private float colossusCooldownTimer = 0f;

    // Clone referanslari
    private GameObject activeClone = null;

    // Colossus referanslari
    private bool _isColossusActive = false;
    public bool IsColossusActive => _isColossusActive;
    private Vector3 originalScale;

    // Public getters - UI ve diger scriptlerin degerleri sorgulamasi icin
    public WeaponMode GetCurrentWeaponMode() => currentWeaponMode;

    public Sprite GetCurrentWeaponSprite()
    {
        if (weaponSpriteRenderer != null) return weaponSpriteRenderer.sprite;
        
        switch (currentWeaponMode)
        {
            case WeaponMode.Shotgun: return shotgunSprite;
            case WeaponMode.Raygun: return raygunSprite;
            default: return pistolSprite;
        }
    }

    // Guncel bekleme ve yetenek durumu oranlari (0 = hazir/doldu, 1 = yeni basladi/full bekleme)
    public float GetWeaponCooldownRatio()
    {
        float maxCD = 20f; // Global silah bekleme suresi 20sn
        float activeCD = Mathf.Max(shotgunCooldownTimer, raygunCooldownTimer);
        return activeCD > 0f ? (activeCD / maxCD) : 0f;
    }

    public float GetBodyCooldownRatio()
    {
        float maxCD = 30f; // Global vucut bekleme suresi 30sn
        float activeCD = Mathf.Max(cloneCooldownTimer, colossusCooldownTimer);
        return activeCD > 0f ? (activeCD / maxCD) : 0f;
    }

    public bool IsWeaponAbilityActive() => isWeaponAbilityActive;
    public bool IsBodyAbilityActive() => activeBodyAbility != BodyAbilityType.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        characterSpriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        SetWeaponMode(WeaponMode.Pistol);
    }

    void Update()
    {
        Hareket();
        SilahDondur();
        AtesEt();
        SilahYetenekGirdisi();
        VucutYetenekGirdisi();
        SilahYetenekSureKontrolu();
        VucutYetenekSureKontrolu();
    }

    void Hareket()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 yon = new Vector2(x, y).normalized;
        rb.linearVelocity = yon * moveSpeed;
    }

    void SilahDondur()
    {
        if (weaponTransform == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 yon = (mousePos - transform.position).normalized;

        float angle = Mathf.Atan2(yon.y, yon.x) * Mathf.Rad2Deg;
        weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // Ayna efekti
        if (mousePos.x < transform.position.x)
        {
            if (characterSpriteRenderer != null) characterSpriteRenderer.flipX = true;
            weaponTransform.localScale = new Vector3(1f, -1f, 1f);
        }
        else
        {
            if (characterSpriteRenderer != null) characterSpriteRenderer.flipX = false;
            weaponTransform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    // =============================================
    // SILAH YETENEKLERI (1 = Shotgun, 2 = Raygun)
    // =============================================
    void SilahYetenekGirdisi()
    {
        // Global bekleme kontrolu: Shotgun veya Raygun cooldown'larindan herhangi biri aktifse kullanilamaz
        float activeWeaponCooldown = Mathf.Max(shotgunCooldownTimer, raygunCooldownTimer);

        // 1 tusu - Shotgun
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (isWeaponAbilityActive)
            {
                Debug.Log("Zaten aktif bir silah yetenegi devrede!");
                return;
            }
            if (activeWeaponCooldown > 0f)
            {
                Debug.Log($"Silah yetenekleri bekleme suresinde! (Kalan: {activeWeaponCooldown:F1}s)");
                return;
            }

            SetWeaponMode(WeaponMode.Shotgun);
            weaponAbilityTimer = shotgunDuration;
            isWeaponAbilityActive = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAbility();
            Debug.Log("Shotgun Formuna gecildi!");
        }

        // 2 tusu - Raygun
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (isWeaponAbilityActive)
            {
                Debug.Log("Zaten aktif bir silah yetenegi devrede!");
                return;
            }
            if (activeWeaponCooldown > 0f)
            {
                Debug.Log($"Silah yetenekleri bekleme suresinde! (Kalan: {activeWeaponCooldown:F1}s)");
                return;
            }

            SetWeaponMode(WeaponMode.Raygun);
            weaponAbilityTimer = raygunDuration;
            isWeaponAbilityActive = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAbility();
            Debug.Log("Raygun Formuna gecildi!");
        }
    }

    void SilahYetenekSureKontrolu()
    {
        if (isWeaponAbilityActive)
        {
            weaponAbilityTimer -= Time.deltaTime;
            if (weaponAbilityTimer <= 0)
            {
                // Silah yetenegi bittiginde global 20sn cooldown'i baslatmak icin ikisine de atama yapiliyor
                shotgunCooldownTimer = shotgunCooldown;
                raygunCooldownTimer = shotgunCooldown; // Shotgun ve Raygun cooldown'lari ayni (20sn)

                Debug.Log("Silah yetenek suresi bitti. 20sn global bekleme basladi.");

                SetWeaponMode(WeaponMode.Pistol);
                isWeaponAbilityActive = false;
            }
        }

        if (shotgunCooldownTimer > 0f) shotgunCooldownTimer -= Time.deltaTime;
        if (raygunCooldownTimer > 0f) raygunCooldownTimer -= Time.deltaTime;
    }

    // =============================================
    // VUCUT YETENEKLERI (C = Clone, V = Colossus)
    // =============================================
    void VucutYetenekGirdisi()
    {
        // Global bekleme kontrolu: Clone veya Colossus cooldown'larindan herhangi biri aktifse kullanilamaz
        float activeBodyCooldown = Mathf.Max(cloneCooldownTimer, colossusCooldownTimer);

        // C tusu - Clone (Klonlama)
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (activeBodyAbility != BodyAbilityType.None)
            {
                Debug.Log("Zaten aktif bir vucut yetenegi devrede!");
                return;
            }
            if (activeBodyCooldown > 0f)
            {
                Debug.Log($"Vucut yetenekleri bekleme suresinde! (Kalan: {activeBodyCooldown:F1}s)");
                return;
            }

            ActivateClone();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAbility();
        }

        // V tusu - Colossus (Devlesme)
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (activeBodyAbility != BodyAbilityType.None)
            {
                Debug.Log("Zaten aktif bir vucut yetenegi devrede!");
                return;
            }
            if (activeBodyCooldown > 0f)
            {
                Debug.Log($"Vucut yetenekleri bekleme suresinde! (Kalan: {activeBodyCooldown:F1}s)");
                return;
            }

            ActivateColossus();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayAbility();
        }
    }

    void VucutYetenekSureKontrolu()
    {
        if (activeBodyAbility != BodyAbilityType.None)
        {
            bodyAbilityTimer -= Time.deltaTime;
            if (bodyAbilityTimer <= 0)
            {
                if (activeBodyAbility == BodyAbilityType.Clone)
                {
                    DeactivateClone();
                }
                else if (activeBodyAbility == BodyAbilityType.Colossus)
                {
                    DeactivateColossus();
                }

                // Vucut yetenegi bittikten sonra global 30sn cooldown baslat
                cloneCooldownTimer = 30f;
                colossusCooldownTimer = 30f;
                Debug.Log("Vucut yetenegi bitti. 30sn global bekleme basladi.");

                activeBodyAbility = BodyAbilityType.None;
            }
        }

        if (cloneCooldownTimer > 0f) cloneCooldownTimer -= Time.deltaTime;
        if (colossusCooldownTimer > 0f) colossusCooldownTimer -= Time.deltaTime;
    }

    // --- Clone Aktivasyonu ---
    void ActivateClone()
    {
        activeBodyAbility = BodyAbilityType.Clone;
        bodyAbilityTimer = cloneDuration;

        // Yeni bir klon objesi olustur
        activeClone = new GameObject("PlayerClone");
        // Oyuncunun pozisyonunda, Z ekseni kesinlikle 0 olacak sekilde olustur
        activeClone.transform.position = new Vector3(transform.position.x + 1f, transform.position.y + 0.5f, 0f);

        // Klon scriptini ekle ve baslat
        PlayerClone cloneScript = activeClone.AddComponent<PlayerClone>();
        
        // Eger karakterin kendi altinda veya ustunde bir SpriteRenderer varsa onu bulup spriteni alalim
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();
        
        Sprite currentSprite = sr != null ? sr.sprite : null;
        cloneScript.Initialize(transform, this, bulletPrefab, currentSprite);

        Debug.Log($"Clone aktif! Sure: {cloneDuration}s - Silah moduyla kombolanir!");
    }

    void DeactivateClone()
    {
        if (activeClone != null)
        {
            Destroy(activeClone);
            activeClone = null;
        }
    }

    // --- Colossus Aktivasyonu ---
    void ActivateColossus()
    {
        activeBodyAbility = BodyAbilityType.Colossus;
        bodyAbilityTimer = colossusDuration;
        _isColossusActive = true;

        // Eger karakterde bir Animator varsa, scale degisimini ezmesini engellemek icin gecici olarak kapat
        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = false;

        // Boyutu buyut (Z ekseninin 1f kalmasini sagla)
        originalScale = transform.localScale;
        transform.localScale = new Vector3(originalScale.x * colossusScaleMultiplier, originalScale.y * colossusScaleMultiplier, 1f);

        // Cani 10 katina cikar
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.ActivateColossusHealth(colossusHealthMultiplier);
        }

        Debug.Log($"COLOSSUS aktif! Sure: {colossusDuration}s | Boyut: {colossusScaleMultiplier}x | Can: {colossusHealthMultiplier}x | Carpma Hasari: {colossusContactDamage}");
    }

    void DeactivateColossus()
    {
        _isColossusActive = false;

        // Boyutu geri al
        transform.localScale = originalScale;

        // Animatoru tekrar aktif et
        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.enabled = true;

        // Cani geri al
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.DeactivateColossusHealth();
        }

        Debug.Log("Colossus sona erdi. Normal boyuta donuldu.");
    }

    // =============================================
    // SILAH MODLARI
    // =============================================
    void SetWeaponMode(WeaponMode mode)
    {
        currentWeaponMode = mode;

        if (weaponSpriteRenderer != null)
        {
            switch (mode)
            {
                case WeaponMode.Pistol:
                    weaponSpriteRenderer.sprite = pistolSprite;
                    break;
                case WeaponMode.Shotgun:
                    weaponSpriteRenderer.sprite = shotgunSprite;
                    break;
                case WeaponMode.Raygun:
                    weaponSpriteRenderer.sprite = raygunSprite;
                    break;
            }
        }
    }

    void AtesEt()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            if (bulletPrefab == null || firePoint == null) return;

            GameObject bullet1 = null;
            GameObject bullet2 = null;
            GameObject bullet3 = null;

            switch (currentWeaponMode)
            {
                case WeaponMode.Pistol:
                    bullet1 = Instantiate(bulletPrefab, firePoint.position, weaponTransform.rotation);
                    nextFireTime = Time.time + pistolFireRate;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();
                    break;

                case WeaponMode.Shotgun:
                    bullet1 = Instantiate(bulletPrefab, firePoint.position, weaponTransform.rotation);
                    Quaternion solSapma = weaponTransform.rotation * Quaternion.Euler(0, 0, -15f);
                    bullet2 = Instantiate(bulletPrefab, firePoint.position, solSapma);
                    Quaternion sagSapma = weaponTransform.rotation * Quaternion.Euler(0, 0, 15f);
                    bullet3 = Instantiate(bulletPrefab, firePoint.position, sagSapma);
                    nextFireTime = Time.time + shotgunFireRate;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();
                    break;

                case WeaponMode.Raygun:
                    bullet1 = Instantiate(bulletPrefab, firePoint.position, weaponTransform.rotation);
                    nextFireTime = Time.time + raygunFireRate;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayShoot();
                    break;
            }

            // Eger Colossus (Dev) modu aktifse mermileri buyut ve hasarini yukselt
            if (_isColossusActive)
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
            bulletScript.SetScale(2.5f); // 2.5 kat buyuk mermiler
            bulletScript.SetDamage(3);   // 3 kat daha fazla hasar (normal hasar 1 idi)
        }
    }
}
