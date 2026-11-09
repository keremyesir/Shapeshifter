using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Oyuncu Referanslari")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Can Arayuzu (Heart UI)")]
    [SerializeField] private GameObject heartPrefab;            // Can gostergesi icin uretilecek kalp UI prefab'i
    [SerializeField] private Transform heartsContainer;          // Kalplerin yan yana dizilecegi Grid/Layout Group container
    [SerializeField] private Sprite fullHeartSprite;             // Dolu kalp gorseli
    [SerializeField] private Sprite emptyHeartSprite;            // Bos kalp gorseli

    [Header("Yetenek Cooldown Arayuzu")]
    [SerializeField] private Image weaponAbilityIcon;            // Silah yetenegi ikonu
    [SerializeField] private Image weaponCooldownOverlay;        // Silah bekleme suresi karartma maskesi (Fill Amount)
    [SerializeField] private Text weaponCooldownText;            // Silah bekleme saniye yazisi (Opsiyonel)

    [SerializeField] private Image bodyAbilityIcon;              // Vucut yetenegi ikonu
    [SerializeField] private Image bodyCooldownOverlay;          // Vucut bekleme suresi karartma maskesi (Fill Amount)
    [SerializeField] private Text bodyCooldownText;              // Vucut bekleme saniye yazisi (Opsiyonel)

    [Header("Oyun Suresi Sayaci")]
    [SerializeField] private Text gameTimerText;                 // Ekranin orta ustundeki sayac Text bileseni

    [Header("Gorsel Renk Durumlari")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color activeColor = new Color(0f, 1f, 0f, 1f); // Parlak net yesil
    [SerializeField] private Color cooldownColor = new Color(0.15f, 0.15f, 0.15f, 0.95f); // Neredeyse tamamen koyu siyahimsi gri

    private List<Image> spawnedHearts = new List<Image>();
    private float elapsedTime = 0f; // Gecen sure sayaci

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Eger oyuncu referanslari sahnede bulunamadiysa otomatik esitleyelim
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerHealth == null) playerHealth = FindFirstObjectByType<PlayerHealth>();

        InitializeHearts();
    }

    private void Update()
    {
        // Ana menu veya game over sirasinda HUD elemanlarini gizle
        bool hideHUD = GameManager.Instance != null && (GameManager.Instance.IsInMainMenu || GameManager.Instance.IsGameOver);

        // HUD konteynerlarini goster/gizle
        if (heartsContainer != null) heartsContainer.gameObject.SetActive(!hideHUD);
        if (weaponAbilityIcon != null) weaponAbilityIcon.gameObject.SetActive(!hideHUD);
        if (weaponCooldownOverlay != null) weaponCooldownOverlay.transform.parent.gameObject.SetActive(!hideHUD);
        if (bodyAbilityIcon != null) bodyAbilityIcon.gameObject.SetActive(!hideHUD);
        if (bodyCooldownOverlay != null) bodyCooldownOverlay.transform.parent.gameObject.SetActive(!hideHUD);
        
        // Sayac sadece Ana Menude gizlenir, Game Over olunca gizlenmez, gorunur ve donmus sekilde kalir!
        bool showTimer = GameManager.Instance == null || !GameManager.Instance.IsInMainMenu;
        if (gameTimerText != null) gameTimerText.gameObject.SetActive(showTimer);

        if (hideHUD) return; // Menudeyken veya Game Overken asagidaki can/cooldown guncellemelerini yapma

        UpdateHearts();
        UpdateCooldowns();
        UpdateGameTimer();
    }

    // Oyun süresi sayacını günceller. Oyuncu öldüğünde otomatik olarak durur.
    private void UpdateGameTimer()
    {
        // Oyuncu yoksa (olduyse) sayaci durdur ve artik ilerleme yapma
        if (playerHealth == null || playerHealth.GetCurrentHealth() <= 0) return;

        // Zamani sadece oyun dondurulmemisse (Time.timeScale > 0) ilerlet
        if (Time.timeScale > 0f)
        {
            elapsedTime += Time.deltaTime;
        }

        if (gameTimerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Oyuncunun maksimum canina gore kalp objelerini olusturur
    private void InitializeHearts()
    {
        if (heartsContainer == null || heartPrefab == null || playerHealth == null) return;

        // Onceden kalan kalpleri temizle
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedHearts.Clear();

        int maxHealth = playerHealth.GetMaxHealth();
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartObj = Instantiate(heartPrefab, heartsContainer);
            Image heartImage = heartObj.GetComponent<Image>();
            if (heartImage != null)
            {
                heartImage.sprite = fullHeartSprite;
                spawnedHearts.Add(heartImage);
            }
        }
    }

    // Hasar durumlari ve can degisimine gore kalpleri gunceller
    private void UpdateHearts()
    {
        if (playerHealth == null) return;

        int currentHealth = playerHealth.GetCurrentHealth();
        int maxHealth = playerHealth.GetMaxHealth();

        // Eger Colossus moduyla can limiti degistiyse kalp sayisini yeniden sekillendir
        if (spawnedHearts.Count != maxHealth)
        {
            InitializeHearts();
            return;
        }

        for (int i = 0; i < spawnedHearts.Count; i++)
        {
            if (i < currentHealth)
            {
                spawnedHearts[i].sprite = fullHeartSprite;
                spawnedHearts[i].enabled = true;
            }
            else
            {
                // Can gitmisse ya bos kalp sprite'i gosteririz ya da gizleriz. Bos kalp spriti yoksa gizleyelim.
                if (emptyHeartSprite != null)
                {
                    spawnedHearts[i].sprite = emptyHeartSprite;
                }
                else
                {
                    spawnedHearts[i].enabled = false;
                }
            }
        }
    }

    // Cooldown durumlarini maskeler araciligiyla dairesel veya duz doldurma efektiyle gunceller
    private void UpdateCooldowns()
    {
        if (playerController == null) return;

        // --- SILAH YETENEGI UI ---
        float weaponRatio = playerController.GetWeaponCooldownRatio();
        bool isWeaponActive = playerController.IsWeaponAbilityActive();

        if (weaponCooldownOverlay != null)
        {
            if (weaponRatio > 0f)
            {
                weaponCooldownOverlay.gameObject.SetActive(true);
                weaponCooldownOverlay.fillAmount = weaponRatio; // Cooldown suresine gore karartma dolumu
            }
            else
            {
                weaponCooldownOverlay.gameObject.SetActive(false); // Cooldown bittiyse maskeyi tamamen gizle
            }
        }

        if (weaponAbilityIcon != null)
        {
            if (isWeaponActive)
            {
                weaponAbilityIcon.color = activeColor; // Yetenek kullanimda (Yesil)
            }
            else if (weaponRatio > 0f)
            {
                weaponAbilityIcon.color = cooldownColor; // Beklemede (Koyu/Gri)
            }
            else
            {
                weaponAbilityIcon.color = readyColor; // Hazir (Normal/Parlak)
            }
        }

        if (weaponCooldownText != null)
        {
            float activeCD = weaponRatio * 20f;
            if (activeCD > 0.1f)
            {
                weaponCooldownText.gameObject.SetActive(true);
                weaponCooldownText.text = Mathf.CeilToInt(activeCD).ToString();
            }
            else
            {
                weaponCooldownText.gameObject.SetActive(false);
            }
        }


        // --- VUCUT YETENEGI UI ---
        float bodyRatio = playerController.GetBodyCooldownRatio();
        bool isBodyActive = playerController.IsBodyAbilityActive();

        if (bodyCooldownOverlay != null)
        {
            if (bodyRatio > 0f)
            {
                bodyCooldownOverlay.gameObject.SetActive(true);
                bodyCooldownOverlay.fillAmount = bodyRatio;
            }
            else
            {
                bodyCooldownOverlay.gameObject.SetActive(false); // Cooldown bittiyse maskeyi tamamen gizle
            }
        }

        if (bodyAbilityIcon != null)
        {
            if (isBodyActive)
            {
                bodyAbilityIcon.color = activeColor; // Yetenek kullanimda
            }
            else if (bodyRatio > 0f)
            {
                bodyAbilityIcon.color = cooldownColor; // Beklemede
            }
            else
            {
                bodyAbilityIcon.color = readyColor; // Hazir
            }
        }

        if (bodyCooldownText != null)
        {
            float activeCD = bodyRatio * 30f;
            if (activeCD > 0.1f)
            {
                bodyCooldownText.gameObject.SetActive(true);
                bodyCooldownText.text = Mathf.CeilToInt(activeCD).ToString();
            }
            else
            {
                bodyCooldownText.gameObject.SetActive(false);
            }
        }
    }
}
