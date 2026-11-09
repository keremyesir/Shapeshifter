using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Ayarlari")]
    public float spawnInterval = 2f;
    public int maxEnemies = 20;

    [Header("Zorluk Artis Ayarlari (20sn)")]
    [SerializeField] private float difficultyInterval = 20f;
    [SerializeField] private float spawnIntervalDecrease = 0.15f;
    [SerializeField] private float minSpawnInterval = 0.3f;

    [Header("Mini Boss Ayarlari (30sn)")]
    [SerializeField] private float bossInterval = 30f;

    private float spawnTimer = 0f;
    private float difficultyTimer = 0f;
    private float bossTimer = 0f;

    void Update()
    {
        // 1. Klasik Dusman Spawn Zamanlayicisi
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
                SpawnEnemy();
        }

        // 2. Zorluk Artis Zamanlayicisi (Her 20 saniyede bir)
        difficultyTimer += Time.deltaTime;
        if (difficultyTimer >= difficultyInterval)
        {
            difficultyTimer = 0f;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecrease);
            Debug.Log("Zorluk artti! Yeni Spawn Sikligi: " + spawnInterval + " saniye.");
        }

        // 3. Mini Boss Zamanlayicisi (Her 30 saniyede bir)
        bossTimer += Time.deltaTime;
        if (bossTimer >= bossInterval)
        {
            bossTimer = 0f;
            SpawnMiniBoss();
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        int rastgeleIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject secilenPrefab = enemyPrefabs[rastgeleIndex];

        Vector2 spawnPos = RandomKenarNokta();
        Instantiate(secilenPrefab, spawnPos, Quaternion.identity);
    }

    void SpawnMiniBoss()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Mevcut dusman havuzundan rastgele birini secip boss yapiyoruz
        int rastgeleIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject secilenPrefab = enemyPrefabs[rastgeleIndex];

        Vector2 spawnPos = RandomKenarNokta();
        GameObject boss = Instantiate(secilenPrefab, spawnPos, Quaternion.identity);

        // Mini Boss Ozelliklerini Guncelleme (Can ve Hasar x5)
        Enemy enemyComponent = boss.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.maxHealth *= 5;
            enemyComponent.damage *= 5;
        }

        // Gorsel Boyutunu Buyutme (2.5 kat)
        boss.transform.localScale *= 2.5f;

        Debug.Log("Bir MINI BOSS dogdu! (Can ve Hasar 5 kat, Boyut 2.5 kat)");
    }

    Vector2 RandomKenarNokta()
    {
        Camera cam = Camera.main;
        Vector3 camPos = cam.transform.position;

        float yukseklik = cam.orthographicSize;
        float genislik = yukseklik * cam.aspect;

        // 0=ust, 1=alt, 2=sol, 3=sag
        int kenar = Random.Range(0, 4);

        switch (kenar)
        {
            case 0:
                return new Vector2(camPos.x + Random.Range(-genislik - 1f, genislik + 1f), camPos.y + yukseklik + 1f);
            case 1:
                return new Vector2(camPos.x + Random.Range(-genislik - 1f, genislik + 1f), camPos.y - yukseklik - 1f);
            case 2:
                return new Vector2(camPos.x - genislik - 1f, camPos.y + Random.Range(-yukseklik - 1f, yukseklik + 1f));
            default:
                return new Vector2(camPos.x + genislik + 1f, camPos.y + Random.Range(-yukseklik - 1f, yukseklik + 1f));
        }
    }
}
