using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;

    [Header("Spawn Ayarlarý")]
    public float spawnInterval = 2f;    // kaç saniyede bir spawn olsun
    public int maxEnemies = 20;         // sahnede max kaç enemy olsun

    private float spawnTimer = 0f;

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            // Max enemy sayýsýný geçme
            if (GameObject.FindGameObjectsWithTag("Enemy").Length < maxEnemies)
                SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Ekranýn rastgele bir kenarýndan çýksýn
        Vector2 spawnPos = RandomKenarNokta();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    Vector2 RandomKenarNokta()
    {
        // Kameranýn kenarlarýný al
        Camera cam = Camera.main;
        float yukseklik = cam.orthographicSize;
        float genislik = yukseklik * cam.aspect;

        // 0=üst, 1=alt, 2=sol, 3=sað
        int kenar = Random.Range(0, 4);

        switch (kenar)
        {
            case 0: return new Vector2(Random.Range(-genislik, genislik), yukseklik + 0.5f);   // üst
            case 1: return new Vector2(Random.Range(-genislik, genislik), -yukseklik - 0.5f);  // alt
            case 2: return new Vector2(-genislik - 0.5f, Random.Range(-yukseklik, yukseklik)); // sol
            default: return new Vector2(genislik + 0.5f, Random.Range(-yukseklik, yukseklik)); // sað
        }
    }
}